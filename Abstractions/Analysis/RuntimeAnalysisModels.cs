namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Represents the complete result of a runtime-aware code analysis.
    /// </summary>
    public record RuntimeAnalysisResult
    {
        /// <summary>
        /// Gets the list of runtime issues discovered during analysis.
        /// </summary>
        public IReadOnlyList<RuntimeIssue> RuntimeIssues { get; init; } = Array.Empty<RuntimeIssue>();

        /// <summary>
        /// Gets the list of environment risks identified during analysis.
        /// </summary>
        public IReadOnlyList<EnvironmentRisk> EnvironmentRisks { get; init; } = Array.Empty<EnvironmentRisk>();

        /// <summary>
        /// Gets the list of edge case failures found during analysis.
        /// </summary>
        public IReadOnlyList<EdgeCaseFailure> EdgeCaseFailures { get; init; } = Array.Empty<EdgeCaseFailure>();

        /// <summary>
        /// Gets metadata about the analysis execution.
        /// </summary>
        public AnalysisMetadata Metadata { get; init; } = new();

        /// <summary>
        /// Gets a value indicating whether any critical issues were found.
        /// </summary>
        public bool HasCriticalIssues => RuntimeIssues.Any(i => i.Severity == RuntimeIssueSeverity.Critical) ||
                                         EnvironmentRisks.Any(r => r.Likelihood == RiskLikelihood.High);

        /// <summary>
        /// Gets the total number of issues found across all categories.
        /// </summary>
        public int TotalIssueCount => RuntimeIssues.Count + EnvironmentRisks.Count + EdgeCaseFailures.Count;
    }

    /// <summary>
    /// Represents a runtime issue discovered during code analysis.
    /// </summary>
    public record RuntimeIssue
    {
        /// <summary>
        /// Gets the unique identifier for this issue.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the type of runtime issue.
        /// </summary>
        public RuntimeIssueType Type { get; init; }

        /// <summary>
        /// Gets the severity level of the issue.
        /// </summary>
        public RuntimeIssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets a clear description of the runtime problem.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets proof details showing where and how the issue occurs.
        /// </summary>
        public IssueProof Proof { get; init; } = new();

        /// <summary>
        /// Gets the recommended solution for this issue.
        /// </summary>
        public IssueSolution Solution { get; init; } = new();

        /// <summary>
        /// Gets the file path where this issue was found.
        /// </summary>
        public string? FilePath { get; init; }

        /// <summary>
        /// Gets the line number where this issue occurs.
        /// </summary>
        public int? LineNumber { get; init; }
    }

    /// <summary>
    /// Represents an environment risk that could cause runtime failures.
    /// </summary>
    public record EnvironmentRisk
    {
        /// <summary>
        /// Gets the unique identifier for this risk.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the component that poses the risk.
        /// </summary>
        public string Component { get; init; } = string.Empty;

        /// <summary>
        /// Gets a description of what might fail at runtime.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the likelihood of this risk occurring.
        /// </summary>
        public RiskLikelihood Likelihood { get; init; }

        /// <summary>
        /// Gets the mitigation strategies for this risk.
        /// </summary>
        public RiskMitigation Mitigation { get; init; } = new();
    }

    /// <summary>
    /// Represents an edge case failure scenario.
    /// </summary>
    public record EdgeCaseFailure
    {
        /// <summary>
        /// Gets the unique identifier for this edge case.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Gets the input that triggers this edge case.
        /// </summary>
        public string Input { get; init; } = string.Empty;

        /// <summary>
        /// Gets the expected correct behavior.
        /// </summary>
        public string Expected { get; init; } = string.Empty;

        /// <summary>
        /// Gets the actual observed outcome from simulation.
        /// </summary>
        public string Actual { get; init; } = string.Empty;

        /// <summary>
        /// Gets the recommended fix for this edge case.
        /// </summary>
        public string Fix { get; init; } = string.Empty;

        /// <summary>
        /// Gets the file path where this edge case was found.
        /// </summary>
        public string? FilePath { get; init; }
    }

    /// <summary>
    /// Contains proof details for a runtime issue.
    /// </summary>
    public record IssueProof
    {
        /// <summary>
        /// Gets the simulated execution step where the issue occurs.
        /// </summary>
        public string SimulatedExecutionStep { get; init; } = string.Empty;

        /// <summary>
        /// Gets what triggers this issue.
        /// </summary>
        public string Trigger { get; init; } = string.Empty;

        /// <summary>
        /// Gets the observed behavior when the issue occurs.
        /// </summary>
        public string Result { get; init; } = string.Empty;
    }

    /// <summary>
    /// Contains solution details for a runtime issue.
    /// </summary>
    public record IssueSolution
    {
        /// <summary>
        /// Gets the recommended code or configuration change.
        /// </summary>
        public string Fix { get; init; } = string.Empty;

        /// <summary>
        /// Gets the test scenario to verify the fix.
        /// </summary>
        public string Verification { get; init; } = string.Empty;
    }

    /// <summary>
    /// Contains mitigation strategies for environment risks.
    /// </summary>
    public record RiskMitigation
    {
        /// <summary>
        /// Gets the list of required changes to mitigate the risk.
        /// </summary>
        public IReadOnlyList<string> RequiredChanges { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets the monitoring recommendations for this risk.
        /// </summary>
        public string Monitoring { get; init; } = string.Empty;
    }

    /// <summary>
    /// Contains metadata about the analysis execution.
    /// </summary>
    public record AnalysisMetadata
    {
        /// <summary>
        /// Gets the timestamp when the analysis was performed.
        /// </summary>
        public DateTime AnalysisTimestamp { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// Gets the total duration of the analysis.
        /// </summary>
        public TimeSpan Duration { get; init; }

        /// <summary>
        /// Gets the list of files that were analyzed.
        /// </summary>
        public IReadOnlyList<string> AnalyzedFiles { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets the version of the analyzer that performed this analysis.
        /// </summary>
        public string AnalyzerVersion { get; init; } = "1.0.0";
    }

    /// <summary>
    /// Defines the types of runtime issues that can be detected.
    /// </summary>
    public enum RuntimeIssueType
    {
        /// <summary>
        /// Performance-related runtime issue.
        /// </summary>
        Performance,

        /// <summary>
        /// Issue that causes application crashes.
        /// </summary>
        Crash,

        /// <summary>
        /// Issue that produces incorrect output.
        /// </summary>
        IncorrectOutput,

        /// <summary>
        /// Environment or dependency-related issue.
        /// </summary>
        Environment
    }

    /// <summary>
    /// Defines severity levels for runtime issues.
    /// </summary>
    public enum RuntimeIssueSeverity
    {
        /// <summary>
        /// Low severity issue.
        /// </summary>
        Low,

        /// <summary>
        /// Medium severity issue.
        /// </summary>
        Medium,

        /// <summary>
        /// High severity issue.
        /// </summary>
        High,

        /// <summary>
        /// Critical severity issue requiring immediate attention.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Defines likelihood levels for environment risks.
    /// </summary>
    public enum RiskLikelihood
    {
        /// <summary>
        /// Low likelihood of occurrence.
        /// </summary>
        Low,

        /// <summary>
        /// Medium likelihood of occurrence.
        /// </summary>
        Medium,

        /// <summary>
        /// High likelihood of occurrence.
        /// </summary>
        High
    }
}