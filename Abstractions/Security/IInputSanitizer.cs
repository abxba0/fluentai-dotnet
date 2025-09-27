using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Security
{
    /// <summary>
    /// Provides input sanitization and validation for AI chat models with PII detection support.
    /// </summary>
    public interface IInputSanitizer
    {
        /// <summary>
        /// Sanitizes chat message content to prevent prompt injection and other security issues.
        /// </summary>
        /// <param name="content">The content to sanitize.</param>
        /// <returns>The sanitized content.</returns>
        string SanitizeContent(string content);

        /// <summary>
        /// Sanitizes content with PII detection and redaction.
        /// </summary>
        /// <param name="content">The content to sanitize.</param>
        /// <param name="piiOptions">PII detection options.</param>
        /// <returns>The sanitized content with PII handled according to policy.</returns>
        Task<string> SanitizeContentWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null);

        /// <summary>
        /// Validates that chat message content is safe and doesn't contain malicious patterns.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <returns>True if content is safe, false otherwise.</returns>
        bool IsContentSafe(string content);

        /// <summary>
        /// Validates content safety including PII detection results.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <param name="piiOptions">PII detection options.</param>
        /// <returns>True if content is safe including PII considerations, false otherwise.</returns>
        Task<bool> IsContentSafeWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null);

        /// <summary>
        /// Analyzes content for potential prompt injection attempts.
        /// </summary>
        /// <param name="content">The content to analyze.</param>
        /// <returns>Risk assessment result.</returns>
        SecurityRiskAssessment AssessRisk(string content);

        /// <summary>
        /// Performs comprehensive risk assessment including PII detection.
        /// </summary>
        /// <param name="content">The content to assess.</param>
        /// <param name="piiOptions">PII detection options.</param>
        /// <returns>Comprehensive security risk assessment including PII risks.</returns>
        Task<SecurityRiskAssessment> AssessRiskWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null);
    }
}