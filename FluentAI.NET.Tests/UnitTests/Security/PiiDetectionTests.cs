using FluentAI.Abstractions.Security;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Security;

/// <summary>
/// Unit tests for PII detection services following the existing test patterns.
/// </summary>
public class PiiDetectionTests
{
    private readonly Mock<ILogger<HybridPiiDetectionService>> _mockLogger;
    private readonly Mock<ILogger<InMemoryPiiPatternRegistry>> _mockRegistryLogger;
    private readonly Mock<ILogger<DefaultPiiClassificationEngine>> _mockEngineLogger;
    private readonly IOptions<FluentAI.Configuration.PiiDetectionOptions> _options;

    public PiiDetectionTests()
    {
        _mockLogger = new Mock<ILogger<HybridPiiDetectionService>>();
        _mockRegistryLogger = new Mock<ILogger<InMemoryPiiPatternRegistry>>();
        _mockEngineLogger = new Mock<ILogger<DefaultPiiClassificationEngine>>();
        
        _options = Options.Create(new FluentAI.Configuration.PiiDetectionOptions
        {
            Enabled = true,
            Performance = new FluentAI.Configuration.PiiPerformanceOptions
            {
                CacheResults = true,
                CacheTTL = TimeSpan.FromMinutes(30)
            }
        });
    }

