# Common Patterns & Best Practices

This guide covers common patterns, best practices, and reusable code snippets for FluentAI.NET integration across different project types.

## Configuration Patterns

### Environment-Based Configuration

```csharp
// appsettings.json (base configuration)
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  }
}

// appsettings.Development.json
{
  "OpenAI": {
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 500
  }
}

// appsettings.Production.json
{
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "PermitLimit": 1000,
    "WindowInSeconds": 60
  }
}
```

### Secure Configuration with Azure Key Vault

```csharp
public static class ConfigurationExtensions
{
    public static WebApplicationBuilder AddSecureConfiguration(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsProduction())
        {
            var keyVaultUrl = builder.Configuration["KeyVault:Url"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                var credential = new DefaultAzureCredential();
                builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), credential);
            }
        }

        return builder;
    }
}

// Usage
var builder = WebApplication.CreateBuilder(args);
builder.AddSecureConfiguration();
```

### Configuration Validation

```csharp
public class AiConfigurationValidator : IValidateOptions<AiSdkOptions>
{
    public ValidateOptionsResult Validate(string? name, AiSdkOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(options.DefaultProvider))
        {
            errors.Add("DefaultProvider must be specified");
        }

        if (options.Failover?.PrimaryProvider == options.Failover?.FallbackProvider)
        {
            errors.Add("Primary and fallback providers cannot be the same");
        }

        return errors.Any() 
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}

// Register validator
services.AddSingleton<IValidateOptions<AiSdkOptions>, AiConfigurationValidator>();
```

## Security Patterns

### Input Sanitization Service

```csharp
public interface IInputSanitizationService
{
    Task<string> SanitizeAsync(string input);
    Task<bool> IsContentSafeAsync(string input);
}

public class InputSanitizationService : IInputSanitizationService
{
    private readonly IInputSanitizer _sanitizer;
    private readonly ILogger<InputSanitizationService> _logger;

    public InputSanitizationService(IInputSanitizer sanitizer, ILogger<InputSanitizationService> logger)
    {
        _sanitizer = sanitizer;
        _logger = logger;
    }

    public async Task<string> SanitizeAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        try
        {
            // Basic sanitization
            var sanitized = _sanitizer.SanitizeContent(input);
            
            // Additional custom sanitization
            sanitized = await PerformCustomSanitization(sanitized);
            
            return sanitized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sanitizing input");
            throw;
        }
    }

    public async Task<bool> IsContentSafeAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
            return true;

        try
        {
            var isSafe = _sanitizer.IsContentSafe(input);
            var riskAssessment = _sanitizer.AssessRisk(input);
            
            // Log security events
            if (!isSafe || riskAssessment.RiskLevel >= SecurityRiskLevel.Medium)
            {
                _logger.LogWarning("Potentially unsafe content detected: {RiskLevel}", riskAssessment.RiskLevel);
            }
            
            return isSafe && riskAssessment.RiskLevel < SecurityRiskLevel.High;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing content safety");
            return false; // Fail safe
        }
    }

    private async Task<string> PerformCustomSanitization(string input)
    {
        // Custom sanitization logic
        // Remove potential prompt injections
        var patterns = new[]
        {
            @"ignore\s+previous\s+instructions",
            @"system\s*:",
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"data:text\/html"
        };

        foreach (var pattern in patterns)
        {
            input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase);
        }

        return input.Trim();
    }
}
```

### Content Filtering Middleware

```csharp
public class ContentFilteringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IInputSanitizationService _sanitization;
    private readonly ILogger<ContentFilteringMiddleware> _logger;

    public ContentFilteringMiddleware(
        RequestDelegate next,
        IInputSanitizationService sanitization,
        ILogger<ContentFilteringMiddleware> logger)
    {
        _next = next;
        _sanitization = sanitization;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldFilter(context.Request))
        {
            await FilterRequest(context);
        }

        await _next(context);
    }

    private bool ShouldFilter(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api/chat") && 
               request.Method == HttpMethods.Post;
    }

    private async Task FilterRequest(HttpContext context)
    {
        var body = await ReadRequestBody(context.Request);
        
        if (!string.IsNullOrEmpty(body))
        {
            var isContentSafe = await _sanitization.IsContentSafeAsync(body);
            
            if (!isContentSafe)
            {
                _logger.LogWarning("Blocked unsafe content from {IP}", context.Connection.RemoteIpAddress);
                
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Content blocked due to security policy");
                return;
            }
        }
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        request.EnableBuffering();
        
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;
        
        return body;
    }
}
```

## Performance Patterns

### Response Caching Service

