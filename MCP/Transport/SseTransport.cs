using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FluentAI.MCP.Transport;

/// <summary>
/// Server-Sent Events (SSE) transport implementation for MCP server communication.
/// </summary>
public class SseTransport : IMcpTransport
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SseTransport> _logger;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the SseTransport class.
    /// </summary>
    /// <param name="httpClientFactory">HTTP client factory for creating HTTP clients.</param>
    /// <param name="logger">Logger instance.</param>
    public SseTransport(IHttpClientFactory httpClientFactory, ILogger<SseTransport> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool SupportsConfig(McpServerConfig config)
    {
        return config.TransportType == McpTransportType.SSE;
    }

    /// <inheritdoc />
    public async Task<IMcpConnection> ConnectAsync(McpServerConfig config, CancellationToken cancellationToken = default)
    {
        if (!SupportsConfig(config))
            throw new ArgumentException($"SSE transport does not support transport type: {config.TransportType}");

        _logger.LogDebug("Creating SSE connection to MCP server {ServerId}", config.ServerId);

        var connection = new SseConnection(config, _httpClientFactory, _logger);
        await connection.ConnectAsync(cancellationToken);

        _logger.LogInformation("Successfully connected to MCP server {ServerId} via SSE", config.ServerId);
        return connection;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the SseTransport and optionally releases the managed resources.
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
/// SSE-based MCP connection implementation.
/// </summary>
internal class SseConnection : IMcpConnection
{
    private readonly McpServerConfig _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonDocument>> _pendingRequests = new();
    
    private HttpClient? _httpClient;
    private Stream? _sseStream;
    private ConnectionState _state = ConnectionState.Disconnected;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _readerTask;
    private bool _disposed = false;

    public SseConnection(McpServerConfig config, IHttpClientFactory httpClientFactory, ILogger logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ConnectionId => _config.ServerId;

    /// <inheritdoc />
    public bool IsConnected => _state == ConnectionState.Connected && _sseStream != null;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        try
        {
            SetConnectionState(ConnectionState.Connecting);

            // Parse the SSE endpoint URI
            if (!Uri.TryCreate(_config.ConnectionString, UriKind.Absolute, out var uri))
                throw new ArgumentException($"Invalid SSE endpoint URI: {_config.ConnectionString}");

            _logger.LogDebug("Connecting to SSE endpoint: {Uri}", uri);

            // Create HTTP client
            _httpClient = _httpClientFactory.CreateClient("MCP-SSE");
            ConfigureHttpClient(_httpClient, _config);

            // Create SSE request
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Accept", "text/event-stream");
            request.Headers.Add("Cache-Control", "no-cache");

            // Add custom headers if configured
            if (_config.Options.TryGetValue("Headers", out var headers) && headers is Dictionary<string, string> headerDict)
            {
                foreach (var (key, value) in headerDict)
                {
                    request.Headers.Add(key, value);
                }
            }

            // Send request and get response stream
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_config.ConnectionTimeout);

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            response.EnsureSuccessStatusCode();

            _sseStream = await response.Content.ReadAsStreamAsync();

            // Start reading SSE events
            _cancellationTokenSource = new CancellationTokenSource();
            _readerTask = ReadSseEventsAsync(_cancellationTokenSource.Token);

            SetConnectionState(ConnectionState.Connected);
            _logger.LogInformation("Connected to MCP server {ServerId} via SSE", ConnectionId);
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
            throw new InvalidOperationException("SSE connection is not active");

        var requestId = ExtractRequestId(request);
        if (string.IsNullOrEmpty(requestId))
            throw new ArgumentException("Request must have an 'id' field");

        var tcs = new TaskCompletionSource<JsonDocument>();
        _pendingRequests[requestId] = tcs;

        try
        {
            // For SSE, we need a separate endpoint for sending requests
            var sendEndpoint = GetSendEndpoint();
            var requestJson = JsonSerializer.Serialize(request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, sendEndpoint)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };

            // Add authentication if configured
            if (_config.Options.TryGetValue("ApiKey", out var apiKey) && apiKey is string apiKeyStr)
            {
                httpRequest.Headers.Add("Authorization", $"Bearer {apiKeyStr}");
            }

            // Send the request (fire and forget for SSE pattern)
            var response = await _httpClient!.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            // Wait for response via SSE stream
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_config.RequestTimeout);

            var result = await tcs.Task.WaitAsync(timeoutCts.Token);
            return result;
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

                // Wait for reader task to complete
                _readerTask?.Wait(TimeSpan.FromSeconds(5));

                // Dispose resources
                _sseStream?.Dispose();
                _httpClient?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during SSE connection disposal");
            }
            finally
            {
                SetConnectionState(ConnectionState.Disconnected);
                _disposed = true;
            }
        }
    }

    private void ConfigureHttpClient(HttpClient httpClient, McpServerConfig config)
    {
        // Set reasonable timeout for SSE connections
        httpClient.Timeout = TimeSpan.FromMinutes(30); // SSE connections are long-lived
        
        // Configure user agent
        httpClient.DefaultRequestHeaders.Add("User-Agent", "FluentAI.NET-MCP/1.0");
    }

    private string GetSendEndpoint()
    {
        // Convention: if SSE endpoint is /events, send endpoint is /send
        // This can be configured via options
        if (_config.Options.TryGetValue("SendEndpoint", out var sendEndpoint) && sendEndpoint is string endpoint)
        {
            return endpoint;
        }

        // Default convention
        var baseUri = _config.ConnectionString.TrimEnd('/');
        return baseUri.Replace("/events", "/send");
    }

    private async Task ReadSseEventsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var reader = new StreamReader(_sseStream!, Encoding.UTF8, leaveOpen: true);
            var eventBuilder = new SseEventBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await reader.ReadLineAsync();
                if (line == null)
                    break;

                var sseEvent = eventBuilder.ProcessLine(line);
                if (sseEvent != null)
                {
                    await ProcessSseEvent(sseEvent);
                    eventBuilder.Reset();
                }
            }
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Error in SSE event reader for connection {ConnectionId}", ConnectionId);
            SetConnectionState(ConnectionState.Failed, ex);
        }
    }

    private async Task ProcessSseEvent(SseEvent sseEvent)
    {
        if (string.IsNullOrEmpty(sseEvent.Data))
            return;

        try
        {
            var message = JsonDocument.Parse(sseEvent.Data);
            var messageId = ExtractRequestId(message);
            
            // Check if this is a response to a pending request
            if (!string.IsNullOrEmpty(messageId) && _pendingRequests.TryRemove(messageId, out var tcs))
            {
                tcs.SetResult(message);
            }
            else
            {
                // Handle notifications or unsolicited messages
                _logger.LogDebug("Received unsolicited SSE event: {EventType} - {MessageId}", 
                    sseEvent.EventType, messageId ?? "no-id");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse SSE event data as JSON: {Data}", sseEvent.Data);
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
            _logger.LogDebug("SSE connection {ConnectionId} state changed: {PreviousState} -> {NewState}", 
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

/// <summary>
/// Represents a Server-Sent Event.
/// </summary>
internal class SseEvent
{
    public string? EventType { get; set; }
    public string? Data { get; set; }
    public string? Id { get; set; }
    public int? Retry { get; set; }
}

/// <summary>
/// Builder for constructing SSE events from raw text lines.
/// </summary>
internal class SseEventBuilder
{
    private string? _eventType;
    private readonly StringBuilder _data = new();
    private string? _id;
    private int? _retry;

    public SseEvent? ProcessLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            // Empty line indicates end of event
            if (_data.Length > 0 || _eventType != null || _id != null)
            {
                return new SseEvent
                {
                    EventType = _eventType,
                    Data = _data.ToString().TrimEnd('\n'),
                    Id = _id,
                    Retry = _retry
                };
            }
            return null;
        }

        if (line.StartsWith(':'))
        {
            // Comment line, ignore
            return null;
        }

        var colonIndex = line.IndexOf(':');
        if (colonIndex == -1)
        {
            // Field with no value
            return null;
        }

        var field = line.Substring(0, colonIndex).Trim();
        var value = line.Substring(colonIndex + 1).TrimStart();

        switch (field.ToLowerInvariant())
        {
            case "event":
                _eventType = value;
                break;
            case "data":
                _data.AppendLine(value);
                break;
            case "id":
                _id = value;
                break;
            case "retry":
                if (int.TryParse(value, out var retryValue))
                    _retry = retryValue;
                break;
        }

        return null;
    }

    public void Reset()
    {
        _eventType = null;
        _data.Clear();
        _id = null;
        _retry = null;
    }
}