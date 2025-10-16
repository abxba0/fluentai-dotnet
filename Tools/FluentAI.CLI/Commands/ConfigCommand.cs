using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.CommandLine;

namespace FluentAI.CLI.Commands;

/// <summary>
/// Configuration command for viewing and testing configuration
/// </summary>
public static class ConfigCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("config", "View and test FluentAI configuration");
        
        var showCommand = new Command("show", "Show current configuration");
        showCommand.SetHandler(() => ShowConfiguration(services));
        
        var validateCommand = new Command("validate", "Validate configuration");
        validateCommand.SetHandler(() => ValidateConfiguration(services));

        command.AddCommand(showCommand);
        command.AddCommand(validateCommand);

        return command;
    }

    private static void ShowConfiguration(IServiceProvider services)
    {
        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();
            
            AnsiConsole.MarkupLine("[bold green]FluentAI.NET Configuration[/]");
            AnsiConsole.WriteLine();

            var tree = new Tree("Configuration");

            // AiSdk section
            var aiSdkNode = tree.AddNode("[cyan]AiSdk[/]");
            var defaultProvider = configuration["AiSdk:DefaultProvider"];
            if (!string.IsNullOrEmpty(defaultProvider))
            {
                aiSdkNode.AddNode($"[green]DefaultProvider:[/] {defaultProvider}");
            }

            // OpenAI section
            var openAiNode = tree.AddNode("[cyan]OpenAI[/]");
            AddProviderConfig(openAiNode, configuration, "OpenAI");

            // Anthropic section
            var anthropicNode = tree.AddNode("[cyan]Anthropic[/]");
            AddProviderConfig(anthropicNode, configuration, "Anthropic");

            // Google section
            var googleNode = tree.AddNode("[cyan]Google[/]");
            AddProviderConfig(googleNode, configuration, "Google");

            AnsiConsole.Write(tree);

            // Check for API keys
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]API Key Status:[/]");
            CheckApiKey("OPENAI_API_KEY");
            CheckApiKey("ANTHROPIC_API_KEY");
            CheckApiKey("GOOGLE_API_KEY");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to show configuration: {ex.Message}[/]");
        }
    }

    private static void AddProviderConfig(TreeNode node, IConfiguration config, string provider)
    {
        var model = config[$"{provider}:Model"];
        var maxTokens = config[$"{provider}:MaxTokens"];
        var timeout = config[$"{provider}:RequestTimeout"];

        if (!string.IsNullOrEmpty(model))
            node.AddNode($"Model: {model}");
        if (!string.IsNullOrEmpty(maxTokens))
            node.AddNode($"MaxTokens: {maxTokens}");
        if (!string.IsNullOrEmpty(timeout))
            node.AddNode($"Timeout: {timeout}");
    }

    private static void CheckApiKey(string keyName)
    {
        var value = Environment.GetEnvironmentVariable(keyName);
        if (!string.IsNullOrEmpty(value))
        {
            var masked = value.Length > 8 
                ? value.Substring(0, 4) + "..." + value.Substring(value.Length - 4)
                : "***";
            AnsiConsole.MarkupLine($"  [green]✓[/] {keyName}: {masked}");
        }
        else
        {
            AnsiConsole.MarkupLine($"  [red]✗[/] {keyName}: [dim]not set[/]");
        }
    }

    private static void ValidateConfiguration(IServiceProvider services)
    {
        AnsiConsole.MarkupLine("[bold green]Validating Configuration...[/]");
        AnsiConsole.WriteLine();

        var issues = new List<string>();
        var configuration = services.GetRequiredService<IConfiguration>();

        // Check default provider
        var defaultProvider = configuration["AiSdk:DefaultProvider"];
        if (string.IsNullOrEmpty(defaultProvider))
        {
            issues.Add("AiSdk:DefaultProvider is not set");
        }
        else if (!new[] { "OpenAI", "Anthropic", "Google" }.Contains(defaultProvider))
        {
            issues.Add($"Invalid default provider: {defaultProvider}");
        }

        // Check API keys
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
        {
            issues.Add("OPENAI_API_KEY environment variable is not set");
        }
        
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY")))
        {
            issues.Add("ANTHROPIC_API_KEY environment variable is not set");
        }
        
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GOOGLE_API_KEY")))
        {
            issues.Add("GOOGLE_API_KEY environment variable is not set");
        }

        if (issues.Count == 0)
        {
            AnsiConsole.MarkupLine("[green]✓ Configuration is valid[/]");
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]⚠ Configuration issues found:[/]");
            foreach (var issue in issues)
            {
                AnsiConsole.MarkupLine($"  [red]✗[/] {issue}");
            }
        }
    }
}
