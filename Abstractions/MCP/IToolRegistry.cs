using System.Text.Json;

namespace FluentAI.Abstractions.MCP;

/// <summary>
/// Manages tool registration and discovery for MCP servers.
/// </summary>
public interface IToolRegistry
{
    /// <summary>
    /// Registers tools from an MCP server.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="tools">The tools to register.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task RegisterToolsAsync(string serverId, IEnumerable<ToolSchema> tools, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all registered tools.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>All registered tools.</returns>
    Task<IReadOnlyList<ToolSchema>> GetToolsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tools registered from a specific server.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>Tools from the specified server.</returns>
    Task<IReadOnlyList<ToolSchema>> GetToolsByServerAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific tool by name.
    /// </summary>
    /// <param name="toolName">The tool name.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The tool schema if found, null otherwise.</returns>
    Task<ToolSchema?> GetToolAsync(string toolName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates and removes tools from a specific server.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task InvalidateServerToolsAsync(string serverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a specific tool.
    /// </summary>
    /// <param name="toolName">The tool name.</param>
    /// <param name="newVersion">The new version of the tool.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    Task InvalidateToolAsync(string toolName, string newVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when tools are registered or updated.
    /// </summary>
    event EventHandler<ToolRegistryChangedEventArgs>? ToolsChanged;
}

/// <summary>
/// Represents a tool schema from an MCP server.
/// </summary>
public class ToolSchema
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
    /// Gets or sets the input schema for the tool.
    /// </summary>
    public JsonDocument? InputSchema { get; set; }

    /// <summary>
    /// Gets or sets the server that provides this tool.
    /// </summary>
    public required string ServerId { get; set; }

    /// <summary>
    /// Gets or sets the tool version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets additional metadata for the tool.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Event arguments for tool registry changes.
/// </summary>
public class ToolRegistryChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of change that occurred.
    /// </summary>
    public ToolRegistryChangeType ChangeType { get; init; }

    /// <summary>
    /// Gets the server identifier associated with the change.
    /// </summary>
    public required string ServerId { get; init; }

    /// <summary>
    /// Gets the tools that were affected by the change.
    /// </summary>
    public IReadOnlyList<ToolSchema> AffectedTools { get; init; } = Array.Empty<ToolSchema>();
}

/// <summary>
/// Types of changes that can occur in the tool registry.
/// </summary>
public enum ToolRegistryChangeType
{
    /// <summary>
    /// Tools were registered or updated.
    /// </summary>
    Registered,

    /// <summary>
    /// Tools were removed or invalidated.
    /// </summary>
    Invalidated,

    /// <summary>
    /// A specific tool was updated.
    /// </summary>
    Updated
}