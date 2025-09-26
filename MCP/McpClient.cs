using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FluentAI.MCP;

/// <summary>
/// Default implementation of the MCP client.
/// </summary>
public class McpClient : IMcpClient
{
    private readonly IMcpConnection _connection;
    private readonly ILogger<McpClient> _logger;
    private bool _disposed = false;
    private bool _initialized = false;

    /// <summary>
    /// Initializes a new instance of the McpClient class.
    /// </summary>
    /// <param name="connection">The MCP connection to use.</param>
    /// <param name="logger">Logger instance.</param>
    public McpClient(IMcpConnection connection, ILogger<McpClient> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        // Forward connection state changes
        _connection.ConnectionStateChanged += (sender, args) => 
            ConnectionStateChanged?.Invoke(this, args);
    }

    /// <inheritdoc />
    public string ServerId => _connection.ConnectionId;

    /// <inheritdoc />
    public bool IsConnected => _connection.IsConnected;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? ConnectionStateChanged;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
            return;

        try
        {
            _logger.LogDebug("Initializing MCP client for server {ServerId}", ServerId);

            // Perform MCP initialization handshake
            var initRequest = CreateInitializeRequest();
            var response = await _connection.SendRequestAsync(initRequest, cancellationToken);

            // Process initialization response
            ProcessInitializeResponse(response);

            _initialized = true;
            _logger.LogInformation("MCP client initialized successfully for server {ServerId}", ServerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MCP client for server {ServerId}", ServerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ToolSchema>> ListToolsAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotInitialized();

        try
        {
            _logger.LogDebug("Listing tools from MCP server {ServerId}", ServerId);

            var listRequest = CreateListToolsRequest();
            var response = await _connection.SendRequestAsync(listRequest, cancellationToken);

            var tools = ParseToolsFromResponse(response);
            _logger.LogDebug("Found {ToolCount} tools from server {ServerId}", tools.Count, ServerId);

            return tools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list tools from server {ServerId}", ServerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ToolResult> ExecuteToolAsync(ToolCall toolCall, CancellationToken cancellationToken = default)
    {
        ThrowIfNotInitialized();
        ArgumentNullException.ThrowIfNull(toolCall);

        try
        {
            _logger.LogDebug("Executing tool {ToolName} on server {ServerId}", toolCall.ToolName, ServerId);

            var executeRequest = CreateExecuteToolRequest(toolCall);
            var response = await _connection.SendRequestAsync(executeRequest, cancellationToken);

            var result = ParseToolResultFromResponse(response, toolCall.CallId);
            
            if (result.IsSuccess)
            {
                _logger.LogDebug("Tool {ToolName} executed successfully", toolCall.ToolName);
            }
            else
            {
                _logger.LogWarning("Tool {ToolName} execution failed: {Error}", 
                    toolCall.ToolName, result.Error?.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute tool {ToolName} on server {ServerId}", 
                toolCall.ToolName, ServerId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (!_initialized || _disposed)
            return;

        try
        {
            _logger.LogDebug("Closing MCP client for server {ServerId}", ServerId);
            
            // Send close notification if connected
            if (_connection.IsConnected)
            {
                var closeRequest = CreateCloseRequest();
                await _connection.SendRequestAsync(closeRequest, cancellationToken);
            }

            _logger.LogInformation("MCP client closed for server {ServerId}", ServerId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during MCP client close for server {ServerId}", ServerId);
        }
        finally
        {
            _initialized = false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the McpClient and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during disposal of MCP client for server {ServerId}", ServerId);
            }
            finally
            {
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("MCP client must be initialized before use. Call InitializeAsync() first.");
    }

    private JsonDocument CreateInitializeRequest()
    {
        var request = new
        {
            jsonrpc = "2.0",
            method = "initialize",
            id = Guid.NewGuid().ToString(),
            @params = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                clientInfo = new
                {
                    name = "FluentAI.NET",
                    version = "1.0.0"
                }
            }
        };

        return JsonSerializer.SerializeToDocument(request);
    }

    private JsonDocument CreateListToolsRequest()
    {
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/list",
            id = Guid.NewGuid().ToString()
        };

        return JsonSerializer.SerializeToDocument(request);
    }

    private JsonDocument CreateExecuteToolRequest(ToolCall toolCall)
    {
        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            id = toolCall.CallId ?? Guid.NewGuid().ToString(),
            @params = new
            {
                name = toolCall.ToolName,
                arguments = toolCall.Parameters
            }
        };

        return JsonSerializer.SerializeToDocument(request);
    }

    private JsonDocument CreateCloseRequest()
    {
        var request = new
        {
            jsonrpc = "2.0",
            method = "notifications/cancelled",
            @params = new { }
        };

        return JsonSerializer.SerializeToDocument(request);
    }

    private void ProcessInitializeResponse(JsonDocument response)
    {
        // Parse and validate initialization response
        if (response.RootElement.TryGetProperty("error", out var error))
        {
            var errorMessage = error.GetProperty("message").GetString();
            throw new InvalidOperationException($"MCP initialization failed: {errorMessage}");
        }

        // Additional capability negotiation can be added here
    }

    private IReadOnlyList<ToolSchema> ParseToolsFromResponse(JsonDocument response)
    {
        var tools = new List<ToolSchema>();

        if (!response.RootElement.TryGetProperty("result", out var result) ||
            !result.TryGetProperty("tools", out var toolsArray))
        {
            return tools;
        }

        foreach (var toolElement in toolsArray.EnumerateArray())
        {
            var tool = new ToolSchema
            {
                Name = toolElement.GetProperty("name").GetString()!,
                Description = toolElement.TryGetProperty("description", out var desc) 
                    ? desc.GetString() : null,
                ServerId = ServerId
            };

            if (toolElement.TryGetProperty("inputSchema", out var schema))
            {
                tool.InputSchema = JsonDocument.Parse(schema.GetRawText());
            }

            tools.Add(tool);
        }

        return tools;
    }

    private ToolResult ParseToolResultFromResponse(JsonDocument response, string? callId)
    {
        var result = new ToolResult { CallId = callId };

        if (response.RootElement.TryGetProperty("error", out var error))
        {
            result.IsSuccess = false;
            result.Error = new ToolError
            {
                Code = error.TryGetProperty("code", out var code) ? code.GetInt32().ToString() : "unknown",
                Message = error.TryGetProperty("message", out var msg) ? msg.GetString()! : "Unknown error"
            };
        }
        else if (response.RootElement.TryGetProperty("result", out var resultElement))
        {
            result.IsSuccess = true;
            result.Content = JsonDocument.Parse(resultElement.GetRawText());
        }

        return result;
    }
}