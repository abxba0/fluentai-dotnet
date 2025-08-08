using Genius.Core.Abstractions.Models;

namespace Genius.Core.Configuration;

public record OpenAiRequestOptions : ChatRequestOptions
{
    public float? Temperature { get; set; }
    public int? MaxTokens { get; set; }
    public float? TopP { get; set; }
}