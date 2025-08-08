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

namespace FluentAI.Providers.Anthropic
{
    internal class AnthropicChatModel : ChatModelBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<AnthropicOptions> _optionsMonitor;

        public AnthropicChatModel(IHttpClientFactory httpClientFactory, IOptionsMonitor<AnthropicOptions> optionsMonitor, ILogger<AnthropicChatModel> logger) : base(logger)
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
                Logger.LogError(ex, "Anthropic API request failed with status {Status}", ex.Data["StatusCode"]);
                throw new AiSdkException($"Anthropic API request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected error during Anthropic chat completion");
                throw new AiSdkException($"Unexpected error: {ex.Message}", ex);
            }
        }

        public override async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            ValidateConfiguration(currentOptions);
            var requestDto = PrepareRequest(messages, true, currentOptions, options);
            var httpClient = _httpClientFactory.CreateClient("AnthropicClient");

            // **VERIFIED FIX**: Use modern 'using' declarations and set headers on the request, not the shared client.
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages") { Content = JsonContent.Create(requestDto) };
            request.Headers.Add("x-api-key", currentOptions.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            response.EnsureSuccessStatusCode(); // Throws HttpRequestException on failure

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
                if (jsonDoc.RootElement.TryGetProperty("delta", out var delta) &&
                    delta.TryGetProperty("text", out var text))
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

        private void ValidateConfiguration(AnthropicOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey))
                throw new AiSdkConfigurationException("Anthropic API key is required");

            if (string.IsNullOrWhiteSpace(options.Model))
                throw new AiSdkConfigurationException("Anthropic model is required");
        }

        private object PrepareRequest(IEnumerable<ChatMessage> messages, bool stream, AnthropicOptions configOptions, ChatRequestOptions? requestOptions)
        {
            var messageList = base.ValidateMessages(messages, configOptions.MaxRequestSize);
            var providerOptions = requestOptions as AnthropicRequestOptions;

            var anthropicMessages = new List<object>();
            string? systemPrompt = null;

            foreach (var message in messageList)
            {
                if (message.Role == ChatRole.System)
                {
                    systemPrompt = message.Content;
                }
                else
                {
                    anthropicMessages.Add(new
                    {
                        role = message.Role == ChatRole.User ? "user" : "assistant",
                        content = message.Content
                    });
                }
            }

            var request = new
            {
                model = configOptions.Model,
                max_tokens = providerOptions?.MaxTokens ?? configOptions.MaxTokens ?? 1000,
                messages = anthropicMessages,
                stream = stream
            };

            // Add optional parameters
            var requestDict = new Dictionary<string, object>
            {
                ["model"] = request.model,
                ["max_tokens"] = request.max_tokens,
                ["messages"] = request.messages,
                ["stream"] = request.stream
            };

            if (!string.IsNullOrEmpty(systemPrompt) || !string.IsNullOrEmpty(providerOptions?.SystemPrompt))
            {
                requestDict["system"] = providerOptions?.SystemPrompt ?? systemPrompt ?? string.Empty;
            }

            if (providerOptions?.Temperature.HasValue == true)
            {
                requestDict["temperature"] = providerOptions.Temperature.Value;
            }

            return requestDict;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(object requestDto, AnthropicOptions options, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("AnthropicClient");
            
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
            {
                Content = JsonContent.Create(requestDto)
            };
            request.Headers.Add("x-api-key", options.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            return await httpClient.SendAsync(request, cancellationToken);
        }

        private ChatResponse ProcessResponse(HttpResponseMessage response)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            using var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            var messageContent = string.Empty;
            if (root.TryGetProperty("content", out var contentArray) && 
                contentArray.ValueKind == JsonValueKind.Array && 
                contentArray.GetArrayLength() > 0)
            {
                var firstContent = contentArray[0];
                if (firstContent.TryGetProperty("text", out var textProp))
                {
                    messageContent = textProp.GetString() ?? string.Empty;
                }
            }

            var modelId = root.TryGetProperty("model", out var modelProp) ? modelProp.GetString() ?? "unknown" : "unknown";
            var finishReason = root.TryGetProperty("stop_reason", out var stopProp) ? stopProp.GetString() ?? "unknown" : "unknown";

            var inputTokens = 0;
            var outputTokens = 0;
            if (root.TryGetProperty("usage", out var usageProp))
            {
                if (usageProp.TryGetProperty("input_tokens", out var inputProp))
                    inputTokens = inputProp.GetInt32();
                if (usageProp.TryGetProperty("output_tokens", out var outputProp))
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