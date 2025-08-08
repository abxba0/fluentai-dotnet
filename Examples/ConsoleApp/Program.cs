using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FluentAI.Examples.ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("FluentAI.NET Console Example");
        Console.WriteLine("============================");
        
        // Create host builder with dependency injection
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Add FluentAI services
                services.AddFluentAI()
                    .AddOpenAI(config =>
                    {
                        // In a real app, load from configuration
                        config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "your-api-key-here";
                        config.Model = "gpt-3.5-turbo";
                        config.MaxTokens = 150;
                    })
                    .AddAnthropic(config =>
                    {
                        // In a real app, load from configuration  
                        config.ApiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") ?? "your-api-key-here";
                        config.Model = "claude-3-haiku-20240307";
                        config.MaxTokens = 150;
                    })
                    .UseDefaultProvider("OpenAI");
                    
                services.AddTransient<ChatService>();
            });

        using var host = builder.Build();
        
        try
        {
            var chatService = host.Services.GetRequiredService<ChatService>();
            await chatService.RunInteractiveChat();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

public class ChatService
{
    private readonly IChatModel _chatModel;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IChatModel chatModel, ILogger<ChatService> logger)
    {
        _chatModel = chatModel;
        _logger = logger;
    }

    public async Task RunInteractiveChat()
    {
        Console.WriteLine("\nFluentAI Chat Demo");
        Console.WriteLine("Type 'exit' to quit, 'stream' to toggle streaming mode\n");
        
        bool useStreaming = false;
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant. Keep responses concise.")
        };

        while (true)
        {
            Console.Write($"You ({(useStreaming ? "streaming" : "normal")}): ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(input))
                continue;
                
            if (input.ToLower() == "exit")
                break;
                
            if (input.ToLower() == "stream")
            {
                useStreaming = !useStreaming;
                Console.WriteLine($"Switched to {(useStreaming ? "streaming" : "normal")} mode");
                continue;
            }

            messages.Add(new ChatMessage(ChatRole.User, input));

            try
            {
                Console.Write("Assistant: ");
                
                if (useStreaming)
                {
                    var response = "";
                    await foreach (var token in _chatModel.StreamResponseAsync(messages))
                    {
                        Console.Write(token);
                        response += token;
                    }
                    Console.WriteLine();
                    messages.Add(new ChatMessage(ChatRole.Assistant, response));
                }
                else
                {
                    var response = await _chatModel.GetResponseAsync(messages);
                    Console.WriteLine(response.Content);
                    Console.WriteLine($"[Model: {response.ModelId}, Tokens: {response.Usage.InputTokens}→{response.Usage.OutputTokens}]");
                    messages.Add(new ChatMessage(ChatRole.Assistant, response.Content));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during chat completion");
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine();
        }
        
        Console.WriteLine("Chat ended. Goodbye!");
    }
}