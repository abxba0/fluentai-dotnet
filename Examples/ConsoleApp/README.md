# FluentAI.NET Console Example

This comprehensive console application demonstrates the full capabilities of the FluentAI.NET SDK through an interactive menu system.

## Features Demonstrated

### 🚀 Quick Start
```bash
cd Examples/ConsoleApp
dotnet run
```

### 📋 Demo Menu Options

1. **💬 Basic Chat Completion Demo**
   - Simple request/response interaction
   - Token usage and model information display
   - Error handling demonstration

2. **🌊 Streaming Response Demo**
   - Real-time token-by-token streaming
   - Visual streaming demonstration
   - Performance comparison with regular responses

3. **🔄 Multi-Provider Comparison**
   - Tests multiple prompts with the configured provider
   - Shows provider-specific features and capabilities
   - Demonstrates unified interface across providers

4. **🔒 Security Features Demo**
   - Input sanitization capabilities
   - Risk assessment and threat detection
   - Security best practices
   - Content filtering demonstrations

5. **⚡ Performance & Caching Demo**
   - Response caching mechanisms
   - Performance monitoring and metrics
   - Memory management demonstrations
   - Benchmark comparisons

6. **⚙️ Configuration Management Demo**
   - Current configuration display
   - Environment variable usage
   - Provider-specific configuration options
   - Configuration validation

7. **🚨 Error Handling & Resilience Demo**
   - Comprehensive error scenarios
   - Retry mechanisms with exponential backoff
   - Rate limiting demonstrations
   - Failover capabilities
   - Validation error handling

8. **🔧 Advanced Features Demo**
   - Overview of all SDK capabilities
   - Feature matrix and descriptions
   - Integration patterns

9. **💻 Interactive Chat (Original Demo)**
   - Free-form chat interface
   - Streaming mode toggle
   - Conversation history maintenance

## Configuration

### API Keys Setup
Set your API keys as environment variables:

```bash
# OpenAI
export OPENAI_API_KEY="your-openai-api-key"

# Anthropic
export ANTHROPIC_API_KEY="your-anthropic-api-key"

# Google AI
export GOOGLE_API_KEY="your-google-api-key"

# HuggingFace
export HUGGINGFACE_API_KEY="your-huggingface-api-key"
```

### Configuration Options
The application reads from `appsettings.json` which includes:

- **Provider Configuration**: Model selection, timeouts, token limits
- **Rate Limiting**: Request limits and time windows
- **Failover Settings**: Primary and fallback provider configuration
- **Logging**: Debug levels for different components

## Architecture

### Service Registration
The application demonstrates proper dependency injection setup:

```csharp
services.AddAiSdk(context.Configuration);
services.AddOpenAiChatModel(context.Configuration);
services.AddAnthropicChatModel(context.Configuration);
services.AddGoogleGeminiChatModel(context.Configuration);
services.AddHuggingFaceChatModel(context.Configuration);
```

### Demo Services
- **DemoService**: Main menu coordinator
- **ProviderDemoService**: Multi-provider demonstrations
- **SecurityDemoService**: Security feature showcase
- **PerformanceDemoService**: Performance and caching demos
- **ConfigurationDemoService**: Configuration management
- **ErrorHandlingDemoService**: Resilience and error handling

## Educational Value

This console application serves as:

1. **Learning Tool**: Understand all SDK features through hands-on demos
2. **Integration Example**: See how to properly set up and use FluentAI.NET
3. **Best Practices Guide**: Learn recommended patterns and configurations
4. **Troubleshooting Aid**: Test configurations and identify issues
5. **Feature Exploration**: Discover capabilities before integrating into projects

## Prerequisites

- .NET 8.0 or later
- At least one AI provider API key
- Internet connection for API calls

## Next Steps

After exploring the console demo:

1. Review the integration guides for your project type
2. Check the main SDK documentation for detailed API reference
3. Explore the test suite for additional implementation examples
4. Consider the security and performance features for production use

---

**Note**: Some demos require actual API keys to function fully. Without API keys, the application will still run and show the feature overviews and configurations, but won't make actual API calls.