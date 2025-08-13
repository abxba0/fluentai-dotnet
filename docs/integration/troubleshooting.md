# Troubleshooting Guide

This guide helps you diagnose and resolve common issues when using FluentAI.NET.

## Common Issues

### Configuration Issues

#### Issue: "OpenAI API key is not configured"

**Error Message:**
```
AiSdkConfigurationException: OpenAI API key is not configured
```

**Causes & Solutions:**

1. **Missing Environment Variable**
   ```bash
   # Windows
   set OPENAI_API_KEY=your-actual-api-key
   
   # Linux/macOS
   export OPENAI_API_KEY="your-actual-api-key"
   ```

2. **Missing appsettings.json Configuration**
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-actual-api-key"
     }
   }
   ```

3. **Configuration Not Loaded**
   ```csharp
   // Ensure configuration is properly loaded
   services.AddAiSdk(Configuration) // Pass IConfiguration
       .AddOpenAiChatModel(Configuration);
   ```

#### Issue: "Default provider not found"

**Error Message:**
```
AiSdkConfigurationException: Default provider 'OpenAI' not found
```

**Solution:**
```csharp
// Ensure the provider is registered
services.AddAiSdk(Configuration)
    .AddOpenAiChatModel(Configuration); // This must be called

// Or check your configuration
{
  "AiSdk": {
    "DefaultProvider": "OpenAI" // Must match registered provider
  }
}
```

#### Issue: "Invalid model name"

**Error Message:**
```
HttpRequestException: The model 'invalid-model' does not exist
```

**Solution:**
```json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo", // Use valid model names
    // Valid: gpt-3.5-turbo, gpt-4, gpt-4-turbo-preview
  },
  "Anthropic": {
    "Model": "claude-3-haiku-20240307", // Use valid model names
    // Valid: claude-3-haiku-20240307, claude-3-sonnet-20240229
  }
}
```

### Network and Connectivity Issues

#### Issue: "Unable to connect to the remote server"

**Error Message:**
```
HttpRequestException: Unable to connect to the remote server
```

**Diagnostic Steps:**

1. **Check Internet Connection**
   ```bash
   # Test connectivity to OpenAI
   curl -I https://api.openai.com/v1/models
   
   # Test connectivity to Anthropic
   curl -I https://api.anthropic.com/v1/messages
   ```

2. **Check Firewall/Proxy Settings**
   ```csharp
   // Configure HTTP client for proxy
   services.AddHttpClient<OpenAiChatModel>(client =>
   {
       client.Timeout = TimeSpan.FromMinutes(2);
   }).ConfigurePrimaryHttpMessageHandler(() =>
   {
       return new HttpClientHandler
       {
           Proxy = new WebProxy("http://proxy.company.com:8080"),
           UseProxy = true
       };
   });
   ```

3. **Increase Timeout**
   ```json
   {
     "OpenAI": {
       "RequestTimeout": "00:03:00" // 3 minutes
     }
   }
   ```

#### Issue: "The request was canceled due to the configured HttpClient.Timeout"

**Error Message:**
```
TaskCanceledException: The request was canceled due to the configured HttpClient.Timeout
```

**Solutions:**

1. **Increase Timeout in Configuration**
   ```json
   {
     "OpenAI": {
       "RequestTimeout": "00:05:00" // 5 minutes for longer responses
     }
   }
   ```

2. **Use Streaming for Long Responses**
   ```csharp
   // Use streaming instead of waiting for complete response
   await foreach (var token in chatModel.StreamResponseAsync(messages))
   {
       Console.Write(token);
   }
   ```

### Rate Limiting Issues

#### Issue: "Rate limit exceeded"

**Error Message:**
```
AiSdkRateLimitException: Rate limit exceeded
```

**Solutions:**

1. **Configure Rate Limiting**
   ```json
   {
     "OpenAI": {
       "PermitLimit": 50,       // Requests per window
       "WindowInSeconds": 60    // Time window
     }
   }
   ```

2. **Implement Retry Logic**
   ```csharp
   public async Task<ChatResponse> GetResponseWithRetry(IEnumerable<ChatMessage> messages)
   {
       var maxRetries = 3;
       var baseDelay = TimeSpan.FromSeconds(1);
   
       for (int attempt = 1; attempt <= maxRetries; attempt++)
       {
           try
           {
               return await _chatModel.GetResponseAsync(messages);
           }
           catch (AiSdkRateLimitException ex)
           {
               if (attempt == maxRetries)
                   throw;
   
               var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
               await Task.Delay(delay);
           }
       }
   
       throw new InvalidOperationException("All retry attempts failed");
   }
   ```

3. **Use Different Rate Limits per Provider**
   ```json
   {
     "OpenAI": {
       "PermitLimit": 100,
       "WindowInSeconds": 60
     },
     "Anthropic": {
       "PermitLimit": 50,
       "WindowInSeconds": 60
     }
   }
   ```

### Authentication Issues

#### Issue: "Invalid API key"

**Error Message:**
```
HttpRequestException: 401 Unauthorized
```

**Diagnostic Steps:**

1. **Verify API Key Format**
   ```csharp
   // OpenAI keys start with 'sk-'
   // Anthropic keys start with 'sk-ant-'
   
   public static bool IsValidOpenAIKey(string key)
   {
       return !string.IsNullOrEmpty(key) && key.StartsWith("sk-") && key.Length > 20;
   }
   ```

2. **Test API Key Manually**
   ```bash
   # Test OpenAI key
   curl https://api.openai.com/v1/models \
     -H "Authorization: Bearer YOUR_API_KEY"
   
   # Test Anthropic key
   curl https://api.anthropic.com/v1/messages \
     -H "x-api-key: YOUR_API_KEY" \
     -H "Content-Type: application/json" \
     --data '{"model":"claude-3-haiku-20240307","max_tokens":10,"messages":[{"role":"user","content":"Hi"}]}'
   ```

3. **Check API Key Permissions**
   - Ensure the API key has necessary permissions
   - Check if the key is active and not suspended
   - Verify billing/usage limits

### Dependency Injection Issues

#### Issue: "Unable to resolve service"

**Error Message:**
```
InvalidOperationException: Unable to resolve service for type 'FluentAI.Abstractions.IChatModel'
```

**Solution:**
```csharp
// Ensure services are registered in correct order
public void ConfigureServices(IServiceCollection services)
{
    // 1. Add configuration
    services.AddAiSdk(Configuration);
    
    // 2. Add providers
    services.AddOpenAiChatModel(Configuration);
    services.AddAnthropicChatModel(Configuration);
    
    // 3. Add your services
    services.AddTransient<MyChatService>();
}
```

#### Issue: "Circular dependency detected"

**Error Message:**
```
InvalidOperationException: A circular dependency was detected
```

**Solution:**
```csharp
// Use factory pattern to resolve circular dependencies
public interface IChatServiceFactory
{
    IChatService CreateChatService();
}

