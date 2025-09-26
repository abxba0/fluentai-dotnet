using System.Text.Json;

namespace FluentAI.Abstractions.MCP;

/// <summary>
/// Adapts MCP tool schemas to provider-specific formats for function calling.
/// </summary>
public interface IToolSchemaAdapter
{
    /// <summary>
    /// Gets the provider identifier this adapter supports.
    /// </summary>
    string ProviderId { get; }

    /// <summary>
    /// Determines if this adapter can handle the specified MCP tool schema.
    /// </summary>
    /// <param name="mcpSchema">The MCP tool schema to evaluate.</param>
    /// <returns>True if this adapter can handle the schema, false otherwise.</returns>
    bool CanAdapt(ToolSchema mcpSchema);

    /// <summary>
    /// Adapts an MCP tool schema to the provider's function calling format.
    /// </summary>
    /// <param name="mcpSchema">The MCP tool schema to adapt.</param>
    /// <returns>The provider-specific tool schema.</returns>
    ProviderToolSchema AdaptSchema(ToolSchema mcpSchema);

    /// <summary>
    /// Adapts a provider tool call result back to MCP format.
    /// </summary>
    /// <param name="providerResult">The provider-specific tool result.</param>
    /// <returns>The MCP tool result.</returns>
    ToolResult AdaptResult(ProviderToolResult providerResult);

    /// <summary>
    /// Adapts an MCP tool call to the provider-specific format.
    /// </summary>
    /// <param name="toolCall">The MCP tool call to adapt.</param>
    /// <returns>The provider-specific tool call.</returns>
    ProviderToolCall AdaptToolCall(ToolCall toolCall);
}

/// <summary>
/// Represents a provider-specific tool schema.
/// </summary>
public abstract class ProviderToolSchema
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
    /// Gets or sets the original MCP schema this was adapted from.
    /// </summary>
    public ToolSchema? OriginalSchema { get; set; }
}

/// <summary>
/// Represents a provider-specific tool call.
/// </summary>
public abstract class ProviderToolCall
{
    /// <summary>
    /// Gets or sets the call identifier.
    /// </summary>
    public string? CallId { get; set; }

    /// <summary>
    /// Gets or sets the tool name.
    /// </summary>
    public required string ToolName { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a provider-specific tool execution result.
/// </summary>
public abstract class ProviderToolResult
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
    /// Gets or sets the error information if execution failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// OpenAI-specific tool schema implementation.
/// </summary>
public class OpenAiFunctionSchema : ProviderToolSchema
{
    /// <summary>
    /// Gets or sets the function parameters schema.
    /// </summary>
    public JsonDocument? Parameters { get; set; }
}

/// <summary>
/// OpenAI-specific tool call implementation.
/// </summary>
public class OpenAiFunctionCall : ProviderToolCall
{
    /// <summary>
    /// Gets or sets the function arguments as JSON.
    /// </summary>
    public string? Arguments { get; set; }
}

/// <summary>
/// OpenAI-specific tool result implementation.
/// </summary>
public class OpenAiFunctionResult : ProviderToolResult
{
    /// <summary>
    /// Gets or sets the function result content.
    /// </summary>
    public string? Content { get; set; }
}