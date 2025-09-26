using System.Text.Json;

namespace FluentAI.Abstractions.MCP;

/// <summary>
/// Defines the contract for MCP transport layer implementations.
/// </summary>
public interface IMcpTransport : IDisposable
{
    /// <summary>
    /// Establishes a connection to the MCP server using the specified configuration.
    /// </summary>
    /// <param name="config">The MCP server configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An active MCP connection.</returns>
    Task<IMcpConnection> ConnectAsync(McpServerConfig config, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines if this transport can handle the specified configuration.
    /// </summary>
    /// <param name="config">The MCP server configuration to evaluate.</param>
    /// <returns>True if this transport can handle the configuration, false otherwise.</returns>
    bool SupportsConfig(McpServerConfig config);
}

/// <summary>
/// Represents an active connection to an MCP server.
/// </summary>
public interface IMcpConnection : IDisposable
{
    /// <summary>
    /// Gets the connection identifier.
    /// </summary>
    string ConnectionId { get; }

    /// <summary>
    /// Gets a value indicating whether the connection is active.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Sends a request to the MCP server and awaits a response.
    /// </summary>
    /// <param name="request">The JSON-RPC request to send.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The JSON-RPC response from the server.</returns>
    Task<JsonDocument> SendRequestAsync(JsonDocument request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when the connection state changes.
    /// </summary>
    event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;
}

/// <summary>
/// Event arguments for connection state changes.
/// </summary>
public class ConnectionStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previous connection state.
    /// </summary>
    public ConnectionState PreviousState { get; init; }

    /// <summary>
    /// Gets the current connection state.
    /// </summary>
    public ConnectionState CurrentState { get; init; }

    /// <summary>
    /// Gets the optional error that caused the state change.
    /// </summary>
    public Exception? Error { get; init; }
}

/// <summary>
/// Represents the state of an MCP connection.
/// </summary>
public enum ConnectionState
{
    /// <summary>
    /// Connection is disconnected.
    /// </summary>
    Disconnected,

    /// <summary>
    /// Connection is in the process of connecting.
    /// </summary>
    Connecting,

    /// <summary>
    /// Connection is active and ready for use.
    /// </summary>
    Connected,

    /// <summary>
    /// Connection is in the process of disconnecting.
    /// </summary>
    Disconnecting,

    /// <summary>
    /// Connection has failed due to an error.
    /// </summary>
    Failed
}