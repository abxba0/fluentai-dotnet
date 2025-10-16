# FluentAI.NET Developer Tools Guide

Comprehensive guide to FluentAI.NET developer tools for testing, debugging, and rapid prototyping.

## Overview

FluentAI.NET provides three essential developer tools:

1. **CLI Tool** - Interactive command-line interface for testing and benchmarking
2. **Visual Dashboard** - Real-time monitoring and debugging interface
3. **Project Templates** - Quick-start templates for common scenarios

## Table of Contents

- [CLI Tool](#cli-tool)
- [Visual Dashboard](#visual-dashboard)
- [Project Templates](#project-templates)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## CLI Tool

### Overview

The FluentAI CLI is a powerful command-line tool for:
- Interactive model testing
- Performance benchmarking
- Configuration management
- Stream visualization
- Error diagnostics

### Installation

#### Option 1: Run from Source

```bash
cd Tools/FluentAI.CLI
dotnet run -- [command] [options]
```

#### Option 2: Install as Global Tool (Future)

```bash
dotnet tool install --global FluentAI.CLI
fluentai [command] [options]
```

### Commands

#### Interactive Chat

Start an interactive chat session:

```bash
# Default chat
dotnet run -- chat

# With specific provider and model
dotnet run -- chat --provider OpenAI --model gpt-4

# With custom system prompt
dotnet run -- chat --system "You are a helpful coding assistant"
```

**Chat Commands:**
- Type your message and press Enter
- `exit` or `quit` - End the session
- `clear` - Clear conversation history

#### Performance Benchmarking

Compare model performance:

```bash
# Basic benchmark
dotnet run -- benchmark

# Custom prompt and iterations
dotnet run -- benchmark --prompt "Explain quantum computing" --iterations 10
```

**Metrics Tracked:**
- Success rate
- Average/min/max response time
- Token usage
- Response length

#### Stream Visualization

Visualize token streaming:

```bash
# Default streaming
dotnet run -- stream

# Custom prompt
dotnet run -- stream --prompt "Write a poem about AI"
```

**Metrics Displayed:**
- Total time
- Time to first token
- Tokens streamed
- Tokens per second

#### Configuration Management

View and validate configuration:

```bash
# Show current configuration
dotnet run -- config show

# Validate configuration
dotnet run -- config validate
```

#### Diagnostics

Run system diagnostics:

```bash
# Test connectivity
dotnet run -- diagnostics test

# Check system health
dotnet run -- diagnostics health
```

### Configuration

#### appsettings.json

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000
  }
}
```

#### Environment Variables

```bash
export OPENAI_API_KEY="your-key"
export ANTHROPIC_API_KEY="your-key"
export GOOGLE_API_KEY="your-key"
```

### Use Cases

**1. Quick Model Testing**
```bash
dotnet run -- chat --provider OpenAI
```

**2. Performance Comparison**
```bash
dotnet run -- benchmark --iterations 10
```

**3. Streaming Quality Check**
```bash
dotnet run -- stream --prompt "Long response test"
```

**4. Configuration Validation**
```bash
dotnet run -- config validate
dotnet run -- diagnostics test
```

---

## Visual Dashboard

### Overview

The Visual Dashboard provides real-time monitoring of:
- Token usage
- Performance metrics
- Cache statistics
- Memory usage
- Cost tracking

### Getting Started

#### Running the Dashboard

```bash
cd Tools/FluentAI.Dashboard
dotnet run
```

Navigate to `https://localhost:5001`

#### Configuration

##### appsettings.json

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000
  }
}
```

##### Environment Variables

```bash
export OPENAI_API_KEY="your-key"
export ANTHROPIC_API_KEY="your-key"
export GOOGLE_API_KEY="your-key"
```

**Note:** Dashboard works with simulated data if API keys are not configured.

### Dashboard Pages

#### Home Page (`/`)
- Feature overview
- Quick navigation
- Getting started guide

#### Main Dashboard (`/dashboard`)
- Total requests and success rate cards
- Token usage over time
- Response time metrics
- Cache hit/miss visualization
- Memory usage
- Cost estimation

**Key Metrics:**
- Total Requests
- Success Rate
- Total Tokens Used
- Estimated Cost
- Cache Hit Rate
- Memory Usage (MB)

#### Test Requests (`/test`)
- Send individual test requests
- Send batch requests (10 at a time)
- Simulate cache hits/misses
- Choose different models
- View request responses

### Features

**Real-time Updates**
- Dashboard auto-refreshes every 2 seconds
- Metrics update on every request
- Live token consumption tracking

**Simulated Data Mode**
- Works without API keys
- Perfect for development and testing
- Generates realistic metrics

**Interactive Controls**
- Refresh button - Manual refresh
- Reset button - Clear all metrics
- Test interface - Generate sample data

### Integration

#### Adding to Your Application

```csharp
// In your Startup.cs or Program.cs
services.AddSingleton<MetricsCollector>();

// In your service
public class MyService
{
    private readonly MetricsCollector _metrics;

    public MyService(MetricsCollector metrics)
    {
        _metrics = metrics;
    }

    public async Task ProcessRequestAsync()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await _chatModel.GetResponseAsync(messages);
            stopwatch.Stop();
            
