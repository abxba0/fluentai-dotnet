using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FluentAI.NET.Tests.Integration;

/// <summary>
/// Integration tests demonstrating the API usage patterns described in the issue requirements.
/// </summary>
public class ApiUsageTests
{
    [Fact]
    public void FluentAI_QuickStartPattern_ConfiguresCorrectly()
    {
        // Arrange - Test the quick start pattern from the issue
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging services
        
        // Act - Using the exact pattern from the issue
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-api-key")
            .UseDefaultProvider("OpenAI");
            
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Verify the service can be resolved
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        Assert.NotNull(chatModel);
    }
    
    [Fact]
    public void FluentAI_MultipleProviders_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging services
        
        // Act - Configure multiple providers
        services
            .AddFluentAI()
            .AddOpenAI(config => 
            {
                config.ApiKey = "openai-key";
                config.Model = "gpt-4";
            })
            .AddAnthropic(config => 
            {
                config.ApiKey = "anthropic-key";
                config.Model = "claude-3-sonnet-20240229";
            })
            .UseDefaultProvider("OpenAI");
            
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Verify services are registered
        var defaultChatModel = serviceProvider.GetRequiredService<IChatModel>();
        Assert.NotNull(defaultChatModel);
        
        // Verify the default provider is OpenAI by checking the type name
        Assert.Contains("OpenAi", defaultChatModel.GetType().Name);
    }
    
    [Fact]
    public void FluentAI_StreamingSupport_HasCorrectInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(); // Add logging services
        services
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
            
        var serviceProvider = services.BuildServiceProvider();
        var chatModel = serviceProvider.GetRequiredService<IChatModel>();
        
        // Act & Assert - Verify streaming interface exists
        var messages = new[] { new ChatMessage(ChatRole.User, "Test") };
        var streamMethod = chatModel.GetType().GetMethod(nameof(IChatModel.StreamResponseAsync));
        
        Assert.NotNull(streamMethod);
        Assert.StartsWith("IAsyncEnumerable", streamMethod.ReturnType.Name);
    }
    
    [Fact]
    public void FluentAI_HasRequiredAbstractions()
    {
        // Verify key abstractions exist as specified in the requirements
        
        // Core interface
        var chatModelInterface = typeof(IChatModel);
        Assert.NotNull(chatModelInterface);
        
        // Models
        var chatMessageType = typeof(ChatMessage);
        var chatResponseType = typeof(ChatResponse);
        var chatRoleType = typeof(ChatRole);
        
        Assert.NotNull(chatMessageType);
        Assert.NotNull(chatResponseType);
        Assert.NotNull(chatRoleType);
        
        // Verify ChatRole has expected values
        var userRole = Enum.Parse<ChatRole>("User");
        var assistantRole = Enum.Parse<ChatRole>("Assistant");
        var systemRole = Enum.Parse<ChatRole>("System");
        
        Assert.Equal(ChatRole.User, userRole);
        Assert.Equal(ChatRole.Assistant, assistantRole);
        Assert.Equal(ChatRole.System, systemRole);
    }
    
    [Fact]
    public void FluentAI_ProviderAgnostic_SwitchingWorks()
    {
        // Test provider switching capability
        var services1 = new ServiceCollection();
        services1.AddLogging(); // Add logging services
        services1
            .AddFluentAI()
            .AddOpenAI(config => config.ApiKey = "test-key")
            .UseDefaultProvider("OpenAI");
            
        var services2 = new ServiceCollection();
        services2.AddLogging(); // Add logging services
        services2.AddHttpClient(); // Add HTTP client for Anthropic
        services2
            .AddFluentAI()
            .AddAnthropic(config => config.ApiKey = "test-key")
            .UseDefaultProvider("Anthropic");
            
        // Both should resolve IChatModel but with different implementations
        var provider1 = services1.BuildServiceProvider();
        var provider2 = services2.BuildServiceProvider();
        
        var chatModel1 = provider1.GetRequiredService<IChatModel>();
        var chatModel2 = provider2.GetRequiredService<IChatModel>();
        
        Assert.NotNull(chatModel1);
        Assert.NotNull(chatModel2);
        Assert.NotEqual(chatModel1.GetType(), chatModel2.GetType());
    }
}