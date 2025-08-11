using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Security
{
    /// <summary>
    /// Provides input sanitization and validation for AI chat models.
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
        /// Validates that chat message content is safe and doesn't contain malicious patterns.
        /// </summary>
        /// <param name="content">The content to validate.</param>
        /// <returns>True if content is safe, false otherwise.</returns>
        bool IsContentSafe(string content);

        /// <summary>
        /// Analyzes content for potential prompt injection attempts.
        /// </summary>
        /// <param name="content">The content to analyze.</param>
        /// <returns>Risk assessment result.</returns>
        SecurityRiskAssessment AssessRisk(string content);
    }
}