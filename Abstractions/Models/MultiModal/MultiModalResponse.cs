namespace FluentAI.Abstractions.Models;

/// <summary>
/// Base class for all multi-modal AI responses.
/// </summary>
public abstract class MultiModalResponse
{
    /// <summary>
    /// Gets or sets the model that was used to generate this response.
    /// </summary>
    public string ModelUsed { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the provider that generated this response.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets token usage information for this request.
    /// </summary>
    public TokenUsage? TokenUsage { get; set; }

    /// <summary>
    /// Gets or sets the time spent processing this request.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the response.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when this response was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; } = DateTimeOffset.UtcNow;
}