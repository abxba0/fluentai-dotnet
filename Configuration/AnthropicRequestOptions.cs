using Genius.Core.Abstractions.Models;

public class AnthropicRequestOptions : ChatRequestOptions
{
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public string? SystemPrompt { get; set; }
}