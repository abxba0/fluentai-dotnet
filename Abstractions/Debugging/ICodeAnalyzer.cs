using FluentAI.Abstractions.Debugging.Models;

namespace FluentAI.Abstractions.Debugging
{
    /// <summary>
    /// Provides comprehensive code analysis capabilities for bug detection and system diagnosis.
    /// </summary>
    public interface ICodeAnalyzer
    {
        /// <summary>
        /// Performs flow analysis on code execution paths.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Flow analysis results containing execution paths, conditions, and issues.</returns>
        Task<FlowAnalysisResult> AnalyzeFlowAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes state management and data flow patterns.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>State analysis results containing transitions, concurrency issues, and integrity checks.</returns>
        Task<StateAnalysisResult> AnalyzeStateManagementAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs edge case and boundary analysis.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Edge case analysis results containing boundary violations and risk assessments.</returns>
        Task<EdgeCaseAnalysisResult> AnalyzeEdgeCasesAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes error propagation and recovery mechanisms.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Error analysis results containing propagation paths and recovery strategies.</returns>
        Task<ErrorAnalysisResult> AnalyzeErrorPropagationAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs performance and resource analysis.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Performance analysis results containing bottlenecks and resource usage patterns.</returns>
        Task<PerformanceAnalysisResult> AnalyzePerformanceAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Performs comprehensive analysis combining all analysis types.
        /// </summary>
        /// <param name="analysisContext">Context for the analysis operation.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        /// <returns>Comprehensive analysis results containing all analysis findings.</returns>
        Task<ComprehensiveAnalysisResult> AnalyzeComprehensivelyAsync(AnalysisContext analysisContext, CancellationToken cancellationToken = default);
    }
}