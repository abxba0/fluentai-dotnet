namespace FluentAI.Abstractions.Models;

/// <summary>
/// Represents a response from an AI chat model.
/// </summary>
/// <param name="Content">The generated content from the model.</param>
/// <param name="ModelId">The identifier of the model that generated the response.</param>
/// <param name="FinishReason">The reason why the model stopped generating (e.g., completed, length limit).</param>
/// <param name="Usage">Token usage information for the request.</param>
public record ChatResponse(
    string Content,
    string ModelId,
    string FinishReason,
    TokenUsage Usage
);