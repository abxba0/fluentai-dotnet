using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Unit tests for HuggingFaceRequestOptions configuration class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate HuggingFaceRequestOptions correctly stores and provides access to request-specific parameters
/// EXPECTED BEHAVIOR: Record type correctly handles all Hugging Face-specific parameters
/// METRICS: Correctness (must be 100% accurate), parameter validation
/// </summary>
public class HuggingFaceRequestOptionsTests
{
    // TEST #1: Normal case - default constructor creates valid instance
    [Fact]
    public void Constructor_Default_CreatesInstanceWithDefaultValues()
    {
        // INPUT: Default constructor
        // EXPECTED: Instance created with null default values for optional parameters
        
        var options = new HuggingFaceRequestOptions();

        Assert.Null(options.Temperature);
        Assert.Null(options.MaxNewTokens);
        Assert.Null(options.TopP);
        Assert.Null(options.TopK);
    }

    // TEST #2: Normal case - Temperature property set and get
    [Fact]
    public void Temperature_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set Temperature property to valid value
        var expectedTemperature = 0.7f;

        // EXPECTED: Property stores and returns the value
        var options = new HuggingFaceRequestOptions { Temperature = expectedTemperature };
        
        Assert.Equal(expectedTemperature, options.Temperature);
    }

    // TEST #3: Normal case - MaxNewTokens property set and get
    [Fact]
    public void MaxNewTokens_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set MaxNewTokens property to valid value
        var expectedMaxNewTokens = 1000;

        // EXPECTED: Property stores and returns the value
        var options = new HuggingFaceRequestOptions { MaxNewTokens = expectedMaxNewTokens };
        
        Assert.Equal(expectedMaxNewTokens, options.MaxNewTokens);
    }

    // TEST #4: Normal case - TopP property set and get
    [Fact]
    public void TopP_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set TopP property to valid value
        var expectedTopP = 0.9f;

        // EXPECTED: Property stores and returns the value
        var options = new HuggingFaceRequestOptions { TopP = expectedTopP };
        
        Assert.Equal(expectedTopP, options.TopP);
    }

    // TEST #5: Normal case - TopK property set and get
    [Fact]
    public void TopK_SetAndGet_StoresValueCorrectly()
    {
        // INPUT: Set TopK property to valid value
        var expectedTopK = 50;

        // EXPECTED: Property stores and returns the value
        var options = new HuggingFaceRequestOptions { TopK = expectedTopK };
        
        Assert.Equal(expectedTopK, options.TopK);
    }

    // TEST #6: Boundary case - Temperature at minimum value
    [Fact]
    public void Temperature_SetToMinimum_StoresValueCorrectly()
    {
        // INPUT: Set Temperature to 0.0
        var minTemperature = 0.0f;
        
        // EXPECTED: Minimum temperature value stored correctly
        var options = new HuggingFaceRequestOptions { Temperature = minTemperature };
        
        Assert.Equal(minTemperature, options.Temperature);
    }

    // TEST #7: Boundary case - Temperature at maximum value
    [Fact]
    public void Temperature_SetToMaximum_StoresValueCorrectly()
    {
        // INPUT: Set Temperature to 2.0
        var maxTemperature = 2.0f;
        
        // EXPECTED: Maximum temperature value stored correctly
        var options = new HuggingFaceRequestOptions { Temperature = maxTemperature };
        
        Assert.Equal(maxTemperature, options.Temperature);
    }

    // TEST #8: Boundary case - TopP at minimum value
    [Fact]
    public void TopP_SetToMinimum_StoresValueCorrectly()
    {
        // INPUT: Set TopP to 0.0
        var minTopP = 0.0f;
        
        // EXPECTED: Minimum TopP value stored correctly
        var options = new HuggingFaceRequestOptions { TopP = minTopP };
        
        Assert.Equal(minTopP, options.TopP);
    }

    // TEST #9: Boundary case - TopP at maximum value
    [Fact]
    public void TopP_SetToMaximum_StoresValueCorrectly()
    {
        // INPUT: Set TopP to 1.0
        var maxTopP = 1.0f;
        
        // EXPECTED: Maximum TopP value stored correctly
        var options = new HuggingFaceRequestOptions { TopP = maxTopP };
        
        Assert.Equal(maxTopP, options.TopP);
    }

    // TEST #10: Normal case - All properties can be set together
    [Fact]
    public void AllProperties_SetTogether_StoreValuesCorrectly()
    {
        // INPUT: Set all properties to valid values
        var options = new HuggingFaceRequestOptions
        {
            Temperature = 0.8f,
            MaxNewTokens = 512,
            TopP = 0.95f,
            TopK = 40
        };

        // EXPECTED: All properties store their values correctly
        Assert.Equal(0.8f, options.Temperature);
        Assert.Equal(512, options.MaxNewTokens);
        Assert.Equal(0.95f, options.TopP);
        Assert.Equal(40, options.TopK);
    }

    // TEST #11: Record equality - instances with same values are equal
    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // INPUT: Two instances with identical property values
        var options1 = new HuggingFaceRequestOptions
        {
            Temperature = 0.7f,
            MaxNewTokens = 1000,
            TopP = 0.9f,
            TopK = 50
        };
        
        var options2 = new HuggingFaceRequestOptions
        {
            Temperature = 0.7f,
            MaxNewTokens = 1000,
            TopP = 0.9f,
            TopK = 50
        };

        // EXPECTED: Record equality returns true
        Assert.Equal(options1, options2);
        Assert.True(options1 == options2);
        Assert.False(options1 != options2);
    }

    // TEST #12: Record inequality - instances with different values are not equal
    [Fact]
    public void RecordEquality_DifferentValues_AreNotEqual()
    {
        // INPUT: Two instances with different property values
        var options1 = new HuggingFaceRequestOptions { Temperature = 0.7f };
        var options2 = new HuggingFaceRequestOptions { Temperature = 0.8f };

        // EXPECTED: Record equality returns false
        Assert.NotEqual(options1, options2);
        Assert.False(options1 == options2);
        Assert.True(options1 != options2);
    }
}