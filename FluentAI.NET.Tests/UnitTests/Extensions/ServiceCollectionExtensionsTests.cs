using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddFluentAI_ShouldReturnFluentAiBuilder()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddFluentAI();

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void AddFluentAI_ShouldAllowChainingProviderRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var builder = services.AddFluentAI();

        // Assert
        Assert.NotNull(builder);
        Assert.IsAssignableFrom<IFluentAiBuilder>(builder);
    }

    [Fact]
    public void AddAiSdk_WithMissingAiSdkSection_ShouldThrowConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() => 
            services.AddAiSdk(configuration));
        
        Assert.Contains("'AiSdk' configuration section is missing", exception.Message);
    }

    [Fact]
    public void AddAiSdk_WithValidConfiguration_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configDict = new Dictionary<string, string>
        {
            ["AiSdk:DefaultProvider"] = "OpenAI",
            ["AiSdk:OpenAI:ApiKey"] = "test-key",
            ["AiSdk:OpenAI:DefaultModel"] = "gpt-4"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict!)
            .Build();

        services.AddLogging();

        // Act
        var result = services.AddAiSdk(configuration);

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
        
        // Verify services are registered
        var serviceProvider = services.BuildServiceProvider();
        var chatModelFactory = serviceProvider.GetService<IChatModelFactory>();
        Assert.NotNull(chatModelFactory);
    }


}
