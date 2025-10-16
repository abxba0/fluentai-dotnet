using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;

namespace FluentAI.CLI.Commands;

/// <summary>
/// Stream command for visualizing real-time token streaming
/// </summary>
public static class StreamCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("stream", "Visualize real-time token streaming from AI models");
        
        var promptOption = new Option<string>(
            aliases: new[] { "--prompt", "-p" },
            description: "Prompt to stream",
            getDefaultValue: () => "Write a short story about a robot learning to paint.");

        command.AddOption(promptOption);

        command.SetHandler(async (prompt) =>
        {
            await ExecuteStreamAsync(services, prompt);
        }, promptOption);

        return command;
    }

    private static async Task ExecuteStreamAsync(IServiceProvider services, string prompt)
    {
        try
        {
            var chatModel = services.GetRequiredService<IChatModel>();
            
            AnsiConsole.MarkupLine("[bold green]FluentAI.NET Token Streaming Visualization[/]");
            AnsiConsole.MarkupLine($"[dim]Prompt: {prompt}[/]");
            AnsiConsole.WriteLine();

            var messages = new[]
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            AnsiConsole.MarkupLine("[bold cyan]Streaming response:[/]");
            AnsiConsole.WriteLine();

            var stopwatch = Stopwatch.StartNew();
            var tokenCount = 0;
            var firstTokenTime = TimeSpan.Zero;
            var fullResponse = new System.Text.StringBuilder();

            try
            {
                await foreach (var token in chatModel.StreamResponseAsync(messages))
                {
                    if (tokenCount == 0)
                    {
                        firstTokenTime = stopwatch.Elapsed;
                    }

                    AnsiConsole.Markup($"[white]{token}[/]");
                    fullResponse.Append(token);
                    tokenCount++;
                }
                stopwatch.Stop();

                AnsiConsole.WriteLine();
                AnsiConsole.WriteLine();

                // Display streaming statistics
                var table = new Table();
                table.Border = TableBorder.Rounded;
                table.AddColumn("Metric");
                table.AddColumn("Value");

                table.AddRow("Total Time", $"{stopwatch.ElapsedMilliseconds} ms");
                table.AddRow("Time to First Token", $"{firstTokenTime.TotalMilliseconds:F2} ms");
                table.AddRow("Tokens Streamed", tokenCount.ToString());
                table.AddRow("Response Length", $"{fullResponse.Length} chars");
                
                if (tokenCount > 0 && stopwatch.ElapsedMilliseconds > 0)
                {
                    var tokensPerSecond = tokenCount / stopwatch.Elapsed.TotalSeconds;
                    table.AddRow("Tokens/Second", $"{tokensPerSecond:F2}");
                }

                AnsiConsole.Write(table);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[red]Streaming error: {ex.Message}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to initialize streaming: {ex.Message}[/]");
        }
    }
}
