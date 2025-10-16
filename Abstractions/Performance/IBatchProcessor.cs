using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Provides parallelized batch processing capabilities for AI operations.
/// </summary>
public interface IBatchProcessor
{
    /// <summary>
    /// Processes multiple chat requests in parallel with optimal batching.
    /// </summary>
    /// <param name="requests">Collection of batch requests to process.</param>
    /// <param name="options">Batch processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results for each request in the same order as input.</returns>
    Task<IReadOnlyList<BatchResult<ChatResponse>>> ProcessBatchAsync(
        IEnumerable<BatchRequest> requests,
        BatchProcessingOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes batch requests with streaming results as they complete.
    /// </summary>
    /// <param name="requests">Collection of batch requests to process.</param>
    /// <param name="options">Batch processing options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Stream of results as they complete.</returns>
    IAsyncEnumerable<BatchResult<ChatResponse>> ProcessBatchStreamAsync(
        IEnumerable<BatchRequest> requests,
        BatchProcessingOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a single request in a batch operation.
/// </summary>
public class BatchRequest
{
    /// <summary>
    /// Unique identifier for this request in the batch.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Messages for this chat request.
    /// </summary>
    public required IEnumerable<ChatMessage> Messages { get; set; }

    /// <summary>
    /// Optional request-specific options.
    /// </summary>
    public ChatRequestOptions? Options { get; set; }

    /// <summary>
    /// Priority for this request (higher values processed first). Default is 0.
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// Result of a batch operation request.
/// </summary>
/// <typeparam name="T">Type of the result data.</typeparam>
public class BatchResult<T>
{
    /// <summary>
    /// Request identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Whether the request completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Result data if successful.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error information if failed.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Processing duration for this request.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Index of this request in the original batch.
    /// </summary>
    public int Index { get; init; }
}

/// <summary>
/// Configuration options for batch processing.
/// </summary>
public class BatchProcessingOptions
{
    /// <summary>
    /// Maximum number of requests to process in parallel. Default is 5.
    /// </summary>
    public int MaxDegreeOfParallelism { get; set; } = 5;

    /// <summary>
    /// Whether to stop processing on first error. Default is false.
    /// </summary>
    public bool StopOnFirstError { get; set; }

    /// <summary>
    /// Timeout for each individual request. Default is 2 minutes.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Whether to preserve the order of results. Default is true.
    /// </summary>
    public bool PreserveOrder { get; set; } = true;

    /// <summary>
    /// Whether to retry failed requests. Default is true.
    /// </summary>
    public bool RetryFailedRequests { get; set; } = true;

    /// <summary>
    /// Maximum number of retry attempts per request. Default is 2.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 2;

    /// <summary>
    /// Delay between retry attempts. Default is 1 second.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
}
