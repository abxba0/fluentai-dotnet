namespace FluentAI.Abstractions.Models;

/// <summary>
/// Base class for provider-specific request options.
/// </summary>
public abstract record ChatRequestOptions
{
    /// <summary>
    /// Gets or sets the available tools for function calling.
    /// </summary>
    public IEnumerable<object>? Tools { get; set; }

    /// <summary>
    /// Gets or sets the tool choice behavior.
    /// </summary>
    public object? ToolChoice { get; set; }
}