namespace FluentAI.Abstractions.Models;

/// <summary>
/// Represents a chat message with a role and content.
/// </summary>
/// <param name="Role">The role of the message sender (User, Assistant, System).</param>
/// <param name="Content">The content of the message.</param>
public record ChatMessage(ChatRole Role, string Content);