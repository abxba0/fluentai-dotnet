namespace FluentAI.Abstractions.Models;

/// <summary>
/// Represents a chat message with a role and content.
/// </summary>
/// <param name="Role">The role of the message sender (User, Assistant, System).</param>
/// <param name="Content">The content of the message.</param>
public record ChatMessage(ChatRole Role, string Content)
{
    /// <summary>
    /// Gets or sets tool calls associated with this message.
    /// </summary>
    public IEnumerable<object>? ToolCalls { get; set; }

    /// <summary>
    /// Gets or sets the tool call identifier this message is responding to.
    /// </summary>
    public string? ToolCallId { get; set; }
}