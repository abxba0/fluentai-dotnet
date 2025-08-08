using Genius.Core.Abstractions.Models;

namespace Genius.Core.Configuration;

public record AnthropicRequestOptions : ChatRequestOptions
{
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? SystemPrompt { get; set; }
}