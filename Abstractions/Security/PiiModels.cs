using System.Text.RegularExpressions;

namespace FluentAI.Abstractions.Security;

/// <summary>
/// Represents the result of a PII detection scan.
/// </summary>
public record PiiDetectionResult
{
    /// <summary>
    /// Gets the unique identifier for this detection result.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the timestamp when the detection was performed.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the original content that was scanned.
    /// </summary>
    public string OriginalContent { get; init; } = string.Empty;

    /// <summary>
    /// Gets detections found in the content.
    /// </summary>
    public IReadOnlyList<PiiDetection> Detections { get; init; } = Array.Empty<PiiDetection>();

    /// <summary>
    /// Gets the overall risk level for all detections.
    /// </summary>
    public SecurityRiskLevel OverallRiskLevel { get; init; }

    /// <summary>
    /// Gets metadata about the detection process.
    /// </summary>
    public PiiDetectionMetadata Metadata { get; init; } = new();

    /// <summary>
    /// Gets a value indicating whether any PII was detected.
    /// </summary>
    public bool HasPii => Detections.Any();

    /// <summary>
    /// Gets a value indicating whether the content should be blocked based on detection results.
    /// </summary>
    public bool ShouldBlock => OverallRiskLevel >= SecurityRiskLevel.High || 
                               Detections.Any(d => d.Action == PiiAction.Block);
}

/// <summary>
/// Represents a single PII detection within content.
/// </summary>
public record PiiDetection
{
    /// <summary>
    /// Gets the PII category that was detected.
    /// </summary>
    public PiiCategory Category { get; init; }

    /// <summary>
    /// Gets the specific PII type (e.g., "CreditCard", "SSN").
    /// </summary>
    public string Type { get; init; } = string.Empty;

    /// <summary>
    /// Gets the detected content.
    /// </summary>
    public string DetectedContent { get; init; } = string.Empty;

    /// <summary>
    /// Gets the start position in the original content.
    /// </summary>
    public int StartPosition { get; init; }

    /// <summary>
    /// Gets the end position in the original content.
    /// </summary>
    public int EndPosition { get; init; }

    /// <summary>
    /// Gets the confidence score (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets the action to take for this detection.
    /// </summary>
    public PiiAction Action { get; init; }

    /// <summary>
    /// Gets the replacement text for redaction.
    /// </summary>
    public string? Replacement { get; init; }

    /// <summary>
    /// Gets additional context or metadata.
    /// </summary>
    public Dictionary<string, object> Context { get; init; } = new();
}

/// <summary>
/// Represents a PII detection pattern.
/// </summary>
public record PiiPattern
{
    /// <summary>
    /// Gets the unique name of the pattern.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the PII category this pattern detects.
    /// </summary>
    public PiiCategory Category { get; init; }

    /// <summary>
    /// Gets the regular expression pattern.
    /// </summary>
    public string Pattern { get; init; } = string.Empty;

    /// <summary>
    /// Gets the compiled regex for performance.
    /// </summary>
    public Regex CompiledPattern { get; init; } = new(string.Empty);

    /// <summary>
    /// Gets the confidence threshold for this pattern.
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets the default action to take when this pattern matches.
    /// </summary>
    public PiiAction DefaultAction { get; init; }

    /// <summary>
    /// Gets the default replacement text.
    /// </summary>
    public string? DefaultReplacement { get; init; }

    /// <summary>
    /// Gets supported regions/locales for this pattern.
    /// </summary>
    public IReadOnlyList<string> SupportedRegions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets a value indicating whether this pattern is enabled.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets additional validation logic for this pattern.
    /// </summary>
    public Func<string, bool>? CustomValidator { get; init; }
}

/// <summary>
/// Configuration options for PII detection.
/// </summary>
public record PiiDetectionOptions
{
    /// <summary>
    /// Gets or sets the detection mode.
    /// </summary>
    public PiiDetectionMode Mode { get; set; } = PiiDetectionMode.Comprehensive;

    /// <summary>
    /// Gets or sets the minimum confidence threshold.
    /// </summary>
    public double MinimumConfidence { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the specific PII categories to detect.
    /// </summary>
    public ISet<PiiCategory> EnabledCategories { get; set; } = new HashSet<PiiCategory>();

    /// <summary>
    /// Gets or sets the regions/locales to consider for detection.
    /// </summary>
    public ISet<string> TargetRegions { get; set; } = new HashSet<string>();

    /// <summary>
    /// Gets or sets custom patterns to include in detection.
    /// </summary>
    public IList<PiiPattern> CustomPatterns { get; set; } = new List<PiiPattern>();

    /// <summary>
    /// Gets or sets the processing timeout.
    /// </summary>
    public TimeSpan ProcessingTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets whether to include context around detections.
    /// </summary>
    public bool IncludeContext { get; set; } = true;

    /// <summary>
    /// Gets or sets the compliance profile to apply.
    /// </summary>
    public string? ComplianceProfile { get; set; }
}

/// <summary>
/// Metadata about the PII detection process.
/// </summary>
public record PiiDetectionMetadata
{
    /// <summary>
    /// Gets the detection engine version used.
    /// </summary>
    public string EngineVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Gets the patterns that were evaluated.
    /// </summary>
    public IReadOnlyList<string> EvaluatedPatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the processing time taken.
    /// </summary>
    public TimeSpan ProcessingTime { get; init; }