public class ChatServiceFactory : IChatServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ChatServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public IChatService CreateChatService()
    {
        return _serviceProvider.GetRequiredService<IChatService>();
    }
}
```

### Memory and Performance Issues

#### Issue: "OutOfMemoryException"

**Causes & Solutions:**

1. **Large Conversation History**
   ```csharp
   public class ConversationManager
   {
       private const int MaxMessages = 50; // Limit conversation length
       
       public void AddMessage(ChatMessage message)
       {
           _messages.Add(message);
           
           // Keep only recent messages
           if (_messages.Count > MaxMessages)
           {
               _messages.RemoveRange(0, _messages.Count - MaxMessages);
           }
       }
   }
   ```

2. **Memory Leaks in Streaming**
   ```csharp
   public async Task ProcessStreamingResponse()
   {
       var response = new StringBuilder();
       
       await foreach (var token in _chatModel.StreamResponseAsync(messages))
       {
           response.Append(token);
           
           // Limit response size
           if (response.Length > 50000)
           {
               break;
           }
       }
   }
   ```

#### Issue: "High Memory Usage"

**Diagnostic Tools:**
```csharp
public class MemoryDiagnostics
{
    public static void LogMemoryUsage(ILogger logger)
    {
        var totalMemory = GC.GetTotalMemory(false);
        var workingSet = Environment.WorkingSet;
        
        logger.LogInformation("Memory usage: {TotalMemory:N0} bytes, Working set: {WorkingSet:N0} bytes",
            totalMemory, workingSet);
    }
    
    public static void ForceGarbageCollection()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}
