namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for the FluentAI SDK.
/// </summary>
public class AiSdkOptions
{
    /// <summary>
    /// Gets or sets the default AI provider to use.
    /// </summary>
    public string? DefaultProvider { get; set; }
    
    /// <summary>
    /// Gets or sets the failover configuration.
    /// </summary>
    public FailoverOptions? Failover { get; set; }

    /// <summary>
    /// Gets or sets the multi-modal configuration options.
    /// </summary>
    public MultiModalOptions? MultiModal { get; set; }
}

/// <summary>
/// Configuration options for failover strategy.
/// </summary>
public class FailoverOptions
{
    /// <summary>
    /// Gets or sets the primary provider name.
    /// </summary>
    public string? PrimaryProvider { get; set; }
    
    /// <summary>
    /// Gets or sets the fallback provider name.
    /// </summary>
    public string? FallbackProvider { get; set; }
}