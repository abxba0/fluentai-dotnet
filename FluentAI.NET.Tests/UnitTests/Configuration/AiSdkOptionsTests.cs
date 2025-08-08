using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

/// <summary>
/// Unit tests for AiSdkOptions configuration class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate AiSdkOptions correctly stores and provides access to SDK configuration
/// EXPECTED BEHAVIOR: Configuration class initializes with defaults and allows property modification
/// METRICS: Correctness (must be 100% accurate), default values (proper initialization)
/// </summary>
public class AiSdkOptionsTests
{
    // TEST #1: Normal case - default constructor creates valid instance
    [Fact]
    public void Constructor_Default_CreatesInstanceWithDefaultValues()
    {
        // INPUT: Default constructor
        // EXPECTED: Instance created with null DefaultProvider
        
        var options = new AiSdkOptions();

        Assert.Null(options.DefaultProvider);
    }

    // TEST #2: Normal case - DefaultProvider property set and get
    [Theory]
    [InlineData("openai")]
    [InlineData("anthropic")]
    [InlineData("OpenAI")]
    [InlineData("Anthropic")]
    [InlineData("custom-provider")]
    public void DefaultProvider_SetAndGet_StoresValueCorrectly(string provider)
    {
        // INPUT: Set DefaultProvider property to various valid provider names
        var options = new AiSdkOptions();

        // EXPECTED: Property stores and returns the value
        options.DefaultProvider = provider;
        
        Assert.Equal(provider, options.DefaultProvider);
    }

    // TEST #3: Edge case - DefaultProvider set to null
    [Fact]
    public void DefaultProvider_SetToNull_StoresNullCorrectly()
    {
        // INPUT: Set DefaultProvider to null
        var options = new AiSdkOptions { DefaultProvider = "openai" };

        // EXPECTED: Property stores null value
        options.DefaultProvider = null;
        
        Assert.Null(options.DefaultProvider);
    }

    // TEST #4: Edge case - DefaultProvider set to empty string
    [Fact]
    public void DefaultProvider_SetToEmptyString_StoresEmptyString()
    {
        // INPUT: Set DefaultProvider to empty string
        var options = new AiSdkOptions { DefaultProvider = "openai" };

        // EXPECTED: Property stores empty string
        options.DefaultProvider = string.Empty;
        
        Assert.Equal(string.Empty, options.DefaultProvider);
    }

    // TEST #5: Edge case - DefaultProvider set to whitespace
    [Fact]
    public void DefaultProvider_SetToWhitespace_StoresWhitespace()
    {
        // INPUT: Set DefaultProvider to whitespace string
        var options = new AiSdkOptions();
        var whitespace = "   ";

        // EXPECTED: Property stores whitespace string
        options.DefaultProvider = whitespace;
        
        Assert.Equal(whitespace, options.DefaultProvider);
    }

    // TEST #6: Normal case - DefaultProvider with case sensitivity
    [Fact]
    public void DefaultProvider_IsCaseSensitive_StoresExactCase()
    {
        // INPUT: Set DefaultProvider with specific casing
        var options = new AiSdkOptions();
        var mixedCase = "OpenAI";

        // EXPECTED: Property preserves exact casing
        options.DefaultProvider = mixedCase;
        
        Assert.Equal(mixedCase, options.DefaultProvider);
        Assert.NotEqual("openai", options.DefaultProvider);
    }

    // TEST #7: Normal case - DefaultProvider with special characters
    [Theory]
    [InlineData("provider-1")]
    [InlineData("provider_2")]
    [InlineData("provider.3")]
    [InlineData("provider@4")]
    public void DefaultProvider_WithSpecialCharacters_StoresValueCorrectly(string provider)
    {
        // INPUT: Set DefaultProvider with various special characters
        var options = new AiSdkOptions();

        // EXPECTED: Property stores value with special characters
        options.DefaultProvider = provider;
        
        Assert.Equal(provider, options.DefaultProvider);
    }

    // TEST #8: Normal case - DefaultProvider with numeric values
    [Theory]
    [InlineData("123")]
    [InlineData("provider123")]
    [InlineData("123provider")]
    public void DefaultProvider_WithNumericValues_StoresValueCorrectly(string provider)
    {
        // INPUT: Set DefaultProvider with numeric characters
        var options = new AiSdkOptions();

        // EXPECTED: Property stores value correctly
        options.DefaultProvider = provider;
        
        Assert.Equal(provider, options.DefaultProvider);
    }

    // TEST #9: Normal case - DefaultProvider property modification
    [Fact]
    public void DefaultProvider_ModifyAfterInitialSet_UpdatesValueCorrectly()
    {
        // INPUT: Set DefaultProvider then change it
        var options = new AiSdkOptions { DefaultProvider = "openai" };
        var newProvider = "anthropic";

        // EXPECTED: Property updates to new value
        options.DefaultProvider = newProvider;
        
        Assert.Equal(newProvider, options.DefaultProvider);
        Assert.NotEqual("openai", options.DefaultProvider);
    }

    // TEST #10: Normal case - multiple instances independence
    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // INPUT: Create multiple AiSdkOptions instances
        var options1 = new AiSdkOptions { DefaultProvider = "openai" };
        var options2 = new AiSdkOptions { DefaultProvider = "anthropic" };

        // EXPECTED: Each instance maintains its own state
        Assert.Equal("openai", options1.DefaultProvider);
        Assert.Equal("anthropic", options2.DefaultProvider);
        
        // Modify one instance
        options1.DefaultProvider = "custom";
        
        // Other instance should be unaffected
        Assert.Equal("custom", options1.DefaultProvider);
        Assert.Equal("anthropic", options2.DefaultProvider);
    }
}