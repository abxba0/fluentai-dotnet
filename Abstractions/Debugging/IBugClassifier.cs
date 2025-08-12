using FluentAI.Abstractions.Debugging.Models;

namespace FluentAI.Abstractions.Debugging
{
    /// <summary>
    /// Provides bug classification and categorization capabilities.
    /// </summary>
    public interface IBugClassifier
    {
        /// <summary>
        /// Classifies a bug based on its characteristics and impact.
        /// </summary>
        /// <param name="bugEvidence">Evidence and details about the bug.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Bug classification result with priority and category.</returns>
        Task<BugClassification> ClassifyBugAsync(BugEvidence bugEvidence, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a comprehensive bug report based on analysis findings.
        /// </summary>
        /// <param name="analysisResults">Results from various analysis operations.</param>
        /// <param name="classification">Bug classification result.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Detailed bug report with recommendations.</returns>
        Task<BugReport> GenerateBugReportAsync(ComprehensiveAnalysisResult analysisResults, BugClassification classification, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assesses the impact of a bug on system functionality and users.
        /// </summary>
        /// <param name="bugEvidence">Evidence and details about the bug.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Impact assessment with severity and affected areas.</returns>
        Task<BugImpactAssessment> AssessImpactAsync(BugEvidence bugEvidence, CancellationToken cancellationToken = default);

        /// <summary>
        /// Prioritizes multiple bugs based on severity, impact, and business criteria.
        /// </summary>
        /// <param name="bugs">Collection of bug reports to prioritize.</param>
        /// <param name="prioritizationCriteria">Criteria for prioritization.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Prioritized list of bug reports.</returns>
        Task<IReadOnlyList<PrioritizedBugReport>> PrioritizeBugsAsync(IEnumerable<BugReport> bugs, PrioritizationCriteria prioritizationCriteria, CancellationToken cancellationToken = default);
    }
}