using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FluentAI.MCP;

/// <summary>
/// Thread-safe connection pool for managing MCP server connections with bounded concurrency.
/// </summary>
public class McpConnectionPool : IDisposable
{
    private readonly IEnumerable<IMcpTransport> _transports;
    private readonly ILogger<McpConnectionPool> _logger;
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly ConcurrentDictionary<string, IMcpConnection> _connections = new();
    private readonly ConcurrentDictionary<string, McpServerConfig> _configs = new();
    private readonly object _lockObject = new();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the McpConnectionPool class.
    /// </summary>
    /// <param name="transports">Available transport implementations.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="maxConcurrentConnections">Maximum number of concurrent connections. Default is 10.</param>
    public McpConnectionPool(
        IEnumerable<IMcpTransport> transports, 
        ILogger<McpConnectionPool> logger,
        int maxConcurrentConnections = 10)
    {
        _transports = transports ?? throw new ArgumentNullException(nameof(transports));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connectionSemaphore = new SemaphoreSlim(maxConcurrentConnections, maxConcurrentConnections);

        if (maxConcurrentConnections <= 0)
            throw new ArgumentException("Maximum concurrent connections must be greater than zero", nameof(maxConcurrentConnections));
    }

    /// <summary>
    /// Acquires a connection to the specified MCP server.
    /// </summary>
    /// <param name="config">The MCP server configuration.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>An active MCP connection.</returns>
    public async Task<IMcpConnection> AcquireConnectionAsync(McpServerConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        
        await _connectionSemaphore.WaitAsync(cancellationToken);

        try
        {
            // Check if we already have a connection for this server
            if (_connections.TryGetValue(config.ServerId, out var existingConnection) && 
                existingConnection.IsConnected)
            {
                _logger.LogDebug("Reusing existing connection for server {ServerId}", config.ServerId);
                return existingConnection;
            }

            // Remove any stale connection
            if (existingConnection != null)
            {
                _logger.LogDebug("Removing stale connection for server {ServerId}", config.ServerId);
                _connections.TryRemove(config.ServerId, out _);
                existingConnection.Dispose();
            }

            // Create new connection
            _logger.LogDebug("Creating new connection for server {ServerId}", config.ServerId);
            var connection = await CreateConnectionAsync(config, cancellationToken);
            
            // Store the connection and config
            _connections[config.ServerId] = connection;
            _configs[config.ServerId] = config;

            // Monitor connection state for cleanup
            connection.ConnectionStateChanged += OnConnectionStateChanged;

            _logger.LogInformation("Successfully acquired connection for server {ServerId}", config.ServerId);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acquire connection for server {ServerId}", config.ServerId);
            throw;
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    /// <summary>
    /// Gets the current number of active connections.
    /// </summary>
    public int ActiveConnectionCount => _connections.Count(kvp => kvp.Value.IsConnected);

    /// <summary>
    /// Gets the current number of available connection slots.
    /// </summary>
    public int AvailableConnectionSlots => _connectionSemaphore.CurrentCount;

    /// <summary>
    /// Removes and disposes a connection for the specified server.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    public void RemoveConnection(string serverId)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverId);

        if (_connections.TryRemove(serverId, out var connection))
        {
            _logger.LogDebug("Removing connection for server {ServerId}", serverId);
            
            try
            {
                connection.ConnectionStateChanged -= OnConnectionStateChanged;
                connection.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error disposing connection for server {ServerId}", serverId);
            }

            _configs.TryRemove(serverId, out _);
        }
    }

    /// <summary>
    /// Performs cleanup of disconnected connections.
    /// </summary>
    public void CleanupDisconnectedConnections()
    {
        _logger.LogDebug("Starting cleanup of disconnected connections");

        var disconnectedServers = new List<string>();

        foreach (var kvp in _connections)
        {
            if (!kvp.Value.IsConnected)
            {
                disconnectedServers.Add(kvp.Key);
            }
        }

        foreach (var serverId in disconnectedServers)
        {
            RemoveConnection(serverId);
        }

        if (disconnectedServers.Any())
        {
            _logger.LogInformation("Cleaned up {Count} disconnected connections", disconnectedServers.Count);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the McpConnectionPool and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _logger.LogDebug("Disposing MCP connection pool");

            // Close all connections
            foreach (var kvp in _connections)
            {
                try
                {
                    kvp.Value.ConnectionStateChanged -= OnConnectionStateChanged;
                    kvp.Value.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing connection for server {ServerId}", kvp.Key);
                }
            }

            _connections.Clear();
            _configs.Clear();
            _connectionSemaphore.Dispose();
            _disposed = true;

            _logger.LogInformation("MCP connection pool disposed");
        }
    }

    private async Task<IMcpConnection> CreateConnectionAsync(McpServerConfig config, CancellationToken cancellationToken)
    {
        // Find a suitable transport
        var transport = _transports.FirstOrDefault(t => t.SupportsConfig(config));
        if (transport == null)
        {
            throw new NotSupportedException($"No transport available for server type: {config.TransportType}");
        }

        _logger.LogDebug("Using transport {TransportType} for server {ServerId}", 
            transport.GetType().Name, config.ServerId);

        // Create and establish connection
        var connection = await transport.ConnectAsync(config, cancellationToken);
        
        _logger.LogDebug("Successfully created connection for server {ServerId}", config.ServerId);
        return connection;
    }

    private void OnConnectionStateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        if (sender is IMcpConnection connection)
        {
            _logger.LogDebug("Connection {ConnectionId} state changed: {PreviousState} -> {CurrentState}",
                connection.ConnectionId, e.PreviousState, e.CurrentState);

            // If connection failed or disconnected, schedule it for removal
            if (e.CurrentState == ConnectionState.Failed || e.CurrentState == ConnectionState.Disconnected)
            {
                // Schedule cleanup on a background thread to avoid blocking
                Task.Run(() =>
                {
                    try
                    {
                        CleanupDisconnectedConnections();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error during background connection cleanup");
                    }
                });
            }
        }
    }
}