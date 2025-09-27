namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Represents a runtime issue detected during code analysis.
    /// </summary>
    public class RuntimeIssue
    {
        /// <summary>
        /// Gets or sets the unique identifier for this issue.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of runtime issue.
        /// </summary>
        public RuntimeIssueType Type { get; set; }

        /// <summary>
        /// Gets or sets the severity of the issue.
        /// </summary>
        public RuntimeIssueSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the description of the issue.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the issue was found.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the suggested fix for the issue.
        /// </summary>
        public string SuggestedFix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path where the issue was found.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line number where the issue was found.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the proof of the issue.
        /// </summary>
        public IssueProof? Proof { get; set; }

        /// <summary>
        /// Gets or sets the solution for the issue.
        /// </summary>
        public IssueSolution? Solution { get; set; }
    }

    /// <summary>
    /// Specifies the type of runtime issue.
    /// </summary>
    public enum RuntimeIssueType
    {
        Performance,
        MemoryLeak,
        ResourceManagement,
        Threading,
        AsyncVoid,
        NullReference,
        CollectionModification,
        StringConcatenation,
        LargeObjectAllocation,
        ConnectionPoolExhaustion,
        MutableStaticField,
        DivisionByZero,
        UnhandledException
    }

    /// <summary>
    /// Specifies the severity level of a runtime issue.
    /// </summary>
    public enum RuntimeIssueSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Represents proof of a runtime issue.
    /// </summary>
    public class IssueProof
    {
        /// <summary>
        /// Gets or sets the simulated execution step.
        /// </summary>
        public string SimulatedExecutionStep { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trigger for the issue.
        /// </summary>
        public string Trigger { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the result of the issue.
        /// </summary>
        public string Result { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a solution for a runtime issue.
    /// </summary>
    public class IssueSolution
    {
        /// <summary>
        /// Gets or sets the fix for the issue.
        /// </summary>
        public string Fix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the verification steps for the fix.
        /// </summary>
        public string Verification { get; set; } = string.Empty;
    }
}