```

### Deployment Issues

#### Issue: "Service starts but API calls fail in production"

**Common Causes:**

1. **Missing Environment Variables**
   ```bash
   # Check if environment variables are set
   docker exec -it container-name env | grep API_KEY
   ```

2. **Incorrect Configuration Section Names**
   ```json
   // Wrong
   {
     "openai": { // lowercase 'o'
       "ApiKey": "..."
     }
   }
   
   // Correct
   {
     "OpenAI": { // uppercase 'O'
       "ApiKey": "..."
     }
   }
   ```

3. **Production vs Development Configuration**
   ```csharp
   // Check which configuration is loaded
   public class ConfigurationDiagnostics
   {
       public static void LogConfiguration(IConfiguration config, ILogger logger)
       {
           var environment = config["ASPNETCORE_ENVIRONMENT"];
           var openAiKey = config["OpenAI:ApiKey"];
           
           logger.LogInformation("Environment: {Environment}", environment);
           logger.LogInformation("OpenAI key configured: {HasKey}", !string.IsNullOrEmpty(openAiKey));
       }
   }
   ```

### Testing Issues

#### Issue: "Tests fail with real API calls"

**Solution - Use Mocks:**
```csharp
public class ChatServiceTests
{
    [Test]
    public async Task GetResponse_ShouldReturnExpectedContent()
    {
        // Arrange
        var mockChatModel = new Mock<IChatModel>();
        mockChatModel.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), null, default))
            .ReturnsAsync(new ChatResponse
            {
                Content = "Test response",
                ModelId = "test-model",
                Usage = new TokenUsage { InputTokens = 10, OutputTokens = 5, TotalTokens = 15 }
            });

        var service = new ChatService(mockChatModel.Object);

        // Act
        var result = await service.GetResponseAsync("Test message");

        // Assert
        Assert.AreEqual("Test response", result);
    }
}
```

#### Issue: "Integration tests are flaky"

**Solutions:**
1. **Use Test Containers**
   ```csharp
   public class IntegrationTestBase : IAsyncLifetime
   {
       private readonly TestcontainersContainer _container;
       
       public async Task InitializeAsync()
       {
           await _container.StartAsync();
           // Setup test environment
       }
       
       public async Task DisposeAsync()
       {
           await _container.StopAsync();
       }
   }
   ```

2. **Implement Retry Logic for Tests**
   ```csharp
   [Test]
   [Retry(3)]
   public async Task FluentAI_Integration_Test()
   {
       // Test implementation
   }
   ```

## Diagnostic Tools

### Configuration Validator

```csharp
public class ConfigurationValidator
{
    public static ValidationResult ValidateConfiguration(IConfiguration config)
    {
        var result = new ValidationResult();
        
        // Check required configuration
        ValidateOpenAIConfig(config.GetSection("OpenAI"), result);
        ValidateAnthropicConfig(config.GetSection("Anthropic"), result);
        
        return result;
    }
    
    private static void ValidateOpenAIConfig(IConfigurationSection section, ValidationResult result)
    {
        var apiKey = section["ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            result.AddError("OpenAI:ApiKey is required");
        }
        else if (!apiKey.StartsWith("sk-"))
        {
            result.AddWarning("OpenAI API key format may be invalid");
        }
        
        var model = section["Model"];
        if (string.IsNullOrEmpty(model))
        {
            result.AddWarning("OpenAI:Model not specified, using default");
        }
    }
}

public class ValidationResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
    public bool IsValid => !Errors.Any();
    
    public void AddError(string error) => Errors.Add(error);
    public void AddWarning(string warning) => Warnings.Add(warning);
}
```

### Health Check Endpoint

```csharp
[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    private readonly IChatModel _chatModel;
    private readonly IConfiguration _configuration;
    
    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        try
        {
            var testMessage = new[] { new ChatMessage(ChatRole.User, "Health check") };
            var response = await _chatModel.GetResponseAsync(testMessage);
            
            return Ok(new
            {
                status = "healthy",
                model = response.ModelId,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                error = ex.Message,
                timestamp = DateTime.UtcNow
            });
        }
    }
    
    [HttpGet("config")]
    public IActionResult ConfigurationCheck()
    {
        var validation = ConfigurationValidator.ValidateConfiguration(_configuration);
        
        return Ok(new
        {
            isValid = validation.IsValid,
            errors = validation.Errors,
            warnings = validation.Warnings
        });
    }
}
```

### Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "FluentAI": "Debug",
      "FluentAI.Providers": "Trace",
      "System.Net.Http": "Warning"
    }
  }
}
```

## Getting Help

### Before Reporting Issues

1. **Check Configuration**
   - Verify API keys are correct and active
   - Ensure all required configuration sections are present
   - Test with minimal configuration first

2. **Review Logs**
   - Enable debug logging for FluentAI namespace
   - Check for any warnings or errors in startup logs
   - Look for HTTP request/response details

3. **Test Network Connectivity**
   - Verify internet connection
   - Test API endpoints manually with curl/Postman
   - Check firewall and proxy settings

4. **Verify Dependencies**
   - Ensure you're using compatible .NET version (8.0+)
   - Check NuGet package versions
   - Verify all required services are registered

### Where to Get Help

- 📖 [Documentation](../README.md)
- 🐛 [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- 💬 [GitHub Discussions](https://github.com/abxba0/fluentai-dotnet/discussions)
- 🧪 [Example Applications](../../Examples/)

### When Reporting Issues

Include the following information:

1. **Environment Details**
   - .NET version
   - FluentAI.NET version
   - Operating system
   - Deployment environment (local, Azure, AWS, etc.)

2. **Configuration** (sanitized, no API keys)
   ```json
   {
     "AiSdk": { ... },
     "OpenAI": { "Model": "...", "MaxTokens": "..." }
   }
   ```

3. **Code Sample** (minimal reproduction)
4. **Error Messages** (full exception details)
5. **Logs** (with debug logging enabled)

This will help maintainers and community members provide better assistance.