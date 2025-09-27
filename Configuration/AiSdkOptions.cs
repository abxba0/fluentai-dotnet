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

    /// <summary>
    /// Gets or sets the RAG (Retrieval Augmented Generation) configuration options.
    /// </summary>
    public RagOptions? Rag { get; set; }

    /// <summary>
    /// Gets or sets the security configuration options.
    /// </summary>
    public SecurityOptions? Security { get; set; }
}

/// <summary>
/// Security configuration options.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Gets or sets the PII detection configuration.
    /// </summary>
    public PiiDetectionOptions? PiiDetection { get; set; }

    /// <summary>
    /// Gets or sets whether input sanitization is enabled.
    /// </summary>
    public bool EnableInputSanitization { get; set; } = true;

    /// <summary>
    /// Gets or sets whether content filtering is enabled.
    /// </summary>
    public bool EnableContentFiltering { get; set; } = true;

    /// <summary>
    /// Gets or sets custom security policy rules.
    /// </summary>
    public Dictionary<string, object> CustomPolicyRules { get; set; } = new();
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