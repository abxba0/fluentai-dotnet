namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Evidence and details about a bug for classification purposes.
    /// </summary>
    public record BugEvidence
    {
        /// <summary>
        /// Gets or sets the bug title or summary.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed description of the bug.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the steps to reproduce the bug.
        /// </summary>
        public IReadOnlyList<string> ReproductionSteps { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the expected behavior.
        /// </summary>
        public string ExpectedBehavior { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the actual behavior observed.
        /// </summary>
        public string ActualBehavior { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the environment information.
        /// </summary>
        public EnvironmentInfo Environment { get; init; } = new();

        /// <summary>
        /// Gets or sets the stack trace if available.
        /// </summary>
        public string? StackTrace { get; init; }

        /// <summary>
        /// Gets or sets related error logs.
        /// </summary>
        public IReadOnlyList<string> ErrorLogs { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the frequency of occurrence.
        /// </summary>
        public BugFrequency Frequency { get; init; }

        /// <summary>
        /// Gets or sets affected user scenarios.
        /// </summary>
        public IReadOnlyList<string> AffectedScenarios { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Classification result for a bug.
    /// </summary>
    public record BugClassification
    {
        /// <summary>
        /// Gets or sets the bug category.
        /// </summary>
        public BugCategory Category { get; init; }

        /// <summary>
        /// Gets or sets the bug priority level.
        /// </summary>
        public BugPriority Priority { get; init; }

        /// <summary>
        /// Gets or sets the bug severity.
        /// </summary>
        public BugSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets the functional area affected.
        /// </summary>
        public string FunctionalArea { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence level of the classification (0.0 to 1.0).
        /// </summary>
        public double ConfidenceLevel { get; init; }

        /// <summary>
        /// Gets or sets the reasoning behind the classification.
        /// </summary>
        public string ClassificationReasoning { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets tags for categorization.
        /// </summary>
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Comprehensive bug report with detailed analysis.
    /// </summary>
    public record BugReport
    {
        /// <summary>
        /// Gets or sets the unique bug identifier.
        /// </summary>
        public string BugId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the bug evidence.
        /// </summary>
        public BugEvidence Evidence { get; init; } = new();

        /// <summary>
        /// Gets or sets the bug classification.
        /// </summary>
        public BugClassification Classification { get; init; } = new();

        /// <summary>
        /// Gets or sets the root cause analysis.
        /// </summary>
        public RootCauseAnalysis RootCause { get; init; } = new();

        /// <summary>
        /// Gets or sets the impact assessment.
        /// </summary>
        public BugImpactAssessment Impact { get; init; } = new();

        /// <summary>
        /// Gets or sets immediate fix recommendations.
        /// </summary>
        public IReadOnlyList<FixRecommendation> ImmediateFixes { get; init; } = Array.Empty<FixRecommendation>();

        /// <summary>
        /// Gets or sets proper solution recommendations.
        /// </summary>
        public IReadOnlyList<SolutionRecommendation> ProperSolutions { get; init; } = Array.Empty<SolutionRecommendation>();

        /// <summary>
        /// Gets or sets the testing strategy for verification.
        /// </summary>
        public TestingStrategy TestingStrategy { get; init; } = new();

        /// <summary>
        /// Gets or sets the timestamp when the report was generated.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Impact assessment for a bug.
    /// </summary>
    public record BugImpactAssessment
    {
        /// <summary>
        /// Gets or sets the user impact description.
        /// </summary>
        public string UserImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the business impact description.
        /// </summary>
        public string BusinessImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the technical debt implications.
        /// </summary>
        public string TechnicalDebtImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the estimated number of affected users.
        /// </summary>
        public int EstimatedAffectedUsers { get; init; }

        /// <summary>
        /// Gets or sets the potential revenue impact.
        /// </summary>
        public decimal PotentialRevenueImpact { get; init; }

        /// <summary>
        /// Gets or sets the security implications.
        /// </summary>
        public SecurityImplications SecurityImplications { get; init; } = new();

        /// <summary>
        /// Gets or sets the performance implications.
        /// </summary>
        public PerformanceImplications PerformanceImplications { get; init; } = new();
    }

    /// <summary>
    /// Root cause analysis for a bug.
    /// </summary>
    public record RootCauseAnalysis
    {
        /// <summary>
        /// Gets or sets the technical root cause.
        /// </summary>
        public string TechnicalCause { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the design-level root cause.
        /// </summary>
        public string DesignCause { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the process-level root cause.
        /// </summary>
        public string ProcessCause { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets contributing factors.
        /// </summary>
        public IReadOnlyList<string> ContributingFactors { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the fault injection points.
        /// </summary>
        public IReadOnlyList<string> FaultInjectionPoints { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets prevention strategies for similar bugs.
        /// </summary>
        public IReadOnlyList<string> PreventionStrategies { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Environment information for bug reproduction.
    /// </summary>
    public record EnvironmentInfo
    {
        /// <summary>
        /// Gets or sets the operating system.
        /// </summary>
        public string OperatingSystem { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the runtime version.
        /// </summary>
        public string RuntimeVersion { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        public string ApplicationVersion { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets additional environment variables.
        /// </summary>
        public Dictionary<string, string> EnvironmentVariables { get; init; } = new();

        /// <summary>
        /// Gets or sets hardware specifications.
        /// </summary>
        public string HardwareSpecs { get; init; } = string.Empty;
    }

    /// <summary>
    /// Security implications of a bug.
    /// </summary>
    public record SecurityImplications
    {
        /// <summary>
        /// Gets or sets whether the bug has security implications.
        /// </summary>
        public bool HasSecurityImplications { get; init; }

        /// <summary>
        /// Gets or sets the vulnerability type.
        /// </summary>
        public string VulnerabilityType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the CVSS score if applicable.
        /// </summary>
        public double? CvssScore { get; init; }

        /// <summary>
        /// Gets or sets potential attack vectors.
        /// </summary>
        public IReadOnlyList<string> AttackVectors { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Performance implications of a bug.
    /// </summary>
    public record PerformanceImplications
    {
        /// <summary>
        /// Gets or sets whether the bug affects performance.
        /// </summary>
        public bool AffectsPerformance { get; init; }

        /// <summary>
        /// Gets or sets the performance degradation percentage.
        /// </summary>
        public double PerformanceDegradation { get; init; }

        /// <summary>
        /// Gets or sets the resource impact description.
        /// </summary>
        public string ResourceImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets scalability concerns.
        /// </summary>
        public IReadOnlyList<string> ScalabilityConcerns { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Bug categories for classification.
    /// </summary>
    public enum BugCategory
    {
        /// <summary>
        /// Functional defect affecting features.
        /// </summary>
        Functional,

        /// <summary>
        /// Performance-related issue.
        /// </summary>
        Performance,

        /// <summary>
        /// Security vulnerability.
        /// </summary>
        Security,

        /// <summary>
        /// Usability or user experience issue.
        /// </summary>
        Usability,

        /// <summary>
        /// Data integrity or corruption issue.
        /// </summary>
        DataIntegrity,

        /// <summary>
        /// Integration or compatibility issue.
        /// </summary>
        Integration,

        /// <summary>
        /// Configuration or deployment issue.
        /// </summary>
        Configuration,

        /// <summary>
        /// Documentation or help issue.
        /// </summary>
        Documentation
    }

    /// <summary>
    /// Bug priority levels.
    /// </summary>
    public enum BugPriority
    {
        /// <summary>
        /// Low priority - can be addressed in future releases.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium priority - should be addressed in current cycle.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High priority - needs prompt attention.
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical priority - requires immediate action.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Bug severity levels.
    /// </summary>
    public enum BugSeverity
    {
        /// <summary>
        /// Trivial issue with minimal impact.
        /// </summary>
        Trivial = 1,

        /// <summary>
        /// Minor issue with limited impact.
        /// </summary>
        Minor = 2,

        /// <summary>
        /// Major issue with significant impact.
        /// </summary>
        Major = 3,

        /// <summary>
        /// Critical issue that blocks functionality.
        /// </summary>
        Critical = 4,

        /// <summary>
        /// Blocker issue that prevents system use.
        /// </summary>
        Blocker = 5
    }

    /// <summary>
    /// Bug frequency of occurrence.
    /// </summary>
    public enum BugFrequency
    {
        /// <summary>
        /// Occurs once or rarely.
        /// </summary>
        Rare,

        /// <summary>
        /// Occurs occasionally.
        /// </summary>
        Occasional,

        /// <summary>
        /// Occurs frequently.
        /// </summary>
        Frequent,

        /// <summary>
        /// Occurs consistently or always.
        /// </summary>
        Always
    }
}