```csharp
public interface IChatCacheService
{
    Task<ChatResponse?> GetCachedResponseAsync(string cacheKey);
    Task SetCachedResponseAsync(string cacheKey, ChatResponse response, TimeSpan? expiry = null);
    string GenerateCacheKey(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null);
}

public class ChatCacheService : IChatCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ChatCacheService> _logger;
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);

    public ChatCacheService(IMemoryCache cache, ILogger<ChatCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<ChatResponse?> GetCachedResponseAsync(string cacheKey)
    {
        try
        {
            return _cache.Get<ChatResponse>(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached response for key {CacheKey}", cacheKey);
            return null;
        }
    }

    public async Task SetCachedResponseAsync(string cacheKey, ChatResponse response, TimeSpan? expiry = null)
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry,
                SlidingExpiration = TimeSpan.FromMinutes(10),
                Priority = CacheItemPriority.Normal
            };

            _cache.Set(cacheKey, response, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching response for key {CacheKey}", cacheKey);
        }
    }

    public string GenerateCacheKey(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null)
    {
        var content = string.Join("|", messages.Select(m => $"{m.Role}:{m.Content}"));
        var optionsHash = options?.GetHashCode() ?? 0;
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{content}|{optionsHash}"));
        
        return Convert.ToBase64String(hash);
    }
}
```

### Cached Chat Service

```csharp
public class CachedChatService : IChatModel
{
    private readonly IChatModel _chatModel;
    private readonly IChatCacheService _cacheService;
    private readonly ILogger<CachedChatService> _logger;

    public CachedChatService(
        IChatModel chatModel,
        IChatCacheService cacheService,
        ILogger<CachedChatService> logger)
    {
        _chatModel = chatModel;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = _cacheService.GenerateCacheKey(messages, options);
        
        // Try to get from cache first
        var cachedResponse = await _cacheService.GetCachedResponseAsync(cacheKey);
        if (cachedResponse != null)
        {
            _logger.LogDebug("Cache hit for request {CacheKey}", cacheKey);
            return cachedResponse;
        }

        // Get from AI service
        _logger.LogDebug("Cache miss for request {CacheKey}", cacheKey);
        var response = await _chatModel.GetResponseAsync(messages, options, cancellationToken);
        
        // Cache the response
        await _cacheService.SetCachedResponseAsync(cacheKey, response);
        
        return response;
    }

    public IAsyncEnumerable<string> StreamResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Streaming responses are typically not cached
        return _chatModel.StreamResponseAsync(messages, options, cancellationToken);
    }
}
```

### Circuit Breaker Pattern

```csharp
public class CircuitBreakerChatService : IChatModel
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<CircuitBreakerChatService> _logger;
    private readonly CircuitBreakerState _state = new();

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_state.IsOpen)
        {
            throw new AiSdkException("Circuit breaker is open. Service temporarily unavailable.");
        }

        try
        {
            var response = await _chatModel.GetResponseAsync(messages, options, cancellationToken);
            _state.RecordSuccess();
            return response;
        }
        catch (Exception ex)
        {
            _state.RecordFailure();
            
            if (_state.ShouldOpen())
            {
                _logger.LogWarning("Circuit breaker opened due to consecutive failures");
                _state.Open();
            }
            
            throw;
        }
    }

    // Similar implementation for StreamResponseAsync
}

public class CircuitBreakerState
{
    private int _consecutiveFailures = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private bool _isOpen = false;
    
    private const int FailureThreshold = 5;
    private readonly TimeSpan _timeout = TimeSpan.FromMinutes(1);

    public bool IsOpen => _isOpen && DateTime.UtcNow - _lastFailureTime < _timeout;

    public void RecordSuccess()
    {
        _consecutiveFailures = 0;
        _isOpen = false;
    }

    public void RecordFailure()
    {
        _consecutiveFailures++;
        _lastFailureTime = DateTime.UtcNow;
    }

    public bool ShouldOpen() => _consecutiveFailures >= FailureThreshold;

    public void Open() => _isOpen = true;
}
```

## Error Handling Patterns

### Resilient Chat Service

```csharp
public class ResilientChatService : IChatModel
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<ResilientChatService> _logger;

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var retryPolicy = Policy
            .Handle<AiSdkRateLimitException>()
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for chat request after {Delay}ms",
                        retryCount, timespan.TotalMilliseconds);
                });

        return await retryPolicy.ExecuteAsync(async () =>
        {
            return await _chatModel.GetResponseAsync(messages, options, cancellationToken);
        });
    }
}
```

