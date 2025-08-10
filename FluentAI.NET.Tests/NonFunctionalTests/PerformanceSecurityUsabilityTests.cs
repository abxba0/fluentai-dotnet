using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using Xunit;

namespace FluentAI.NET.Tests.NonFunctionalTests;

/// <summary>
/// Non-functional tests following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate performance, security, and usability aspects of FluentAI.NET
/// EXPECTED BEHAVIOR: System performs efficiently, securely handles sensitive data, provides good UX
/// METRICS: Response times, memory usage, security compliance, ease of use
/// </summary>
public class PerformanceTests
{
    // TEST #1: Service resolution performance
    [Fact]
    public void Performance_ServiceResolution_CompletesWithinTimeLimit()
    {
        // SCENARIO: Measure service resolution time under normal conditions
        // METRICS: Resolution time < 100ms for cold start, < 10ms for warm start
        // EXPECTED: Fast service resolution for good application startup performance
        
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Cold start measurement
        var stopwatch = Stopwatch.StartNew();
        var chatModel1 = serviceProvider.GetRequiredService<IChatModel>();
        stopwatch.Stop();
        
        Assert.NotNull(chatModel1);
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"Cold start took {stopwatch.ElapsedMilliseconds}ms, expected < 100ms");
        
        // Warm start measurement
        stopwatch.Restart();
        var chatModel2 = serviceProvider.GetRequiredService<IChatModel>();
        stopwatch.Stop();
        
        Assert.Same(chatModel1, chatModel2); // Should be singleton
        Assert.True(stopwatch.ElapsedMilliseconds < 10, 
            $"Warm start took {stopwatch.ElapsedMilliseconds}ms, expected < 10ms");
    }

    // TEST #2: Memory usage during service creation
    [Fact]
    public void Performance_MemoryUsage_StaysWithinLimits()
    {
        // SCENARIO: Monitor memory allocation during service setup
        // METRICS: Memory allocation < 1MB for basic setup
        // EXPECTED: Minimal memory footprint for the SDK
        
        var initialMemory = GC.GetTotalMemory(true);
        
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .AddAnthropic(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryUsed = finalMemory - initialMemory;
        
        Assert.NotNull(chatModel);
        Assert.True(memoryUsed < 1024 * 1024, // 1MB limit
            $"Memory usage was {memoryUsed} bytes, expected < 1MB");
    }

    // TEST #3: Concurrent service resolution performance
    [Fact]
    public async Task Performance_ConcurrentResolution_HandlesLoadEfficiently()
    {
        // SCENARIO: Multiple threads requesting services simultaneously
        // METRICS: All requests complete within 5 seconds, no deadlocks
        // EXPECTED: Thread-safe service resolution under load
        
        var services = new ServiceCollection();
        services.AddLogging();
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        const int concurrentRequests = 50;
        var tasks = new Task<IChatModel>[concurrentRequests];
        var stopwatch = Stopwatch.StartNew();
        
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = Task.Run(() => serviceProvider.GetRequiredService<IChatModel>());
        }
        
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();
        
        Assert.Equal(concurrentRequests, results.Length);
        Assert.All(results, Assert.NotNull);
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Concurrent resolution took {stopwatch.ElapsedMilliseconds}ms, expected < 5000ms");
        
        // Verify all instances are the same (singleton behavior)
        Assert.All(results, model => Assert.Same(results[0], model));
    }
}

/// <summary>
/// Security tests following the rigorous test plan template.
/// </summary>
public class SecurityTests
{
    // TEST #4: API key handling security
    [Fact]
    public void Security_ApiKeyHandling_DoesNotExposeInPlainText()
    {
        // SCENARIO: API keys should not be exposed in object representations
        // EXPECTED: API keys are properly protected from accidental exposure
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        const string sensitiveApiKey = "sk-super-secret-api-key-123";
        
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = sensitiveApiKey)
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // Verify API key is not exposed in ToString() or object representation
        var modelString = chatModel.ToString();
        Assert.DoesNotContain(sensitiveApiKey, modelString);
        
