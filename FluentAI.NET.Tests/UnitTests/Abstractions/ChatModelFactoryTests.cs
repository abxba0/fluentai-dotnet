using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Providers.Anthropic;
using FluentAI.Providers.Google;
using FluentAI.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Abstractions;

public class ChatModelFactoryTests
{
    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ChatModelFactory(null!));
    }

    [Fact]
    public void GetModel_WithNullProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetModel(null!));
    }

    [Fact]
    public void GetModel_WithEmptyProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetModel(""));
    }

    [Fact]
    public void GetModel_WithWhitespaceProviderName_ShouldThrowArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => factory.GetModel("   "));
    }

    [Fact]
    public void GetModel_WithUnsupportedProvider_ShouldThrowConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel("UnsupportedProvider"));
        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public void GetModel_WithOpenAiNotRegistered_ShouldThrowConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel("OpenAI"));
        Assert.Contains("OpenAI provider is not registered", exception.Message);
    }

    [Fact]
    public void GetModel_WithAnthropicNotRegistered_ShouldThrowConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel("Anthropic"));
        Assert.Contains("Anthropic provider is not registered", exception.Message);
    }

    [Fact]
    public void GetModel_WithGoogleNotRegistered_ShouldThrowConfigurationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel("Google"));
        Assert.Contains("Google provider is not registered", exception.Message);
    }

    [Theory]
    [InlineData("openai")]
    [InlineData("OPENAI")]
    [InlineData("OpenAI")]
    [InlineData("  OpenAI  ")]
    public void GetModel_WithOpenAiVariations_ShouldHandleCaseInsensitivity(string providerName)
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert - All should throw same exception since not registered
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel(providerName));
        Assert.Contains("OpenAI provider is not registered", exception.Message);
    }

    [Theory]
    [InlineData("anthropic")]
    [InlineData("ANTHROPIC")]
    [InlineData("Anthropic")]
    [InlineData("  Anthropic  ")]
    public void GetModel_WithAnthropicVariations_ShouldHandleCaseInsensitivity(string providerName)
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert - All should throw same exception since not registered
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel(providerName));
        Assert.Contains("Anthropic provider is not registered", exception.Message);
    }

    [Theory]
    [InlineData("google")]
    [InlineData("GOOGLE")]
    [InlineData("Google")]
    [InlineData("  Google  ")]
    public void GetModel_WithGoogleVariations_ShouldHandleCaseInsensitivity(string providerName)
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var factory = new ChatModelFactory(serviceProvider);

        // Act & Assert - All should throw same exception since not registered
        var exception = Assert.Throws<AiSdkConfigurationException>(() => factory.GetModel(providerName));
        Assert.Contains("Google provider is not registered", exception.Message);
    }
}
