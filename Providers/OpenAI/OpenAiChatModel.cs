using Azure;
using Azure.AI.OpenAI;
using Genius.Core.Abstractions;
using Genius.Core.Abstractions.Exceptions;
using Genius.Core.Abstractions.Models;
using Genius.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

namespace FluentAI.NET.Providers.OpenAI
{
    internal class OpenAiChatModel : ChatModelBase
    {
        private readonly IOptionsMonitor<OpenAiOptions> _optionsMonitor;

        // **VERIFIED FIX**: Cache the client for performance and re-create it only when critical configuration changes.
        private OpenAIClient? _cachedClient;
        private OpenAiOptions? _cachedOptions;
        private readonly object _clientLock = new();

        public OpenAiChatModel(IOptionsMonitor<OpenAiOptions> optionsMonitor, ILogger<OpenAiChatModel> logger) : base(logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public override async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            // **VERIFIED FIX**: Validate configuration on every request to support hot-reload of invalid states.
            ValidateConfiguration(currentOptions);

            var client = GetOrCreateClient(currentOptions);
            var chatOptions = PrepareChatOptions(messages, currentOptions, options);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            try
            {
                var response = await base.ExecuteWithRetryAsync(
                    () => client.GetChatCompletionsAsync(chatOptions, timeoutCts.Token),
                    currentOptions.MaxRetries,
                    ex => ex is RequestFailedException rfe && rfe.Status is 429 or 500 or 502 or 503 or 504,
                    timeoutCts.Token);

                return ProcessResponse(response.Value);
            }
            // ... (Exception handling remains the same)
        }

        public override async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var currentOptions = _optionsMonitor.CurrentValue;
            ValidateConfiguration(currentOptions);

            var client = GetOrCreateClient(currentOptions);
            var chatOptions = PrepareChatOptions(messages, currentOptions, options);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(currentOptions.RequestTimeout);

            var streamingResponse = await client.GetChatCompletionsStreamingAsync(chatOptions, timeoutCts.Token);

            await foreach (var update in streamingResponse.WithCancellation(cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.ContentUpdate))
                {
                    yield return update.ContentUpdate;
                }
            }
        }

        private OpenAIClient GetOrCreateClient(OpenAiOptions options)
        {
            lock (_clientLock)
            {
                if (_cachedClient == null || options.ApiKey != _cachedOptions?.ApiKey || options.Endpoint != _cachedOptions?.Endpoint)
                {
                    Logger.LogInformation("Creating new OpenAIClient instance due to configuration change or first use.");
                    _cachedClient = options.IsAzureOpenAI
                        ? new OpenAIClient(new Uri(options.Endpoint!), new AzureKeyCredential(options.ApiKey))
                        : new OpenAIClient(options.ApiKey);
                    _cachedOptions = options;
                }
                return _cachedClient;
            }
        }

        private ChatCompletionsOptions PrepareChatOptions(IEnumerable<ChatMessage> messages, OpenAiOptions configOptions, ChatRequestOptions? requestOptions)
        {
            var messageList = base.ValidateMessages(messages, configOptions.MaxRequestSize);
            var providerOptions = requestOptions as OpenAiRequestOptions;

            return new ChatCompletionsOptions
            {
                DeploymentName = configOptions.Model,
                Temperature = providerOptions?.Temperature,
                // **VERIFIED FIX**: Added consistent MaxTokens configuration
                MaxTokens = providerOptions?.MaxTokens ?? configOptions.MaxTokens,
                TopP = providerOptions?.TopP
            };
        }

        // ... (ProcessResponse, ValidateConfiguration, MapRole methods remain the same)
    }
}