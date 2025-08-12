namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents a boundary violation in input processing.
    /// </summary>
    public record BoundaryViolation
    {
        /// <summary>
        /// Gets or sets the type of boundary violation.
        /// </summary>
        public BoundaryViolationType ViolationType { get; init; }

        /// <summary>
        /// Gets or sets the input parameter or field involved.
        /// </summary>
        public string InputParameter { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the boundary that was violated.
        /// </summary>
        public string ViolatedBoundary { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the actual value that caused the violation.
        /// </summary>
        public string ActualValue { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected range or constraint.
        /// </summary>
        public string ExpectedRange { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the risk level of this violation.
        /// </summary>
        public IssueSeverity RiskLevel { get; init; }
    }

    /// <summary>
    /// Represents an edge case scenario.
    /// </summary>
    public record EdgeCaseScenario
    {
        /// <summary>
        /// Gets or sets the scenario name or description.
        /// </summary>
        public string Scenario { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the test input for this scenario.
        /// </summary>
        public string TestInput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the current output behavior.
        /// </summary>
        public string CurrentOutput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected output behavior.
        /// </summary>
        public string ExpectedOutput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the risk level of this edge case.
        /// </summary>
        public IssueSeverity RiskLevel { get; init; }

        /// <summary>
        /// Gets or sets whether this edge case is handled correctly.
        /// </summary>
        public bool IsHandledCorrectly { get; init; }

        /// <summary>
        /// Gets or sets suggestions for handling this edge case.
        /// </summary>
        public IReadOnlyList<string> HandlingSuggestions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a temporal edge case (timing-related).
    /// </summary>
    public record TemporalEdgeCase
    {
        /// <summary>
        /// Gets or sets the type of temporal edge case.
        /// </summary>
        public TemporalEdgeCaseType CaseType { get; init; }

        /// <summary>
        /// Gets or sets the description of the timing issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the timing dependencies involved.
        /// </summary>
        public IReadOnlyList<string> TimingDependencies { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the timeout scenarios.
        /// </summary>
        public IReadOnlyList<TimeoutScenario> TimeoutScenarios { get; init; } = Array.Empty<TimeoutScenario>();

        /// <summary>
        /// Gets or sets interrupt handling issues.
        /// </summary>
        public IReadOnlyList<string> InterruptHandlingIssues { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the severity of this temporal edge case.
        /// </summary>
        public IssueSeverity Severity { get; init; }
    }

    /// <summary>
    /// Represents a timeout scenario.
    /// </summary>
    public record TimeoutScenario
    {
        /// <summary>
        /// Gets or sets the operation that can timeout.
        /// </summary>
        public string Operation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the timeout duration.
        /// </summary>
        public TimeSpan TimeoutDuration { get; init; }

        /// <summary>
        /// Gets or sets what happens when timeout occurs.
        /// </summary>
        public string TimeoutBehavior { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the timeout is handled gracefully.
        /// </summary>
        public bool IsHandledGracefully { get; init; }

        /// <summary>
        /// Gets or sets potential impacts of the timeout.
        /// </summary>
        public IReadOnlyList<string> PotentialImpacts { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Edge case matrix for comprehensive boundary analysis.
    /// </summary>
    public record EdgeCaseMatrix
    {
        /// <summary>
        /// Gets or sets the component being analyzed.
        /// </summary>
        public string Component { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets input boundary scenarios.
        /// </summary>
        public IReadOnlyList<InputBoundaryScenario> InputBoundaries { get; init; } = Array.Empty<InputBoundaryScenario>();

        /// <summary>
        /// Gets or sets temporal edge cases.
        /// </summary>
        public IReadOnlyList<TemporalEdgeCase> TemporalEdgeCases { get; init; } = Array.Empty<TemporalEdgeCase>();

        /// <summary>
        /// Gets or sets type mismatch scenarios.
        /// </summary>
        public IReadOnlyList<TypeMismatchScenario> TypeMismatches { get; init; } = Array.Empty<TypeMismatchScenario>();
    }

    /// <summary>
    /// Represents an input boundary scenario.
    /// </summary>
    public record InputBoundaryScenario
    {
        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string Scenario { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the test input.
        /// </summary>
        public string TestInput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets whether overflow/underflow risk exists.
        /// </summary>
        public bool HasOverflowRisk { get; init; }

        /// <summary>
        /// Gets or sets the memory impact of this scenario.
        /// </summary>
        public string MemoryImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the current handling approach.
        /// </summary>
        public string CurrentHandling { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the improved handling approach.
        /// </summary>
        public string ImprovedHandling { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a type mismatch scenario.
    /// </summary>
    public record TypeMismatchScenario
    {
        /// <summary>
        /// Gets or sets the invalid types that could be passed.
        /// </summary>
        public IReadOnlyList<string> InvalidTypes { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets how it's currently handled.
        /// </summary>
        public string CurrentHandling { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the improved handling approach.
        /// </summary>
        public string ImprovedHandling { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the risk level of type mismatches.
        /// </summary>
        public IssueSeverity RiskLevel { get; init; }
    }

    /// <summary>
    /// Types of boundary violations.
    /// </summary>
    public enum BoundaryViolationType
    {
        /// <summary>
        /// Empty or null input where value is required.
        /// </summary>
        EmptyOrNull,

        /// <summary>
        /// Value exceeds maximum allowed.
        /// </summary>
        ExceedsMaximum,

        /// <summary>
        /// Value below minimum allowed.
        /// </summary>
        BelowMinimum,

        /// <summary>
        /// Buffer overflow potential.
        /// </summary>
        BufferOverflow,

        /// <summary>
        /// Array index out of bounds.
        /// </summary>
        IndexOutOfBounds,

        /// <summary>
        /// Invalid format or pattern.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// Type mismatch.
        /// </summary>
        TypeMismatch
    }

    /// <summary>
    /// Types of temporal edge cases.
    /// </summary>
    public enum TemporalEdgeCaseType
    {
        /// <summary>
        /// Race condition timing issue.
        /// </summary>
        RaceCondition,

        /// <summary>
        /// Operation timeout.
        /// </summary>
        Timeout,

        /// <summary>
        /// Interrupt handling issue.
        /// </summary>
        InterruptHandling,

        /// <summary>
        /// Timing dependency issue.
        /// </summary>
        TimingDependency,

        /// <summary>
        /// Clock synchronization issue.
        /// </summary>
        ClockSynchronization,

        /// <summary>
        /// Event ordering issue.
        /// </summary>
        EventOrdering
    }
}