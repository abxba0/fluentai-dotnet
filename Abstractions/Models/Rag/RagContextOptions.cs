namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Configuration options for RAG context injection in chat models.
/// </summary>
public class RagContextOptions
{
    /// <summary>
    /// Gets or sets the knowledge base identifier to search within.
    /// </summary>
    public string? KnowledgeBase { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of context chunks to include.
    /// Default is 5.
    /// </summary>
    public int MaxContextChunks { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum similarity threshold for context inclusion (0.0 to 1.0).
    /// Default is 0.7.
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets whether to enable automatic context injection.
    /// When true, context is automatically retrieved and injected based on the conversation.
    /// Default is true.
    /// </summary>
    public bool AutoContextInjection { get; set; } = true;

    /// <summary>
    /// Gets or sets the context injection strategy.
    /// </summary>
    public ContextInjectionStrategy InjectionStrategy { get; set; } = ContextInjectionStrategy.SystemMessage;

    /// <summary>
    /// Gets or sets custom context filters to apply during retrieval.
    /// </summary>
    public Dictionary<string, object> ContextFilters { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to include source citations in the response.
    /// Default is true.
    /// </summary>
    public bool IncludeCitations { get; set; } = true;

    /// <summary>
    /// Gets or sets the template for formatting retrieved context.
    /// </summary>
    public string? ContextTemplate { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the RAG operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Defines how retrieved context is injected into the conversation.
/// </summary>
public enum ContextInjectionStrategy
{
    /// <summary>
    /// Inject context as a system message at the beginning of the conversation.
    /// </summary>
    SystemMessage,

    /// <summary>
    /// Append context to the user's last message.
    /// </summary>
    AppendToUserMessage,

    /// <summary>
    /// Insert context as a separate user message before the last message.
    /// </summary>
    SeparateContextMessage,

    /// <summary>
    /// Let the implementation decide the best injection strategy.
    /// </summary>
    Automatic
}