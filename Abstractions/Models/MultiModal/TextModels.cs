namespace FluentAI.Abstractions.Models;

/// <summary>
/// Request for text generation operations.
/// </summary>
public class TextRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the text prompt for generation.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the temperature for randomness control (0.0 to 2.0).
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the messages for conversational context.
    /// </summary>
    public IEnumerable<ChatMessage>? Messages { get; set; }

    /// <summary>
    /// Gets or sets the system message for context setting.
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Gets or sets the top-p value for nucleus sampling.
    /// </summary>
    public float? TopP { get; set; }
}

/// <summary>
/// Response from text generation operations.
/// </summary>
public class TextResponse : MultiModalResponse
{
    /// <summary>
    /// Gets or sets the generated text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the reason why generation finished.
    /// </summary>
    public string FinishReason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score of the generation (0.0 to 1.0).
    /// </summary>
    public float? ConfidenceScore { get; set; }
}