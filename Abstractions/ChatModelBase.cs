using Genius.Core.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Genius.Core.Abstractions
{
    public abstract class ChatModelBase : IChatModel
    {
        protected readonly ILogger Logger;

        protected ChatModelBase(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default);
        public abstract IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default);

        protected virtual List<ChatMessage> ValidateMessages(IEnumerable<ChatMessage> messages, long maxRequestSize)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var messageList = messages.ToList();
            if (!messageList.Any())
                throw new ArgumentException("Message list cannot be empty.", nameof(messages));

            long totalLength = 0;
            foreach (var message in messageList)
            {
                if (message == null)
                    throw new ArgumentException("The message collection cannot contain null elements.", nameof(messages));
                if (string.IsNullOrWhiteSpace(message.Content))
                    throw new ArgumentException("ChatMessage content cannot be null or whitespace.", nameof(messages));

                totalLength += message.Content.Length;
                if (totalLength > maxRequestSize)
                    throw new ArgumentException($"Total message content size exceeds the configured limit of {maxRequestSize} characters.");
            }
            return messageList;
        }

        protected virtual async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> operation,
            int maxRetries,
            Func<Exception, bool> isRetriableError,
            CancellationToken cancellationToken)
        {
            var retryCount = 0;
            Exception? lastException = null;

            while (retryCount <= maxRetries)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (ex is not OperationCanceledException && isRetriableError(ex))
                {
                    lastException = ex;
                    if (retryCount >= maxRetries) break;

                    var delay = TimeSpan.FromMilliseconds(Math.Min(1000 * Math.Pow(2, retryCount), 30000));
                    Logger.LogWarning("Operation failed with retriable error, retrying in {Delay}ms (attempt {Attempt}/{MaxRetries}): {Error}",
                        delay.TotalMilliseconds, retryCount + 1, maxRetries, ex.Message);

                    await Task.Delay(delay, cancellationToken);
                    retryCount++;
                }
            }
            throw lastException ?? new InvalidOperationException("Retry logic completed without a captured exception.");
        }
    }
}