using FluentAI.Abstractions.MCP;
using FluentAI.MCP.Adapters;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MCP;

/// <summary>
/// Unit tests for OpenAiToolAdapter functionality.
/// </summary>
public class OpenAiToolAdapterTests
{
    private readonly Mock<ILogger<OpenAiToolAdapter>> _mockLogger;
    private readonly OpenAiToolAdapter _adapter;

    public OpenAiToolAdapterTests()
    {
        _mockLogger = new Mock<ILogger<OpenAiToolAdapter>>();
        _adapter = new OpenAiToolAdapter(_mockLogger.Object);
    }

    [Fact]
    public void ProviderId_ShouldReturnOpenAI()
    {
        // Act & Assert
        Assert.Equal("OpenAI", _adapter.ProviderId);
    }

    [Fact]
    public void CanAdapt_WithValidSchema_ShouldReturnTrue()
    {
        // Arrange
        var schema = new ToolSchema
        {
            Name = "test-tool",
            Description = "Test tool",
            ServerId = "test-server",
            Version = "1.0.0"
        };

        // Act
        var result = _adapter.CanAdapt(schema);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanAdapt_WithEmptyName_ShouldReturnFalse()
    {
        // Arrange
        var schema = new ToolSchema
        {
            Name = "",
            Description = "Test tool",
            ServerId = "test-server",
            Version = "1.0.0"
        };

        // Act
        var result = _adapter.CanAdapt(schema);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AdaptSchema_WithValidMcpSchema_ShouldReturnOpenAiSchema()
    {
        // Arrange
        var inputSchema = JsonSerializer.SerializeToDocument(new
        {
            type = "object",
            properties = new
            {
                text = new { type = "string", description = "Input text" }
            },
            required = new[] { "text" }
        });

        var mcpSchema = new ToolSchema
        {
            Name = "text-processor",
            Description = "Processes text input",
            ServerId = "test-server",
            Version = "1.0.0",
            InputSchema = inputSchema
        };

        // Act
        var result = _adapter.AdaptSchema(mcpSchema);

        // Assert
        Assert.IsType<OpenAiFunctionSchema>(result);
        var openAiSchema = (OpenAiFunctionSchema)result;
        Assert.Equal("text-processor", openAiSchema.Name);
        Assert.Equal("Processes text input", openAiSchema.Description);
        Assert.NotNull(openAiSchema.Parameters);
        Assert.Equal(mcpSchema, openAiSchema.OriginalSchema);
    }

    [Fact]
    public void AdaptSchema_WithNullInputSchema_ShouldProvideEmptyParameters()
    {
        // Arrange
        var mcpSchema = new ToolSchema
        {
            Name = "simple-tool",
            Description = "Simple tool with no parameters",
            ServerId = "test-server",
            Version = "1.0.0",
            InputSchema = null
        };

        // Act
        var result = _adapter.AdaptSchema(mcpSchema);

        // Assert
        Assert.IsType<OpenAiFunctionSchema>(result);
        var openAiSchema = (OpenAiFunctionSchema)result;
        Assert.NotNull(openAiSchema.Parameters);
        
        var parameters = openAiSchema.Parameters!.RootElement;
        Assert.Equal("object", parameters.GetProperty("type").GetString());
        Assert.True(parameters.TryGetProperty("properties", out _));
        Assert.True(parameters.TryGetProperty("required", out _));
    }

    [Fact]
    public void AdaptSchema_WithNullDescription_ShouldProvideDefaultDescription()
    {
        // Arrange
        var mcpSchema = new ToolSchema
        {
            Name = "no-desc-tool",
            Description = null,
            ServerId = "test-server",
            Version = "1.0.0"
        };

        // Act
        var result = _adapter.AdaptSchema(mcpSchema);

        // Assert
        Assert.IsType<OpenAiFunctionSchema>(result);
        var openAiSchema = (OpenAiFunctionSchema)result;
        Assert.Equal("MCP tool function", openAiSchema.Description);
    }

    [Fact]
    public void AdaptToolCall_WithValidCall_ShouldReturnOpenAiCall()
    {
        // Arrange
        var parameters = JsonSerializer.SerializeToDocument(new
        {
            text = "Hello, world!"
        });

        var toolCall = new ToolCall
        {
            CallId = "call-123",
            ToolName = "text-processor",
            Parameters = parameters
        };

        // Act
        var result = _adapter.AdaptToolCall(toolCall);

        // Assert
        Assert.IsType<OpenAiFunctionCall>(result);
        var openAiCall = (OpenAiFunctionCall)result;
        Assert.Equal("call-123", openAiCall.CallId);
        Assert.Equal("text-processor", openAiCall.ToolName);
        Assert.Contains("Hello, world!", openAiCall.Arguments);
    }

    [Fact]
    public void AdaptToolCall_WithNullParameters_ShouldProvideEmptyArguments()
    {
        // Arrange
        var toolCall = new ToolCall
        {
            CallId = "call-456",
            ToolName = "simple-tool",
            Parameters = null
        };

        // Act
        var result = _adapter.AdaptToolCall(toolCall);

        // Assert
        Assert.IsType<OpenAiFunctionCall>(result);
        var openAiCall = (OpenAiFunctionCall)result;
        Assert.Equal("{}", openAiCall.Arguments);
    }

    [Fact]
    public void AdaptResult_WithSuccessfulResult_ShouldReturnMcpResult()
    {
        // Arrange
        var openAiResult = new OpenAiFunctionResult
        {
            CallId = "call-789",
            IsSuccess = true,
            Content = "Processing completed successfully"
        };

        // Act
        var result = _adapter.AdaptResult(openAiResult);

        // Assert
        Assert.Equal("call-789", result.CallId);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Content);
        
        var content = result.Content!.RootElement;
        Assert.Equal("Processing completed successfully", content.GetProperty("content").GetString());
        Assert.Equal("text", content.GetProperty("type").GetString());
    }

    [Fact]
    public void AdaptResult_WithFailedResult_ShouldReturnMcpError()
    {
        // Arrange
        var openAiResult = new OpenAiFunctionResult
        {
            CallId = "call-error",
            IsSuccess = false,
            Error = "Tool execution failed"
        };

        // Act
        var result = _adapter.AdaptResult(openAiResult);

        // Assert
        Assert.Equal("call-error", result.CallId);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("execution_error", result.Error.Code);
        Assert.Equal("Tool execution failed", result.Error.Message);
    }

    [Fact]
    public void AdaptResult_WithWrongResultType_ShouldThrowArgumentException()
    {
        // Arrange
        var wrongResult = new Mock<ProviderToolResult>().Object;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _adapter.AdaptResult(wrongResult));
    }

    [Fact]
    public void AdaptSchema_WithNullSchema_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _adapter.AdaptSchema(null!));
    }

    [Fact]
    public void AdaptToolCall_WithNullCall_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _adapter.AdaptToolCall(null!));
    }

    [Fact]
    public void AdaptResult_WithNullResult_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _adapter.AdaptResult(null!));
    }
}