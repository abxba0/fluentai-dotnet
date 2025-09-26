namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a chunk of a document that has been processed for RAG indexing.
/// </summary>
public class DocumentChunk
{
    /// <summary>
    /// Gets or sets the unique identifier for this chunk.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the parent document.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the text content of this chunk.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title or heading of this chunk.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the embedding vector for this chunk.
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Gets or sets the chunk index within the parent document.
    /// </summary>
    public int ChunkIndex { get; set; }

    /// <summary>
    /// Gets or sets the character start position in the original document.
    /// </summary>
    public int StartPosition { get; set; }

    /// <summary>
    /// Gets or sets the character end position in the original document.
    /// </summary>
    public int EndPosition { get; set; }

    /// <summary>
    /// Gets or sets the length of the chunk in characters.
    /// </summary>
    public int Length => Content.Length;

    /// <summary>
    /// Gets or sets the confidence score for this chunk's relevance (0.0 to 1.0).
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets metadata inherited from the parent document.
    /// </summary>
    public Dictionary<string, object> DocumentMetadata { get; set; } = new();

    /// <summary>
    /// Gets or sets chunk-specific metadata.
    /// </summary>
    public Dictionary<string, object> ChunkMetadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when this chunk was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the source information for citations.
    /// </summary>
    public SourceInfo? Source { get; set; }
}