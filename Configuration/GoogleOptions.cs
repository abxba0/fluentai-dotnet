namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for Google Gemini provider.
/// </summary>
public class GoogleOptions
{
    /// <summary>
    /// Gets or sets the Google AI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the model to use (e.g., "gemini-1.5-pro-latest").
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
}