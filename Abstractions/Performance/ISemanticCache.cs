using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Provides semantic similarity-based caching for chat responses.
/// </summary>
public interface ISemanticCache : IResponseCache
{
    /// <summary>
    /// Gets a cached response using semantic similarity matching.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <param name="options">The request options.</param>
    /// <param name="similarityThreshold">Minimum similarity score (0-1) for cache hit. Default is 0.95.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached response if a semantically similar request is found, null otherwise.</returns>
    Task<SemanticCacheResult?> GetSemanticAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        double similarityThreshold = 0.95,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a response with semantic embedding for similarity matching.
    /// </summary>
    /// <param name="messages">The chat messages.</param>
    /// <param name="options">The request options.</param>
    /// <param name="response">The response to cache.</param>
    /// <param name="ttl">Time to live for the cached entry.</param>
    /// <param name="tags">Optional tags for categorizing cache entries.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetSemanticAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options,
        ChatResponse response,
        TimeSpan? ttl = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries based on semantic distance from a reference.
    /// </summary>
    /// <param name="referenceMessages">Reference messages for similarity comparison.</param>
    /// <param name="maxSemanticDistance">Maximum semantic distance for invalidation (0-1).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entries invalidated.</returns>
    Task<int> InvalidateBySemanticDistanceAsync(
        IEnumerable<ChatMessage> referenceMessages,
        double maxSemanticDistance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries older than the specified age.
    /// </summary>
    /// <param name="maxAge">Maximum age of cache entries to keep.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entries invalidated.</returns>
    Task<int> InvalidateByAgeAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cache entries by tag.
    /// </summary>
    /// <param name="tag">Tag to match for invalidation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entries invalidated.</returns>
    Task<int> InvalidateByTagAsync(string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics including hit rate and semantic match information.
    /// </summary>
    /// <returns>Semantic cache statistics.</returns>
    Task<SemanticCacheStatistics> GetStatisticsAsync();

    /// <summary>
    /// Finds similar cached entries for analysis or debugging.
    /// </summary>
    /// <param name="messages">Query messages.</param>
    /// <param name="limit">Maximum number of similar entries to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of similar cache entries with similarity scores.</returns>
    Task<IReadOnlyList<SimilarCacheEntry>> FindSimilarEntriesAsync(
        IEnumerable<ChatMessage> messages,
        int limit = 10,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from a semantic cache lookup.
/// </summary>
public class SemanticCacheResult
{
    /// <summary>
    /// The cached response.
    /// </summary>
    public required ChatResponse Response { get; init; }

    /// <summary>
    /// Similarity score (0-1) between the query and cached entry.
    /// </summary>
    public double SimilarityScore { get; init; }

    /// <summary>
    /// Whether this was an exact match or semantic match.
    /// </summary>
    public bool IsExactMatch { get; init; }

    /// <summary>
    /// Age of the cached entry.
    /// </summary>
    public TimeSpan Age { get; init; }

    /// <summary>
    /// Original messages that were cached.
    /// </summary>
    public IEnumerable<ChatMessage>? OriginalMessages { get; init; }

    /// <summary>
    /// Cache entry identifier.
    /// </summary>
    public string CacheKey { get; init; } = string.Empty;
}

/// <summary>
/// Statistics for semantic cache performance.
/// </summary>
public class SemanticCacheStatistics
{
    /// <summary>
    /// Total number of cache lookups.
    /// </summary>
    public long TotalLookups { get; init; }

    /// <summary>
    /// Number of exact cache hits.
    /// </summary>
    public long ExactHits { get; init; }

    /// <summary>
    /// Number of semantic cache hits.
    /// </summary>
    public long SemanticHits { get; init; }

    /// <summary>
    /// Number of cache misses.
    /// </summary>
    public long Misses { get; init; }

    /// <summary>
    /// Total hit rate (exact + semantic).
    /// </summary>
    public double HitRate => TotalLookups > 0 ? (double)(ExactHits + SemanticHits) / TotalLookups : 0;

    /// <summary>
    /// Semantic hit rate.
    /// </summary>
    public double SemanticHitRate => TotalLookups > 0 ? (double)SemanticHits / TotalLookups : 0;

    /// <summary>
    /// Total number of cached entries.
    /// </summary>
    public int TotalEntries { get; init; }

    /// <summary>
    /// Average similarity score for semantic hits.
    /// </summary>
    public double AverageSimilarityScore { get; init; }

    /// <summary>
    /// Average age of cache entries.
    /// </summary>
    public TimeSpan AverageEntryAge { get; init; }

    /// <summary>
    /// Total cache size in bytes.
    /// </summary>
    public long CacheSizeBytes { get; init; }

    /// <summary>
    /// Number of entries invalidated by time.
    /// </summary>
    public long TimeInvalidations { get; init; }

    /// <summary>
    /// Number of entries invalidated by semantic distance.
    /// </summary>
    public long SemanticInvalidations { get; init; }
}

/// <summary>
/// Represents a similar cache entry found during search.
/// </summary>
public class SimilarCacheEntry
{
    /// <summary>
    /// Cache entry identifier.
    /// </summary>
    public required string CacheKey { get; init; }

    /// <summary>
    /// Original messages.
    /// </summary>
    public required IEnumerable<ChatMessage> Messages { get; init; }

    /// <summary>
    /// Cached response.
    /// </summary>
    public required ChatResponse Response { get; init; }

    /// <summary>
    /// Similarity score to the query.
    /// </summary>
    public double SimilarityScore { get; init; }

    /// <summary>
    /// When the entry was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Number of times this entry has been retrieved.
    /// </summary>
    public int HitCount { get; init; }

    /// <summary>
    /// Tags associated with this entry.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Configuration options for semantic caching.
/// </summary>
public class SemanticCacheOptions
{
    /// <summary>
    /// Default similarity threshold for cache hits. Default is 0.95.
    /// </summary>
    public double DefaultSimilarityThreshold { get; set; } = 0.95;

    /// <summary>
    /// Default TTL for cache entries. Default is 1 hour.
    /// </summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Maximum number of cache entries. Default is 1000.
    /// </summary>
    public int MaxEntries { get; set; } = 1000;

    /// <summary>
    /// Whether to enable automatic cleanup of expired entries. Default is true.
    /// </summary>
    public bool EnableAutomaticCleanup { get; set; } = true;

    /// <summary>
    /// Interval for automatic cleanup. Default is 5 minutes.
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Maximum age for cache entries before automatic invalidation. Default is 24 hours.
    /// </summary>
    public TimeSpan MaxEntryAge { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Whether to track detailed statistics. Default is true.
    /// </summary>
    public bool TrackStatistics { get; set; } = true;

    /// <summary>
    /// Eviction strategy when max entries is reached.
    /// </summary>
    public CacheEvictionStrategy EvictionStrategy { get; set; } = CacheEvictionStrategy.LeastRecentlyUsed;
}

/// <summary>
/// Strategy for evicting cache entries when capacity is reached.
/// </summary>
public enum CacheEvictionStrategy
{
    /// <summary>
    /// Remove least recently used entries.
    /// </summary>
    LeastRecentlyUsed,

    /// <summary>
    /// Remove least frequently used entries.
    /// </summary>
    LeastFrequentlyUsed,

    /// <summary>
    /// Remove oldest entries first.
    /// </summary>
    OldestFirst,

    /// <summary>
    /// Remove entries with lowest similarity scores.
    /// </summary>
    LowestSimilarity
}
