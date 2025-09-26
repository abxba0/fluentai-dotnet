using System.Text.Json;

namespace FluentAI.Abstractions.MCP;

/// <summary>
/// Provides high-level interface for interacting with MCP servers.
/// </summary>
public interface IMcpClient : IDisposable
{
    /// <summary>
    /// Gets the server identifier for this client.
    /// </summary>
    string ServerId { get; }

    /// <summary>
    /// Gets a value indicating whether the client is connected.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Initializes the MCP session and performs capability negotiation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available tools from the MCP server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>List of available tools.</returns>
    Task<IReadOnlyList<ToolSchema>> ListToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool with the specified parameters.
    /// </summary>
    /// <param name="toolCall">The tool call to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the tool execution.</returns>
    Task<ToolResult> ExecuteToolAsync(ToolCall toolCall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the MCP session and connection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when the connection state changes.
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
}

/// <summary>
/// Represents a tool call to be executed on an MCP server.
/// </summary>
public class ToolCall
{
    /// <summary>
    /// Gets or sets the name of the tool to call.
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// Gets or sets the parameters to pass to the tool.
    /// </summary>
    public JsonDocument? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the call identifier for tracking purposes.
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for the call.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of a tool execution.
/// </summary>
public class ToolResult
{
    /// <summary>
    /// Gets or sets the call identifier that was executed.
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tool execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the result content from the tool execution.
    /// </summary>
    public JsonDocument? Content { get; set; }

    /// <summary>
    /// Gets or sets the error information if the execution failed.
    /// </summary>
    public ToolError? Error { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the execution.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents an error that occurred during tool execution.
/// </summary>
public class ToolError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public required string Code { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets additional error data.
    /// </summary>
    public JsonDocument? Data { get; set; }
}