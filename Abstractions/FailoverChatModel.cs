using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Failover chat model implementation that tries a primary provider and falls back to a secondary provider on retriable errors.
    /// </summary>
    internal class FailoverChatModel : IChatModel
    {
        private readonly IChatModel _primaryProvider;
        private readonly IChatModel _fallbackProvider;
        private readonly ILogger<FailoverChatModel> _logger;

        public FailoverChatModel(IChatModel primaryProvider, IChatModel fallbackProvider, ILogger<FailoverChatModel> logger)
        {
            _primaryProvider = primaryProvider ?? throw new ArgumentNullException(nameof(primaryProvider));
            _fallbackProvider = fallbackProvider ?? throw new ArgumentNullException(nameof(fallbackProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _primaryProvider.GetResponseAsync(messages, options, cancellationToken);
            }
            catch (Exception ex) when (IsRetriableError(ex))
            {
                _logger.LogWarning(ex, "Primary provider failed with retriable error, attempting failover to fallback provider");
                
                try
                {
                    var result = await _fallbackProvider.GetResponseAsync(messages, options, cancellationToken);
                    _logger.LogInformation("Failover successful, response received from fallback provider");
                    return result;
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, "Fallback provider also failed");
                    throw;
                }
            }
        }

        public async IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Try to get streaming response from primary provider
            var primaryStream = await TryGetPrimaryStreamAsync(messages, options, cancellationToken);
            if (primaryStream != null)
            {
                await foreach (var token in primaryStream)
                {
                    yield return token;
                }
                yield break;
            }

            // Primary failed, try fallback
            var fallbackStream = await TryGetFallbackStreamAsync(messages, options, cancellationToken);
            await foreach (var token in fallbackStream)
            {
                yield return token;
            }
        }

        private async Task<IAsyncEnumerable<string>?> TryGetPrimaryStreamAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options, CancellationToken cancellationToken)
        {
            try
            {
                var stream = _primaryProvider.StreamResponseAsync(messages, options, cancellationToken);
                var enumerator = stream.GetAsyncEnumerator(cancellationToken);
                
                // Test if we can get the first token
                if (await enumerator.MoveNextAsync())
                {
                    // Reset enumerator by creating a new stream and return it
                    await enumerator.DisposeAsync();
                    return _primaryProvider.StreamResponseAsync(messages, options, cancellationToken);
                }
                
                await enumerator.DisposeAsync();
                return null;
            }
            catch (Exception ex) when (IsRetriableError(ex))
            {
                _logger.LogWarning(ex, "Primary provider failed with retriable error during streaming, will attempt failover to fallback provider");
                return null;
            }
        }

        private Task<IAsyncEnumerable<string>> TryGetFallbackStreamAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options, CancellationToken cancellationToken)
        {
            try
            {
                var stream = _fallbackProvider.StreamResponseAsync(messages, options, cancellationToken);
                _logger.LogInformation("Failover successful, streaming response received from fallback provider");
                return Task.FromResult(stream);
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback provider also failed during streaming");
                throw;
            }
        }

        private static bool IsRetriableError(Exception exception)
        {
            return exception switch
            {
                AiSdkException aiEx when aiEx.InnerException is Azure.RequestFailedException rfe && IsRetriableHttpStatus(rfe.Status) => true,
                AiSdkException aiEx when aiEx.InnerException is HttpRequestException => true,
                AiSdkRateLimitException => true,
                TimeoutException => true,
                TaskCanceledException tce when !tce.CancellationToken.IsCancellationRequested => true, // Timeout, not user cancellation
                _ => false
            };
        }

        private static bool IsRetriableHttpStatus(int statusCode)
        {
            return statusCode is >= 500 and <= 599 or 429; // 5xx errors and rate limiting
        }
    }
}