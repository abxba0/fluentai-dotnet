using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Performance;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;
using System.Diagnostics;

namespace FluentAI.CLI.Commands;

/// <summary>
/// Diagnostics command for error analysis and troubleshooting
/// </summary>
public static class DiagnosticsCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("diagnostics", "Run diagnostics and troubleshooting checks");
        
        var testCommand = new Command("test", "Test connectivity to AI providers");
        testCommand.SetHandler(async () => await TestConnectivityAsync(services));

        var healthCommand = new Command("health", "Check system health");
        healthCommand.SetHandler(() => CheckHealth(services));

        command.AddCommand(testCommand);
        command.AddCommand(healthCommand);

        return command;
    }

    private static async Task TestConnectivityAsync(IServiceProvider services)
    {
        AnsiConsole.MarkupLine("[bold green]Testing AI Provider Connectivity[/]");
        AnsiConsole.WriteLine();

        var chatModel = services.GetService<IChatModel>();
        if (chatModel == null)
        {
            AnsiConsole.MarkupLine("[red]✗ Failed to get IChatModel service[/]");
            return;
        }

        var testMessages = new[]
        {
            new ChatMessage(ChatRole.User, "Hello")
        };

        await AnsiConsole.Status()
            .StartAsync("Testing...", async ctx =>
            {
                var stopwatch = Stopwatch.StartNew();
                try
                {
                    ctx.Status("Testing default provider...");
                    var response = await chatModel.GetResponseAsync(testMessages);
                    stopwatch.Stop();
                    
                    AnsiConsole.MarkupLine($"[green]✓ Default provider: Connected successfully[/]");
                    AnsiConsole.MarkupLine($"  Response time: {stopwatch.ElapsedMilliseconds} ms");
                    AnsiConsole.MarkupLine($"  Model: {response.ModelId ?? "unknown"}");
                    
                    if (response.Usage != null)
                    {
                        AnsiConsole.MarkupLine($"  Tokens used: {response.Usage.TotalTokens}");
                    }
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    AnsiConsole.MarkupLine($"[red]✗ Default provider: {ex.Message}[/]");
                    AnsiConsole.MarkupLine($"[dim]  Error type: {ex.GetType().Name}[/]");
                }
            });
    }

    private static void CheckHealth(IServiceProvider services)
    {
        AnsiConsole.MarkupLine("[bold green]System Health Check[/]");
        AnsiConsole.WriteLine();

        var table = new Table();
        table.AddColumn("Component");
        table.AddColumn("Status");
        table.AddColumn("Details");

        // Check services
        CheckService<IChatModel>(table, "IChatModel", services);
        CheckService<IPerformanceMonitor>(table, "IPerformanceMonitor", services);
        CheckService<IResponseCache>(table, "IResponseCache", services);

        // Check memory
        var memoryMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
        table.AddRow("Memory Usage", "[green]OK[/]", $"{memoryMB:F2} MB");

        // Check .NET version
        var dotnetVersion = Environment.Version.ToString();
        table.AddRow(".NET Version", "[green]OK[/]", dotnetVersion);

        AnsiConsole.Write(table);
    }

    private static void CheckService<T>(Table table, string name, IServiceProvider services) where T : class
    {
        try
        {
            var service = services.GetService<T>();
            if (service != null)
            {
                table.AddRow(name, "[green]✓ Available[/]", "Service registered");
            }
            else
            {
                table.AddRow(name, "[yellow]⚠ Not Found[/]", "Service not registered");
            }
        }
        catch (Exception ex)
        {
            table.AddRow(name, "[red]✗ Error[/]", ex.Message);
        }
    }
}
