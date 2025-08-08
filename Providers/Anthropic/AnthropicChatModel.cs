using Genius.Core.Abstractions;
using Genius.Core.Abstractions.Exceptions;
using Genius.Core.Abstractions.Models;
using Genius.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text;

namespace Genius.Core.Providers.Anthropic
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

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages") 
            { 
                Content = JsonContent.Create(requestDto) 
            };
            request.Headers.Add("x-api-key", currentOptions.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

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
                    var jsonData = line.Substring(6).Trim();
                    if (jsonData == "[DONE]")
                        break;

                    string? textToYield = null;
                    try
                    {
                        var eventData = JsonSerializer.Deserialize<JsonElement>(jsonData);
                        if (eventData.TryGetProperty("type", out var typeProperty) && 
                            typeProperty.GetString() == "content_block_delta")
                        {
                            if (eventData.TryGetProperty("delta", out var deltaProperty) &&
                                deltaProperty.TryGetProperty("text", out var textProperty))
                            {
                                textToYield = textProperty.GetString();
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Logger.LogWarning(ex, "Failed to parse streaming response: {Data}", jsonData);
                        // Continue processing other lines instead of throwing
                    }

                    if (!string.IsNullOrEmpty(textToYield))
                    {
                        yield return textToYield;
                    }
                }
            }
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
            var anthropicOptions = requestOptions as AnthropicRequestOptions;

            var anthropicMessages = messageList.Where(m => m.Role != ChatRole.System).Select(m => new
            {
                role = m.Role == ChatRole.User ? "user" : "assistant",
                content = m.Content
            }).ToList();

            var systemMessage = messageList.FirstOrDefault(m => m.Role == ChatRole.System);
            var systemPrompt = anthropicOptions?.SystemPrompt ?? systemMessage?.Content;

            var request = new
            {
                model = configOptions.Model,
                max_tokens = anthropicOptions?.MaxTokens ?? configOptions.MaxTokens ?? 1000,
                messages = anthropicMessages,
                stream = stream
            };

            // Add optional fields if they have values
            var requestDict = new Dictionary<string, object>
            {
                ["model"] = request.model,
                ["max_tokens"] = request.max_tokens,
                ["messages"] = request.messages,
                ["stream"] = request.stream
            };

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                requestDict["system"] = systemPrompt;

            if (anthropicOptions?.Temperature.HasValue == true)
                requestDict["temperature"] = anthropicOptions.Temperature.Value;

            return requestDict;
        }

        private async Task<HttpResponseMessage> SendRequestAsync(object requestDto, AnthropicOptions options, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("AnthropicClient");
            
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = JsonContent.Create(requestDto)
            };
            
            request.Headers.Add("x-api-key", options.ApiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");

            var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return response;
        }

        private ChatResponse ProcessResponse(HttpResponseMessage response)
        {
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(response.Content.ReadAsStream());
            
            var content = "";
            if (jsonResponse.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                var firstContent = contentArray.EnumerateArray().FirstOrDefault();
                if (firstContent.TryGetProperty("text", out var textProperty))
                {
                    content = textProperty.GetString() ?? "";
                }
            }

            var model = jsonResponse.TryGetProperty("model", out var modelProperty) ? modelProperty.GetString() ?? "unknown" : "unknown";
            var stopReason = jsonResponse.TryGetProperty("stop_reason", out var stopProperty) ? stopProperty.GetString() ?? "unknown" : "unknown";

            var inputTokens = 0;
            var outputTokens = 0;
            if (jsonResponse.TryGetProperty("usage", out var usageProperty))
            {
                if (usageProperty.TryGetProperty("input_tokens", out var inputProperty))
                    inputTokens = inputProperty.GetInt32();
                if (usageProperty.TryGetProperty("output_tokens", out var outputProperty))
                    outputTokens = outputProperty.GetInt32();
            }

            return new ChatResponse(
                Content: content,
                ModelId: model,
                FinishReason: stopReason,
                Usage: new TokenUsage(InputTokens: inputTokens, OutputTokens: outputTokens)
            );
        }
    }
}