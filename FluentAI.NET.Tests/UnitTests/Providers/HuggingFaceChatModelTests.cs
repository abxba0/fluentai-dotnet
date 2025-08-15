using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Providers.HuggingFace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Providers;

/// <summary>
/// Unit tests for HuggingFaceChatModel focusing on chat completions endpoint support.
/// 
/// REQUIREMENT: Test that HuggingFace integration correctly handles both traditional inference API
/// and new OpenAI-compatible chat completions endpoints with proper token usage
/// EXPECTED BEHAVIOR: Chat model should detect endpoint type and format requests/responses accordingly
/// METRICS: Correctness (request format), token usage extraction, backward compatibility
/// </summary>
public class HuggingFaceChatModelTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<IOptionsMonitor<HuggingFaceOptions>> _optionsMonitorMock;
    private readonly Mock<ILogger<HuggingFaceChatModel>> _loggerMock;
    private readonly HuggingFaceOptions _defaultOptions;

    public HuggingFaceChatModelTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _optionsMonitorMock = new Mock<IOptionsMonitor<HuggingFaceOptions>>();
        _loggerMock = new Mock<ILogger<HuggingFaceChatModel>>();

        _defaultOptions = new HuggingFaceOptions
        {
            ApiKey = "test-api-key",
            ModelId = "microsoft/DialoGPT-medium",
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(_defaultOptions);
    }

    // TEST #1: Traditional inference API endpoint should use legacy format
    [Fact]
    public void PrepareRequest_TraditionalEndpoint_UsesInferenceApiFormat()
    {
        // INPUT: Traditional inference endpoint (does not contain "/chat/completions")
        var options = new HuggingFaceOptions
        {
            ApiKey = "test-key",
            ModelId = "https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium",
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(options);

        var chatModel = new TestableHuggingFaceChatModel(_httpClientFactoryMock.Object, _optionsMonitorMock.Object, _loggerMock.Object);
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!")
        };

        // EXPECTED: Should use inference API format with "inputs" field
        var request = chatModel.TestPrepareRequest(messages, false, options, null);
        var requestDict = request as Dictionary<string, object>;

        Assert.NotNull(requestDict);
        Assert.True(requestDict.ContainsKey("inputs"));
        Assert.False(requestDict.ContainsKey("messages"));
        Assert.Equal(false, requestDict["stream"]);
    }

    // TEST #2: Chat completions endpoint should use OpenAI format
    [Fact]
    public void PrepareRequest_ChatCompletionsEndpoint_UsesOpenAiFormat()
    {
        // INPUT: Chat completions endpoint (contains "/chat/completions")
        var options = new HuggingFaceOptions
        {
            ApiKey = "test-key",
            ModelId = "https://router.huggingface.co/v1/chat/completions",
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        _optionsMonitorMock.Setup(x => x.CurrentValue).Returns(options);

        var chatModel = new TestableHuggingFaceChatModel(_httpClientFactoryMock.Object, _optionsMonitorMock.Object, _loggerMock.Object);
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello!")
        };

        var requestOptions = new HuggingFaceRequestOptions
        {
            Model = "openai/gpt-oss-20b",
            Temperature = 0.7f
        };

        // EXPECTED: Should use OpenAI format with "messages" field and "model"
        var request = chatModel.TestPrepareRequest(messages, false, options, requestOptions);
        var requestDict = request as Dictionary<string, object>;

        Assert.NotNull(requestDict);
        Assert.True(requestDict.ContainsKey("messages"));
        Assert.False(requestDict.ContainsKey("inputs"));
        Assert.Equal("openai/gpt-oss-20b", requestDict["model"]);
        Assert.Equal(0.7f, requestDict["temperature"]);
        Assert.Equal(false, requestDict["stream"]);

        // Verify messages format
        var messagesArray = requestDict["messages"] as Array;
        Assert.NotNull(messagesArray);
        Assert.Equal(1, messagesArray.Length);
    }

    // TEST #3: ProcessResponse should handle OpenAI-compatible response with token usage
    [Fact]
    public void ProcessResponse_ChatCompletionsResponse_ExtractsTokenUsage()
    {
        // INPUT: OpenAI-compatible response with token usage
        var responseJson = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Hello! I'm Claude, an AI assistant created by Anthropic."
                    },
                    finish_reason = "stop"
                }
            },
            model = "openai/gpt-oss-20b",
            usage = new
            {
                prompt_tokens = 15,
                completion_tokens = 12,
                total_tokens = 27
            }
        });

        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        var chatModel = new TestableHuggingFaceChatModel(_httpClientFactoryMock.Object, _optionsMonitorMock.Object, _loggerMock.Object);

        // EXPECTED: Should extract content, model, and token usage correctly
        var result = chatModel.TestProcessResponse(httpResponse);

        Assert.NotNull(result);
        Assert.Equal("Hello! I'm Claude, an AI assistant created by Anthropic.", result.Content);
        Assert.Equal("openai/gpt-oss-20b", result.ModelId);
        Assert.Equal("stop", result.FinishReason);
        Assert.Equal(15, result.Usage.InputTokens);
        Assert.Equal(12, result.Usage.OutputTokens);
    }

    // TEST #4: ProcessResponse should handle traditional inference API response
    [Fact]
    public void ProcessResponse_InferenceApiResponse_ProcessesCorrectly()
    {
        // INPUT: Traditional inference API response format
        var responseJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                generated_text = "User: Hello!\nAssistant: Hello! How can I help you today?"
            }
        });

        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
        };

        var chatModel = new TestableHuggingFaceChatModel(_httpClientFactoryMock.Object, _optionsMonitorMock.Object, _loggerMock.Object);

        // EXPECTED: Should extract assistant's response and set default values
        var result = chatModel.TestProcessResponse(httpResponse);

        Assert.NotNull(result);
        Assert.Equal("Hello! How can I help you today?", result.Content);
        Assert.Equal("huggingface-inference", result.ModelId);
        Assert.Equal("stop", result.FinishReason);
        Assert.Equal(0, result.Usage.InputTokens); // Traditional API doesn't provide token usage
        Assert.Equal(0, result.Usage.OutputTokens);
    }

    // TEST #5: Chat completions with all parameters should format correctly
    [Fact]
    public void PrepareRequest_ChatCompletionsWithAllParameters_FormatsCorrectly()
    {
        // INPUT: Chat completions with all supported parameters
        var options = new HuggingFaceOptions
        {
            ApiKey = "test-key",
            ModelId = "https://router.huggingface.co/v1/chat/completions",
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        var requestOptions = new HuggingFaceRequestOptions
        {
            Model = "openai/gpt-oss-20b",
            Temperature = 0.8f,
            MaxNewTokens = 500,
            TopP = 0.9f
        };

        var chatModel = new TestableHuggingFaceChatModel(_httpClientFactoryMock.Object, _optionsMonitorMock.Object, _loggerMock.Object);
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, "You are a helpful assistant."),
            new ChatMessage(ChatRole.User, "Hello!")
        };

        // EXPECTED: Should include all parameters in OpenAI format
        var request = chatModel.TestPrepareRequest(messages, false, options, requestOptions);
        var requestDict = request as Dictionary<string, object>;

        Assert.NotNull(requestDict);
        Assert.Equal("openai/gpt-oss-20b", requestDict["model"]);
        Assert.Equal(0.8f, requestDict["temperature"]);
        Assert.Equal(500, requestDict["max_tokens"]);
        Assert.Equal(0.9f, requestDict["top_p"]);
        Assert.False(requestDict.ContainsKey("top_k")); // Should not include top_k in OpenAI format
    }
}

/// <summary>
/// Testable version of HuggingFaceChatModel that exposes private methods for testing.
/// </summary>
internal class TestableHuggingFaceChatModel : HuggingFaceChatModel
{
    public TestableHuggingFaceChatModel(IHttpClientFactory httpClientFactory, IOptionsMonitor<HuggingFaceOptions> optionsMonitor, ILogger<HuggingFaceChatModel> logger)
        : base(httpClientFactory, optionsMonitor, logger)
    {
    }

    public object TestPrepareRequest(IEnumerable<ChatMessage> messages, bool stream, HuggingFaceOptions configOptions, ChatRequestOptions? requestOptions)
    {
        // Use reflection to call the private PrepareRequest method
        var method = typeof(HuggingFaceChatModel).GetMethod("PrepareRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return method!.Invoke(this, new object[] { messages, stream, configOptions, requestOptions });
    }

    public ChatResponse TestProcessResponse(HttpResponseMessage response)
    {
        // Use reflection to call the private ProcessResponse method
        var method = typeof(HuggingFaceChatModel).GetMethod("ProcessResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ChatResponse)method!.Invoke(this, new object[] { response });
    }
}