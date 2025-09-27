namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Represents an edge case failure detected during analysis.
    /// </summary>
    public class EdgeCaseFailure
    {
        /// <summary>
        /// Gets or sets the unique identifier for this edge case.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the input that could cause the failure.
        /// </summary>
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the scenario description.
        /// </summary>
        public string Scenario { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected failure description.
        /// </summary>
        public string ExpectedFailure { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the edge case.
        /// </summary>
        public EdgeCaseSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the location where the edge case could occur.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the expected result.
        /// </summary>
        public string Expected { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the actual result.
        /// </summary>
        public string Actual { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the fix for the edge case.
        /// </summary>
        public string Fix { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file path where the edge case could occur.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// Specifies the severity of an edge case failure.
    /// </summary>
    public enum EdgeCaseSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}