    /// <summary>
    /// Gets performance metrics.
    /// </summary>
    public Dictionary<string, object> PerformanceMetrics { get; init; } = new();
}

/// <summary>
/// Represents PII classification results.
/// </summary>
public record PiiClassification
{
    /// <summary>
    /// Gets the classification identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the PII detection this classification is for.
    /// </summary>
    public PiiDetection Detection { get; init; } = new();

    /// <summary>
    /// Gets the assigned risk level.
    /// </summary>
    public SecurityRiskLevel RiskLevel { get; init; }

    /// <summary>
    /// Gets the sensitivity level.
    /// </summary>
    public PiiSensitivityLevel SensitivityLevel { get; init; }

    /// <summary>
    /// Gets applicable regulatory requirements.
    /// </summary>
    public IReadOnlyList<string> ApplicableRegulations { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the recommended action.
    /// </summary>
    public PiiAction RecommendedAction { get; init; }

    /// <summary>
    /// Gets additional classification context.
    /// </summary>
    public Dictionary<string, object> Context { get; init; } = new();
}

/// <summary>
/// Comprehensive risk assessment for multiple PII detections.
/// </summary>
public record RiskAssessment
{
    /// <summary>
    /// Gets the assessment identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the overall risk score (0.0 to 1.0).
    /// </summary>
    public double OverallRiskScore { get; init; }

    /// <summary>
    /// Gets the highest risk level detected.
    /// </summary>
    public SecurityRiskLevel HighestRiskLevel { get; init; }

