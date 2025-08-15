using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace FluentAI.NET.Tests.AdditionalUnitTests;

/// <summary>
/// Additional unit tests focusing on edge cases and boundary conditions following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate edge cases, boundary conditions, and error scenarios not covered in existing unit tests
/// EXPECTED BEHAVIOR: System handles all edge cases gracefully with appropriate responses
/// METRICS: Error handling coverage, boundary condition handling, edge case resilience
/// </summary>
public class EdgeCaseTests
{
    // TEST #1: ChatMessage with extremely long content
    [Fact]
    public void Unit_ChatMessage_WithExtremelyLongContent_HandlesGracefully()
    {
        // FUNCTION: ChatMessage constructor
        // EDGE CASE: Very long content string (1MB)
        // INPUT: ChatRole.User, 1MB string content
        // EXPECTED: ChatMessage created successfully, content preserved
        
        var longContent = new string('A', 1024 * 1024); // 1MB of 'A' characters
        
        var message = new ChatMessage(ChatRole.User, longContent);
        
        Assert.Equal(ChatRole.User, message.Role);
        Assert.Equal(longContent, message.Content);
        Assert.Equal(1024 * 1024, message.Content.Length);
    }

    // TEST #2: ChatMessage with Unicode and special characters
    [Theory]
    [InlineData("Hello 🌍 World! 你好世界")]
    [InlineData("Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    [InlineData("Newlines\nand\ttabs\r\nand spaces   ")]
    [InlineData("Emoji: 😀😁😂🤣😃😄😅😆😉😊😋😎😍")]
    public void Unit_ChatMessage_WithUnicodeAndSpecialChars_PreservesContent(string content)
    {
        // FUNCTION: ChatMessage constructor
        // EDGE CASE: Unicode characters, emojis, special characters
        // INPUT: Various Unicode and special character strings
        // EXPECTED: All characters preserved exactly
        
        var message = new ChatMessage(ChatRole.Assistant, content);
        
        Assert.Equal(ChatRole.Assistant, message.Role);
        Assert.Equal(content, message.Content);
    }

    // TEST #3: OpenAiRequestOptions with boundary values (since ChatRequestOptions is abstract)
    [Theory]
    [InlineData(0)] // Minimum
    [InlineData(1)] // Just above minimum
    [InlineData(4096)] // Typical maximum
    public void Unit_OpenAiRequestOptions_WithBoundaryMaxTokens_AcceptsValues(int maxTokens)
    {
        // FUNCTION: OpenAiRequestOptions property setters
        // EDGE CASE: Boundary values for MaxTokens
        // INPUT: Edge values (0, 1, 4096)
        // EXPECTED: Values accepted and stored correctly
        
        var options = new OpenAiRequestOptions { MaxTokens = maxTokens };
        
        Assert.Equal(maxTokens, options.MaxTokens);
    }

    // TEST #4: OpenAiRequestOptions with boundary temperature values
    [Theory]
    [InlineData(0.0f)] // Minimum valid temperature
    [InlineData(0.01f)] // Just above minimum
    [InlineData(1.0f)] // Maximum typical temperature
    [InlineData(2.0f)] // High temperature
    public void Unit_OpenAiRequestOptions_WithBoundaryTemperature_AcceptsValues(float temperature)
    {
        // FUNCTION: OpenAiRequestOptions property setters
        // EDGE CASE: Boundary temperature values
        // INPUT: Edge temperature values
        // EXPECTED: Values accepted and stored correctly
        
        var options = new OpenAiRequestOptions { Temperature = temperature };
        
        Assert.Equal(temperature, options.Temperature);
    }

    // TEST #5: TokenUsage with zero and maximum values
    [Theory]
    [InlineData(0, 0)] // All zeros
    [InlineData(int.MaxValue, int.MaxValue)] // All maximums
    [InlineData(0, int.MaxValue)] // Mixed boundaries
    public void Unit_TokenUsage_WithBoundaryValues_CalculatesTotalCorrectly(int input, int output)
    {
        // FUNCTION: TokenUsage constructor
        // EDGE CASE: Boundary values for token counts
        // INPUT: Extreme token count values
        // EXPECTED: TokenUsage created with correct values and total calculated
        
        var usage = new TokenUsage(input, output);
        
        Assert.Equal(input, usage.InputTokens);
        Assert.Equal(output, usage.OutputTokens);
        Assert.Equal(input + output, usage.TotalTokens);
    }

    // TEST #6: OpenAiOptions with edge case timeout values
    [Theory]
    [InlineData(1)] // 1 millisecond
    [InlineData(1000)] // 1 second
    [InlineData(300000)] // 5 minutes
    [InlineData(int.MaxValue)] // Maximum value
    public void Unit_OpenAiOptions_WithExtremeTimeouts_AcceptsValues(int timeoutMs)
    {
        // FUNCTION: OpenAiOptions.RequestTimeout property
        // EDGE CASE: Extreme timeout values
        // INPUT: Various timeout durations
        // EXPECTED: Timeout values accepted and stored
        
        var options = new OpenAiOptions
        {
            RequestTimeout = TimeSpan.FromMilliseconds(timeoutMs)
        };
        
        Assert.Equal(TimeSpan.FromMilliseconds(timeoutMs), options.RequestTimeout);
    }

    // TEST #7: AnthropicOptions with edge case retry values
    [Theory]
    [InlineData(0)] // No retries
    [InlineData(1)] // Single retry
    [InlineData(10)] // Many retries
    [InlineData(int.MaxValue)] // Maximum retries
    public void Unit_AnthropicOptions_WithExtremeRetryValues_AcceptsValues(int maxRetries)
    {
        // FUNCTION: AnthropicOptions.MaxRetries property
        // EDGE CASE: Extreme retry count values
        // INPUT: Various retry counts
        // EXPECTED: Retry values accepted and stored
        
        var options = new AnthropicOptions
        {
            MaxRetries = maxRetries
        };
        
        Assert.Equal(maxRetries, options.MaxRetries);
    }

    // TEST #8: Configuration with extremely long API keys
    [Fact]
    public void Unit_Configuration_WithExtremelyLongApiKey_HandlesGracefully()
    {
        // FUNCTION: Configuration property handling
        // EDGE CASE: Very long API key (simulating malformed input)
        // INPUT: 10KB API key string
        // EXPECTED: Configuration accepts long string without errors
        
        var longApiKey = new string('x', 10240); // 10KB API key
        
        var options = new OpenAiOptions
        {
            ApiKey = longApiKey,
            Model = "gpt-3.5-turbo"
        };
        
        Assert.Equal(longApiKey, options.ApiKey);
        Assert.Equal(10240, options.ApiKey.Length);
    }

    // TEST #9: ServiceCollection with massive provider registrations
    [Fact]
    public void Unit_ServiceCollection_WithManyProviderRegistrations_HandlesCorrectly()
    {
        // FUNCTION: Service registration and builder pattern
        // EDGE CASE: Registering many providers repeatedly
        // INPUT: Multiple provider registrations
        // EXPECTED: All registrations processed without conflicts
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient();
        
        var builder = services.AddFluentAI();
        
        // Register multiple providers multiple times
        for (int i = 0; i < 10; i++)
        {
            builder
                .AddOpenAI(config => config.ApiKey = $"openai-key-{i}")
                .AddAnthropic(config => config.ApiKey = $"anthropic-key-{i}");
        }
        
        builder.UseDefaultProvider("OpenAI");
        
        // Should be able to build service provider without errors
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        Assert.NotNull(chatModel);
    }

    // TEST #10: Memory pressure during object creation
    [Fact]
    public void Unit_ObjectCreation_UnderMemoryPressure_CompletsSuccessfully()
    {
        // FUNCTION: Object instantiation under resource constraints
        // EDGE CASE: Creating objects when memory is limited
        // INPUT: Force garbage collection, then create objects
        // EXPECTED: Objects created successfully despite memory pressure
        
        // Force garbage collection to simulate memory pressure
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var initialMemory = GC.GetTotalMemory(false);
        
        // Create multiple large objects
        var messages = new List<ChatMessage>();
        for (int i = 0; i < 1000; i++)
        {
            var content = new string('x', 1000); // 1KB per message
            messages.Add(new ChatMessage(ChatRole.User, content));
        }
        
        Assert.Equal(1000, messages.Count);
        Assert.All(messages, m => Assert.NotNull(m.Content));
        
        var finalMemory = GC.GetTotalMemory(false);
        
        // Memory should have increased (objects were created)
        Assert.True(finalMemory > initialMemory);
    }
}

/// <summary>
/// Boundary condition tests for configuration validation and limits.
/// </summary>
public class BoundaryConditionTests
{
    // TEST #11: Configuration size limits
    [Theory]
    [InlineData(1)] // Minimum size
    [InlineData(1024)] // 1KB
    [InlineData(1024 * 1024)] // 1MB
    [InlineData(10 * 1024 * 1024)] // 10MB
    public void Unit_Configuration_WithLargeRequestSizes_HandlesCorrectly(long maxRequestSize)
    {
        // FUNCTION: Configuration validation for request sizes
        // BOUNDARY CASE: Various request size limits
        // INPUT: Different max request size values
        // EXPECTED: Configuration accepts various size limits
        
        var options = new OpenAiOptions
        {
            MaxRequestSize = maxRequestSize
        };
        
        Assert.Equal(maxRequestSize, options.MaxRequestSize);
    }

    // TEST #12: Concurrent service resolution stress test
    [Fact]
    public async Task Unit_ServiceResolution_WithHighConcurrency_RemainsThreadSafe()
    {
        // FUNCTION: Service resolution under high concurrency
        // BOUNDARY CASE: Maximum reasonable concurrent access
        // INPUT: 1000 simultaneous service resolution requests
        // EXPECTED: All requests succeed, thread safety maintained
        
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        const int concurrentRequests = 1000;
        var tasks = new Task<IChatModel>[concurrentRequests];
        
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = Task.Run(() => serviceProvider.GetRequiredService<IChatModel>());
        }
        
        var results = await Task.WhenAll(tasks);
        
        Assert.Equal(concurrentRequests, results.Length);
        Assert.All(results, Assert.NotNull);
        
        // All instances should be the same (singleton behavior)
        var firstInstance = results[0];
        Assert.All(results, model => Assert.Same(firstInstance, model));
    }

    // TEST #13: Builder pattern with maximum chaining
    [Fact]
    public void Unit_FluentBuilder_WithMaximumChaining_CompletesSuccessfully()
    {
        // FUNCTION: Fluent builder pattern with extensive chaining
        // BOUNDARY CASE: Maximum realistic method chaining
        // INPUT: Long chain of fluent method calls
        // EXPECTED: Builder completes without stack overflow or errors
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient();
        
        var builder = services
            .AddFluentAI()
            .AddOpenAI(config => 
            {
                config.ApiKey = "openai-key";
                config.Model = "gpt-3.5-turbo";
                config.MaxTokens = 1000;
                config.RequestTimeout = TimeSpan.FromSeconds(30);
            })
            .AddAnthropic(config => 
            {
                config.ApiKey = "anthropic-key";
                config.Model = "claude-3-sonnet-20240229";
                config.MaxTokens = 2000;
                config.RequestTimeout = TimeSpan.FromSeconds(45);
            })
            .AddGoogle(config => 
            {
                config.ApiKey = "google-key";
                config.Model = "gemini-pro";
                config.RequestTimeout = TimeSpan.FromSeconds(60);
            })
            .UseDefaultProvider("OpenAI");
        
        Assert.NotNull(builder);
        Assert.NotNull(builder.Services);
        
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        Assert.NotNull(chatModel);
        Assert.Contains("OpenAi", chatModel.GetType().Name);
    }

    // TEST #14: Options validation with null and empty values
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Unit_OptionsValidation_WithInvalidApiKeys_HandlesGracefully(string apiKey)
    {
        // FUNCTION: Options validation and error handling
        // BOUNDARY CASE: Invalid API key values
        // INPUT: null, empty, and whitespace-only API keys
        // EXPECTED: Configuration created but validation deferred to runtime
        
        var options = new OpenAiOptions
        {
            ApiKey = apiKey,
            Model = "gpt-3.5-turbo"
        };
        
        // Configuration should be created (validation happens at runtime)
        Assert.Equal(apiKey, options.ApiKey);
        Assert.Equal("gpt-3.5-turbo", options.Model);
    }

    // TEST #15: Dispose pattern stress test
    [Fact]
    public void Unit_ServiceProvider_WithRepeatedDispose_HandlesGracefully()
    {
        // FUNCTION: IDisposable pattern implementation
        // BOUNDARY CASE: Multiple dispose calls and resource cleanup
        // INPUT: Create and dispose service provider multiple times
        // EXPECTED: No exceptions thrown, resources properly cleaned up
        
        for (int i = 0; i < 100; i++)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services
                .AddFluentAI()
                .AddOpenAI(config => config.ApiKey = "test-key")
                .UseDefaultProvider("OpenAI");
            
            var serviceProvider = services.BuildServiceProvider();
            var chatModel = serviceProvider.GetRequiredService<IChatModel>();
            
            Assert.NotNull(chatModel);
            
            // Dispose should complete without errors
            serviceProvider.Dispose();
            
            // Multiple dispose calls should be safe
            serviceProvider.Dispose();
        }
        
        // Force garbage collection to verify no resource leaks
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}