using FluentAI.Abstractions.MCP;
using FluentAI.Extensions;
using FluentAI.MCP;
using FluentAI.MCP.Adapters;
using FluentAI.MCP.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FluentAI.NET.Tests.Integration;

/// <summary>
/// Integration tests for MCP functionality with the FluentAI service container.
/// </summary>
public class McpIntegrationTests
{
    [Fact]
    public void ServiceRegistration_ShouldRegisterMcpServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFluentAI()
            .AddMcpSupport();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IToolRegistry>());
        Assert.NotNull(serviceProvider.GetService<McpConnectionPool>());
        Assert.NotNull(serviceProvider.GetService<IMcpTransport>());
        Assert.NotNull(serviceProvider.GetService<IToolSchemaAdapter>());
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterStdioTransport()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFluentAI()
            .AddMcpSupport();

        var serviceProvider = services.BuildServiceProvider();
        var transport = serviceProvider.GetService<IMcpTransport>();

        // Assert
        Assert.NotNull(transport);
        Assert.IsType<StdioTransport>(transport);
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterOpenAiAdapter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFluentAI()
            .AddMcpSupport();

        var serviceProvider = services.BuildServiceProvider();
        var adapter = serviceProvider.GetService<IToolSchemaAdapter>();

        // Assert
        Assert.NotNull(adapter);
        Assert.IsType<OpenAiToolAdapter>(adapter);
        Assert.Equal("OpenAI", adapter.ProviderId);
    }

    [Fact]
    public void ServiceRegistration_ShouldRegisterManagedToolRegistry()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddFluentAI()
            .AddMcpSupport();

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetService<IToolRegistry>();

        // Assert
        Assert.NotNull(registry);
        Assert.IsType<ManagedToolRegistry>(registry);
    }

    [Fact]
    public void AddStdioMcpServer_ShouldConfigureServer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var builder = services.AddFluentAI()
            .AddMcpSupport()
            .AddStdioMcpServer("test-server", "echo hello", config =>
            {
                config.ConnectionTimeout = TimeSpan.FromSeconds(10);
                config.RequestTimeout = TimeSpan.FromSeconds(30);
            });

        // Assert - Should not throw and builder should be returned
        Assert.NotNull(builder);
    }

    [Fact]
    public void AddWebSocketMcpServer_ShouldConfigureServer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var builder = services.AddFluentAI()
            .AddMcpSupport()
            .AddWebSocketMcpServer("ws-server", "ws://localhost:8080/mcp");

        // Assert - Should not throw and builder should be returned
        Assert.NotNull(builder);
    }

    [Fact]
    public void AddSseMcpServer_ShouldConfigureServer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        var builder = services.AddFluentAI()
            .AddMcpSupport()
            .AddSseMcpServer("sse-server", "https://api.example.com/mcp/events");

        // Assert - Should not throw and builder should be returned
        Assert.NotNull(builder);
    }

    [Fact]
    public async Task ToolRegistry_ShouldSupportBasicOperations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluentAI()
            .AddMcpSupport();

        var serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<IToolRegistry>();

        // Act & Assert
        var tools = await registry.GetToolsAsync();
        Assert.Empty(tools);

        var tool = await registry.GetToolAsync("non-existent");
        Assert.Null(tool);
    }

    [Fact]
    public void ConnectionPool_ShouldBeCreatedWithDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFluentAI()
            .AddMcpSupport();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var pool = serviceProvider.GetService<McpConnectionPool>();

        // Assert
        Assert.NotNull(pool);
        Assert.Equal(0, pool.ActiveConnectionCount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddStdioMcpServer_WithInvalidServerId_ShouldThrowArgumentException(string invalidServerId)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = services.AddFluentAI().AddMcpSupport();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            builder.AddStdioMcpServer(invalidServerId, "echo test"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void AddStdioMcpServer_WithInvalidCommand_ShouldThrowArgumentException(string invalidCommand)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var builder = services.AddFluentAI().AddMcpSupport();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            builder.AddStdioMcpServer("test-server", invalidCommand));
    }
}