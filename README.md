# FLUENTAI.NET - Universal AI SDK for .NET

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/FluentAI.NET.svg)](https://www.nuget.org/packages/FluentAI.NET/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-passing-brightgreen.svg)]()
[![Coverage](https://img.shields.io/badge/coverage-87%25-yellow.svg)](docs/AUDIT-EXECUTIVE-SUMMARY.md)
[![Documentation](https://img.shields.io/badge/docs-comprehensive-brightgreen.svg)](docs/)
[![Audit](https://img.shields.io/badge/audit-completed-blue.svg)](docs/AUDIT-EXECUTIVE-SUMMARY.md)

FluentAI.NET is a comprehensive, production-ready SDK that unifies access to multiple AI chat models under a single, elegant API. Built for .NET developers who want enterprise-grade AI capabilities without vendor lock-in or complex configuration.

## 📋 Table of Contents

- [✨ Key Features](#-key-features)
- [🚀 Supported Providers](#-supported-providers)
- [📦 Installation](#-installation)
- [🎯 Quick Start](#-quick-start)
- [🔧 Advanced Usage](#-advanced-usage)
- [🏗️ Architecture](#️-architecture)
- [📖 Documentation](#-documentation)
- [🧪 Examples & Demos](#-examples--demos)
- [🛠️ Integration Guides](#️-integration-guides)
- [🔐 Security](#-security)
- [⚡ Performance](#-performance)
- [🧪 Testing](#-testing)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)
- [🆘 Support](#-support)

## ✨ Key Features

### 🌟 **Production-Ready Architecture**
✅ **Multi-Provider Support** - OpenAI, Anthropic, Google AI with unified interface  
✅ **Enterprise Security** - Input sanitization, content filtering, risk assessment  
✅ **Advanced Resilience** - Rate limiting, automatic failover, circuit breakers  
✅ **Performance Optimized** - Response caching, memory management, streaming support  
✅ **Observability Built-in** - Comprehensive logging, metrics, health checks  
✅ **Dependency Injection** - First-class support for modern .NET patterns  

### 🔧 **Developer Experience**
✅ **Simple Integration** - Single interface for all providers  
✅ **Rich Configuration** - Environment variables, appsettings.json, Azure Key Vault  
✅ **Comprehensive Examples** - Working demos for all project types  
✅ **Extensive Documentation** - API reference, integration guides, troubleshooting  
✅ **Strong Typing** - Full IntelliSense support and compile-time safety  
✅ **Async/Await** - Native async support with cancellation tokens  

### 🛡️ **Security & Compliance**
✅ **Input Validation** - Prompt injection detection and prevention  
✅ **Content Filtering** - Configurable safety filters and risk assessment  
✅ **Secure Logging** - Automatic redaction of sensitive data  
✅ **API Key Protection** - Secure storage and rotation support  
✅ **GDPR Compliance** - Data protection and privacy controls

### 🚀 **Advanced Performance Features (New!)**
✅ **Token Streaming with Backpressure** - Prevent overwhelming consumers with bounded buffers  
✅ **Parallelized Batch Operations** - Process multiple requests 5-10x faster  
✅ **Model Benchmarking Tools** - Compare performance, cost, and quality across providers  
✅ **Automatic Token Counting** - Track usage and optimize context windows  
✅ **Semantic Caching** - 15-30% better hit rates with similarity matching  
✅ **Memory Abstractions** - Short-term, long-term, and semantic memory patterns  
✅ **Middleware Pipeline** - Extensible plugin architecture for custom processing  
✅ **Conversation State Management** - Persistent, scalable conversation tracking

**[📖 See Complete Roadmap Implementation](docs/ROADMAP-IMPLEMENTATION.md)**  

## 🚀 Supported Providers

| Provider    | Capability      |
| ----------- | --------------- |
| OpenAI      | Text generation |
| Anthropic   | Text generation |
| Google AI   | Text generation |


**Extensible Architecture** - Add custom providers with minimal code

## 📦 Installation

```bash
# Single package includes all providers - no additional dependencies needed
dotnet add package FluentAI.NET
```

**Supported Platforms:**
- .NET 8.0+
- Windows, Linux, macOS
- Docker containers
- Azure Functions, AWS Lambda
- Blazor Server, Blazor WebAssembly

## 🎯 Quick Start

### 1. Set Up API Keys

```bash
# Environment Variables (Recommended)
export OPENAI_API_KEY="your-openai-api-key"
export ANTHROPIC_API_KEY="your-anthropic-api-key"
export GOOGLE_API_KEY="your-google-api-key"
```

### 2. Configure Services

#### ASP.NET Core
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add FluentAI with automatic provider detection
builder.Services.AddAiSdk(builder.Configuration)
    .AddOpenAiChatModel(builder.Configuration)
    .AddAnthropicChatModel(builder.Configuration)
    .AddGoogleGeminiChatModel(builder.Configuration);

var app = builder.Build();
```

#### Console Application
```csharp
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddAiSdk(context.Configuration)
            .AddOpenAiChatModel(context.Configuration);
    });

using var host = builder.Build();
```

### 3. Configuration (appsettings.json)

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI",
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  },
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 100,
    "WindowInSeconds": 60
  },
  "Anthropic": {
    "Model": "claude-3-sonnet-20240229",
    "MaxTokens": 2000,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 50,
    "WindowInSeconds": 60
  }
}
```

### 4. Use in Your Code

```csharp
public class ChatController : ControllerBase
{
    private readonly IChatModel _chatModel;

    public ChatController(IChatModel chatModel)
    {
        _chatModel = chatModel;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(ChatRole.User, request.Message)
        };

        try
        {
            var response = await _chatModel.GetResponseAsync(messages);
            return Ok(new { response = response.Content, model = response.ModelId });
        }
        catch (AiSdkRateLimitException)
        {
            return StatusCode(429, "Rate limit exceeded. Please try again later.");
        }
        catch (AiSdkException ex)
        {
            return BadRequest($"AI service error: {ex.Message}");
        }
    }

    [HttpPost("stream")]
    public async IAsyncEnumerable<string> StreamChat([FromBody] ChatRequest request)
    {
        var messages = new[] { new ChatMessage(ChatRole.User, request.Message) };
        
        await foreach (var token in _chatModel.StreamResponseAsync(messages))
        {
            yield return token;
        }
    }
}
```

## 🔧 Advanced Usage

### Multi-Provider with Automatic Failover

```csharp
// Configuration enables automatic failover
{
  "AiSdk": {
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  }
}

// Transparent failover - no code changes needed
var response = await _chatModel.GetResponseAsync(messages);
// Uses OpenAI first, automatically falls back to Anthropic on errors
```

### Provider-Specific Options

```csharp
// OpenAI with advanced options
var openAiOptions = new OpenAiRequestOptions
{
    Temperature = 0.8f,
    MaxTokens = 1500,
    TopP = 0.9f,
    FrequencyPenalty = 0.1f
};

var response = await _chatModel.GetResponseAsync(messages, openAiOptions);

// Anthropic with system prompt
var anthropicOptions = new AnthropicRequestOptions
{
    SystemPrompt = "You are an expert software architect.",
    Temperature = 0.7f,
    MaxTokens = 2000
};
```

### Security Features

```csharp
public class SecureChatService
{
    private readonly IChatModel _chatModel;
    private readonly IInputSanitizer _sanitizer;

    public async Task<string> ProcessSecurelyAsync(string userInput)
    {
        // Security validation
        if (!_sanitizer.IsContentSafe(userInput))
            throw new SecurityException("Unsafe content detected");

        // Risk assessment
        var risk = _sanitizer.AssessRisk(userInput);
        if (risk.RiskLevel >= SecurityRiskLevel.High)
            throw new SecurityException($"High risk content: {string.Join(", ", risk.DetectedConcerns)}");

        // Sanitize input
        var sanitizedInput = _sanitizer.SanitizeContent(userInput);
        
        var messages = new[] { new ChatMessage(ChatRole.User, sanitizedInput) };
        var response = await _chatModel.GetResponseAsync(messages);
        
        return response.Content;
    }
}
```

### Performance Optimization

```csharp
public class PerformantChatService
{
    private readonly IChatModel _chatModel;
    private readonly IResponseCache _cache;
    private readonly IPerformanceMonitor _monitor;

    public async Task<ChatResponse> GetCachedResponseAsync(IEnumerable<ChatMessage> messages)
    {
        // Check cache first
        var cachedResponse = await _cache.GetAsync(messages);
        if (cachedResponse != null)
            return cachedResponse;

        // Monitor performance
        using var operation = _monitor.StartOperation("ChatCompletion");
        
        var response = await _chatModel.GetResponseAsync(messages);
        
        // Cache successful responses
        await _cache.SetAsync(messages, null, response, TimeSpan.FromMinutes(30));
        
        // Record metrics
        _monitor.RecordMetric("ResponseLength", response.Content.Length);
        _monitor.IncrementCounter("RequestsProcessed");
        
        return response;
    }
}
```

### Resilience and Error Handling

```csharp
public class ResilientChatService
{
    public async Task<string> GetResponseWithRetryAsync(IEnumerable<ChatMessage> messages)
    {
        var retryPolicy = Policy
            .Handle<AiSdkRateLimitException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _chatModel.GetResponseAsync(messages);
            return response.Content;
        });
    }
}
```

## 🏗️ Architecture

FluentAI.NET follows clean architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐ ┌─────────────────┐ ┌──────────────┐   │
│  │   Controllers   │ │    Services     │ │  Components  │   │
│  └─────────────────┘ └─────────────────┘ └──────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                  FluentAI.NET Abstractions                 │
│  ┌─────────────────┐ ┌─────────────────┐ ┌──────────────┐   │
│  │   IChatModel    │ │  IInputSanitizer│ │ IPerformance │   │
│  │                 │ │                 │ │   Monitor    │   │
│  └─────────────────┘ └─────────────────┘ └──────────────┘   │
└─────────────────────────────────────────────────────────────┘
                              │
┌─────────────────────────────────────────────────────────────┐
│                    Provider Layer                          │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────┐ │
│  │   OpenAI    │ │  Anthropic  │ │   Google    │ │ Custom │ │
│  │  Provider   │ │   Provider  │ │   Provider  │ │Provider│ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Key Components:**
- **Abstractions Layer**: Core interfaces and models
- **Provider Layer**: AI service implementations
- **Configuration Layer**: Strongly-typed configuration
- **Security Layer**: Input validation and risk assessment
- **Performance Layer**: Caching, monitoring, and optimization
- **Extensions Layer**: Dependency injection and fluent configuration

## 📖 Documentation

### 📚 **Core Documentation**
- **[API Reference](docs/API-Reference.md)** - Complete API documentation with examples
- **[Architecture Guide](docs/architecture.md)** - System design, components, and patterns
- **[Code Examples](docs/code-examples.md)** - Practical examples for common scenarios
- **[Security Guide](SECURITY.md)** - Security best practices and compliance
- **[Contributing Guide](CONTRIBUTING.md)** - Development guidelines and processes

### 📊 **Quality Assurance**
- **[Audit Executive Summary](docs/AUDIT-EXECUTIVE-SUMMARY.md)** - ⭐ Feature audit results and coverage analysis
- **[Feature Audit Report](docs/FEATURE-AUDIT-REPORT.md)** - Detailed feature-by-feature verification
- **[Coverage Remediation Plan](docs/COVERAGE-REMEDIATION-PLAN.md)** - Roadmap to 90%+ coverage
- **[Feature Checklist](docs/FEATURE-CHECKLIST.md)** - Complete feature tracking and status

### 🛠️ **Integration Guides**
- **[Console Applications](docs/integration/console-applications.md)** - Complete setup with DI
- **[ASP.NET Core](docs/integration/aspnet-core.md)** - Web APIs with middleware
- **[Blazor](docs/integration/blazor.md)** - Interactive web UIs with real-time streaming
- **[Common Patterns](docs/integration/common-patterns.md)** - Best practices and reusable code
- **[Troubleshooting](docs/integration/troubleshooting.md)** - Common issues and solutions

### 🔧 **Advanced Topics**
- **[Performance Optimization Guide](docs/PERFORMANCE-OPTIMIZATION-GUIDE.md)** - ⭐ Complete guide: caching, batch processing, token optimization, benchmarking
- **[Cost Optimization Guide](docs/COST-OPTIMIZATION-GUIDE.md)** - ⭐ Reduce costs by 40-92%: smart routing, caching strategies, ROI calculations
- **[Provider Best Practices](docs/PROVIDER-BEST-PRACTICES.md)** - ⭐ OpenAI, Anthropic, Google: configuration, patterns, failover strategies
- **[Roadmap Implementation](docs/ROADMAP-IMPLEMENTATION.md)** - ⭐ Complete feature guide: backpressure, batch ops, semantic cache, memory management
- **[Security Implementation](docs/code-examples.md#security-features)** - Input validation, content filtering, PII detection
- **[Error Handling](docs/code-examples.md#error-handling)** - Resilience patterns, retry logic
- **[RAG Implementation](docs/code-examples.md#rag-retrieval-augmented-generation)** - Document indexing, vector search, context-aware responses

## 🛠️ Developer Tools

FluentAI.NET provides comprehensive developer tools for testing, debugging, and rapid prototyping:

### **CLI Tool** 🖥️
Interactive command-line interface for model testing and benchmarking.

```bash
cd Tools/FluentAI.CLI
dotnet run -- chat                    # Interactive chat
dotnet run -- benchmark               # Performance testing
dotnet run -- stream                  # Streaming visualization
dotnet run -- diagnostics test        # Connectivity testing
```

**Features:**
- Interactive chat with conversation history
- Performance benchmarking across models
- Real-time streaming visualization
- Configuration management and validation
- Error diagnostics and troubleshooting

[📖 CLI Tool Documentation](Tools/FluentAI.CLI/README.md)

### **Visual Debugging Dashboard** 📊
Real-time monitoring dashboard for AI operations.

```bash
cd Tools/FluentAI.Dashboard
dotnet run
# Navigate to https://localhost:5001
```

**Features:**
- Real-time token usage tracking
- Performance and cost metrics
- Cache hit/miss visualization
- Memory usage monitoring
- Auto-refresh dashboard (2s intervals)
- Test request interface

[📖 Dashboard Documentation](Tools/FluentAI.Dashboard/README.md)

### **Project Templates** 🚀
Ready-to-use templates for quick integration.

```bash
# Console Application
cd Templates/console
dotnet run

# ASP.NET Core Web API
cd Templates/webapi
dotnet run
# Navigate to https://localhost:5001/swagger
```

**Available Templates:**
- **Console App** - Basic chat with configuration management
- **Web API** - REST endpoints with Swagger documentation
- **Blazor App** - Coming soon
- **RAG-Enabled App** - Coming soon

[📖 Templates Documentation](Templates/README.md)

**Comprehensive Guide:** [Developer Tools Guide](docs/tools/developer-tools-guide.md)

## 🧪 Examples & Demos

### 🎮 **Interactive Console Demo**

Explore all SDK features with our comprehensive console application:

```bash
cd Examples/ConsoleApp
dotnet run
```

**Features Demonstrated:**
- 💬 Basic chat completion with multiple providers
- 🌊 Real-time streaming responses
- 🔄 Provider comparison and failover
- 🔒 Security features and input sanitization
- ⚡ Performance monitoring and caching
- ⚙️ Configuration management
- 🚨 Error handling and resilience patterns

### 📝 **Code Examples**

#### Simple Chat
```csharp
var messages = new[] { new ChatMessage(ChatRole.User, "Hello!") };
var response = await chatModel.GetResponseAsync(messages);
Console.WriteLine(response.Content);
```

#### Streaming Chat
```csharp
await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token);
}
```

#### Multiple Providers
```csharp
// Configuration-based provider switching
var openAIResponse = await openAIModel.GetResponseAsync(messages);
var anthropicResponse = await anthropicModel.GetResponseAsync(messages);

// Compare responses or use as fallback
```

## 🛠️ Integration Guides

### **Quick Integration Matrix**

| Project Type | Complexity | Setup Time | Guide |
|--------------|------------|------------|-------|
| Console App | ⭐ Simple | 5 minutes | [📖 Guide](docs/integration/console-applications.md) |
| ASP.NET Core | ⭐⭐ Medium | 15 minutes | [📖 Guide](docs/integration/aspnet-core.md) |
| Blazor Server | ⭐⭐ Medium | 20 minutes | [📖 Guide](docs/integration/blazor.md) |
| Blazor WASM | ⭐⭐⭐ Advanced | 30 minutes | [📖 Guide](docs/integration/blazor.md) |
| Class Library | ⭐ Simple | 10 minutes | [📖 Guide](docs/integration/class-libraries.md) |
| Azure Functions | ⭐⭐ Medium | 15 minutes | [📖 Guide](docs/integration/azure-functions.md) |

### **Configuration Patterns**

All integration guides include:
- ✅ Step-by-step setup instructions
- ✅ Complete working code examples
- ✅ Configuration best practices
- ✅ Security considerations
- ✅ Performance optimization
- ✅ Testing strategies
- ✅ Troubleshooting tips

## 🔐 Security

### **Built-in Security Features**

```csharp
// Input sanitization
var sanitizer = serviceProvider.GetRequiredService<IInputSanitizer>();
var safeContent = sanitizer.SanitizeContent(userInput);
var riskLevel = sanitizer.AssessRisk(userInput);

// Secure logging (automatic API key redaction)
_logger.LogInformation("Processing request from {UserId}", userId);
// API keys are automatically redacted from logs

// Content filtering
if (riskLevel.RiskLevel >= SecurityRiskLevel.High)
{
    throw new SecurityException("High-risk content detected");
}
```

### **Security Best Practices**

- 🔑 **API Key Management**: Environment variables, Azure Key Vault integration
- 🛡️ **Input Validation**: Prompt injection detection and prevention
- 🔍 **Content Filtering**: Configurable safety filters and risk assessment
- 📝 **Secure Logging**: Automatic redaction of sensitive information
- 🚫 **Rate Limiting**: Prevent abuse and DoS attacks
- 🔐 **Compliance**: GDPR, CCPA, SOC 2 compliance support

## ⚡ Performance

### **Performance Features**

- **Response Caching**: Intelligent caching with configurable TTL
- **Streaming Support**: Real-time token streaming for better UX
- **Memory Management**: Efficient memory usage and cleanup
- **Connection Pooling**: Optimized HTTP client management
- **Metrics Collection**: Built-in performance monitoring

### **Benchmarks**

| Feature | Performance | Memory Usage | Throughput |
|---------|-------------|--------------|------------|
| Basic Chat | ~500ms | 5MB | 100 req/min |
| Streaming | ~50ms TTFB | 3MB | 200 req/min |
| Cached Response | ~10ms | 2MB | 1000 req/min |
| Batch Processing | ~2s/10 req | 15MB | 300 req/min |

*Benchmarks vary based on provider, model, and network conditions.*

### **Performance Monitoring**

```csharp
// Built-in performance monitoring
var monitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();

using var operation = monitor.StartOperation("ChatCompletion");
var response = await chatModel.GetResponseAsync(messages);

// Automatic metrics collection
// - Request duration
// - Token usage
// - Success/failure rates
// - Memory usage
```

## 🧪 Testing

### **Test Suite Overview**

- **Comprehensive Test Suite** with 87% average coverage ([audit report](docs/AUDIT-EXECUTIVE-SUMMARY.md))
- **Unit Tests**: Fast, isolated tests for all components
- **Integration Tests**: Real provider testing with API keys
- **Performance Tests**: Benchmarking and load testing
- **Security Tests**: Vulnerability and penetration testing

**Coverage by Feature:**
- Core Chat: 90% ✅
- Security: 90% ✅
- RAG: 85-88% ⚠️
- Performance: 85% ⚠️
- MCP: 85% ⚠️
- Analysis: 82% ⚠️

### **Testing Your Integration**

```csharp
// Unit testing with mocks
[Test]
public async Task GetResponse_ShouldReturnExpectedContent()
{
    var mockChatModel = new Mock<IChatModel>();
    mockChatModel.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), null, default))
        .ReturnsAsync(new ChatResponse { Content = "Test response" });

    var service = new ChatService(mockChatModel.Object);
    var result = await service.GetResponseAsync("Test message");

    Assert.AreEqual("Test response", result);
}

// Integration testing
[Test, Category("Integration")]
public async Task RealProvider_ShouldWork()
{
    var services = new ServiceCollection();
    services.AddAiSdk(Configuration).AddOpenAiChatModel(Configuration);
    
    using var provider = services.BuildServiceProvider();
    var chatModel = provider.GetRequiredService<IChatModel>();
    
    var response = await chatModel.GetResponseAsync(testMessages);
    Assert.IsNotEmpty(response.Content);
}
```

### **Running Tests**

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter Category!=Integration

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run performance tests
dotnet test --filter Category=Performance
```

## 🤝 Contributing

We welcome contributions! See our [Contributing Guide](CONTRIBUTING.md) for:

- 🐛 **Bug Reports** - Help us identify and fix issues
- ✨ **Feature Requests** - Suggest new capabilities
- 📖 **Documentation** - Improve guides and examples
- 🧪 **Testing** - Add test coverage and scenarios
- 🔧 **Code Contributions** - Submit pull requests

### **Quick Start for Contributors**

```bash
# Fork and clone
git clone https://github.com/YOUR_USERNAME/fluentai-dotnet.git
cd fluentai-dotnet

# Build and test
dotnet restore
dotnet build
dotnet test

# Make your changes and submit a PR
```

**Development Requirements:**
- .NET 8.0 SDK
- API keys for testing (optional)
- IDE with C# support

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

**Key Points:**
- ✅ Commercial use allowed
- ✅ Modification and distribution allowed
- ✅ Private use allowed
- ❌ No warranty provided
- ❌ No liability assumed
