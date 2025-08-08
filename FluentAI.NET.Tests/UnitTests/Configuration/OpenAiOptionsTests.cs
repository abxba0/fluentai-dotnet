using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Unit tests for OpenAiOptions configuration class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate OpenAiOptions correctly stores and provides access to OpenAI configuration
/// EXPECTED BEHAVIOR: Configuration class initializes with defaults and allows property modification
/// METRICS: Correctness (must be 100% accurate), default values (proper initialization)
/// </summary>
public class OpenAiOptionsTests
{
    // TEST #1: Normal case - default constructor creates valid instance
    [Fact]
    public void Constructor_Default_CreatesInstanceWithDefaultValues()
    {
        // INPUT: Default constructor
        // EXPECTED: Instance created with expected default values
        
        var options = new OpenAiOptions();

        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(string.Empty, options.Model);
        Assert.False(options.IsAzureOpenAI);
        Assert.Null(options.Endpoint);
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
        var options = new OpenAiOptions();
        var expectedApiKey = "sk-test-key-12345";

        // EXPECTED: Property stores and returns the value
        options.ApiKey = expectedApiKey;
        
        Assert.Equal(expectedApiKey, options.ApiKey);
    }

    // TEST #3: Normal case - Model property set and get
    [Fact]
    public void Model_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set Model property to valid model name
        var options = new OpenAiOptions();
        var expectedModel = "gpt-4";

        // EXPECTED: Property stores and returns the value
        options.Model = expectedModel;
        
        Assert.Equal(expectedModel, options.Model);
    }

    // TEST #4: Normal case - IsAzureOpenAI property set and get
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsAzureOpenAI_SetAndGet_StoresValueCorrectly(bool isAzure)
    {
        // INPUT: Set IsAzureOpenAI property to true/false
        var options = new OpenAiOptions();

        // EXPECTED: Property stores and returns the value
        options.IsAzureOpenAI = isAzure;
        
        Assert.Equal(isAzure, options.IsAzureOpenAI);
    }

    // TEST #5: Normal case - Endpoint property set and get
    [Fact]
    public void Endpoint_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set Endpoint property to valid URL
        var options = new OpenAiOptions();
        var expectedEndpoint = "https://my-azure-openai.openai.azure.com";

        // EXPECTED: Property stores and returns the value
        options.Endpoint = expectedEndpoint;
        
        Assert.Equal(expectedEndpoint, options.Endpoint);
    }

    // TEST #6: Edge case - Endpoint property set to null
    [Fact]
    public void Endpoint_SetToNull_StoresNullCorrectly()
    {
        // INPUT: Set Endpoint property to null
        var options = new OpenAiOptions { Endpoint = "test" };

        // EXPECTED: Property stores null value
        options.Endpoint = null;
        
        Assert.Null(options.Endpoint);
    }

    // TEST #7: Normal case - RequestTimeout property set and get
    [Theory]
    [InlineData(30)] // 30 seconds
    [InlineData(300)] // 5 minutes
    [InlineData(1)] // 1 second
    public void RequestTimeout_SetAndGet_StoresValueCorrectly(int seconds)
    {
        // INPUT: Set RequestTimeout property to various timespan values
        var options = new OpenAiOptions();
        var expectedTimeout = TimeSpan.FromSeconds(seconds);

        // EXPECTED: Property stores and returns the value
        options.RequestTimeout = expectedTimeout;
        
        Assert.Equal(expectedTimeout, options.RequestTimeout);
    }

    // TEST #8: Normal case - MaxRetries property set and get
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void MaxRetries_SetAndGet_StoresValueCorrectly(int maxRetries)
    {
        // INPUT: Set MaxRetries property to various valid values
        var options = new OpenAiOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxRetries = maxRetries;
        
        Assert.Equal(maxRetries, options.MaxRetries);
    }

    // TEST #9: Normal case - MaxRequestSize property set and get
    [Theory]
    [InlineData(1000L)]
    [InlineData(50_000L)]
    [InlineData(100_000L)]
    [InlineData(1_000_000L)]
    public void MaxRequestSize_SetAndGet_StoresValueCorrectly(long maxRequestSize)
    {
        // INPUT: Set MaxRequestSize property to various valid values
        var options = new OpenAiOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxRequestSize = maxRequestSize;
        
        Assert.Equal(maxRequestSize, options.MaxRequestSize);
    }

    // TEST #10: Normal case - MaxTokens property set and get
    [Theory]
    [InlineData(null)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(4000)]
    public void MaxTokens_SetAndGet_StoresValueCorrectly(int? maxTokens)
    {
        // INPUT: Set MaxTokens property to various values including null
        var options = new OpenAiOptions();

        // EXPECTED: Property stores and returns the value
        options.MaxTokens = maxTokens;
        
        Assert.Equal(maxTokens, options.MaxTokens);
    }

    // TEST #11: Edge case - ApiKey with empty string
    [Fact]
    public void ApiKey_SetToEmptyString_StoresEmptyString()
    {
        // INPUT: Set ApiKey to empty string
        var options = new OpenAiOptions { ApiKey = "test" };

        // EXPECTED: Property stores empty string
        options.ApiKey = string.Empty;
        
        Assert.Equal(string.Empty, options.ApiKey);
    }

    // TEST #12: Edge case - Model with empty string
    [Fact]
    public void Model_SetToEmptyString_StoresEmptyString()
    {
        // INPUT: Set Model to empty string
        var options = new OpenAiOptions { Model = "gpt-4" };

        // EXPECTED: Property stores empty string
        options.Model = string.Empty;
        
        Assert.Equal(string.Empty, options.Model);
    }

    // TEST #13: Edge case - very large MaxRequestSize
    [Fact]
    public void MaxRequestSize_SetToLargeValue_StoresValueCorrectly()
    {
        // INPUT: Set MaxRequestSize to very large value
        var options = new OpenAiOptions();
        var largeSize = long.MaxValue;

        // EXPECTED: Property stores large value without overflow
        options.MaxRequestSize = largeSize;
        
        Assert.Equal(largeSize, options.MaxRequestSize);
    }

    // TEST #14: Edge case - zero RequestTimeout
    [Fact]
    public void RequestTimeout_SetToZero_StoresZeroTimeout()
    {
        // INPUT: Set RequestTimeout to zero
        var options = new OpenAiOptions();
        var zeroTimeout = TimeSpan.Zero;

        // EXPECTED: Property stores zero timeout
        options.RequestTimeout = zeroTimeout;
        
        Assert.Equal(zeroTimeout, options.RequestTimeout);
    }

    // TEST #15: Normal case - all properties set together
    [Fact]
    public void AllProperties_SetTogether_StoreAllValuesCorrectly()
    {
        // INPUT: Set all properties to specific values
        var options = new OpenAiOptions
        {
            ApiKey = "sk-test-123",
            Model = "gpt-4-turbo",
            IsAzureOpenAI = true,
            Endpoint = "https://test.openai.azure.com",
            RequestTimeout = TimeSpan.FromSeconds(60),
            MaxRetries = 3,
            MaxRequestSize = 120_000L,
            MaxTokens = 2000
        };

        // EXPECTED: All properties store their values correctly
        Assert.Equal("sk-test-123", options.ApiKey);
        Assert.Equal("gpt-4-turbo", options.Model);
        Assert.True(options.IsAzureOpenAI);
        Assert.Equal("https://test.openai.azure.com", options.Endpoint);
        Assert.Equal(TimeSpan.FromSeconds(60), options.RequestTimeout);
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(120_000L, options.MaxRequestSize);
        Assert.Equal(2000, options.MaxTokens);
    }
}