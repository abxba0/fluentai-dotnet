using FluentAI.Abstractions.Security;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace FluentAI.Abstractions.Security
{
    /// <summary>
    /// Default implementation of input sanitizer for AI chat models.
    /// Provides protection against prompt injection and other security threats.
    /// </summary>
    public class DefaultInputSanitizer : IInputSanitizer
    {
        private readonly ILogger<DefaultInputSanitizer> _logger;

        // Common prompt injection patterns
        private static readonly Regex[] PromptInjectionPatterns = new[]
        {
            // Enhanced pattern to catch compound injection phrases like "ignore all previous instructions"
            new Regex(@"(ignore\s+(?:(?:all|previous|above|prior)\s+)*(?:previous|all|above|prior)\s+(?:instructions?|prompts?|rules?))", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(forget\s+(?:everything|all|previous|above))", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(act\s+as\s+(?:a\s+)?(?:different|new|another)\s+(?:ai|assistant|character|persona))", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(system\s*[:]\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(assistant\s*[:]\s*)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(\[/?(?:system|assistant|user)\])", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(simulate\s+(?:being|that you are))", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(pretend\s+(?:to be|that you are))", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(roleplay\s+as)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(developer\s+mode)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new Regex(@"(jailbreak|dan\s+mode)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        // Suspicious token sequences that might indicate injection attempts
        private static readonly string[] SuspiciousTokens = new[]
        {
            "###", "---", "```", "<|", "|>", "</s>", "<s>", "[INST]", "[/INST]",
            "<human>", "</human>", "<assistant>", "</assistant>", "<system>", "</system>"
        };

        public DefaultInputSanitizer(ILogger<DefaultInputSanitizer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string SanitizeContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            var sanitized = content;

            // Remove or escape suspicious tokens
            foreach (var token in SuspiciousTokens)
            {
                sanitized = sanitized.Replace(token, $"[ESCAPED:{token}]", StringComparison.Ordinal);
            }

            // Normalize excessive whitespace and special characters
            sanitized = Regex.Replace(sanitized, @"\s{3,}", " ", RegexOptions.Compiled);
            // Allow escaped tokens to pass through by excluding characters used in escaping and suspicious tokens
            sanitized = Regex.Replace(sanitized, @"[^\w\s\.,!?;:()\-""'\[\]:<>#|`/]+", "", RegexOptions.Compiled);

            return sanitized.Trim();
        }

        public bool IsContentSafe(string content)
        {
            var assessment = AssessRisk(content);
            return !assessment.ShouldBlock;
        }

        public SecurityRiskAssessment AssessRisk(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new SecurityRiskAssessment { RiskLevel = SecurityRiskLevel.None };
            }

            var concerns = new List<string>();
            var riskLevel = SecurityRiskLevel.None;

            // Check for prompt injection patterns
            foreach (var pattern in PromptInjectionPatterns)
            {
                if (pattern.IsMatch(content))
                {
                    concerns.Add($"Potential prompt injection detected: {pattern}");
                    riskLevel = SecurityRiskLevel.High;
                }
            }

            // Check for suspicious tokens - Improved logic to correctly assess risk levels
            var suspiciousTokenCount = SuspiciousTokens.Count(token => 
                content.Contains(token, StringComparison.OrdinalIgnoreCase));
            
            if (suspiciousTokenCount > 0)
            {
                concerns.Add("Suspicious tokens detected");
                // Improved risk assessment: 3+ tokens = High, 1-2 tokens = Medium  
                riskLevel = (SecurityRiskLevel)Math.Max((int)riskLevel, suspiciousTokenCount >= 3 ? (int)SecurityRiskLevel.High : (int)SecurityRiskLevel.Medium);
            }

            // Check for excessive length (potential DoS)
            if (content.Length > 50000)
            {
                concerns.Add("Content length exceeds safe limits");
                riskLevel = (SecurityRiskLevel)Math.Max((int)riskLevel, (int)SecurityRiskLevel.Medium);
            }

            // Check for repeated patterns (potential injection)
            var repeatedPatterns = Regex.Matches(content, @"(.{10,})\1{3,}", RegexOptions.Compiled);
            if (repeatedPatterns.Count > 0)
            {
                concerns.Add("Repeated patterns detected (potential injection)");
                riskLevel = (SecurityRiskLevel)Math.Max((int)riskLevel, (int)SecurityRiskLevel.Medium);
            }

            var finalRiskLevel = (SecurityRiskLevel)riskLevel;

            // Log high-risk content for monitoring
            if (finalRiskLevel >= SecurityRiskLevel.High)
            {
                _logger.LogWarning("High-risk content detected: {RiskLevel}, Concerns: {Concerns}", 
                    finalRiskLevel, string.Join(", ", concerns));
            }

            return new SecurityRiskAssessment
            {
                RiskLevel = finalRiskLevel,
                DetectedConcerns = concerns.AsReadOnly(),
                AdditionalInfo = concerns.Any() ? $"Assessment based on {concerns.Count} detected issues" : null
            };
        }
    }
}