namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for Anthropic provider.
/// </summary>
public class AnthropicOptions
{
    /// <summary>
    /// Gets or sets the Anthropic API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the model to use (e.g., "claude-3-sonnet-20240229").
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);
    
    /// <summary>
    /// Gets or sets the maximum number of retries for failed requests.
    /// </summary>
    public int MaxRetries { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the maximum request size in characters.
    /// </summary>
    public long MaxRequestSize { get; set; } = 80_000;
    
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    public int? MaxTokens { get; set; }
}