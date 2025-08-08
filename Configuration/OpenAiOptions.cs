namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for OpenAI provider.
/// </summary>
public class OpenAiOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the model to use (e.g., "gpt-4", "gpt-3.5-turbo").
    /// </summary>
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether to use Azure OpenAI instead of OpenAI.
    /// </summary>
    public bool IsAzureOpenAI { get; set; } = false;
    
    /// <summary>
    /// Gets or sets the Azure OpenAI endpoint (required when IsAzureOpenAI is true).
    /// </summary>
    public string? Endpoint { get; set; }
    
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