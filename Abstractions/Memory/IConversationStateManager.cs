using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Memory;

/// <summary>
/// Manages conversation state across multiple interactions.
/// </summary>
public interface IConversationStateManager
{
    /// <summary>
    /// Creates a new conversation.
    /// </summary>
    /// <param name="conversationId">Optional conversation identifier. If not provided, one will be generated.</param>
    /// <param name="metadata">Optional metadata for the conversation.</param>
    /// <returns>The created conversation state.</returns>
    Task<ConversationState> CreateConversationAsync(string? conversationId = null, IDictionary<string, object>? metadata = null);

    /// <summary>
    /// Gets the current state of a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <returns>Current conversation state or null if not found.</returns>
    Task<ConversationState?> GetConversationAsync(string conversationId);

    /// <summary>
    /// Adds a message to a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="message">Message to add.</param>
    Task AddMessageAsync(string conversationId, ChatMessage message);

    /// <summary>
    /// Gets all messages in a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="limit">Maximum number of messages to retrieve.</param>
    /// <param name="offset">Number of messages to skip.</param>
    /// <returns>Collection of messages.</returns>
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(string conversationId, int? limit = null, int offset = 0);

    /// <summary>
    /// Updates conversation metadata.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="metadata">Metadata to update.</param>
    Task UpdateMetadataAsync(string conversationId, IDictionary<string, object> metadata);

    /// <summary>
    /// Deletes a conversation and all its messages.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    Task DeleteConversationAsync(string conversationId);

    /// <summary>
    /// Gets a summary of recent conversations.
    /// </summary>
    /// <param name="limit">Maximum number of conversations to retrieve.</param>
    /// <param name="includeArchived">Whether to include archived conversations.</param>
    /// <returns>Collection of conversation summaries.</returns>
    Task<IReadOnlyList<ConversationSummary>> GetRecentConversationsAsync(int limit = 10, bool includeArchived = false);

    /// <summary>
    /// Archives a conversation.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    Task ArchiveConversationAsync(string conversationId);

    /// <summary>
    /// Clears old messages from a conversation to manage memory.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="keepLastN">Number of recent messages to keep.</param>
    Task TrimConversationAsync(string conversationId, int keepLastN);
}

/// <summary>
/// Represents the complete state of a conversation.
/// </summary>
public class ConversationState
{
    /// <summary>
    /// Unique conversation identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// When the conversation was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the conversation was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// All messages in the conversation.
    /// </summary>
    public List<ChatMessage> Messages { get; init; } = new();

    /// <summary>
    /// Custom metadata associated with the conversation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; } = new();

    /// <summary>
    /// Whether the conversation is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Total number of tokens used in the conversation.
    /// </summary>
    public int TotalTokens { get; set; }

    /// <summary>
    /// User identifier associated with the conversation.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Conversation title or summary.
    /// </summary>
    public string? Title { get; set; }
}

/// <summary>
/// Summary information about a conversation.
/// </summary>
public class ConversationSummary
{
    /// <summary>
    /// Conversation identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Conversation title.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Number of messages in the conversation.
    /// </summary>
    public int MessageCount { get; init; }

    /// <summary>
    /// When the conversation was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the conversation was last updated.
    /// </summary>
    public DateTime LastUpdatedAt { get; init; }

    /// <summary>
    /// Whether the conversation is archived.
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Preview of the last message.
    /// </summary>
    public string? LastMessagePreview { get; init; }
}
