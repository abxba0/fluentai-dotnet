namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a request for RAG (Retrieval Augmented Generation) processing.
/// </summary>
public class RagRequest
{
    /// <summary>
    /// Gets or sets the user's query or question.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conversation context messages.
    /// </summary>
    public IEnumerable<ChatMessage> Messages { get; set; } = Enumerable.Empty<ChatMessage>();

    /// <summary>
    /// Gets or sets the knowledge base identifier to search within.
    /// </summary>
    public string? KnowledgeBase { get; set; }

    /// <summary>
    /// Gets or sets retrieval options for context discovery.
    /// </summary>
    public RetrievalOptions? RetrievalOptions { get; set; }

    /// <summary>
    /// Gets or sets generation options for response creation.
    /// </summary>
    public ChatRequestOptions? GenerationOptions { get; set; }

    /// <summary>
    /// Gets or sets user-specific context or session information.
    /// </summary>
    public Dictionary<string, object> UserContext { get; set; } = new();

    /// <summary>
    /// Gets or sets additional metadata for the request.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}