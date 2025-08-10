using FluentAI.Abstractions.Models;

namespace FluentAI.Configuration;

/// <summary>
/// Hugging Face-specific request options.
/// </summary>
public record HuggingFaceRequestOptions : ChatRequestOptions
{
    /// <summary>
    /// Gets or sets the temperature for response generation (0.0 to 2.0).
    /// </summary>
    public float? Temperature { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of new tokens to generate.
    /// </summary>
    public int? MaxNewTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the nucleus sampling parameter (0.0 to 1.0).
    /// </summary>
    public float? TopP { get; set; }
    
    /// <summary>
    /// Gets or sets the top-k sampling parameter.
    /// </summary>
    public int? TopK { get; set; }
}