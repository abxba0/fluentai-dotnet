using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Security;
using FluentAI.Abstractions.Performance;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Base class for chat model implementations providing common functionality.
    /// </summary>
    public abstract class ChatModelBase : IChatModel
    {
        /// <summary>
        /// Logger instance for this chat model.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Input sanitizer for security validation.
        /// </summary>
        protected readonly IInputSanitizer InputSanitizer;

        /// <summary>
        /// Performance monitor for tracking operation metrics.
        /// </summary>
        protected readonly IPerformanceMonitor PerformanceMonitor;

        /// <summary>
        /// Initializes a new instance of the ChatModelBase class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="inputSanitizer">Input sanitizer for security validation.</param>
        /// <param name="performanceMonitor">Performance monitor for tracking metrics.</param>
        protected ChatModelBase(ILogger logger, IInputSanitizer? inputSanitizer = null, IPerformanceMonitor? performanceMonitor = null)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InputSanitizer = inputSanitizer ?? new DefaultInputSanitizer(logger as ILogger<DefaultInputSanitizer> ?? throw new InvalidOperationException("Cannot create default input sanitizer without compatible logger"));
            PerformanceMonitor = performanceMonitor ?? new DefaultPerformanceMonitor(logger as ILogger<DefaultPerformanceMonitor> ?? throw new InvalidOperationException("Cannot create default performance monitor without compatible logger"));
        }

        /// <summary>
        /// Gets a complete response from the model for a given series of messages.
        /// </summary>
        public abstract Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Streams a response from the model token-by-token.
        /// </summary>
        public abstract IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates the input messages and ensures they don't exceed size limits.
        /// Also performs security validation to prevent prompt injection.
        /// </summary>
        /// <param name="messages">Messages to validate.</param>
        /// <param name="maxRequestSize">Maximum allowed total content size.</param>
        /// <param name="enableSanitization">Whether to sanitize potentially dangerous content.</param>
        /// <returns>Validated list of messages.</returns>
        protected virtual List<ChatMessage> ValidateMessages(IEnumerable<ChatMessage> messages, long maxRequestSize, bool enableSanitization = true)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var messageList = messages.ToList();
            if (!messageList.Any())
                throw new ArgumentException("Message list cannot be empty.", nameof(messages));

            long totalLength = 0;
            var validatedMessages = new List<ChatMessage>(messageList.Count);

            foreach (var message in messageList)
            {
                if (message == null)
                    throw new ArgumentException("The message collection cannot contain null elements.", nameof(messages));
                if (string.IsNullOrWhiteSpace(message.Content))
                    throw new ArgumentException("ChatMessage content cannot be null or whitespace.", nameof(messages));

                var content = message.Content;
                
                // Security validation and sanitization
                if (enableSanitization)
                {
                    var riskAssessment = InputSanitizer.AssessRisk(content);
                    if (riskAssessment.ShouldBlock)
                    {
                        Logger.LogWarning("Blocking message due to security risk: {RiskLevel}. Concerns: {Concerns}", 
                            riskAssessment.RiskLevel, string.Join(", ", riskAssessment.DetectedConcerns));
                        throw new Exceptions.AiSdkException($"Message content blocked due to security concerns: {string.Join(", ", riskAssessment.DetectedConcerns)}");
                    }

                    if (riskAssessment.RiskLevel >= SecurityRiskLevel.Medium)
                    {
                        Logger.LogInformation("Sanitizing message content due to security risk: {RiskLevel}", riskAssessment.RiskLevel);
                        content = InputSanitizer.SanitizeContent(content);
                    }
                }

                totalLength += content.Length;
                if (totalLength > maxRequestSize)
                    throw new ArgumentException($"Total message content size exceeds the configured limit of {maxRequestSize} characters.");

                validatedMessages.Add(message with { Content = content });
            }
            
            return validatedMessages;
        }

        /// <summary>
        /// Executes an operation with retry logic for transient failures.
        /// </summary>
        /// <typeparam name="T">Return type of the operation.</typeparam>
        /// <param name="operation">The operation to execute.</param>
        /// <param name="maxRetries">Maximum number of retry attempts.</param>
        /// <param name="isRetriableError">Function to determine if an error is retriable.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result of the operation.</returns>
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