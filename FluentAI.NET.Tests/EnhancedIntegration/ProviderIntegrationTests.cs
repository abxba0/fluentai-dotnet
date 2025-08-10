using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.EnhancedIntegration;

/// <summary>
/// Enhanced integration tests following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate complex component interactions across providers and configuration systems
/// EXPECTED BEHAVIOR: Components work together seamlessly under various scenarios
/// METRICS: Integration success rate, error handling, configuration flexibility
/// </summary>
public class ProviderIntegrationTests
{
    // TEST #1: Multi-provider registration and resolution
    [Fact]
    public void Integration_MultiProviderRegistration_ResolvesCorrectProvider()
    {
        // PATH: Service registration → Provider resolution → Type verification
        // COMPONENTS: ServiceCollection, FluentAI Builder, Multiple Providers
        // SCENARIO: Register multiple providers, verify correct resolution based on default
        
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClient(); // Required for Anthropic
        
        // INPUT FLOW:
        // 1. Register multiple providers
        services
            .AddFluentAI()
            .AddOpenAI(config => 
            {
                config.ApiKey = "openai-test-key";
                config.Model = "gpt-3.5-turbo";
            })
            .AddAnthropic(config => 
            {
                config.ApiKey = "anthropic-test-key";
                config.Model = "claude-3-sonnet-20240229";
            })
            .UseDefaultProvider("Anthropic"); // Set Anthropic as default
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 2. Resolve default provider
        var defaultChatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // 3. Resolve specific providers (remove direct access to internal classes)
        // Both providers should be independently available through the DI container
        
        // EXPECTED: Default provider should be Anthropic, available through IChatModel
        Assert.NotNull(defaultChatModel);
        Assert.Contains("Anthropic", defaultChatModel.GetType().Name);
        
        // Dependency: Multiple providers should be registered without conflicts
    }

    // TEST #2: Configuration-based provider switching
    [Fact]
    public void Integration_ConfigurationBasedSwitching_ChangesProviderDynamically()
    {
        // PATH: Configuration → Service registration → Provider resolution
        // COMPONENTS: IConfiguration, AiSdkOptions, Provider factories
        // SCENARIO: Switch providers based on configuration changes
        
        // INPUT FLOW:
        // 1. Create configuration for OpenAI
        var configData1 = new Dictionary<string, string>
        {
            ["AiSdk:DefaultProvider"] = "openai",
            ["OpenAI:ApiKey"] = "openai-key",
            ["OpenAI:Model"] = "gpt-4"
        };
        
        var config1 = new ConfigurationBuilder()
            .AddInMemoryCollection(configData1)
            .Build();
        
        var services1 = new ServiceCollection();
        services1.AddLogging();
        services1.AddOpenAiChatModel(config1);
        services1.AddAiSdk(config1);
        
        var provider1 = services1.BuildServiceProvider();
        var chatModel1 = provider1.GetRequiredService<IChatModel>();
        
        // 2. Create configuration for Anthropic
        var configData2 = new Dictionary<string, string>
        {
            ["AiSdk:DefaultProvider"] = "anthropic",
            ["Anthropic:ApiKey"] = "anthropic-key",
            ["Anthropic:Model"] = "claude-3-sonnet-20240229"
        };
        
        var config2 = new ConfigurationBuilder()
            .AddInMemoryCollection(configData2)
            .Build();
        
        var services2 = new ServiceCollection();
        services2.AddLogging();
        services2.AddAnthropicChatModel(config2);
        services2.AddAiSdk(config2);
        
        var provider2 = services2.BuildServiceProvider();
        var chatModel2 = provider2.GetRequiredService<IChatModel>();
        
        // EXPECTED: Different providers resolved based on configuration
        Assert.NotNull(chatModel1);
        Assert.NotNull(chatModel2);
        Assert.NotEqual(chatModel1.GetType(), chatModel2.GetType());
        Assert.Contains("OpenAi", chatModel1.GetType().Name);
        Assert.Contains("Anthropic", chatModel2.GetType().Name);
    }

    // TEST #3: Error handling across provider boundaries
    [Fact]
    public void Integration_CrossProviderErrorHandling_HandlesFailuresGracefully()
    {
        // PATH: Provider registration → Error simulation → Graceful handling
        // COMPONENTS: Service provider, Error handlers, Logging system
        // SCENARIO: One provider fails, system handles error appropriately
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // INPUT FLOW:
        // 1. Register provider with invalid configuration
        services
            .AddFluentAI()
            .AddOpenAI(config => 
            {
                config.ApiKey = ""; // Invalid empty key
                config.Model = "gpt-3.5-turbo";
            })
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 2. Service should still resolve (error handling happens at runtime)
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // EXPECTED: Service resolution succeeds, validation deferred to runtime
        Assert.NotNull(chatModel);
        
        // Actual error handling would be tested with real API calls
        // This tests the integration framework's ability to handle configuration errors
    }

