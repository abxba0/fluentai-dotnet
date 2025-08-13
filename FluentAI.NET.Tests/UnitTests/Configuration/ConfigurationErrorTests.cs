using FluentAI.Extensions;
using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Tests for configuration error diagnostics.
/// </summary>
public class ConfigurationErrorTests
{
    [Fact]
    public void AddAiSdk_MissingAiSdkSection_ThrowsWithDiagnosticInfo()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "test-key"
            })
            .Build();
        
        var services = new ServiceCollection();

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() =>
            services.AddAiSdk(configuration));

        Assert.Contains("Error: AiSdk Section Exists: False", exception.Message);
        Assert.Contains("DefaultProvider: [Not Available]", exception.Message);
        Assert.Contains("Configuration Error: The 'AiSdk' configuration section is missing", exception.Message);
    }

    [Fact]
    public void AddAiSdk_EmptyDefaultProvider_ThrowsWithDiagnosticInfo()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiSdk:DefaultProvider"] = "",
                ["OpenAI:ApiKey"] = "test-key"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddAiSdk(configuration);

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        var exception = Assert.Throws<AiSdkConfigurationException>(() =>
            serviceProvider.GetRequiredService<IChatModel>());

        Assert.Contains("Error: AiSdk Section Exists: True", exception.Message);
        Assert.Contains("DefaultProvider:", exception.Message);
        Assert.Contains("Configuration Error: A default provider is not specified", exception.Message);
    }

    [Fact]
    public void AddAiSdk_NullDefaultProvider_ThrowsWithDiagnosticInfo()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AiSdk:Failover:PrimaryProvider"] = "",
                ["AiSdk:Failover:FallbackProvider"] = "",
                ["OpenAI:ApiKey"] = "test-key"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddAiSdk(configuration);

        // Act & Assert
        var serviceProvider = services.BuildServiceProvider();
        var exception = Assert.Throws<AiSdkConfigurationException>(() =>
            serviceProvider.GetRequiredService<IChatModel>());

        Assert.Contains("Error: AiSdk Section Exists: True", exception.Message);
        Assert.Contains("DefaultProvider: [Not Specified]", exception.Message);
        Assert.Contains("Configuration Error: A default provider is not specified", exception.Message);
    }
}