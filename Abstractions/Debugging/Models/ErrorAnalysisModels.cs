namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents an error propagation chain through the system.
    /// </summary>
    public record ErrorPropagationChain
    {
        /// <summary>
        /// Gets or sets the fault injection point where the error originates.
        /// </summary>
        public string FaultInjectionPoint { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the propagation path through the system.
        /// </summary>
        public IReadOnlyList<PropagationStep> PropagationPath { get; init; } = Array.Empty<PropagationStep>();

        /// <summary>
        /// Gets or sets the final error resolution point.
        /// </summary>
        public string ResolutionPoint { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the error type at the origin.
        /// </summary>
        public ErrorType ErrorType { get; init; }

        /// <summary>
        /// Gets or sets the detection method for this error.
        /// </summary>
        public string DetectionMethod { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the potential for error masking.
        /// </summary>
        public bool HasMaskingRisk { get; init; }
    }

    /// <summary>
    /// Represents a step in error propagation.
    /// </summary>
    public record PropagationStep
    {
        /// <summary>
        /// Gets or sets the component where this step occurs.
        /// </summary>
        public string Component { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets how the error is transformed at this step.
        /// </summary>
        public string ErrorTransformation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the risk of masking at this step.
        /// </summary>
        public bool MaskingRisk { get; init; }

        /// <summary>
        /// Gets or sets the handling approach at this step.
        /// </summary>
        public string HandlingApproach { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents an exception safety issue.
    /// </summary>
    public record ExceptionSafetyIssue
    {
        /// <summary>
        /// Gets or sets the type of exception safety issue.
        /// </summary>
        public ExceptionSafetyIssueType IssueType { get; init; }

        /// <summary>
        /// Gets or sets the location where the issue occurs.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets potential resource leaks.
        /// </summary>
        public IReadOnlyList<string> ResourceLeaks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets partial update risks.
        /// </summary>
        public IReadOnlyList<string> PartialUpdateRisks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the rollback capability.
        /// </summary>
        public RollbackCapability RollbackCapability { get; init; } = new();

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public IssueSeverity Severity { get; init; }
    }

    /// <summary>
    /// Represents a recovery strategy for error handling.
    /// </summary>
    public record RecoveryStrategy
    {
        /// <summary>
        /// Gets or sets the strategy name.
        /// </summary>
        public string StrategyName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of recovery strategy.
        /// </summary>
        public RecoveryStrategyType StrategyType { get; init; }

        /// <summary>
        /// Gets or sets the description of the strategy.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the implementation steps.
        /// </summary>
        public IReadOnlyList<string> ImplementationSteps { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the effectiveness rating (0.0 to 1.0).
        /// </summary>
        public double Effectiveness { get; init; }

        /// <summary>
        /// Gets or sets the implementation complexity.
        /// </summary>
        public ImplementationComplexity Complexity { get; init; }

        /// <summary>
        /// Gets or sets the user impact of this strategy.
        /// </summary>
        public string UserImpact { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a performance bottleneck.
    /// </summary>
    public record PerformanceBottleneck
    {
        /// <summary>
        /// Gets or sets the location of the bottleneck.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the type of bottleneck.
        /// </summary>
        public BottleneckType BottleneckType { get; init; }

        /// <summary>
        /// Gets or sets the description of the bottleneck.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the performance impact (percentage degradation).
        /// </summary>
        public double PerformanceImpact { get; init; }

        /// <summary>
        /// Gets or sets the CPU hotspots.
        /// </summary>
        public IReadOnlyList<string> CpuHotspots { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets memory usage patterns.
        /// </summary>
        public string MemoryUsagePattern { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the scalability breaking points.
        /// </summary>
        public IReadOnlyList<ScalabilityBreakingPoint> BreakingPoints { get; init; } = Array.Empty<ScalabilityBreakingPoint>();
    }

    /// <summary>
    /// Represents a resource management issue.
    /// </summary>
    public record ResourceManagementIssue
    {
        /// <summary>
        /// Gets or sets the type of resource management issue.
        /// </summary>
        public ResourceIssueType IssueType { get; init; }

        /// <summary>
        /// Gets or sets the affected resource.
        /// </summary>
        public string AffectedResource { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the issue occurs.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the leak rate if applicable.
        /// </summary>
        public string LeakRate { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets remediation suggestions.
        /// </summary>
        public IReadOnlyList<string> RemediationSuggestions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents algorithmic complexity analysis.
    /// </summary>
    public record AlgorithmicComplexityAnalysis
    {
        /// <summary>
        /// Gets or sets the time complexity analysis.
        /// </summary>
        public string TimeComplexity { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the space complexity analysis.
        /// </summary>
        public string SpaceComplexity { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets identified scalability issues.
        /// </summary>
        public IReadOnlyList<string> ScalabilityIssues { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets optimization opportunities.
        /// </summary>
        public IReadOnlyList<string> OptimizationOpportunities { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the complexity score (0-100, lower is better).
        /// </summary>
        public int ComplexityScore { get; init; }
    }

    /// <summary>
    /// Represents a scalability breaking point.
    /// </summary>
    public record ScalabilityBreakingPoint
    {
        /// <summary>
        /// Gets or sets the metric that breaks.
        /// </summary>
        public string Metric { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the threshold value.
        /// </summary>
        public string Threshold { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the consequences of reaching this threshold.
        /// </summary>
        public string Consequences { get; init; } = string.Empty;
    }

    /// <summary>
    /// Rollback capability assessment.
    /// </summary>
    public record RollbackCapability
    {
        /// <summary>
        /// Gets or sets whether rollback is possible.
        /// </summary>
        public bool CanRollback { get; init; }

        /// <summary>
        /// Gets or sets the rollback method.
        /// </summary>
        public string RollbackMethod { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the rollback completeness (percentage).
        /// </summary>
        public double CompletenessPercentage { get; init; }

        /// <summary>
        /// Gets or sets rollback limitations.
        /// </summary>
        public IReadOnlyList<string> Limitations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Types of errors in propagation analysis.
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// Exception thrown.
        /// </summary>
        Exception,

        /// <summary>
        /// Error return code.
        /// </summary>
        ReturnCode,

        /// <summary>
        /// Silent failure.
        /// </summary>
        SilentFailure,

        /// <summary>
        /// Data corruption.
        /// </summary>
        DataCorruption,

        /// <summary>
        /// Resource exhaustion.
        /// </summary>
        ResourceExhaustion,

        /// <summary>
        /// Timeout.
        /// </summary>
        Timeout
    }

    /// <summary>
    /// Types of exception safety issues.
    /// </summary>
    public enum ExceptionSafetyIssueType
    {
        /// <summary>
        /// Resource leak on exception.
        /// </summary>
        ResourceLeak,

        /// <summary>
        /// Partial state update.
        /// </summary>
        PartialUpdate,

        /// <summary>
        /// Inconsistent state.
        /// </summary>
        InconsistentState,

        /// <summary>
        /// Missing cleanup.
        /// </summary>
        MissingCleanup,

        /// <summary>
        /// Exception safety guarantee violation.
        /// </summary>
        SafetyGuaranteeViolation
    }

    /// <summary>
    /// Types of recovery strategies.
    /// </summary>
    public enum RecoveryStrategyType
    {
        /// <summary>
        /// Retry the operation.
        /// </summary>
        Retry,

        /// <summary>
        /// Fallback to alternative approach.
        /// </summary>
        Fallback,

        /// <summary>
        /// Graceful degradation.
        /// </summary>
        GracefulDegradation,

        /// <summary>
        /// Circuit breaker pattern.
        /// </summary>
        CircuitBreaker,

        /// <summary>
        /// Compensation transaction.
        /// </summary>
        Compensation,

        /// <summary>
        /// Manual intervention required.
        /// </summary>
        ManualIntervention
    }

    /// <summary>
    /// Implementation complexity levels.
    /// </summary>
    public enum ImplementationComplexity
    {
        /// <summary>
        /// Simple implementation.
        /// </summary>
        Simple = 1,

        /// <summary>
        /// Moderate implementation complexity.
        /// </summary>
        Moderate = 2,

        /// <summary>
        /// Complex implementation.
        /// </summary>
        Complex = 3,

        /// <summary>
        /// Very complex implementation.
        /// </summary>
        VeryComplex = 4
    }

    /// <summary>
    /// Types of performance bottlenecks.
    /// </summary>
    public enum BottleneckType
    {
        /// <summary>
        /// CPU-bound bottleneck.
        /// </summary>
        CpuBound,

        /// <summary>
        /// Memory-bound bottleneck.
        /// </summary>
        MemoryBound,

        /// <summary>
        /// I/O-bound bottleneck.
        /// </summary>
        IoBound,

        /// <summary>
        /// Network-bound bottleneck.
        /// </summary>
        NetworkBound,

        /// <summary>
        /// Lock contention bottleneck.
        /// </summary>
        LockContention,

        /// <summary>
        /// Algorithmic inefficiency.
        /// </summary>
        AlgorithmicInefficiency
    }

    /// <summary>
    /// Types of resource management issues.
    /// </summary>
    public enum ResourceIssueType
    {
        /// <summary>
        /// Memory leak.
        /// </summary>
        MemoryLeak,

        /// <summary>
        /// Handle leak.
        /// </summary>
        HandleLeak,

        /// <summary>
        /// Connection leak.
        /// </summary>
        ConnectionLeak,

        /// <summary>
        /// File handle exhaustion.
        /// </summary>
        FileHandleExhaustion,

        /// <summary>
        /// Thread pool exhaustion.
        /// </summary>
        ThreadPoolExhaustion,

        /// <summary>
        /// Excessive resource consumption.
        /// </summary>
        ExcessiveConsumption
    }
}