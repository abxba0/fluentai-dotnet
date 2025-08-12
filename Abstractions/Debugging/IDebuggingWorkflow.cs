using FluentAI.Abstractions.Debugging.Models;

namespace FluentAI.Abstractions.Debugging
{
    /// <summary>
    /// Provides automated debugging workflow orchestration and management.
    /// </summary>
    public interface IDebuggingWorkflow
    {
        /// <summary>
        /// Executes a complete debugging workflow from analysis to solution validation.
        /// </summary>
        /// <param name="workflowContext">Context and configuration for the debugging workflow.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Complete debugging workflow result with analysis, bugs, and solutions.</returns>
        Task<DebuggingWorkflowResult> ExecuteWorkflowAsync(DebuggingWorkflowContext workflowContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates test cases for identified bugs and edge cases.
        /// </summary>
        /// <param name="analysisResults">Results from code analysis operations.</param>
        /// <param name="bugReports">Identified bug reports.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Generated test cases for validation and regression prevention.</returns>
        Task<TestCaseGenerationResult> GenerateTestCasesAsync(ComprehensiveAnalysisResult analysisResults, IEnumerable<BugReport> bugReports, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates proposed solutions before implementation.
        /// </summary>
        /// <param name="solution">Proposed solution to validate.</param>
        /// <param name="originalBug">Original bug being addressed.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Solution validation result with recommendations.</returns>
        Task<SolutionValidationResult> ValidateSolutionAsync(ProposedSolution solution, BugReport originalBug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs post-fix validation to ensure the bug is resolved and no regressions are introduced.
        /// </summary>
        /// <param name="implementedSolution">Solution that was implemented.</param>
        /// <param name="originalBug">Original bug that was addressed.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Post-fix validation result with regression analysis.</returns>
        Task<PostFixValidationResult> ValidatePostFixAsync(ImplementedSolution implementedSolution, BugReport originalBug, CancellationToken cancellationToken = default);

        /// <summary>
        /// Monitors system health and detects new issues after fixes are applied.
        /// </summary>
        /// <param name="monitoringContext">Context for health monitoring.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>System health monitoring result.</returns>
        Task<SystemHealthResult> MonitorSystemHealthAsync(HealthMonitoringContext monitoringContext, CancellationToken cancellationToken = default);
    }
}