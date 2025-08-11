using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Providers.Anthropic;
using FluentAI.Providers.Google;
using FluentAI.Providers.HuggingFace;
using FluentAI.Providers.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Factory implementation for creating chat model instances by provider name.
    /// </summary>
    internal class ChatModelFactory : IChatModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IChatModel GetModel(string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentException("Provider name cannot be null or empty.", nameof(providerName));

            return providerName.ToLowerInvariant() switch
            {
                "openai" => _serviceProvider.GetService<OpenAiChatModel>()
                            ?? throw new AiSdkConfigurationException("OpenAI provider is not registered. Call .AddOpenAiChatModel()."),
                "anthropic" => _serviceProvider.GetService<AnthropicChatModel>()
                               ?? throw new AiSdkConfigurationException("Anthropic provider is not registered. Call .AddAnthropicChatModel()."),
                "google" => _serviceProvider.GetService<GoogleGeminiChatModel>()
                           ?? throw new AiSdkConfigurationException("Google provider is not registered. Call .AddGoogleGeminiChatModel()."),
                "huggingface" => _serviceProvider.GetService<HuggingFaceChatModel>()
                                ?? throw new AiSdkConfigurationException("Hugging Face provider is not registered. Call .AddHuggingFaceChatModel()."),
                _ => throw new AiSdkConfigurationException($"Provider '{providerName}' is not supported.")
            };
        }
    }
}