using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Unit tests for AnthropicOptions configuration class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate AnthropicOptions correctly stores and provides access to Anthropic configuration
/// EXPECTED BEHAVIOR: Configuration class initializes with defaults and allows property modification
/// METRICS: Correctness (must be 100% accurate), default values (proper initialization)
/// </summary>
public class AnthropicOptionsTests
{
    // TEST #1: Normal case - default constructor creates valid instance
    [Fact]
    public void Constructor_Default_CreatesInstanceWithDefaultValues()
    {
        // INPUT: Default constructor
        // EXPECTED: Instance created with expected default values
        
        var options = new AnthropicOptions();

        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(string.Empty, options.Model);
        Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
        Assert.Equal(2, options.MaxRetries);
        Assert.Equal(80_000L, options.MaxRequestSize);
        Assert.Null(options.MaxTokens);
    }

    // TEST #2: Normal case - ApiKey property set and get
    [Fact]
    public void ApiKey_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set ApiKey property to valid value
        var options = new AnthropicOptions();
        var expectedApiKey = "sk-ant-test-key-12345";

        // EXPECTED: Property stores and returns the value
        options.ApiKey = expectedApiKey;
        
        Assert.Equal(expectedApiKey, options.ApiKey);
    }

    // TEST #3: Normal case - Model property set and get
    [Theory]
    [InlineData("claude-3-sonnet-20240229")]
    [InlineData("claude-3-opus-20240229")]
    [InlineData("claude-3-haiku-20240307")]
    public void Model_SetAndGet_StoresValueCorrectly(string model)
    {
        // INPUT: Set Model property to valid Anthropic model names
        var options = new AnthropicOptions();

        // EXPECTED: Property stores and returns the value
        options.Model = model;
        
        Assert.Equal(model, options.Model);
    }

    // TEST #4: Normal case - RequestTimeout property set and get
    [Theory]
    [InlineData(30)] // 30 seconds
    [InlineData(300)] // 5 minutes
    [InlineData(1)] // 1 second
    public void RequestTimeout_SetAndGet_StoresValueCorrectly(int seconds)
    {
        // INPUT: Set RequestTimeout property to various timespan values
        var options = new AnthropicOptions();
        var expectedTimeout = TimeSpan.FromSeconds(seconds);

        // EXPECTED: Property stores and returns the value
        options.RequestTimeout = expectedTimeout;
        
        Assert.Equal(expectedTimeout, options.RequestTimeout);
    }

    // TEST #5: Normal case - MaxRetries property set and get
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetries_SetAndGet_StoresValueCorrectly(int maxRetries)
    {
        // INPUT: Set MaxRetries property to various valid values
        var options = new AnthropicOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxRetries = maxRetries;
        
        Assert.Equal(maxRetries, options.MaxRetries);
    }

    // TEST #6: Normal case - MaxRequestSize property set and get
    [Theory]
    [InlineData(1000L)]
    [InlineData(50_000L)]
    [InlineData(100_000L)]
    [InlineData(200_000L)] // Anthropic supports larger contexts
    public void MaxRequestSize_SetAndGet_StoresValueCorrectly(long maxRequestSize)
    {
        // INPUT: Set MaxRequestSize property to various valid values
        var options = new AnthropicOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxRequestSize = maxRequestSize;
        
        Assert.Equal(maxRequestSize, options.MaxRequestSize);
    }

    // TEST #7: Normal case - MaxTokens property set and get
    [Theory]
    [InlineData(null)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(4000)]
    [InlineData(8192)] // Common Anthropic limit
    public void MaxTokens_SetAndGet_StoresValueCorrectly(int? maxTokens)
    {
        // INPUT: Set MaxTokens property to various values including null
        var options = new AnthropicOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxTokens = maxTokens;
        
        Assert.Equal(maxTokens, options.MaxTokens);
    }

    // TEST #8: Normal case - all properties set together
    [Fact]
    public void AllProperties_SetTogether_StoreAllValuesCorrectly()
    {
        // INPUT: Set all properties to specific values
        var options = new AnthropicOptions
        {
            ApiKey = "sk-ant-api-test-123",
            Model = "claude-3-opus-20240229",
            RequestTimeout = TimeSpan.FromSeconds(90),
            MaxRetries = 3,
            MaxRequestSize = 150_000L,
            MaxTokens = 4096
        };

        // EXPECTED: All properties store their values correctly
        Assert.Equal("sk-ant-api-test-123", options.ApiKey);
        Assert.Equal("claude-3-opus-20240229", options.Model);
        Assert.Equal(TimeSpan.FromSeconds(90), options.RequestTimeout);
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(150_000L, options.MaxRequestSize);
        Assert.Equal(4096, options.MaxTokens);
    }

    // TEST #9: Edge case - ApiKey with empty string
    [Fact]
    public void ApiKey_SetToEmptyString_StoresEmptyString()
    {
        // INPUT: Set ApiKey to empty string
        var options = new AnthropicOptions { ApiKey = "test" };

        // EXPECTED: Property stores empty string
        options.ApiKey = string.Empty;
        
        Assert.Equal(string.Empty, options.ApiKey);
    }

    // TEST #10: Edge case - Model with empty string
    [Fact]
    public void Model_SetToEmptyString_StoresEmptyString()
    {
        // INPUT: Set Model to empty string
        var options = new AnthropicOptions { Model = "claude-3-sonnet" };

        // EXPECTED: Property stores empty string
        options.Model = string.Empty;
        
        Assert.Equal(string.Empty, options.Model);
    }
}