using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FluentAI.MCP;

/// <summary>
/// Orchestrates tool execution across multiple MCP servers with error handling and retry logic.
/// </summary>
public class ToolExecutionOrchestrator : IDisposable
{
    private readonly McpConnectionPool _connectionPool;
    private readonly IToolRegistry _toolRegistry;
    private readonly IEnumerable<IToolSchemaAdapter> _adapters;
    private readonly ILogger<ToolExecutionOrchestrator> _logger;
    private readonly ConcurrentDictionary<string, IMcpClient> _clients = new();
    private readonly object _lockObject = new();
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the ToolExecutionOrchestrator class.
    /// </summary>
    /// <param name="connectionPool">The MCP connection pool.</param>
    /// <param name="toolRegistry">The tool registry.</param>
    /// <param name="adapters">Available tool schema adapters.</param>
    /// <param name="logger">Logger instance.</param>
    public ToolExecutionOrchestrator(
        McpConnectionPool connectionPool,
        IToolRegistry toolRegistry,
        IEnumerable<IToolSchemaAdapter> adapters,
        ILogger<ToolExecutionOrchestrator> logger)
    {
        _connectionPool = connectionPool ?? throw new ArgumentNullException(nameof(connectionPool));
        _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
        _adapters = adapters ?? throw new ArgumentNullException(nameof(adapters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executes a tool across all available MCP servers.
    /// </summary>
    /// <param name="toolName">The name of the tool to execute.</param>
    /// <param name="parameters">The parameters to pass to the tool.</param>
    /// <param name="providerId">Optional provider ID for adapter selection.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the tool execution.</returns>
    public async Task<ToolExecutionResult> ExecuteToolAsync(
        string toolName, 
        object? parameters = null,
        string? providerId = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toolName);

        _logger.LogDebug("Executing tool {ToolName} with provider {ProviderId}", 
            toolName, providerId ?? "any");

        try
        {
            // Find the tool in the registry
            var tool = await _toolRegistry.GetToolAsync(toolName, cancellationToken);
            if (tool == null)
            {
                _logger.LogWarning("Tool {ToolName} not found in registry", toolName);
                return new ToolExecutionResult
                {
                    IsSuccess = false,
                    Error = new ToolExecutionError
                    {
                        Code = "tool_not_found",
                        Message = $"Tool '{toolName}' not found in registry"
                    }
                };
            }

            // Get or create MCP client for the server
            var client = await GetOrCreateClientAsync(tool.ServerId, cancellationToken);
            if (client == null)
            {
                _logger.LogError("Failed to get MCP client for server {ServerId}", tool.ServerId);
                return new ToolExecutionResult
                {
                    IsSuccess = false,
                    Error = new ToolExecutionError
                    {
                        Code = "server_unavailable",
                        Message = $"MCP server '{tool.ServerId}' is not available"
                    }
                };
            }

            // Create tool call
            var toolCall = CreateToolCall(toolName, parameters);

            // Execute the tool with retry logic
            var result = await ExecuteWithRetryAsync(client, toolCall, cancellationToken);

            // Adapt result if provider is specified
            if (!string.IsNullOrEmpty(providerId) && result.IsSuccess)
            {
                result = AdaptResultForProvider(result, providerId);
            }

            _logger.LogDebug("Tool {ToolName} execution completed: {Success}", 
                toolName, result.IsSuccess);

            return ConvertToExecutionResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
            return new ToolExecutionResult
            {
                IsSuccess = false,
                Error = new ToolExecutionError
                {
                    Code = "execution_error",
                    Message = $"Tool execution failed: {ex.Message}"
                }
            };
        }
    }

    /// <summary>
    /// Lists all available tools from all registered MCP servers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>List of available tools with their capabilities.</returns>
    public async Task<IReadOnlyList<ToolInfo>> ListAvailableToolsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Listing all available tools from MCP servers");

        try
        {
            var tools = await _toolRegistry.GetToolsAsync(cancellationToken);
            var toolInfos = new List<ToolInfo>();

            foreach (var tool in tools)
            {
                var adaptedCapabilities = new List<string>();

                // Check which providers can use this tool
                foreach (var adapter in _adapters)
                {
                    if (adapter.CanAdapt(tool))
                    {
                        adaptedCapabilities.Add(adapter.ProviderId);
                    }
                }

                toolInfos.Add(new ToolInfo
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    ServerId = tool.ServerId,
                    Version = tool.Version,
                    SupportedProviders = adaptedCapabilities,
                    HasInputSchema = tool.InputSchema != null
                });
            }

            _logger.LogInformation("Found {ToolCount} available tools across {ServerCount} servers", 
                toolInfos.Count, tools.Select(t => t.ServerId).Distinct().Count());

            return toolInfos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing available tools");
            return Array.Empty<ToolInfo>();
        }
    }

    /// <summary>
    /// Initializes all registered MCP servers and discovers their tools.
    /// </summary>
    /// <param name="serverConfigs">The server configurations to initialize.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    public async Task InitializeServersAsync(
        IEnumerable<McpServerConfig> serverConfigs, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing MCP servers");

