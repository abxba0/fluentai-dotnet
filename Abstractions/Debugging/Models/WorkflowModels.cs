namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Context and configuration for debugging workflow execution.
    /// </summary>
    public record DebuggingWorkflowContext
    {
        /// <summary>
        /// Gets or sets the analysis context for the workflow.
        /// </summary>
        public AnalysisContext AnalysisContext { get; init; } = new();

        /// <summary>
        /// Gets or sets the workflow configuration.
        /// </summary>
        public WorkflowConfiguration Configuration { get; init; } = new();

        /// <summary>
        /// Gets or sets the target components for analysis.
        /// </summary>
        public IReadOnlyList<string> TargetComponents { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the analysis phases to execute.
        /// </summary>
        public IReadOnlyList<AnalysisPhase> AnalysisPhases { get; init; } = Array.Empty<AnalysisPhase>();

        /// <summary>
        /// Gets or sets whether to generate test cases automatically.
        /// </summary>
        public bool GenerateTestCases { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to validate solutions before reporting.
        /// </summary>
        public bool ValidateSolutions { get; init; } = true;
    }

    /// <summary>
    /// Configuration for debugging workflow execution.
    /// </summary>
    public record WorkflowConfiguration
    {
        /// <summary>
        /// Gets or sets the maximum execution time for the workflow.
        /// </summary>
        public TimeSpan MaxExecutionTime { get; init; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Gets or sets the parallel execution settings.
        /// </summary>
        public ParallelExecutionSettings ParallelExecution { get; init; } = new();

        /// <summary>
        /// Gets or sets the quality gates for the workflow.
        /// </summary>
        public WorkflowQualityGates QualityGates { get; init; } = new();

        /// <summary>
        /// Gets or sets the reporting preferences.
        /// </summary>
        public ReportingPreferences ReportingPreferences { get; init; } = new();
    }

    /// <summary>
    /// Result of debugging workflow execution.
    /// </summary>
    public record DebuggingWorkflowResult
    {
        /// <summary>
        /// Gets or sets the comprehensive analysis results.
        /// </summary>
        public ComprehensiveAnalysisResult AnalysisResults { get; init; } = new();

        /// <summary>
        /// Gets or sets the identified and classified bugs.
        /// </summary>
        public IReadOnlyList<BugReport> BugReports { get; init; } = Array.Empty<BugReport>();

        /// <summary>
        /// Gets or sets the prioritized bug list.
        /// </summary>
        public IReadOnlyList<PrioritizedBugReport> PrioritizedBugs { get; init; } = Array.Empty<PrioritizedBugReport>();

        /// <summary>
        /// Gets or sets the generated test cases.
        /// </summary>
        public TestCaseGenerationResult GeneratedTestCases { get; init; } = new();

        /// <summary>
        /// Gets or sets the workflow execution summary.
        /// </summary>
        public WorkflowExecutionSummary ExecutionSummary { get; init; } = new();

        /// <summary>
        /// Gets or sets the quality gate results.
        /// </summary>
        public QualityGateResults QualityGateResults { get; init; } = new();
    }

    /// <summary>
    /// Result of test case generation.
    /// </summary>
    public record TestCaseGenerationResult
    {
        /// <summary>
        /// Gets or sets the generated unit test cases.
        /// </summary>
        public IReadOnlyList<GeneratedTestCase> UnitTestCases { get; init; } = Array.Empty<GeneratedTestCase>();

        /// <summary>
        /// Gets or sets the generated integration test cases.
        /// </summary>
        public IReadOnlyList<GeneratedTestCase> IntegrationTestCases { get; init; } = Array.Empty<GeneratedTestCase>();

        /// <summary>
        /// Gets or sets the generated edge case test cases.
        /// </summary>
        public IReadOnlyList<GeneratedTestCase> EdgeCaseTestCases { get; init; } = Array.Empty<GeneratedTestCase>();

        /// <summary>
        /// Gets or sets the generated regression test cases.
        /// </summary>
        public IReadOnlyList<GeneratedTestCase> RegressionTestCases { get; init; } = Array.Empty<GeneratedTestCase>();

        /// <summary>
        /// Gets or sets the test coverage analysis.
        /// </summary>
        public TestCoverageAnalysis CoverageAnalysis { get; init; } = new();
    }

    /// <summary>
    /// Represents a generated test case.
    /// </summary>
    public record GeneratedTestCase
    {
        /// <summary>
        /// Gets or sets the test case name.
        /// </summary>
        public string TestName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the test case description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the test type.
        /// </summary>
        public TestType TestType { get; init; }

        /// <summary>
        /// Gets or sets the test input data.
        /// </summary>
        public string TestInput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected output.
        /// </summary>
        public string ExpectedOutput { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the test setup requirements.
        /// </summary>
        public string SetupRequirements { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated test code.
        /// </summary>
        public string TestCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the priority of this test case.
        /// </summary>
        public TestPriority Priority { get; init; }
    }

    /// <summary>
    /// Test coverage analysis results.
    /// </summary>
    public record TestCoverageAnalysis
    {
        /// <summary>
        /// Gets or sets the current statement coverage percentage.
        /// </summary>
        public double StatementCoverage { get; init; }

        /// <summary>
        /// Gets or sets the current branch coverage percentage.
        /// </summary>
        public double BranchCoverage { get; init; }

        /// <summary>
        /// Gets or sets the current path coverage percentage.
        /// </summary>
        public double PathCoverage { get; init; }

        /// <summary>
        /// Gets or sets the uncovered critical paths.
        /// </summary>
        public IReadOnlyList<string> UncoveredCriticalPaths { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets recommendations for improving coverage.
        /// </summary>
        public IReadOnlyList<string> CoverageImprovementRecommendations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Represents a proposed solution for validation.
    /// </summary>
    public record ProposedSolution
    {
        /// <summary>
        /// Gets or sets the solution identifier.
        /// </summary>
        public string SolutionId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the bug this solution addresses.
        /// </summary>
        public string TargetBugId { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the solution type.
        /// </summary>
        public SolutionType SolutionType { get; init; }

        /// <summary>
        /// Gets or sets the proposed changes.
        /// </summary>
        public IReadOnlyList<ProposedChange> ProposedChanges { get; init; } = Array.Empty<ProposedChange>();

        /// <summary>
        /// Gets or sets the implementation plan.
        /// </summary>
        public ImplementationPlan ImplementationPlan { get; init; } = new();

        /// <summary>
        /// Gets or sets the risk assessment for this solution.
        /// </summary>
        public SolutionRiskAssessment RiskAssessment { get; init; } = new();
    }

    /// <summary>
    /// Represents a proposed change as part of a solution.
    /// </summary>
    public record ProposedChange
    {
        /// <summary>
        /// Gets or sets the type of change.
        /// </summary>
        public ChangeType ChangeType { get; init; }

        /// <summary>
        /// Gets or sets the file or component being changed.
        /// </summary>
        public string TargetLocation { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the change.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the before state.
        /// </summary>
        public string BeforeState { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the after state.
        /// </summary>
        public string AfterState { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the impact assessment of this change.
        /// </summary>
        public ChangeImpactAssessment ImpactAssessment { get; init; } = new();
    }

    /// <summary>
    /// Implementation plan for a solution.
    /// </summary>
    public record ImplementationPlan
    {
        /// <summary>
        /// Gets or sets the implementation phases.
        /// </summary>
        public IReadOnlyList<ImplementationPhase> Phases { get; init; } = Array.Empty<ImplementationPhase>();

        /// <summary>
        /// Gets or sets the estimated total duration.
        /// </summary>
        public TimeSpan EstimatedDuration { get; init; }

        /// <summary>
        /// Gets or sets the required resources.
        /// </summary>
        public IReadOnlyList<string> RequiredResources { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the dependencies for implementation.
        /// </summary>
        public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the rollback plan.
        /// </summary>
        public string RollbackPlan { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents an implementation phase.
    /// </summary>
    public record ImplementationPhase
    {
        /// <summary>
        /// Gets or sets the phase name.
        /// </summary>
        public string PhaseName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the phase description.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the estimated duration for this phase.
        /// </summary>
        public TimeSpan EstimatedDuration { get; init; }

        /// <summary>
        /// Gets or sets the deliverables for this phase.
        /// </summary>
        public IReadOnlyList<string> Deliverables { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the acceptance criteria for this phase.
        /// </summary>
        public IReadOnlyList<string> AcceptanceCriteria { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Analysis phases for the debugging workflow.
    /// </summary>
    public enum AnalysisPhase
    {
        /// <summary>
        /// Structural analysis phase.
        /// </summary>
        StructuralAnalysis,

        /// <summary>
        /// Logic validation phase.
        /// </summary>
        LogicValidation,

        /// <summary>
        /// Data safety analysis phase.
        /// </summary>
        DataSafety,

        /// <summary>
        /// Error resilience analysis phase.
        /// </summary>
        ErrorResilience,

        /// <summary>
        /// Quality assurance phase.
        /// </summary>
        QualityAssurance
    }

    /// <summary>
    /// Test case priority levels.
    /// </summary>
    public enum TestPriority
    {
        /// <summary>
        /// Low priority test case.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Medium priority test case.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// High priority test case.
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical priority test case.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// Types of changes in a solution.
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// Code modification.
        /// </summary>
        CodeModification,

        /// <summary>
        /// Configuration change.
        /// </summary>
        ConfigurationChange,

        /// <summary>
        /// Architecture change.
        /// </summary>
        ArchitectureChange,

        /// <summary>
        /// Dependency update.
        /// </summary>
        DependencyUpdate,

        /// <summary>
        /// Documentation update.
        /// </summary>
        DocumentationUpdate,

        /// <summary>
        /// Test addition.
        /// </summary>
        TestAddition
    }
}