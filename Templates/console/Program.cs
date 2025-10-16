using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Add FluentAI services
        services.AddAiSdk(context.Configuration);
        
        // Add providers (configure at least one)
        services.AddOpenAiChatModel(context.Configuration);
        services.AddAnthropicChatModel(context.Configuration);
        services.AddGoogleGeminiChatModel(context.Configuration);
    });

using var host = builder.Build();

var chatModel = host.Services.GetRequiredService<IChatModel>();

Console.WriteLine("FluentAI.NET Console Application");
Console.WriteLine("=================================\n");

var messages = new[]
{
    new ChatMessage(ChatRole.System, "You are a helpful AI assistant."),
    new ChatMessage(ChatRole.User, "Hello! Can you help me understand FluentAI.NET?")
};

try
{
    Console.WriteLine("Sending request to AI model...\n");
    
    var response = await chatModel.GetResponseAsync(messages);
    
    Console.WriteLine("Response:");
    Console.WriteLine(response.Content);
    Console.WriteLine($"\nModel: {response.ModelId}");
    
    if (response.Usage != null)
    {
        Console.WriteLine($"Tokens used: {response.Usage.TotalTokens} " +
            $"(input: {response.Usage.InputTokens}, output: {response.Usage.OutputTokens})");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("\nMake sure you have:");
    Console.WriteLine("1. Configured API keys in environment variables or appsettings.json");
    Console.WriteLine("2. Set the DefaultProvider in appsettings.json");
    Console.WriteLine("3. Installed the FluentAI.NET package");
}
