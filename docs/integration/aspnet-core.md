# ASP.NET Core Integration

This guide shows how to integrate FluentAI.NET into ASP.NET Core web applications with proper dependency injection and configuration.

## Basic Setup

### 1. Create Project and Install Package

```bash
dotnet new webapi -n MyAIWebApp
cd MyAIWebApp
dotnet add package FluentAI.NET
```

### 2. Configure Services in Program.cs

```csharp
using FluentAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentAI services
builder.Services.AddAiSdk(builder.Configuration)
    .AddOpenAiChatModel(builder.Configuration)
    .AddAnthropicChatModel(builder.Configuration)
    .AddGoogleGeminiChatModel(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 3. Add Configuration

Update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
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
    "MaxTokens": 1500,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 100,
    "WindowInSeconds": 60
  },
  "Anthropic": {
    "Model": "claude-3-haiku-20240307",
    "MaxTokens": 1500,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 50,
    "WindowInSeconds": 60
  },
  "AllowedHosts": "*"
}
```

### 4. Create a Chat Controller

```csharp
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MyAIWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatModel chatModel, ILogger<ChatController> logger)
    {
        _chatModel = chatModel;
        _logger = logger;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        try
        {
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, request.Message)
            };

            var response = await _chatModel.GetResponseAsync(messages);

            return Ok(new ChatResponse
            {
                Message = response.Content,
                ModelId = response.ModelId,
                TokenUsage = new TokenUsageResponse
                {
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens,
                    TotalTokens = response.Usage.TotalTokens
                }
            });
        }
        catch (AiSdkRateLimitException ex)
        {
            _logger.LogWarning(ex, "Rate limit exceeded for request");
            return StatusCode(429, new { error = "Rate limit exceeded. Please try again later." });
        }
        catch (AiSdkConfigurationException ex)
        {
            _logger.LogError(ex, "Configuration error");
            return StatusCode(500, new { error = "Service configuration error" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing chat request");
            return StatusCode(500, new { error = "An unexpected error occurred" });
        }
    }

    [HttpPost("stream")]
    public async Task<IActionResult> StreamMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest("Message cannot be empty");
        }

        try
        {
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, request.Message)
            };

            Response.Headers.Add("Content-Type", "text/plain");
            Response.Headers.Add("Cache-Control", "no-cache");

            await foreach (var token in _chatModel.StreamResponseAsync(messages))
            {
                await Response.WriteAsync(token);
                await Response.Body.FlushAsync();
            }

            return new EmptyResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming response");
            return StatusCode(500, new { error = "Streaming error occurred" });
        }
    }
}

public record ChatRequest(string Message);

public record ChatResponse
{
    public string Message { get; init; } = string.Empty;
    public string ModelId { get; init; } = string.Empty;
    public TokenUsageResponse TokenUsage { get; init; } = new();
}

public record TokenUsageResponse
{
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
    public int TotalTokens { get; init; }
}
```

## Advanced Features

### Conversation Management Service

```csharp
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using System.Collections.Concurrent;

public interface IConversationService
{
    Task<string> SendMessageAsync(string conversationId, string message);
    Task<string> StreamMessageAsync(string conversationId, string message);
    void ClearConversation(string conversationId);
}

public class ConversationService : IConversationService
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<ConversationService> _logger;
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _conversations = new();
    private readonly TimeSpan _conversationTimeout = TimeSpan.FromHours(2);

    public ConversationService(IChatModel chatModel, ILogger<ConversationService> logger)
    {
        _chatModel = chatModel;
        _logger = logger;
    }

    public async Task<string> SendMessageAsync(string conversationId, string message)
    {
        var messages = GetOrCreateConversation(conversationId);
        messages.Add(new ChatMessage(ChatRole.User, message));

        try
        {
            var response = await _chatModel.GetResponseAsync(messages);
            messages.Add(new ChatMessage(ChatRole.Assistant, response.Content));
            
            // Limit conversation history to prevent token overflow
            if (messages.Count > 20)
            {
                messages.RemoveRange(1, 2); // Keep system message, remove oldest user/assistant pair
            }

            return response.Content;
        }
        catch (Exception)
        {
            // Remove the user message if the request failed
            messages.RemoveAt(messages.Count - 1);
            throw;
        }
    }

    public async Task<string> StreamMessageAsync(string conversationId, string message)
    {
        var messages = GetOrCreateConversation(conversationId);
        messages.Add(new ChatMessage(ChatRole.User, message));

        var fullResponse = "";
        try
        {
            await foreach (var token in _chatModel.StreamResponseAsync(messages))
            {
                fullResponse += token;
            }
            
            messages.Add(new ChatMessage(ChatRole.Assistant, fullResponse));
            return fullResponse;
        }
        catch (Exception)
        {
            messages.RemoveAt(messages.Count - 1);
            throw;
        }
    }

    public void ClearConversation(string conversationId)
    {
        _conversations.TryRemove(conversationId, out _);
    }

    private List<ChatMessage> GetOrCreateConversation(string conversationId)
    {
        return _conversations.GetOrAdd(conversationId, _ => new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant.")
        });
    }
}
```

