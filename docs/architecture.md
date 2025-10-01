# FluentAI.NET Architecture

## Overview

FluentAI.NET is designed with a modular, extensible architecture that prioritizes separation of concerns, testability, and enterprise-grade reliability.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      Application Layer                          │
│  (ASP.NET Core, Blazor, Console Apps, Azure Functions, etc.)   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    FluentAI.NET SDK                             │
│                                                                  │
│  ┌────────────────────────────────────────────────────────┐   │
│  │              Core Abstractions Layer                    │   │
│  │  • IChatModel (unified chat interface)                 │   │
│  │  • IRagService (RAG operations)                        │   │
│  │  • IEmbeddingGenerator (text embeddings)              │   │
│  │  • IVectorDatabase (vector storage)                    │   │
│  │  • IDocumentProcessor (document handling)              │   │
│  │  • IPerformanceMonitor (metrics)                       │   │
│  │  • IInputSanitizer (security)                          │   │
│  │  • IPiiDetectionService (PII detection)               │   │
│  └────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────┐   │
│  │              Provider Implementations                   │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │   │
│  │  │   OpenAI     │  │  Anthropic   │  │  Google AI   │ │   │
│  │  │   Provider   │  │   Provider   │  │   Provider   │ │   │
│  │  └──────────────┘  └──────────────┘  └──────────────┘ │   │
│  └────────────────────────────────────────────────────────┘   │
│                              │                                   │
│  ┌────────────────────────────────────────────────────────┐   │
│  │              Cross-Cutting Concerns                     │   │
│  │  • Configuration (IOptions pattern)                     │   │
│  │  • Logging (ILogger)                                   │   │
│  │  • Dependency Injection                                 │   │
│  │  • Error Handling & Resilience                         │   │
│  │  • Performance Monitoring                               │   │
│  │  • Security & Compliance                                │   │
│  └────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    External Services                             │
│  • OpenAI API      • Anthropic API      • Google AI API        │
│  • Vector Databases (Pinecone, Weaviate, Chroma, etc.)        │
└─────────────────────────────────────────────────────────────────┘
```

## Core Components

### 1. Abstractions Layer

The abstractions layer defines contracts that enable loose coupling and testability:

- **IChatModel**: Universal interface for chat completions across all providers
- **IRagService**: Contract for Retrieval Augmented Generation operations
- **IEmbeddingGenerator**: Interface for generating text embeddings
- **IVectorDatabase**: Contract for vector storage and similarity search
- **IDocumentProcessor**: Interface for document parsing and chunking
- **IPerformanceMonitor**: Contract for performance metrics and monitoring
- **IInputSanitizer**: Interface for input validation and sanitization
- **IPiiDetectionService**: Contract for PII detection and remediation

### 2. Provider Implementations

Each AI provider is implemented as a separate service following the same interface:

#### OpenAI Provider
- Supports GPT-4, GPT-3.5-turbo models
- Streaming responses
- Function calling (MCP tools)
- Embeddings (text-embedding-ada-002)

#### Anthropic Provider
- Supports Claude 3 family models
- Long context windows
- Safety-focused design

#### Google AI Provider
- Supports Gemini models
- Multi-modal capabilities
- Cost-effective options

### 3. RAG (Retrieval Augmented Generation) System

The RAG system consists of multiple cooperating components:

```
┌──────────────────────────────────────────────────────────────┐
│                     RAG Architecture                          │
│                                                                │
│  Document Input → IDocumentProcessor → Chunks                │
│                                          ↓                     │
│                                   IEmbeddingGenerator          │
│                                          ↓                     │
│                                   Vector Embeddings            │
│                                          ↓                     │
│                                   IVectorDatabase              │
│                                                                │
│  Query → IEmbeddingGenerator → Query Vector                  │
│                                          ↓                     │
│                              Similarity Search                 │
│                                          ↓                     │
│                              Retrieved Context                 │
│                                          ↓                     │
│                         IChatModel (with context)             │
│                                          ↓                     │
│                              Generated Response                │
└──────────────────────────────────────────────────────────────┘
```

**Key Features:**
- Automatic document chunking with configurable strategies
- Multiple vector database support (in-memory, Pinecone, etc.)
- Context-aware response generation
- Streaming support for real-time responses

### 4. Security System

Multi-layered security architecture:

```
┌──────────────────────────────────────────────────────────────┐
│                  Security Architecture                        │
│                                                                │
│  User Input                                                   │
│       ↓                                                       │
│  IInputSanitizer                                             │
│   • Prompt injection detection                               │
│   • Content validation                                       │
│   • Risk assessment                                          │
│       ↓                                                       │
│  IPiiDetectionService                                        │
│   • Pattern-based detection                                  │
│   • ML-based classification                                  │
│   • Confidence scoring                                       │
│       ↓                                                       │
│  PII Remediation                                             │
│   • Redaction                                                │
│   • Tokenization                                             │
│   • Masking                                                  │
│       ↓                                                       │
│  Sanitized Content → AI Provider                             │
└──────────────────────────────────────────────────────────────┘
```

### 5. Performance & Resilience

Built-in patterns for production workloads:

**Response Caching:**
- In-memory cache with configurable TTL
- Cache key generation based on messages + options
- Automatic cleanup of expired entries

**Failover & Circuit Breaking:**
- Automatic provider failover
- Circuit breaker pattern for transient failures
- Configurable retry policies

**Rate Limiting:**
- Token bucket algorithm
- Per-provider rate limits
- Graceful degradation

**Performance Monitoring:**
- Operation timing and metrics
- Resource usage tracking
- Health checks

## Configuration Architecture

FluentAI.NET uses the IOptions pattern for type-safe configuration:

```csharp
public class AiSdkOptions
{
    public string DefaultProvider { get; set; }
    public FailoverOptions? Failover { get; set; }
}

