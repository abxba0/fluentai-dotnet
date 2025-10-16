# FluentAI.NET Console Application Template

A ready-to-use console application template with FluentAI.NET integrated.

## Features

- ✅ Pre-configured dependency injection
- ✅ Support for multiple AI providers (OpenAI, Anthropic, Google)
- ✅ Configuration management via appsettings.json
- ✅ Environment variable support
- ✅ Error handling and diagnostics

## Getting Started

### 1. Configure API Keys

Set your API keys via environment variables:

```bash
export OPENAI_API_KEY="your-openai-key"
export ANTHROPIC_API_KEY="your-anthropic-key"
export GOOGLE_API_KEY="your-google-key"
```

### 2. Update Configuration

Edit `appsettings.json` to set your preferred default provider and model:

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

### 3. Run the Application

```bash
dotnet run
```

## Project Structure

```
FluentAI.Templates.Console/
├── Program.cs              # Main application entry point
├── appsettings.json        # Configuration file
└── *.csproj                # Project file with FluentAI.NET reference
```

## Usage Examples

### Basic Chat

```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.User, "Your question here")
};

var response = await chatModel.GetResponseAsync(messages);
Console.WriteLine(response.Content);
```

### Streaming Responses

```csharp
await foreach (var token in chatModel.StreamResponseAsync(messages))
{
    Console.Write(token);
}
```

### With System Prompt

```csharp
var messages = new[]
{
    new ChatMessage(ChatRole.System, "You are a helpful coding assistant."),
    new ChatMessage(ChatRole.User, "How do I use async/await in C#?")
};

var response = await chatModel.GetResponseAsync(messages);
```

## Customization

### Add Performance Monitoring

```csharp
services.AddSingleton<IPerformanceMonitor, DefaultPerformanceMonitor>();
```

### Add Caching

```csharp
services.AddSingleton<IResponseCache, MemoryResponseCache>();
```

### Add Security Features

```csharp
services.AddSingleton<IInputSanitizer, DefaultInputSanitizer>();
```

## Troubleshooting

### "Configuration Error"
- Ensure `appsettings.json` exists and contains an `AiSdk` section
- Set `AiSdk:DefaultProvider` to one of: `OpenAI`, `Anthropic`, or `Google`

### "API Key Not Found"
- Set environment variables for your chosen provider
- Verify the key names match: `OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, `GOOGLE_API_KEY`

### "Provider Not Available"
- Ensure the provider is registered in `Program.cs`
- Check that FluentAI.NET package is installed

## Next Steps

- Add conversation state management
- Implement RAG (Retrieval-Augmented Generation)
- Add multi-modal support (images, audio)
- Integrate with databases or external APIs

## Documentation

- [FluentAI.NET Documentation](https://github.com/abxba0/fluentai-dotnet/tree/main/docs)
- [API Reference](https://github.com/abxba0/fluentai-dotnet/blob/main/docs/API-Reference.md)
- [Code Examples](https://github.com/abxba0/fluentai-dotnet/blob/main/docs/code-examples.md)

## Support

- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Contributing Guide](https://github.com/abxba0/fluentai-dotnet/blob/main/CONTRIBUTING.md)
