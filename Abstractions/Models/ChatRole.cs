namespace FluentAI.Abstractions.Models;

/// <summary>
/// Defines the role of a chat message participant.
/// </summary>
public enum ChatRole 
{ 
    /// <summary>System message providing context or instructions.</summary>
    System, 
    /// <summary>Message from the user.</summary>
    User, 
    /// <summary>Message from the AI assistant.</summary>
    Assistant 
}