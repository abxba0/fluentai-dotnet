# PII Detection Configuration Guide

FluentAI.NET provides enterprise-grade PII (Personally Identifiable Information) detection capabilities with built-in compliance support for GDPR, HIPAA, CCPA, and PCI-DSS.

## Quick Start

### 1. Basic Configuration

Add PII detection to your `appsettings.json`:

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI",
    "Security": {
      "PiiDetection": {
        "Enabled": true,
        "Provider": "Hybrid",
        "ProcessingMode": "Streaming",
        "DetectionTypes": {
          "CreditCard": {
            "Enabled": true,
            "Confidence": 0.9,
            "Action": "Redact",
            "Replacement": "[CREDIT_CARD]"
          },
          "SSN": {
            "Enabled": true,
            "Confidence": 0.95,
            "Action": "Block",
            "Regions": ["US"]
          },
          "Email": {
            "Enabled": true,
            "Confidence": 0.8,
            "Action": "Tokenize",
            "PreserveDomain": true
          },
          "PersonName": {
            "Enabled": true,
            "Confidence": 0.7,
            "Action": "Mask",
            "PartialMask": true
          }
        },
        "Performance": {
          "MaxProcessingTime": "00:00:05",
          "BatchSize": 100,
          "CacheResults": true,
          "CacheTTL": "01:00:00"
        }
      }
    }
  }
}
```

### 2. Service Registration

Register PII detection services in your application:

```csharp
// ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFluentAI()
    .AddOpenAI(config => config.ApiKey = "your-api-key");

// Add PII detection services
builder.Services.AddPiiDetection(builder.Configuration);

var app = builder.Build();

// Console Application
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddFluentAI()
               .AddOpenAI(config => config.ApiKey = "your-api-key");
        
        services.AddPiiDetection(context.Configuration);
    })
    .Build();
```

### 3. Basic Usage

```csharp
public class ChatService
{
    private readonly IPiiDetectionService _piiDetection;
    private readonly IInputSanitizer _sanitizer;

    public ChatService(IPiiDetectionService piiDetection, IInputSanitizer sanitizer)
    {
        _piiDetection = piiDetection;
        _sanitizer = sanitizer;
    }

    public async Task<string> ProcessUserInputAsync(string userInput)
    {
        // Scan for PII
        var detectionResult = await _piiDetection.ScanAsync(userInput);
        
        if (detectionResult.ShouldBlock)
        {
            return "Content blocked due to sensitive information detected.";
        }

        // Sanitize content with PII handling
        var sanitizedInput = await _sanitizer.SanitizeContentWithPiiAsync(userInput);

        // Process with AI service
        // ... continue with AI processing
        
        return sanitizedInput;
    }
}
```

## Advanced Configuration

### Compliance Profiles

Configure specific compliance requirements:

```json
{
  "AiSdk": {
    "Security": {
      "PiiDetection": {
        "ComplianceProfiles": {
          "GDPR": {
            "StrictMode": true,
            "RequiredDetections": ["PersonName", "Email", "Phone", "Address"],
            "LogRetention": "30d",
            "ConsentTracking": true
          },
          "HIPAA": {
            "RequiredDetections": ["SSN", "MedicalRecord", "PatientName"],
            "AuditLevel": "Full",
            "EncryptionRequired": true
          },
          "PCI_DSS": {
            "RequiredDetections": ["CreditCard", "CardholderData"],
            "AuditLevel": "Full",
            "EncryptionRequired": true
          }
        }
      }
    }
  }
}
```

### Custom PII Patterns

Add organization-specific PII detection:

```json
{
  "AiSdk": {
    "Security": {
      "PiiDetection": {
        "DetectionTypes": {
          "Custom": [
            {
              "Name": "EmployeeId",
              "Pattern": "EMP\\d{6}",
              "Confidence": 1.0,
              "Action": "Redact",
              "Replacement": "[EMPLOYEE_ID]",
              "Category": "Custom",
              "Description": "Internal employee identification numbers"
            },
            {
              "Name": "CustomerAccount",
              "Pattern": "CUST-[A-Z]{2}\\d{8}",
              "Confidence": 0.95,
              "Action": "Tokenize",
              "Category": "Custom"
            }
          ]
        }
      }
    }
  }
}
```

### Programmatic Configuration

Configure PII detection in code:

```csharp
services.AddPiiDetection(options =>
{
    options.Enabled = true;
    options.Performance.CacheResults = true;
    options.Performance.MaxProcessingTime = TimeSpan.FromSeconds(10);

    // Configure detection types
    options.DetectionTypes.CreditCard.Action = "Block";
    options.DetectionTypes.Email.Action = "Tokenize";
    options.DetectionTypes.Email.PreserveDomain = true;

    // Add custom patterns
    options.DetectionTypes.Custom.Add(new CustomPiiTypeOptions
    {
        Name = "InternalId",
        Pattern = @"ID\d{8}",
        Confidence = 0.9,
        Action = "Redact",
        Replacement = "[INTERNAL_ID]"
    });
});
```

## PII Actions

### Available Actions

1. **Allow** - Content passes through unchanged
2. **Log** - Detection is logged but content is not modified
3. **Redact** - Replace PII with configurable replacement text
4. **Tokenize** - Replace with reversible tokens for secure processing
5. **Mask** - Partially hide PII (e.g., show last 4 digits)
6. **Block** - Reject the entire content/request

### Action Examples

```csharp
// Redaction
"My card number is 4532015112830366" 
→ "My card number is [CREDIT_CARD]"

