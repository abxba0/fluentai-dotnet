using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.Integration;

/// <summary>
/// Integration test demonstrating the HuggingFace chat completions endpoint usage as described in the issue.
/// </summary>
public class HuggingFaceChatCompletionsIntegrationTests
{
    [Fact]
    public void HuggingFaceRequestOptions_WithChatCompletionsModel_ConfiguresCorrectly()
    {
        // INPUT: Configuration matching the issue scenario
        var requestOptions = new HuggingFaceRequestOptions
        {
            Model = "openai/gpt-oss-20b",
            Temperature = 0.7f,
            MaxNewTokens = 1000
        };

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello! Please introduce yourself briefly.")
        };

        // EXPECTED: Options should store all values correctly for chat completions usage
        Assert.Equal("openai/gpt-oss-20b", requestOptions.Model);
        Assert.Equal(0.7f, requestOptions.Temperature);
        Assert.Equal(1000, requestOptions.MaxNewTokens);
        
        // Verify message creation works correctly
        Assert.Single(messages);
        Assert.Equal(ChatRole.User, messages[0].Role);
        Assert.Equal("Hello! Please introduce yourself briefly.", messages[0].Content);
    }

    [Fact]
    public void HuggingFaceOptions_ChatCompletionsEndpoint_IsValidUrl()
    {
        // INPUT: Chat completions endpoint URL from the issue
        var options = new HuggingFaceOptions
        {
            ApiKey = "hf_test_token_from_issue",
            ModelId = "https://router.huggingface.co/v1/chat/completions",
            MaxRetries = 2,
            RequestTimeout = TimeSpan.FromMinutes(2)
        };

        // EXPECTED: URL should be valid and parseable
        Assert.True(Uri.TryCreate(options.ModelId, UriKind.Absolute, out var uri));
        Assert.Equal("https", uri.Scheme);
        Assert.Equal("router.huggingface.co", uri.Host);
        Assert.Contains("/v1/chat/completions", uri.PathAndQuery);
        
        // Configuration should be valid
        Assert.Equal("hf_test_token_from_issue", options.ApiKey);
        Assert.Equal(TimeSpan.FromMinutes(2), options.RequestTimeout);
        Assert.Equal(2, options.MaxRetries);
    }
}