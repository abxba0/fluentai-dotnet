using System.ComponentModel.DataAnnotations;

namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for OpenAI provider.
/// </summary>
public class OpenAiOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    [Required(ErrorMessage = "OpenAI API key is required.")]
    [MinLength(10, ErrorMessage = "API key must be at least 10 characters long.")]
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the model to use (e.g., "gpt-4", "gpt-3.5-turbo").
    /// </summary>
    [Required(ErrorMessage = "Model name is required.")]
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
    [Range(typeof(TimeSpan), "00:00:10", "00:10:00", ErrorMessage = "Request timeout must be between 10 seconds and 10 minutes.")]
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);
    
    /// <summary>
    /// Gets or sets the maximum number of retries for failed requests.
    /// </summary>
    [Range(0, 10, ErrorMessage = "Maximum retries must be between 0 and 10.")]
    public int MaxRetries { get; set; } = 2;
    
    /// <summary>
    /// Gets or sets the maximum request size in characters.
    /// </summary>
    [Range(100, 1_000_000, ErrorMessage = "Maximum request size must be between 100 and 1,000,000 characters.")]
    public long MaxRequestSize { get; set; } = 80_000;
    
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// </summary>
    [Range(1, 32000, ErrorMessage = "Maximum tokens must be between 1 and 32,000.")]
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the rate limit permit count per window. If null, rate limiting is disabled.
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Permit limit must be between 1 and 10,000.")]
    public int? PermitLimit { get; set; }
    
    /// <summary>
    /// Gets or sets the rate limit window duration in seconds. If null, rate limiting is disabled.
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Window in seconds must be between 1 and 3,600.")]
    public int? WindowInSeconds { get; set; }
}