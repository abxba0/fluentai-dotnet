namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a request to index a document into the knowledge base.
/// </summary>
public class DocumentIndexRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for this document.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document input containing content and metadata.
    /// </summary>
    public DocumentInput Document { get; set; } = new();

    /// <summary>
    /// Gets or sets the knowledge base identifier where the document should be indexed.
    /// </summary>
    public string? KnowledgeBase { get; set; }

    /// <summary>
    /// Gets or sets processing options for the document.
    /// </summary>
    public ProcessingOptions? ProcessingOptions { get; set; }

    /// <summary>
    /// Gets or sets chunking options for breaking the document into segments.
    /// </summary>
    public ChunkingOptions? ChunkingOptions { get; set; }

    /// <summary>
    /// Gets or sets embedding options for vector generation.
    /// </summary>
    public EmbeddingRequestOptions? EmbeddingOptions { get; set; }

    /// <summary>
    /// Gets or sets security settings for document access control.
    /// </summary>
    public DocumentSecurity? Security { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the indexing operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}