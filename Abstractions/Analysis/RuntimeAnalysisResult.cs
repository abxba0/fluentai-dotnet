using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Represents the result of a runtime code analysis.
    /// </summary>
    public class RuntimeAnalysisResult
    {
        /// <summary>
        /// Gets or sets the runtime issues detected during analysis.
        /// </summary>
        public IEnumerable<RuntimeIssue> RuntimeIssues { get; set; } = Array.Empty<RuntimeIssue>();

        /// <summary>
        /// Gets or sets the environment risks detected during analysis.
        /// </summary>
        public IEnumerable<EnvironmentRisk> EnvironmentRisks { get; set; } = Array.Empty<EnvironmentRisk>();

        /// <summary>
        /// Gets or sets the edge case failures detected during analysis.
        /// </summary>
        public IEnumerable<EdgeCaseFailure> EdgeCaseFailures { get; set; } = Array.Empty<EdgeCaseFailure>();

        /// <summary>
        /// Gets or sets the metadata about the analysis.
        /// </summary>
        public AnalysisMetadata Metadata { get; set; } = new AnalysisMetadata();

        /// <summary>
        /// Gets the total number of issues detected.
        /// </summary>
        public int TotalIssueCount => RuntimeIssues.Count() + EnvironmentRisks.Count() + EdgeCaseFailures.Count();

        /// <summary>
        /// Gets a value indicating whether critical issues were detected.
        /// </summary>
        public bool HasCriticalIssues => 
            RuntimeIssues.Any(r => r.Severity == RuntimeIssueSeverity.Critical) ||
            EnvironmentRisks.Any(e => e.Likelihood == RiskLikelihood.High);
    }
}