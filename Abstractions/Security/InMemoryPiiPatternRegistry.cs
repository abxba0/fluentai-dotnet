using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace FluentAI.Abstractions.Security;

/// <summary>
/// In-memory implementation of PII pattern registry for development and testing.
/// In production, this would typically be backed by a database or configuration store.
/// </summary>
public class InMemoryPiiPatternRegistry : IPiiPatternRegistry
{
    private readonly ILogger<InMemoryPiiPatternRegistry> _logger;
    private readonly ConcurrentDictionary<string, PiiPattern> _patterns;
    private readonly ConcurrentDictionary<PiiCategory, List<string>> _categoryIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryPiiPatternRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryPiiPatternRegistry(ILogger<InMemoryPiiPatternRegistry> logger)
    {
        _logger = logger;
        _patterns = new ConcurrentDictionary<string, PiiPattern>();
        _categoryIndex = new ConcurrentDictionary<PiiCategory, List<string>>();
        
        // Initialize with some default patterns
        InitializeDefaultPatterns();
    }

    /// <inheritdoc />
    public Task RegisterPatternAsync(PiiPattern pattern)
    {
        if (string.IsNullOrEmpty(pattern.Name))
        {
            throw new ArgumentException("Pattern name cannot be null or empty", nameof(pattern));
        }

        if (string.IsNullOrEmpty(pattern.Pattern))
        {
            throw new ArgumentException("Pattern regex cannot be null or empty", nameof(pattern));
        }

        try
        {
            // Validate the regex pattern
            var compiledPattern = new Regex(pattern.Pattern, RegexOptions.Compiled);
            
            // Create pattern with compiled regex
            var patternWithCompiled = pattern with { CompiledPattern = compiledPattern };
            
            _patterns.AddOrUpdate(pattern.Name, patternWithCompiled, (key, existing) => patternWithCompiled);

            // Update category index
            _categoryIndex.AddOrUpdate(
                pattern.Category,
                new List<string> { pattern.Name },
                (category, existing) =>
                {
                    if (!existing.Contains(pattern.Name))
                    {
                        existing.Add(pattern.Name);
                    }
                    return existing;
                });

            _logger.LogInformation("Registered PII pattern: {PatternName} for category {Category}", 
                pattern.Name, pattern.Category);

            return Task.CompletedTask;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid regex pattern for {PatternName}: {Pattern}", 
                pattern.Name, pattern.Pattern);
            throw;
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PiiPattern>> GetPatternsAsync(PiiCategory category)
    {
        if (_categoryIndex.TryGetValue(category, out var patternNames))
        {
            var patterns = patternNames
                .Select(name => _patterns.TryGetValue(name, out var pattern) ? pattern : null)
                .Where(p => p != null)
                .Cast<PiiPattern>()
                .ToList();

            _logger.LogDebug("Retrieved {Count} patterns for category {Category}", 
                patterns.Count, category);

            return Task.FromResult<IEnumerable<PiiPattern>>(patterns);
        }

        return Task.FromResult<IEnumerable<PiiPattern>>(Array.Empty<PiiPattern>());
    }

    /// <inheritdoc />
    public Task<PiiPattern?> GetPatternAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Task.FromResult<PiiPattern?>(null);
        }

        var pattern = _patterns.TryGetValue(name, out var foundPattern) ? foundPattern : null;
        
        if (pattern != null)
        {
            _logger.LogDebug("Retrieved pattern: {PatternName}", name);
        }
        else
        {
            _logger.LogDebug("Pattern not found: {PatternName}", name);
        }

