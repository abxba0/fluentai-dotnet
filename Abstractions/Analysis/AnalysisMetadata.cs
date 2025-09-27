using System;
using System.Collections.Generic;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Contains metadata about the runtime analysis.
    /// </summary>
    public class AnalysisMetadata
    {
        /// <summary>
        /// Gets or sets the timestamp when the analysis was performed.
        /// </summary>
        public DateTime AnalysisTimestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the list of files that were analyzed.
        /// </summary>
        public IEnumerable<string> AnalyzedFiles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the duration of the analysis.
        /// </summary>
        public TimeSpan AnalysisDuration { get; set; }

        /// <summary>
        /// Gets or sets the version of the analyzer used.
        /// </summary>
        public string AnalyzerVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Gets or sets any additional notes about the analysis.
        /// </summary>
        public string Notes { get; set; } = string.Empty;
    }
}