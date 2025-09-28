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
    
    /// <summary>
    /// SECURITY FIX: Validates failover configuration to prevent circular dependencies and invalid configurations.
    /// </summary>
    public void Validate()
    {
        // Prevent circular dependencies
        if (!string.IsNullOrEmpty(PrimaryProvider) && 
            !string.IsNullOrEmpty(FallbackProvider) && 
            string.Equals(PrimaryProvider, FallbackProvider, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Primary and fallback providers cannot be the same to prevent circular dependencies");
        }
        
        // Validate provider names are not empty or whitespace
        if (PrimaryProvider is not null && string.IsNullOrWhiteSpace(PrimaryProvider))
        {
            throw new ArgumentException("Primary provider name cannot be empty or whitespace");
        }
        
        if (FallbackProvider is not null && string.IsNullOrWhiteSpace(FallbackProvider))
        {
            throw new ArgumentException("Fallback provider name cannot be empty or whitespace");
        }
    }
}