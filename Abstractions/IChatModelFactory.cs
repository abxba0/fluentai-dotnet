using FluentAI.Abstractions;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Factory interface for creating chat model instances by provider name.
    /// </summary>
    public interface IChatModelFactory
    {
        /// <summary>
        /// Gets a chat model instance for the specified provider name.
        /// </summary>
        /// <param name="providerName">The name of the provider (e.g., "openai", "anthropic").</param>
        /// <returns>A chat model instance for the specified provider.</returns>
        IChatModel GetModel(string providerName);
    }
}