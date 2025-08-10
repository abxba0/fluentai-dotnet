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

namespace FluentAI.Providers.HuggingFace
{
    internal class HuggingFaceChatModel : ChatModelBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<HuggingFaceOptions> _optionsMonitor;

        public HuggingFaceChatModel(IHttpClientFactory httpClientFactory, IOptionsMonitor<HuggingFaceOptions> optionsMonitor, ILogger<HuggingFaceChatModel> logger) : base(logger)
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
                Logger.LogError(ex, "Hugging Face API request failed with status {Status}", ex.Data["StatusCode"]);
                throw new AiSdkException($"Hugging Face API request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during Hugging Face chat completion");
                throw new AiSdkException($"Unexpected error: {ex.Message}", ex);
            }
        }

        public override async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            ValidateConfiguration(currentOptions);
            var requestDto = PrepareRequest(messages, true, currentOptions, options);
            var httpClient = _httpClientFactory.CreateClient("HuggingFaceClient");

            using var request = new HttpRequestMessage(HttpMethod.Post, currentOptions.ModelId) { Content = JsonContent.Create(requestDto) };
            request.Headers.Add("Authorization", $"Bearer {currentOptions.ApiKey}");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
            {
                if (line.StartsWith("data: "))
                {
                    var jsonData = line[6..]; // Remove "data: " prefix
                    if (jsonData != "[DONE]")
                    {
                        var text = ParseStreamChunk(jsonData);
                        if (!string.IsNullOrEmpty(text))
                        {
                            yield return text;
                        }
                    }
                }
            }
        }

        private string? ParseStreamChunk(string jsonData)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(jsonData);
                if (jsonDoc.RootElement.TryGetProperty("token", out var token) &&
                    token.TryGetProperty("text", out var text))
                {
                    return text.GetString();
                }
            }
            catch (JsonException)
            {
                // Skip malformed JSON
            }
            return null;
        }

        private void ValidateConfiguration(HuggingFaceOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new AiSdkConfigurationException("Hugging Face API key is required");

            if (string.IsNullOrWhiteSpace(options.ModelId))
                throw new AiSdkConfigurationException("Hugging Face model ID (Inference Endpoint URL) is required");

            if (!Uri.TryCreate(options.ModelId, UriKind.Absolute, out _))
                throw new AiSdkConfigurationException("Hugging Face model ID must be a valid URL");
        }

        private object PrepareRequest(IEnumerable<ChatMessage> messages, bool stream, HuggingFaceOptions configOptions, ChatRequestOptions? requestOptions)
        {
            var messageList = base.ValidateMessages(messages, configOptions.MaxRequestSize);
            var providerOptions = requestOptions as HuggingFaceRequestOptions;

            // Convert messages to a single input string for Hugging Face
            var inputText = BuildInputText(messageList);

            var requestDict = new Dictionary<string, object>
            {
                ["inputs"] = inputText,
                ["stream"] = stream
            };

            // Add parameters if specified
            var parameters = new Dictionary<string, object>();
            
            if (providerOptions?.Temperature.HasValue == true)
            {
                parameters["temperature"] = providerOptions.Temperature.Value;
            }
            
            if (providerOptions?.MaxNewTokens.HasValue == true)
            {
                parameters["max_new_tokens"] = providerOptions.MaxNewTokens.Value;
            }
            
            if (providerOptions?.TopP.HasValue == true)
            {
                parameters["top_p"] = providerOptions.TopP.Value;
            }
            
            if (providerOptions?.TopK.HasValue == true)
            {
                parameters["top_k"] = providerOptions.TopK.Value;
            }

            if (parameters.Count > 0)
            {
                requestDict["parameters"] = parameters;
            }

            return requestDict;
        }

        private string BuildInputText(List<ChatMessage> messages)
        {
            var inputBuilder = new StringBuilder();
            
            foreach (var message in messages)
            {
                var rolePrefix = message.Role switch
                {
                    ChatRole.System => "System: ",
                    ChatRole.User => "User: ",
                    ChatRole.Assistant => "Assistant: ",
                    _ => throw new ArgumentException($"Unsupported chat role: {message.Role}")
                };
                
                inputBuilder.AppendLine($"{rolePrefix}{message.Content}");
            }
            
            inputBuilder.AppendLine("Assistant: ");
            return inputBuilder.ToString();
        }

        private async Task<HttpResponseMessage> SendRequestAsync(object requestDto, HuggingFaceOptions options, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("HuggingFaceClient");
            
            using var request = new HttpRequestMessage(HttpMethod.Post, options.ModelId)
            {
                Content = JsonContent.Create(requestDto)
            };
            request.Headers.Add("Authorization", $"Bearer {options.ApiKey}");

            return await httpClient.SendAsync(request, cancellationToken);
        }

        private ChatResponse ProcessResponse(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            using var jsonDoc = JsonDocument.Parse(content);
            
            var messageContent = string.Empty;
            
            // Hugging Face Inference API returns an array with generated_text
            if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array && 
                jsonDoc.RootElement.GetArrayLength() > 0)
            {
                var firstElement = jsonDoc.RootElement[0];
                if (firstElement.TryGetProperty("generated_text", out var generatedText))
                {
                    messageContent = generatedText.GetString() ?? string.Empty;
                    
                    // Extract only the assistant's response (everything after the last "Assistant: ")
                    var lastAssistantIndex = messageContent.LastIndexOf("Assistant: ");
                    if (lastAssistantIndex >= 0)
                    {
                        messageContent = messageContent.Substring(lastAssistantIndex + "Assistant: ".Length).Trim();
                    }
                }
            }

            return new ChatResponse(
                Content: messageContent,
                ModelId: "huggingface-inference",
                FinishReason: "stop",
                Usage: new TokenUsage(
                    InputTokens: 0, // Hugging Face doesn't provide token usage in basic inference
                    OutputTokens: 0
                )
            );
        }

        // DTO records for Hugging Face API
        private record HuggingFaceRequest(
            string inputs,
            Dictionary<string, object>? parameters = null,
            bool stream = false
        );

        private record HuggingFaceResponse(
            string generated_text
        );

        private record HuggingFaceStreamChunk(
            HuggingFaceToken token
        );

        private record HuggingFaceToken(
            string text
        );
    }
}