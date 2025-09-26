namespace FluentAI.Abstractions.Models;

/// <summary>
/// Base class for all multi-modal AI requests.
/// </summary>
public abstract class MultiModalRequest
{
    /// <summary>
    /// Gets or sets the model name override for this specific request.
    /// If not specified, the default model from configuration will be used.
    /// </summary>
    public string? ModelOverride { get; set; }

    /// <summary>
    /// Gets or sets additional properties that may be provider-specific.
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();

    /// <summary>
    /// Gets or sets security options for content filtering and validation.
    /// </summary>
    public SecurityOptions? Security { get; set; }
}

/// <summary>
/// Security options for multi-modal requests.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Gets or sets whether to enable content filtering.
    /// </summary>
    public bool EnableContentFiltering { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable PII detection and filtering.
    /// </summary>
    public bool EnablePiiDetection { get; set; } = true;

    /// <summary>
    /// Gets or sets custom content policy rules.
    /// </summary>
    public IEnumerable<string>? CustomPolicyRules { get; set; }
}