// Masking  
"Contact john.doe@example.com"
→ "Contact j***@example.com"

"SSN: 123-45-6789"
→ "SSN: XXX-XX-6789"

// Tokenization
"Email: user@company.com"
→ "Email: TOKEN_abc123def456"
```

## Compliance Features

### Risk Assessment

```csharp
public async Task<RiskReport> AssessContentRisk(string content)
{
    var detectionResult = await _piiDetection.ScanAsync(content);
    var riskAssessment = await _classificationEngine.AssessRiskAsync(new[] { detectionResult });
    
    return new RiskReport 
    {
        OverallRisk = riskAssessment.OverallRiskScore,
        HighestRiskLevel = riskAssessment.HighestRiskLevel,
        Recommendations = riskAssessment.MitigationRecommendations
    };
}
```

### Compliance Reporting

```csharp
public async Task<ComplianceReport> GenerateGdprReport(IEnumerable<PiiDetectionResult> detections)
{
    var report = await _classificationEngine.GenerateComplianceReportAsync(detections, "GDPR");
    
    if (!report.IsCompliant)
    {
        foreach (var violation in report.Violations)
        {
            _logger.LogWarning("GDPR Violation: {Description}", violation.Description);
        }
    }
    
    return report;
}
```

## Performance Optimization

### Caching Configuration

```json
{
  "Performance": {
    "CacheResults": true,
    "CacheTTL": "01:00:00",
    "MaxContentSize": 10485760,
    "MaxParallelism": 4,
    "EnableMonitoring": true
  }
}
```

### Streaming Processing

```csharp
public async Task ProcessLargeContentAsync(IAsyncEnumerable<string> contentStream)
{
    await foreach (var result in _piiDetection.ScanStreamAsync(contentStream))
    {
        if (result.HasPii)
        {
            // Handle detected PII
            await ProcessPiiDetectionAsync(result);
        }
    }
}
```

## Security Best Practices

1. **Fail Secure** - Block content when PII detection fails
2. **Audit Logging** - Log all PII detection events for compliance
3. **Regular Updates** - Keep detection patterns updated
4. **Performance Monitoring** - Monitor detection performance and accuracy
5. **Data Minimization** - Only detect necessary PII types
6. **Secure Storage** - Encrypt cached detection results

## Built-in Detection Types

| Type | Category | Default Action | Validation |
|------|----------|----------------|------------|
| CreditCard | Financial | Redact | Luhn Algorithm |
| SSN | Government | Block | Format + Range |
| Email | Contact | Tokenize | RFC Compliant |
| Phone | Contact | Mask | Multi-format |
| Address | Contact | Redact | Pattern-based |
| IPv4/IPv6 | Contact | Log | Format Validation |
| MAC Address | Contact | Redact | IEEE Format |

## Monitoring and Debugging

Enable detailed logging:

```json
{
  "Logging": {
    "LogLevel": {
      "FluentAI.Abstractions.Security": "Debug"
    }
  }
}
```

This will provide detailed information about PII detection operations, performance metrics, and compliance assessments.