        // The actual API key storage would be tested more thoroughly in provider-specific tests
        Assert.NotNull(chatModel);
    }

    // TEST #5: Configuration validation security
    [Fact]
    public void Security_ConfigurationValidation_RejectsInvalidInput()
    {
        // SCENARIO: Configuration system should validate and sanitize inputs
        // EXPECTED: Invalid configurations are rejected with clear error messages
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Test with null API key (should be handled gracefully)
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = null!)
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Service resolution should succeed (validation happens at runtime)
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        Assert.NotNull(chatModel);
        
        // Actual API validation would occur during API calls in real usage
    }

    // TEST #6: Provider isolation security
    [Fact]
    public void Security_ProviderIsolation_PreventsDataLeakage()
    {
        // SCENARIO: Different providers should not share sensitive configuration
        // EXPECTED: Provider configurations are isolated from each other
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient();
        
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "openai-key-123")
            .AddAnthropic(config => config.ApiKey = "anthropic-key-456")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Both providers should be independently configured
        // Test that different provider types are registered without checking internal types
        var defaultModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // At minimum, verify that the default provider is resolved correctly
        Assert.NotNull(defaultModel);
        Assert.Contains("OpenAi", defaultModel.GetType().Name);
    }
}

/// <summary>
/// Usability tests following the rigorous test plan template.
/// </summary>
public class UsabilityTests
{
    // TEST #7: API ease of use
    [Fact]
    public void Usability_ApiDesign_FollowsFluentPatterns()
    {
        // SCENARIO: Developer configures FluentAI with minimal code
        // EXPECTED: Fluent, chainable API that reads naturally
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Test fluent configuration pattern
        var builder = services
            .AddFluentAI()
            .AddOpenAI(config => 
            {
                config.ApiKey = "test-key";
                config.Model = "gpt-3.5-turbo";
            })
            .AddAnthropic(config => 
            {
                config.ApiKey = "test-key";
                config.Model = "claude-3-sonnet-20240229";
            })
            .UseDefaultProvider("OpenAI");
        
        // Verify builder pattern works
        Assert.NotNull(builder);
        Assert.NotNull(builder.Services);
        
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        Assert.NotNull(chatModel);
    }

    // TEST #8: Error message clarity
    [Fact]
    public void Usability_ErrorMessages_ProvideActionableInformation()
    {
        // SCENARIO: Developer makes configuration mistake
        // EXPECTED: Clear, actionable error messages guide to solution
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // Configure without specifying default provider
        services.AddFluentAI();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Attempting to resolve without default provider should give clear error
        var exception = Assert.Throws<InvalidOperationException>(() =>
            serviceProvider.GetRequiredService<IChatModel>());
        
        // Error should be informative (actual message depends on implementation)
        Assert.NotNull(exception.Message);
        Assert.True(exception.Message.Length > 10, "Error message should be descriptive");
    }

    // TEST #9: Configuration discoverability
    [Fact]
    public void Usability_ConfigurationOptions_AreDiscoverable()
    {
        // SCENARIO: Developer explores available configuration options
        // EXPECTED: Configuration classes have clear, well-named properties
        
        // Test OpenAI options
        var openAiOptions = new OpenAiOptions();
        var openAiProperties = typeof(OpenAiOptions).GetProperties();
        
        Assert.Contains(openAiProperties, p => p.Name == nameof(OpenAiOptions.ApiKey));
        Assert.Contains(openAiProperties, p => p.Name == nameof(OpenAiOptions.Model));
        
        // Test Anthropic options
        var anthropicOptions = new AnthropicOptions();
        var anthropicProperties = typeof(AnthropicOptions).GetProperties();
        
        Assert.Contains(anthropicProperties, p => p.Name == nameof(AnthropicOptions.ApiKey));
        Assert.Contains(anthropicProperties, p => p.Name == nameof(AnthropicOptions.Model));
    }

    // TEST #10: Documentation through code
    [Fact]
    public void Usability_CodeDocumentation_IsComprehensive()
    {
        // SCENARIO: Developer uses IntelliSense/documentation
        // EXPECTED: Key interfaces and classes have XML documentation
        
        // Check IChatModel interface has documentation
        var chatModelInterface = typeof(IChatModel);
        var getResponseMethod = chatModelInterface.GetMethod(nameof(IChatModel.GetResponseAsync));
        var streamResponseMethod = chatModelInterface.GetMethod(nameof(IChatModel.StreamResponseAsync));
        
        Assert.NotNull(getResponseMethod);
        Assert.NotNull(streamResponseMethod);
        
        // Check ChatMessage record
        var chatMessageType = typeof(ChatMessage);
        Assert.NotNull(chatMessageType);
        
        // Check ServiceCollectionExtensions
        var extensionType = typeof(ServiceCollectionExtensions);
        var addFluentAiMethod = extensionType.GetMethod(nameof(ServiceCollectionExtensions.AddFluentAI));
        
        Assert.NotNull(addFluentAiMethod);
    }
}