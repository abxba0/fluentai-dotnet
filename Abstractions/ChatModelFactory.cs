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
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Provider name cannot be null, empty, or whitespace.", nameof(providerName));

            var normalizedProviderName = providerName.Trim().ToLowerInvariant();

            return normalizedProviderName switch
            {
                "openai" => _serviceProvider.GetService<OpenAiChatModel>()
                            ?? throw new AiSdkConfigurationException($"OpenAI provider is not registered. Please ensure you have called services.AddOpenAiChatModel(configuration) and provided valid OpenAI configuration including API key via environment variable 'OPENAI_API_KEY' or configuration."),
                "anthropic" => _serviceProvider.GetService<AnthropicChatModel>()
                               ?? throw new AiSdkConfigurationException($"Anthropic provider is not registered. Please ensure you have called services.AddAnthropicChatModel(configuration) and provided valid Anthropic configuration including API key via environment variable 'ANTHROPIC_API_KEY' or configuration."),
                "google" => _serviceProvider.GetService<GoogleGeminiChatModel>()
                           ?? throw new AiSdkConfigurationException($"Google provider is not registered. Please ensure you have called services.AddGoogleGeminiChatModel(configuration) and provided valid Google configuration including API key via environment variable 'GOOGLE_API_KEY' or configuration."),
                "huggingface" => _serviceProvider.GetService<HuggingFaceChatModel>()
                                ?? throw new AiSdkConfigurationException($"Hugging Face provider is not registered. Please ensure you have called services.AddHuggingFaceChatModel(configuration) and provided valid HuggingFace configuration including API key via environment variable 'HUGGINGFACE_API_KEY' or configuration."),
                _ => throw new AiSdkConfigurationException($"Provider '{providerName}' is not supported. Supported providers are: 'OpenAI', 'Anthropic', 'Google', 'HuggingFace'.")
            };
        }
    }
}