namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a response from RAG (Retrieval Augmented Generation) processing.
/// </summary>
public class RagResponse
{
    /// <summary>
    /// Gets or sets the generated response content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the retrieved context chunks that were used for generation.
    /// </summary>
    public IEnumerable<DocumentChunk> RetrievedContext { get; set; } = Enumerable.Empty<DocumentChunk>();

    /// <summary>
    /// Gets or sets the confidence score for the response (0.0 to 1.0).
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets the model that generated the response.
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider that handled the request.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets token usage information.
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Gets or sets the time taken to process the request.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets sources and citations used in the response.
    /// </summary>
    public IEnumerable<Citation> Citations { get; set; } = Enumerable.Empty<Citation>();

    /// <summary>
    /// Gets or sets additional metadata about the response.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the response was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}