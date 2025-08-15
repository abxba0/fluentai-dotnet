# FluentAI.NET API Reference

This document provides comprehensive API reference for FluentAI.NET SDK.

## Table of Contents

- [Core Interfaces](#core-interfaces)
- [Models](#models)
- [Configuration](#configuration)
- [Providers](#providers)
- [Security](#security)
- [Performance](#performance)
- [Extensions](#extensions)
- [Exceptions](#exceptions)

## Core Interfaces

### IChatModel

The main interface for interacting with AI chat models.

```csharp
namespace FluentAI.Abstractions
{
    public interface IChatModel
    {
        Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages, 
            ChatRequestOptions? options = null, 
            CancellationToken cancellationToken = default);

        IAsyncEnumerable<string> StreamResponseAsync(
            IEnumerable<ChatMessage> messages, 
            ChatRequestOptions? options = null, 
            CancellationToken cancellationToken = default);
    }
}
```

#### Methods

##### GetResponseAsync

Gets a complete response from the AI model.

**Parameters:**
- `messages` (IEnumerable&lt;ChatMessage&gt;): The conversation messages
- `options` (ChatRequestOptions?, optional): Request configuration options
- `cancellationToken` (CancellationToken, optional): Cancellation token

**Returns:** Task&lt;ChatResponse&gt; - The complete AI response

**Exceptions:**
- `ArgumentNullException`: When messages is null
- `ArgumentException`: When messages is empty
- `AiSdkConfigurationException`: When configuration is invalid
- `AiSdkRateLimitException`: When rate limit is exceeded
- `AiSdkException`: For other AI service errors

**Example:**
```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.System, "You are a helpful assistant."),
    new ChatMessage(ChatRole.User, "What is the capital of France?")
};

var response = await chatModel.GetResponseAsync(messages);
Console.WriteLine(response.Content); // "The capital of France is Paris."
```

##### StreamResponseAsync

Streams the response token by token for real-time interaction.

**Parameters:**
- `messages` (IEnumerable&lt;ChatMessage&gt;): The conversation messages
- `options` (ChatRequestOptions?, optional): Request configuration options
- `cancellationToken` (CancellationToken, optional): Cancellation token

**Returns:** IAsyncEnumerable&lt;string&gt; - Stream of response tokens

**Example:**
```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.User, "Tell me a short story.")
};

await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token);
}
```

### IChatModelFactory

Factory interface for creating chat model instances.

```csharp
namespace FluentAI.Abstractions
{
    public interface IChatModelFactory
    {
        IChatModel CreateChatModel(string providerName);
        IChatModel CreateFailoverChatModel(string primaryProvider, string fallbackProvider);
    }
}
```

## Models

### ChatMessage

Represents a single message in a conversation.

```csharp
namespace FluentAI.Abstractions.Models
{
    public record ChatMessage
    {
        public ChatMessage(ChatRole role, string content);
        
        public ChatRole Role { get; init; }
        public string Content { get; init; }
    }
}
```

**Properties:**
- `Role` (ChatRole): The role of the message sender
- `Content` (string): The message content

**Example:**
```csharp
var userMessage = new ChatMessage(ChatRole.User, "Hello, AI!");
var systemMessage = new ChatMessage(ChatRole.System, "You are a helpful assistant.");
var assistantMessage = new ChatMessage(ChatRole.Assistant, "Hello! How can I help you today?");
```

### ChatRole

Enumeration of possible message roles.

```csharp
namespace FluentAI.Abstractions.Models
{
    public enum ChatRole
    {
        System = 0,
        User = 1,
        Assistant = 2
    }
}
```

**Values:**
- `System`: System prompt/instructions
- `User`: User messages
- `Assistant`: AI assistant responses

### ChatResponse

Represents a complete response from an AI model.

```csharp
namespace FluentAI.Abstractions.Models
{
    public record ChatResponse
    {
        public string Content { get; init; } = string.Empty;
        public string ModelId { get; init; } = string.Empty;
        public TokenUsage Usage { get; init; } = new();
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }
}
```

**Properties:**
- `Content` (string): The response text
- `ModelId` (string): ID of the model that generated the response
- `Usage` (TokenUsage): Token usage statistics
- `Timestamp` (DateTimeOffset): When the response was generated

### TokenUsage

Token usage statistics for a request/response.

```csharp
namespace FluentAI.Abstractions.Models
{
    public record TokenUsage
    {
        public int InputTokens { get; init; }
        public int OutputTokens { get; init; }
        public int TotalTokens { get; init; }
    }
}
```

**Properties:**
- `InputTokens` (int): Tokens used for input
- `OutputTokens` (int): Tokens used for output
- `TotalTokens` (int): Total tokens used

### ChatRequestOptions

Base class for request configuration options.

```csharp
namespace FluentAI.Abstractions.Models
{
    public abstract record ChatRequestOptions
    {
        public float? Temperature { get; init; }
        public int? MaxTokens { get; init; }
    }
}
```

**Properties:**
- `Temperature` (float?, optional): Controls randomness (0.0-2.0)
- `MaxTokens` (int?, optional): Maximum tokens to generate

## Configuration

### AiSdkOptions

Main SDK configuration options.

```csharp
namespace FluentAI.Configuration
{
    public class AiSdkOptions
    {
        public string? DefaultProvider { get; set; }
        public FailoverOptions? Failover { get; set; }
    }
}
```

**Properties:**
- `DefaultProvider` (string?): Name of the default provider to use
- `Failover` (FailoverOptions?): Failover configuration

### FailoverOptions

Configuration for provider failover.

```csharp
namespace FluentAI.Configuration
{
    public class FailoverOptions
    {
        public string PrimaryProvider { get; set; } = string.Empty;
        public string FallbackProvider { get; set; } = string.Empty;
    }
}
```

### Provider-Specific Options

#### OpenAiOptions

```csharp
namespace FluentAI.Configuration
{
    public class OpenAiOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gpt-3.5-turbo";
        public int MaxTokens { get; set; } = 1000;
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public int? PermitLimit { get; set; }
        public int WindowInSeconds { get; set; } = 60;
    }
}
```

#### AnthropicOptions

```csharp
namespace FluentAI.Configuration
{
    public class AnthropicOptions
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "claude-3-haiku-20240307";
        public int MaxTokens { get; set; } = 1000;
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public int? PermitLimit { get; set; }
        public int WindowInSeconds { get; set; } = 60;
    }
}
```

#### Provider-Specific Request Options

##### OpenAiRequestOptions

```csharp
namespace FluentAI.Configuration
{
    public record OpenAiRequestOptions : ChatRequestOptions
    {
        public float? TopP { get; init; }
        public float? FrequencyPenalty { get; init; }
        public float? PresencePenalty { get; init; }
        public string[]? Stop { get; init; }
    }
}
```

##### AnthropicRequestOptions

```csharp
namespace FluentAI.Configuration
{
    public record AnthropicRequestOptions : ChatRequestOptions
    {
        public string? SystemPrompt { get; init; }
    }
}
```

## Security

### IInputSanitizer

Interface for input sanitization and security validation.

```csharp
namespace FluentAI.Abstractions.Security
{
    public interface IInputSanitizer
    {
        string SanitizeContent(string content);
        bool IsContentSafe(string content);
        SecurityRiskAssessment AssessRisk(string content);
    }
}
```

#### Methods

##### SanitizeContent

Sanitizes input content to remove potentially dangerous elements.

**Parameters:**
- `content` (string): Content to sanitize

**Returns:** string - Sanitized content

##### IsContentSafe

Determines if content is safe for processing.

**Parameters:**
- `content` (string): Content to check

**Returns:** bool - True if content is safe

##### AssessRisk

Performs detailed risk assessment of content.

**Parameters:**
- `content` (string): Content to assess

**Returns:** SecurityRiskAssessment - Detailed risk assessment

### SecurityRiskAssessment

Results of security risk assessment.

```csharp
namespace FluentAI.Abstractions.Security
{
    public record SecurityRiskAssessment
    {
        public SecurityRiskLevel RiskLevel { get; init; }
        public IReadOnlyList<string> DetectedConcerns { get; init; } = Array.Empty<string>();
        public string? AdditionalInfo { get; init; }
        public bool ShouldBlock => RiskLevel >= SecurityRiskLevel.High;
    }
}
```

### SecurityRiskLevel

Enumeration of security risk levels.

```csharp
namespace FluentAI.Abstractions.Security
{
    public enum SecurityRiskLevel
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}
```

## Performance

### IPerformanceMonitor

Interface for performance monitoring and metrics collection.

```csharp
namespace FluentAI.Abstractions.Performance
{
    public interface IPerformanceMonitor
    {
        IDisposable StartOperation(string operationName);
        void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null);
        void IncrementCounter(string counterName, int increment = 1, Dictionary<string, string>? tags = null);
        OperationStats? GetOperationStats(string operationName);
    }
}
```

### IResponseCache

Interface for caching AI responses.

```csharp
namespace FluentAI.Abstractions.Performance
{
    public interface IResponseCache
    {
        Task<ChatResponse?> GetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null);
        Task SetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options, ChatResponse response, TimeSpan? ttl = null);
        Task CleanupExpiredEntriesAsync();
    }
}
```

### OperationStats

Performance statistics for operations.

```csharp
namespace FluentAI.Abstractions.Performance
{
    public record OperationStats
    {
        public string OperationName { get; init; } = string.Empty;
        public long ExecutionCount { get; init; }
        public double AverageExecutionTimeMs { get; init; }
        public double MinExecutionTimeMs { get; init; }
        public double MaxExecutionTimeMs { get; init; }
        public double TotalExecutionTimeMs { get; init; }
        public DateTimeOffset FirstExecution { get; init; }
        public DateTimeOffset LastExecution { get; init; }
        public long FailedExecutions { get; init; }
        public double SuccessRate => ExecutionCount > 0 ? ((double)(ExecutionCount - FailedExecutions) / ExecutionCount) * 100.0 : 0.0;
    }
}
```

## Extensions

### ServiceCollectionExtensions

Extension methods for dependency injection setup.

```csharp
namespace FluentAI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IFluentAiBuilder AddFluentAI(this IServiceCollection services);
        public static IServiceCollection AddAiSdk(this IServiceCollection services, IConfiguration configuration);
        public static IServiceCollection AddOpenAiChatModel(this IServiceCollection services, IConfiguration configuration);
        public static IServiceCollection AddAnthropicChatModel(this IServiceCollection services, IConfiguration configuration);
        public static IServiceCollection AddGoogleGeminiChatModel(this IServiceCollection services, IConfiguration configuration);
    }
}
```

### IFluentAiBuilder

Fluent builder interface for service configuration.

```csharp
namespace FluentAI.Extensions
{
    public interface IFluentAiBuilder
    {
        IFluentAiBuilder AddOpenAI(Action<OpenAiOptions> configure);
        IFluentAiBuilder AddAnthropic(Action<AnthropicOptions> configure);
        IFluentAiBuilder AddGoogle(Action<GoogleOptions> configure);
        IFluentAiBuilder UseDefaultProvider(string providerName);
    }
}
```

**Example:**
```csharp
services.AddFluentAI()
    .AddOpenAI(config =>
    {
        config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        config.Model = "gpt-4";
        config.MaxTokens = 2000;
    })
    .AddAnthropic(config =>
    {
        config.ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        config.Model = "claude-3-sonnet-20240229";
    })
    .UseDefaultProvider("OpenAI");
```

## Exceptions

### AiSdkException

Base exception for all FluentAI.NET exceptions.

```csharp
namespace FluentAI.Abstractions.Exceptions
{
    public class AiSdkException : Exception
    {
        public AiSdkException(string message);
        public AiSdkException(string message, Exception innerException);
    }
}
```

### AiSdkConfigurationException

Exception thrown for configuration errors.

```csharp
namespace FluentAI.Abstractions.Exceptions
{
    public class AiSdkConfigurationException : AiSdkException
    {
        public AiSdkConfigurationException(string message);
        public AiSdkConfigurationException(string message, Exception innerException);
    }
}
```

**Common scenarios:**
- Missing API keys
- Invalid provider names
- Invalid configuration values

### AiSdkRateLimitException

Exception thrown when rate limits are exceeded.

```csharp
namespace FluentAI.Abstractions.Exceptions
{
    public class AiSdkRateLimitException : AiSdkException
    {
        public AiSdkRateLimitException(string message);
        public AiSdkRateLimitException(string message, Exception innerException);
    }
}
```

## Usage Examples

### Basic Usage

```csharp
// Setup
var services = new ServiceCollection();
services.AddAiSdk(configuration)
    .AddOpenAiChatModel(configuration);

var serviceProvider = services.BuildServiceProvider();
var chatModel = serviceProvider.GetRequiredService<IChatModel>();

// Simple request
var messages = new[]
{
    new ChatMessage(ChatRole.User, "Hello!")
};

var response = await chatModel.GetResponseAsync(messages);
Console.WriteLine(response.Content);
```

### Advanced Usage with Options

```csharp
// Custom request options
var options = new OpenAiRequestOptions
{
    Temperature = 0.7f,
    MaxTokens = 500,
    TopP = 0.9f
};

var response = await chatModel.GetResponseAsync(messages, options);
```

### Streaming Response

```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.User, "Tell me a story")
};

Console.Write("AI: ");
await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token);
}
Console.WriteLine();
```

### Error Handling

```csharp
try
{
    var response = await chatModel.GetResponseAsync(messages);
    return response.Content;
}
catch (AiSdkRateLimitException ex)
{
    // Handle rate limiting
    await Task.Delay(TimeSpan.FromSeconds(60));
    // Retry logic here
}
catch (AiSdkConfigurationException ex)
{
    // Handle configuration issues
    logger.LogError(ex, "Configuration error");
    throw;
}
catch (AiSdkException ex)
{
    // Handle other AI service errors
    logger.LogError(ex, "AI service error");
    return "Sorry, I'm having trouble right now.";
}
```

### Multiple Providers with Failover

```csharp
// Configuration
{
  "AiSdk": {
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  }
}

// The SDK automatically handles failover
var response = await chatModel.GetResponseAsync(messages);
// Uses OpenAI first, falls back to Anthropic if OpenAI fails
```

## Best Practices

### 1. Configuration Management

```csharp
// Use environment variables for API keys
services.AddAiSdk(configuration)
    .AddOpenAiChatModel(configuration);

// appsettings.json
{
  "OpenAI": {
    "ApiKey": "", // Leave empty, use environment variable
    "Model": "gpt-3.5-turbo"
  }
}

// Environment variable
OPENAI_API_KEY=your-actual-api-key
```

### 2. Resource Management

```csharp
// Dispose of services properly
await using var serviceProvider = services.BuildServiceProvider();
var chatModel = serviceProvider.GetRequiredService<IChatModel>();

// Use cancellation tokens
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
var response = await chatModel.GetResponseAsync(messages, cancellationToken: cts.Token);
```

### 3. Error Handling and Resilience

```csharp
// Implement retry logic
var retryPolicy = Policy
    .Handle<AiSdkException>()
    .WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

var response = await retryPolicy.ExecuteAsync(async () =>
    await chatModel.GetResponseAsync(messages));
```

### 4. Performance Optimization

```csharp
// Use streaming for long responses
if (expectedLongResponse)
{
    await foreach (var token in chatModel.StreamResponseAsync(messages))
    {
        // Process tokens as they arrive
        await ProcessTokenAsync(token);
    }
}

// Implement caching for repeated requests
var cacheKey = GenerateCacheKey(messages);
var cachedResponse = await cache.GetAsync(cacheKey);
if (cachedResponse != null)
    return cachedResponse;

var response = await chatModel.GetResponseAsync(messages);
await cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(30));
```

---

For more examples and detailed guides, see the [Integration Documentation](docs/integration/) and [Examples](Examples/).