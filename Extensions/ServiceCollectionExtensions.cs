// ServiceCollectionExtensions.cs
using FluentAI.Providers.OpenAI;
using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Configuration;
using FluentAI.Providers.Anthropic;
using FluentAI.Providers.Google;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FluentAI.Extensions
{
    /// <summary>
    /// Extension methods for configuring FluentAI services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds FluentAI core services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The FluentAI builder for chaining provider registrations.</returns>
        public static IFluentAiBuilder AddFluentAI(this IServiceCollection services)
        {
            return new FluentAiBuilder(services);
        }

        /// <summary>
        /// Adds FluentAI services to the dependency injection container using configuration-based setup.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
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
                    "google" => serviceProvider.GetService<GoogleGeminiChatModel>()
                               ?? throw new AiSdkConfigurationException("Google is configured as default, but was not registered. Call .AddGoogleGeminiChatModel()."),
                    _ => throw new AiSdkConfigurationException($"Default provider '{providerName}' is not supported or registered.")
                };
            });

            return services;
        }

        /// <summary>
        /// Adds OpenAI chat model provider to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddOpenAiChatModel(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<OpenAiOptions>(configuration.GetSection("OpenAI"));
            services.AddSingleton<OpenAiChatModel>();
            return services;
        }

        /// <summary>
        /// Adds Anthropic chat model provider to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddAnthropicChatModel(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AnthropicOptions>(configuration.GetSection("Anthropic"));
            services.AddHttpClient("AnthropicClient");
            services.AddSingleton<AnthropicChatModel>();
            return services;
        }

        /// <summary>
        /// Adds Google Gemini chat model provider to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddGoogleGeminiChatModel(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GoogleOptions>(configuration.GetSection("Google"));
            services.AddHttpClient("GoogleClient", client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
            });
            services.AddSingleton<GoogleGeminiChatModel>();
            return services;
        }
    }

    /// <summary>
    /// Builder interface for configuring FluentAI providers.
    /// </summary>
    public interface IFluentAiBuilder
    {
        /// <summary>
        /// Gets the service collection.
        /// </summary>
        IServiceCollection Services { get; }

        /// <summary>
        /// Adds OpenAI provider to the FluentAI configuration.
        /// </summary>
        /// <param name="configure">Configuration action for OpenAI options.</param>
        /// <returns>The builder for chaining.</returns>
        IFluentAiBuilder AddOpenAI(Action<OpenAiOptions> configure);

        /// <summary>
        /// Adds Anthropic provider to the FluentAI configuration.
        /// </summary>
        /// <param name="configure">Configuration action for Anthropic options.</param>
        /// <returns>The builder for chaining.</returns>
        IFluentAiBuilder AddAnthropic(Action<AnthropicOptions> configure);

        /// <summary>
        /// Adds Google Gemini provider to the FluentAI configuration.
        /// </summary>
        /// <param name="configure">Configuration action for Google options.</param>
        /// <returns>The builder for chaining.</returns>
        IFluentAiBuilder AddGoogle(Action<GoogleOptions> configure);

        /// <summary>
        /// Sets the default provider to use when multiple providers are registered.
        /// </summary>
        /// <param name="providerName">The name of the provider to use as default.</param>
        /// <returns>The builder for chaining.</returns>
        IFluentAiBuilder UseDefaultProvider(string providerName);
    }

    /// <summary>
    /// Implementation of the FluentAI builder for configuring providers.
    /// </summary>
    internal class FluentAiBuilder : IFluentAiBuilder
    {
        private readonly Dictionary<string, IChatModel> _providers = new();
        private string? _defaultProvider;

        public IServiceCollection Services { get; }

        public FluentAiBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IFluentAiBuilder AddOpenAI(Action<OpenAiOptions> configure)
        {
            var options = new OpenAiOptions();
            configure(options);

            Services.Configure<OpenAiOptions>(opt =>
            {
                opt.ApiKey = options.ApiKey;
                opt.Model = options.Model;
                opt.IsAzureOpenAI = options.IsAzureOpenAI;
                opt.Endpoint = options.Endpoint;
                opt.RequestTimeout = options.RequestTimeout;
                opt.MaxRetries = options.MaxRetries;
                opt.MaxRequestSize = options.MaxRequestSize;
                opt.MaxTokens = options.MaxTokens;
            });

            Services.AddSingleton<OpenAiChatModel>();
            
            return this;
        }

        public IFluentAiBuilder AddAnthropic(Action<AnthropicOptions> configure)
        {
            var options = new AnthropicOptions();
            configure(options);

            Services.Configure<AnthropicOptions>(opt =>
            {
                opt.ApiKey = options.ApiKey;
                opt.Model = options.Model;
                opt.RequestTimeout = options.RequestTimeout;
                opt.MaxRetries = options.MaxRetries;
                opt.MaxRequestSize = options.MaxRequestSize;
                opt.MaxTokens = options.MaxTokens;
            });

            Services.AddHttpClient("AnthropicClient", client =>
            {
                client.BaseAddress = new Uri("https://api.anthropic.com");
            });
            Services.AddSingleton<AnthropicChatModel>();
            
            return this;
        }

        public IFluentAiBuilder AddGoogle(Action<GoogleOptions> configure)
        {
            var options = new GoogleOptions();
            configure(options);

            Services.Configure<GoogleOptions>(opt =>
            {
                opt.ApiKey = options.ApiKey;
                opt.Model = options.Model;
                opt.RequestTimeout = options.RequestTimeout;
                opt.MaxRetries = options.MaxRetries;
                opt.MaxRequestSize = options.MaxRequestSize;
            });

            Services.AddHttpClient("GoogleClient", client =>
            {
                client.BaseAddress = new Uri("https://generativelanguage.googleapis.com");
            });
            Services.AddSingleton<GoogleGeminiChatModel>();
            
            return this;
        }

        public IFluentAiBuilder UseDefaultProvider(string providerName)
        {
            _defaultProvider = providerName;

            // Register the default IChatModel resolver
            Services.AddSingleton<IChatModel>(serviceProvider =>
            {
                return providerName.ToLowerInvariant() switch
                {
                    "openai" => serviceProvider.GetService<OpenAiChatModel>()
                                ?? throw new AiSdkConfigurationException("OpenAI is configured as default, but was not registered. Call .AddOpenAI()."),
                    "anthropic" => serviceProvider.GetService<AnthropicChatModel>()
                                   ?? throw new AiSdkConfigurationException("Anthropic is configured as default, but was not registered. Call .AddAnthropic()."),
                    "google" => serviceProvider.GetService<GoogleGeminiChatModel>()
                               ?? throw new AiSdkConfigurationException("Google is configured as default, but was not registered. Call .AddGoogle()."),
                    _ => throw new AiSdkConfigurationException($"Default provider '{providerName}' is not supported or registered.")
                };
            });

            return this;
        }
    }
}