namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents the result of an embedding generation operation.
/// </summary>
public class EmbeddingResult
{
    /// <summary>
    /// Gets or sets the generated embeddings.
    /// </summary>
    public IEnumerable<Embedding> Embeddings { get; set; } = Enumerable.Empty<Embedding>();

    /// <summary>
    /// Gets or sets the model used for embedding generation.
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider that generated the embeddings.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets token usage information for the embedding operation.
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Gets or sets the time taken to generate the embeddings.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the embedding operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a single embedding vector with its associated text.
/// </summary>
public class Embedding
{
    /// <summary>
    /// Gets or sets the input text that was embedded.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the embedding vector values.
    /// </summary>
    public float[] Vector { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the index of this embedding in the batch request.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for this embedding.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}