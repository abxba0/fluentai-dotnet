using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;

namespace FluentAI.CLI.Commands;

/// <summary>
/// Interactive chat command for testing AI models
/// </summary>
public static class ChatCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("chat", "Start an interactive chat session with an AI model");
        
        var providerOption = new Option<string>(
            aliases: new[] { "--provider", "-p" },
            description: "AI provider to use (OpenAI, Anthropic, Google)",
            getDefaultValue: () => "OpenAI");
        
        var modelOption = new Option<string>(
            aliases: new[] { "--model", "-m" },
            description: "Model to use (e.g., gpt-4, claude-3-sonnet, gemini-pro)");
        
        var systemPromptOption = new Option<string>(
            aliases: new[] { "--system", "-s" },
            description: "System prompt for the conversation",
            getDefaultValue: () => "You are a helpful AI assistant.");

        command.AddOption(providerOption);
        command.AddOption(modelOption);
        command.AddOption(systemPromptOption);

        command.SetHandler(async (provider, model, systemPrompt) =>
        {
            await ExecuteChatAsync(services, provider, model, systemPrompt);
        }, providerOption, modelOption, systemPromptOption);

        return command;
    }

    private static async Task ExecuteChatAsync(IServiceProvider services, string provider, string? model, string systemPrompt)
    {
        try
        {
            var chatModel = services.GetRequiredService<IChatModel>();
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, systemPrompt)
            };

            AnsiConsole.MarkupLine("[bold green]FluentAI.NET Interactive Chat[/]");
            AnsiConsole.MarkupLine($"[dim]Provider: {provider}[/]");
            if (!string.IsNullOrEmpty(model))
                AnsiConsole.MarkupLine($"[dim]Model: {model}[/]");
            AnsiConsole.MarkupLine("[dim]Type 'exit' or 'quit' to end the session[/]");
            AnsiConsole.WriteLine();

            while (true)
            {
                AnsiConsole.Markup("[bold cyan]You:[/] ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                    continue;

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    AnsiConsole.MarkupLine("[yellow]Goodbye![/]");
                    break;
                }

                if (userInput.Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    conversationHistory.Clear();
                    conversationHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));
                    AnsiConsole.Clear();
                    AnsiConsole.MarkupLine("[green]Conversation history cleared.[/]");
                    continue;
                }

                conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

                await AnsiConsole.Status()
                    .StartAsync("Thinking...", async ctx =>
                    {
                        try
                        {
                            var response = await chatModel.GetResponseAsync(conversationHistory);
                            conversationHistory.Add(new ChatMessage(ChatRole.Assistant, response.Content));

                            AnsiConsole.MarkupLine("[bold magenta]Assistant:[/]");
                            AnsiConsole.MarkupLine(response.Content);
                            AnsiConsole.WriteLine();

                            if (response.Usage != null)
                            {
                                AnsiConsole.MarkupLine($"[dim]Tokens: {response.Usage.TotalTokens} " +
                                    $"(input: {response.Usage.InputTokens}, output: {response.Usage.OutputTokens})[/]");
                            }
                        }
                        catch (Exception ex)
                        {
                            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                        }
                    });

                AnsiConsole.WriteLine();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to initialize chat: {ex.Message}[/]");
            AnsiConsole.MarkupLine("[yellow]Make sure you have configured your API keys in environment variables or appsettings.json[/]");
        }
    }
}
