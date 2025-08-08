using FluentAI.Abstractions.Models;

namespace FluentAI.Configuration;

/// <summary>
/// OpenAI-specific request options.
/// </summary>
public record OpenAiRequestOptions : ChatRequestOptions
{
    /// <summary>
    /// Gets or sets the temperature for response generation (0.0 to 2.0).
    /// </summary>
    public float? Temperature { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the nucleus sampling parameter (0.0 to 1.0).
    /// </summary>
    public float? TopP { get; set; }
}