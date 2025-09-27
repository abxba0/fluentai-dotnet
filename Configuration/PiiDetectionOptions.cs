using FluentAI.Abstractions.Security;

namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for PII detection functionality.
/// </summary>
public class PiiDetectionOptions
{
    /// <summary>
    /// Gets or sets whether PII detection is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the PII detection provider.
    /// </summary>
    public string Provider { get; set; } = "Hybrid";

    /// <summary>
    /// Gets or sets the processing mode for PII detection.
    /// </summary>
    public string ProcessingMode { get; set; } = "Streaming";

    /// <summary>
    /// Gets or sets the detection type configurations.
    /// </summary>
    public PiiDetectionTypesOptions DetectionTypes { get; set; } = new();

    /// <summary>
    /// Gets or sets the compliance profile configurations.
    /// </summary>
    public Dictionary<string, ComplianceProfileOptions> ComplianceProfiles { get; set; } = new();

    /// <summary>
    /// Gets or sets performance configuration options.
    /// </summary>
    public PiiPerformanceOptions Performance { get; set; } = new();
}

/// <summary>
/// Configuration for specific PII detection types.
/// </summary>
public class PiiDetectionTypesOptions
{
    /// <summary>
    /// Gets or sets credit card detection options.
    /// </summary>
    public PiiTypeOptions CreditCard { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.9,
        Action = "Redact",
        Replacement = "[CREDIT_CARD]"
    };

    /// <summary>
    /// Gets or sets SSN detection options.
    /// </summary>
    public PiiTypeOptions SSN { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.95,
        Action = "Block",
        Regions = new[] { "US" }
    };

    /// <summary>
    /// Gets or sets email detection options.
    /// </summary>
    public PiiTypeOptions Email { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.8,
        Action = "Tokenize",
        PreserveDomain = true
    };

    /// <summary>
    /// Gets or sets person name detection options.
    /// </summary>
    public PiiTypeOptions PersonName { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.7,
        Action = "Mask",
        PartialMask = true
    };

    /// <summary>
    /// Gets or sets phone number detection options.
    /// </summary>
    public PiiTypeOptions Phone { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.85,
        Action = "Mask"
    };

    /// <summary>
    /// Gets or sets address detection options.
    /// </summary>
    public PiiTypeOptions Address { get; set; } = new()
    {
        Enabled = true,
        Confidence = 0.75,
        Action = "Redact",
        Replacement = "[ADDRESS]"
    };

    /// <summary>
    /// Gets or sets custom PII detection patterns.
    /// </summary>
    public List<CustomPiiTypeOptions> Custom { get; set; } = new();
}

/// <summary>
/// Configuration options for a specific PII type.
/// </summary>
public class PiiTypeOptions
{
    /// <summary>
    /// Gets or sets whether detection for this type is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the confidence threshold for this type.
    /// </summary>
    public double Confidence { get; set; } = 0.8;

    /// <summary>
    /// Gets or sets the action to take when this type is detected.
    /// </summary>
    public string Action { get; set; } = "Log";

    /// <summary>
    /// Gets or sets the replacement text for redaction.
    /// </summary>
    public string? Replacement { get; set; }

    /// <summary>
    /// Gets or sets the supported regions for this detection type.
    /// </summary>
    public string[]? Regions { get; set; }

    /// <summary>
    /// Gets or sets whether to preserve domain for email addresses.
    /// </summary>
    public bool PreserveDomain { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to use partial masking.
    /// </summary>
    public bool PartialMask { get; set; } = false;

    /// <summary>
    /// Gets or sets additional configuration properties.
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

/// <summary>
/// Configuration options for custom PII types.
/// </summary>
public class CustomPiiTypeOptions : PiiTypeOptions
{
    /// <summary>
    /// Gets or sets the name of the custom PII type.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the regex pattern for detection.
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the PII category.
    /// </summary>
    public string Category { get; set; } = "Custom";

    /// <summary>
    /// Gets or sets the description of this custom type.
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Configuration options for compliance profiles.
/// </summary>
public class ComplianceProfileOptions
{
    /// <summary>
    /// Gets or sets whether strict mode is enabled.
    /// </summary>
    public bool StrictMode { get; set; } = false;

    /// <summary>
    /// Gets or sets required detection types for this profile.
    /// </summary>
    public string[] RequiredDetections { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the log retention period.
    /// </summary>
    public string? LogRetention { get; set; }

    /// <summary>
    /// Gets or sets whether consent tracking is required.
    /// </summary>
    public bool ConsentTracking { get; set; } = false;

    /// <summary>
    /// Gets or sets the audit level.
    /// </summary>
    public string AuditLevel { get; set; } = "Basic";

    /// <summary>
    /// Gets or sets whether encryption is required.
    /// </summary>
    public bool EncryptionRequired { get; set; } = false;

    /// <summary>
    /// Gets or sets whether right to delete is supported.
    /// </summary>
    public bool RightToDelete { get; set; } = false;

    /// <summary>
    /// Gets or sets whether data mapping is enabled.
    /// </summary>
    public bool DataMapping { get; set; } = false;

    /// <summary>
    /// Gets or sets additional compliance properties.
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();
}

/// <summary>
/// Performance configuration options for PII detection.
/// </summary>
public class PiiPerformanceOptions
{
    /// <summary>
    /// Gets or sets the maximum processing time allowed.
    /// </summary>
    public TimeSpan MaxProcessingTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the batch size for processing.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether to cache detection results.
    /// </summary>
    public bool CacheResults { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache time-to-live duration.
    /// </summary>
    public TimeSpan CacheTTL { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the maximum content size to process (in bytes).
    /// </summary>
    public long MaxContentSize { get; set; } = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// Gets or sets the number of parallel processing threads.
    /// </summary>
    public int MaxParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets whether to enable performance monitoring.
    /// </summary>
    public bool EnableMonitoring { get; set; } = true;
}