namespace Genius.Core.Abstractions.Models;
public record ChatResponse(
    string Content,
    string ModelId,
    string FinishReason,
    TokenUsage Usage
);