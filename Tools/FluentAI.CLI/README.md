# FluentAI.NET CLI Tool

Interactive command-line interface for testing, benchmarking, and debugging FluentAI.NET AI models.

## Installation

### Install as Global Tool

```bash
dotnet tool install --global FluentAI.CLI
```

### Install from Source

```bash
cd Tools/FluentAI.CLI
dotnet build
dotnet run -- [command] [options]
```

## Configuration

The CLI uses the same configuration as the main FluentAI.NET SDK. Configure via:

1. **Environment Variables** (Recommended):
```bash
export OPENAI_API_KEY="your-key"
export ANTHROPIC_API_KEY="your-key"
export GOOGLE_API_KEY="your-key"
```

2. **appsettings.json** in the current directory

## Commands

### Interactive Chat

Start an interactive chat session:

```bash
fluentai chat
fluentai chat --provider OpenAI --model gpt-4
fluentai chat --system "You are a helpful coding assistant"
```

**Options:**
- `--provider, -p`: AI provider (OpenAI, Anthropic, Google)
- `--model, -m`: Specific model to use
- `--system, -s`: System prompt

**Chat Commands:**
- Type your message and press Enter
- `exit` or `quit`: End the session
- `clear`: Clear conversation history

### Benchmark Models

Compare performance across models:

```bash
fluentai benchmark
fluentai benchmark --prompt "Explain quantum computing" --iterations 5
```

**Options:**
- `--prompt, -p`: Test prompt
- `--iterations, -i`: Number of iterations (default: 3)

**Metrics:**
- Success rate
- Average/min/max response time
- Token usage
- Response length

### Stream Visualization

Visualize real-time token streaming:

```bash
fluentai stream
fluentai stream --prompt "Write a poem about AI"
```

**Metrics:**
- Total time
- Time to first token
- Tokens streamed
- Tokens per second

### Configuration Management

View and validate configuration:

```bash
# Show current configuration
fluentai config show

# Validate configuration
fluentai config validate
```

### Diagnostics

Run system diagnostics:

```bash
# Test connectivity
fluentai diagnostics test

# System health check
fluentai diagnostics health
```

## Examples

### Quick Test

```bash
# Test default provider
fluentai chat

# Quick benchmark
fluentai benchmark --prompt "What is AI?" --iterations 3
```

### Performance Testing

```bash
# Test streaming performance
fluentai stream --prompt "Write a detailed explanation of machine learning"

# Benchmark multiple iterations
fluentai benchmark --iterations 10
```

### Troubleshooting

```bash
# Check configuration
fluentai config show

# Validate setup
fluentai config validate

# Test connectivity
fluentai diagnostics test
```

## Features

### ✅ Implemented
- Interactive chat with conversation history
- Model benchmarking and comparison
- Real-time streaming visualization
- Configuration management
- Connectivity diagnostics
- Performance metrics
- Error handling and reporting

### 🔄 Planned
- Multi-provider comparison in single session
- Custom test suite execution
- Export results to JSON/CSV
- Token usage tracking and reporting
- Cost estimation

## Requirements

- .NET 8.0 or later
- Valid API keys for AI providers
- Internet connection

## Troubleshooting

### "Configuration Error"
- Ensure `appsettings.json` exists or environment variables are set
- Check `AiSdk:DefaultProvider` is set to a valid provider

### "API Key Not Found"
- Set environment variables: `OPENAI_API_KEY`, `ANTHROPIC_API_KEY`, `GOOGLE_API_KEY`
- Or configure in `appsettings.json` (not recommended for production)

### "Provider Not Available"
- Ensure the provider package is installed
- Check network connectivity
- Verify API key is valid

## Support

- [Documentation](../../docs/)
- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Examples](../../Examples/)