    /// <summary>
    /// Gets the risk factors identified.
    /// </summary>
    public IReadOnlyList<string> RiskFactors { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets mitigation recommendations.
    /// </summary>
    public IReadOnlyList<string> MitigationRecommendations { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the compliance status.
    /// </summary>
    public Dictionary<string, bool> ComplianceStatus { get; init; } = new();
}

/// <summary>
/// Compliance report for regulatory requirements.
/// </summary>
public record ComplianceReport
{
    /// <summary>
    /// Gets the report identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the compliance profile name.
    /// </summary>
    public string ProfileName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the compliance status.
    /// </summary>
    public bool IsCompliant { get; init; }

    /// <summary>
    /// Gets compliance violations found.
    /// </summary>
    public IReadOnlyList<ComplianceViolation> Violations { get; init; } = Array.Empty<ComplianceViolation>();

    /// <summary>
    /// Gets required actions for compliance.
    /// </summary>
    public IReadOnlyList<string> RequiredActions { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the report generation timestamp.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a compliance violation.
/// </summary>
public record ComplianceViolation
{
    /// <summary>
    /// Gets the regulation that was violated.
    /// </summary>
    public string Regulation { get; init; } = string.Empty;

    /// <summary>
    /// Gets the specific requirement that was violated.
    /// </summary>
    public string Requirement { get; init; } = string.Empty;

    /// <summary>
    /// Gets the severity of the violation.
    /// </summary>
    public ViolationSeverity Severity { get; init; }

    /// <summary>
    /// Gets a description of the violation.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the PII detection that caused this violation.
    /// </summary>
    public PiiDetection? RelatedDetection { get; init; }
}

/// <summary>
/// Secure request wrapper with PII detection options.
/// </summary>
public record SecureRequest
{
    /// <summary>
    /// Gets the request identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the original request content.
    /// </summary>
    public object Content { get; init; } = new();

    /// <summary>
    /// Gets PII detection options for this request.
    /// </summary>
    public PiiDetectionOptions? PiiOptions { get; init; }

    /// <summary>
    /// Gets additional security options.
    /// </summary>
    public Dictionary<string, object> SecurityOptions { get; init; } = new();
}

/// <summary>
/// Secure response with security metadata.
/// </summary>
public record SecureResponse
{
    /// <summary>
    /// Gets the response identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the processed response content.
    /// </summary>
    public object Content { get; init; } = new();

    /// <summary>
    /// Gets the PII detection results from processing.
    /// </summary>
    public PiiDetectionResult? PiiDetectionResult { get; init; }

    /// <summary>
    /// Gets the security assessment.
    /// </summary>
    public SecurityRiskAssessment? SecurityAssessment { get; init; }

    /// <summary>
    /// Gets security metadata.
    /// </summary>
    public Dictionary<string, object> SecurityMetadata { get; init; } = new();
}

/// <summary>
/// Security report for processed requests.
/// </summary>
public record SecurityReport
{
    /// <summary>
    /// Gets the report identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the request identifier this report is for.
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Gets all PII detections from the request.
    /// </summary>
    public IReadOnlyList<PiiDetectionResult> PiiDetections { get; init; } = Array.Empty<PiiDetectionResult>();

    /// <summary>
    /// Gets security assessments performed.
    /// </summary>
    public IReadOnlyList<SecurityRiskAssessment> SecurityAssessments { get; init; } = Array.Empty<SecurityRiskAssessment>();

    /// <summary>
    /// Gets actions taken during processing.
    /// </summary>
    public IReadOnlyList<string> ActionsTaken { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the report generation timestamp.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Compliance audit information.
/// </summary>
public record ComplianceAudit
{
    /// <summary>
    /// Gets the audit identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Gets the audit date range.
    /// </summary>
    public DateRange DateRange { get; init; } = new();

    /// <summary>
    /// Gets compliance reports generated during the period.
    /// </summary>
    public IReadOnlyList<ComplianceReport> ComplianceReports { get; init; } = Array.Empty<ComplianceReport>();

    /// <summary>
    /// Gets audit summary statistics.
    /// </summary>
    public AuditStatistics Statistics { get; init; } = new();
}

/// <summary>
/// Date range for audits.
/// </summary>
public record DateRange
{
    /// <summary>
    /// Gets the start date.
    /// </summary>
    public DateTimeOffset StartDate { get; init; }

    /// <summary>
    /// Gets the end date.
    /// </summary>
    public DateTimeOffset EndDate { get; init; }
}

/// <summary>
/// Audit statistics summary.
/// </summary>
public record AuditStatistics
{
    /// <summary>
    /// Gets the total number of requests processed.
    /// </summary>
    public int TotalRequests { get; init; }

    /// <summary>
    /// Gets the number of requests with PII detected.
    /// </summary>
    public int RequestsWithPii { get; init; }

    /// <summary>
    /// Gets the most common PII types detected.
    /// </summary>
    public Dictionary<string, int> PiiTypeCounts { get; init; } = new();

    /// <summary>
    /// Gets compliance violation counts by regulation.
    /// </summary>
    public Dictionary<string, int> ViolationCounts { get; init; } = new();
}

/// <summary>
/// PII categories for classification.
/// </summary>
public enum PiiCategory
{
    /// <summary>Personal identifiers like names, IDs.</summary>
    PersonalIdentifier,
    /// <summary>Financial information like credit cards, bank accounts.</summary>
    Financial,
    /// <summary>Health and medical information.</summary>
    Health,
    /// <summary>Contact information like emails, phones, addresses.</summary>
    Contact,
    /// <summary>Government-issued identifiers like SSN, passport numbers.</summary>
    Government,
    /// <summary>Biometric data.</summary>
    Biometric,
    /// <summary>Custom or organization-specific PII.</summary>
    Custom
}

/// <summary>
/// Actions to take when PII is detected.
/// </summary>
public enum PiiAction
{
    /// <summary>Allow the content to pass through unchanged.</summary>
    Allow,
    /// <summary>Log the detection but allow processing.</summary>
    Log,
    /// <summary>Replace PII with a redaction marker.</summary>
    Redact,
    /// <summary>Replace PII with a token for later de-tokenization.</summary>
    Tokenize,
    /// <summary>Mask part of the PII (e.g., show last 4 digits).</summary>
    Mask,
    /// <summary>Block the entire request/content.</summary>
    Block
}

/// <summary>
/// PII detection modes.
/// </summary>
public enum PiiDetectionMode
{
    /// <summary>Fast detection using basic patterns only.</summary>
    Fast,
    /// <summary>Balanced detection with moderate accuracy.</summary>
    Balanced,
    /// <summary>Comprehensive detection with highest accuracy.</summary>
    Comprehensive
}

/// <summary>
/// PII sensitivity levels.
/// </summary>
public enum PiiSensitivityLevel
{
    /// <summary>Low sensitivity PII.</summary>
    Low,
    /// <summary>Medium sensitivity PII.</summary>
    Medium,
    /// <summary>High sensitivity PII.</summary>
    High,
    /// <summary>Critical sensitivity PII.</summary>
    Critical
}

/// <summary>
/// Compliance violation severities.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>Informational violation.</summary>
    Info,
    /// <summary>Low severity violation.</summary>
    Low,
    /// <summary>Medium severity violation.</summary>
    Medium,
    /// <summary>High severity violation.</summary>
    High,
    /// <summary>Critical violation requiring immediate action.</summary>
    Critical
}