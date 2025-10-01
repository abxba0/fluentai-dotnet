using System;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Represents an environment-related risk detected during analysis.
    /// </summary>
    public class EnvironmentRisk
    {
        /// <summary>
        /// Gets or sets the unique identifier for this risk.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of environment risk.
        /// </summary>
        public EnvironmentRiskType Type { get; set; }

        /// <summary>
        /// Gets or sets the likelihood of this risk occurring.
        /// </summary>
        public RiskLikelihood Likelihood { get; set; }

        /// <summary>
        /// Gets or sets the description of the risk.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the potential impact of the risk.
        /// </summary>
        public string Impact { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the recommended mitigation for the risk.
        /// </summary>
        public RiskMitigation? Mitigation { get; set; }

        /// <summary>
        /// Gets or sets the component associated with the risk.
        /// </summary>
        public string Component { get; set; } = string.Empty;
    }

    /// <summary>
    /// Specifies the type of environment risk.
    /// </summary>
    public enum EnvironmentRiskType
    {
        /// <summary>
        /// Configuration-related risk.
        /// </summary>
        Configuration,
        
        /// <summary>
        /// Dependency-related risk.
        /// </summary>
        Dependency,
        
        /// <summary>
        /// Security-related risk.
        /// </summary>
        Security,
        
        /// <summary>
        /// Performance-related risk.
        /// </summary>
        Performance,
        
        /// <summary>
        /// Compatibility-related risk.
        /// </summary>
        Compatibility,
        
        /// <summary>
        /// Deployment-related risk.
        /// </summary>
        Deployment
    }

    /// <summary>
    /// Specifies the likelihood of a risk occurring.
    /// </summary>
    public enum RiskLikelihood
    {
        /// <summary>
        /// Low likelihood - unlikely to occur.
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium likelihood - may occur under certain conditions.
        /// </summary>
        Medium,
        
        /// <summary>
        /// High likelihood - likely to occur.
        /// </summary>
        High,
        
        /// <summary>
        /// Critical likelihood - very likely or certain to occur.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Represents detailed mitigation information for a risk.
    /// </summary>
    public class RiskMitigation
    {
        /// <summary>
        /// Gets or sets the required changes to mitigate the risk.
        /// </summary>
        public string[] RequiredChanges { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the monitoring information.
        /// </summary>
        public string Monitoring { get; set; } = string.Empty;
    }
}