namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for Hugging Face provider.
/// </summary>
public class HuggingFaceOptions
{
    /// <summary>
    /// Gets or sets the Hugging Face API token.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the full Inference Endpoint URL.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;
    
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
}