using FluentAI.Abstractions.MCP;
using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions;

/// <summary>
/// Extended interface for chat models that support MCP tool integration.
/// </summary>
public interface IToolEnabledChatModel : IChatModel
{
    /// <summary>
    /// Gets a value indicating whether the model supports tool/function calling.
    /// </summary>
    bool SupportsTools { get; }

    /// <summary>
    /// Gets the maximum number of tools that can be used in a single request.
    /// </summary>
    int MaxToolsPerRequest { get; }

    /// <summary>
    /// Gets a response from the model with automatic MCP tool execution.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="availableTools">The MCP tools available for use.</param>
    /// <param name="options">Optional request options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The chat response with any tool executions performed.</returns>
    Task<ToolEnabledChatResponse> GetResponseWithToolsAsync(
        IEnumerable<ChatMessage> messages,
        IEnumerable<ToolSchema> availableTools,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a response from the model with automatic MCP tool execution.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="availableTools">The MCP tools available for use.</param>
    /// <param name="options">Optional request options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Stream of response chunks and tool execution results.</returns>
    IAsyncEnumerable<ToolEnabledResponseChunk> StreamResponseWithToolsAsync(
        IEnumerable<ChatMessage> messages,
        IEnumerable<ToolSchema> availableTools,
        ChatRequestOptions? options = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a chat response that may include tool executions.
/// </summary>
public class ToolEnabledChatResponse
{
    /// <summary>
    /// Gets or sets the chat content.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the model identifier.
    /// </summary>
    public string? ModelId { get; set; }

    /// <summary>
    /// Gets or sets the usage statistics for the request.
    /// </summary>
    public TokenUsage? Usage { get; set; }

    /// <summary>
    /// Gets or sets the tool calls made during the response.
    /// </summary>
    public IReadOnlyList<ToolCallExecution> ToolExecutions { get; set; } = Array.Empty<ToolCallExecution>();

    /// <summary>
    /// Gets or sets a value indicating whether the response requires additional tool execution.
    /// </summary>
    public bool RequiresToolExecution { get; set; }

    /// <summary>
    /// Gets or sets the updated conversation messages including tool interactions.
    /// </summary>
    public IReadOnlyList<ChatMessage> UpdatedMessages { get; set; } = Array.Empty<ChatMessage>();

    /// <summary>
    /// Converts this tool-enabled response to a standard ChatResponse.
    /// </summary>
    /// <returns>A ChatResponse containing the core response data.</returns>
    public ChatResponse ToChatResponse()
    {
        return new ChatResponse(
            Content,
            ModelId ?? "unknown",
            "stop", // Default finish reason
            Usage ?? new TokenUsage(0, 0) // Default empty usage
        );
    }
}

/// <summary>
/// Represents a streaming response chunk that may include tool execution information.
/// </summary>
public class ToolEnabledResponseChunk
{
    /// <summary>
    /// Gets or sets the response text content.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the type of chunk.
    /// </summary>
    public ResponseChunkType ChunkType { get; set; }

    /// <summary>
    /// Gets or sets tool execution information if applicable.
    /// </summary>
    public ToolCallExecution? ToolExecution { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the chunk.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the execution of a tool call.
/// </summary>
public class ToolCallExecution
{
    /// <summary>
    /// Gets or sets the call identifier.
    /// </summary>
    public required string CallId { get; set; }

    /// <summary>
    /// Gets or sets the name of the tool that was called.
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// Gets or sets the parameters that were passed to the tool.
    /// </summary>
    public System.Text.Json.JsonDocument? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the result of the tool execution.
    /// </summary>
    public System.Text.Json.JsonDocument? Result { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tool execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the error information if the execution failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets the execution duration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Types of response chunks in a streaming tool-enabled response.
/// </summary>
public enum ResponseChunkType
{
    /// <summary>
    /// Regular text content chunk.
    /// </summary>
    Content,

    /// <summary>
    /// Tool call initiation chunk.
    /// </summary>
    ToolCallStart,

    /// <summary>
    /// Tool execution result chunk.
    /// </summary>
    ToolCallResult,

    /// <summary>
    /// Tool execution error chunk.
    /// </summary>
    ToolCallError,

    /// <summary>
    /// End of response marker.
    /// </summary>
    EndOfResponse
}