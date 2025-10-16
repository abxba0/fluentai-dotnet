# FluentAI.NET Project Templates

Ready-to-use project templates for quick integration with FluentAI.NET.

## Available Templates

### 1. Console Application
**Location:** `Templates/console/`

A simple console application demonstrating basic FluentAI.NET usage.

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

[📖 View Console Template README](console/README.md)

### 2. ASP.NET Core Web API
**Location:** `Templates/webapi/`

A RESTful API with chat and streaming endpoints.

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

[📖 View Web API Template README](webapi/README.md)

### 3. Blazor Web Application (Coming Soon)
**Location:** `Templates/blazor/`

Interactive web UI with real-time chat.

**Features:**
- Interactive UI components
- Real-time streaming
- State management
- Responsive design

## Using Templates

### Option 1: Copy and Modify

1. Copy the template directory to your project location
2. Rename the project and namespace
3. Configure API keys
4. Run and customize

```bash
# Example
cp -r Templates/console MyFluentAIApp
cd MyFluentAIApp
# Update namespace in files
dotnet run
```

### Option 2: Install as Template (Future)

```bash
# Install template package
dotnet new install FluentAI.Templates

# Create new project from template
dotnet new fluentai-console -n MyApp
dotnet new fluentai-webapi -n MyApi
dotnet new fluentai-blazor -n MyBlazorApp
```

## Template Structure

Each template includes:

```
template-name/
├── *.csproj              # Project file with FluentAI.NET reference
├── Program.cs            # Main application entry point
├── appsettings.json      # Configuration file
└── README.md             # Template-specific documentation
```

## Configuration

All templates use the same configuration structure:

### appsettings.json

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "RequestTimeout": "00:02:00"
  }
}
```

### Environment Variables

```bash
export OPENAI_API_KEY="your-openai-key"
export ANTHROPIC_API_KEY="your-anthropic-key"
export GOOGLE_API_KEY="your-google-key"
```

## Common Customizations

### Add Performance Monitoring

```csharp
services.AddSingleton<IPerformanceMonitor, DefaultPerformanceMonitor>();
```

### Add Response Caching

```csharp
services.AddSingleton<IResponseCache, MemoryResponseCache>();
```

### Add Security Features

```csharp
services.AddSingleton<IInputSanitizer, DefaultInputSanitizer>();
```

### Enable RAG (Retrieval-Augmented Generation)

```csharp
services.AddScoped<IRagService, DefaultRagService>();
services.AddSingleton<IVectorDatabase, InMemoryVectorDatabase>();
services.AddSingleton<IDocumentProcessor, DefaultDocumentProcessor>();
```

## Advanced Templates (Planned)

### RAG-Enabled Application
Features:
- Document processing
- Vector database integration
- Semantic search
- Context-aware responses

### Multi-Modal Application
Features:
- Image analysis
- Audio transcription
- Multi-modal prompts
- Combined media processing

### Microservices Template
Features:
- Service discovery
- API Gateway integration
- Distributed tracing
- Event-driven architecture

## Template Development

### Creating Custom Templates

1. Create your project structure
2. Add `template.json` in `.template.config/` directory
3. Test locally
4. Package and distribute

Example `template.json`:

```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "FluentAI.NET Contributors",
  "classifications": ["FluentAI", "AI", "Console"],
  "identity": "FluentAI.Templates.Console",
  "name": "FluentAI.NET Console Application",
  "shortName": "fluentai-console",
  "tags": {
    "language": "C#",
    "type": "project"
  },
  "sourceName": "FluentAI.Templates.Console"
}
```

## Best Practices

### Security
- ✅ Store API keys in environment variables
- ✅ Never commit secrets to source control
- ✅ Use Azure Key Vault in production
- ✅ Implement rate limiting
- ✅ Validate all user inputs

### Performance
- ✅ Enable response caching
- ✅ Use async/await consistently
- ✅ Implement connection pooling
- ✅ Monitor token usage
- ✅ Set appropriate timeouts

### Architecture
- ✅ Use dependency injection
- ✅ Separate concerns
- ✅ Follow SOLID principles
- ✅ Write testable code
- ✅ Document your code

## Documentation

- [FluentAI.NET Documentation](../docs/)
- [API Reference](../docs/API-Reference.md)
- [Code Examples](../docs/code-examples.md)
- [Integration Guides](../docs/integration/)

## Support

- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Contributing Guide](../CONTRIBUTING.md)
- [Examples](../Examples/)

## Contributing

We welcome template contributions! Please:

1. Follow existing template structure
2. Include comprehensive README
3. Add configuration examples
4. Test thoroughly
5. Submit a pull request

## License

Same as FluentAI.NET - MIT License
