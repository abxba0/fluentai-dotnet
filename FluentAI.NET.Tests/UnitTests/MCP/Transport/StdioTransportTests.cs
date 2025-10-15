using FluentAI.Abstractions.MCP;
using FluentAI.MCP.Transport;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MCP.Transport;

public class StdioTransportTests
{
    private readonly Mock<ILogger<StdioTransport>> _mockLogger;

    public StdioTransportTests()
    {
        _mockLogger = new Mock<ILogger<StdioTransport>>();
    }

    [Fact]
    public void Constructor_WithValidLogger_ShouldCreateInstance()
    {
        // Act
        var transport = new StdioTransport(_mockLogger.Object);

        // Assert
        Assert.NotNull(transport);
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new StdioTransport(null!));
    }

    [Fact]
    public void SupportsConfig_WithStdioTransportType_ShouldReturnTrue()
    {
        // Arrange
        var transport = new StdioTransport(_mockLogger.Object);
        var config = new McpServerConfig
        {
            ServerId = "test-server",
            TransportType = McpTransportType.Stdio,
            ConnectionString = "node server.js"
        };

        // Act
        var result = transport.SupportsConfig(config);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void SupportsConfig_WithWebSocketTransportType_ShouldReturnFalse()
    {
        // Arrange
        var transport = new StdioTransport(_mockLogger.Object);
        var config = new McpServerConfig
        {
            ServerId = "test-server",
            TransportType = McpTransportType.WebSocket,
            ConnectionString = "wss://test.example.com/ws"
        };

        // Act
        var result = transport.SupportsConfig(config);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void SupportsConfig_WithSseTransportType_ShouldReturnFalse()
    {
        // Arrange
        var transport = new StdioTransport(_mockLogger.Object);
        var config = new McpServerConfig
        {
            ServerId = "test-server",
            TransportType = McpTransportType.SSE,
            ConnectionString = "https://test.example.com/sse"
        };

        // Act
        var result = transport.SupportsConfig(config);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var transport = new StdioTransport(_mockLogger.Object);

        // Act & Assert
        transport.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var transport = new StdioTransport(_mockLogger.Object);

        // Act & Assert
        transport.Dispose();
        transport.Dispose();
    }
}
