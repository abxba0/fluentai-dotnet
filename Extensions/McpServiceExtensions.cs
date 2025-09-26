using FluentAI.Abstractions.MCP;
using FluentAI.MCP;
using FluentAI.MCP.Adapters;
using FluentAI.MCP.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FluentAI.Extensions;

/// <summary>
/// Extension methods for registering MCP (Model Context Protocol) services.
/// </summary>
public static class McpServiceExtensions
{
    /// <summary>
    /// Adds MCP server support to the FluentAI builder.
    /// </summary>
    /// <param name="builder">The FluentAI builder.</param>
    /// <returns>The builder for method chaining.</returns>
    public static IFluentAiBuilder AddMcpSupport(this IFluentAiBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register core MCP services
        builder.Services.TryAddSingleton<IToolRegistry, ManagedToolRegistry>();
        builder.Services.TryAddSingleton<McpConnectionPool>();

        // Register transport implementations
        builder.Services.TryAddTransient<IMcpTransport, StdioTransport>();

        // Register tool schema adapters
        builder.Services.TryAddTransient<IToolSchemaAdapter, OpenAiToolAdapter>();

        // Register MCP client factory function
        builder.Services.TryAddTransient<Func<McpServerConfig, IMcpClient>>(serviceProvider =>
        {
            return config =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<McpClient>>();
                var connectionPool = serviceProvider.GetRequiredService<McpConnectionPool>();

                // This would create a client with the connection pool
                // For now, return a basic implementation
                var connection = connectionPool.AcquireConnectionAsync(config).GetAwaiter().GetResult();
                return new McpClient(connection, logger);
            };
        });

        return builder;
    }

    /// <summary>
    /// Registers an MCP server configuration.
    /// </summary>
    /// <param name="builder">The FluentAI builder.</param>
    /// <param name="config">The MCP server configuration.</param>
    /// <returns>The builder for method chaining.</returns>
    public static IFluentAiBuilder AddMcpServer(this IFluentAiBuilder builder, McpServerConfig config)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(config);

        // Register the server configuration
        builder.Services.Configure<List<McpServerConfig>>(configs =>
        {
            configs.Add(config);
        });

        return builder;
    }

    /// <summary>
    /// Registers an MCP server with stdio transport.
    /// </summary>
    /// <param name="builder">The FluentAI builder.</param>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="command">The command to execute the MCP server.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The builder for method chaining.</returns>
    public static IFluentAiBuilder AddStdioMcpServer(
        this IFluentAiBuilder builder, 
        string serverId, 
        string command,
        Action<McpServerConfig>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(serverId);
        ArgumentException.ThrowIfNullOrEmpty(command);

        var config = new McpServerConfig
        {
            ServerId = serverId,
            TransportType = McpTransportType.Stdio,
            ConnectionString = command
        };

        configureOptions?.Invoke(config);

        return builder.AddMcpServer(config);
    }

    /// <summary>
    /// Registers an MCP server with WebSocket transport.
    /// </summary>
    /// <param name="builder">The FluentAI builder.</param>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="uri">The WebSocket URI.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The builder for method chaining.</returns>
    public static IFluentAiBuilder AddWebSocketMcpServer(
        this IFluentAiBuilder builder,
        string serverId,
        string uri,
        Action<McpServerConfig>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(serverId);
        ArgumentException.ThrowIfNullOrEmpty(uri);

        var config = new McpServerConfig
        {
            ServerId = serverId,
            TransportType = McpTransportType.WebSocket,
            ConnectionString = uri
        };

        configureOptions?.Invoke(config);

        return builder.AddMcpServer(config);
    }

    /// <summary>
    /// Registers an MCP server with Server-Sent Events transport.
    /// </summary>
    /// <param name="builder">The FluentAI builder.</param>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="endpoint">The SSE endpoint URL.</param>
    /// <param name="configureOptions">Optional configuration action.</param>
    /// <returns>The builder for method chaining.</returns>
    public static IFluentAiBuilder AddSseMcpServer(
        this IFluentAiBuilder builder,
        string serverId,
        string endpoint,
        Action<McpServerConfig>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(serverId);
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        var config = new McpServerConfig
        {
            ServerId = serverId,
            TransportType = McpTransportType.SSE,
            ConnectionString = endpoint
        };

        configureOptions?.Invoke(config);

        return builder.AddMcpServer(config);
    }
}