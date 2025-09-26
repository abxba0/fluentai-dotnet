using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Services
{
    /// <summary>
    /// Text generation service that provides a multi-modal wrapper around the existing IChatModel.
    /// This maintains backward compatibility while enabling multi-modal functionality.
    /// </summary>
    public class TextGenerationService : ITextGenerationService
    {
        private readonly IChatModel _chatModel;
        private readonly ILogger<TextGenerationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextGenerationService"/> class.
        /// </summary>
        /// <param name="chatModel">The underlying chat model.</param>
        /// <param name="logger">The logger instance.</param>
        public TextGenerationService(IChatModel chatModel, ILogger<TextGenerationService> logger)
        {
            _chatModel = chatModel ?? throw new ArgumentNullException(nameof(chatModel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public string ProviderName => "DefaultTextProvider"; // This will be overridden by actual providers

        /// <inheritdoc />
        public string DefaultModelName => "gpt-3.5-turbo"; // This will be overridden by actual providers

        /// <inheritdoc />
        public async Task<TextResponse> GenerateAsync(TextRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Generating text with prompt: {Prompt}", request.Prompt);

            var startTime = DateTimeOffset.UtcNow;

            try
            {
                // Convert TextRequest to ChatMessage format
                var messages = ConvertToMessages(request);
                
                // Create ChatRequestOptions from TextRequest
                var options = CreateChatRequestOptions(request);

                // Call the underlying IChatModel
                var chatResponse = await _chatModel.GetResponseAsync(messages, options, cancellationToken);

                var processingTime = DateTimeOffset.UtcNow - startTime;

                // Convert ChatResponse to TextResponse
                return new TextResponse
                {
                    Content = chatResponse.Content,
                    FinishReason = chatResponse.FinishReason,
                    ConfidenceScore = null, // Not available from ChatResponse
                    ModelUsed = chatResponse.ModelId,
                    Provider = ProviderName,
                    TokenUsage = chatResponse.Usage,
                    ProcessingTime = processingTime.Duration(),
                    GeneratedAt = startTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with prompt: {Prompt}", request.Prompt);
                throw;
            }
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<string> StreamAsync(TextRequest request, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Streaming text generation with prompt: {Prompt}", request.Prompt);

            // Convert TextRequest to ChatMessage format
            var messages = ConvertToMessages(request);
            
            // Create ChatRequestOptions from TextRequest
            var options = CreateChatRequestOptions(request);

            // Stream from the underlying IChatModel
            await foreach (var token in _chatModel.StreamResponseAsync(messages, options, cancellationToken))
            {
                yield return token;
            }
        }

        /// <inheritdoc />
        public async Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Test with a simple request to validate configuration
                var testMessages = new[] { new ChatMessage(ChatRole.User, "Hello") };
                await _chatModel.GetResponseAsync(testMessages, null, cancellationToken);
                
                _logger.LogInformation("Text generation service configuration validated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Text generation service configuration validation failed");
                throw;
            }
        }

        /// <summary>
        /// Converts a TextRequest to a collection of ChatMessage objects.
        /// </summary>
        /// <param name="request">The text request.</param>
        /// <returns>A collection of chat messages.</returns>
        private static IEnumerable<ChatMessage> ConvertToMessages(TextRequest request)
        {
            var messages = new List<ChatMessage>();

            // Add system message if provided
            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                messages.Add(new ChatMessage(ChatRole.System, request.SystemMessage));
            }

            // Add existing messages if provided
            if (request.Messages != null)
            {
                messages.AddRange(request.Messages);
            }

            // Add the prompt as a user message if not already included in Messages
            if (!string.IsNullOrEmpty(request.Prompt) && 
                (request.Messages == null || !request.Messages.Any(m => m.Role == ChatRole.User && m.Content == request.Prompt)))
            {
                messages.Add(new ChatMessage(ChatRole.User, request.Prompt));
            }

            return messages;
        }

        /// <summary>
        /// Creates ChatRequestOptions from a TextRequest.
        /// </summary>
        /// <param name="request">The text request.</param>
        /// <returns>Chat request options.</returns>
        private static ChatRequestOptions? CreateChatRequestOptions(TextRequest request)
        {
            // Since ChatRequestOptions is abstract and doesn't have the properties we need,
            // we'll return null for now. Provider-specific implementations can override this
            // to create appropriate options based on their concrete ChatRequestOptions implementation.
            return null;
        }
    }
}