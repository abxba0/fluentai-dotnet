using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentAI.Abstractions.Security;

/// <summary>
/// Default implementation of PII classification engine with built-in compliance framework support.
/// </summary>
public class DefaultPiiClassificationEngine : IPiiClassificationEngine
{
    private readonly ILogger<DefaultPiiClassificationEngine> _logger;
    private readonly IOptions<FluentAI.Configuration.PiiDetectionOptions> _options;

    // Compliance framework requirements
    private static readonly Dictionary<string, ComplianceFramework> ComplianceFrameworks = new()
    {
        ["GDPR"] = new()
        {
            Name = "GDPR",
            RequiredDetections = new[] { "PersonName", "Email", "Phone", "Address" },
            HighRiskTypes = new[] { "PersonName", "Email", "Phone", "Address", "BiometricData" },
            StrictModeEnabled = true,
            RetentionPeriod = TimeSpan.FromDays(30),
            RequiresConsentTracking = true
        },
        ["HIPAA"] = new()
        {
            Name = "HIPAA",
            RequiredDetections = new[] { "SSN", "MedicalRecord", "PatientName", "HealthInsurance" },
            HighRiskTypes = new[] { "SSN", "MedicalRecord", "PatientName", "HealthInsurance", "BiometricData" },
            StrictModeEnabled = true,
            RequiresEncryption = true,
            AuditLevel = "Full"
        },
        ["CCPA"] = new()
        {
            Name = "CCPA",
            RequiredDetections = new[] { "PersonalInfo", "BiometricData", "GeolocationData" },
            HighRiskTypes = new[] { "BiometricData", "GeolocationData", "PersonalInfo" },
            SupportsRightToDelete = true,
            RequiresDataMapping = true
        },
        ["PCI_DSS"] = new()
        {
            Name = "PCI_DSS",
            RequiredDetections = new[] { "CreditCard", "CardholderData" },
            HighRiskTypes = new[] { "CreditCard", "CardholderData", "SecurityCode" },
            StrictModeEnabled = true,
            RequiresEncryption = true,
            AuditLevel = "Full"
        }
    };

    public DefaultPiiClassificationEngine(
        ILogger<DefaultPiiClassificationEngine> logger,
        IOptions<FluentAI.Configuration.PiiDetectionOptions> options)
    {
        _logger = logger;
        _options = options;
    }

    public Task<PiiClassification> ClassifyAsync(PiiDetectionResult detection)
    {
        if (detection == null)
        {
            throw new ArgumentNullException(nameof(detection));
        }

        var classifications = new List<PiiClassification>();

        foreach (var piiDetection in detection.Detections)
        {
            var classification = ClassifyDetection(piiDetection);
            classifications.Add(classification);
        }

        // For single detection, return the first classification
        // For multiple detections, this would need to be aggregated
        var result = classifications.FirstOrDefault() ?? new PiiClassification
        {
            Detection = new PiiDetection(),
            RiskLevel = SecurityRiskLevel.None,
            SensitivityLevel = PiiSensitivityLevel.Low,
            RecommendedAction = PiiAction.Allow
        };

        _logger.LogDebug("Classified PII detection: {Type} -> Risk: {Risk}, Sensitivity: {Sensitivity}", 
            result.Detection.Type, result.RiskLevel, result.SensitivityLevel);

        return Task.FromResult(result);
    }

    public Task<RiskAssessment> AssessRiskAsync(IEnumerable<PiiDetectionResult> detections)
    {
        var allDetections = detections.SelectMany(d => d.Detections).ToList();
        
        if (!allDetections.Any())
        {
            return Task.FromResult(new RiskAssessment
            {
                OverallRiskScore = 0.0,
                HighestRiskLevel = SecurityRiskLevel.None
            });
        }

        var riskFactors = new List<string>();
        var mitigationRecommendations = new List<string>();
        var complianceStatus = new Dictionary<string, bool>();

        // Calculate overall risk score
        var totalRiskScore = 0.0;
        var highestRiskLevel = SecurityRiskLevel.None;

        foreach (var detection in allDetections)
        {
            var classification = ClassifyDetection(detection);
            var detectionRiskScore = CalculateDetectionRiskScore(classification);
            totalRiskScore += detectionRiskScore;

            if (classification.RiskLevel > highestRiskLevel)
            {
                highestRiskLevel = classification.RiskLevel;
            }

            // Add risk factors
            if (classification.SensitivityLevel >= PiiSensitivityLevel.High)
            {
                riskFactors.Add($"High sensitivity {detection.Type} detected");
            }

            if (detection.Confidence < 0.8)
            {
                riskFactors.Add($"Low confidence detection for {detection.Type}");
            }
        }

        // Normalize risk score
        var normalizedRiskScore = Math.Min(1.0, totalRiskScore / allDetections.Count);

        // Generate mitigation recommendations
        GenerateMitigationRecommendations(allDetections, mitigationRecommendations);

        // Assess compliance status
        AssessComplianceStatus(allDetections, complianceStatus);

        var assessment = new RiskAssessment
        {
            OverallRiskScore = normalizedRiskScore,
            HighestRiskLevel = highestRiskLevel,
            RiskFactors = riskFactors.AsReadOnly(),
            MitigationRecommendations = mitigationRecommendations.AsReadOnly(),
            ComplianceStatus = complianceStatus
        };

        _logger.LogInformation("Risk assessment completed: Score {Score:F2}, Highest Risk {Risk}, {FactorCount} factors", 
            normalizedRiskScore, highestRiskLevel, riskFactors.Count);

        return Task.FromResult(assessment);
    }

