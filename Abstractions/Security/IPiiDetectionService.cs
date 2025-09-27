using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Security;

/// <summary>
/// Core service for detecting personally identifiable information (PII) in content.
/// </summary>
public interface IPiiDetectionService
{
    /// <summary>
    /// Scans text content for PII and returns detection results.
    /// </summary>
    /// <param name="content">The text content to scan.</param>
    /// <param name="options">Optional detection configuration.</param>
    /// <returns>PII detection results.</returns>
    Task<PiiDetectionResult> ScanAsync(string content, Abstractions.Security.PiiDetectionOptions? options = null);

    /// <summary>
    /// Scans binary content for PII and returns detection results.
    /// </summary>
    /// <param name="content">The binary content to scan.</param>
    /// <param name="contentType">The MIME type of the content.</param>
    /// <param name="options">Optional detection configuration.</param>
    /// <returns>PII detection results.</returns>
    Task<PiiDetectionResult> ScanAsync(byte[] content, string contentType, Abstractions.Security.PiiDetectionOptions? options = null);

    /// <summary>
    /// Scans streaming content for PII and returns detection results as they're found.
    /// </summary>
    /// <param name="contentStream">The streaming content to scan.</param>
    /// <returns>Stream of PII detection results.</returns>
    IAsyncEnumerable<PiiDetectionResult> ScanStreamAsync(IAsyncEnumerable<string> contentStream);

    /// <summary>
    /// Redacts detected PII from content using configured replacement strategies.
    /// </summary>
    /// <param name="content">The original content.</param>
    /// <param name="detectionResult">The PII detection results.</param>
    /// <returns>Content with PII redacted.</returns>
    Task<string> RedactAsync(string content, PiiDetectionResult detectionResult);

    /// <summary>
    /// Tokenizes detected PII from content for secure processing.
    /// </summary>
    /// <param name="content">The original content.</param>
    /// <param name="detectionResult">The PII detection results.</param>
    /// <returns>Content with PII tokenized.</returns>
    Task<string> TokenizeAsync(string content, PiiDetectionResult detectionResult);
}

/// <summary>
/// Registry for managing PII detection patterns.
/// </summary>
public interface IPiiPatternRegistry
{
    /// <summary>
    /// Registers a new PII detection pattern.
    /// </summary>
    /// <param name="pattern">The pattern to register.</param>
    Task RegisterPatternAsync(PiiPattern pattern);

    /// <summary>
    /// Gets all patterns for a specific PII category.
    /// </summary>
    /// <param name="category">The PII category.</param>
    /// <returns>Collection of patterns for the category.</returns>
    Task<IEnumerable<PiiPattern>> GetPatternsAsync(PiiCategory category);

    /// <summary>
    /// Gets a specific pattern by name.
    /// </summary>
    /// <param name="name">The pattern name.</param>
    /// <returns>The pattern if found.</returns>
    Task<PiiPattern?> GetPatternAsync(string name);

    /// <summary>
    /// Updates an existing pattern.
    /// </summary>
    /// <param name="pattern">The updated pattern.</param>
    Task UpdatePatternAsync(PiiPattern pattern);

    /// <summary>
    /// Removes a pattern by name.
    /// </summary>
    /// <param name="name">The pattern name to remove.</param>
    Task RemovePatternAsync(string name);
}

/// <summary>
/// Engine for classifying and assessing risk of detected PII.
/// </summary>
public interface IPiiClassificationEngine
{
    /// <summary>
    /// Classifies detected PII and assigns risk levels.
    /// </summary>
    /// <param name="detection">The PII detection result.</param>
    /// <returns>Classification with risk assessment.</returns>
    Task<PiiClassification> ClassifyAsync(PiiDetectionResult detection);

    /// <summary>
    /// Assesses overall risk from multiple PII detections.
    /// </summary>
    /// <param name="detections">Collection of PII detections.</param>
    /// <returns>Aggregated risk assessment.</returns>
    Task<RiskAssessment> AssessRiskAsync(IEnumerable<PiiDetectionResult> detections);

    /// <summary>
    /// Generates compliance report for detected PII.
    /// </summary>
    /// <param name="detections">Collection of PII detections.</param>
    /// <param name="profile">Compliance profile name (GDPR, HIPAA, CCPA).</param>
    /// <returns>Compliance report.</returns>
    Task<ComplianceReport> GenerateComplianceReportAsync(IEnumerable<PiiDetectionResult> detections, string profile);
}

/// <summary>
/// Extended AI service interface with secure processing capabilities.
/// </summary>
public interface ISecureAiService : IAiService
{
    /// <summary>
    /// Processes requests with integrated PII detection and security controls.
    /// </summary>
    /// <param name="request">The secure request with PII detection options.</param>
    /// <returns>Response with security metadata.</returns>
    Task<SecureResponse> ProcessSecurelyAsync(SecureRequest request);

    /// <summary>
    /// Gets security report for a processed request.
    /// </summary>
    /// <param name="requestId">The request identifier.</param>
    /// <returns>Security report including PII detection results.</returns>
    Task<SecurityReport> GetSecurityReportAsync(string requestId);

    /// <summary>
    /// Gets compliance audit information for a date range.
    /// </summary>
    /// <param name="range">The date range for the audit.</param>
    /// <returns>Compliance audit report.</returns>
    Task<ComplianceAudit> GetComplianceAuditAsync(DateRange range);
}