# FluentAI.NET Integration Guide

This guide provides comprehensive instructions for integrating FluentAI.NET into various types of .NET applications.

## Table of Contents

- [Console Applications](console-applications.md)
- [ASP.NET Core Web Applications](aspnet-core.md)
- [Blazor Applications](blazor.md)
- [WPF/WinUI Desktop Applications](wpf-winui.md)
- [Class Libraries](class-libraries.md)
- [Azure Functions](azure-functions.md)
- [Common Patterns & Best Practices](common-patterns.md)
- [Troubleshooting](troubleshooting.md)

## Quick Start

1. **Install the Package**
   ```bash
   dotnet add package FluentAI.NET
   ```

2. **Set API Keys**
   ```bash
   export OPENAI_API_KEY="your-openai-key"
   export ANTHROPIC_API_KEY="your-anthropic-key"
   ```

3. **Configure Services**
   ```csharp
   services.AddAiSdk(Configuration)
       .AddOpenAiChatModel(Configuration)
       .AddAnthropicChatModel(Configuration);
   ```

4. **Use in Your Code**
   ```csharp
   public class MyService
   {
       private readonly IChatModel _chatModel;
       
       public MyService(IChatModel chatModel)
       {
           _chatModel = chatModel;
       }
       
       public async Task<string> GetResponseAsync(string message)
       {
           var messages = new[] { new ChatMessage(ChatRole.User, message) };
           var response = await _chatModel.GetResponseAsync(messages);
           return response.Content;
       }
   }
   ```

## Project-Specific Integration

Each project type has unique considerations for integration. Select your project type from the table of contents above for detailed guidance.

## Support & Resources

- 📖 [Main Documentation](../README.md)
- 🧪 [Console Example](../Examples/ConsoleApp/README.md)
- 🐛 [Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- 💬 [Discussions](https://github.com/abxba0/fluentai-dotnet/discussions)