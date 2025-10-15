using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

public class MultiModalOptionsTests
{
    [Fact]
    public void MultiModalOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new MultiModalOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal("Performance", options.DefaultStrategy);
        Assert.NotNull(options.Models);
    }

    [Fact]
    public void MultiModalOptions_SetDefaultStrategy_ShouldUpdateStrategy()
    {
        // Arrange
        var options = new MultiModalOptions();
        const string strategy = "Quality";

        // Act
        options.DefaultStrategy = strategy;

        // Assert
        Assert.Equal(strategy, options.DefaultStrategy);
    }

    [Fact]
    public void MultiModalOptions_SetModels_ShouldUpdateModels()
    {
        // Arrange
        var options = new MultiModalOptions();
        var models = new MultiModalModelsOptions
        {
            ImageGeneration = new ModalityModelOptions
            {
                Primary = new ModelConfiguration { Provider = "OpenAI", ModelName = "dall-e-3" }
            }
        };

        // Act
        options.Models = models;

        // Assert
        Assert.Same(models, options.Models);
        Assert.NotNull(options.Models.ImageGeneration);
    }

    [Fact]
    public void MultiModalModelsOptions_DefaultConstructor_ShouldAllowModelConfiguration()
    {
        // Arrange & Act
        var models = new MultiModalModelsOptions
        {
            TextGeneration = new ModalityModelOptions
            {
                Primary = new ModelConfiguration { Provider = "OpenAI", ModelName = "gpt-4" }
            },
            ImageAnalysis = new ModalityModelOptions
            {
                Primary = new ModelConfiguration { Provider = "OpenAI", ModelName = "gpt-4-vision" }
            }
        };

        // Assert
        Assert.NotNull(models.TextGeneration);
        Assert.NotNull(models.ImageAnalysis);
        Assert.Equal("gpt-4", models.TextGeneration.Primary.ModelName);
    }

    [Fact]
    public void ModelConfiguration_SetAllProperties_ShouldRetainValues()
    {
        // Arrange
        var config = new ModelConfiguration
        {
            Provider = "OpenAI",
            ModelName = "dall-e-3",
            MaxTokens = 4000,
            Temperature = 0.7f,
            Quality = "hd",
            Size = "1024x1024"
        };

        // Assert
        Assert.Equal("OpenAI", config.Provider);
        Assert.Equal("dall-e-3", config.ModelName);
        Assert.Equal(4000, config.MaxTokens);
        Assert.Equal(0.7f, config.Temperature);
        Assert.Equal("hd", config.Quality);
        Assert.Equal("1024x1024", config.Size);
    }
}