    public Task<ComplianceReport> GenerateComplianceReportAsync(IEnumerable<PiiDetectionResult> detections, string profile)
    {
        if (!ComplianceFrameworks.TryGetValue(profile.ToUpper(), out var framework))
        {
            throw new ArgumentException($"Unknown compliance profile: {profile}", nameof(profile));
        }

        var allDetections = detections.SelectMany(d => d.Detections).ToList();
        var violations = new List<ComplianceViolation>();
        var requiredActions = new List<string>();

        // Check for required detections
        var detectedTypes = allDetections.Select(d => d.Type).Distinct().ToHashSet();
        var missingRequiredTypes = framework.RequiredDetections.Except(detectedTypes).ToList();

        if (missingRequiredTypes.Any())
        {
            foreach (var missingType in missingRequiredTypes)
            {
                violations.Add(new ComplianceViolation
                {
                    Regulation = framework.Name,
                    Requirement = $"Required PII type detection: {missingType}",
                    Severity = ViolationSeverity.High,
                    Description = $"Compliance framework {framework.Name} requires detection of {missingType} but it was not configured"
                });
            }
            requiredActions.Add($"Configure detection for required PII types: {string.Join(", ", missingRequiredTypes)}");
        }

        // Check for high-risk types without proper protection
        foreach (var detection in allDetections)
        {
            if (framework.HighRiskTypes.Contains(detection.Type))
            {
                if (detection.Action == PiiAction.Allow || detection.Action == PiiAction.Log)
                {
                    violations.Add(new ComplianceViolation
                    {
                        Regulation = framework.Name,
                        Requirement = "High-risk PII protection",
                        Severity = ViolationSeverity.Critical,
                        Description = $"High-risk PII type {detection.Type} detected but not properly protected (action: {detection.Action})",
                        RelatedDetection = detection
                    });
                    requiredActions.Add($"Apply stronger protection to {detection.Type} (recommend Block or Redact)");
                }
            }
        }

        // Framework-specific checks
        PerformFrameworkSpecificChecks(framework, allDetections, violations, requiredActions);

        var isCompliant = !violations.Any(v => v.Severity >= ViolationSeverity.High);

        var report = new ComplianceReport
        {
            ProfileName = profile,
            IsCompliant = isCompliant,
            Violations = violations.AsReadOnly(),
            RequiredActions = requiredActions.AsReadOnly()
        };

        _logger.LogInformation("Generated compliance report for {Framework}: {Status} ({ViolationCount} violations)", 
            framework.Name, isCompliant ? "Compliant" : "Non-Compliant", violations.Count);

        return Task.FromResult(report);
    }

    private PiiClassification ClassifyDetection(PiiDetection detection)
    {
        var riskLevel = DetermineRiskLevel(detection);
        var sensitivityLevel = DetermineSensitivityLevel(detection);
        var applicableRegulations = DetermineApplicableRegulations(detection);
        var recommendedAction = DetermineRecommendedAction(detection, riskLevel, sensitivityLevel);

        return new PiiClassification
        {
            Detection = detection,
            RiskLevel = riskLevel,
            SensitivityLevel = sensitivityLevel,
            ApplicableRegulations = applicableRegulations.AsReadOnly(),
            RecommendedAction = recommendedAction,
            Context = new Dictionary<string, object>
            {
                ["ClassificationVersion"] = "1.0",
                ["ClassifiedAt"] = DateTimeOffset.UtcNow,
                ["ConfidenceThreshold"] = detection.Confidence
            }
        };
    }

