namespace FluentAI.Abstractions.Memory;

/// <summary>
/// Provides short-term and long-term memory storage for AI conversations.
/// </summary>
public interface IMemoryStore
{
    /// <summary>
    /// Stores a memory item.
    /// </summary>
    /// <param name="memory">Memory to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreMemoryAsync(Memory memory, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves memories relevant to the given query.
    /// </summary>
    /// <param name="query">Query text to find relevant memories.</param>
    /// <param name="limit">Maximum number of memories to retrieve.</param>
    /// <param name="memoryType">Type of memory to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of relevant memories.</returns>
    Task<IReadOnlyList<Memory>> RetrieveMemoriesAsync(
        string query,
        int limit = 10,
        MemoryType? memoryType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing memory.
    /// </summary>
    /// <param name="memoryId">Memory identifier.</param>
    /// <param name="content">Updated content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateMemoryAsync(string memoryId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a memory.
    /// </summary>
    /// <param name="memoryId">Memory identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteMemoryAsync(string memoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all short-term memories.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearShortTermMemoryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Consolidates short-term memories into long-term storage.
    /// </summary>
    /// <param name="criteria">Criteria for selecting memories to consolidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of memories consolidated.</returns>
    Task<int> ConsolidateMemoriesAsync(
        ConsolidationCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets memory statistics.
    /// </summary>
    /// <returns>Current memory statistics.</returns>
    Task<MemoryStatistics> GetStatisticsAsync();
}

/// <summary>
/// Represents a single memory item.
/// </summary>
public class Memory
{
    /// <summary>
    /// Unique memory identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Memory content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Type of memory.
    /// </summary>
    public MemoryType Type { get; init; }

    /// <summary>
    /// When the memory was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the memory was last accessed.
    /// </summary>
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Number of times the memory has been accessed.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Importance score (0-1).
    /// </summary>
    public double Importance { get; set; } = 0.5;

    /// <summary>
    /// Associated conversation identifier.
    /// </summary>
    public string? ConversationId { get; init; }

    /// <summary>
    /// Custom metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Vector embedding of the memory content.
    /// </summary>
    public float[]? Embedding { get; set; }
}

/// <summary>
/// Type of memory storage.
/// </summary>
public enum MemoryType
{
    /// <summary>
    /// Short-term working memory (recent conversation context).
    /// </summary>
    ShortTerm,

    /// <summary>
    /// Long-term episodic memory (important past events).
    /// </summary>
    LongTerm,

    /// <summary>
    /// Semantic knowledge (general facts and knowledge).
    /// </summary>
    Semantic,

    /// <summary>
    /// Procedural memory (how to do things).
    /// </summary>
    Procedural
}

/// <summary>
/// Criteria for consolidating memories.
/// </summary>
public class ConsolidationCriteria
{
    /// <summary>
    /// Minimum importance score for consolidation.
    /// </summary>
    public double MinImportance { get; set; } = 0.7;

    /// <summary>
    /// Minimum access count for consolidation.
    /// </summary>
    public int MinAccessCount { get; set; } = 3;

    /// <summary>
    /// Maximum age of short-term memories to consider (in hours).
    /// </summary>
    public int MaxAgeHours { get; set; } = 24;

    /// <summary>
    /// Whether to use semantic similarity for grouping related memories.
    /// </summary>
    public bool UseSemanticGrouping { get; set; } = true;
}

/// <summary>
/// Statistics about the memory store.
/// </summary>
public class MemoryStatistics
{
    /// <summary>
    /// Total number of memories.
    /// </summary>
    public int TotalMemories { get; init; }

    /// <summary>
    /// Number of short-term memories.
    /// </summary>
    public int ShortTermCount { get; init; }

    /// <summary>
    /// Number of long-term memories.
    /// </summary>
    public int LongTermCount { get; init; }

    /// <summary>
    /// Number of semantic memories.
    /// </summary>
    public int SemanticCount { get; init; }

    /// <summary>
    /// Average importance score.
    /// </summary>
    public double AverageImportance { get; init; }

    /// <summary>
    /// Total storage size in bytes.
    /// </summary>
    public long StorageSizeBytes { get; init; }
}
