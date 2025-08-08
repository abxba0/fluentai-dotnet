using FluentAI.Abstractions.Models;

namespace FluentAI.Configuration;

/// <summary>
/// Anthropic-specific request options.
/// </summary>
public record AnthropicRequestOptions : ChatRequestOptions
{
    /// <summary>
    /// Gets or sets the temperature for response generation (0.0 to 1.0).
    /// </summary>
    public float? Temperature { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the system prompt for the conversation.
    /// </summary>
    public string? SystemPrompt { get; set; }
}