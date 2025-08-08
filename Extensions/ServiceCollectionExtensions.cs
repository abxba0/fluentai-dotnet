// ServiceCollectionExtensions.cs
using Genius.Core.Providers.OpenAI;
using Genius.Core.Abstractions;
using Genius.Core.Abstractions.Exceptions;
using Genius.Core.Configuration;
using Genius.Core.Providers.Anthropic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Genius.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAiSdk(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AiSdkOptions>(configuration.GetSection("AiSdk"));

            services.AddSingleton<IChatModel>(serviceProvider =>
            {
                var sdkOptions = serviceProvider.GetRequiredService<IOptions<AiSdkOptions>>().Value;
                var providerName = sdkOptions.DefaultProvider;

                if (string.IsNullOrEmpty(providerName))
                    throw new AiSdkConfigurationException("A default provider is not specified in the 'AiSdk' configuration section.");

                return providerName.ToLowerInvariant() switch
                {
                    "openai" => serviceProvider.GetService<OpenAiChatModel>()
                                ?? throw new AiSdkConfigurationException("OpenAI is configured as default, but was not registered. Call .AddOpenAiChatModel()."),
                    "anthropic" => serviceProvider.GetService<AnthropicChatModel>()
                                   ?? throw new AiSdkConfigurationException("Anthropic is configured as default, but was not registered. Call .AddAnthropicChatModel()."),
                    _ => throw new AiSdkConfigurationException($"Default provider '{providerName}' is not supported or registered.")
                };
            });

            return services;
        }

        public static IServiceCollection AddOpenAiChatModel(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OpenAiOptions>(configuration.GetSection("OpenAI"));
            services.AddSingleton<OpenAiChatModel>();
            return services;
        }

        public static IServiceCollection AddAnthropicChatModel(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AnthropicOptions>(configuration.GetSection("Anthropic"));
            services.AddHttpClient("AnthropicClient");
            services.AddSingleton<AnthropicChatModel>();
            return services;
        }
    }
}