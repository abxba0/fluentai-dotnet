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
            // Try primary provider first
            var primaryResult = TryStreamFromProviderAsync(_primaryProvider, "primary", messages, options, cancellationToken);
            
            await foreach (var result in primaryResult)
            {
                if (result.IsSuccess)
                {
                    yield return result.Token!;
                }
                else
                {
                    // Primary failed, try fallback
                    _logger.LogWarning(result.Exception, "Primary provider failed with retriable error during streaming, attempting failover to fallback provider");
                    
                    var fallbackResult = TryStreamFromProviderAsync(_fallbackProvider, "fallback", messages, options, cancellationToken);
                    await foreach (var fallbackToken in fallbackResult)
                    {
                        if (fallbackToken.IsSuccess)
                        {
                            yield return fallbackToken.Token!;
                        }
                        else
                        {
                            _logger.LogError(fallbackToken.Exception, "Fallback provider also failed during streaming");
                            throw result.Exception ?? fallbackToken.Exception!;
                        }
                    }
                    yield break;
                }
            }
        }

        private async IAsyncEnumerable<StreamResult> TryStreamFromProviderAsync(IChatModel provider, string providerName, IEnumerable<ChatMessage> messages, ChatRequestOptions? options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var results = new List<StreamResult>();
            IAsyncEnumerator<string>? enumerator = null;
            
            try
            {
                var stream = provider.StreamResponseAsync(messages, options, cancellationToken);
                enumerator = stream.GetAsyncEnumerator(cancellationToken);
                
                while (await enumerator.MoveNextAsync())
                {
                    results.Add(new StreamResult { IsSuccess = true, Token = enumerator.Current });
                }
            }
            catch (Exception ex) when (IsRetriableError(ex))
            {
                results.Add(new StreamResult { IsSuccess = false, Exception = ex });
            }
            finally
            {
                if (enumerator != null)
                {
                    await enumerator.DisposeAsync();
                }
            }

            foreach (var result in results)
            {
                yield return result;
            }
        }

        private class StreamResult
        {
            public bool IsSuccess { get; set; }
            public string? Token { get; set; }
            public Exception? Exception { get; set; }
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