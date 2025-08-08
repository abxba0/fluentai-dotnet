using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Models;

/// <summary>
/// Unit tests for ChatResponse record following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate ChatResponse record correctly stores response data from AI models
/// EXPECTED BEHAVIOR: Record creates, stores, and provides access to content, model ID, finish reason, and usage
/// METRICS: Correctness (must be 100% accurate), data integrity (all properties preserved)
/// </summary>
public class ChatResponseTests
{
    // TEST #1: Normal case - complete valid response
    [Fact]
    public void Constructor_WithValidData_CreatesCorrectChatResponse()
    {
        // INPUT: Valid content, model ID, finish reason, and token usage
        var content = "Hello! How can I help you today?";
        var modelId = "gpt-3.5-turbo";
        var finishReason = "stop";
        var usage = new TokenUsage(15, 8);

        // EXPECTED: ChatResponse with all correct properties
        var response = new ChatResponse(content, modelId, finishReason, usage);

        Assert.Equal(content, response.Content);
        Assert.Equal(modelId, response.ModelId);
        Assert.Equal(finishReason, response.FinishReason);
        Assert.Equal(usage, response.Usage);
    }

    // TEST #2: Edge case - empty content
    [Fact]
    public void Constructor_WithEmptyContent_CreatesValidChatResponse()
    {
        // INPUT: Empty content string with other valid data
        var content = "";
        var modelId = "claude-3-sonnet";
        var finishReason = "stop";
        var usage = new TokenUsage(10, 0);

        // EXPECTED: ChatResponse created successfully with empty content
        var response = new ChatResponse(content, modelId, finishReason, usage);

        Assert.Equal("", response.Content);
        Assert.Equal(modelId, response.ModelId);
        Assert.Equal(finishReason, response.FinishReason);
        Assert.Equal(usage, response.Usage);
    }

    // TEST #3: Various finish reasons
    [Theory]
    [InlineData("stop")]
    [InlineData("length")]
    [InlineData("content_filter")]
    [InlineData("tool_calls")]
    [InlineData("")]
    public void Constructor_WithVariousFinishReasons_CreatesValidChatResponse(string finishReason)
    {
        // INPUT: Different possible finish reason values
        var content = "Test response";
        var modelId = "test-model";
        var usage = new TokenUsage(10, 5);

        // EXPECTED: ChatResponse handles all finish reason types
        var response = new ChatResponse(content, modelId, finishReason, usage);

        Assert.Equal(content, response.Content);
        Assert.Equal(modelId, response.ModelId);
        Assert.Equal(finishReason, response.FinishReason);
        Assert.Equal(usage, response.Usage);
    }

    // TEST #4: Various model IDs
    [Theory]
    [InlineData("gpt-3.5-turbo")]
    [InlineData("gpt-4")]
    [InlineData("gpt-4-turbo")]
    [InlineData("claude-3-sonnet-20240229")]
    [InlineData("claude-3-opus-20240229")]
    [InlineData("custom-model-v1")]
    public void Constructor_WithVariousModelIds_CreatesValidChatResponse(string modelId)
    {
        // INPUT: Different model ID formats
        var content = "Test response";
        var finishReason = "stop";
        var usage = new TokenUsage(15, 10);

        // EXPECTED: ChatResponse handles various model ID formats
        var response = new ChatResponse(content, modelId, finishReason, usage);

        Assert.Equal(content, response.Content);
        Assert.Equal(modelId, response.ModelId);
        Assert.Equal(finishReason, response.FinishReason);
        Assert.Equal(usage, response.Usage);
    }

    // TEST #5: Record equality behavior
    [Fact]
    public void Equality_WithIdenticalData_ReturnsTrueForEquality()
    {
        // INPUT: Two ChatResponse instances with identical data
        var usage = new TokenUsage(20, 15);
        var response1 = new ChatResponse("Hello", "gpt-3.5-turbo", "stop", usage);
        var response2 = new ChatResponse("Hello", "gpt-3.5-turbo", "stop", usage);

        // EXPECTED: Records should be equal (value equality)
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
        Assert.False(response1 != response2);
    }

    // TEST #6: Record inequality behavior - different content
    [Fact]
    public void Equality_WithDifferentContent_ReturnsFalseForEquality()
    {
        // INPUT: Two ChatResponse instances with different content
        var usage = new TokenUsage(20, 15);
        var response1 = new ChatResponse("Hello", "gpt-3.5-turbo", "stop", usage);
        var response2 = new ChatResponse("Hi", "gpt-3.5-turbo", "stop", usage);

        // EXPECTED: Records should not be equal
        Assert.NotEqual(response1, response2);
        Assert.False(response1 == response2);
        Assert.True(response1 != response2);
    }

    // TEST #7: Record immutability behavior (with expression)
    [Fact]
    public void WithExpression_ModifyingContent_CreatesNewInstance()
    {
        // INPUT: ChatResponse with modification using 'with' expression
        var usage = new TokenUsage(10, 8);
        var original = new ChatResponse("Original", "gpt-3.5-turbo", "stop", usage);
        var modified = original with { Content = "Modified" };

        // EXPECTED: New instance created, original unchanged
        Assert.Equal("Original", original.Content);
        Assert.Equal("Modified", modified.Content);
        Assert.Equal("gpt-3.5-turbo", original.ModelId);
        Assert.Equal("gpt-3.5-turbo", modified.ModelId);
        Assert.NotSame(original, modified);
    }

    // TEST #8: GetHashCode consistency
    [Fact]
    public void GetHashCode_WithEqualResponses_ReturnsSameHashCode()
    {
        // INPUT: Two equal ChatResponse instances
        var usage = new TokenUsage(30, 20);
        var response1 = new ChatResponse("Same content", "gpt-3.5-turbo", "stop", usage);
        var response2 = new ChatResponse("Same content", "gpt-3.5-turbo", "stop", usage);

        // EXPECTED: Same hash code for equal objects
        Assert.Equal(response1.GetHashCode(), response2.GetHashCode());
    }
}