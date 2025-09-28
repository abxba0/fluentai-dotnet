using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;

namespace FluentAI.Providers.Google
{
    internal class GoogleGeminiChatModel : ChatModelBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<GoogleOptions> _optionsMonitor;

        public GoogleGeminiChatModel(IHttpClientFactory httpClientFactory, IOptionsMonitor<GoogleOptions> optionsMonitor, ILogger<GoogleGeminiChatModel> logger) : base(logger)
        {
            _httpClientFactory = httpClientFactory;
            _optionsMonitor = optionsMonitor;
        }

        public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            ValidateConfiguration(currentOptions);

            var requestDto = PrepareRequest(messages, false, currentOptions, options);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            try
            {
                var response = await base.ExecuteWithRetryAsync(
                    () => SendRequestAsync(requestDto, currentOptions, timeoutCts.Token),
                    currentOptions.MaxRetries,
                    ex => ex is HttpRequestException hre && hre.StatusCode is System.Net.HttpStatusCode.TooManyRequests or System.Net.HttpStatusCode.InternalServerError or System.Net.HttpStatusCode.BadGateway or System.Net.HttpStatusCode.ServiceUnavailable or System.Net.HttpStatusCode.GatewayTimeout,
                    timeoutCts.Token);

                return ProcessResponse(response);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, "Google Gemini API request failed with status {Status}", ex.Data["StatusCode"]);
                throw new AiSdkException($"Google Gemini API request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during Google Gemini chat completion");
                throw new AiSdkException($"Unexpected error: {ex.Message}", ex);
            }
        }

        public override async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            ValidateConfiguration(currentOptions);
            var requestDto = PrepareRequest(messages, true, currentOptions, options);
            var httpClient = _httpClientFactory.CreateClient("GoogleClient");

            // SECURITY FIX: Move API key from URL to header to prevent logging exposure
            var requestUri = $"/v1beta/models/{currentOptions.Model}:streamGenerateContent";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) 
            { 
                Content = JsonContent.Create(requestDto)
            };
            
            // Add API key in header instead of URL
            request.Headers.Add("X-Goog-Api-Key", currentOptions.ApiKey);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var text = ParseStreamChunk(line);
                    if (!string.IsNullOrEmpty(text))
                    {
                        yield return text;
                    }
                }
            }
        }

        private string? ParseStreamChunk(string jsonData)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonData);
                if (jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) &&
                    candidates.ValueKind == JsonValueKind.Array &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content) &&
                        content.TryGetProperty("parts", out var parts) &&
                        parts.ValueKind == JsonValueKind.Array &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var text))
                        {
                            return text.GetString();
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Skip malformed JSON
            }
            return null;
        }

        private void ValidateConfiguration(GoogleOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new AiSdkConfigurationException("Google API key is required");

            if (string.IsNullOrWhiteSpace(options.Model))
                throw new AiSdkConfigurationException("Google model is required");
        }

        private object PrepareRequest(IEnumerable<ChatMessage> messages, bool stream, GoogleOptions configOptions, ChatRequestOptions? requestOptions)
        {
            var messageList = base.ValidateMessages(messages, configOptions.MaxRequestSize);
            var providerOptions = requestOptions as GoogleRequestOptions;

            var contents = new List<object>();

            foreach (var message in messageList)
            {
                // Gemini doesn't have a separate system role, system messages go as user messages
                var role = message.Role == ChatRole.Assistant ? "model" : "user";
                
                contents.Add(new
                {
                    role = role,
                    parts = new[] { new { text = message.Content } }
                });
            }

            var requestDict = new Dictionary<string, object>
            {
                ["contents"] = contents
            };

            // Add generation config if options are provided
            var generationConfig = new Dictionary<string, object>();
            
            if (providerOptions?.Temperature.HasValue == true)
            {
                generationConfig["temperature"] = providerOptions.Temperature.Value;
            }
            
            if (providerOptions?.TopP.HasValue == true)
            {
                generationConfig["topP"] = providerOptions.TopP.Value;
            }
            
            if (providerOptions?.MaxOutputTokens.HasValue == true)
            {
                generationConfig["maxOutputTokens"] = providerOptions.MaxOutputTokens.Value;
            }

            if (generationConfig.Any())
            {
                requestDict["generationConfig"] = generationConfig;
            }

            return requestDict;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(object requestDto, GoogleOptions options, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("GoogleClient");
            
            // SECURITY FIX: Move API key from URL to header to prevent logging exposure
            var requestUri = $"/v1beta/models/{options.Model}:generateContent";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(requestDto)
            };
            
            // Add API key in header instead of URL
            request.Headers.Add("X-Goog-Api-Key", options.ApiKey);

            return await httpClient.SendAsync(request, cancellationToken);
        }

        private ChatResponse ProcessResponse(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            var messageContent = string.Empty;
            var finishReason = "unknown";
            
            if (root.TryGetProperty("candidates", out var candidates) &&
                candidates.ValueKind == JsonValueKind.Array &&
                candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                
                // Extract content
                if (firstCandidate.TryGetProperty("content", out var contentProp) &&
                    contentProp.TryGetProperty("parts", out var parts) &&
                    parts.ValueKind == JsonValueKind.Array &&
                    parts.GetArrayLength() > 0)
                {
                    var firstPart = parts[0];
                    if (firstPart.TryGetProperty("text", out var textProp))
                    {
                        messageContent = textProp.GetString() ?? string.Empty;
                    }
                }
                
                // Extract finish reason
                if (firstCandidate.TryGetProperty("finishReason", out var finishProp))
                {
                    finishReason = finishProp.GetString() ?? "unknown";
                }
            }

            var modelId = _optionsMonitor.CurrentValue.Model;

            // Gemini doesn't provide detailed token usage in the response
            // We'll provide basic estimates or zeros
            var inputTokens = 0;
            var outputTokens = 0;
            
            if (root.TryGetProperty("usageMetadata", out var usageProp))
            {
                if (usageProp.TryGetProperty("promptTokenCount", out var inputProp))
                    inputTokens = inputProp.GetInt32();
                if (usageProp.TryGetProperty("candidatesTokenCount", out var outputProp))
                    outputTokens = outputProp.GetInt32();
            }

            return new ChatResponse(
                Content: messageContent,
                ModelId: modelId,
                FinishReason: finishReason,
                Usage: new TokenUsage(
                    InputTokens: inputTokens,
                    OutputTokens: outputTokens
                )
            );
        }
    }
}