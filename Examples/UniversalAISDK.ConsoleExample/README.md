# Universal AI SDK Console Example

## Overview

This comprehensive console application demonstrates all major features of the Universal AI SDK for .NET. It provides educational examples with detailed comments, showing how to integrate AI capabilities into your .NET applications using industry best practices.

Note: Currently uses a mock implementation to work around SDK dependency injection issues.

## Features Demonstrated

### ✅ Core Features
- **Multi-provider setup** - OpenAI, Anthropic, Google, HuggingFace configuration
- **Basic chat completion** - Standard request/response patterns
- **Streaming responses** - Real-time token-by-token output
- **Message construction** - System, user, and assistant message types
- **Token usage tracking** - Monitor input/output tokens and costs

### ✅ Advanced Features
- **Error handling** - Comprehensive exception handling patterns
- **Configuration-based setup** - appsettings.json integration
- **Rate limiting** - API quota management
- **Provider failover** - Automatic fallback between providers
- **Input sanitization** - Security best practices
- **Performance monitoring** - Request timing and metrics (concept)
- **Response caching** - Efficiency optimization (concept)

### ✅ Production Features
- **Dependency injection** - ASP.NET Core compatible DI setup
- **Logging integration** - Microsoft.Extensions.Logging support
- **Environment variables** - Secure API key management
- **Incremental examples** - Commented sections for selective testing

## Requirements

- **.NET 8.0** or later
- **API Keys** (optional for basic examples):
  - OpenAI API key
  - Anthropic API key
  - Google AI API key (optional)
  - HuggingFace API key (optional)

## Setup

### 1. Navigate to the Project Directory
```bash
cd Examples/UniversalAISDK.ConsoleExample
```

### 2. Set API Keys (Optional)
Set your API keys as environment variables:

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=your-actual-openai-api-key
set ANTHROPIC_API_KEY=your-actual-anthropic-api-key
```

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="your-actual-openai-api-key"
$env:ANTHROPIC_API_KEY="your-actual-anthropic-api-key"
```

**macOS/Linux:**
```bash
export OPENAI_API_KEY="your-actual-openai-api-key"
export ANTHROPIC_API_KEY="your-actual-anthropic-api-key"
```

### 3. Restore Dependencies and Run
```bash
dotnet restore
dotnet run
```

## How to Implement in a New Project

### Step 1: Install the Package
```bash
dotnet add package FluentAI.NET
```

### Step 2: Add to Your Program.cs
```csharp
using FluentAI.Abstractions;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add Universal AI SDK
        services.AddFluentAI()
            .AddOpenAI(config =>
            {
                config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                config.Model = "gpt-3.5-turbo";
                config.MaxTokens = 500;
            })
            .UseDefaultProvider("OpenAI");
    });

using var host = builder.Build();
```

### Step 3: Use in Your Services
```csharp
public class ChatService
{
    private readonly IChatModel _chatModel;

    public ChatService(IChatModel chatModel)
    {
        _chatModel = chatModel;
    }

    public async Task<string> GetAIResponse(string userMessage)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, userMessage)
        };

        var response = await _chatModel.GetResponseAsync(messages);
        return response.Content;
    }
}
```

### Step 4: Configuration (appsettings.json)
```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 500,
    "PermitLimit": 100,
    "WindowInSeconds": 60
  }
}
```

## Code Examples by Feature

### Basic Chat Completion
```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant."),
    new ChatMessage(ChatRole.User, "What is the capital of France?")
};

var response = await chatModel.GetResponseAsync(messages);
Console.WriteLine(response.Content); // "The capital of France is Paris."
```

### Streaming Responses
```csharp
await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token); // Real-time output
}
```

### Multiple Providers with Failover
```csharp
services.AddFluentAI()
    .AddOpenAI(config => { /* OpenAI config */ })
    .AddAnthropic(config => { /* Anthropic config */ })
    .UseDefaultProvider("OpenAI");

// Configure failover in appsettings.json:
{
  "AiSdk": {
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  }
}
```

### Error Handling
```csharp
try
{
    var response = await chatModel.GetResponseAsync(messages);
}
catch (AiSdkConfigurationException ex)
{
    // Handle configuration issues
}
catch (AiSdkRateLimitException ex)
{
    // Handle rate limiting
}
catch (AiSdkException ex)
{
    // Handle AI service errors
}
```

### Rate Limiting
```csharp
// In appsettings.json:
{
  "OpenAI": {
    "PermitLimit": 100,      // Max requests
    "WindowInSeconds": 60    // Time window
  }
}
```

### Performance Monitoring
```csharp
// Register performance monitoring
services.AddSingleton<IPerformanceMonitor, DefaultPerformanceMonitor>();

// Monitor operations
using var operation = performanceMonitor.StartOperation("chat-completion");
var response = await chatModel.GetResponseAsync(messages);
// Automatically tracked
```

### Response Caching
```csharp
// Register caching
services.AddSingleton<IResponseCache, MemoryResponseCache>();

// Automatic caching based on message content
var response = await chatModel.GetResponseAsync(messages); // First call
var cachedResponse = await chatModel.GetResponseAsync(messages); // Returns cached result
```

## Example Output

