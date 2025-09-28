using System.Threading.Tasks;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Interface for runtime-aware code analysis services.
    /// </summary>
    public interface IRuntimeAnalyzer
    {
        /// <summary>
        /// Analyzes source code for runtime issues and potential problems.
        /// </summary>
        /// <param name="sourceCode">The source code to analyze.</param>
        /// <param name="fileName">The name of the file being analyzed.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A runtime analysis result containing detected issues.</returns>
        Task<RuntimeAnalysisResult> AnalyzeSourceAsync(string sourceCode, string fileName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes multiple source files for runtime issues.
        /// </summary>
        /// <param name="filePaths">The paths to the files to analyze.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A combined runtime analysis result.</returns>
        Task<RuntimeAnalysisResult> AnalyzeFilesAsync(string[] filePaths, CancellationToken cancellationToken = default);
    }
}