### Register the Service

Add to Program.cs:

```csharp
builder.Services.AddSingleton<IConversationService, ConversationService>();
```

### Health Check Integration

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;

public class FluentAIHealthCheck : IHealthCheck
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<FluentAIHealthCheck> _logger;

    public FluentAIHealthCheck(IChatModel chatModel, ILogger<FluentAIHealthCheck> logger)
    {
        _chatModel = chatModel;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testMessage = new[] { new ChatMessage(ChatRole.User, "Health check") };
            var response = await _chatModel.GetResponseAsync(testMessage, cancellationToken: cancellationToken);
            
            return HealthCheckResult.Healthy($"FluentAI service is working. Model: {response.ModelId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FluentAI health check failed");
            return HealthCheckResult.Unhealthy($"FluentAI service failed: {ex.Message}");
        }
    }
}

// Register in Program.cs
builder.Services.AddHealthChecks()
    .AddCheck<FluentAIHealthCheck>("fluentai");

// Add health check endpoint
app.MapHealthChecks("/health");
```

### Authentication and Authorization

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication
public class ChatController : ControllerBase
{
    [HttpPost("message")]
    [RateLimiting("ChatPolicy")] // Rate limiting per user
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        // Get user ID from claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Use userId for conversation management
        var conversationId = $"{userId}-{DateTime.UtcNow:yyyy-MM-dd}";
        
        // ... rest of implementation
    }
}
```

### Middleware for Request Logging

```csharp
public class AIRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AIRequestLoggingMiddleware> _logger;

    public AIRequestLoggingMiddleware(RequestDelegate next, ILogger<AIRequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/chat"))
        {
            var startTime = DateTime.UtcNow;
            
            await _next(context);
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "AI request completed: {Method} {Path} responded {StatusCode} in {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
        else
        {
            await _next(context);
        }
    }
}

// Register in Program.cs
app.UseMiddleware<AIRequestLoggingMiddleware>();
```

## Production Considerations

### 1. Configuration Management

```csharp
// Use Azure Key Vault or similar for production
builder.Configuration.AddAzureKeyVault(keyVaultUrl, credential);
```

### 2. Caching

```csharp
// Add response caching
builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Use in controller
[ResponseCache(Duration = 300)] // Cache for 5 minutes
public async Task<IActionResult> GetPopularQuestions()
{
    // Implementation
}
```

### 3. Rate Limiting

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ChatPolicy", configure =>
    {
        configure.PermitLimit = 10;
        configure.Window = TimeSpan.FromMinutes(1);
    });
});

app.UseRateLimiter();
```

### 4. API Documentation

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "AI Chat API", 
        Version = "v1",
        Description = "An API for AI-powered chat interactions using FluentAI.NET"
    });
    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
});
```

## Testing

### Unit Tests

```csharp
using Microsoft.Extensions.Logging;
using Moq;
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;

public class ChatControllerTests
{
    [Test]
    public async Task SendMessage_ValidRequest_ReturnsOkResult()
    {
        // Arrange
        var mockChatModel = new Mock<IChatModel>();
        var mockLogger = new Mock<ILogger<ChatController>>();
        
        mockChatModel.Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), null, default))
            .ReturnsAsync(new ChatResponse
            {
                Content = "Test response",
                ModelId = "test-model",
                Usage = new TokenUsage { InputTokens = 10, OutputTokens = 20, TotalTokens = 30 }
            });

        var controller = new ChatController(mockChatModel.Object, mockLogger.Object);

        // Act
        var result = await controller.SendMessage(new ChatRequest("Hello"));

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
    }
}
```

### Integration Tests

```csharp
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

public class ChatControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChatControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Test]
    public async Task SendMessage_ReturnsSuccessfulResponse()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new ChatRequest("Hello, AI!");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/chat/message", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();
        var chatResponse = JsonSerializer.Deserialize<ChatResponse>(responseString);
        
        Assert.IsNotNull(chatResponse);
        Assert.IsNotEmpty(chatResponse.Message);
    }
}
```

## Next Steps

- Explore [Blazor integration](blazor.md) for interactive web UIs
- Learn about [performance optimization](common-patterns.md#performance-optimization)
- Check [security best practices](common-patterns.md#security)
- Review the [troubleshooting guide](troubleshooting.md)