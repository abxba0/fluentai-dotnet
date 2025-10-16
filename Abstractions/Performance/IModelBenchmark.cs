using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Provides benchmarking capabilities for comparing AI model performance.
/// </summary>
public interface IModelBenchmark
{
    /// <summary>
    /// Runs a benchmark comparison across multiple models.
    /// </summary>
    /// <param name="models">Models to benchmark.</param>
    /// <param name="testCases">Test cases to run against each model.</param>
    /// <param name="options">Benchmark options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Benchmark results for each model.</returns>
    Task<BenchmarkReport> RunBenchmarkAsync(
        IEnumerable<ModelConfig> models,
        IEnumerable<BenchmarkTestCase> testCases,
        BenchmarkOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a single model benchmark.
    /// </summary>
    /// <param name="model">Model configuration to benchmark.</param>
    /// <param name="testCases">Test cases to run.</param>
    /// <param name="options">Benchmark options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Benchmark results for the model.</returns>
    Task<ModelBenchmarkResult> BenchmarkModelAsync(
        ModelConfig model,
        IEnumerable<BenchmarkTestCase> testCases,
        BenchmarkOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for a model to benchmark.
/// </summary>
public class ModelConfig
{
    /// <summary>
    /// Model identifier.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Provider name.
    /// </summary>
    public required string Provider { get; init; }

    /// <summary>
    /// Display name for reporting.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Chat model instance.
    /// </summary>
    public required IChatModel ChatModel { get; init; }
}

/// <summary>
/// A test case for benchmarking.
/// </summary>
public class BenchmarkTestCase
{
    /// <summary>
    /// Test case identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Test case description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Messages to send.
    /// </summary>
    public required IEnumerable<ChatMessage> Messages { get; init; }

    /// <summary>
    /// Expected response characteristics for validation.
    /// </summary>
    public ResponseExpectations? Expectations { get; init; }
}

/// <summary>
/// Expected characteristics of a response for validation.
/// </summary>
public class ResponseExpectations
{
    /// <summary>
    /// Minimum expected response length.
    /// </summary>
    public int? MinLength { get; init; }

    /// <summary>
    /// Maximum expected response length.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Keywords that should appear in the response.
    /// </summary>
    public IEnumerable<string>? MustContain { get; init; }

    /// <summary>
    /// Keywords that should not appear in the response.
    /// </summary>
    public IEnumerable<string>? MustNotContain { get; init; }
}

/// <summary>
/// Complete benchmark report across all models.
/// </summary>
public class BenchmarkReport
{
    /// <summary>
    /// Timestamp when the benchmark started.
    /// </summary>
    public DateTime StartTime { get; init; }

    /// <summary>
    /// Total duration of the benchmark.
    /// </summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>
    /// Results for each model.
    /// </summary>
    public required IReadOnlyList<ModelBenchmarkResult> Results { get; init; }

    /// <summary>
    /// Summary statistics comparing all models.
    /// </summary>
    public required BenchmarkSummary Summary { get; init; }
}

/// <summary>
/// Benchmark results for a single model.
/// </summary>
public class ModelBenchmarkResult
{
    /// <summary>
    /// Model configuration.
    /// </summary>
    public required ModelConfig Model { get; init; }

    /// <summary>
    /// Results for each test case.
    /// </summary>
    public required IReadOnlyList<TestCaseResult> TestCaseResults { get; init; }

    /// <summary>
    /// Aggregated statistics for this model.
    /// </summary>
    public required ModelStatistics Statistics { get; init; }
}

/// <summary>
/// Result of a single test case execution.
/// </summary>
public class TestCaseResult
{
    /// <summary>
    /// Test case identifier.
    /// </summary>
    public required string TestCaseId { get; init; }

    /// <summary>
    /// Whether the test completed successfully.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Response from the model.
    /// </summary>
    public ChatResponse? Response { get; init; }

    /// <summary>
    /// Error if failed.
    /// </summary>
    public Exception? Error { get; init; }

    /// <summary>
    /// Time to first token (for streaming).
    /// </summary>
    public TimeSpan? TimeToFirstToken { get; init; }

    /// <summary>
    /// Total response time.
    /// </summary>
    public TimeSpan ResponseTime { get; init; }

    /// <summary>
    /// Whether response met expectations.
    /// </summary>
    public bool MetExpectations { get; init; }

    /// <summary>
    /// Validation failures if any.
    /// </summary>
    public IReadOnlyList<string>? ValidationFailures { get; init; }
}

/// <summary>
/// Aggregated statistics for a model's performance.
/// </summary>
public class ModelStatistics
{
    /// <summary>
    /// Average response time.
    /// </summary>
    public TimeSpan AverageResponseTime { get; init; }

    /// <summary>
    /// Median response time.
    /// </summary>
    public TimeSpan MedianResponseTime { get; init; }

    /// <summary>
    /// 95th percentile response time.
    /// </summary>
    public TimeSpan P95ResponseTime { get; init; }

    /// <summary>
    /// Success rate (0-1).
    /// </summary>
    public double SuccessRate { get; init; }

    /// <summary>
    /// Average tokens per second (if available).
    /// </summary>
    public double? AverageTokensPerSecond { get; init; }

    /// <summary>
    /// Total tokens used.
    /// </summary>
    public int? TotalTokens { get; init; }

    /// <summary>
    /// Estimated cost (if cost data available).
    /// </summary>
    public decimal? EstimatedCost { get; init; }
}

/// <summary>
/// Summary comparing all benchmarked models.
/// </summary>
public class BenchmarkSummary
{
    /// <summary>
    /// Model with the fastest average response time.
    /// </summary>
    public string? FastestModel { get; init; }

    /// <summary>
    /// Model with the highest success rate.
    /// </summary>
    public string? MostReliableModel { get; init; }

    /// <summary>
    /// Model with the best cost efficiency.
    /// </summary>
    public string? MostCostEfficient { get; init; }

    /// <summary>
    /// Model with highest quality (met expectations).
    /// </summary>
    public string? HighestQuality { get; init; }

    /// <summary>
    /// Overall rankings.
    /// </summary>
    public required IReadOnlyList<ModelRanking> Rankings { get; init; }
}

/// <summary>
/// Ranking information for a model.
/// </summary>
public class ModelRanking
{
    /// <summary>
    /// Model identifier.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Overall rank (1-based).
    /// </summary>
    public int Rank { get; init; }

    /// <summary>
    /// Overall score (0-100).
    /// </summary>
    public double Score { get; init; }
}

/// <summary>
/// Options for benchmark execution.
/// </summary>
public class BenchmarkOptions
{
    /// <summary>
    /// Number of times to run each test case. Default is 3.
    /// </summary>
    public int Iterations { get; set; } = 3;

    /// <summary>
    /// Whether to warm up before benchmarking. Default is true.
    /// </summary>
    public bool WarmUp { get; set; } = true;

    /// <summary>
    /// Number of warm-up iterations. Default is 1.
    /// </summary>
    public int WarmUpIterations { get; set; } = 1;

    /// <summary>
    /// Whether to run tests in parallel across models. Default is false.
    /// </summary>
    public bool ParallelExecution { get; set; }

    /// <summary>
    /// Whether to include detailed metrics. Default is true.
    /// </summary>
    public bool IncludeDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Timeout for each test case. Default is 60 seconds.
    /// </summary>
    public TimeSpan TestTimeout { get; set; } = TimeSpan.FromSeconds(60);
}
