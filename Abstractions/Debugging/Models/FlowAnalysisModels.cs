namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents an execution path through the code.
    /// </summary>
    public record ExecutionPath
    {
        /// <summary>
        /// Gets or sets the path description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the entry conditions required for this path.
        /// </summary>
        public IReadOnlyList<string> EntryConditions { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the execution steps in this path.
        /// </summary>
        public IReadOnlyList<ExecutionStep> ExecutionSteps { get; init; } = Array.Empty<ExecutionStep>();

        /// <summary>
        /// Gets or sets the exit conditions after this path.
        /// </summary>
        public IReadOnlyList<string> ExitConditions { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets potential failure points in this path.
        /// </summary>
        public IReadOnlyList<FailurePoint> FailurePoints { get; init; } = Array.Empty<FailurePoint>();
    }

    /// <summary>
    /// Represents a single execution step.
    /// </summary>
    public record ExecutionStep
    {
        /// <summary>
        /// Gets or sets the operation being performed.
        /// </summary>
        public string Operation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the state change caused by this step.
        /// </summary>
        public string StateChange { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the side effects of this step.
        /// </summary>
        public IReadOnlyList<string> SideEffects { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the assumptions made by this step.
        /// </summary>
        public IReadOnlyList<string> Assumptions { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the dependencies required by this step.
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a potential failure point in execution.
    /// </summary>
    public record FailurePoint
    {
        /// <summary>
        /// Gets or sets the location of the failure point.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the potential failure.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the likelihood of failure (0.0 to 1.0).
        /// </summary>
        public double Likelihood { get; init; }

        /// <summary>
        /// Gets or sets the potential impact of the failure.
        /// </summary>
        public IssueImpact Impact { get; init; }

        /// <summary>
        /// Gets or sets mitigation strategies for this failure point.
        /// </summary>
        public IReadOnlyList<string> MitigationStrategies { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a control flow issue.
    /// </summary>
    public record ControlFlowIssue
    {
        /// <summary>
        /// Gets or sets the type of control flow issue.
        /// </summary>
        public ControlFlowIssueType IssueType { get; init; }

        /// <summary>
        /// Gets or sets the location where the issue occurs.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets suggestions for fixing the issue.
        /// </summary>
        public IReadOnlyList<string> FixSuggestions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents an invariant that should be maintained.
    /// </summary>
    public record Invariant
    {
        /// <summary>
        /// Gets or sets the invariant description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the condition that must remain true.
        /// </summary>
        public string Condition { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the scope where this invariant applies.
        /// </summary>
        public string Scope { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this invariant is currently violated.
        /// </summary>
        public bool IsViolated { get; init; }

        /// <summary>
        /// Gets or sets the consequences of violating this invariant.
        /// </summary>
        public IReadOnlyList<string> ViolationConsequences { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Types of control flow issues.
    /// </summary>
    public enum ControlFlowIssueType
    {
        /// <summary>
        /// Code that can never be reached.
        /// </summary>
        UnreachableCode,

        /// <summary>
        /// Potential infinite loop.
        /// </summary>
        InfiniteLoop,

        /// <summary>
        /// Missing branch in conditional logic.
        /// </summary>
        MissingBranch,

        /// <summary>
        /// Dead code that serves no purpose.
        /// </summary>
        DeadCode,

        /// <summary>
        /// Complex control flow that's hard to understand.
        /// </summary>
        ComplexControlFlow,

        /// <summary>
        /// Missing return statement.
        /// </summary>
        MissingReturn,

        /// <summary>
        /// Potential null pointer dereference.
        /// </summary>
        NullDereference
    }

    /// <summary>
    /// Severity levels for issues.
    /// </summary>
    public enum IssueSeverity
    {
        /// <summary>
        /// Low severity issue.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium severity issue.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High severity issue.
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical severity issue.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Impact levels for issues.
    /// </summary>
    public enum IssueImpact
    {
        /// <summary>
        /// Minimal impact on system functionality.
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// Moderate impact on system functionality.
        /// </summary>
        Moderate = 2,

        /// <summary>
        /// High impact on system functionality.
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical impact that could break the system.
        /// </summary>
        Critical = 4
    }
}