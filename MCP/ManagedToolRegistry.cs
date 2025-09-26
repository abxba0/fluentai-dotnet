using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FluentAI.MCP;

/// <summary>
/// Thread-safe implementation of the tool registry with schema caching and versioning.
/// </summary>
public class ManagedToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, ToolSchema> _tools = new();
    private readonly ConcurrentDictionary<string, string> _schemaVersions = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _serverTools = new();
    private readonly ILogger<ManagedToolRegistry> _logger;
    private readonly object _eventLock = new();

    /// <summary>
    /// Initializes a new instance of the ManagedToolRegistry class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public ManagedToolRegistry(ILogger<ManagedToolRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public event EventHandler<ToolRegistryChangedEventArgs>? ToolsChanged;

    /// <inheritdoc />
    public Task RegisterToolsAsync(string serverId, IEnumerable<ToolSchema> tools, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverId);
        ArgumentNullException.ThrowIfNull(tools);

        var toolList = tools.ToList();
        if (!toolList.Any())
        {
            _logger.LogDebug("No tools to register for server {ServerId}", serverId);
            return Task.CompletedTask;
        }

        _logger.LogDebug("Registering {ToolCount} tools for server {ServerId}", toolList.Count, serverId);

        var registeredTools = new List<ToolSchema>();
        var serverToolNames = _serverTools.GetOrAdd(serverId, _ => new HashSet<string>());

        foreach (var tool in toolList)
        {
            // Ensure the tool is associated with the correct server
            tool.ServerId = serverId;
            
            // Register or update the tool
            var key = $"{serverId}:{tool.Name}";
            _tools.AddOrUpdate(key, tool, (_, existing) =>
            {
                _logger.LogDebug("Updating tool {ToolName} for server {ServerId}", tool.Name, serverId);
                return tool;
            });

            // Track version if provided
            if (!string.IsNullOrEmpty(tool.Version))
            {
                _schemaVersions.AddOrUpdate(key, tool.Version, (_, _) => tool.Version);
            }

            // Add to server's tool list
            lock (serverToolNames)
            {
                serverToolNames.Add(tool.Name);
            }

            registeredTools.Add(tool);
        }

        _logger.LogInformation("Registered {ToolCount} tools for server {ServerId}", registeredTools.Count, serverId);

        // Notify about the registration
        NotifyToolsChanged(ToolRegistryChangeType.Registered, serverId, registeredTools);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ToolSchema>> GetToolsAsync(CancellationToken cancellationToken = default)
    {
        var allTools = _tools.Values.ToList();
        _logger.LogDebug("Retrieved {ToolCount} total tools from registry", allTools.Count);
        return Task.FromResult<IReadOnlyList<ToolSchema>>(allTools);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ToolSchema>> GetToolsByServerAsync(string serverId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverId);

        var serverTools = _tools.Values
            .Where(tool => tool.ServerId == serverId)
            .ToList();

        _logger.LogDebug("Retrieved {ToolCount} tools for server {ServerId}", serverTools.Count, serverId);
        return Task.FromResult<IReadOnlyList<ToolSchema>>(serverTools);
    }

    /// <inheritdoc />
    public Task<ToolSchema?> GetToolAsync(string toolName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toolName);

        // Find tool by name across all servers (first match)
        var tool = _tools.Values.FirstOrDefault(t => t.Name == toolName);
        
        if (tool != null)
        {
            _logger.LogDebug("Found tool {ToolName} from server {ServerId}", toolName, tool.ServerId);
        }
        else
        {
            _logger.LogDebug("Tool {ToolName} not found in registry", toolName);
        }

        return Task.FromResult(tool);
    }

    /// <inheritdoc />
    public Task InvalidateServerToolsAsync(string serverId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(serverId);

        _logger.LogDebug("Invalidating all tools for server {ServerId}", serverId);

        var invalidatedTools = new List<ToolSchema>();

        // Remove all tools for this server
        var keysToRemove = _tools.Keys.Where(key => key.StartsWith($"{serverId}:")).ToList();
        foreach (var key in keysToRemove)
        {
            if (_tools.TryRemove(key, out var tool))
            {
                invalidatedTools.Add(tool);
                _schemaVersions.TryRemove(key, out _);
            }
        }

        // Clear server's tool list
        _serverTools.TryRemove(serverId, out _);

        _logger.LogInformation("Invalidated {ToolCount} tools for server {ServerId}", invalidatedTools.Count, serverId);

        if (invalidatedTools.Any())
        {
            NotifyToolsChanged(ToolRegistryChangeType.Invalidated, serverId, invalidatedTools);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task InvalidateToolAsync(string toolName, string newVersion, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(toolName);
        ArgumentException.ThrowIfNullOrEmpty(newVersion);

        _logger.LogDebug("Checking tool {ToolName} for version update to {NewVersion}", toolName, newVersion);

        var updatedTools = new List<ToolSchema>();

        // Find all instances of this tool across servers
        var toolKeys = _tools.Keys.Where(key => key.EndsWith($":{toolName}")).ToList();

        foreach (var key in toolKeys)
        {
            if (_schemaVersions.TryGetValue(key, out var currentVersion) && currentVersion != newVersion)
            {
                // Version has changed - invalidate the cached tool
                if (_tools.TryRemove(key, out var tool))
                {
                    _schemaVersions.TryUpdate(key, newVersion, currentVersion);
                    updatedTools.Add(tool);
                    
                    _logger.LogDebug("Invalidated tool {ToolName} for server {ServerId} due to version change: {OldVersion} -> {NewVersion}",
                        toolName, tool.ServerId, currentVersion, newVersion);
                }
            }
        }

        if (updatedTools.Any())
        {
            _logger.LogInformation("Invalidated {ToolCount} instances of tool {ToolName} due to version update", 
                updatedTools.Count, toolName);

            // Group by server and notify
            var groupedByServer = updatedTools.GroupBy(t => t.ServerId);
            foreach (var group in groupedByServer)
            {
                NotifyToolsChanged(ToolRegistryChangeType.Updated, group.Key, group.ToList());
            }
        }

        return Task.CompletedTask;
    }

    private void NotifyToolsChanged(ToolRegistryChangeType changeType, string serverId, IReadOnlyList<ToolSchema> affectedTools)
    {
        try
        {
            lock (_eventLock)
            {
                ToolsChanged?.Invoke(this, new ToolRegistryChangedEventArgs
                {
                    ChangeType = changeType,
                    ServerId = serverId,
                    AffectedTools = affectedTools
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error notifying tool registry change for server {ServerId}", serverId);
        }
    }
}