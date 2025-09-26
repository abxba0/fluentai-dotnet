using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FluentAI.MCP.Transport;

/// <summary>
/// WebSocket transport implementation for MCP server communication.
/// </summary>
public class WebSocketTransport : IMcpTransport
{
    private readonly ILogger<WebSocketTransport> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the WebSocketTransport class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public WebSocketTransport(ILogger<WebSocketTransport> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool SupportsConfig(McpServerConfig config)
    {
        return config.TransportType == McpTransportType.WebSocket;
    }

    /// <inheritdoc />
    public async Task<IMcpConnection> ConnectAsync(McpServerConfig config, CancellationToken cancellationToken = default)
    {
        if (!SupportsConfig(config))
            throw new ArgumentException($"WebSocket transport does not support transport type: {config.TransportType}");

        _logger.LogDebug("Creating WebSocket connection to MCP server {ServerId}", config.ServerId);

        var connection = new WebSocketConnection(config, _logger);
        await connection.ConnectAsync(cancellationToken);

        _logger.LogInformation("Successfully connected to MCP server {ServerId} via WebSocket", config.ServerId);
        return connection;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the WebSocketTransport and optionally releases the managed resources.
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
/// WebSocket-based MCP connection implementation.
/// </summary>
internal class WebSocketConnection : IMcpConnection
{
    private readonly McpServerConfig _config;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonDocument>> _pendingRequests = new();
    private readonly Channel<JsonDocument> _outgoingChannel;
    private readonly ChannelWriter<JsonDocument> _outgoingWriter;
    private readonly ChannelReader<JsonDocument> _outgoingReader;

    private ClientWebSocket? _webSocket;
    private ConnectionState _state = ConnectionState.Disconnected;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _readerTask;
    private Task? _writerTask;
    private bool _disposed = false;

    public WebSocketConnection(McpServerConfig config, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var channel = Channel.CreateUnbounded<JsonDocument>();
        _outgoingChannel = channel;
        _outgoingWriter = channel.Writer;
        _outgoingReader = channel.Reader;
    }

    /// <inheritdoc />
    public string ConnectionId => _config.ServerId;

    /// <inheritdoc />
    public bool IsConnected => _state == ConnectionState.Connected && 
                              _webSocket?.State == WebSocketState.Open;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        try
        {
            SetConnectionState(ConnectionState.Connecting);

            // Parse the WebSocket URI
            if (!Uri.TryCreate(_config.ConnectionString, UriKind.Absolute, out var uri))
                throw new ArgumentException($"Invalid WebSocket URI: {_config.ConnectionString}");

            _logger.LogDebug("Connecting to WebSocket URI: {Uri}", uri);

            // Create and configure WebSocket client
            _webSocket = new ClientWebSocket();
            
            // Configure WebSocket options
            ConfigureWebSocket(_webSocket, _config);

            // Connect with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_config.ConnectionTimeout);

            await _webSocket.ConnectAsync(uri, timeoutCts.Token);

            // Start communication tasks
            _cancellationTokenSource = new CancellationTokenSource();
            _readerTask = ReadMessagesAsync(_cancellationTokenSource.Token);
            _writerTask = WriteMessagesAsync(_cancellationTokenSource.Token);

            SetConnectionState(ConnectionState.Connected);
            _logger.LogInformation("Connected to MCP server {ServerId} via WebSocket", ConnectionId);
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
            throw new InvalidOperationException("WebSocket connection is not active");

        // Extract request ID for response correlation
        var requestId = ExtractRequestId(request);
        if (string.IsNullOrEmpty(requestId))
            throw new ArgumentException("Request must have an 'id' field");

        var tcs = new TaskCompletionSource<JsonDocument>();
        _pendingRequests[requestId] = tcs;

        try
        {
            // Send the request
            await _outgoingWriter.WriteAsync(request, cancellationToken);

            // Wait for response with timeout
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_config.RequestTimeout);

            var response = await tcs.Task.WaitAsync(timeoutCts.Token);
            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Request {requestId} timed out after {_config.RequestTimeout}");
        }
        finally
        {
            _pendingRequests.TryRemove(requestId, out _);
        }
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
                // Cancel ongoing operations
                _cancellationTokenSource?.Cancel();

                // Close outgoing channel
                _outgoingWriter.Complete();

                // Wait for tasks to complete
                var tasks = new[] { _readerTask, _writerTask }.Where(t => t != null).Cast<Task>().ToArray();
                if (tasks.Any())
                {
                    Task.WaitAll(tasks, TimeSpan.FromSeconds(5));
                }

                // Close WebSocket
                if (_webSocket?.State == WebSocketState.Open)
                {
                    _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None)
                        .GetAwaiter().GetResult();
                }

                _webSocket?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during WebSocket connection disposal");
            }
            finally
            {
                SetConnectionState(ConnectionState.Disconnected);
                _disposed = true;
            }
        }
    }

    private void ConfigureWebSocket(ClientWebSocket webSocket, McpServerConfig config)
    {
        // Configure WebSocket options based on config
        if (config.Options.TryGetValue("Subprotocol", out var subprotocol) && subprotocol is string subprotocolStr)
        {
            webSocket.Options.AddSubProtocol(subprotocolStr);
        }

        // Set keep-alive interval
        webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

        // Configure headers if provided
        if (config.Options.TryGetValue("Headers", out var headers) && headers is Dictionary<string, string> headerDict)
        {
            foreach (var (key, value) in headerDict)
            {
                webSocket.Options.SetRequestHeader(key, value);
            }
        }
    }

    private async Task ReadMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var buffer = new byte[4096];
            var messageBuffer = new List<byte>();

            while (!cancellationToken.IsCancellationRequested && _webSocket!.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed by server: {CloseStatus} - {CloseDescription}", 
                        result.CloseStatus, result.CloseStatusDescription);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    messageBuffer.AddRange(buffer.Take(result.Count));

                    if (result.EndOfMessage)
                    {
                        var messageJson = Encoding.UTF8.GetString(messageBuffer.ToArray());
                        messageBuffer.Clear();

                        try
                        {
                            var message = JsonDocument.Parse(messageJson);
                            await ProcessIncomingMessage(message);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse JSON message: {Message}", messageJson);
                        }
                    }
                }
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error in WebSocket message reader for connection {ConnectionId}", ConnectionId);
            SetConnectionState(ConnectionState.Failed, ex);
        }
    }

    private async Task WriteMessagesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in _outgoingReader.ReadAllAsync(cancellationToken))
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);

                await _webSocket!.SendAsync(
                    new ArraySegment<byte>(bytes), 
                    WebSocketMessageType.Text, 
                    true, 
                    cancellationToken);

                _logger.LogTrace("Sent WebSocket message: {Message}", json);
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error in WebSocket message writer for connection {ConnectionId}", ConnectionId);
            SetConnectionState(ConnectionState.Failed, ex);
        }
    }

    private async Task ProcessIncomingMessage(JsonDocument message)
    {
        var messageId = ExtractRequestId(message);
        
        // Check if this is a response to a pending request
        if (!string.IsNullOrEmpty(messageId) && _pendingRequests.TryRemove(messageId, out var tcs))
        {
            tcs.SetResult(message);
        }
        else
        {
            // Handle notifications or unsolicited messages
            _logger.LogDebug("Received unsolicited WebSocket message: {MessageId}", messageId ?? "no-id");
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
            _logger.LogDebug("WebSocket connection {ConnectionId} state changed: {PreviousState} -> {NewState}", 
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