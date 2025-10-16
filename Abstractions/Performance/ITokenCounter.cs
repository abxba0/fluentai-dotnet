using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Provides token counting and context window management capabilities.
/// </summary>
public interface ITokenCounter
{
    /// <summary>
    /// Counts tokens in a text string.
    /// </summary>
    /// <param name="text">Text to count tokens for.</param>
    /// <param name="modelId">Model identifier for model-specific tokenization.</param>
    /// <returns>Estimated token count.</returns>
    int CountTokens(string text, string? modelId = null);

    /// <summary>
    /// Counts tokens in a collection of chat messages.
    /// </summary>
    /// <param name="messages">Messages to count tokens for.</param>
    /// <param name="modelId">Model identifier for model-specific tokenization.</param>
    /// <returns>Estimated token count including message formatting overhead.</returns>
    int CountMessageTokens(IEnumerable<ChatMessage> messages, string? modelId = null);

    /// <summary>
    /// Gets the context window size for a specific model.
    /// </summary>
    /// <param name="modelId">Model identifier.</param>
    /// <returns>Maximum context window size in tokens.</returns>
    int GetContextWindowSize(string modelId);

    /// <summary>
    /// Estimates the number of tokens a response will use based on max_tokens setting.
    /// </summary>
    /// <param name="maxTokens">Maximum tokens setting.</param>
    /// <param name="modelId">Model identifier.</param>
    /// <returns>Estimated response token usage.</returns>
    int EstimateResponseTokens(int maxTokens, string? modelId = null);

    /// <summary>
    /// Checks if messages fit within the model's context window.
    /// </summary>
    /// <param name="messages">Messages to check.</param>
    /// <param name="maxResponseTokens">Maximum tokens to reserve for response.</param>
    /// <param name="modelId">Model identifier.</param>
    /// <returns>True if messages fit, false otherwise.</returns>
    bool FitsInContextWindow(IEnumerable<ChatMessage> messages, int maxResponseTokens, string modelId);

    /// <summary>
    /// Calculates remaining tokens available for response.
    /// </summary>
    /// <param name="messages">Messages that will be sent.</param>
    /// <param name="modelId">Model identifier.</param>
    /// <returns>Number of tokens available for the response.</returns>
    int GetAvailableResponseTokens(IEnumerable<ChatMessage> messages, string modelId);
}

/// <summary>
/// Provides context window optimization strategies.
/// </summary>
public interface IContextWindowOptimizer
{
    /// <summary>
    /// Optimizes messages to fit within context window while preserving important content.
    /// </summary>
    /// <param name="messages">Messages to optimize.</param>
    /// <param name="modelId">Model identifier.</param>
    /// <param name="maxResponseTokens">Tokens to reserve for response.</param>
    /// <param name="strategy">Optimization strategy to use.</param>
    /// <returns>Optimized message collection that fits in context window.</returns>
    Task<IEnumerable<ChatMessage>> OptimizeForContextWindowAsync(
        IEnumerable<ChatMessage> messages,
        string modelId,
        int maxResponseTokens,
        ContextOptimizationStrategy strategy = ContextOptimizationStrategy.TruncateOldest);

    /// <summary>
    /// Summarizes older messages to reduce token count while preserving context.
    /// </summary>
    /// <param name="messages">Messages to summarize.</param>
    /// <param name="targetTokenCount">Target token count after summarization.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Messages with older content summarized.</returns>
    Task<IEnumerable<ChatMessage>> SummarizeOldMessagesAsync(
        IEnumerable<ChatMessage> messages,
        int targetTokenCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Splits a long conversation into multiple context windows.
    /// </summary>
    /// <param name="messages">Messages to split.</param>
    /// <param name="modelId">Model identifier.</param>
    /// <param name="maxResponseTokens">Tokens to reserve for response.</param>
    /// <returns>Collection of message batches that fit in context windows.</returns>
    IEnumerable<IEnumerable<ChatMessage>> SplitIntoContextWindows(
        IEnumerable<ChatMessage> messages,
        string modelId,
        int maxResponseTokens);
}

/// <summary>
/// Strategy for optimizing messages to fit context window.
/// </summary>
public enum ContextOptimizationStrategy
{
    /// <summary>
    /// Remove oldest messages first.
    /// </summary>
    TruncateOldest,

    /// <summary>
    /// Summarize older messages.
    /// </summary>
    SummarizeOlder,

    /// <summary>
    /// Keep only system and recent user messages.
    /// </summary>
    KeepSystemAndRecent,

    /// <summary>
    /// Intelligently select most important messages.
    /// </summary>
    SmartSelection,

    /// <summary>
    /// Compress message content.
    /// </summary>
    CompressContent
}

/// <summary>
/// Information about token usage and context window utilization.
/// </summary>
public class TokenUsageInfo
{
    /// <summary>
    /// Total tokens in the messages.
    /// </summary>
    public int MessageTokens { get; init; }

    /// <summary>
    /// Tokens reserved for response.
    /// </summary>
    public int ResponseTokens { get; init; }

    /// <summary>
    /// Total tokens used (messages + response).
    /// </summary>
    public int TotalTokens { get; init; }

    /// <summary>
    /// Context window size for the model.
    /// </summary>
    public int ContextWindowSize { get; init; }

    /// <summary>
    /// Percentage of context window used (0-100).
    /// </summary>
    public double UtilizationPercentage => (double)TotalTokens / ContextWindowSize * 100.0;

    /// <summary>
    /// Whether the messages fit in the context window.
    /// </summary>
    public bool FitsInWindow => TotalTokens <= ContextWindowSize;

    /// <summary>
    /// Number of tokens over the limit (if any).
    /// </summary>
    public int OverageTokens => Math.Max(0, TotalTokens - ContextWindowSize);
}

/// <summary>
/// Model-specific token limits and pricing information.
/// </summary>
public class ModelTokenInfo
{
    /// <summary>
    /// Model identifier.
    /// </summary>
    public required string ModelId { get; init; }

    /// <summary>
    /// Context window size in tokens.
    /// </summary>
    public int ContextWindow { get; init; }

    /// <summary>
    /// Maximum output tokens.
    /// </summary>
    public int? MaxOutputTokens { get; init; }

    /// <summary>
    /// Cost per 1K input tokens (if available).
    /// </summary>
    public decimal? InputCostPer1K { get; init; }

    /// <summary>
    /// Cost per 1K output tokens (if available).
    /// </summary>
    public decimal? OutputCostPer1K { get; init; }

    /// <summary>
    /// Tokenizer encoding name.
    /// </summary>
    public string? EncodingName { get; init; }
}
