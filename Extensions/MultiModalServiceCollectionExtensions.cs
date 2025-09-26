using FluentAI.Abstractions;
using FluentAI.Abstractions.Implementations;
using FluentAI.Abstractions.Services;
using FluentAI.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAI.Extensions
{
    /// <summary>
    /// Extension methods for configuring multi-modal AI services.
    /// </summary>
    public static class MultiModalServiceCollectionExtensions
    {
        /// <summary>
        /// Adds multi-modal AI support to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMultiModalSupport(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register multi-modal configuration
            var multiModalSection = configuration.GetSection("AiSdk:MultiModal");
            services.Configure<MultiModalOptions>(multiModalSection);

            // Register core multi-modal services
            services.AddSingleton<IMultiModalProviderFactory, MultiModalProviderFactory>();
            services.AddSingleton<IModelSelector, ConfigurationBasedModelSelector>();

            // Register modality services
            services.AddScoped<ITextGenerationService, TextGenerationService>();
            services.AddScoped<IImageAnalysisService, ImageAnalysisService>();
            services.AddScoped<IImageGenerationService, ImageGenerationService>();
            services.AddScoped<IAudioTranscriptionService, AudioTranscriptionService>();
            services.AddScoped<IAudioGenerationService, AudioGenerationService>();

            return services;
        }

        /// <summary>
        /// Adds a multi-modal provider to the service collection.
        /// </summary>
        /// <typeparam name="TProvider">The provider type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="providerName">The provider name.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddMultiModalProvider<TProvider>(
            this IServiceCollection services,
            string? providerName = null)
            where TProvider : MultiModalProvider
        {
            // Register the provider with DI
            services.AddScoped<TProvider>();

            // Register with the factory using the provider name or type name
            providerName ??= typeof(TProvider).Name.Replace("MultiModalProvider", "").Replace("Provider", "");

            services.AddSingleton<Action<IMultiModalProviderFactory>>(factory =>
            {
                if (factory is MultiModalProviderFactory concreteFactory)
                {
                    concreteFactory.RegisterProvider<TProvider>(providerName);
                }
            });

            return services;
        }

        /// <summary>
        /// Configures multi-modal services with the FluentAI builder.
        /// </summary>
        /// <param name="builder">The FluentAI builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns>The FluentAI builder for chaining.</returns>
        public static IFluentAiBuilder AddMultiModalSupport(
            this IFluentAiBuilder builder,
            IConfiguration configuration)
        {
            builder.Services.AddMultiModalSupport(configuration);
            return builder;
        }

        /// <summary>
        /// Adds a multi-modal provider to the FluentAI builder.
        /// </summary>
        /// <typeparam name="TProvider">The provider type.</typeparam>
        /// <param name="builder">The FluentAI builder.</param>
        /// <param name="providerName">The provider name.</param>
        /// <returns>The FluentAI builder for chaining.</returns>
        public static IFluentAiBuilder AddMultiModalProvider<TProvider>(
            this IFluentAiBuilder builder,
            string? providerName = null)
            where TProvider : MultiModalProvider
        {
            builder.Services.AddMultiModalProvider<TProvider>(providerName);
            return builder;
        }
    }
}