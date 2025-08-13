using FluentAI.Abstractions.Analysis;
using Microsoft.Extensions.DependencyInjection;

namespace FluentAI.Extensions.Analysis
{
    /// <summary>
    /// Extension methods for registering runtime analysis services.
    /// </summary>
    public static class RuntimeAnalysisServiceCollectionExtensions
    {
        /// <summary>
        /// Adds runtime-aware code analysis services to the dependency injection container.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRuntimeAnalyzer(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<IRuntimeAnalyzer, DefaultRuntimeAnalyzer>();
            
            return services;
        }

        /// <summary>
        /// Adds runtime-aware code analysis services with a custom implementation.
        /// </summary>
        /// <typeparam name="T">The custom runtime analyzer implementation.</typeparam>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRuntimeAnalyzer<T>(this IServiceCollection services)
            where T : class, IRuntimeAnalyzer
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddTransient<IRuntimeAnalyzer, T>();
            
            return services;
        }

        /// <summary>
        /// Adds runtime-aware code analysis services with a factory method.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="factory">Factory method to create the runtime analyzer instance.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddRuntimeAnalyzer(
            this IServiceCollection services, 
            Func<IServiceProvider, IRuntimeAnalyzer> factory)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            services.AddTransient(factory);
            
            return services;
        }
    }
}