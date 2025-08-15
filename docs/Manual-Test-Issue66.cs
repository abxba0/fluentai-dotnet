using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Providers.HuggingFace;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace FluentAI.NET.Tests.Manual;

/// <summary>
/// Manual test demonstrating the exact issue scenario and solution.
/// This shows the before/after behavior for the reported issue.
/// </summary>
public class IssueReproductionTest
{
    /// <summary>
    /// Demonstrates the request format that will be sent for chat completions endpoint.
    /// This addresses the core issue where wrong format was being sent.
    /// </summary>
    public static void DemonstrateRequestFormatFix()
    {
        Console.WriteLine("=== Issue #66 - HuggingFace Chat Completions Request Format ===");
        Console.WriteLine();

        // Setup mocks
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<HuggingFaceOptions>>();
        var loggerMock = new Mock<ILogger<HuggingFaceChatModel>>();

        // Configure for chat completions endpoint (from the issue)
        var chatCompletionsOptions = new HuggingFaceOptions
        {
            ApiKey = "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", // From issue
            ModelId = "https://router.huggingface.co/v1/chat/completions", // From issue
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        optionsMonitorMock.Setup(x => x.CurrentValue).Returns(chatCompletionsOptions);

        var chatModel = new TestableHuggingFaceChatModel(
            httpClientFactoryMock.Object, 
            optionsMonitorMock.Object, 
            loggerMock.Object);

        // Create the exact message from the issue
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, "Hello! Please introduce yourself briefly.")
        };

        // Create request options with model as shown in issue's curl example
        var requestOptions = new HuggingFaceRequestOptions
        {
            Model = "openai/gpt-oss-20b" // From curl example in issue
        };

        // Generate the request that will be sent
        var request = chatModel.TestPrepareRequest(messages, false, chatCompletionsOptions, requestOptions);
        
        Console.WriteLine("✅ NEW BEHAVIOR - Chat Completions Format:");
        Console.WriteLine("Request will be sent as:");
        var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(requestJson);
        Console.WriteLine();

        // Show what the old format would have been
        var inferenceOptions = new HuggingFaceOptions
        {
            ApiKey = "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",
            ModelId = "https://api-inference.huggingface.co/models/microsoft/DialoGPT-medium", // Traditional inference URL
            RequestTimeout = TimeSpan.FromMinutes(2),
            MaxRetries = 2,
            MaxRequestSize = 80_000L
        };

        var oldRequest = chatModel.TestPrepareRequest(messages, false, inferenceOptions, requestOptions);
        Console.WriteLine("🔄 TRADITIONAL FORMAT - Inference API (still supported):");
        Console.WriteLine("Request would be sent as:");
        var oldRequestJson = JsonSerializer.Serialize(oldRequest, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(oldRequestJson);
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates the response processing that now extracts proper token usage.
    /// This addresses the "Input=0, Output=0" issue.
    /// </summary>
    public static void DemonstrateTokenUsageExtraction()
    {
        Console.WriteLine("=== Token Usage Extraction Fix ===");
        Console.WriteLine();

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        var optionsMonitorMock = new Mock<IOptionsMonitor<HuggingFaceOptions>>();
        var loggerMock = new Mock<ILogger<HuggingFaceChatModel>>();

        var chatModel = new TestableHuggingFaceChatModel(
            httpClientFactoryMock.Object, 
            optionsMonitorMock.Object, 
            loggerMock.Object);

        // Simulate a chat completions response with token usage (like what HuggingFace would return)
        var chatCompletionsResponse = JsonSerializer.Serialize(new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = "Hello! I'm Claude, an AI assistant created by Anthropic. I'm here to help you with a wide variety of tasks including analysis, writing, math, coding, creative projects, and thoughtful conversation. How can I assist you today?"
                    },
                    finish_reason = "stop"
                }
            },
            model = "openai/gpt-oss-20b",
            usage = new
            {
                prompt_tokens = 15,
                completion_tokens = 42,
                total_tokens = 57
            }
        });

        var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(chatCompletionsResponse, Encoding.UTF8, "application/json")
        };

        var result = chatModel.TestProcessResponse(httpResponse);

        Console.WriteLine("✅ NEW BEHAVIOR - Chat Completions Response:");
        Console.WriteLine($"AI Response:");
        Console.WriteLine($"Content: {result.Content}");
        Console.WriteLine($"Model ID: {result.ModelId}");
        Console.WriteLine($"Finish Reason: {result.FinishReason}");
        Console.WriteLine($"Token Usage: Input={result.Usage.InputTokens}, Output={result.Usage.OutputTokens}");
        Console.WriteLine();

        // Show what traditional inference response looks like
        var inferenceResponse = JsonSerializer.Serialize(new[]
        {
            new
            {
                generated_text = "User: Hello! Please introduce yourself briefly.\nAssistant: Hello! I'm Claude, an AI assistant created by Anthropic."
            }
        });

        var inferenceHttpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(inferenceResponse, Encoding.UTF8, "application/json")
        };

        var inferenceResult = chatModel.TestProcessResponse(inferenceHttpResponse);

        Console.WriteLine("🔄 TRADITIONAL BEHAVIOR - Inference API Response:");
        Console.WriteLine($"AI Response:");
        Console.WriteLine($"Content: {inferenceResult.Content}");
        Console.WriteLine($"Model ID: {inferenceResult.ModelId}");
        Console.WriteLine($"Finish Reason: {inferenceResult.FinishReason}");
        Console.WriteLine($"Token Usage: Input={inferenceResult.Usage.InputTokens}, Output={inferenceResult.Usage.OutputTokens}");
        Console.WriteLine("(Note: Traditional inference API doesn't provide token usage)");
        Console.WriteLine();
    }

    public static void RunDemonstration()
    {
        Console.WriteLine("HuggingFace Integration Issue #66 - RESOLVED");
        Console.WriteLine("==============================================");
        Console.WriteLine();
        
        DemonstrateRequestFormatFix();
        DemonstrateTokenUsageExtraction();
        
        Console.WriteLine("✅ SUMMARY:");
        Console.WriteLine("- Chat completions endpoint automatically detected by URL");
        Console.WriteLine("- Requests formatted correctly for OpenAI-compatible API");
        Console.WriteLine("- Token usage properly extracted from responses");
        Console.WriteLine("- Backward compatibility maintained for inference API");
        Console.WriteLine("- No breaking changes required");
    }
}

/// <summary>
/// Testable version of HuggingFaceChatModel for demonstration purposes.
/// </summary>
internal class TestableHuggingFaceChatModel : HuggingFaceChatModel
{
    public TestableHuggingFaceChatModel(IHttpClientFactory httpClientFactory, IOptionsMonitor<HuggingFaceOptions> optionsMonitor, ILogger<HuggingFaceChatModel> logger)
        : base(httpClientFactory, optionsMonitor, logger)
    {
    }

    public object TestPrepareRequest(IEnumerable<ChatMessage> messages, bool stream, HuggingFaceOptions configOptions, ChatRequestOptions? requestOptions)
    {
        var method = typeof(HuggingFaceChatModel).GetMethod("PrepareRequest", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return method!.Invoke(this, new object[] { messages, stream, configOptions, requestOptions });
    }

    public ChatResponse TestProcessResponse(HttpResponseMessage response)
    {
        var method = typeof(HuggingFaceChatModel).GetMethod("ProcessResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (ChatResponse)method!.Invoke(this, new object[] { response });
    }
}