using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

public class GoogleOptionsTests
{
    [Fact]
    public void GoogleOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new GoogleOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(string.Empty, options.Model);
        Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
        Assert.Equal(2, options.MaxRetries);
    }

    [Fact]
    public void GoogleOptions_SetApiKey_ShouldUpdateApiKey()
    {
        // Arrange
        var options = new GoogleOptions();
        const string apiKey = "test-google-api-key";

        // Act
        options.ApiKey = apiKey;

        // Assert
        Assert.Equal(apiKey, options.ApiKey);
    }

    [Fact]
    public void GoogleOptions_SetModel_ShouldUpdateModel()
    {
        // Arrange
        var options = new GoogleOptions();
        const string model = "gemini-pro";

        // Act
        options.Model = model;

        // Assert
        Assert.Equal(model, options.Model);
    }

    [Fact]
    public void GoogleOptions_SetRequestTimeout_ShouldUpdateTimeout()
    {
        // Arrange
        var options = new GoogleOptions();
        var timeout = TimeSpan.FromMinutes(5);

        // Act
        options.RequestTimeout = timeout;

        // Assert
        Assert.Equal(timeout, options.RequestTimeout);
    }

    [Fact]
    public void GoogleOptions_SetAllProperties_ShouldRetainValues()
    {
        // Arrange
        var options = new GoogleOptions
        {
            ApiKey = "test-key-123",
            Model = "gemini-1.5-pro",
            MaxRetries = 5,
            PermitLimit = 100,
            WindowInSeconds = 60
        };

        // Assert
        Assert.Equal("test-key-123", options.ApiKey);
        Assert.Equal("gemini-1.5-pro", options.Model);
        Assert.Equal(5, options.MaxRetries);
        Assert.Equal(100, options.PermitLimit);
        Assert.Equal(60, options.WindowInSeconds);
    }
}
