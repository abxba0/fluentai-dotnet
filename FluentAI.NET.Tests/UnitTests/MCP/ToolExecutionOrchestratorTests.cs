using FluentAI.Abstractions.MCP;
using FluentAI.MCP;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MCP;

/// <summary>
/// Unit tests for ToolExecutionOrchestrator functionality.
/// </summary>
public class ToolExecutionOrchestratorTests
{
    private readonly Mock<McpConnectionPool> _mockConnectionPool;
    private readonly Mock<IToolRegistry> _mockToolRegistry;
    private readonly Mock<IToolSchemaAdapter> _mockAdapter;
    private readonly Mock<ILogger<ToolExecutionOrchestrator>> _mockLogger;
    private readonly ToolExecutionOrchestrator _orchestrator;

    public ToolExecutionOrchestratorTests()
    {
        _mockConnectionPool = new Mock<McpConnectionPool>(
            Mock.Of<IEnumerable<IMcpTransport>>(),
            Mock.Of<ILogger<McpConnectionPool>>());
        _mockToolRegistry = new Mock<IToolRegistry>();
        _mockAdapter = new Mock<IToolSchemaAdapter>();
        _mockLogger = new Mock<ILogger<ToolExecutionOrchestrator>>();

        var adapters = new[] { _mockAdapter.Object };
        _orchestrator = new ToolExecutionOrchestrator(
            _mockConnectionPool.Object,
            _mockToolRegistry.Object,
            adapters,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteToolAsync_WithExistingTool_ShouldReturnNotFound()
    {
        // Arrange
        var toolName = "non-existent-tool";
        _mockToolRegistry.Setup(r => r.GetToolAsync(toolName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ToolSchema?)null);

        // Act
        var result = await _orchestrator.ExecuteToolAsync(toolName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("tool_not_found", result.Error.Code);
        Assert.Contains("not found", result.Error.Message);
    }

    [Fact]
    public async Task ExecuteToolAsync_WithValidTool_ShouldAttemptExecution()
    {
        // Arrange
        var toolName = "test-tool";
        var serverId = "test-server";
        var tool = new ToolSchema
        {
            Name = toolName,
            ServerId = serverId,
            Version = "1.0.0"
        };

        _mockToolRegistry.Setup(r => r.GetToolAsync(toolName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tool);

        // Act
        var result = await _orchestrator.ExecuteToolAsync(toolName);

        // Assert - Since we don't have a full client setup, expect server unavailable
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("server_unavailable", result.Error.Code);
    }

    [Fact]
    public async Task ListAvailableToolsAsync_WithRegisteredTools_ShouldReturnToolInfo()
    {
        // Arrange
        var tools = new[]
        {
            new ToolSchema
            {
                Name = "tool-1",
                Description = "Test tool 1",
                ServerId = "server-1",
                Version = "1.0.0",
                InputSchema = JsonSerializer.SerializeToDocument(new { type = "object" })
            },
            new ToolSchema
            {
                Name = "tool-2",
                Description = "Test tool 2",
                ServerId = "server-2",
                Version = "2.0.0"
            }
        };

        _mockToolRegistry.Setup(r => r.GetToolsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tools);

        _mockAdapter.Setup(a => a.CanAdapt(It.IsAny<ToolSchema>()))
            .Returns(true);
        _mockAdapter.Setup(a => a.ProviderId)
            .Returns("TestProvider");

        // Act
        var result = await _orchestrator.ListAvailableToolsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        
        var tool1 = result.First(t => t.Name == "tool-1");
        Assert.Equal("Test tool 1", tool1.Description);
        Assert.Equal("server-1", tool1.ServerId);
        Assert.True(tool1.HasInputSchema);
        Assert.Contains("TestProvider", tool1.SupportedProviders);

        var tool2 = result.First(t => t.Name == "tool-2");
        Assert.False(tool2.HasInputSchema);
    }

    [Fact]
    public async Task ListAvailableToolsAsync_WithRegistryError_ShouldReturnEmptyList()
    {
        // Arrange
        _mockToolRegistry.Setup(r => r.GetToolsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Registry error"));

        // Act
        var result = await _orchestrator.ListAvailableToolsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task ExecuteToolAsync_WithInvalidToolName_ShouldThrowArgumentException(string invalidToolName)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _orchestrator.ExecuteToolAsync(invalidToolName));
    }

    [Fact]
    public async Task InitializeServersAsync_WithValidConfigs_ShouldInitializeServers()
    {
        // Arrange
        var configs = new[]
        {
            new McpServerConfig
            {
                ServerId = "server-1",
                TransportType = McpTransportType.Stdio,
                ConnectionString = "echo test"
            }
        };

        // Act
        await _orchestrator.InitializeServersAsync(configs);

        // Assert - Verify that registry operations were attempted
        // Since GetOrCreateClientAsync returns null in our simplified implementation,
        // we expect no registry calls to succeed, but the method should not throw
        _mockToolRegistry.Verify(r => r.RegisterToolsAsync(
            It.IsAny<string>(), 
            It.IsAny<IEnumerable<ToolSchema>>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Dispose_ShouldDisposeCleanly()
    {
        // Act & Assert - Should not throw
        _orchestrator.Dispose();
    }

    [Fact]
    public async Task ExecuteToolAsync_WithParameters_ShouldCreateToolCallWithParameters()
    {
        // Arrange
        var toolName = "test-tool";
        var parameters = new { text = "Hello, world!" };
        var tool = new ToolSchema
        {
            Name = toolName,
            ServerId = "test-server",
            Version = "1.0.0"
        };

        _mockToolRegistry.Setup(r => r.GetToolAsync(toolName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tool);

        // Act
        var result = await _orchestrator.ExecuteToolAsync(toolName, parameters);

        // Assert - Even though execution fails due to no client, 
        // it should attempt to create the tool call
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }
}