public class OpenAiOptions
{
    public string ApiKey { get; set; }
    public string Model { get; set; }
    // ... additional options
}
```

Configuration sources (in order of precedence):
1. Code-based configuration
2. Environment variables
3. appsettings.json
4. Azure Key Vault / AWS Secrets Manager

## Dependency Injection

All services are registered and resolved through DI:

```csharp
services
    .AddFluentAI()
    .AddOpenAI(config => config.ApiKey = "...")
    .UseDefaultProvider("OpenAI");

// RAG services
services
    .AddRagServices(Configuration)
    .AddInMemoryVectorDatabase()
    .AddOpenAiEmbeddings()
    .EnableRagEnhancement();
```

## Extension Points

The architecture supports extensibility at multiple levels:

### Custom Providers
Implement `IChatModel` to add new AI providers:
```csharp
public class MyCustomProvider : IChatModel
{
    public async Task<ChatResponse> GenerateResponseAsync(...)
    {
        // Custom implementation
    }
}
```

### Custom Vector Databases
Implement `IVectorDatabase` for custom storage:
```csharp
public class PineconeVectorDatabase : IVectorDatabase
{
    // Pinecone-specific implementation
}
```

### Custom Document Processors
Implement `IDocumentProcessor` for specialized formats:
```csharp
public class PdfDocumentProcessor : IDocumentProcessor
{
    // PDF parsing logic
}
```

## Threading & Async Model

All I/O operations are async with proper cancellation support:
- `CancellationToken` parameters throughout
- ConfigureAwait(false) in library code
- Thread-safe implementations using concurrent collections
- No async void methods

## Error Handling Strategy

Structured error handling with custom exceptions:

```csharp
AiSdkConfigurationException  // Configuration errors
AiProviderException           // Provider-specific errors
RagException                  // RAG operation errors
SecurityException             // Security violations
```

All exceptions include:
- Detailed error messages
- Context information
- Inner exceptions for debugging

## Testing Strategy

The architecture enables comprehensive testing:

- **Unit Tests**: Mock interfaces for isolated testing
- **Integration Tests**: Test real provider interactions
- **Performance Tests**: Benchmark critical paths
- **Security Tests**: Validate security controls

## Performance Characteristics

### Response Times
- **Cached responses**: < 1ms
- **Simple completions**: 500ms - 2s (provider dependent)
- **Streaming**: First token in ~300ms
- **RAG queries**: 1-3s (including retrieval)

### Resource Usage
- **Memory**: ~50MB baseline, +2MB per cached response
- **CPU**: Minimal (most work on provider side)
- **Network**: Efficient chunked transfer encoding for streaming

## Deployment Considerations

### Supported Platforms
- .NET 8.0+
- Windows, Linux, macOS
- Docker containers
- Kubernetes
- Azure App Service / Functions
- AWS Lambda / ECS

### Scaling Patterns
- Stateless design enables horizontal scaling
- Response caching reduces provider API calls
- Rate limiting prevents quota exhaustion
- Circuit breakers protect against cascading failures

## Best Practices

1. **Always use dependency injection** for service registration
2. **Configure providers via IOptions** for testability
3. **Enable response caching** for frequently asked questions
4. **Use failover** for production workloads
5. **Monitor performance metrics** to track health
6. **Enable security features** for user-facing applications
7. **Test with mock providers** before going to production
8. **Use streaming** for better UX in interactive scenarios

## Future Enhancements

Planned architectural improvements:
- Distributed caching (Redis, Memcached)
- Event sourcing for audit trails
- GraphQL API for complex queries
- Plugin system for community extensions
- Advanced telemetry (OpenTelemetry)
- Multi-tenant support with isolation

## Related Documentation

- [API Reference](API-Reference.md)
- [Integration Guides](integration/README.md)
- [Troubleshooting](integration/troubleshooting.md)
- [Security Guide](../SECURITY.md)