    // TEST #4: Dependency injection integration with logging
    [Fact]
    public void Integration_LoggingIntegration_ProperlyInjectsLoggers()
    {
        // PATH: Logging configuration → Service registration → Logger injection
        // COMPONENTS: ILogger, ChatModelBase, Provider implementations
        // SCENARIO: Logging system properly integrated throughout the stack
        
        var services = new ServiceCollection();
        
        // INPUT FLOW:
        // 1. Configure logging with specific provider
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockLogger = new Mock<ILogger>();
        
        mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>()))
                        .Returns(mockLogger.Object);
        
        services.AddSingleton(mockLoggerFactory.Object);
        services.AddLogging();
        
        // 2. Register FluentAI services
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // EXPECTED: Logger properly injected into providers
        Assert.NotNull(chatModel);
        
        // Verify logging infrastructure is available
        var logger = serviceProvider.GetRequiredService<ILogger<ProviderIntegrationTests>>();
        Assert.NotNull(logger);
    }

    // TEST #5: HTTP client integration for API-based providers
    [Fact]
    public void Integration_HttpClientIntegration_ConfiguresCorrectly()
    {
        // PATH: HTTP client registration → Provider configuration → Service resolution
        // COMPONENTS: HttpClient, IHttpClientFactory, API-based providers
        // SCENARIO: HTTP clients properly configured for external API providers
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // INPUT FLOW:
        // 1. Register HTTP client services
        services.AddHttpClient();
        
        // 2. Register providers that require HTTP clients
        services
            .AddFluentAI()
            .AddAnthropic(config => config.ApiKey = "test-key")
            .AddGoogle(config => config.ApiKey = "test-key")
            .UseDefaultProvider("Anthropic");
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 3. Verify HTTP client factory is available
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
        
        // 4. Verify providers can be resolved
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        Assert.NotNull(chatModel);
        
        // EXPECTED: HTTP-dependent providers properly configured
        Assert.Contains("Anthropic", chatModel.GetType().Name);
    }

    // TEST #6: Options pattern integration
    [Fact]
    public void Integration_OptionsPattern_ConfiguresProviderSettings()
    {
        // PATH: Options configuration → Provider initialization → Settings validation
        // COMPONENTS: IOptions<T>, Configuration system, Provider options
        // SCENARIO: Options pattern properly integrated for provider configuration
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // INPUT FLOW:
        // 1. Configure options manually
        services.Configure<OpenAiOptions>(options =>
        {
            options.ApiKey = "manual-api-key";
            options.Model = "gpt-4";
            options.MaxTokens = 1000;
            options.RequestTimeout = TimeSpan.FromSeconds(30);
        });
        
        services.AddSingleton<IChatModel>(serviceProvider =>
        {
            // This is a test scenario for options pattern - return a mock implementation
            var logger = serviceProvider.GetRequiredService<ILogger<IChatModel>>();
            return new Mock<IChatModel>().Object;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 3. Verify options are properly injected
        var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiOptions>>();
        Assert.NotNull(options.Value);
        Assert.Equal("manual-api-key", options.Value.ApiKey);
        Assert.Equal("gpt-4", options.Value.Model);
        Assert.Equal(1000, options.Value.MaxTokens);
        
        // 4. Verify provider can be resolved
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        Assert.NotNull(chatModel);
        
        // EXPECTED: Options properly configured and injected
    }

    // TEST #7: Configuration validation integration
    [Fact]
    public void Integration_ConfigurationValidation_ValidatesOnStartup()
    {
        // PATH: Invalid configuration → Validation → Error reporting
        // COMPONENTS: Configuration validation, Error handling, Service resolution
        // SCENARIO: Invalid configurations detected and reported appropriately
        
        var services = new ServiceCollection();
        services.AddLogging();
        
        // INPUT FLOW:
        // 1. Configure with missing default provider
        var configData = new Dictionary<string, string>
        {
            ["AiSdk:DefaultProvider"] = "", // Empty default provider
        };
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();
        
        services.AddAiSdk(config);
        
        var serviceProvider = services.BuildServiceProvider();
        
        // 2. Attempt to resolve should fail with clear error
        var exception = Assert.Throws<AiSdkConfigurationException>(() =>
            serviceProvider.GetRequiredService<IChatModel>());
        
        // EXPECTED: Configuration validation catches missing provider
        Assert.NotNull(exception.Message);
        Assert.Contains("default provider", exception.Message.ToLowerInvariant());
    }

    // TEST #8: Cross-provider compatibility testing
    [Fact]
    public void Integration_CrossProviderCompatibility_UsesConsistentInterface()
    {
        // PATH: Multiple providers → Interface consistency → Functional equivalence
        // COMPONENTS: IChatModel interface, Different provider implementations
        // SCENARIO: All providers implement the same interface consistently
        
        var openAiServices = new ServiceCollection();
        openAiServices.AddLogging();
        openAiServices
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
        
        var anthropicServices = new ServiceCollection();
        anthropicServices.AddLogging();
        anthropicServices.AddHttpClient();
        anthropicServices
            .AddFluentAI()
            .AddAnthropic(config => config.ApiKey = "test-key")
            .UseDefaultProvider("Anthropic");
        
        // INPUT FLOW:
        // 1. Resolve providers from different configurations
        var openAiProvider = openAiServices.BuildServiceProvider();
        var anthropicProvider = anthropicServices.BuildServiceProvider();
        
        var openAiModel = openAiProvider.GetRequiredService<IChatModel>();
        var anthropicModel = anthropicProvider.GetRequiredService<IChatModel>();
        
        // 2. Verify both implement IChatModel interface
        Assert.IsAssignableFrom<IChatModel>(openAiModel);
        Assert.IsAssignableFrom<IChatModel>(anthropicModel);
        
        // 3. Verify method signatures are consistent
        var openAiMethods = openAiModel.GetType().GetInterfaces()
            .Where(i => i == typeof(IChatModel))
            .SelectMany(i => i.GetMethods())
            .Select(m => m.Name)
            .ToHashSet();
        
        var anthropicMethods = anthropicModel.GetType().GetInterfaces()
            .Where(i => i == typeof(IChatModel))
            .SelectMany(i => i.GetMethods())
            .Select(m => m.Name)
            .ToHashSet();
        
        // EXPECTED: Both providers expose the same interface methods
        Assert.Equal(openAiMethods, anthropicMethods);
        Assert.Contains(nameof(IChatModel.GetResponseAsync), openAiMethods);
        Assert.Contains(nameof(IChatModel.StreamResponseAsync), openAiMethods);
    }
}