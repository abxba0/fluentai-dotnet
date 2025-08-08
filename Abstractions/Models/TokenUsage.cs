namespace FluentAI.Abstractions.Models;

/// <summary>
/// Represents token usage information for a chat completion request.
/// </summary>
/// <param name="InputTokens">Number of tokens in the input prompt.</param>
/// <param name="OutputTokens">Number of tokens in the generated response.</param>
public record TokenUsage(int InputTokens, int OutputTokens)
{
    /// <summary>
    /// Gets the total number of tokens used (input + output).
    /// </summary>
    public int TotalTokens => InputTokens + OutputTokens;
}