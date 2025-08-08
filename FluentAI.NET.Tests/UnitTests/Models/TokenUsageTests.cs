using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Models;

/// <summary>
/// Unit tests for TokenUsage record following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate TokenUsage record correctly tracks input/output tokens and calculates totals
/// EXPECTED BEHAVIOR: Record stores token counts and correctly calculates total tokens
/// METRICS: Correctness (must be 100% accurate), calculation accuracy (TotalTokens = InputTokens + OutputTokens)
/// </summary>
public class TokenUsageTests
{
    // TEST #1: Normal case - typical token usage values
    [Fact]
    public void Constructor_WithValidTokenCounts_CreatesCorrectTokenUsage()
    {
        // INPUT: Typical input and output token counts
        var inputTokens = 150;
        var outputTokens = 75;

        // EXPECTED: TokenUsage with correct properties and calculated total
        var usage = new TokenUsage(inputTokens, outputTokens);

        Assert.Equal(inputTokens, usage.InputTokens);
        Assert.Equal(outputTokens, usage.OutputTokens);
        Assert.Equal(225, usage.TotalTokens);
    }

    // TEST #2: Edge case - zero input tokens
    [Fact]
    public void Constructor_WithZeroInputTokens_CreatesValidTokenUsage()
    {
        // INPUT: Zero input tokens with positive output tokens
        var inputTokens = 0;
        var outputTokens = 50;

        // EXPECTED: TokenUsage handles zero input correctly
        var usage = new TokenUsage(inputTokens, outputTokens);

        Assert.Equal(0, usage.InputTokens);
        Assert.Equal(50, usage.OutputTokens);
        Assert.Equal(50, usage.TotalTokens);
    }

    // TEST #3: Edge case - zero output tokens
    [Fact]
    public void Constructor_WithZeroOutputTokens_CreatesValidTokenUsage()
    {
        // INPUT: Positive input tokens with zero output tokens
        var inputTokens = 100;
        var outputTokens = 0;

        // EXPECTED: TokenUsage handles zero output correctly
        var usage = new TokenUsage(inputTokens, outputTokens);

        Assert.Equal(100, usage.InputTokens);
        Assert.Equal(0, usage.OutputTokens);
        Assert.Equal(100, usage.TotalTokens);
    }

    // TEST #4: Edge case - both zero tokens
    [Fact]
    public void Constructor_WithBothZeroTokens_CreatesValidTokenUsage()
    {
        // INPUT: Zero tokens for both input and output
        var inputTokens = 0;
        var outputTokens = 0;

        // EXPECTED: TokenUsage handles zero values correctly
        var usage = new TokenUsage(inputTokens, outputTokens);

        Assert.Equal(0, usage.InputTokens);
        Assert.Equal(0, usage.OutputTokens);
        Assert.Equal(0, usage.TotalTokens);
    }

    // TEST #5: TotalTokens calculation consistency
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(10, 5, 15)]
    [InlineData(100, 200, 300)]
    [InlineData(1000, 0, 1000)]
    [InlineData(0, 500, 500)]
    public void TotalTokens_WithVariousInputs_CalculatesCorrectly(int input, int output, int expectedTotal)
    {
        // INPUT: Various combinations of input and output token counts
        var usage = new TokenUsage(input, output);

        // EXPECTED: TotalTokens always equals InputTokens + OutputTokens
        Assert.Equal(expectedTotal, usage.TotalTokens);
        Assert.Equal(input + output, usage.TotalTokens);
    }

    // TEST #6: Record equality behavior
    [Fact]
    public void Equality_WithSameTokenCounts_ReturnsTrueForEquality()
    {
        // INPUT: Two TokenUsage instances with identical token counts
        var usage1 = new TokenUsage(100, 50);
        var usage2 = new TokenUsage(100, 50);

        // EXPECTED: Records should be equal (value equality)
        Assert.Equal(usage1, usage2);
        Assert.True(usage1 == usage2);
        Assert.False(usage1 != usage2);
    }

    // TEST #7: Record inequality behavior
    [Fact]
    public void Equality_WithDifferentTokenCounts_ReturnsFalseForEquality()
    {
        // INPUT: Two TokenUsage instances with different token counts
        var usage1 = new TokenUsage(100, 50);
        var usage2 = new TokenUsage(100, 75);

        // EXPECTED: Records should not be equal
        Assert.NotEqual(usage1, usage2);
        Assert.False(usage1 == usage2);
        Assert.True(usage1 != usage2);
    }

    // TEST #8: Record immutability behavior (with expression)
    [Fact]
    public void WithExpression_ModifyingInputTokens_CreatesNewInstance()
    {
        // INPUT: TokenUsage with modification using 'with' expression
        var original = new TokenUsage(100, 50);
        var modified = original with { InputTokens = 200 };

        // EXPECTED: New instance created, original unchanged, total recalculated
        Assert.Equal(100, original.InputTokens);
        Assert.Equal(200, modified.InputTokens);
        Assert.Equal(50, original.OutputTokens);
        Assert.Equal(50, modified.OutputTokens);
        Assert.Equal(150, original.TotalTokens);
        Assert.Equal(250, modified.TotalTokens);
        Assert.NotSame(original, modified);
    }

    // TEST #9: GetHashCode consistency
    [Fact]
    public void GetHashCode_WithEqualTokenUsage_ReturnsSameHashCode()
    {
        // INPUT: Two equal TokenUsage instances
        var usage1 = new TokenUsage(200, 100);
        var usage2 = new TokenUsage(200, 100);

        // EXPECTED: Same hash code for equal objects
        Assert.Equal(usage1.GetHashCode(), usage2.GetHashCode());
    }
}