            _metrics.RecordRequest(
                response.ModelId,
                response.Usage?.TotalTokens ?? 0,
                stopwatch.ElapsedMilliseconds,
                true,
                estimatedCost
            );
        }
        catch (Exception ex)
        {
            _metrics.RecordRequest("unknown", 0, 0, false);
        }
    }
}
```

### Use Cases

**1. Development Monitoring**
- Track token usage during development
- Monitor performance of different prompts
- Identify bottlenecks

**2. Testing and QA**
- Generate test data with simulated requests
- Validate caching behavior
- Test different model configurations

**3. Production Monitoring**
- Real-time operational visibility
- Cost tracking and estimation
- Performance trending

---

## Project Templates

### Overview

Ready-to-use project templates for:
- Console applications
- ASP.NET Core Web APIs
- Blazor web applications (planned)
- RAG-enabled applications (planned)
- Multi-modal applications (planned)

### Available Templates

#### 1. Console Application

**Location:** `Templates/console/`

**Features:**
- Basic chat functionality
- Configuration management
- Error handling
- Token usage tracking

**Quick Start:**
```bash
cd Templates/console
dotnet run
```

[📖 Detailed Console Template Guide](../../Templates/console/README.md)

#### 2. ASP.NET Core Web API

**Location:** `Templates/webapi/`

**Features:**
- REST API endpoints
- Swagger/OpenAPI documentation
- Streaming support
- Rate limiting
- Production-ready structure

**Quick Start:**
```bash
cd Templates/webapi
dotnet run
```

Navigate to `https://localhost:5001/swagger`

[📖 Detailed Web API Template Guide](../../Templates/webapi/README.md)

### Using Templates

#### Option 1: Copy and Modify

```bash
# Copy template to your project location
cp -r Templates/console MyFluentAIApp
cd MyFluentAIApp

# Configure API keys
export OPENAI_API_KEY="your-key"

# Run
dotnet run
```

#### Option 2: Install as Template (Future)

```bash
# Install template package
dotnet new install FluentAI.Templates

# Create new project
dotnet new fluentai-console -n MyApp
dotnet new fluentai-webapi -n MyApi
```

### Customization

#### Adding Performance Monitoring

```csharp
services.AddSingleton<IPerformanceMonitor, DefaultPerformanceMonitor>();
```

#### Adding Response Caching

```csharp
services.AddSingleton<IResponseCache, MemoryResponseCache>();
```

#### Adding Security Features

```csharp
services.AddSingleton<IInputSanitizer, DefaultInputSanitizer>();
```

#### Enabling RAG

```csharp
services.AddScoped<IRagService, DefaultRagService>();
services.AddSingleton<IVectorDatabase, InMemoryVectorDatabase>();
services.AddSingleton<IDocumentProcessor, DefaultDocumentProcessor>();
```

---

## Best Practices

### CLI Tool

**Testing Strategy:**
1. Start with interactive chat to validate configuration
2. Use benchmarking for performance baselines
3. Test streaming for real-time scenarios
4. Run diagnostics before production deployment

**Performance Tips:**
- Use multiple iterations in benchmarks
- Test with realistic prompts
- Monitor token usage patterns
- Compare different models

### Visual Dashboard

**Development Workflow:**
1. Run dashboard alongside your application
2. Use test interface to generate sample data
3. Monitor metrics during development
4. Validate caching behavior

**Production Monitoring:**
- Set up continuous monitoring
- Track cost trends
- Monitor success rates
- Set alert thresholds

### Project Templates

**Starting a New Project:**
1. Choose appropriate template
2. Copy to your project location
3. Update namespace and project name
4. Configure API keys
5. Customize for your needs

**Production Deployment:**
- Remove unused providers
- Configure production settings
- Enable HTTPS
- Add authentication
- Implement rate limiting

---

## Troubleshooting

### CLI Tool Issues

**"Configuration Error"**
- Check `appsettings.json` exists
- Verify `AiSdk:DefaultProvider` is set
- Ensure provider is valid (OpenAI, Anthropic, Google)

**"API Key Not Found"**
- Set environment variables
- Check variable names
- Verify key format

**"Provider Not Available"**
- Check provider registration in `Program.cs`
- Verify network connectivity
- Validate API key

### Dashboard Issues

**Dashboard Not Updating**
- Check MetricsCollector is singleton
- Verify auto-refresh timer
- Check browser console for errors

**No Metrics Showing**
- Send test requests from `/test` page
- Use "Send 10 Requests" button
- Click "Refresh" button

**"FluentAI services not configured"**
- This is normal without API keys
- Dashboard uses simulated data
- To use real data, configure API keys

### Template Issues

**Build Errors**
- Run `dotnet restore`
- Check .NET 8.0 is installed
- Verify FluentAI.NET package version

**Runtime Errors**
- Check API keys are set
- Verify configuration file exists
- Check provider configuration

---

## Additional Resources

### Documentation
- [ROADMAP-IMPLEMENTATION.md](../ROADMAP-IMPLEMENTATION.md)
- [API Reference](../API-Reference.md)
- [Code Examples](../code-examples.md)

### Tools
- [CLI README](../../Tools/FluentAI.CLI/README.md)
- [Dashboard README](../../Tools/FluentAI.Dashboard/README.md)
- [Templates README](../../Templates/README.md)

### Support
- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Contributing Guide](../../CONTRIBUTING.md)

---

## Contributing

We welcome contributions to developer tools! Areas for improvement:

### CLI Tool
- Additional commands
- Export functionality
- Custom test suites
- Multi-provider comparison

### Dashboard
- Interactive charts
- Historical data storage
- Alert thresholds
- Export to CSV/JSON

### Templates
- Blazor template
- RAG template
- Multi-modal template
- Microservices template

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.