```
🤖 Universal AI SDK for .NET - Comprehensive Example
=====================================================

📋 Example 1: Basic Configuration and Initialization
──────────────────────────────────────────────────
✓ SDK initialized with dependency injection
✓ Multiple providers configured (OpenAI, Anthropic)
✓ Default provider set via configuration
✓ Logging and configuration integrated

💬 Example 2: Message Construction and Validation
──────────────────────────────────────────────────
Created message conversation:
  System: You are a helpful AI assistant.
  User: What is the capital of France?
  Assistant: The capital of France is Paris.

🤖 Example 3: Basic Chat Completion
──────────────────────────────────────────────────
Sending request to AI model...
✓ Response received: 2 + 2 equals 4.
✓ Model: gpt-3.5-turbo
✓ Tokens: 15 input → 8 output

📡 Example 4: Streaming Response Handling
──────────────────────────────────────────────────
Streaming response: 1... 2... 3... 4... 5!
✓ Full response received: 15 characters

📊 Example 6: Token Usage Tracking
──────────────────────────────────────────────────
Token Usage Analysis:
  Input tokens:  22
  Output tokens: 28
  Total tokens:  50
  Efficiency:    56.0% (output/total)
  Est. cost:     $0.0001 (example rates)

🔒 Example 9: Input Sanitization and Security
──────────────────────────────────────────────────
Input sanitization examples:
  Input: Normal user input
    Long input: False
    Script tags: False
    SQL keywords: False
    Recommendation: SAFE

  Input: <script>alert('xss')</script>
    Long input: False
    Script tags: True
    SQL keywords: False
    Recommendation: SANITIZE

⚙️  Example 10: Configuration-Based Setup
──────────────────────────────────────────────────
Configuration values loaded from appsettings.json:
  Default provider: OpenAI
  OpenAI model: gpt-3.5-turbo
  Max tokens: 500
  Rate limit: 10 requests per 60 seconds
  Failover: OpenAI → Anthropic

✅ All examples completed successfully!
```

## Advanced Configuration

### Environment-Specific Settings
Create environment-specific configuration files:
- `appsettings.Development.json`
- `appsettings.Production.json`

### Custom Provider Configuration
```csharp
services.AddFluentAI()
    .AddOpenAI(config =>
    {
        config.ApiKey = configuration["OpenAI:ApiKey"];
        config.Model = "gpt-4";
        config.MaxTokens = 1000;
        config.Temperature = 0.7f;
    })
    .AddAnthropic(config =>
    {
        config.ApiKey = configuration["Anthropic:ApiKey"];
        config.Model = "claude-3-sonnet-20240229";
        config.MaxTokens = 1000;
        config.SystemPrompt = "You are an expert assistant.";
    });
```

### Production Considerations
- **Security**: Never commit API keys to source control
- **Rate Limiting**: Configure appropriate limits for your use case
- **Failover**: Set up multiple providers for high availability
- **Monitoring**: Implement performance and error monitoring
- **Caching**: Use response caching for repeated queries
- **Logging**: Configure appropriate log levels for production

## Troubleshooting

### Common Issues

**"API key not found" errors:**
- Ensure environment variables are set correctly
- Check that variable names match exactly
- Restart your terminal/IDE after setting environment variables

**Rate limiting errors:**
- Reduce `PermitLimit` in configuration
- Increase `WindowInSeconds` for longer rate limit windows
- Implement exponential backoff in your application

**Provider connection errors:**
- Verify API keys are valid and active
- Check network connectivity
- Ensure the correct model names are configured

**Build errors:**
- Ensure you're using .NET 8.0 or later
- Run `dotnet restore` to restore packages
- Check that project references are correct

### Current SDK Issues

⚠️  **Dependency Injection Architecture Issue**: The Universal AI SDK currently has a design flaw in its dependency injection system:

**Problem**: The `ChatModelBase` constructor attempts to cast `ILogger<TProvider>` to `ILogger<DefaultInputSanitizer>`, which fails at runtime.

**Root Cause**: 
```csharp
// In ChatModelBase constructor - this fails
InputSanitizer = inputSanitizer ?? new DefaultInputSanitizer(
    logger as ILogger<DefaultInputSanitizer> ?? throw new InvalidOperationException(...)
);
```

**Resolution Needed**: The SDK maintainers should:
1. Update provider constructors to accept `IInputSanitizer` and `IPerformanceMonitor` as dependencies
2. Register these services properly in the FluentAI builder
3. Remove the problematic logger casting logic

**Current Workaround**: This example uses a mock implementation that demonstrates the correct API patterns while avoiding the DI issues.

## Next Steps

1. **Explore the source code** - Review `Program.cs` for detailed implementation
2. **Uncomment optional providers** - Enable Google, HuggingFace if you have API keys
3. **Add performance monitoring** - Register `IPerformanceMonitor` in DI
4. **Implement caching** - Register `IResponseCache` for improved performance
5. **Build your own application** - Use this example as a starting point

## Support

- 📖 [SDK Documentation](https://github.com/abxba0/fluentai-dotnet)
- 🐛 [Report Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- 💬 [Discussions](https://github.com/abxba0/fluentai-dotnet/discussions)

---

**Universal AI SDK for .NET** - Making AI integration simple, scalable, and vendor-agnostic.