    [Fact]
    public async Task HybridPiiDetectionService_ScanAsync_DetectsCreditCard()
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);
        
        var testContent = "Please charge my credit card 4532015112830366 for the purchase.";

        // Act
        var result = await service.ScanAsync(testContent);

        // Assert
        Assert.True(result.HasPii);
        Assert.Single(result.Detections);
        
        var detection = result.Detections.First();
        Assert.Equal(PiiCategory.Financial, detection.Category);
        Assert.Equal("CreditCard", detection.Type);
        Assert.Equal("4532015112830366", detection.DetectedContent);
        Assert.True(detection.Confidence >= 0.9);
    }

    [Fact]
    public async Task HybridPiiDetectionService_ScanAsync_DetectsEmail()
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);
        
        var testContent = "Please contact me at john.doe@example.com for more information.";

        // Act
        var result = await service.ScanAsync(testContent);

        // Assert
        Assert.True(result.HasPii);
        Assert.Single(result.Detections);
        
        var detection = result.Detections.First();
        Assert.Equal(PiiCategory.Contact, detection.Category);
        Assert.Equal("Email", detection.Type);
        Assert.Equal("john.doe@example.com", detection.DetectedContent);
    }

    [Fact]
    public async Task HybridPiiDetectionService_ScanAsync_DetectsSSN()
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);
        
        var testContent = "My SSN is 123-45-6789 for verification.";

        // Act
        var result = await service.ScanAsync(testContent);

        // Assert
        Assert.True(result.HasPii);
        Assert.Single(result.Detections);
        
        var detection = result.Detections.First();
        Assert.Equal(PiiCategory.Government, detection.Category);
        Assert.Equal("SSN", detection.Type);
        Assert.Equal("123-45-6789", detection.DetectedContent);
        Assert.Equal(PiiAction.Block, detection.Action);
    }

    [Fact]
    public async Task HybridPiiDetectionService_RedactAsync_ReplacesDetectedPii()
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);
        
        var testContent = "Please charge my credit card 4532015112830366 for the purchase.";
        var detectionResult = await service.ScanAsync(testContent);

        // Act
        var redactedContent = await service.RedactAsync(testContent, detectionResult);

        // Assert
        Assert.Contains("[CREDIT_CARD]", redactedContent);
        Assert.DoesNotContain("4532015112830366", redactedContent);
    }

    [Fact]
    public async Task InMemoryPiiPatternRegistry_RegisterPatternAsync_AddsPattern()
    {
        // Arrange
        var registry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var customPattern = new PiiPattern
        {
            Name = "CustomId",
            Category = PiiCategory.Custom,
            Pattern = @"CUST\d{6}",
            CompiledPattern = new System.Text.RegularExpressions.Regex(@"CUST\d{6}"),
            Confidence = 0.95,
            DefaultAction = PiiAction.Redact,
            DefaultReplacement = "[CUSTOMER_ID]"
        };

        // Act
        await registry.RegisterPatternAsync(customPattern);
        var retrievedPattern = await registry.GetPatternAsync("CustomId");

        // Assert
        Assert.NotNull(retrievedPattern);
        Assert.Equal("CustomId", retrievedPattern.Name);
        Assert.Equal(PiiCategory.Custom, retrievedPattern.Category);
        Assert.Equal(0.95, retrievedPattern.Confidence);
    }

    [Fact]
    public async Task DefaultPiiClassificationEngine_AssessRiskAsync_CalculatesCorrectRisk()
    {
        // Arrange
        var engine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var detections = new[]
        {
            new PiiDetectionResult
            {
                Detections = new[]
                {
                    new PiiDetection
                    {
                        Category = PiiCategory.Financial,
                        Type = "CreditCard",
                        DetectedContent = "4532015112830366",
                        Confidence = 0.95,
                        Action = PiiAction.Redact
                    }
                }
            }
        };

        // Act
        var riskAssessment = await engine.AssessRiskAsync(detections);

        // Assert
        Assert.True(riskAssessment.OverallRiskScore > 0.5);
        Assert.Equal(SecurityRiskLevel.High, riskAssessment.HighestRiskLevel);
        Assert.Contains("High sensitivity", riskAssessment.RiskFactors.FirstOrDefault() ?? "");
    }

    [Fact]
    public async Task DefaultPiiClassificationEngine_GenerateComplianceReportAsync_ValidatesGdpr()
    {
        // Arrange
        var engine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var detections = new[]
        {
            new PiiDetectionResult
            {
                Detections = new[]
                {
                    new PiiDetection
                    {
                        Category = PiiCategory.PersonalIdentifier,
                        Type = "PersonName",
                        DetectedContent = "John Doe",
                        Confidence = 0.8,
                        Action = PiiAction.Allow // This should trigger a violation
                    }
                }
            }
        };

        // Act
        var complianceReport = await engine.GenerateComplianceReportAsync(detections, "GDPR");

        // Assert
        Assert.Equal("GDPR", complianceReport.ProfileName);
        Assert.False(complianceReport.IsCompliant);
        Assert.NotEmpty(complianceReport.RequiredActions);
    }

    [Fact]
    public void PiiDetectionResult_ShouldBlock_ReturnsTrueForHighRisk()
    {
        // Arrange
        var result = new PiiDetectionResult
        {
            OverallRiskLevel = SecurityRiskLevel.High,
            Detections = new[]
            {
                new PiiDetection
                {
                    Action = PiiAction.Redact
                }
            }
        };

        // Assert
        Assert.True(result.ShouldBlock);
    }

    [Fact]
    public void PiiDetectionResult_ShouldBlock_ReturnsTrueForBlockAction()
    {
        // Arrange
        var result = new PiiDetectionResult
        {
            OverallRiskLevel = SecurityRiskLevel.Low,
            Detections = new[]
            {
                new PiiDetection
                {
                    Action = PiiAction.Block
                }
            }
        };

        // Assert
        Assert.True(result.ShouldBlock);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task HybridPiiDetectionService_ScanAsync_HandlesEmptyContent(string? content)
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);

        // Act
        var result = await service.ScanAsync(content ?? "");

        // Assert
        Assert.False(result.HasPii);
        Assert.Empty(result.Detections);
        Assert.Equal(SecurityRiskLevel.None, result.OverallRiskLevel);
    }

    [Fact]
    public async Task HybridPiiDetectionService_ScanAsync_RespectsConfidenceThreshold()
    {
        // Arrange
        var patternRegistry = new InMemoryPiiPatternRegistry(_mockRegistryLogger.Object);
        var classificationEngine = new DefaultPiiClassificationEngine(_mockEngineLogger.Object, _options);
        var service = new HybridPiiDetectionService(_mockLogger.Object, _options, patternRegistry, classificationEngine);
        
        var testContent = "Contact john.doe@example.com";
        var options = new FluentAI.Abstractions.Security.PiiDetectionOptions
        {
            MinimumConfidence = 0.95 // Higher than email detection confidence
        };

        // Act
        var result = await service.ScanAsync(testContent, options);

        // Assert - Should not detect email due to high confidence threshold
        var emailDetections = result.Detections.Where(d => d.Type == "Email");
        Assert.Empty(emailDetections.Where(d => d.Confidence < 0.95));
    }
}