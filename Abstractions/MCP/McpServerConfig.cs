namespace FluentAI.Abstractions.MCP;

/// <summary>
/// Configuration for connecting to an MCP server.
/// </summary>
public class McpServerConfig
{
    /// <summary>
    /// Gets or sets the server identifier.
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// Gets or sets the transport type to use for connection.
    /// </summary>
    public McpTransportType TransportType { get; set; }

    /// <summary>
    /// Gets or sets the connection string or parameters specific to the transport type.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the timeout for connection establishment.
    /// </summary>
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the timeout for individual requests.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Gets or sets additional transport-specific options.
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic reconnection.
    /// </summary>
    public bool EnableAutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between reconnection attempts.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);
}

/// <summary>
/// Supported MCP transport types.
/// </summary>
public enum McpTransportType
{
    /// <summary>
    /// Standard Input/Output transport (subprocess).
    /// </summary>
    Stdio,

    /// <summary>
    /// Server-Sent Events transport (HTTP).
    /// </summary>
    SSE,

    /// <summary>
    /// WebSocket transport.
    /// </summary>
    WebSocket
}