# Console Applications Integration

This guide shows how to integrate FluentAI.NET into console applications with dependency injection and configuration.

## Basic Setup

### 1. Create Project and Install Package

```bash
dotnet new console -n MyAIConsoleApp
cd MyAIConsoleApp
dotnet add package FluentAI.NET
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Microsoft.Extensions.Configuration.Json
```

### 2. Add Configuration File

Create `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FluentAI": "Debug"
    }
  },
  "AiSdk": {
    "DefaultProvider": "OpenAI",
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  },
  "OpenAI": {
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 1000,
    "RequestTimeout": "00:01:30",
    "PermitLimit": 50,
    "WindowInSeconds": 60
  },
  "Anthropic": {
    "Model": "claude-3-haiku-20240307",
    "MaxTokens": 1000,
    "RequestTimeout": "00:01:30",
    "PermitLimit": 30,
    "WindowInSeconds": 60
  }
}
```

### 3. Update Program.cs

```csharp
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MyAIConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // Create host builder with dependency injection
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register FluentAI services
                services.AddAiSdk(context.Configuration)
                    .AddOpenAiChatModel(context.Configuration)
                    .AddAnthropicChatModel(context.Configuration);
                
                // Register your application services
                services.AddTransient<ChatService>();
            });

        using var host = builder.Build();
        
        try
        {
            var chatService = host.Services.GetRequiredService<ChatService>();
            await chatService.RunAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

public class ChatService
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatModel chatModel, ILogger<ChatService> logger)
    {
        _chatModel = chatModel;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("AI Console App - Type 'exit' to quit");
        
        while (true)
        {
            Console.Write("You: ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "exit")
                break;

            try
            {
                var messages = new[] { new ChatMessage(ChatRole.User, input) };
                var response = await _chatModel.GetResponseAsync(messages);
                
                Console.WriteLine($"AI: {response.Content}");
                Console.WriteLine($"[Model: {response.ModelId}, Tokens: {response.Usage.TotalTokens}]");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI response");
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
    }
}
```

## Advanced Features

### Environment Variables

Set API keys as environment variables (recommended for security):

```bash
# Windows
set OPENAI_API_KEY=your-openai-key
set ANTHROPIC_API_KEY=your-anthropic-key

# Linux/macOS
export OPENAI_API_KEY="your-openai-key"
export ANTHROPIC_API_KEY="your-anthropic-key"
```

### Streaming Responses

```csharp
public async Task StreamChatAsync(string message)
{
    var messages = new[] { new ChatMessage(ChatRole.User, message) };
    
    Console.Write("AI (streaming): ");
    await foreach (var token in _chatModel.StreamResponseAsync(messages))
    {
        Console.Write(token);
    }
    Console.WriteLine();
}
```

### Error Handling

```csharp
public async Task<string> GetResponseWithRetryAsync(string message)
{
    var messages = new[] { new ChatMessage(ChatRole.User, message) };
    var maxRetries = 3;
    var retryDelay = TimeSpan.FromSeconds(1);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            var response = await _chatModel.GetResponseAsync(messages);
            return response.Content;
        }
        catch (AiSdkRateLimitException ex)
        {
            _logger.LogWarning("Rate limit hit, attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
            
            if (attempt == maxRetries)
                throw;
                
            await Task.Delay(retryDelay * attempt); // Exponential backoff
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on attempt {Attempt}", attempt);
            
            if (attempt == maxRetries)
                throw;
        }
    }
    
    throw new InvalidOperationException("All retry attempts failed");
}
```

### Configuration Validation

```csharp
public class ChatService
{
    public async Task ValidateConfigurationAsync()
    {
        try
        {
            // Test with a simple message
            var testMessages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var response = await _chatModel.GetResponseAsync(testMessages);
            
            Console.WriteLine("✅ Configuration is valid");
            Console.WriteLine($"Connected to: {response.ModelId}");
        }
        catch (AiSdkConfigurationException ex)
        {
            Console.WriteLine($"❌ Configuration error: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Connection error: {ex.Message}");
            throw;
        }
    }
}
```

## Project Structure

```
MyAIConsoleApp/
├── Program.cs
├── appsettings.json
├── Services/
│   ├── ChatService.cs
│   └── ValidationService.cs
├── Models/
│   └── ChatHistory.cs
└── MyAIConsoleApp.csproj
```

## Best Practices

### 1. Configuration Management
- Store API keys in environment variables
- Use different configurations for development/production
- Validate configuration at startup

### 2. Error Handling
- Implement retry logic for transient failures
- Log errors appropriately
- Provide meaningful error messages to users

### 3. Performance
- Use streaming for long responses
- Implement caching for repeated requests
- Monitor token usage

### 4. Security
- Never commit API keys to source control
- Use input sanitization for user content
- Implement rate limiting

## Common Issues

### Missing API Keys
```
AiSdkConfigurationException: OpenAI API key is not configured
```
**Solution**: Set the `OPENAI_API_KEY` environment variable or configure it in appsettings.json

### Rate Limiting
```
AiSdkRateLimitException: Rate limit exceeded
```
**Solution**: Implement retry logic with exponential backoff or adjust rate limits in configuration

### Network Issues
```
HttpRequestException: Unable to connect to the remote server
```
**Solution**: Check internet connection and firewall settings

## Complete Example

See the [FluentAI Console Example](../../Examples/ConsoleApp/) for a comprehensive demonstration of all features.

## Next Steps

- Explore [ASP.NET Core integration](aspnet-core.md) for web applications
- Learn about [performance optimization](common-patterns.md#performance-optimization)
- Check the [troubleshooting guide](troubleshooting.md) for common issues