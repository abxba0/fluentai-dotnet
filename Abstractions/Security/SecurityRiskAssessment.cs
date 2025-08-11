namespace FluentAI.Abstractions.Security
{
    /// <summary>
    /// Represents the result of a security risk assessment for input content.
    /// </summary>
    public record SecurityRiskAssessment
    {
        /// <summary>
        /// Gets the overall risk level of the content.
        /// </summary>
        public SecurityRiskLevel RiskLevel { get; init; }

        /// <summary>
        /// Gets a list of detected security concerns.
        /// </summary>
        public IReadOnlyList<string> DetectedConcerns { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets additional context about the risk assessment.
        /// </summary>
        public string? AdditionalInfo { get; init; }

        /// <summary>
        /// Gets a value indicating whether the content should be blocked.
        /// </summary>
        public bool ShouldBlock => RiskLevel >= SecurityRiskLevel.High;
    }

    /// <summary>
    /// Defines security risk levels for content assessment.
    /// </summary>
    public enum SecurityRiskLevel
    {
        /// <summary>
        /// Content appears safe with no detected risks.
        /// </summary>
        None = 0,

        /// <summary>
        /// Content has minor potential risks but is generally safe.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Content has moderate risks that should be monitored.
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Content has high risks and should be blocked or heavily scrutinized.
        /// </summary>
        High = 3,

        /// <summary>
        /// Content is critically dangerous and should be immediately blocked.
        /// </summary>
        Critical = 4
    }
}