    private SecurityRiskLevel DetermineRiskLevel(PiiDetection detection)
    {
        return detection.Category switch
        {
            PiiCategory.Biometric => SecurityRiskLevel.Critical,
            PiiCategory.Government => detection.Type == "SSN" ? SecurityRiskLevel.Critical : SecurityRiskLevel.High,
            PiiCategory.Financial => SecurityRiskLevel.High,
            PiiCategory.Health => SecurityRiskLevel.High,
            PiiCategory.PersonalIdentifier => SecurityRiskLevel.Medium,
            PiiCategory.Contact => SecurityRiskLevel.Low,
            PiiCategory.Custom => SecurityRiskLevel.Medium,
            _ => SecurityRiskLevel.Low
        };
    }

    private PiiSensitivityLevel DetermineSensitivityLevel(PiiDetection detection)
    {
        return detection.Category switch
        {
            PiiCategory.Biometric => PiiSensitivityLevel.Critical,
            PiiCategory.Government => PiiSensitivityLevel.Critical,
            PiiCategory.Health => PiiSensitivityLevel.High,
            PiiCategory.Financial => PiiSensitivityLevel.High,
            PiiCategory.PersonalIdentifier => PiiSensitivityLevel.Medium,
            PiiCategory.Contact => PiiSensitivityLevel.Low,
            PiiCategory.Custom => PiiSensitivityLevel.Medium,
            _ => PiiSensitivityLevel.Low
        };
    }

    private List<string> DetermineApplicableRegulations(PiiDetection detection)
    {
        var regulations = new List<string>();

        foreach (var framework in ComplianceFrameworks.Values)
        {
            if (framework.RequiredDetections.Contains(detection.Type) || 
                framework.HighRiskTypes.Contains(detection.Type))
            {
                regulations.Add(framework.Name);
            }
        }

        return regulations;
    }

    private PiiAction DetermineRecommendedAction(PiiDetection detection, SecurityRiskLevel riskLevel, PiiSensitivityLevel sensitivityLevel)
    {
        // Override with existing action if it's already restrictive enough
        if (detection.Action == PiiAction.Block || detection.Action == PiiAction.Redact)
        {
            return detection.Action;
        }

        return (riskLevel, sensitivityLevel) switch
        {
            (SecurityRiskLevel.Critical, _) => PiiAction.Block,
            (SecurityRiskLevel.High, PiiSensitivityLevel.Critical) => PiiAction.Block,
            (SecurityRiskLevel.High, _) => PiiAction.Redact,
            (SecurityRiskLevel.Medium, PiiSensitivityLevel.High) => PiiAction.Redact,
            (SecurityRiskLevel.Medium, _) => PiiAction.Tokenize,
            (SecurityRiskLevel.Low, _) => PiiAction.Log,
            _ => PiiAction.Allow
        };
    }

    private double CalculateDetectionRiskScore(PiiClassification classification)
    {
        var baseScore = classification.RiskLevel switch
        {
            SecurityRiskLevel.Critical => 1.0,
            SecurityRiskLevel.High => 0.8,
            SecurityRiskLevel.Medium => 0.6,
            SecurityRiskLevel.Low => 0.4,
            SecurityRiskLevel.None => 0.0,
            _ => 0.0
        };

        var sensitivityMultiplier = classification.SensitivityLevel switch
        {
            PiiSensitivityLevel.Critical => 1.2,
            PiiSensitivityLevel.High => 1.1,
            PiiSensitivityLevel.Medium => 1.0,
            PiiSensitivityLevel.Low => 0.9,
            _ => 1.0
        };

        return Math.Min(1.0, baseScore * sensitivityMultiplier);
    }

    private void GenerateMitigationRecommendations(List<PiiDetection> detections, List<string> recommendations)
    {
        var actionCounts = detections.GroupBy(d => d.Action).ToDictionary(g => g.Key, g => g.Count());

        if (actionCounts.GetValueOrDefault(PiiAction.Allow, 0) > 0)
        {
            recommendations.Add("Consider applying protection to PII currently allowed without restrictions");
        }

        if (actionCounts.GetValueOrDefault(PiiAction.Log, 0) > 0)
        {
            recommendations.Add("Review logged PII for potential upgrade to active protection measures");
        }

        var highRiskDetections = detections.Where(d => 
            d.Category == PiiCategory.Government || 
            d.Category == PiiCategory.Financial || 
            d.Category == PiiCategory.Biometric).ToList();

        if (highRiskDetections.Any())
        {
            recommendations.Add("Implement additional security controls for high-risk PII categories");
            recommendations.Add("Consider encryption at rest for sensitive PII data");
        }

        if (detections.Any(d => d.Confidence < 0.8))
        {
            recommendations.Add("Review and improve detection patterns for low-confidence matches");
        }
    }

