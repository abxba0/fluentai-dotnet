namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents a state transition in the system.
    /// </summary>
    public record StateTransition
    {
        /// <summary>
        /// Gets or sets the operation that triggers this transition.
        /// </summary>
        public string Operation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the state before the transition.
        /// </summary>
        public StateSnapshot BeforeState { get; init; } = new();

        /// <summary>
        /// Gets or sets intermediate states during the transition.
        /// </summary>
        public IReadOnlyList<StateSnapshot> IntermediateStates { get; init; } = Array.Empty<StateSnapshot>();

        /// <summary>
        /// Gets or sets the state after the transition.
        /// </summary>
        public StateSnapshot AfterState { get; init; } = new();

        /// <summary>
        /// Gets or sets validation checks for state consistency.
        /// </summary>
        public IReadOnlyList<StateValidation> Validations { get; init; } = Array.Empty<StateValidation>();
    }

    /// <summary>
    /// Represents a snapshot of system state at a point in time.
    /// </summary>
    public record StateSnapshot
    {
        /// <summary>
        /// Gets or sets the timestamp of this snapshot.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the component or module state.
        /// </summary>
        public string ComponentName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the current state values.
        /// </summary>
        public Dictionary<string, object> StateValues { get; init; } = new();

        /// <summary>
        /// Gets or sets memory usage at this snapshot.
        /// </summary>
        public long MemoryUsageBytes { get; init; }

        /// <summary>
        /// Gets or sets active resources at this snapshot.
        /// </summary>
        public IReadOnlyList<string> ActiveResources { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents state validation checks.
    /// </summary>
    public record StateValidation
    {
        /// <summary>
        /// Gets or sets the validation rule name.
        /// </summary>
        public string RuleName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the validation condition.
        /// </summary>
        public string Condition { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the validation passed.
        /// </summary>
        public bool Passed { get; init; }

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string? ErrorMessage { get; init; }
    }

    /// <summary>
    /// Represents a concurrency issue in the system.
    /// </summary>
    public record ConcurrencyIssue
    {
        /// <summary>
        /// Gets or sets the type of concurrency issue.
        /// </summary>
        public ConcurrencyIssueType IssueType { get; init; }

        /// <summary>
        /// Gets or sets the location where the issue occurs.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the threads involved in the issue.
        /// </summary>
        public IReadOnlyList<int> InvolvedThreads { get; init; } = Array.Empty<int>();

        /// <summary>
        /// Gets or sets the resources involved in the issue.
        /// </summary>
        public IReadOnlyList<string> InvolvedResources { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the severity of the concurrency issue.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets potential solutions for the issue.
        /// </summary>
        public IReadOnlyList<string> PotentialSolutions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a data integrity issue.
    /// </summary>
    public record DataIntegrityIssue
    {
        /// <summary>
        /// Gets or sets the type of data integrity issue.
        /// </summary>
        public DataIntegrityIssueType IssueType { get; init; }

        /// <summary>
        /// Gets or sets the data field or structure affected.
        /// </summary>
        public string AffectedData { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the issue occurs.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected value or constraint.
        /// </summary>
        public string ExpectedConstraint { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the actual value that violates the constraint.
        /// </summary>
        public string ActualValue { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the data integrity issue.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets corruption points where data can be corrupted.
        /// </summary>
        public IReadOnlyList<string> CorruptionPoints { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Types of concurrency issues.
    /// </summary>
    public enum ConcurrencyIssueType
    {
        /// <summary>
        /// Race condition between threads.
        /// </summary>
        RaceCondition,

        /// <summary>
        /// Potential deadlock scenario.
        /// </summary>
        Deadlock,

        /// <summary>
        /// Memory consistency issue.
        /// </summary>
        MemoryConsistency,

        /// <summary>
        /// Thread safety violation.
        /// </summary>
        ThreadSafetyViolation,

        /// <summary>
        /// Resource contention issue.
        /// </summary>
        ResourceContention,

        /// <summary>
        /// Atomic operation violation.
        /// </summary>
        AtomicityViolation
    }

    /// <summary>
    /// Types of data integrity issues.
    /// </summary>
    public enum DataIntegrityIssueType
    {
        /// <summary>
        /// Data corruption detected.
        /// </summary>
        DataCorruption,

        /// <summary>
        /// Missing input validation.
        /// </summary>
        MissingValidation,

        /// <summary>
        /// Boundary violation (array/buffer overrun).
        /// </summary>
        BoundaryViolation,

        /// <summary>
        /// Inconsistent state detected.
        /// </summary>
        InconsistentState,

        /// <summary>
        /// Data loss scenario.
        /// </summary>
        DataLoss,

        /// <summary>
        /// Constraint violation.
        /// </summary>
        ConstraintViolation
    }
}