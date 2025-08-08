namespace Genius.Core.Configuration;
public class AnthropicOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(2);
    public int MaxRetries { get; set; } = 2;
    public long MaxRequestSize { get; set; } = 80_000;
    public int? MaxTokens { get; set; }
}