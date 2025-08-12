namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Results from flow analysis operations.
    /// </summary>
    public record FlowAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets the execution paths identified during analysis.
        /// </summary>
        public IReadOnlyList<ExecutionPath> ExecutionPaths { get; init; } = Array.Empty<ExecutionPath>();

        /// <summary>
        /// Gets or sets control flow issues found during analysis.
        /// </summary>
        public IReadOnlyList<ControlFlowIssue> ControlFlowIssues { get; init; } = Array.Empty<ControlFlowIssue>();

        /// <summary>
        /// Gets or sets invariants that should be maintained throughout execution.
        /// </summary>
        public IReadOnlyList<Invariant> Invariants { get; init; } = Array.Empty<Invariant>();
    }

    /// <summary>
    /// Results from state management analysis.
    /// </summary>
    public record StateAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets state transitions identified during analysis.
        /// </summary>
        public IReadOnlyList<StateTransition> StateTransitions { get; init; } = Array.Empty<StateTransition>();

        /// <summary>
        /// Gets or sets concurrent access issues found.
        /// </summary>
        public IReadOnlyList<ConcurrencyIssue> ConcurrencyIssues { get; init; } = Array.Empty<ConcurrencyIssue>();

        /// <summary>
        /// Gets or sets data integrity violations found.
        /// </summary>
        public IReadOnlyList<DataIntegrityIssue> DataIntegrityIssues { get; init; } = Array.Empty<DataIntegrityIssue>();
    }

    /// <summary>
    /// Results from edge case and boundary analysis.
    /// </summary>
    public record EdgeCaseAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets input boundary violations found.
        /// </summary>
        public IReadOnlyList<BoundaryViolation> BoundaryViolations { get; init; } = Array.Empty<BoundaryViolation>();

        /// <summary>
        /// Gets or sets edge case scenarios identified.
        /// </summary>
        public IReadOnlyList<EdgeCaseScenario> EdgeCaseScenarios { get; init; } = Array.Empty<EdgeCaseScenario>();

        /// <summary>
        /// Gets or sets temporal edge cases found.
        /// </summary>
        public IReadOnlyList<TemporalEdgeCase> TemporalEdgeCases { get; init; } = Array.Empty<TemporalEdgeCase>();
    }

    /// <summary>
    /// Results from error propagation analysis.
    /// </summary>
    public record ErrorAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets error propagation chains identified.
        /// </summary>
        public IReadOnlyList<ErrorPropagationChain> PropagationChains { get; init; } = Array.Empty<ErrorPropagationChain>();

        /// <summary>
        /// Gets or sets exception safety issues found.
        /// </summary>
        public IReadOnlyList<ExceptionSafetyIssue> ExceptionSafetyIssues { get; init; } = Array.Empty<ExceptionSafetyIssue>();

        /// <summary>
        /// Gets or sets recovery strategies available.
        /// </summary>
        public IReadOnlyList<RecoveryStrategy> RecoveryStrategies { get; init; } = Array.Empty<RecoveryStrategy>();
    }

    /// <summary>
    /// Results from performance analysis.
    /// </summary>
    public record PerformanceAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets performance bottlenecks identified.
        /// </summary>
        public IReadOnlyList<PerformanceBottleneck> Bottlenecks { get; init; } = Array.Empty<PerformanceBottleneck>();

        /// <summary>
        /// Gets or sets resource management issues found.
        /// </summary>
        public IReadOnlyList<ResourceManagementIssue> ResourceIssues { get; init; } = Array.Empty<ResourceManagementIssue>();

        /// <summary>
        /// Gets or sets algorithmic complexity analysis.
        /// </summary>
        public AlgorithmicComplexityAnalysis ComplexityAnalysis { get; init; } = new();
    }

    /// <summary>
    /// Comprehensive analysis result combining all analysis types.
    /// </summary>
    public record ComprehensiveAnalysisResult : AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets flow analysis results.
        /// </summary>
        public FlowAnalysisResult FlowAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets state analysis results.
        /// </summary>
        public StateAnalysisResult StateAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets edge case analysis results.
        /// </summary>
        public EdgeCaseAnalysisResult EdgeCaseAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets error analysis results.
        /// </summary>
        public ErrorAnalysisResult ErrorAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets performance analysis results.
        /// </summary>
        public PerformanceAnalysisResult PerformanceAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets the overall analysis summary.
        /// </summary>
        public AnalysisSummary Summary { get; init; } = new();
    }

    /// <summary>
    /// Base class for all analysis results.
    /// </summary>
    public abstract record AnalysisResultBase
    {
        /// <summary>
        /// Gets or sets the timestamp when the analysis was performed.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the duration of the analysis operation.
        /// </summary>
        public TimeSpan Duration { get; init; }

        /// <summary>
        /// Gets or sets whether the analysis completed successfully.
        /// </summary>
        public bool IsSuccessful { get; init; } = true;

        /// <summary>
        /// Gets or sets any error messages from the analysis.
        /// </summary>
        public IReadOnlyList<string> ErrorMessages { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets additional metadata about the analysis.
        /// </summary>
        public Dictionary<string, object> Metadata { get; init; } = new();
    }

    /// <summary>
    /// Summary of comprehensive analysis results.
    /// </summary>
    public record AnalysisSummary
    {
        /// <summary>
        /// Gets or sets the total number of issues found.
        /// </summary>
        public int TotalIssuesFound { get; init; }

        /// <summary>
        /// Gets or sets the number of critical issues.
        /// </summary>
        public int CriticalIssues { get; init; }

        /// <summary>
        /// Gets or sets the number of high priority issues.
        /// </summary>
        public int HighPriorityIssues { get; init; }

        /// <summary>
        /// Gets or sets the number of medium priority issues.
        /// </summary>
        public int MediumPriorityIssues { get; init; }

        /// <summary>
        /// Gets or sets the number of low priority issues.
        /// </summary>
        public int LowPriorityIssues { get; init; }

        /// <summary>
        /// Gets or sets the overall health score (0-100).
        /// </summary>
        public int HealthScore { get; init; }

        /// <summary>
        /// Gets or sets recommendations for addressing the issues.
        /// </summary>
        public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
    }
}