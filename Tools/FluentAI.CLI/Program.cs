using FluentAI.CLI.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using FluentAI.Extensions;
using FluentAI.Extensions.Analysis;

namespace FluentAI.CLI;

/// <summary>
/// FluentAI.NET CLI - Interactive command-line tool for testing and benchmarking AI models
/// </summary>
class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("FluentAI.NET CLI - Interactive AI model testing and benchmarking");

        // Build host for DI
        var host = CreateHostBuilder(args).Build();
        
        // Add commands
        rootCommand.AddCommand(ChatCommand.Create(host.Services));
        rootCommand.AddCommand(BenchmarkCommand.Create(host.Services));
        rootCommand.AddCommand(ConfigCommand.Create(host.Services));
        rootCommand.AddCommand(StreamCommand.Create(host.Services));
        rootCommand.AddCommand(DiagnosticsCommand.Create(host.Services));

        return await rootCommand.InvokeAsync(args);
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Add FluentAI services
                services.AddAiSdk(context.Configuration);
                services.AddOpenAiChatModel(context.Configuration);
                services.AddAnthropicChatModel(context.Configuration);
                services.AddGoogleGeminiChatModel(context.Configuration);
                services.AddRuntimeAnalyzer();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });
}
