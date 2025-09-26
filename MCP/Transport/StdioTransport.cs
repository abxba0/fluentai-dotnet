using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FluentAI.MCP.Transport;

/// <summary>
/// Standard Input/Output transport implementation for MCP using subprocess communication.
/// </summary>
public class StdioTransport : IMcpTransport
{
    private readonly ILogger<StdioTransport> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the StdioTransport class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public StdioTransport(ILogger<StdioTransport> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool SupportsConfig(McpServerConfig config)
    {
        return config.TransportType == McpTransportType.Stdio;
    }

    /// <inheritdoc />
    public async Task<IMcpConnection> ConnectAsync(McpServerConfig config, CancellationToken cancellationToken = default)
    {
        if (!SupportsConfig(config))
            throw new ArgumentException($"Stdio transport does not support transport type: {config.TransportType}");

        _logger.LogDebug("Creating stdio connection to MCP server {ServerId}", config.ServerId);

        var connection = new StdioConnection(config, _logger);
        await connection.ConnectAsync(cancellationToken);

        _logger.LogInformation("Successfully connected to MCP server {ServerId} via stdio", config.ServerId);
        return connection;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the StdioTransport and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
    }
}

/// <summary>
/// Stdio-based MCP connection implementation.
/// </summary>
internal class StdioConnection : IMcpConnection
{
    private readonly McpServerConfig _config;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonDocument>> _pendingRequests = new();
    
    private Process? _process;
    private ConnectionState _state = ConnectionState.Disconnected;
    private bool _disposed = false;

    public StdioConnection(McpServerConfig config, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ConnectionId => _config.ServerId;

    /// <inheritdoc />
    public bool IsConnected => _state == ConnectionState.Connected && _process?.HasExited == false;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        try
        {
            SetConnectionState(ConnectionState.Connecting);

            // Parse connection string to get command and arguments
            var parts = _config.ConnectionString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                throw new ArgumentException("Invalid connection string for stdio transport");

            var command = parts[0];
            var arguments = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

            _logger.LogDebug("Starting MCP server process: {Command} {Arguments}", command, arguments);

            // Start the MCP server process
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            if (!_process.Start())
                throw new InvalidOperationException("Failed to start MCP server process");

            SetConnectionState(ConnectionState.Connected);
            _logger.LogInformation("Connected to MCP server {ServerId} via stdio", ConnectionId);
        }
        catch (Exception ex)
        {
            SetConnectionState(ConnectionState.Failed, ex);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<JsonDocument> SendRequestAsync(JsonDocument request, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Connection is not active");

        // For now, return a simple mock response
        // In a full implementation, this would send the request via stdio and await the response
        var mockResponse = new
        {
            jsonrpc = "2.0",
            id = ExtractRequestId(request),
            result = new { success = true }
        };

        return JsonSerializer.SerializeToDocument(mockResponse);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            SetConnectionState(ConnectionState.Disconnecting);

            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                    _process.WaitForExit(5000);
                }

                _process?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during stdio connection disposal");
            }
            finally
            {
                SetConnectionState(ConnectionState.Disconnected);
                _disposed = true;
            }
        }
    }

    private string? ExtractRequestId(JsonDocument document)
    {
        if (document.RootElement.TryGetProperty("id", out var idElement))
        {
            return idElement.ValueKind == JsonValueKind.String 
                ? idElement.GetString() 
                : idElement.GetRawText();
        }
        return null;
    }

    private void SetConnectionState(ConnectionState newState, Exception? error = null)
    {
        var previousState = _state;
        _state = newState;

        if (previousState != newState)
        {
            _logger.LogDebug("Connection {ConnectionId} state changed: {PreviousState} -> {NewState}", 
                ConnectionId, previousState, newState);

            ConnectionStateChanged?.Invoke(this, new ConnectionStateChangedEventArgs
            {
                PreviousState = previousState,
                CurrentState = newState,
                Error = error
            });
        }
    }
}