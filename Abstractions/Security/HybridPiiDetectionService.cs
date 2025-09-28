using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FluentAI.Abstractions.Security;

/// <summary>
/// Hybrid PII detection service combining regex patterns with extensible detection logic.
/// </summary>
public class HybridPiiDetectionService : IPiiDetectionService
{
    private readonly ILogger<HybridPiiDetectionService> _logger;
    private readonly IOptions<Configuration.PiiDetectionOptions> _configOptions;
    private readonly IPiiPatternRegistry _patternRegistry;
    private readonly IPiiClassificationEngine _classificationEngine;
    private readonly ConcurrentDictionary<string, string> _tokenCache;
    private readonly ConcurrentDictionary<string, PiiDetectionResult> _resultCache;

    // Built-in PII patterns - SECURITY FIX: Add timeouts to prevent ReDoS attacks
    private static readonly Dictionary<string, PiiPattern> BuiltInPatterns = new()
    {
        ["CreditCard"] = new()
        {
            Name = "CreditCard",
            Category = PiiCategory.Financial,
            Pattern = @"\b(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3[0-9]{13}|6(?:011|5[0-9]{2})[0-9]{12})\b",
            CompiledPattern = new Regex(@"\b(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3[0-9]{13}|6(?:011|5[0-9]{2})[0-9]{12})\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)),
            Confidence = 0.9,
            DefaultAction = PiiAction.Redact,
            DefaultReplacement = "[CREDIT_CARD]",
            CustomValidator = ValidateCreditCard
        },
        ["SSN"] = new()
        {
            Name = "SSN",
            Category = PiiCategory.Government,
            Pattern = @"\b(?!000|666|9\d{2})\d{3}-(?!00)\d{2}-(?!0000)\d{4}\b|\b(?!000|666|9\d{2})\d{3}(?!00)\d{2}(?!0000)\d{4}\b",
            CompiledPattern = new Regex(@"\b(?!000|666|9\d{2})\d{3}-(?!00)\d{2}-(?!0000)\d{4}\b|\b(?!000|666|9\d{2})\d{3}(?!00)\d{2}(?!0000)\d{4}\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)),
            Confidence = 0.95,
            DefaultAction = PiiAction.Block,
            SupportedRegions = new[] { "US" }
        },
        ["Email"] = new()
        {
            Name = "Email",
            Category = PiiCategory.Contact,
            Pattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b",
            CompiledPattern = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)),
            Confidence = 0.8,
            DefaultAction = PiiAction.Tokenize
        },
        ["Phone"] = new()
        {
            Name = "Phone",
            Category = PiiCategory.Contact,
            Pattern = @"\b(?:\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})\b",
            CompiledPattern = new Regex(@"\b(?:\+?1[-.\s]?)?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})\b", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100)),
            Confidence = 0.85,
            DefaultAction = PiiAction.Mask
        }
    };

    public HybridPiiDetectionService(
        ILogger<HybridPiiDetectionService> logger,
        IOptions<Configuration.PiiDetectionOptions> configOptions,
        IPiiPatternRegistry patternRegistry,
        IPiiClassificationEngine classificationEngine)
    {
        _logger = logger;
        _configOptions = configOptions;
        _patternRegistry = patternRegistry;
        _classificationEngine = classificationEngine;
        _tokenCache = new ConcurrentDictionary<string, string>();
        _resultCache = new ConcurrentDictionary<string, PiiDetectionResult>();
    }

    public async Task<PiiDetectionResult> ScanAsync(string content, Abstractions.Security.PiiDetectionOptions? options = null)
    {
        if (string.IsNullOrEmpty(content))
        {
            return new PiiDetectionResult { OriginalContent = content };
        }

        var effectiveOptions = options ?? new Abstractions.Security.PiiDetectionOptions();
        var cacheKey = GenerateCacheKey(content, effectiveOptions);

        // Check cache if enabled
        if (_configOptions.Value.Performance.CacheResults && _resultCache.TryGetValue(cacheKey, out var cachedResult))
        {
            _logger.LogDebug("PII detection result retrieved from cache");
            return cachedResult;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var detections = new List<PiiDetection>();

        try
        {
            // PERFORMANCE FIX: Add ConfigureAwait(false) to prevent deadlocks in library code
            var patterns = await GetEffectivePatternsAsync(effectiveOptions).ConfigureAwait(false);
            var evaluatedPatterns = new List<string>();

            foreach (var pattern in patterns)
            {
                if (!pattern.Enabled) continue;

                evaluatedPatterns.Add(pattern.Name);
                
                var matches = pattern.CompiledPattern.Matches(content);
                foreach (Match match in matches)
                {
                    var confidence = pattern.Confidence;
                    
                    // Apply custom validation if available
                    if (pattern.CustomValidator != null)
                    {
                        if (!pattern.CustomValidator(match.Value))
                        {
                            confidence *= 0.5; // Reduce confidence for failed validation
                        }
                    }

                    if (confidence >= effectiveOptions.MinimumConfidence)
                    {
                        var detection = new PiiDetection
                        {
                            Category = pattern.Category,
                            Type = pattern.Name,
                            DetectedContent = match.Value,
                            StartPosition = match.Index,
                            EndPosition = match.Index + match.Length,
                            Confidence = confidence,
                            Action = pattern.DefaultAction,
                            Replacement = pattern.DefaultReplacement,
                            Context = effectiveOptions.IncludeContext ? 
                                GetDetectionContext(content, match.Index, match.Length) : 
                                new Dictionary<string, object>()
                        };

                        detections.Add(detection);
                    }
                }
            }

            stopwatch.Stop();

            var result = new PiiDetectionResult
            {
                OriginalContent = content,
                Detections = detections.AsReadOnly(),
                OverallRiskLevel = CalculateOverallRiskLevel(detections),
                Metadata = new PiiDetectionMetadata
                {
                    EvaluatedPatterns = evaluatedPatterns.AsReadOnly(),
                    ProcessingTime = stopwatch.Elapsed,
                    PerformanceMetrics = new Dictionary<string, object>
                    {
                        ["PatternCount"] = patterns.Count(),
                        ["MatchCount"] = detections.Count,
                        ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds
                    }
                }
            };

            // Cache result if enabled
            if (_configOptions.Value.Performance.CacheResults)
            {
                var cacheTTL = _configOptions.Value.Performance.CacheTTL;
                _resultCache.TryAdd(cacheKey, result);
                
                // Schedule cache cleanup (simplified - in production you'd use a proper cache with expiration)
                _ = Task.Delay(cacheTTL).ContinueWith(_ => 
                {
                    _resultCache.TryRemove(cacheKey, out var _);
                });
            }

            _logger.LogInformation("PII detection completed: {DetectionCount} detections in {ProcessingTime}ms", 
                detections.Count, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PII detection scan");
            throw;
        }
    }

    public async Task<PiiDetectionResult> ScanAsync(byte[] content, string contentType, Abstractions.Security.PiiDetectionOptions? options = null)
    {
        // For binary content, we'd typically need to extract text first
        // This is a simplified implementation
        var textContent = System.Text.Encoding.UTF8.GetString(content);
        // PERFORMANCE FIX: Add ConfigureAwait(false) to prevent deadlocks in library code
        return await ScanAsync(textContent, options).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<PiiDetectionResult> ScanStreamAsync(IAsyncEnumerable<string> contentStream)
    {
        await foreach (var content in contentStream)
        {
            yield return await ScanAsync(content);
        }
    }

    public async Task<string> RedactAsync(string content, PiiDetectionResult detectionResult)
    {
        if (string.IsNullOrEmpty(content) || !detectionResult.HasPii)
        {
            return content;
        }

        var result = content;
        
        // Process detections in reverse order to maintain correct positions
        var sortedDetections = detectionResult.Detections
            .Where(d => d.Action == PiiAction.Redact)
            .OrderByDescending(d => d.StartPosition);

        foreach (var detection in sortedDetections)
        {
            var replacement = detection.Replacement ?? $"[{detection.Type.ToUpper()}]";
            result = result.Remove(detection.StartPosition, detection.EndPosition - detection.StartPosition)
                          .Insert(detection.StartPosition, replacement);
        }

        _logger.LogDebug("Redacted {Count} PII detections from content", sortedDetections.Count());
        return result;
    }

    public async Task<string> TokenizeAsync(string content, PiiDetectionResult detectionResult)
    {
        if (string.IsNullOrEmpty(content) || !detectionResult.HasPii)
        {
            return content;
        }

        var result = content;
        
        // Process detections in reverse order to maintain correct positions
        var sortedDetections = detectionResult.Detections
            .Where(d => d.Action == PiiAction.Tokenize)
            .OrderByDescending(d => d.StartPosition);

        foreach (var detection in sortedDetections)
        {
            var token = GenerateToken(detection);
            result = result.Remove(detection.StartPosition, detection.EndPosition - detection.StartPosition)
                          .Insert(detection.StartPosition, token);
        }

        _logger.LogDebug("Tokenized {Count} PII detections from content", sortedDetections.Count());
        return result;
    }

    private async Task<IEnumerable<PiiPattern>> GetEffectivePatternsAsync(Abstractions.Security.PiiDetectionOptions options)
    {
        var patterns = new List<PiiPattern>();

        // Add built-in patterns based on enabled categories
        foreach (var builtInPattern in BuiltInPatterns.Values)
        {
            if (options.EnabledCategories.Count == 0 || options.EnabledCategories.Contains(builtInPattern.Category))
            {
                patterns.Add(builtInPattern);
            }
        }

        // Add custom patterns from configuration
        foreach (var customPattern in options.CustomPatterns)
        {
            patterns.Add(customPattern);
        }

        // Add patterns from registry if available
        if (options.EnabledCategories.Count > 0)
        {
            foreach (var category in options.EnabledCategories)
            {
                var registryPatterns = await _patternRegistry.GetPatternsAsync(category);
                patterns.AddRange(registryPatterns);
            }
        }

        return patterns.Where(p => p.Enabled);
    }

    private static SecurityRiskLevel CalculateOverallRiskLevel(IEnumerable<PiiDetection> detections)
    {
        if (!detections.Any()) return SecurityRiskLevel.None;

        var highestRisk = SecurityRiskLevel.None;
        foreach (var detection in detections)
        {
            var riskLevel = detection.Category switch
            {
                PiiCategory.Government => SecurityRiskLevel.High,
                PiiCategory.Financial => SecurityRiskLevel.High,
                PiiCategory.Health => SecurityRiskLevel.High,
                PiiCategory.Biometric => SecurityRiskLevel.Critical,
                PiiCategory.PersonalIdentifier => SecurityRiskLevel.Medium,
                PiiCategory.Contact => SecurityRiskLevel.Low,
                _ => SecurityRiskLevel.Low
            };

            if (riskLevel > highestRisk)
            {
                highestRisk = riskLevel;
            }
        }

        return highestRisk;
    }

    private static Dictionary<string, object> GetDetectionContext(string content, int startPos, int length)
    {
        const int contextLength = 20;
        var beforeStart = Math.Max(0, startPos - contextLength);
        var afterEnd = Math.Min(content.Length, startPos + length + contextLength);
        
        var beforeContext = content.Substring(beforeStart, startPos - beforeStart);
        var afterContext = content.Substring(startPos + length, afterEnd - startPos - length);

        return new Dictionary<string, object>
        {
            ["BeforeContext"] = beforeContext,
            ["AfterContext"] = afterContext,
            ["Position"] = startPos,
            ["Length"] = length
        };
    }

    private string GenerateToken(PiiDetection detection)
    {
        var tokenKey = $"{detection.Type}_{detection.DetectedContent.GetHashCode()}_{Guid.NewGuid():N}";
        var token = $"TOKEN_{tokenKey[..16]}";
        
        // Store mapping for potential de-tokenization
        _tokenCache.TryAdd(token, detection.DetectedContent);
        
        return token;
    }

    private static string GenerateCacheKey(string content, Abstractions.Security.PiiDetectionOptions options)
    {
        var combinedHash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{content}_{options.MinimumConfidence}_{string.Join(",", options.EnabledCategories)}"));
        return Convert.ToBase64String(combinedHash)[..16];
    }

    private static bool ValidateCreditCard(string cardNumber)
    {
        // Implement Luhn algorithm for credit card validation
        var digits = cardNumber.Where(char.IsDigit).Select(c => c - '0').ToArray();
        
        for (int i = digits.Length - 2; i >= 0; i -= 2)
        {
            digits[i] *= 2;
            if (digits[i] > 9)
            {
                digits[i] -= 9;
            }
        }
        
        return digits.Sum() % 10 == 0;
    }
}