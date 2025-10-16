# FluentAI.NET Provider Best Practices

Comprehensive guide for getting the best results from each AI provider.

## Table of Contents

1. [OpenAI Best Practices](#openai-best-practices)
2. [Anthropic (Claude) Best Practices](#anthropic-claude-best-practices)
3. [Google (Gemini) Best Practices](#google-gemini-best-practices)
4. [Provider Comparison](#provider-comparison)
5. [Multi-Provider Strategies](#multi-provider-strategies)
6. [Failover and Reliability](#failover-and-reliability)

---

## OpenAI Best Practices

### Models Overview

| Model | Context | Input | Output | Best For |
|-------|---------|-------|--------|----------|
| GPT-4 Turbo | 128K | $0.01/1K | $0.03/1K | Complex tasks, long context |
| GPT-4 | 8K | $0.03/1K | $0.06/1K | High quality reasoning |
| GPT-3.5 Turbo | 16K | $0.0005/1K | $0.0015/1K | Fast, cost-effective |

### Configuration

```csharp
{
  "OpenAI": {
    "ApiKey": "your-api-key",
    "Model": "gpt-4-turbo-preview",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 100,
    "WindowInSeconds": 60
  }
}
```

### Best Practices

#### 1. System Messages

OpenAI models work best with clear, concise system messages:

```csharp
var systemMessage = new ChatMessage(ChatRole.System, 
    "You are a helpful assistant. Be concise and accurate.");
```

**Tips:**
- Keep system messages under 100 tokens
- Be specific about desired behavior
- Update system message instead of repeating instructions

#### 2. Temperature Settings

```csharp
// For factual, deterministic responses
var options = new OpenAiRequestOptions { Temperature = 0.0f };

// For balanced creativity and accuracy
var options = new OpenAiRequestOptions { Temperature = 0.7f };

// For creative writing
var options = new OpenAiRequestOptions { Temperature = 1.0f };
```

| Use Case | Temperature | Why |
|----------|-------------|-----|
| Code generation | 0.0-0.3 | Deterministic, correct |
| FAQs/Support | 0.3-0.5 | Consistent, accurate |
| General chat | 0.5-0.8 | Natural, varied |
| Creative writing | 0.8-1.2 | Diverse, creative |

#### 3. Token Management

```csharp
var options = new OpenAiRequestOptions
{
    MaxTokens = 500,           // Limit response length
    TopP = 0.9f,              // Nucleus sampling
    FrequencyPenalty = 0.1f,   // Reduce repetition
    PresencePenalty = 0.1f     // Encourage topic diversity
};
```

#### 4. Streaming for Better UX

```csharp
await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token);  // Real-time display
}
```

**Benefits:**
- Lower perceived latency
- Better user experience
- Can cancel mid-stream if needed

#### 5. Function Calling

OpenAI has excellent function calling support:

```csharp
var functions = new[]
{
    new FunctionDefinition
    {
        Name = "get_weather",
        Description = "Get weather for a location",
        Parameters = new
        {
            type = "object",
            properties = new
            {
                location = new { type = "string" },
                unit = new { type = "string", @enum = new[] { "celsius", "fahrenheit" } }
            },
            required = new[] { "location" }
        }
    }
};

var options = new OpenAiRequestOptions { Functions = functions };
```

#### 6. Rate Limiting

```csharp
{
  "OpenAI": {
    "PermitLimit": 100,      // Requests per window
    "WindowInSeconds": 60     // Time window
  }
}
```

**Tier Limits (OpenAI):**
- Free tier: 3 RPM (requests per minute)
- Tier 1: 3,500 RPM
- Tier 2: 5,000 RPM
- Tier 3: 10,000 RPM

### Common Pitfalls

❌ **Don't:**
- Repeat instructions in every message
- Use unnecessarily high temperature for factual queries
- Send entire conversation history every time
- Forget to set max_tokens

✅ **Do:**
- Use system message for persistent instructions
- Adjust temperature based on task
- Trim conversation history
- Set appropriate max_tokens

---

## Anthropic (Claude) Best Practices

### Models Overview

| Model | Context | Input | Output | Best For |
|-------|---------|-------|--------|----------|
| Claude 3 Opus | 200K | $0.015/1K | $0.075/1K | Highest intelligence |
| Claude 3 Sonnet | 200K | $0.003/1K | $0.015/1K | Balanced performance |
| Claude 3 Haiku | 200K | $0.00025/1K | $0.00125/1K | Speed, efficiency |

### Configuration

```csharp
{
  "Anthropic": {
    "ApiKey": "your-api-key",
    "Model": "claude-3-sonnet-20240229",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 50,
    "WindowInSeconds": 60
  }
}
```

### Best Practices

#### 1. System Prompts

Claude uses a separate system prompt parameter:

```csharp
var options = new AnthropicRequestOptions
{
    SystemPrompt = "You are a helpful AI assistant. Be concise and accurate."
};
```

**Important:** Don't include system message in the messages array for Claude.

#### 2. Long Context Handling

Claude excels at long context (200K tokens):

```csharp
// Efficient for document analysis
var documentMessages = new List<ChatMessage>
{
    new(ChatRole.User, $"Analyze this document:\n\n{longDocument}\n\nWhat are the key points?")
};

// Claude can handle this without truncation
var response = await claudeModel.GetResponseAsync(documentMessages);
```

**Best for:**
- Document summarization
- Code repository analysis
- Long conversation threads
- Multi-document comparison

#### 3. XML Tags for Structure

Claude responds well to XML-style tags:

```csharp
var prompt = @"
<document>
{documentContent}
</document>

<task>
Summarize the key points from the document.
</task>

<format>
- Use bullet points
- Maximum 5 points
- Be concise
</format>
";
```

#### 4. Thinking and Reasoning

Claude benefits from explicit reasoning steps:

```csharp
var prompt = @"
Before answering, think through the problem step by step:

<thinking>
1. What is being asked?
2. What information do I have?
3. What's the logical approach?
</thinking>

<answer>
Your final answer here.
</answer>
";
```

#### 5. Constitutional AI

Claude is trained with constitutional AI principles:

```csharp
// Claude naturally refuses harmful requests
// No special filtering needed for most cases
var prompt = "How do I bake a cake?";  // Safe, helpful response

// Claude will refuse and explain
var harmful = "How do I hack a computer?";  // Politely refuses
```

#### 6. Caching (Advanced)

For repeated long contexts, use prompt caching:

```csharp
// First request: Full cost
// Subsequent requests with same prefix: Reduced cost

var systemPrompt = longSystemInstructions;  // Cache this
var options = new AnthropicRequestOptions
{
    SystemPrompt = systemPrompt,
    Temperature = 0.7
};

// Multiple requests with same system prompt = cache hits
```

### Common Pitfalls

❌ **Don't:**
- Include system message in messages array
- Use short contexts when Claude's strength is long contexts
- Forget to use XML tags for complex structured tasks
- Assume same behavior as GPT models

✅ **Do:**
- Use SystemPrompt parameter
- Leverage 200K context window
- Use XML tags for structure
- Let Claude think through complex problems

---

## Google (Gemini) Best Practices

### Models Overview

| Model | Context | Input | Output | Best For |
|-------|---------|-------|--------|----------|
| Gemini Ultra | 32K | $0.001/1K | $0.002/1K | High quality |
| Gemini Pro | 32K | $0.00025/1K | $0.0005/1K | General use |

### Configuration

```csharp
{
  "Google": {
    "ApiKey": "your-api-key",
    "Model": "gemini-pro",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "RequestTimeout": "00:02:00",
    "PermitLimit": 60,
    "WindowInSeconds": 60
  }
}
```

### Best Practices

#### 1. Multi-Modal Capabilities

Gemini natively supports images:

```csharp
// Text + Image
var messages = new[]
{
    new ChatMessage(ChatRole.User, "What's in this image?")
    {
        ImageUrl = "https://example.com/image.jpg"
    }
};
```

#### 2. Cost Efficiency

Gemini Pro is extremely cost-effective:

```csharp
// For high-volume applications
services.AddAiSdk(configuration)
    .AddGoogleGeminiChatModel(configuration);

// Use for:
// - FAQs
// - Simple queries
// - High-volume scenarios
// - Cost-sensitive applications
```

**Cost Comparison (per 1K tokens):**
- Gemini Pro: $0.00025 (input)
- GPT-3.5: $0.0005 (2x more expensive)
- GPT-4: $0.03 (120x more expensive)

#### 3. Safety Settings

```csharp
var options = new GoogleRequestOptions
{
    SafetySettings = new[]
    {
        new SafetySetting
        {
            Category = HarmCategory.Harassment,
            Threshold = HarmBlockThreshold.BlockMediumAndAbove
        }
    }
};
```

#### 4. Streaming

```csharp
await foreach (var token in geminiModel.StreamResponseAsync(messages))
{
    // Gemini has efficient streaming
    Console.Write(token);
}
```

### Common Pitfalls

❌ **Don't:**
- Expect same output style as OpenAI/Anthropic
- Forget about multi-modal capabilities
- Ignore safety settings

✅ **Do:**
- Leverage cost efficiency
- Use multi-modal features
- Configure safety settings
- Test output format expectations

---

## Provider Comparison

### When to Use Each Provider

| Scenario | Best Provider | Why |
|----------|---------------|-----|
| Complex reasoning | GPT-4 | Strongest reasoning |
| Code generation | GPT-4 | Best at code |
| Long documents | Claude | 200K context |
| Cost efficiency | Gemini Pro | Cheapest |
| Fast responses | Claude Haiku | Speed optimized |
| Creative writing | GPT-4 | Most creative |
| General chat | GPT-3.5 or Gemini | Cost/quality balance |
| Multi-modal | Gemini | Native support |

### Performance Comparison

| Provider | Avg Response Time | Token Throughput | Reliability |
|----------|-------------------|------------------|-------------|
| OpenAI GPT-4 | 2-4s | 40-60 tokens/s | 99.9% |
| OpenAI GPT-3.5 | 0.5-1.5s | 80-120 tokens/s | 99.9% |
| Claude 3 Opus | 3-5s | 30-50 tokens/s | 99.5% |
| Claude 3 Sonnet | 1.5-3s | 50-80 tokens/s | 99.5% |
| Claude 3 Haiku | 0.5-1s | 100-150 tokens/s | 99.5% |
| Gemini Pro | 1-2s | 60-100 tokens/s | 99.0% |

---

## Multi-Provider Strategies

### 1. Smart Routing by Task Type

```csharp
public class MultiProviderRouter
{
    private readonly IChatModel _gpt4;
    private readonly IChatModel _claude;
    private readonly IChatModel _gemini;
    
    public IChatModel RouteByTask(string taskType) => taskType switch
    {
        "code" => _gpt4,           // Best at code
        "document" => _claude,     // Long context
        "faq" => _gemini,          // Cost efficient
        "creative" => _gpt4,       // Most creative
        _ => _gemini               // Default to cheapest
    };
}
```

### 2. Failover Strategy

```csharp
{
  "AiSdk": {
    "Failover": {
      "PrimaryProvider": "OpenAI",
      "FallbackProvider": "Anthropic"
    }
  }
}
```

**Recommended Failover Chains:**

**High Quality:**
```
GPT-4 → Claude 3 Opus → Claude 3 Sonnet
```

**Balanced:**
```
GPT-3.5 → Claude 3 Sonnet → Gemini Pro
```

**Cost Optimized:**
```
Gemini Pro → Claude 3 Haiku → GPT-3.5
```

### 3. A/B Testing

```csharp
public class ProviderABTest
{
    public async Task<(ChatResponse Response, string Provider)> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        string userId)
    {
        // Hash user ID to determine provider (50/50 split)
        var provider = HashUserId(userId) % 2 == 0 ? "OpenAI" : "Anthropic";
        
        var model = provider == "OpenAI" ? _openAIModel : _claudeModel;
        var response = await model.GetResponseAsync(messages);
        
        // Log for analysis
        await LogABTestResult(userId, provider, response);
        
        return (response, provider);
    }
}
```

### 4. Cost-Based Load Balancing

```csharp
public class CostAwareLoadBalancer
{
    private decimal _monthlyBudget = 1000m;
    private decimal _currentSpend;
    
    public IChatModel SelectProvider()
    {
        var remaining = _monthlyBudget - _currentSpend;
        var daysLeft = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month) 
                      - DateTime.Now.Day;
        
        var dailyBudget = remaining / Math.Max(daysLeft, 1);
        
        // If running low, use cheaper providers
        if (dailyBudget < 30m)
        {
            return _geminiModel;  // Cheapest
        }
        else if (dailyBudget < 50m)
        {
            return _gpt35Model;   // Moderate cost
        }
        else
        {
            return _gpt4Model;    // Highest quality
        }
    }
}
```

---

## Failover and Reliability

### Circuit Breaker Pattern

```csharp
public class ResilientChatService
{
    private readonly IChatModel _primary;
    private readonly IChatModel _fallback;
    private readonly ILogger _logger;
    private int _failureCount = 0;
    private DateTime? _circuitOpenedAt;
    private const int FailureThreshold = 3;
    private readonly TimeSpan CircuitResetTimeout = TimeSpan.FromMinutes(1);
    
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages)
    {
        // Check if circuit is open
        if (_circuitOpenedAt.HasValue)
        {
            if (DateTime.UtcNow - _circuitOpenedAt.Value < CircuitResetTimeout)
            {
                _logger.LogWarning("Circuit is open, using fallback provider");
                return await _fallback.GetResponseAsync(messages);
            }
            else
            {
                // Try to close circuit
                _circuitOpenedAt = null;
                _failureCount = 0;
                _logger.LogInformation("Circuit reset, attempting primary provider");
            }
        }
        
        try
        {
            var response = await _primary.GetResponseAsync(messages);
            _failureCount = 0;  // Reset on success
            return response;
        }
        catch (Exception ex)
        {
            _failureCount++;
            _logger.LogError(ex, "Primary provider failed ({FailureCount}/{Threshold})",
                _failureCount, FailureThreshold);
            
            if (_failureCount >= FailureThreshold)
            {
                _circuitOpenedAt = DateTime.UtcNow;
                _logger.LogWarning("Circuit opened after {Count} failures", FailureCount);
            }
            
            // Use fallback
            return await _fallback.GetResponseAsync(messages);
        }
    }
}
```

### Retry with Exponential Backoff

```csharp
public class RetryPolicy
{
    public async Task<ChatResponse> ExecuteWithRetryAsync(
        Func<Task<ChatResponse>> operation,
        int maxRetries = 3)
    {
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (AiSdkRateLimitException) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }
        
        throw new Exception("Max retries exceeded");
    }
}
```

---

## Best Practices Summary

### OpenAI
✅ Best for: Complex reasoning, code generation
✅ Use: System messages, streaming, function calling
✅ Temperature: 0-0.3 for factual, 0.7-1.0 for creative
✅ Context: Up to 128K with GPT-4 Turbo

### Anthropic (Claude)
✅ Best for: Long documents, analysis, reasoning
✅ Use: System prompt parameter, XML tags, 200K context
✅ Temperature: 0-0.3 for factual, 0.7-1.0 for creative
✅ Context: Up to 200K tokens

### Google (Gemini)
✅ Best for: Cost efficiency, high volume, multi-modal
✅ Use: Multi-modal inputs, safety settings
✅ Temperature: 0-0.3 for factual, 0.7-1.0 for creative
✅ Context: Up to 32K tokens

### Multi-Provider
✅ Use smart routing based on task type
✅ Implement failover for reliability
✅ A/B test for quality comparison
✅ Balance cost and performance

---

## Additional Resources

- [Cost Optimization Guide](COST-OPTIMIZATION-GUIDE.md)
- [Performance Optimization Guide](PERFORMANCE-OPTIMIZATION-GUIDE.md)
- [Roadmap Implementation](ROADMAP-IMPLEMENTATION.md)
- [API Reference](API-Reference.md)
