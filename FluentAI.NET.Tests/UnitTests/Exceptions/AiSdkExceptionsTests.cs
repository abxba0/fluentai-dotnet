using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Exceptions;

public class AiSdkExceptionsTests
{
    [Fact]
    public void AiSdkException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        const string message = "Test exception message";

        // Act
        var exception = new AiSdkException(message);

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void AiSdkException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        const string message = "Test exception message";
        var innerException = new InvalidOperationException("Inner exception");

        // Act
        var exception = new AiSdkException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void AiSdkConfigurationException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        const string message = "Configuration error";

        // Act
        var exception = new AiSdkConfigurationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.IsAssignableFrom<AiSdkException>(exception);
    }

    [Fact]
    public void AiSdkConfigurationException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        const string message = "Configuration error";
        var innerException = new ArgumentException("Invalid argument");

        // Act
        var exception = new AiSdkConfigurationException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void AiSdkRateLimitException_WithMessage_ShouldStoreMessage()
    {
        // Arrange
        const string message = "Rate limit exceeded";

        // Act
        var exception = new AiSdkRateLimitException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.IsAssignableFrom<AiSdkException>(exception);
    }

    [Fact]
    public void AiSdkRateLimitException_WithMessageAndInnerException_ShouldStoreBoth()
    {
        // Arrange
        const string message = "Rate limit exceeded";
        var innerException = new TimeoutException("Request timed out");

        // Act
        var exception = new AiSdkRateLimitException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
    }

    [Fact]
    public void MultiModalException_WithAllParameters_ShouldStoreThemCorrectly()
    {
        // Arrange
        const string message = "Multi-modal processing error";
        var modality = ModalityType.ImageGeneration;
        const string provider = "OpenAI";
        const string modelName = "dall-e-3";

        // Act
        var exception = new MultiModalException(modality, provider, modelName, message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(modality, exception.Modality);
        Assert.Equal(provider, exception.Provider);
        Assert.Equal(modelName, exception.ModelName);
        Assert.IsAssignableFrom<AiSdkException>(exception);
    }

    [Fact]
    public void MultiModalException_WithInnerException_ShouldStoreBoth()
    {
        // Arrange
        const string message = "Multi-modal processing error";
        var innerException = new NotSupportedException("Format not supported");
        var modality = ModalityType.AudioTranscription;
        const string provider = "OpenAI";
        const string modelName = "whisper-1";

        // Act
        var exception = new MultiModalException(modality, provider, modelName, message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(innerException, exception.InnerException);
        Assert.Equal(modality, exception.Modality);
        Assert.Equal(provider, exception.Provider);
        Assert.Equal(modelName, exception.ModelName);
    }

    [Fact]
    public void AiSdkException_IsSerializable()
    {
        // Arrange
        const string message = "Test exception";
        var exception = new AiSdkException(message);

        // Act & Assert
        Assert.NotNull(exception);
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void AiSdkConfigurationException_CanBeCaughtAsAiSdkException()
    {
        // Arrange & Act
        try
        {
            throw new AiSdkConfigurationException("Test config error");
        }
        catch (AiSdkException ex)
        {
            // Assert
            Assert.IsType<AiSdkConfigurationException>(ex);
            Assert.Contains("Test config error", ex.Message);
        }
    }

    [Fact]
    public void AiSdkRateLimitException_CanBeCaughtAsAiSdkException()
    {
        // Arrange & Act
        try
        {
            throw new AiSdkRateLimitException("Test rate limit error");
        }
        catch (AiSdkException ex)
        {
            // Assert
            Assert.IsType<AiSdkRateLimitException>(ex);
            Assert.Contains("Test rate limit error", ex.Message);
        }
    }

    [Fact]
    public void MultiModalException_CanBeCaughtAsAiSdkException()
    {
        // Arrange & Act
        try
        {
            throw new MultiModalException(ModalityType.TextGeneration, "OpenAI", "gpt-4", "Test multi-modal error");
        }
        catch (AiSdkException ex)
        {
            // Assert
            Assert.IsType<MultiModalException>(ex);
            Assert.Contains("Test multi-modal error", ex.Message);
        }
    }

    [Fact]
    public void UnsupportedModalityException_ShouldFormatMessage()
    {
        // Arrange & Act
        var exception = new UnsupportedModalityException(ModalityType.AudioGeneration, "TestProvider");

        // Assert
        Assert.Contains("TestProvider", exception.Message);
        Assert.Contains("AudioGeneration", exception.Message);
        Assert.Equal(ModalityType.AudioGeneration, exception.Modality);
        Assert.Equal("TestProvider", exception.Provider);
    }

    [Fact]
    public void ModelNotAvailableException_WithAvailableModels_ShouldIncludeThemInMessage()
    {
        // Arrange
        var availableModels = new[] { "model-1", "model-2", "model-3" };

        // Act
        var exception = new ModelNotAvailableException(
            ModalityType.TextGeneration,
            "OpenAI",
            "unavailable-model",
            availableModels);

        // Assert
        Assert.Contains("unavailable-model", exception.Message);
        Assert.Contains("model-1", exception.Message);
        Assert.Equal(3, exception.AvailableModels.Count);
    }
}
