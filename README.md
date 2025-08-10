# FLUENTAI.NET - Universal AI SDK for .NET

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![NuGet](https://img.shields.io/nuget/v/FluentAI.NET.svg)](https://www.nuget.org/packages/FluentAI.NET/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

FluentAI.NET is a lightweight, provider-agnostic SDK that unifies access to multiple AI chat models under a single, clean API. Built for .NET developers who want to integrate AI capabilities without vendor lock-in or complex configuration.

## ✨ Key Features

✅ **Provider Agnostic** - Switch between OpenAI, Anthropic, Google, HuggingFace with one line  
✅ **Streaming Support** - Real-time token-by-token responses for interactive experiences  
✅ **Built for Scale** - Thread-safe, memory-efficient, with automatic retry logic  
✅ **DI Integration** - First-class support for ASP.NET Core and modern .NET patterns  
✅ **Extensible** - Add custom providers with minimal code  
✅ **Production Ready** - Comprehensive error handling, resource management, observability  

## 🚀 Supported Providers

- **OpenAI** (GPT-3.5, GPT-4, GPT-4o)
- **Anthropic** (Claude 3 Sonnet, Haiku, Opus)  
- **Google AI** (Gemini Pro, Gemini Flash) 
- **HuggingFace** (Transformers, Inference API)
- Extensible architecture for any HTTP-based AI service

## 📦 Installation

```bash
# Core package
dotnet add package FluentAI.NET

# Provider packages (install as needed)
dotnet add package FluentAI.NET.OpenAI
dotnet add package FluentAI.NET.Anthropic
```

## 🎯 Quick Start

### 1. Configure Services (ASP.NET Core)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add FluentAI with providers
builder.Services
    .AddFluentAI()
    .AddOpenAI(config => config.ApiKey = "your-openai-key")
    .AddAnthropic(config => config.ApiKey = "your-anthropic-key")
    .UseDefaultProvider("OpenAI");

var app = builder.Build();
```

### 2. Use in Your Code

```csharp
public class ChatController : ControllerBase
{
    private readonly IChatModel _chatModel;

    public ChatController(IChatModel chatModel)
    {
        _chatModel = chatModel;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] string message)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, message)
        };

        var response = await _chatModel.GetResponseAsync(messages);
        return Ok(response.Content);
    }

    [HttpPost("stream")]
    public async IAsyncEnumerable<string> StreamChat([FromBody] string message)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, message)
        };

        await foreach (var token in _chatModel.StreamResponseAsync(messages))
        {
            yield return token;
        }
    }
}
```

### 3. Configuration-Based Setup

```json
// appsettings.json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "ApiKey": "your-key-here",
    "Model": "gpt-4",
    "MaxTokens": 1000
  },
  "Anthropic": {
    "ApiKey": "your-key-here", 
    "Model": "claude-3-sonnet-20240229",
    "MaxTokens": 1000
  }
}
```

```csharp
// Program.cs
builder.Services
    .AddAiSdk(builder.Configuration)
    .AddOpenAiChatModel(builder.Configuration)
    .AddAnthropicChatModel(builder.Configuration);
```

## 🔧 Advanced Usage

### Provider-Specific Options

```csharp
// OpenAI with custom options
var response = await chatModel.GetResponseAsync(messages, new OpenAiRequestOptions
{
    Temperature = 0.7f,
    MaxTokens = 500
});

// Anthropic with system prompt
var response = await chatModel.GetResponseAsync(messages, new AnthropicRequestOptions
{
    SystemPrompt = "You are a helpful assistant.",
    Temperature = 0.5f
});
```

### Multiple Providers

```csharp
public class MultiProviderService
{
    private readonly IServiceProvider _serviceProvider;

    public MultiProviderService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<string> GetResponseFromProvider(string providerName, string message)
    {
        var chatModel = providerName.ToLower() switch
        {
            "openai" => _serviceProvider.GetRequiredService<OpenAiChatModel>(),
            "anthropic" => _serviceProvider.GetRequiredService<AnthropicChatModel>(),
            _ => throw new ArgumentException($"Provider {providerName} not supported")
        };

        var messages = new[] { new ChatMessage(ChatRole.User, message) };
        var response = await chatModel.GetResponseAsync(messages);
        return response.Content;
    }
}
```

### Error Handling

```csharp
try
{
    var response = await chatModel.GetResponseAsync(messages);
    return response.Content;
}
catch (AiSdkConfigurationException ex)
{
    // Configuration issues (missing API key, etc.)
    logger.LogError(ex, "Configuration error");
    throw;
}
catch (AiSdkException ex)
{
    // Provider-specific errors (rate limits, API errors)
    logger.LogError(ex, "AI service error");
    throw;
}
```

## 🏗️ Architecture

FluentAI.NET is built on a clean, extensible architecture:

- **`IChatModel`** - Core abstraction for all providers
- **`ChatModelBase`** - Base implementation with retry logic and validation
- **Provider Implementations** - OpenAI, Anthropic, and extensible for more
- **Configuration** - Strongly-typed options with validation
- **DI Extensions** - Fluent registration API

## 🔌 Extending with Custom Providers

```csharp
public class CustomChatModel : ChatModelBase
{
    public CustomChatModel(ILogger<CustomChatModel> logger) : base(logger) { }

    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, 
        ChatRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Implement your custom provider logic
        // Use base.ExecuteWithRetryAsync for retry logic
        // Use base.ValidateMessages for input validation
    }

    public override async IAsyncEnumerable<string> StreamResponseAsync(
        IEnumerable<ChatMessage> messages, 
        ChatRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Implement streaming logic
        yield return "token";
    }
}
```

## 📖 API Reference

### Core Types

- **`ChatMessage(ChatRole, string)`** - Represents a chat message
- **`ChatRole`** - User, Assistant, System
- **`ChatResponse`** - Complete response with usage info
- **`TokenUsage`** - Input/output token counts
- **`ChatRequestOptions`** - Base options for requests

### Provider Options

- **`OpenAiRequestOptions`** - Temperature, MaxTokens, etc.
- **`AnthropicRequestOptions`** - SystemPrompt, Temperature, etc.

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

The project includes comprehensive unit tests covering:
- Core abstractions and models
- Provider implementations  
- Configuration validation
- Error handling scenarios
- Retry logic

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 🆘 Support

- 📖 [Documentation](https://github.com/abxba0/fluentai-dotnet/wiki)
- 🐛 [Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- 💬 [Discussions](https://github.com/abxba0/fluentai-dotnet/discussions)

---

**FluentAI.NET** - Making AI integration in .NET simple, scalable, and vendor-agnostic.
