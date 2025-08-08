using Azure.AI.OpenAI;

namespace Genius.Core.Abstractions.Models;
public record ChatMessage(ChatRole Role, string Content);