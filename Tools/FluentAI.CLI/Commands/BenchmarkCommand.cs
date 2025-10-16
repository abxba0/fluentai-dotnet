using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Performance;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;

namespace FluentAI.CLI.Commands;

/// <summary>
/// Benchmark command for comparing model performance
/// </summary>
public static class BenchmarkCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("benchmark", "Compare performance across different AI models");
        
        var promptOption = new Option<string>(
            aliases: new[] { "--prompt", "-p" },
            description: "Test prompt to benchmark",
            getDefaultValue: () => "What is the capital of France?");
        
        var iterationsOption = new Option<int>(
            aliases: new[] { "--iterations", "-i" },
            description: "Number of iterations per model",
            getDefaultValue: () => 3);

        command.AddOption(promptOption);
        command.AddOption(iterationsOption);

        command.SetHandler(async (prompt, iterations) =>
        {
            await ExecuteBenchmarkAsync(services, prompt, iterations);
        }, promptOption, iterationsOption);

        return command;
    }

    private static async Task ExecuteBenchmarkAsync(IServiceProvider services, string prompt, int iterations)
    {
        try
        {
            var chatModel = services.GetRequiredService<IChatModel>();
            
            AnsiConsole.MarkupLine("[bold green]FluentAI.NET Model Benchmark[/]");
            AnsiConsole.MarkupLine($"[dim]Prompt: {prompt}[/]");
            AnsiConsole.MarkupLine($"[dim]Iterations: {iterations}[/]");
            AnsiConsole.WriteLine();

            var messages = new[]
            {
                new ChatMessage(ChatRole.User, prompt)
            };

            var results = new List<BenchmarkResult>();

            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("[green]Running benchmark...[/]");
                    task.MaxValue = iterations;

                    for (int i = 0; i < iterations; i++)
                    {
                        var stopwatch = Stopwatch.StartNew();
                        try
                        {
                            var response = await chatModel.GetResponseAsync(messages);
                            stopwatch.Stop();

                            results.Add(new BenchmarkResult
                            {
                                Success = true,
                                Duration = stopwatch.Elapsed,
                                TokenUsage = response.Usage,
                                ResponseLength = response.Content.Length,
                                ModelId = response.ModelId
                            });
                        }
                        catch (Exception ex)
                        {
                            stopwatch.Stop();
                            results.Add(new BenchmarkResult
                            {
                                Success = false,
                                Duration = stopwatch.Elapsed,
                                Error = ex.Message
                            });
                        }

                        task.Increment(1);
                    }
                });

            // Display results
            DisplayBenchmarkResults(results);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Benchmark failed: {ex.Message}[/]");
        }
    }

    private static void DisplayBenchmarkResults(List<BenchmarkResult> results)
    {
        var successfulResults = results.Where(r => r.Success).ToList();
        var failedCount = results.Count(r => !r.Success);

        if (successfulResults.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]All benchmark iterations failed[/]");
            return;
        }

        var table = new Table();
        table.AddColumn("Metric");
        table.AddColumn("Value");

        // Calculate statistics
        var avgDuration = successfulResults.Average(r => r.Duration.TotalMilliseconds);
        var minDuration = successfulResults.Min(r => r.Duration.TotalMilliseconds);
        var maxDuration = successfulResults.Max(r => r.Duration.TotalMilliseconds);
        var avgTokens = successfulResults.Average(r => r.TokenUsage?.TotalTokens ?? 0);
        var avgResponseLength = successfulResults.Average(r => r.ResponseLength);

        table.AddRow("Success Rate", $"{successfulResults.Count}/{results.Count} ({(successfulResults.Count * 100.0 / results.Count):F1}%)");
        table.AddRow("Avg Response Time", $"{avgDuration:F2} ms");
        table.AddRow("Min Response Time", $"{minDuration:F2} ms");
        table.AddRow("Max Response Time", $"{maxDuration:F2} ms");
        table.AddRow("Avg Tokens", $"{avgTokens:F0}");
        table.AddRow("Avg Response Length", $"{avgResponseLength:F0} chars");
        
        if (successfulResults.Any() && successfulResults.First().ModelId != null)
        {
            table.AddRow("Model", successfulResults.First().ModelId!);
        }

        AnsiConsole.Write(table);

        if (failedCount > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[yellow]Warning: {failedCount} iteration(s) failed[/]");
        }
    }

    private class BenchmarkResult
    {
        public bool Success { get; set; }
        public TimeSpan Duration { get; set; }
        public TokenUsage? TokenUsage { get; set; }
        public int ResponseLength { get; set; }
        public string? ModelId { get; set; }
        public string? Error { get; set; }
    }
}
