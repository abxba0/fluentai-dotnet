# FluentAI.NET Performance Optimization Guide

This guide provides comprehensive strategies for optimizing performance, reducing costs, and improving the efficiency of your FluentAI.NET applications.

## Table of Contents

1. [Response Caching Strategies](#response-caching-strategies)
2. [Batch Processing](#batch-processing)
3. [Token Management](#token-management)
4. [Streaming Optimization](#streaming-optimization)
5. [Memory Management](#memory-management)
6. [Cost Optimization](#cost-optimization)
7. [Benchmarking and Monitoring](#benchmarking-and-monitoring)

---

## Response Caching Strategies

### Basic Response Caching

Response caching is the most effective way to reduce latency and costs.

```csharp
services.AddSingleton<IResponseCache, MemoryResponseCache>();
```

**Usage:**
```csharp
public class OptimizedChatService
{
    private readonly IChatModel _chatModel;
    private readonly IResponseCache _cache;
    
    public async Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages)
    {
        // Check cache first
        var cached = await _cache.GetAsync(messages);
        if (cached != null)
        {
            return cached;
        }
        
        // Cache miss - get response from model
        var response = await _chatModel.GetResponseAsync(messages);
        
        // Cache for 30 minutes
        await _cache.SetAsync(messages, null, response, TimeSpan.FromMinutes(30));
        
        return response;
    }
}
```

**Cache Hit Rate Optimization:**
- Use consistent message formatting
- Normalize user input (trim, lowercase for case-insensitive)
- Set appropriate TTL based on content volatility
- Monitor cache hit rates and adjust strategy

### Semantic Caching

For better cache hit rates, use semantic caching to match similar queries:

```csharp
services.AddSingleton<ISemanticCache, DefaultSemanticCache>();
```

**Configuration:**
```csharp
var semanticCacheOptions = new SemanticCacheOptions
{
    DefaultSimilarityThreshold = 0.95, // 95% similarity required
    DefaultTtl = TimeSpan.FromHours(1),
    MaxEntries = 1000,
    EnableAutomaticCleanup = true,
    CleanupInterval = TimeSpan.FromMinutes(5),
    EvictionStrategy = CacheEvictionStrategy.LeastRecentlyUsed
};
```

**Usage:**
```csharp
// Store with semantic embedding
await semanticCache.SetSemanticAsync(
    messages,
    options,
    response,
    ttl: TimeSpan.FromHours(2),
    tags: new[] { "product-info" }
);

// Retrieve with semantic matching - matches similar questions
var result = await semanticCache.GetSemanticAsync(
    messages,
    options,
    similarityThreshold: 0.93 // Slightly lower for more hits
);
```

**Benefits:**
- Matches semantically similar queries (e.g., "How much does it cost?" ≈ "What's the price?")
- Higher cache hit rates (typically 15-30% higher than exact matching)
- Reduces redundant API calls for similar questions

**Performance Impact:**
- Cache hit: ~5-10ms (including similarity computation)
- Cache miss: Full model latency (~500-2000ms)
- **Potential savings:** 40-60% reduction in API calls for similar content

---

## Batch Processing

Process multiple requests in parallel to maximize throughput.

### Basic Batch Processing

```csharp
services.AddScoped<IBatchProcessor, DefaultBatchProcessor>();
```

**Usage:**
```csharp
var requests = new List<BatchRequest>
{
    new() { Messages = messages1, Priority = 1 },
    new() { Messages = messages2, Priority = 2 },
    new() { Messages = messages3, Priority = 1 }
};

var options = new BatchProcessingOptions
{
    MaxDegreeOfParallelism = 5, // Process 5 requests simultaneously
    RetryFailedRequests = true,
    MaxRetryAttempts = 2,
    RequestTimeout = TimeSpan.FromMinutes(2),
    PreserveOrder = false // Better performance if order doesn't matter
};

var results = await batchProcessor.ProcessBatchAsync(requests, options);
```

**Performance Guidelines:**

| Request Count | Recommended Parallelism | Expected Speedup |
|---------------|-------------------------|------------------|
| 1-10          | 3-5                     | 2-3x             |
| 11-50         | 5-10                    | 4-6x             |
| 51-100        | 10-15                   | 7-10x            |
| 100+          | 15-20                   | 10-15x           |

**Considerations:**
- Higher parallelism increases throughput but may hit rate limits
- Monitor provider rate limits
- Use exponential backoff for retries
- Consider splitting very large batches into chunks

### Streaming Batch Results

For better responsiveness, stream results as they complete:

```csharp
await foreach (var result in batchProcessor.ProcessBatchStreamAsync(requests, options))
{
    // Process each result immediately as it completes
    await HandleResultAsync(result);
}
```

**Benefits:**
- Start processing results immediately
- Better user experience (progressive results)
- Lower memory footprint

---

## Token Management

### Automatic Token Counting

Track token usage to optimize costs and avoid context window limits:

```csharp
services.AddSingleton<ITokenCounter, DefaultTokenCounter>();
```

**Usage:**
```csharp
var tokenCounter = serviceProvider.GetRequiredService<ITokenCounter>();

// Count before sending
int tokens = tokenCounter.CountMessageTokens(messages, "gpt-4");
int contextWindow = tokenCounter.GetContextWindowSize("gpt-4");

Console.WriteLine($"Using {tokens} of {contextWindow} tokens ({(double)tokens/contextWindow:P})");

// Estimate cost
decimal inputCost = (tokens / 1000m) * 0.03m; // $0.03 per 1K tokens (GPT-4)
```

### Context Window Optimization

Prevent context window overflow:

```csharp
services.AddSingleton<IContextWindowOptimizer, DefaultContextWindowOptimizer>();
```

**Strategy Comparison:**

| Strategy | Speed | Quality | Use Case |
|----------|-------|---------|----------|
| TruncateOldest | Fast | Low | Quick conversations |
| SummarizeOlder | Slow | High | Important context |
| KeepSystemAndRecent | Fast | Medium | Most conversations |
| SmartSelection | Medium | High | Mixed importance |

**Usage:**
```csharp
var optimizer = serviceProvider.GetRequiredService<IContextWindowOptimizer>();

if (!tokenCounter.FitsInContextWindow(messages, maxResponseTokens: 1000, "gpt-4"))
{
    messages = await optimizer.OptimizeForContextWindowAsync(
        messages,
        "gpt-4",
        maxResponseTokens: 1000,
        strategy: ContextOptimizationStrategy.KeepSystemAndRecent
    );
}
```

**Best Practices:**
1. **Always check context window** before sending requests
2. **Use summarization** for important historical context
3. **Keep system prompts** - they're usually small but crucial
4. **Monitor token usage** trends to detect issues early
5. **Set appropriate max_tokens** to leave room for responses

### Token Usage Monitoring

```csharp
public class TokenUsageMiddleware : ChatMiddlewareBase
{
    private readonly ITokenCounter _tokenCounter;
    private readonly ILogger _logger;
    
    public override async Task<ChatResponse> InvokeAsync(
        ChatMiddlewareContext context,
        ChatMiddlewareDelegate next)
    {
        var inputTokens = _tokenCounter.CountMessageTokens(context.Messages, modelId);
        
        var response = await next(context);
        
        var outputTokens = _tokenCounter.CountTokens(response.Content, modelId);
        var totalTokens = inputTokens + outputTokens;
        
        _logger.LogInformation("Request: {Input} tokens, Response: {Output} tokens, Total: {Total}",
            inputTokens, outputTokens, totalTokens);
        
        // Store in context for billing/analytics
        context.Properties["TokenUsage"] = new TokenUsageInfo
        {
            MessageTokens = inputTokens,
            ResponseTokens = outputTokens,
            TotalTokens = totalTokens
        };
        
        return response;
    }
}
```

---

## Streaming Optimization

### Backpressure Control

Prevent overwhelming consumers when streaming:

```csharp
services.AddSingleton<IBackpressureController, DefaultBackpressureController>();
```

**Usage:**
```csharp
var controller = new DefaultBackpressureController(new BackpressureOptions
{
    BufferCapacity = 1000,
    BackpressureThreshold = 0.8
});

await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    if (controller.IsBackpressureActive)
    {
        await controller.WaitForAvailabilityAsync();
    }
    
    await ProcessTokenAsync(token);
    controller.SignalDataConsumed();
}
```

**Performance Characteristics:**
- **Without backpressure:** Tokens buffered indefinitely, high memory usage
- **With backpressure:** Controlled buffer, stable memory, optimal throughput

### Streaming Best Practices

```csharp
public async IAsyncEnumerable<string> OptimizedStreamAsync(
    IEnumerable<ChatMessage> messages,
    [EnumeratorCancellation] CancellationToken cancellationToken = default)
{
    var buffer = new StringBuilder();
    const int FlushThreshold = 10; // Flush every 10 tokens
    
    await foreach (var token in _chatModel.StreamResponseAsync(messages, cancellationToken: cancellationToken))
    {
        buffer.Append(token);
        
        // Batch tokens to reduce overhead
        if (buffer.Length >= FlushThreshold)
        {
            yield return buffer.ToString();
            buffer.Clear();
        }
    }
    
    // Flush remaining
    if (buffer.Length > 0)
    {
        yield return buffer.ToString();
    }
}
```

**Benefits:**
- Reduced network overhead
- Better UI responsiveness (chunked updates)
- Lower CPU usage from fewer updates

---

## Memory Management

### Conversation State Management

Efficiently manage long-running conversations:

```csharp
services.AddScoped<IConversationStateManager, DefaultConversationStateManager>();
```

**Usage:**
```csharp
var stateManager = serviceProvider.GetRequiredService<IConversationStateManager>();

// Trim old messages automatically
await stateManager.TrimConversationAsync(conversationId, keepLastN: 20);

// Archive completed conversations
await stateManager.ArchiveConversationAsync(conversationId);
```

**Memory Optimization Guidelines:**

| Conversation Length | Messages to Keep | Strategy |
|---------------------|------------------|----------|
| Short (< 10)        | All              | No trimming |
| Medium (10-50)      | Last 20          | Simple trim |
| Long (50-100)       | Last 30 + Summary| Summarize old |
| Very Long (100+)    | Last 20 + Summary| Aggressive summary |

### Short-Term and Long-Term Memory

```csharp
services.AddSingleton<IMemoryStore, DefaultMemoryStore>();
```

**Usage:**
```csharp
var memoryStore = serviceProvider.GetRequiredService<IMemoryStore>();

// Store working memory (recent context)
await memoryStore.StoreMemoryAsync(new Memory
{
    Content = "User mentioned they prefer JSON format",
    Type = MemoryType.ShortTerm,
    Importance = 0.7,
    ConversationId = conversationId
});

// Consolidate important memories
var consolidated = await memoryStore.ConsolidateMemoriesAsync(
    new ConsolidationCriteria
    {
        MinImportance = 0.8,
        MinAccessCount = 3,
        MaxAgeHours = 24
    }
);

// Clear short-term memory periodically
await memoryStore.ClearShortTermMemoryAsync();
```

**Performance Impact:**
- Short-term memory: Fast access (~1-5ms)
- Long-term memory: Slightly slower (~5-20ms) but persistent
- Consolidation: Reduces memory footprint by 60-80%

---

## Cost Optimization

### Model Selection Strategy

Choose the right model for each task:

| Task Type | Recommended Model | Cost/1K Tokens | Use Case |
|-----------|-------------------|----------------|----------|
| Simple Q&A | GPT-3.5-turbo | $0.001-0.002 | FAQs, basic queries |
| Complex reasoning | GPT-4 | $0.03-0.06 | Analysis, code generation |
| Long context | Claude 3 Sonnet | $0.003-0.015 | Document analysis |
| Fast inference | Gemini Pro | $0.001-0.004 | Real-time apps |

### Cost Tracking

```csharp
public class CostTrackingMiddleware : ChatMiddlewareBase
{
    private readonly ITokenCounter _tokenCounter;
    private readonly ILogger _logger;
    
    // Model pricing (per 1K tokens)
    private readonly Dictionary<string, (decimal Input, decimal Output)> _pricing = new()
    {
        ["gpt-4"] = (0.03m, 0.06m),
        ["gpt-3.5-turbo"] = (0.001m, 0.002m),
        ["claude-3-sonnet"] = (0.003m, 0.015m),
        ["gemini-pro"] = (0.001m, 0.004m)
    };
    
    public override async Task<ChatResponse> InvokeAsync(
        ChatMiddlewareContext context,
        ChatMiddlewareDelegate next)
    {
        var modelId = context.ChatModel?.GetType().Name ?? "unknown";
        var inputTokens = _tokenCounter.CountMessageTokens(context.Messages, modelId);
        
        var response = await next(context);
        
        var outputTokens = _tokenCounter.CountTokens(response.Content, modelId);
        
        if (_pricing.TryGetValue(modelId, out var price))
        {
            var inputCost = (inputTokens / 1000m) * price.Input;
            var outputCost = (outputTokens / 1000m) * price.Output;
            var totalCost = inputCost + outputCost;
            
            _logger.LogInformation("Request cost: ${Cost:F6} (Input: ${InputCost:F6}, Output: ${OutputCost:F6})",
                totalCost, inputCost, outputCost);
            
            context.Properties["Cost"] = totalCost;
        }
        
        return response;
    }
}
```

### Cost Reduction Strategies

1. **Use Semantic Caching**
   - Saves 40-60% on redundant queries
   - ROI: ~$0.10-0.50 per 1000 cached requests

2. **Implement Smart Model Routing**
   ```csharp
   public async Task<ChatResponse> SmartRoutingAsync(IEnumerable<ChatMessage> messages)
   {
       var complexity = AnalyzeComplexity(messages);
       
       var model = complexity switch
       {
           < 0.3 => _gpt35Model,    // Simple: $0.001/1K
           < 0.7 => _claudeModel,    // Medium: $0.003/1K
           _ => _gpt4Model          // Complex: $0.03/1K
       };
       
       return await model.GetResponseAsync(messages);
   }
   ```

3. **Optimize Context Windows**
   - Trim unnecessary messages
   - Use summarization for historical context
   - Saves 20-40% on token costs

4. **Batch Processing**
   - Reduces overhead
   - Better rate limit utilization
   - 10-20% cost savings from efficiency

5. **Set Max Token Limits**
   ```csharp
   var options = new ChatRequestOptions
   {
       MaxTokens = 500  // Limit response length
   };
   ```

---

## Benchmarking and Monitoring

### Model Performance Comparison

```csharp
services.AddSingleton<IModelBenchmark, DefaultModelBenchmark>();
```

**Usage:**
```csharp
var benchmark = serviceProvider.GetRequiredService<IModelBenchmark>();

var models = new[]
{
    new ModelConfig { ModelId = "gpt-4", Provider = "OpenAI", ChatModel = gpt4 },
    new ModelConfig { ModelId = "gpt-3.5", Provider = "OpenAI", ChatModel = gpt35 },
    new ModelConfig { ModelId = "claude-3", Provider = "Anthropic", ChatModel = claude }
};

var testCases = new[]
{
    new BenchmarkTestCase
    {
        Id = "simple-qa",
        Description = "Simple question",
        Messages = new[] { new ChatMessage(ChatRole.User, "What is 2+2?") }
    },
    new BenchmarkTestCase
    {
        Id = "complex-reasoning",
        Description = "Complex reasoning task",
        Messages = new[] { new ChatMessage(ChatRole.User, "Explain quantum entanglement") }
    }
};

var report = await benchmark.RunBenchmarkAsync(models, testCases, new BenchmarkOptions
{
    Iterations = 5,
    WarmUp = true
});

// Analyze results
foreach (var result in report.Results)
{
    Console.WriteLine($"\n{result.Model.ModelId}:");
    Console.WriteLine($"  Avg Response Time: {result.Statistics.AverageResponseTime.TotalMilliseconds}ms");
    Console.WriteLine($"  P95 Response Time: {result.Statistics.P95ResponseTime.TotalMilliseconds}ms");
    Console.WriteLine($"  Success Rate: {result.Statistics.SuccessRate:P}");
    Console.WriteLine($"  Tokens/Second: {result.Statistics.AverageTokensPerSecond:F2}");
    Console.WriteLine($"  Estimated Cost: ${result.Statistics.EstimatedCost:F6}");
}

Console.WriteLine($"\nFastest: {report.Summary.FastestModel}");
Console.WriteLine($"Most Reliable: {report.Summary.MostReliableModel}");
Console.WriteLine($"Most Cost Efficient: {report.Summary.MostCostEfficient}");
```

### Performance Monitoring

```csharp
services.AddSingleton<IPerformanceMonitor, DefaultPerformanceMonitor>();
```

**Usage:**
```csharp
var monitor = serviceProvider.GetRequiredService<IPerformanceMonitor>();

using (var operation = monitor.StartOperation("ChatCompletion"))
{
    var response = await _chatModel.GetResponseAsync(messages);
    
    monitor.RecordMetric("ResponseLength", response.Content.Length);
    monitor.RecordMetric("TokensUsed", response.Usage?.TotalTokens ?? 0);
}

monitor.IncrementCounter("TotalRequests");
monitor.IncrementCounter("SuccessfulRequests");
```

### Key Performance Indicators (KPIs)

Monitor these metrics:

| Metric | Target | Action If Exceeded |
|--------|--------|--------------------|
| Avg Response Time | < 2s | Optimize prompts, use faster model |
| P95 Response Time | < 5s | Check for outliers, add caching |
| Cache Hit Rate | > 40% | Increase TTL, use semantic cache |
| Error Rate | < 1% | Add retries, check rate limits |
| Cost per Request | Varies | Optimize model selection, caching |
| Tokens per Request | < 2000 | Trim context, optimize prompts |

---

## Complete Optimization Example

```csharp
public class HighPerformanceChatService
{
    private readonly IChatModel _chatModel;
    private readonly ISemanticCache _cache;
    private readonly ITokenCounter _tokenCounter;
    private readonly IContextWindowOptimizer _optimizer;
    private readonly IPerformanceMonitor _monitor;
    
    public async Task<ChatResponse> GetOptimizedResponseAsync(
        IEnumerable<ChatMessage> messages,
        string? conversationId = null,
        CancellationToken cancellationToken = default)
    {
        using var operation = _monitor.StartOperation("OptimizedChatCompletion");
        
        // 1. Check semantic cache first
        var cached = await _cache.GetSemanticAsync(messages, similarityThreshold: 0.95, cancellationToken);
        if (cached != null)
        {
            _monitor.IncrementCounter("CacheHits");
            return cached.Response;
        }
        _monitor.IncrementCounter("CacheMisses");
        
        // 2. Optimize context window
        var messageList = messages.ToList();
        if (!_tokenCounter.FitsInContextWindow(messageList, maxResponseTokens: 1000, "gpt-4"))
        {
            messageList = (await _optimizer.OptimizeForContextWindowAsync(
                messageList,
                "gpt-4",
                maxResponseTokens: 1000,
                strategy: ContextOptimizationStrategy.KeepSystemAndRecent
            )).ToList();
        }
        
        // 3. Get response
        var response = await _chatModel.GetResponseAsync(messageList, cancellationToken: cancellationToken);
        
        // 4. Cache the result
        await _cache.SetSemanticAsync(
            messageList,
            options: null,
            response,
            ttl: TimeSpan.FromHours(1),
            tags: new[] { conversationId ?? "general" },
            cancellationToken: cancellationToken
        );
        
        // 5. Record metrics
        _monitor.RecordMetric("ResponseLength", response.Content.Length);
        _monitor.RecordMetric("TokensUsed", response.Usage?.TotalTokens ?? 0);
        
        return response;
    }
}
```

---

## Summary

### Quick Wins (Immediate Impact)
1. ✅ Enable response caching (40-60% cost savings)
2. ✅ Use batch processing for multiple requests (2-10x speedup)
3. ✅ Set max_tokens limits (20-30% cost savings)
4. ✅ Monitor token usage (visibility for optimization)

### Medium-Term Optimizations
1. ✅ Implement semantic caching (15-30% better hit rate)
2. ✅ Add context window optimization (prevent errors, reduce tokens)
3. ✅ Use smart model routing (match complexity to cost)
4. ✅ Trim conversation history (memory and token savings)

### Advanced Optimizations
1. ✅ Implement middleware pipeline (custom optimization logic)
2. ✅ Use memory abstractions (better context management)
3. ✅ Add performance monitoring (data-driven optimization)
4. ✅ Run regular benchmarks (choose optimal models)

### Expected Results

With full optimization:
- **Response time:** 50-70% reduction (via caching)
- **Cost:** 40-60% reduction (via caching + optimization)
- **Throughput:** 5-10x increase (via batch processing)
- **Reliability:** 99%+ (via retries + failover)

---

## Additional Resources

- [Roadmap Implementation](ROADMAP-IMPLEMENTATION.md)
- [Code Examples](code-examples.md)
- [Architecture Guide](architecture.md)
- [API Reference](API-Reference.md)
