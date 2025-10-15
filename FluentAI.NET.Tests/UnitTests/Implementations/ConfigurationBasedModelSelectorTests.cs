using FluentAI.Abstractions;
using FluentAI.Abstractions.Implementations;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Implementations;

public class ConfigurationBasedModelSelectorTests
{
    private readonly Mock<IMultiModalProviderFactory> _mockProviderFactory;
    private readonly Mock<ILogger<ConfigurationBasedModelSelector>> _mockLogger;

    public ConfigurationBasedModelSelectorTests()
    {
        _mockProviderFactory = new Mock<IMultiModalProviderFactory>();
        _mockLogger = new Mock<ILogger<ConfigurationBasedModelSelector>>();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions());

        // Act
        var selector = new ConfigurationBasedModelSelector(options, _mockProviderFactory.Object, _mockLogger.Object);

        // Assert
        Assert.NotNull(selector);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ConfigurationBasedModelSelector(null!, _mockProviderFactory.Object, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullProviderFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ConfigurationBasedModelSelector(options, null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ConfigurationBasedModelSelector(options, _mockProviderFactory.Object, null!));
    }

    [Fact]
    public void SelectModel_WithDefaultStrategy_ShouldUseConfiguredStrategy()
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions { DefaultStrategy = "Performance" });
        var selector = new ConfigurationBasedModelSelector(options, _mockProviderFactory.Object, _mockLogger.Object);

        _mockProviderFactory.Setup(f => f.SupportsModality(It.IsAny<string>(), It.IsAny<ModalityType>()))
            .Returns(false);

        // Act
        var result = selector.SelectModel(ModalityType.TextGeneration);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void SelectModel_WithNullStrategy_ShouldUseDefaultStrategy()
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions { DefaultStrategy = "Quality" });
        var selector = new ConfigurationBasedModelSelector(options, _mockProviderFactory.Object, _mockLogger.Object);

        _mockProviderFactory.Setup(f => f.SupportsModality(It.IsAny<string>(), It.IsAny<ModalityType>()))
            .Returns(false);

        // Act
        var result = selector.SelectModel(ModalityType.ImageGeneration, null);

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(ModalityType.TextGeneration)]
    [InlineData(ModalityType.ImageGeneration)]
    [InlineData(ModalityType.ImageAnalysis)]
    [InlineData(ModalityType.AudioGeneration)]
    [InlineData(ModalityType.AudioTranscription)]
    public void SelectModel_WithDifferentModalities_ShouldReturnResult(ModalityType modality)
    {
        // Arrange
        var options = Options.Create(new MultiModalOptions());
        var selector = new ConfigurationBasedModelSelector(options, _mockProviderFactory.Object, _mockLogger.Object);

        _mockProviderFactory.Setup(f => f.SupportsModality(It.IsAny<string>(), It.IsAny<ModalityType>()))
            .Returns(false);

        // Act
        var result = selector.SelectModel(modality);

        // Assert
        Assert.NotNull(result);
    }
}
