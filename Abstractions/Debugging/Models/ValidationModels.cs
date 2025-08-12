namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Represents an implemented solution for post-fix validation.
    /// </summary>
    public record ImplementedSolution
    {
        /// <summary>
        /// Gets or sets the solution identifier.
        /// </summary>
        public string SolutionId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the bug this solution addressed.
        /// </summary>
        public string TargetBugId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the implementation timestamp.
        /// </summary>
        public DateTimeOffset ImplementationTimestamp { get; init; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Gets or sets the actual changes made.
        /// </summary>
        public IReadOnlyList<ImplementedChange> ImplementedChanges { get; init; } = Array.Empty<ImplementedChange>();

        /// <summary>
        /// Gets or sets the implementation details.
        /// </summary>
        public ImplementationDetails Details { get; init; } = new();

        /// <summary>
        /// Gets or sets the verification results.
        /// </summary>
        public ImplementationVerificationResults VerificationResults { get; init; } = new();
    }

    /// <summary>
    /// Represents an implemented change.
    /// </summary>
    public record ImplementedChange
    {
        /// <summary>
        /// Gets or sets the type of change that was implemented.
        /// </summary>
        public ChangeType ChangeType { get; init; }

        /// <summary>
        /// Gets or sets the location where the change was made.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the implemented change.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the code diff or change details.
        /// </summary>
        public string ChangeDetails { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the commit or version identifier.
        /// </summary>
        public string CommitId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Implementation details for a solution.
    /// </summary>
    public record ImplementationDetails
    {
        /// <summary>
        /// Gets or sets the actual implementation duration.
        /// </summary>
        public TimeSpan ActualDuration { get; init; }

        /// <summary>
        /// Gets or sets the implementation approach used.
        /// </summary>
        public string ImplementationApproach { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets any deviations from the original plan.
        /// </summary>
        public IReadOnlyList<string> PlanDeviations { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets challenges encountered during implementation.
        /// </summary>
        public IReadOnlyList<string> ChallengesEncountered { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets lessons learned from the implementation.
        /// </summary>
        public IReadOnlyList<string> LessonsLearned { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Verification results for an implemented solution.
    /// </summary>
    public record ImplementationVerificationResults
    {
        /// <summary>
        /// Gets or sets whether the implementation was successful.
        /// </summary>
        public bool IsSuccessful { get; init; }

        /// <summary>
        /// Gets or sets the build verification results.
        /// </summary>
        public BuildVerificationResults BuildResults { get; init; } = new();

        /// <summary>
        /// Gets or sets the test verification results.
        /// </summary>
        public TestVerificationResults TestResults { get; init; } = new();

        /// <summary>
        /// Gets or sets the code quality verification results.
        /// </summary>
        public CodeQualityVerificationResults QualityResults { get; init; } = new();
    }

    /// <summary>
    /// Build verification results.
    /// </summary>
    public record BuildVerificationResults
    {
        /// <summary>
        /// Gets or sets whether the build was successful.
        /// </summary>
        public bool BuildSuccessful { get; init; }

        /// <summary>
        /// Gets or sets build warnings.
        /// </summary>
        public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets build errors.
        /// </summary>
        public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the build duration.
        /// </summary>
        public TimeSpan BuildDuration { get; init; }
    }

    /// <summary>
    /// Test verification results.
    /// </summary>
    public record TestVerificationResults
    {
        /// <summary>
        /// Gets or sets the total number of tests run.
        /// </summary>
        public int TotalTests { get; init; }

        /// <summary>
        /// Gets or sets the number of passed tests.
        /// </summary>
        public int PassedTests { get; init; }

        /// <summary>
        /// Gets or sets the number of failed tests.
        /// </summary>
        public int FailedTests { get; init; }

        /// <summary>
        /// Gets or sets the test execution duration.
        /// </summary>
        public TimeSpan TestDuration { get; init; }

        /// <summary>
        /// Gets or sets the test coverage percentage.
        /// </summary>
        public double CoveragePercentage { get; init; }

        /// <summary>
        /// Gets or sets details of failed tests.
        /// </summary>
        public IReadOnlyList<string> FailedTestDetails { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Code quality verification results.
    /// </summary>
    public record CodeQualityVerificationResults
    {
        /// <summary>
        /// Gets or sets the code quality score (0-100).
        /// </summary>
        public int QualityScore { get; init; }

        /// <summary>
        /// Gets or sets static analysis warnings.
        /// </summary>
        public IReadOnlyList<string> StaticAnalysisWarnings { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets code complexity metrics.
        /// </summary>
        public CodeComplexityMetrics ComplexityMetrics { get; init; } = new();

        /// <summary>
        /// Gets or sets code style violations.
        /// </summary>
        public IReadOnlyList<string> StyleViolations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Code complexity metrics.
    /// </summary>
    public record CodeComplexityMetrics
    {
        /// <summary>
        /// Gets or sets the cyclomatic complexity.
        /// </summary>
        public int CyclomaticComplexity { get; init; }

        /// <summary>
        /// Gets or sets the maintainability index.
        /// </summary>
        public int MaintainabilityIndex { get; init; }

        /// <summary>
        /// Gets or sets the lines of code.
        /// </summary>
        public int LinesOfCode { get; init; }

        /// <summary>
        /// Gets or sets the depth of inheritance.
        /// </summary>
        public int DepthOfInheritance { get; init; }
    }

    /// <summary>
    /// Result of solution validation.
    /// </summary>
    public record SolutionValidationResult
    {
        /// <summary>
        /// Gets or sets whether the solution is valid.
        /// </summary>
        public bool IsValid { get; init; }

        /// <summary>
        /// Gets or sets the validation score (0.0 to 1.0).
        /// </summary>
        public double ValidationScore { get; init; }

        /// <summary>
        /// Gets or sets validation findings.
        /// </summary>
        public IReadOnlyList<ValidationFinding> Findings { get; init; } = Array.Empty<ValidationFinding>();

        /// <summary>
        /// Gets or sets recommendations for improving the solution.
        /// </summary>
        public IReadOnlyList<string> ImprovementRecommendations { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the risk assessment for the solution.
        /// </summary>
        public SolutionRiskAssessment RiskAssessment { get; init; } = new();

        /// <summary>
        /// Gets or sets the impact analysis.
        /// </summary>
        public SolutionImpactAnalysis ImpactAnalysis { get; init; } = new();
    }

    /// <summary>
    /// Validation finding from solution analysis.
    /// </summary>
    public record ValidationFinding
    {
        /// <summary>
        /// Gets or sets the finding type.
        /// </summary>
        public ValidationFindingType FindingType { get; init; }

        /// <summary>
        /// Gets or sets the severity of the finding.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets the description of the finding.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the location where the finding applies.
        /// </summary>
        public string Location { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets recommendations for addressing this finding.
        /// </summary>
        public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Solution risk assessment.
    /// </summary>
    public record SolutionRiskAssessment
    {
        /// <summary>
        /// Gets or sets the overall risk level.
        /// </summary>
        public IssueSeverity OverallRiskLevel { get; init; }

        /// <summary>
        /// Gets or sets implementation risks.
        /// </summary>
        public IReadOnlyList<string> ImplementationRisks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets performance risks.
        /// </summary>
        public IReadOnlyList<string> PerformanceRisks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets security risks.
        /// </summary>
        public IReadOnlyList<string> SecurityRisks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets compatibility risks.
        /// </summary>
        public IReadOnlyList<string> CompatibilityRisks { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets mitigation strategies for identified risks.
        /// </summary>
        public IReadOnlyList<string> MitigationStrategies { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Solution impact analysis.
    /// </summary>
    public record SolutionImpactAnalysis
    {
        /// <summary>
        /// Gets or sets the impact on system performance.
        /// </summary>
        public string PerformanceImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the impact on other components.
        /// </summary>
        public IReadOnlyList<string> ComponentImpacts { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the impact on user experience.
        /// </summary>
        public string UserExperienceImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the impact on maintainability.
        /// </summary>
        public string MaintainabilityImpact { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the impact on technical debt.
        /// </summary>
        public string TechnicalDebtImpact { get; init; } = string.Empty;
    }

    /// <summary>
    /// Result of post-fix validation.
    /// </summary>
    public record PostFixValidationResult
    {
        /// <summary>
        /// Gets or sets whether the fix was successful.
        /// </summary>
        public bool FixSuccessful { get; init; }

        /// <summary>
        /// Gets or sets whether the original issue is resolved.
        /// </summary>
        public bool OriginalIssueResolved { get; init; }

        /// <summary>
        /// Gets or sets the regression analysis results.
        /// </summary>
        public RegressionAnalysisResult RegressionAnalysis { get; init; } = new();

        /// <summary>
        /// Gets or sets the validation test results.
        /// </summary>
        public ValidationTestResults ValidationTestResults { get; init; } = new();

        /// <summary>
        /// Gets or sets the performance impact assessment.
        /// </summary>
        public PostFixPerformanceAssessment PerformanceAssessment { get; init; } = new();

        /// <summary>
        /// Gets or sets any new issues discovered.
        /// </summary>
        public IReadOnlyList<string> NewIssuesDiscovered { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the overall validation score (0.0 to 1.0).
        /// </summary>
        public double ValidationScore { get; init; }
    }

    /// <summary>
    /// Regression analysis results.
    /// </summary>
    public record RegressionAnalysisResult
    {
        /// <summary>
        /// Gets or sets whether any regressions were detected.
        /// </summary>
        public bool RegressionsDetected { get; init; }

        /// <summary>
        /// Gets or sets the detected regressions.
        /// </summary>
        public IReadOnlyList<DetectedRegression> DetectedRegressions { get; init; } = Array.Empty<DetectedRegression>();

        /// <summary>
        /// Gets or sets the regression test coverage.
        /// </summary>
        public double RegressionTestCoverage { get; init; }

        /// <summary>
        /// Gets or sets recommendations for preventing future regressions.
        /// </summary>
        public IReadOnlyList<string> PreventionRecommendations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a detected regression.
    /// </summary>
    public record DetectedRegression
    {
        /// <summary>
        /// Gets or sets the regression description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the affected functionality.
        /// </summary>
        public string AffectedFunctionality { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the regression.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets the detection method.
        /// </summary>
        public string DetectionMethod { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets potential fixes for the regression.
        /// </summary>
        public IReadOnlyList<string> PotentialFixes { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Validation test results.
    /// </summary>
    public record ValidationTestResults
    {
        /// <summary>
        /// Gets or sets the acceptance test results.
        /// </summary>
        public TestExecutionResults AcceptanceTests { get; init; } = new();

        /// <summary>
        /// Gets or sets the regression test results.
        /// </summary>
        public TestExecutionResults RegressionTests { get; init; } = new();

        /// <summary>
        /// Gets or sets the performance test results.
        /// </summary>
        public TestExecutionResults PerformanceTests { get; init; } = new();

        /// <summary>
        /// Gets or sets the security test results.
        /// </summary>
        public TestExecutionResults SecurityTests { get; init; } = new();
    }

    /// <summary>
    /// Test execution results.
    /// </summary>
    public record TestExecutionResults
    {
        /// <summary>
        /// Gets or sets the total number of tests executed.
        /// </summary>
        public int TotalTests { get; init; }

        /// <summary>
        /// Gets or sets the number of passed tests.
        /// </summary>
        public int PassedTests { get; init; }

        /// <summary>
        /// Gets or sets the number of failed tests.
        /// </summary>
        public int FailedTests { get; init; }

        /// <summary>
        /// Gets or sets the number of skipped tests.
        /// </summary>
        public int SkippedTests { get; init; }

        /// <summary>
        /// Gets or sets the execution duration.
        /// </summary>
        public TimeSpan ExecutionDuration { get; init; }

        /// <summary>
        /// Gets or sets details of failed tests.
        /// </summary>
        public IReadOnlyList<string> FailureDetails { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Post-fix performance assessment.
    /// </summary>
    public record PostFixPerformanceAssessment
    {
        /// <summary>
        /// Gets or sets whether performance improved.
        /// </summary>
        public bool PerformanceImproved { get; init; }

        /// <summary>
        /// Gets or sets the performance change percentage.
        /// </summary>
        public double PerformanceChangePercentage { get; init; }

        /// <summary>
        /// Gets or sets memory usage changes.
        /// </summary>
        public string MemoryUsageChanges { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets CPU usage changes.
        /// </summary>
        public string CpuUsageChanges { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets response time changes.
        /// </summary>
        public string ResponseTimeChanges { get; init; } = string.Empty;
    }

    /// <summary>
    /// Types of validation findings.
    /// </summary>
    public enum ValidationFindingType
    {
        /// <summary>
        /// Potential design issue.
        /// </summary>
        DesignIssue,

        /// <summary>
        /// Implementation concern.
        /// </summary>
        ImplementationConcern,

        /// <summary>
        /// Performance concern.
        /// </summary>
        PerformanceConcern,

        /// <summary>
        /// Security concern.
        /// </summary>
        SecurityConcern,

        /// <summary>
        /// Maintainability concern.
        /// </summary>
        MaintainabilityConcern,

        /// <summary>
        /// Compatibility concern.
        /// </summary>
        CompatibilityConcern,

        /// <summary>
        /// Testing gap.
        /// </summary>
        TestingGap,

        /// <summary>
        /// Documentation gap.
        /// </summary>
        DocumentationGap
    }
}