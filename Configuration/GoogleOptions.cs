using System.ComponentModel.DataAnnotations;

namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for Google Gemini provider.
/// </summary>
public class GoogleOptions
{
    /// <summary>
    /// Gets or sets the Google AI API key.
    /// </summary>
    [Required(ErrorMessage = "Google API key is required")]
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the model to use (e.g., "gemini-1.5-pro-latest").
    /// </summary>
    [Required(ErrorMessage = "Google model name is required")]
    public string Model { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);
    
    /// <summary>
    /// Gets or sets the maximum number of retries for failed requests.
    /// </summary>
    [Range(0, 10, ErrorMessage = "MaxRetries must be between 0 and 10")]
    public int MaxRetries { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the maximum request size in characters.
    /// </summary>
    [Range(1, 1_000_000, ErrorMessage = "MaxRequestSize must be between 1 and 1,000,000")]
    public long MaxRequestSize { get; set; } = 80_000;

    // CONSISTENCY FIX: Add rate limiting properties like other providers
    /// <summary>
    /// Gets or sets the rate limiting permit limit (requests per window).
    /// If null, rate limiting is disabled.
    /// </summary>
    [Range(1, 10000, ErrorMessage = "PermitLimit must be between 1 and 10,000")]
    public int? PermitLimit { get; set; }

    /// <summary>
    /// Gets or sets the rate limiting window in seconds.
    /// If null, rate limiting is disabled.
    /// </summary>
    [Range(1, 3600, ErrorMessage = "WindowInSeconds must be between 1 and 3,600")]
    public int? WindowInSeconds { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens for responses.
    /// </summary>
    [Range(1, 100000, ErrorMessage = "MaxTokens must be between 1 and 100,000")]
    public int? MaxTokens { get; set; }
}