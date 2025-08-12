namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents a fix recommendation for addressing a bug.
    /// </summary>
    public record FixRecommendation
    {
        /// <summary>
        /// Gets or sets the recommendation type.
        /// </summary>
        public FixRecommendationType Type { get; init; }

        /// <summary>
        /// Gets or sets the description of the fix.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the implementation steps.
        /// </summary>
        public IReadOnlyList<string> ImplementationSteps { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the estimated implementation effort.
        /// </summary>
        public ImplementationEffort EstimatedEffort { get; init; }

        /// <summary>
        /// Gets or sets the risk level of implementing this fix.
        /// </summary>
        public IssueSeverity RiskLevel { get; init; }

        /// <summary>
        /// Gets or sets potential side effects of the fix.
        /// </summary>
        public IReadOnlyList<string> PotentialSideEffects { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the code changes required.
        /// </summary>
        public string CodeChanges { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a comprehensive solution recommendation.
    /// </summary>
    public record SolutionRecommendation
    {
        /// <summary>
        /// Gets or sets the solution type.
        /// </summary>
        public SolutionType Type { get; init; }

        /// <summary>
        /// Gets or sets the design improvements recommended.
        /// </summary>
        public string DesignImprovements { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the implementation approach.
        /// </summary>
        public string ImplementationApproach { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the testing strategy for the solution.
        /// </summary>
        public string TestingStrategy { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the estimated development effort.
        /// </summary>
        public ImplementationEffort DevelopmentEffort { get; init; }

        /// <summary>
        /// Gets or sets the long-term benefits.
        /// </summary>
        public IReadOnlyList<string> LongTermBenefits { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the impact on other components.
        /// </summary>
        public string ImpactOnOtherComponents { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a testing strategy for bug verification.
    /// </summary>
    public record TestingStrategy
    {
        /// <summary>
        /// Gets or sets the test types to be performed.
        /// </summary>
        public IReadOnlyList<TestType> TestTypes { get; init; } = Array.Empty<TestType>();

        /// <summary>
        /// Gets or sets the test coverage goals.
        /// </summary>
        public TestCoverageGoals CoverageGoals { get; init; } = new();

        /// <summary>
        /// Gets or sets the regression test requirements.
        /// </summary>
        public IReadOnlyList<string> RegressionTestRequirements { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the performance test requirements.
        /// </summary>
        public IReadOnlyList<string> PerformanceTestRequirements { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the security test requirements.
        /// </summary>
        public IReadOnlyList<string> SecurityTestRequirements { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the automated test generation recommendations.
        /// </summary>
        public IReadOnlyList<string> AutomatedTestRecommendations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents test coverage goals.
    /// </summary>
    public record TestCoverageGoals
    {
        /// <summary>
        /// Gets or sets the statement coverage target percentage.
        /// </summary>
        public int StatementCoverageTarget { get; init; } = 90;

        /// <summary>
        /// Gets or sets the branch coverage target percentage.
        /// </summary>
        public int BranchCoverageTarget { get; init; } = 85;

        /// <summary>
        /// Gets or sets the path coverage target percentage.
        /// </summary>
        public int PathCoverageTarget { get; init; } = 75;

        /// <summary>
        /// Gets or sets critical paths that must be covered.
        /// </summary>
        public IReadOnlyList<string> CriticalPaths { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents prioritization criteria for bug prioritization.
    /// </summary>
    public record PrioritizationCriteria
    {
        /// <summary>
        /// Gets or sets the weight for severity in prioritization (0.0 to 1.0).
        /// </summary>
        public double SeverityWeight { get; init; } = 0.4;

        /// <summary>
        /// Gets or sets the weight for user impact in prioritization (0.0 to 1.0).
        /// </summary>
        public double UserImpactWeight { get; init; } = 0.3;

        /// <summary>
        /// Gets or sets the weight for business impact in prioritization (0.0 to 1.0).
        /// </summary>
        public double BusinessImpactWeight { get; init; } = 0.2;

        /// <summary>
        /// Gets or sets the weight for implementation effort in prioritization (0.0 to 1.0).
        /// </summary>
        public double ImplementationEffortWeight { get; init; } = 0.1;

        /// <summary>
        /// Gets or sets additional custom criteria.
        /// </summary>
        public Dictionary<string, double> CustomCriteria { get; init; } = new();

        /// <summary>
        /// Gets or sets whether to prioritize security issues higher.
        /// </summary>
        public bool PrioritizeSecurityIssues { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to prioritize performance issues higher.
        /// </summary>
        public bool PrioritizePerformanceIssues { get; init; } = false;
    }

    /// <summary>
    /// Represents a prioritized bug report.
    /// </summary>
    public record PrioritizedBugReport
    {
        /// <summary>
        /// Gets or sets the original bug report.
        /// </summary>
        public BugReport BugReport { get; init; } = new();

        /// <summary>
        /// Gets or sets the priority score (0.0 to 100.0).
        /// </summary>
        public double PriorityScore { get; init; }

        /// <summary>
        /// Gets or sets the priority rank within the collection.
        /// </summary>
        public int PriorityRank { get; init; }

        /// <summary>
        /// Gets or sets the reasoning for the prioritization.
        /// </summary>
        public string PrioritizationReasoning { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the recommended timeline for addressing this bug.
        /// </summary>
        public RecommendedTimeline RecommendedTimeline { get; init; } = new();
    }

    /// <summary>
    /// Represents a recommended timeline for bug resolution.
    /// </summary>
    public record RecommendedTimeline
    {
        /// <summary>
        /// Gets or sets the recommended start date.
        /// </summary>
        public DateTimeOffset RecommendedStartDate { get; init; }

        /// <summary>
        /// Gets or sets the target resolution date.
        /// </summary>
        public DateTimeOffset TargetResolutionDate { get; init; }

        /// <summary>
        /// Gets or sets the estimated duration for resolution.
        /// </summary>
        public TimeSpan EstimatedDuration { get; init; }

        /// <summary>
        /// Gets or sets dependencies that could affect the timeline.
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Types of fix recommendations.
    /// </summary>
    public enum FixRecommendationType
    {
        /// <summary>
        /// Quick fix to prevent immediate issues.
        /// </summary>
        QuickFix,

        /// <summary>
        /// Workaround to mitigate the issue.
        /// </summary>
        Workaround,

        /// <summary>
        /// Configuration change.
        /// </summary>
        ConfigurationChange,

        /// <summary>
        /// Code patch.
        /// </summary>
        CodePatch,

        /// <summary>
        /// Library update.
        /// </summary>
        LibraryUpdate,

        /// <summary>
        /// Environment change.
        /// </summary>
        EnvironmentChange
    }

    /// <summary>
    /// Types of comprehensive solutions.
    /// </summary>
    public enum SolutionType
    {
        /// <summary>
        /// Architectural improvement.
        /// </summary>
        ArchitecturalImprovement,

        /// <summary>
        /// Design pattern implementation.
        /// </summary>
        DesignPatternImplementation,

        /// <summary>
        /// Refactoring initiative.
        /// </summary>
        RefactoringInitiative,

        /// <summary>
        /// Performance optimization.
        /// </summary>
        PerformanceOptimization,

        /// <summary>
        /// Security enhancement.
        /// </summary>
        SecurityEnhancement,

        /// <summary>
        /// Process improvement.
        /// </summary>
        ProcessImprovement
    }

    /// <summary>
    /// Implementation effort estimates.
    /// </summary>
    public enum ImplementationEffort
    {
        /// <summary>
        /// Minimal effort (hours).
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// Low effort (1-2 days).
        /// </summary>
        Low = 2,

        /// <summary>
        /// Medium effort (3-5 days).
        /// </summary>
        Medium = 3,

        /// <summary>
        /// High effort (1-2 weeks).
        /// </summary>
        High = 4,

        /// <summary>
        /// Very high effort (multiple weeks).
        /// </summary>
        VeryHigh = 5
    }

    /// <summary>
    /// Types of tests for bug verification.
    /// </summary>
    public enum TestType
    {
        /// <summary>
        /// Unit tests.
        /// </summary>
        Unit,

        /// <summary>
        /// Integration tests.
        /// </summary>
        Integration,

        /// <summary>
        /// System tests.
        /// </summary>
        System,

        /// <summary>
        /// Performance tests.
        /// </summary>
        Performance,

        /// <summary>
        /// Security tests.
        /// </summary>
        Security,

        /// <summary>
        /// Load tests.
        /// </summary>
        Load,

        /// <summary>
        /// Stress tests.
        /// </summary>
        Stress,

        /// <summary>
        /// Regression tests.
        /// </summary>
        Regression,

        /// <summary>
        /// Acceptance tests.
        /// </summary>
        Acceptance
    }
}