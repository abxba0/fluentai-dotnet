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
        private readonly IPiiDetectionService? _piiDetectionService;

        // Common prompt injection patterns - SECURITY FIX: ReDoS vulnerability prevention
        private static readonly Regex[] PromptInjectionPatterns = new[]
        {
            // FIXED: Eliminated catastrophic backtracking by removing nested quantifiers
            new Regex(@"ignore\s+(?:all|previous|above|prior)(?:\s+(?:all|previous|above|prior))?\s+(?:instructions?|prompts?|rules?)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"forget\s+(?:everything|all|previous|above)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"act\s+as\s+(?:a\s+)?(?:different|new|another)\s+(?:ai|assistant|character|persona)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"system\s*:\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"assistant\s*:\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"\[/?(?:system|assistant|user)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"simulate\s+(?:being|that you are)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"pretend\s+(?:to be|that you are)", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"roleplay\s+as", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"developer\s+mode", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100)),
            new Regex(@"jailbreak|dan\s+mode", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100))
        };

        // Suspicious token sequences that might indicate injection attempts
        private static readonly string[] SuspiciousTokens = new[]
        {
            "###", "---", "```", "<|", "|>", "</s>", "<s>", "[INST]", "[/INST]",
            "<human>", "</human>", "<assistant>", "</assistant>", "<system>", "</system>"
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInputSanitizer"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="piiDetectionService">Optional PII detection service.</param>
        public DefaultInputSanitizer(ILogger<DefaultInputSanitizer> logger, IPiiDetectionService? piiDetectionService = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _piiDetectionService = piiDetectionService;
        }

        /// <inheritdoc />
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

            // SECURITY FIX: Add timeout to prevent ReDoS attacks
            sanitized = Regex.Replace(sanitized, @"\s{3,}", " ", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
            // Allow escaped tokens to pass through by excluding characters used in escaping and suspicious tokens
            sanitized = Regex.Replace(sanitized, @"[^\w\s\.,!?;:()\-""'\[\]:<>#|`/]+", "", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

            return sanitized.Trim();
        }

        /// <inheritdoc />
        public bool IsContentSafe(string content)
        {
            var assessment = AssessRisk(content);
            return !assessment.ShouldBlock;
        }

        /// <inheritdoc />
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
            else // Only check for repeated patterns if content length is reasonable
            {
                // SECURITY FIX: Add timeout to prevent ReDoS attacks  
                try
                {
                    var repeatedPatterns = Regex.Matches(content, @"(.{10,})\1{3,}", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
                    if (repeatedPatterns.Count > 0)
                    {
                        concerns.Add("Repeated patterns detected (potential injection)");
                        riskLevel = (SecurityRiskLevel)Math.Max((int)riskLevel, (int)SecurityRiskLevel.Medium);
                    }
                }
                catch (RegexMatchTimeoutException)
                {
                    // If regex times out, treat as potential risk
                    concerns.Add("Complex pattern detected (regex timeout)");
                    riskLevel = (SecurityRiskLevel)Math.Max((int)riskLevel, (int)SecurityRiskLevel.Medium);
                }
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

        /// <inheritdoc />
        public async Task<string> SanitizeContentWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // First apply standard sanitization
            var sanitizedContent = SanitizeContent(content);

            // Then apply PII detection and remediation if service is available
            if (_piiDetectionService != null)
            {
                try
                {
                    var detectionResult = await _piiDetectionService.ScanAsync(sanitizedContent, piiOptions).ConfigureAwait(false);
                    
                    if (detectionResult.HasPii)
                    {
                        _logger.LogInformation("PII detected during sanitization: {DetectionCount} detections", 
                            detectionResult.Detections.Count);

                        // Apply different remediation strategies based on detected PII
                        foreach (var detection in detectionResult.Detections)
                        {
                            switch (detection.Action)
                            {
                                case PiiAction.Block:
                                    _logger.LogWarning("Content blocked due to PII detection: {Type}", detection.Type);
                                    return "[CONTENT BLOCKED - PII DETECTED]";
                                
                                case PiiAction.Redact:
                                    sanitizedContent = await _piiDetectionService.RedactAsync(sanitizedContent, detectionResult).ConfigureAwait(false);
                                    break;
                                
                                case PiiAction.Tokenize:
                                    sanitizedContent = await _piiDetectionService.TokenizeAsync(sanitizedContent, detectionResult).ConfigureAwait(false);
                                    break;
                                
                                case PiiAction.Mask:
                                    sanitizedContent = ApplyMasking(sanitizedContent, detection);
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during PII detection in sanitization");
                    // Continue with standard sanitized content if PII detection fails
                }
            }

            return sanitizedContent;
        }

        /// <inheritdoc />
        public async Task<bool> IsContentSafeWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null)
        {
            // Check basic content safety first
            if (!IsContentSafe(content))
            {
                return false;
            }

            // Check PII safety if service is available
            if (_piiDetectionService != null)
            {
                try
                {
                    // PERFORMANCE FIX: Add ConfigureAwait(false) to prevent deadlocks in library code
                    var detectionResult = await _piiDetectionService.ScanAsync(content, piiOptions).ConfigureAwait(false);
                    
                    // Content is unsafe if PII should be blocked
                    if (detectionResult.ShouldBlock)
                    {
                        _logger.LogWarning("Content marked unsafe due to PII detection policy");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during PII safety assessment");
                    // Fail safe - consider content unsafe if PII check fails
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<SecurityRiskAssessment> AssessRiskWithPiiAsync(string content, Abstractions.Security.PiiDetectionOptions? piiOptions = null)
        {
            // Start with basic risk assessment
            var baseAssessment = AssessRisk(content);
            var concerns = new List<string>(baseAssessment.DetectedConcerns);
            var riskLevel = baseAssessment.RiskLevel;
            var additionalInfo = baseAssessment.AdditionalInfo ?? string.Empty;

            // Enhance with PII risk assessment if service is available
            if (_piiDetectionService != null)
            {
                try
                {
                    // PERFORMANCE FIX: Add ConfigureAwait(false) to prevent deadlocks in library code
                    var detectionResult = await _piiDetectionService.ScanAsync(content, piiOptions).ConfigureAwait(false);
                    
                    if (detectionResult.HasPii)
                    {
                        concerns.Add($"PII detected: {detectionResult.Detections.Count} instances");
                        
                        // Escalate risk level based on PII findings
                        if (detectionResult.OverallRiskLevel > riskLevel)
                        {
                            riskLevel = detectionResult.OverallRiskLevel;
                        }

                        // Add specific PII concerns
                        foreach (var detection in detectionResult.Detections)
                        {
                            concerns.Add($"PII type {detection.Type} detected (confidence: {detection.Confidence:F2})");
                        }

                        additionalInfo += $" PII risk level: {detectionResult.OverallRiskLevel}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during PII risk assessment");
                    concerns.Add("PII risk assessment failed - assuming elevated risk");
                    if (riskLevel < SecurityRiskLevel.Medium)
                    {
                        riskLevel = SecurityRiskLevel.Medium;
                    }
                }
            }

            return new SecurityRiskAssessment
            {
                RiskLevel = riskLevel,
                DetectedConcerns = concerns.AsReadOnly(),
                AdditionalInfo = additionalInfo
            };
        }

        private static string ApplyMasking(string content, PiiDetection detection)
        {
            var detectedContent = detection.DetectedContent;
            string maskedContent;

            // Apply different masking strategies based on PII type
            switch (detection.Type.ToLower())
            {
                case "creditcard":
                    // Show only last 4 digits
                    maskedContent = detectedContent.Length > 4 
                        ? "****-****-****-" + detectedContent[^4..] 
                        : "****";
                    break;
                
                case "ssn":
                    // Show only last 4 digits in XXX-XX-1234 format
                    maskedContent = detectedContent.Length >= 4 
                        ? "XXX-XX-" + detectedContent[^4..] 
                        : "XXX-XX-XXXX";
                    break;
                
                case "email":
                    // Show first character and domain
                    var atIndex = detectedContent.IndexOf('@');
                    if (atIndex > 0)
                    {
                        var domain = detectedContent[(atIndex + 1)..];
                        maskedContent = $"{detectedContent[0]}***@{domain}";
                    }
                    else
                    {
                        maskedContent = "***@***.***";
                    }
                    break;
                
                case "phone":
                    // Show only last 4 digits
                    maskedContent = detectedContent.Length >= 4 
                        ? "***-***-" + detectedContent[^4..] 
                        : "***-***-****";
                    break;
                
                default:
                    // Generic masking - show first and last character if long enough
                    maskedContent = detectedContent.Length switch
                    {
                        <= 2 => "***",
                        <= 4 => detectedContent[0] + "***",
                        _ => detectedContent[0] + "***" + detectedContent[^1]
                    };
                    break;
            }

            return content.Remove(detection.StartPosition, detection.EndPosition - detection.StartPosition)
                         .Insert(detection.StartPosition, maskedContent);
        }
    }
}