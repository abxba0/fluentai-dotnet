using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Default implementation of parallelized batch processing for AI operations.
/// </summary>
public class DefaultBatchProcessor : IBatchProcessor
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<DefaultBatchProcessor> _logger;

    /// <summary>
    /// Initializes a new instance of the DefaultBatchProcessor class.
    /// </summary>
    /// <param name="chatModel">Chat model for processing requests.</param>
    /// <param name="logger">Logger instance.</param>
    public DefaultBatchProcessor(IChatModel chatModel, ILogger<DefaultBatchProcessor> logger)
    {
        _chatModel = chatModel ?? throw new ArgumentNullException(nameof(chatModel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<BatchResult<ChatResponse>>> ProcessBatchAsync(
        IEnumerable<BatchRequest> requests,
        BatchProcessingOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        
        var requestList = requests.ToList();
        if (requestList.Count == 0)
        {
            return Array.Empty<BatchResult<ChatResponse>>();
        }

        options ??= new BatchProcessingOptions();
        
        _logger.LogInformation("Starting batch processing of {Count} requests with max parallelism {MaxParallelism}",
            requestList.Count, options.MaxDegreeOfParallelism);

        var results = new ConcurrentDictionary<int, BatchResult<ChatResponse>>();
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = options.MaxDegreeOfParallelism,
            CancellationToken = cancellationToken
        };

        try
        {
            await Parallel.ForEachAsync(
                requestList.Select((req, idx) => (Request: req, Index: idx)),
                parallelOptions,
                async (item, ct) =>
                {
                    if (options.StopOnFirstError && results.Values.Any(r => !r.IsSuccess))
                    {
                        return;
                    }

                    var result = await ProcessSingleRequestAsync(item.Request, item.Index, options, ct)
                        .ConfigureAwait(false);
                    results[item.Index] = result;
                }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Batch processing was cancelled");
            throw;
        }

        var orderedResults = options.PreserveOrder
            ? results.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToList()
            : results.Values.ToList();

        var successCount = orderedResults.Count(r => r.IsSuccess);
        _logger.LogInformation("Batch processing completed: {Success}/{Total} successful",
            successCount, requestList.Count);

        return orderedResults;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<BatchResult<ChatResponse>> ProcessBatchStreamAsync(
        IEnumerable<BatchRequest> requests,
        BatchProcessingOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        var requestList = requests.ToList();
        if (requestList.Count == 0)
        {
            yield break;
        }

        options ??= new BatchProcessingOptions();

        _logger.LogInformation("Starting streaming batch processing of {Count} requests", requestList.Count);

        var semaphore = new SemaphoreSlim(options.MaxDegreeOfParallelism);
        var tasks = new List<Task<BatchResult<ChatResponse>>>();

        for (int i = 0; i < requestList.Count; i++)
        {
            var request = requestList[i];
            var index = i;

            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            var task = Task.Run(async () =>
            {
                try
                {
                    return await ProcessSingleRequestAsync(request, index, options, cancellationToken)
                        .ConfigureAwait(false);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken);

            tasks.Add(task);
        }

        // Yield results as they complete
        while (tasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
            tasks.Remove(completedTask);

            BatchResult<ChatResponse> result;
            try
            {
                result = await completedTask.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing batch request");
                continue;
            }

            yield return result;

            if (options.StopOnFirstError && !result.IsSuccess)
            {
                _logger.LogWarning("Stopping batch processing due to error in request {Id}", result.Id);
                break;
            }
        }

        semaphore.Dispose();
    }

    private async Task<BatchResult<ChatResponse>> ProcessSingleRequestAsync(
        BatchRequest request,
        int index,
        BatchProcessingOptions options,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var attempt = 0;
        Exception? lastError = null;

        while (attempt <= (options.RetryFailedRequests ? options.MaxRetryAttempts : 0))
        {
            try
            {
                if (attempt > 0)
                {
                    await Task.Delay(options.RetryDelay * attempt, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Retrying request {Id}, attempt {Attempt}", request.Id, attempt + 1);
                }

                using var timeoutCts = new CancellationTokenSource(options.RequestTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                var response = await _chatModel.GetResponseAsync(
                    request.Messages,
                    request.Options,
                    linkedCts.Token).ConfigureAwait(false);

                stopwatch.Stop();

                return new BatchResult<ChatResponse>
                {
                    Id = request.Id,
                    IsSuccess = true,
                    Data = response,
                    Duration = stopwatch.Elapsed,
                    Index = index
                };
            }
            catch (Exception ex)
            {
                lastError = ex;
                _logger.LogWarning(ex, "Error processing request {Id}, attempt {Attempt}", request.Id, attempt + 1);
                attempt++;
            }
        }

        stopwatch.Stop();

        return new BatchResult<ChatResponse>
        {
            Id = request.Id,
            IsSuccess = false,
            Error = lastError,
            Duration = stopwatch.Elapsed,
            Index = index
        };
    }
}