### Global Exception Handler (ASP.NET Core)

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            AiSdkRateLimitException => (429, "Rate limit exceeded. Please try again later."),
            AiSdkConfigurationException => (500, "Service configuration error."),
            AiSdkException => (500, "AI service error occurred."),
            ArgumentException => (400, "Invalid request parameters."),
            _ => (500, "An unexpected error occurred.")
        };

        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode;
        
        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            timestamp = DateTime.UtcNow,
            requestId = httpContext.TraceIdentifier
        }, cancellationToken);

        return true;
    }
}
```

## Testing Patterns

### Mock Chat Model for Testing

```csharp
public class MockChatModel : IChatModel
{
    private readonly Queue<ChatResponse> _responses = new();
    private readonly Queue<string[]> _streamingResponses = new();

    public void QueueResponse(ChatResponse response)
    {
        _responses.Enqueue(response);
    }

    public void QueueStreamingResponse(params string[] tokens)
    {
        _streamingResponses.Enqueue(tokens);
    }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No responses queued");
        }

        return Task.FromResult(_responses.Dequeue());
    }

    public async IAsyncEnumerable<string> StreamResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatRequestOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (_streamingResponses.Count == 0)
        {
            throw new InvalidOperationException("No streaming responses queued");
        }

        var tokens = _streamingResponses.Dequeue();
        foreach (var token in tokens)
        {
            await Task.Delay(10, cancellationToken); // Simulate streaming delay
            yield return token;
        }
    }
}
```

### Integration Test Base Class

```csharp
public abstract class ChatIntegrationTestBase : IDisposable
{
    protected WebApplicationFactory<Program> Factory { get; }
    protected HttpClient Client { get; }
    protected MockChatModel MockChatModel { get; }

    protected ChatIntegrationTestBase()
    {
        MockChatModel = new MockChatModel();
        
        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Replace real chat model with mock
                    services.RemoveAll<IChatModel>();
                    services.AddSingleton<IChatModel>(MockChatModel);
                });
            });

        Client = Factory.CreateClient();
    }

    protected void QueueSuccessResponse(string content = "Test response")
    {
        MockChatModel.QueueResponse(new ChatResponse
        {
            Content = content,
            ModelId = "test-model",
            Usage = new TokenUsage { InputTokens = 10, OutputTokens = 20, TotalTokens = 30 }
        });
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }
}
```

## Logging Patterns

### Structured Logging Service

```csharp
public class ChatAuditService
{
    private readonly ILogger<ChatAuditService> _logger;

    public ChatAuditService(ILogger<ChatAuditService> logger)
    {
        _logger = logger;
    }

    public void LogChatRequest(string userId, IEnumerable<ChatMessage> messages, string modelId)
    {
        _logger.LogInformation("Chat request from user {UserId} to model {ModelId} with {MessageCount} messages",
            userId, modelId, messages.Count());
    }

    public void LogChatResponse(string userId, ChatResponse response, TimeSpan duration)
    {
        _logger.LogInformation(
            "Chat response for user {UserId}: {ModelId} responded with {OutputTokens} tokens in {Duration}ms",
            userId, response.ModelId, response.Usage.OutputTokens, duration.TotalMilliseconds);
    }

    public void LogSecurityEvent(string userId, string content, SecurityRiskLevel riskLevel)
    {
        _logger.LogWarning("Security event for user {UserId}: {RiskLevel} risk content detected",
            userId, riskLevel);
    }

    public void LogRateLimitEvent(string userId, string provider)
    {
        _logger.LogWarning("Rate limit exceeded for user {UserId} with provider {Provider}",
            userId, provider);
    }
}
```

## Deployment Patterns

### Health Checks

```csharp
public static class HealthCheckExtensions
{
    public static IServiceCollection AddFluentAIHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<FluentAIHealthCheck>("fluentai")
            .AddCheck("configuration", () =>
            {
                var openAiKey = configuration["OpenAI:ApiKey"];
                return string.IsNullOrEmpty(openAiKey)
                    ? HealthCheckResult.Unhealthy("OpenAI API key not configured")
                    : HealthCheckResult.Healthy();
            });

        return services;
    }
}
```

### Docker Configuration

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyApp.csproj", "."]
RUN dotnet restore "MyApp.csproj"
COPY . .
RUN dotnet build "MyApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MyApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  myapp:
    build: .
    ports:
      - "8080:80"
    environment:
      - OPENAI_API_KEY=${OPENAI_API_KEY}
      - ANTHROPIC_API_KEY=${ANTHROPIC_API_KEY}
      - ASPNETCORE_ENVIRONMENT=Production
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:80/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

These patterns provide a solid foundation for building robust, secure, and performant applications with FluentAI.NET. Choose the patterns that best fit your specific use case and requirements.