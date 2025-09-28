using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FluentAI.Abstractions.Security
{
    /// <summary>
    /// Provides secure logging functionality that masks sensitive information.
    /// </summary>
    public static class SecureLogger
    {
        // SECURITY FIX: Add timeouts to prevent ReDoS attacks
        private static readonly Regex ApiKeyPattern = new(@"(?i)(api[_-]?key|token|secret|password)\s*[=:]\s*([a-zA-Z0-9\-_]{8,})", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        private static readonly Regex EmailPattern = new(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        
        /// <summary>
        /// Masks sensitive information in the given text.
        /// </summary>
        /// <param name="text">The text to mask.</param>
        /// <returns>The text with sensitive information masked.</returns>
        public static string MaskSensitiveData(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text ?? string.Empty;

            var masked = text;

            // Mask API keys, tokens, secrets
            masked = ApiKeyPattern.Replace(masked, match =>
            {
                var key = match.Groups[1].Value;
                var value = match.Groups[2].Value;
                var maskedValue = value.Length > 8 
                    ? $"{value[..4]}***{value[^4..]}" 
                    : "***";
                return $"{key}={maskedValue}";
            });

            // Mask email addresses
            masked = EmailPattern.Replace(masked, match =>
            {
                var email = match.Value;
                var atIndex = email.IndexOf('@');
                if (atIndex > 2)
                {
                    return $"{email[..2]}***{email[atIndex..]}";
                }
                return "***@domain.com";
            });

            return masked;
        }

        /// <summary>
        /// Logs information with sensitive data masking.
        /// </summary>
        public static void LogInformationSecure(this ILogger logger, string message, params object?[] args)
        {
            var maskedArgs = args?.Select(arg => arg?.ToString() is string str ? MaskSensitiveData(str) : arg).ToArray() ?? Array.Empty<object>();
            logger.LogInformation(MaskSensitiveData(message), maskedArgs);
        }

        /// <summary>
        /// Logs warning with sensitive data masking.
        /// </summary>
        public static void LogWarningSecure(this ILogger logger, string message, params object?[] args)
        {
            var maskedArgs = args?.Select(arg => arg?.ToString() is string str ? MaskSensitiveData(str) : arg).ToArray() ?? Array.Empty<object>();
            logger.LogWarning(MaskSensitiveData(message), maskedArgs);
        }

        /// <summary>
        /// Logs error with sensitive data masking.
        /// </summary>
        public static void LogErrorSecure(this ILogger logger, Exception? exception, string message, params object?[] args)
        {
            var maskedArgs = args?.Select(arg => arg?.ToString() is string str ? MaskSensitiveData(str) : arg).ToArray() ?? Array.Empty<object>();
            logger.LogError(exception, MaskSensitiveData(message), maskedArgs);
        }
    }
}