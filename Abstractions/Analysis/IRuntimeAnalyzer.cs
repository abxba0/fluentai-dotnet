using System.IO;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Provides runtime-aware code analysis that examines codebases for issues that occur during execution.
    /// Unlike static analysis, this analyzer simulates runtime behavior to identify environment dependencies,
    /// resource constraints, error propagation patterns, and edge case failures.
    /// </summary>
    public interface IRuntimeAnalyzer
    {
        /// <summary>
        /// Analyzes a single source code file for runtime issues.
        /// </summary>
        /// <param name="filePath">Path to the source code file to analyze.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Runtime analysis result containing identified issues and recommendations.</returns>
        Task<RuntimeAnalysisResult> AnalyzeFileAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes multiple source code files for runtime issues.
        /// </summary>
        /// <param name="filePaths">Collection of file paths to analyze.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Combined runtime analysis result for all files.</returns>
        Task<RuntimeAnalysisResult> AnalyzeFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes an entire directory for runtime issues.
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing source code files.</param>
        /// <param name="searchPattern">File search pattern (default: "*.cs").</param>
        /// <param name="includeSubdirectories">Whether to include subdirectories in analysis.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Runtime analysis result for all matching files in the directory.</returns>
        Task<RuntimeAnalysisResult> AnalyzeDirectoryAsync(
            string directoryPath, 
            string searchPattern = "*.cs", 
            bool includeSubdirectories = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes source code content directly without file system access.
        /// </summary>
        /// <param name="sourceCode">The source code content to analyze.</param>
        /// <param name="fileName">Optional file name for context in reports.</param>
        /// <param name="cancellationToken">Cancellation token for async operation.</param>
        /// <returns>Runtime analysis result for the provided source code.</returns>
        Task<RuntimeAnalysisResult> AnalyzeSourceAsync(string sourceCode, string? fileName = null, CancellationToken cancellationToken = default);
    }
}