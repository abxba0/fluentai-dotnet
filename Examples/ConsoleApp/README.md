# FluentAI.NET Console Example

This example demonstrates how to use FluentAI.NET in a console application with:

- Dependency injection setup
- Multiple provider configuration (OpenAI and Anthropic)
- Interactive chat with streaming support
- Proper error handling
- Configuration-based setup

## Setup

1. Set your API keys as environment variables:
   ```bash
   export OPENAI_API_KEY="your-openai-api-key"
   export ANTHROPIC_API_KEY="your-anthropic-api-key"
   ```

2. Run the example:
   ```bash
   dotnet run
   ```

## Features Demonstrated

- **Provider switching** - Default provider configuration
- **Streaming responses** - Type "stream" to toggle streaming mode
- **Chat history** - Maintains conversation context
- **Error handling** - Graceful error handling with logging
- **Token usage** - Displays token usage information

## Commands

- Type any message to chat with the AI
- Type `stream` to toggle between normal and streaming responses
- Type `exit` to quit the application