        var initializationTasks = serverConfigs.Select(async config =>
        {
            try
            {
                _logger.LogDebug("Initializing MCP server {ServerId}", config.ServerId);

                var client = await GetOrCreateClientAsync(config.ServerId, cancellationToken);
                if (client != null)
                {
                    await client.InitializeAsync(cancellationToken);
                    
                    // Discover and register tools
                    var tools = await client.ListToolsAsync(cancellationToken);
                    await _toolRegistry.RegisterToolsAsync(config.ServerId, tools, cancellationToken);

                    _logger.LogInformation("Initialized MCP server {ServerId} with {ToolCount} tools",
                        config.ServerId, tools.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize MCP server {ServerId}", config.ServerId);
            }
        });

        await Task.WhenAll(initializationTasks);
        _logger.LogInformation("MCP server initialization completed");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the ToolExecutionOrchestrator and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _logger.LogDebug("Disposing tool execution orchestrator");

            foreach (var client in _clients.Values)
            {
                try
                {
                    client.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing MCP client");
                }
            }

            _clients.Clear();
            _disposed = true;
        }
    }

    private async Task<IMcpClient?> GetOrCreateClientAsync(string serverId, CancellationToken cancellationToken)
    {
        if (_clients.TryGetValue(serverId, out var existingClient) && existingClient.IsConnected)
        {
            return existingClient;
        }

        lock (_lockObject)
        {
            // Double-check pattern
            if (_clients.TryGetValue(serverId, out existingClient) && existingClient.IsConnected)
            {
                return existingClient;
            }

            // This is a simplified implementation - in a real scenario, 
            // we would need the server config to create the client
            // For now, we'll return null to indicate the server is not available
            _logger.LogWarning("MCP client for server {ServerId} not available - config needed", serverId);
            return null;
        }
    }

    private ToolCall CreateToolCall(string toolName, object? parameters)
    {
        var call = new ToolCall
        {
            CallId = Guid.NewGuid().ToString(),
            ToolName = toolName
        };

        if (parameters != null)
        {
            call.Parameters = System.Text.Json.JsonSerializer.SerializeToDocument(parameters);
        }

        return call;
    }

    private async Task<ToolResult> ExecuteWithRetryAsync(
        IMcpClient client, 
        ToolCall toolCall, 
        CancellationToken cancellationToken,
        int maxRetries = 3)
    {
        var attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            try
            {
                _logger.LogDebug("Executing tool {ToolName}, attempt {Attempt}/{MaxRetries}",
                    toolCall.ToolName, attempt + 1, maxRetries);

                var result = await client.ExecuteToolAsync(toolCall, cancellationToken);
                
                if (result.IsSuccess || attempt == maxRetries - 1)
                {
                    return result;
                }

                _logger.LogWarning("Tool execution failed, attempt {Attempt}: {Error}",
                    attempt + 1, result.Error?.Message);
                
                // Wait before retry with exponential backoff
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogWarning(ex, "Tool execution exception, attempt {Attempt}", attempt + 1);
                
                if (attempt == maxRetries - 1)
                    break;

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay, cancellationToken);
            }

            attempt++;
        }

        // All retries failed
        return new ToolResult
        {
            CallId = toolCall.CallId,
            IsSuccess = false,
            Error = new ToolError
            {
                Code = "max_retries_exceeded",
                Message = lastException?.Message ?? "Tool execution failed after maximum retries"
            }
        };
    }

    private ToolResult AdaptResultForProvider(ToolResult result, string providerId)
    {
        // This would adapt the result format for specific providers
        // For now, return the result as-is
        return result;
    }

    private ToolExecutionResult ConvertToExecutionResult(ToolResult result)
    {
        return new ToolExecutionResult
        {
            CallId = result.CallId,
            IsSuccess = result.IsSuccess,
            Content = result.Content,
            Error = result.Error != null ? new ToolExecutionError
            {
                Code = result.Error.Code,
                Message = result.Error.Message,
                Data = result.Error.Data
            } : null,
            Metadata = result.Metadata
        };
    }
}

/// <summary>
/// Represents the result of a tool execution operation.
/// </summary>
public class ToolExecutionResult
{
    /// <summary>
    /// Gets or sets the call identifier.
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the execution was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the result content.
    /// </summary>
    public System.Text.Json.JsonDocument? Content { get; set; }

    /// <summary>
    /// Gets or sets the error information if execution failed.
    /// </summary>
    public ToolExecutionError? Error { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents an error that occurred during tool execution.
/// </summary>
public class ToolExecutionError
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
    public System.Text.Json.JsonDocument? Data { get; set; }
}

/// <summary>
/// Represents information about an available tool.
/// </summary>
public class ToolInfo
{
    /// <summary>
    /// Gets or sets the tool name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the tool description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the server providing this tool.
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// Gets or sets the tool version.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Gets or sets the list of AI providers that support this tool.
    /// </summary>
    public List<string> SupportedProviders { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the tool has an input schema.
    /// </summary>
    public bool HasInputSchema { get; set; }
}