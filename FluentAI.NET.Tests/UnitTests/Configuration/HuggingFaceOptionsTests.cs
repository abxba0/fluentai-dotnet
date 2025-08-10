using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Unit tests for HuggingFaceOptions configuration class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate HuggingFaceOptions correctly stores and provides access to Hugging Face configuration
/// EXPECTED BEHAVIOR: Configuration class initializes with defaults and allows property modification
/// METRICS: Correctness (must be 100% accurate), default values (proper initialization)
/// </summary>
public class HuggingFaceOptionsTests
{
    // TEST #1: Normal case - default constructor creates valid instance
    [Fact]
    public void Constructor_Default_CreatesInstanceWithDefaultValues()
    {
        // INPUT: Default constructor
        // EXPECTED: Instance created with expected default values
        
        var options = new HuggingFaceOptions();

        Assert.Equal(string.Empty, options.ApiKey);
        Assert.Equal(string.Empty, options.ModelId);
        Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
        Assert.Equal(2, options.MaxRetries);
        Assert.Equal(80_000L, options.MaxRequestSize);
    }

    // TEST #2: Normal case - ApiKey property set and get
    [Fact]
    public void ApiKey_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set ApiKey property to valid value
        var options = new HuggingFaceOptions();
        var expectedApiKey = "hf_test_token_12345";

        // EXPECTED: Property stores and returns the value
        options.ApiKey = expectedApiKey;
        
        Assert.Equal(expectedApiKey, options.ApiKey);
    }

    // TEST #3: Normal case - ModelId property set and get
    [Fact]
    public void ModelId_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set ModelId property to valid inference endpoint URL
        var options = new HuggingFaceOptions();
        var expectedModelId = "https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium";

        // EXPECTED: Property stores and returns the value
        options.ModelId = expectedModelId;
        
        Assert.Equal(expectedModelId, options.ModelId);
    }

    // TEST #4: Normal case - RequestTimeout property set and get
    [Fact]
    public void RequestTimeout_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set RequestTimeout property to valid TimeSpan
        var options = new HuggingFaceOptions();
        var expectedTimeout = TimeSpan.FromMinutes(5);

        // EXPECTED: Property stores and returns the value
        options.RequestTimeout = expectedTimeout;
        
        Assert.Equal(expectedTimeout, options.RequestTimeout);
    }

    // TEST #5: Normal case - MaxRetries property set and get
    [Fact]
    public void MaxRetries_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set MaxRetries property to valid integer
        var options = new HuggingFaceOptions();
        var expectedMaxRetries = 5;

        // EXPECTED: Property stores and returns the value
        options.MaxRetries = expectedMaxRetries;
        
        Assert.Equal(expectedMaxRetries, options.MaxRetries);
    }

    // TEST #6: Normal case - MaxRequestSize property set and get
    [Fact]
    public void MaxRequestSize_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set MaxRequestSize property to valid long value
        var options = new HuggingFaceOptions();
        var expectedMaxRequestSize = 100_000L;

        // EXPECTED: Property stores and returns the value
        options.MaxRequestSize = expectedMaxRequestSize;
        
        Assert.Equal(expectedMaxRequestSize, options.MaxRequestSize);
    }

    // TEST #7: Boundary case - Zero MaxRetries
    [Fact]
    public void MaxRetries_SetToZero_StoresValueCorrectly()
    {
        // INPUT: Set MaxRetries to zero
        var options = new HuggingFaceOptions();
        
        // EXPECTED: Zero retries allowed and stored correctly
        options.MaxRetries = 0;
        
        Assert.Equal(0, options.MaxRetries);
    }

    // TEST #8: Boundary case - Maximum reasonable timeout
    [Fact]
    public void RequestTimeout_SetToMaxReasonableValue_StoresValueCorrectly()
    {
        // INPUT: Set RequestTimeout to 1 hour
        var options = new HuggingFaceOptions();
        var maxTimeout = TimeSpan.FromHours(1);
        
        // EXPECTED: Large timeout value stored correctly
        options.RequestTimeout = maxTimeout;
        
        Assert.Equal(maxTimeout, options.RequestTimeout);
    }

    // TEST #9: Normal case - All properties can be set together
    [Fact]
    public void AllProperties_SetTogether_StoreValuesCorrectly()
    {
        // INPUT: Set all properties to valid values
        var options = new HuggingFaceOptions
        {
            ApiKey = "hf_test_token",
            ModelId = "https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium",
            RequestTimeout = TimeSpan.FromMinutes(3),
            MaxRetries = 3,
            MaxRequestSize = 50_000L
        };

        // EXPECTED: All properties store their values correctly
        Assert.Equal("hf_test_token", options.ApiKey);
        Assert.Equal("https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium", options.ModelId);
        Assert.Equal(TimeSpan.FromMinutes(3), options.RequestTimeout);
        Assert.Equal(3, options.MaxRetries);
        Assert.Equal(50_000L, options.MaxRequestSize);
    }
}