        return Task.FromResult(pattern);
    }

    /// <inheritdoc />
    public Task UpdatePatternAsync(PiiPattern pattern)
    {
        if (string.IsNullOrEmpty(pattern.Name))
        {
            throw new ArgumentException("Pattern name cannot be null or empty", nameof(pattern));
        }

        if (!_patterns.ContainsKey(pattern.Name))
        {
            throw new InvalidOperationException($"Pattern '{pattern.Name}' does not exist. Use RegisterPatternAsync to add new patterns.");
        }

        return RegisterPatternAsync(pattern); // RegisterPatternAsync handles updates via AddOrUpdate
    }

    /// <inheritdoc />
    public Task RemovePatternAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Pattern name cannot be null or empty", nameof(name));
        }

        if (_patterns.TryRemove(name, out var removedPattern))
        {
            // Update category index
            if (_categoryIndex.TryGetValue(removedPattern.Category, out var patternNames))
            {
                patternNames.Remove(name);
                if (patternNames.Count == 0)
                {
                    _categoryIndex.TryRemove(removedPattern.Category, out _);
                }
            }

            _logger.LogInformation("Removed PII pattern: {PatternName}", name);
        }
        else
        {
            _logger.LogWarning("Attempted to remove non-existent pattern: {PatternName}", name);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all registered patterns.
    /// </summary>
    /// <returns>All patterns in the registry.</returns>
    public Task<IEnumerable<PiiPattern>> GetAllPatternsAsync()
    {
        var allPatterns = _patterns.Values.ToList();
        _logger.LogDebug("Retrieved all {Count} patterns from registry", allPatterns.Count);
        return Task.FromResult<IEnumerable<PiiPattern>>(allPatterns);
    }

    /// <summary>
    /// Gets statistics about the pattern registry.
    /// </summary>
    /// <returns>Registry statistics.</returns>
    public Task<PatternRegistryStatistics> GetStatisticsAsync()
    {
        var stats = new PatternRegistryStatistics
        {
            TotalPatterns = _patterns.Count,
            PatternsByCategory = _categoryIndex.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value.Count),
            EnabledPatterns = _patterns.Values.Count(p => p.Enabled),
            DisabledPatterns = _patterns.Values.Count(p => !p.Enabled)
        };

        return Task.FromResult(stats);
    }

    private void InitializeDefaultPatterns()
    {
        var defaultPatterns = new[]
        {
            new PiiPattern
            {
                Name = "IPv4Address",
                Category = PiiCategory.Contact,
                Pattern = @"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b",
                CompiledPattern = new Regex(@"\b(?:[0-9]{1,3}\.){3}[0-9]{1,3}\b", RegexOptions.Compiled),
                Confidence = 0.8,
                DefaultAction = PiiAction.Log,
                Enabled = true
            },
            new PiiPattern
            {
                Name = "IPv6Address",
                Category = PiiCategory.Contact,
                Pattern = @"\b(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\b",
                CompiledPattern = new Regex(@"\b(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}\b", RegexOptions.Compiled),
                Confidence = 0.8,
                DefaultAction = PiiAction.Log,
                Enabled = true
            },
            new PiiPattern
            {
                Name = "MacAddress",
                Category = PiiCategory.Contact,
                Pattern = @"\b([0-9a-fA-F]{2}[:-]){5}[0-9a-fA-F]{2}\b",
                CompiledPattern = new Regex(@"\b([0-9a-fA-F]{2}[:-]){5}[0-9a-fA-F]{2}\b", RegexOptions.Compiled),
                Confidence = 0.9,
                DefaultAction = PiiAction.Redact,
                DefaultReplacement = "[MAC_ADDRESS]",
                Enabled = true
            },
            new PiiPattern
            {
                Name = "BankAccountNumber",
                Category = PiiCategory.Financial,
                Pattern = @"\b\d{8,17}\b",
                CompiledPattern = new Regex(@"\b\d{8,17}\b", RegexOptions.Compiled),
                Confidence = 0.6, // Lower confidence as this is a generic pattern
                DefaultAction = PiiAction.Block,
                Enabled = false // Disabled by default due to high false positive rate
            },
            new PiiPattern
            {
                Name = "DriversLicense",
                Category = PiiCategory.Government,
                Pattern = @"\b[A-Z]{1,2}[0-9]{6,8}\b",
                CompiledPattern = new Regex(@"\b[A-Z]{1,2}[0-9]{6,8}\b", RegexOptions.Compiled),
                Confidence = 0.7,
                DefaultAction = PiiAction.Redact,
                DefaultReplacement = "[DRIVER_LICENSE]",
                SupportedRegions = new[] { "US" },
                Enabled = true
            }
        };

        foreach (var pattern in defaultPatterns)
        {
            RegisterPatternAsync(pattern).Wait();
        }

        _logger.LogInformation("Initialized PII pattern registry with {Count} default patterns", defaultPatterns.Length);
    }
}

/// <summary>
/// Statistics about the pattern registry.
/// </summary>
public record PatternRegistryStatistics
{
    /// <summary>
    /// Gets the total number of patterns in the registry.
    /// </summary>
    public int TotalPatterns { get; init; }

    /// <summary>
    /// Gets the number of patterns by category.
    /// </summary>
    public Dictionary<string, int> PatternsByCategory { get; init; } = new();

    /// <summary>
    /// Gets the number of enabled patterns.
    /// </summary>
    public int EnabledPatterns { get; init; }

    /// <summary>
    /// Gets the number of disabled patterns.
    /// </summary>
    public int DisabledPatterns { get; init; }
}