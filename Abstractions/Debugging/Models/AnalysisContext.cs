namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Provides context and configuration for code analysis operations.
    /// </summary>
    public record AnalysisContext
    {
        /// <summary>
        /// Gets or sets the component or module being analyzed.
        /// </summary>
        public string ComponentName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the source code or assembly information for analysis.
        /// </summary>
        public string SourceLocation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the analysis configuration and options.
        /// </summary>
        public AnalysisOptions Options { get; init; } = new();

        /// <summary>
        /// Gets or sets runtime context information if available.
        /// </summary>
        public RuntimeContext? RuntimeContext { get; init; }

        /// <summary>
        /// Gets or sets additional metadata for the analysis.
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Configuration options for analysis operations.
    /// </summary>
    public record AnalysisOptions
    {
        /// <summary>
        /// Gets or sets whether to perform deep analysis (slower but more thorough).
        /// </summary>
        public bool DeepAnalysis { get; init; } = false;

        /// <summary>
        /// Gets or sets whether to include performance analysis.
        /// </summary>
        public bool IncludePerformanceAnalysis { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to analyze concurrency issues.
        /// </summary>
        public bool AnalyzeConcurrency { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to check for security vulnerabilities.
        /// </summary>
        public bool CheckSecurity { get; init; } = true;

        /// <summary>
        /// Gets or sets the maximum analysis time before timeout.
        /// </summary>
        public TimeSpan AnalysisTimeout { get; init; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Gets or sets the verbosity level for analysis results.
        /// </summary>
        public AnalysisVerbosity Verbosity { get; init; } = AnalysisVerbosity.Normal;
    }

    /// <summary>
    /// Runtime context information for dynamic analysis.
    /// </summary>
    public record RuntimeContext
    {
        /// <summary>
        /// Gets or sets the current execution state.
        /// </summary>
        public string ExecutionState { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets active threads and their states.
        /// </summary>
        public IReadOnlyList<ThreadInfo> ActiveThreads { get; init; } = Array.Empty<ThreadInfo>();

        /// <summary>
        /// Gets or sets current memory usage information.
        /// </summary>
        public MemoryInfo MemoryInfo { get; init; } = new();

        /// <summary>
        /// Gets or sets current performance metrics.
        /// </summary>
        public PerformanceMetrics PerformanceMetrics { get; init; } = new();
    }

    /// <summary>
    /// Thread information for concurrency analysis.
    /// </summary>
    public record ThreadInfo
    {
        /// <summary>
        /// Gets or sets the thread identifier.
        /// </summary>
        public int ThreadId { get; init; }

        /// <summary>
        /// Gets or sets the thread name.
        /// </summary>
        public string ThreadName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the current thread state.
        /// </summary>
        public string State { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the stack trace if available.
        /// </summary>
        public string? StackTrace { get; init; }
    }

    /// <summary>
    /// Memory usage information.
    /// </summary>
    public record MemoryInfo
    {
        /// <summary>
        /// Gets or sets the total allocated memory in bytes.
        /// </summary>
        public long TotalAllocatedBytes { get; init; }

        /// <summary>
        /// Gets or sets the current working set in bytes.
        /// </summary>
        public long WorkingSetBytes { get; init; }

        /// <summary>
        /// Gets or sets the number of garbage collections.
        /// </summary>
        public long GCCollections { get; init; }
    }

    /// <summary>
    /// Performance metrics information.
    /// </summary>
    public record PerformanceMetrics
    {
        /// <summary>
        /// Gets or sets the CPU usage percentage.
        /// </summary>
        public double CpuUsagePercent { get; init; }

        /// <summary>
        /// Gets or sets the number of requests per second.
        /// </summary>
        public double RequestsPerSecond { get; init; }

        /// <summary>
        /// Gets or sets the average response time in milliseconds.
        /// </summary>
        public double AverageResponseTimeMs { get; init; }
    }

    /// <summary>
    /// Analysis verbosity levels.
    /// </summary>
    public enum AnalysisVerbosity
    {
        /// <summary>
        /// Minimal output with only critical findings.
        /// </summary>
        Minimal = 0,

        /// <summary>
        /// Normal output with important findings.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Detailed output with all findings and explanations.
        /// </summary>
        Detailed = 2,

        /// <summary>
        /// Verbose output with debug information.
        /// </summary>
        Verbose = 3
    }
}