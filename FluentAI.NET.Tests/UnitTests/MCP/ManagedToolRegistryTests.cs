using FluentAI.Abstractions.MCP;
using FluentAI.MCP;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MCP;

/// <summary>
/// Unit tests for ManagedToolRegistry functionality.
/// </summary>
public class ManagedToolRegistryTests
{
    private readonly Mock<ILogger<ManagedToolRegistry>> _mockLogger;
    private readonly ManagedToolRegistry _registry;

    public ManagedToolRegistryTests()
    {
        _mockLogger = new Mock<ILogger<ManagedToolRegistry>>();
        _registry = new ManagedToolRegistry(_mockLogger.Object);
    }

    [Fact]
    public async Task RegisterToolsAsync_WithValidTools_ShouldRegisterSuccessfully()
    {
        // Arrange
        var serverId = "test-server";
        var tools = new[]
        {
            new ToolSchema
            {
                Name = "test-tool-1",
                Description = "Test tool 1",
                ServerId = serverId,
                Version = "1.0.0"
            },
            new ToolSchema
            {
                Name = "test-tool-2",
                Description = "Test tool 2",
                ServerId = serverId,
                Version = "1.0.0"
            }
        };

        var eventTriggered = false;
        _registry.ToolsChanged += (sender, args) =>
        {
            eventTriggered = true;
            Assert.Equal(ToolRegistryChangeType.Registered, args.ChangeType);
            Assert.Equal(serverId, args.ServerId);
            Assert.Equal(2, args.AffectedTools.Count);
        };

        // Act
        await _registry.RegisterToolsAsync(serverId, tools);

        // Assert
        Assert.True(eventTriggered);
        var registeredTools = await _registry.GetToolsByServerAsync(serverId);
        Assert.Equal(2, registeredTools.Count);
        Assert.Contains(registeredTools, t => t.Name == "test-tool-1");
        Assert.Contains(registeredTools, t => t.Name == "test-tool-2");
    }

    [Fact]
    public async Task GetToolAsync_WithExistingTool_ShouldReturnTool()
    {
        // Arrange
        var serverId = "test-server";
        var tool = new ToolSchema
        {
            Name = "existing-tool",
            Description = "An existing tool",
            ServerId = serverId,
            Version = "1.0.0"
        };

        await _registry.RegisterToolsAsync(serverId, new[] { tool });

        // Act
        var result = await _registry.GetToolAsync("existing-tool");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("existing-tool", result.Name);
        Assert.Equal("An existing tool", result.Description);
        Assert.Equal(serverId, result.ServerId);
    }

    [Fact]
    public async Task GetToolAsync_WithNonExistentTool_ShouldReturnNull()
    {
        // Act
        var result = await _registry.GetToolAsync("non-existent-tool");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task InvalidateServerToolsAsync_ShouldRemoveAllServerTools()
    {
        // Arrange
        var serverId = "test-server";
        var tools = new[]
        {
            new ToolSchema { Name = "tool-1", ServerId = serverId, Version = "1.0.0" },
            new ToolSchema { Name = "tool-2", ServerId = serverId, Version = "1.0.0" }
        };

        await _registry.RegisterToolsAsync(serverId, tools);

        var eventTriggered = false;
        _registry.ToolsChanged += (sender, args) =>
        {
            if (args.ChangeType == ToolRegistryChangeType.Invalidated)
            {
                eventTriggered = true;
                Assert.Equal(serverId, args.ServerId);
                Assert.Equal(2, args.AffectedTools.Count);
            }
        };

        // Act
        await _registry.InvalidateServerToolsAsync(serverId);

        // Assert
        Assert.True(eventTriggered);
        var remainingTools = await _registry.GetToolsByServerAsync(serverId);
        Assert.Empty(remainingTools);
    }

    [Fact]
    public async Task InvalidateToolAsync_WithVersionChange_ShouldTriggerUpdate()
    {
        // Arrange
        var serverId = "test-server";
        var tool = new ToolSchema
        {
            Name = "versioned-tool",
            ServerId = serverId,
            Version = "1.0.0"
        };

        await _registry.RegisterToolsAsync(serverId, new[] { tool });

        var eventTriggered = false;
        _registry.ToolsChanged += (sender, args) =>
        {
            if (args.ChangeType == ToolRegistryChangeType.Updated)
            {
                eventTriggered = true;
                Assert.Equal(serverId, args.ServerId);
                Assert.Single(args.AffectedTools);
                Assert.Equal("versioned-tool", args.AffectedTools[0].Name);
            }
        };

        // Act
        await _registry.InvalidateToolAsync("versioned-tool", "2.0.0");

        // Assert
        Assert.True(eventTriggered);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task RegisterToolsAsync_WithInvalidServerId_ShouldThrowArgumentException(string invalidServerId)
    {
        // Arrange
        var tools = new[] { new ToolSchema { Name = "test-tool", ServerId = "test", Version = "1.0.0" } };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _registry.RegisterToolsAsync(invalidServerId, tools));
    }

    [Fact]
    public async Task RegisterToolsAsync_WithNullTools_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _registry.RegisterToolsAsync("test-server", null!));
    }

    [Fact]
    public async Task GetToolsAsync_WithMultipleServers_ShouldReturnAllTools()
    {
        // Arrange
        var server1Tools = new[]
        {
            new ToolSchema { Name = "tool-1", ServerId = "server-1", Version = "1.0.0" }
        };
        var server2Tools = new[]
        {
            new ToolSchema { Name = "tool-2", ServerId = "server-2", Version = "1.0.0" }
        };

        await _registry.RegisterToolsAsync("server-1", server1Tools);
        await _registry.RegisterToolsAsync("server-2", server2Tools);

        // Act
        var allTools = await _registry.GetToolsAsync();

        // Assert
        Assert.Equal(2, allTools.Count);
        Assert.Contains(allTools, t => t.Name == "tool-1" && t.ServerId == "server-1");
        Assert.Contains(allTools, t => t.Name == "tool-2" && t.ServerId == "server-2");
    }
}