    private void AssessComplianceStatus(List<PiiDetection> detections, Dictionary<string, bool> complianceStatus)
    {
        foreach (var framework in ComplianceFrameworks.Values)
        {
            var isCompliant = true;
            var detectedTypes = detections.Select(d => d.Type).Distinct().ToHashSet();

            // Check if all required detections are configured
            if (!framework.RequiredDetections.All(required => detectedTypes.Contains(required)))
            {
                isCompliant = false;
            }

            // Check if high-risk types have proper protection
            var highRiskDetections = detections.Where(d => framework.HighRiskTypes.Contains(d.Type));
            if (highRiskDetections.Any(d => d.Action == PiiAction.Allow || d.Action == PiiAction.Log))
            {
                isCompliant = false;
            }

            complianceStatus[framework.Name] = isCompliant;
        }
    }

    private void PerformFrameworkSpecificChecks(ComplianceFramework framework, List<PiiDetection> detections, 
        List<ComplianceViolation> violations, List<string> requiredActions)
    {
        switch (framework.Name)
        {
            case "GDPR":
                CheckGdprCompliance(detections, violations, requiredActions);
                break;
            case "HIPAA":
                CheckHipaaCompliance(detections, violations, requiredActions);
                break;
            case "CCPA":
                CheckCcpaCompliance(detections, violations, requiredActions);
                break;
            case "PCI_DSS":
                CheckPciDssCompliance(detections, violations, requiredActions);
                break;
        }
    }

    private void CheckGdprCompliance(List<PiiDetection> detections, List<ComplianceViolation> violations, List<string> requiredActions)
    {
        // GDPR requires explicit consent for processing personal data
        var personalDataDetections = detections.Where(d => 
            d.Category == PiiCategory.PersonalIdentifier || 
            d.Category == PiiCategory.Contact).ToList();

        if (personalDataDetections.Any())
        {
            requiredActions.Add("Ensure explicit consent is obtained for processing personal data under GDPR");
            requiredActions.Add("Implement data subject rights (access, rectification, erasure, portability)");
        }
    }

    private void CheckHipaaCompliance(List<PiiDetection> detections, List<ComplianceViolation> violations, List<string> requiredActions)
    {
        // HIPAA requires specific protections for PHI
        var phiDetections = detections.Where(d => 
            d.Category == PiiCategory.Health || 
            d.Type == "SSN").ToList();

        if (phiDetections.Any())
        {
            requiredActions.Add("Implement HIPAA-compliant access controls and audit logging");
            requiredActions.Add("Ensure PHI is encrypted in transit and at rest");
        }
    }

    private void CheckCcpaCompliance(List<PiiDetection> detections, List<ComplianceViolation> violations, List<string> requiredActions)
    {
        // CCPA provides consumer rights regarding personal information
        var personalInfoDetections = detections.Where(d => 
            d.Category == PiiCategory.PersonalIdentifier || 
            d.Category == PiiCategory.Contact || 
            d.Category == PiiCategory.Biometric).ToList();

        if (personalInfoDetections.Any())
        {
            requiredActions.Add("Implement consumer rights under CCPA (right to know, delete, opt-out)");
            requiredActions.Add("Maintain records of personal information processing activities");
        }
    }

    private void CheckPciDssCompliance(List<PiiDetection> detections, List<ComplianceViolation> violations, List<string> requiredActions)
    {
        // PCI DSS focuses on payment card data protection
        var cardDataDetections = detections.Where(d => 
            d.Category == PiiCategory.Financial && 
            (d.Type == "CreditCard" || d.Type == "CardholderData")).ToList();

        if (cardDataDetections.Any())
        {
            foreach (var detection in cardDataDetections)
            {
                if (detection.Action != PiiAction.Block && detection.Action != PiiAction.Redact)
                {
                    violations.Add(new ComplianceViolation
                    {
                        Regulation = "PCI_DSS",
                        Requirement = "Cardholder data protection",
                        Severity = ViolationSeverity.Critical,
                        Description = "PCI DSS requires cardholder data to be blocked or redacted",
                        RelatedDetection = detection
                    });
                }
            }
            requiredActions.Add("Implement PCI DSS-compliant cardholder data protection measures");
        }
    }
}

/// <summary>
/// Represents a compliance framework configuration.
/// </summary>
internal record ComplianceFramework
{
    public string Name { get; init; } = string.Empty;
    public string[] RequiredDetections { get; init; } = Array.Empty<string>();
    public string[] HighRiskTypes { get; init; } = Array.Empty<string>();
    public bool StrictModeEnabled { get; init; }
    public TimeSpan? RetentionPeriod { get; init; }
    public bool RequiresConsentTracking { get; init; }
    public bool RequiresEncryption { get; init; }
    public string AuditLevel { get; init; } = "Basic";
    public bool SupportsRightToDelete { get; init; }
    public bool RequiresDataMapping { get; init; }
}