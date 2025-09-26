using FluentAI.Abstractions.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Implementations
{
    /// <summary>
    /// Default implementation of the multi-modal provider factory.
    /// </summary>
    public class MultiModalProviderFactory : IMultiModalProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MultiModalProviderFactory> _logger;
        private readonly Dictionary<string, Type> _registeredProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiModalProviderFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="logger">The logger instance.</param>
        public MultiModalProviderFactory(IServiceProvider serviceProvider, ILogger<MultiModalProviderFactory> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registeredProviders = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a provider type with the factory.
        /// </summary>
        /// <typeparam name="TProvider">The provider type.</typeparam>
        /// <param name="providerName">The provider name.</param>
        public void RegisterProvider<TProvider>(string providerName) where TProvider : MultiModalProvider
        {
            ArgumentException.ThrowIfNullOrEmpty(providerName);
            
            _registeredProviders[providerName] = typeof(TProvider);
            _logger.LogDebug("Registered multi-modal provider: {ProviderName} -> {ProviderType}", providerName, typeof(TProvider).Name);
        }

        /// <inheritdoc />
        public MultiModalProvider CreateProvider(string providerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(providerName);

            if (!_registeredProviders.TryGetValue(providerName, out var providerType))
            {
                throw new ArgumentException($"Provider '{providerName}' is not registered. Available providers: {string.Join(", ", _registeredProviders.Keys)}");
            }

            try
            {
                var provider = (MultiModalProvider)_serviceProvider.GetRequiredService(providerType);
                _logger.LogDebug("Created multi-modal provider instance: {ProviderName}", providerName);
                return provider;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create provider instance: {ProviderName}", providerName);
                throw;
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAvailableProviders()
        {
            return _registeredProviders.Keys.ToList();
        }

        /// <inheritdoc />
        public IEnumerable<string> GetProvidersForModality(ModalityType modality)
        {
            var supportingProviders = new List<string>();

            foreach (var kvp in _registeredProviders)
            {
                try
                {
                    var provider = CreateProvider(kvp.Key);
                    if (provider.SupportsModality(modality))
                    {
                        supportingProviders.Add(kvp.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to check modality support for provider: {ProviderName}", kvp.Key);
                }
            }

            return supportingProviders;
        }

        /// <inheritdoc />
        public bool SupportsModality(string providerName, ModalityType modality)
        {
            try
            {
                var provider = CreateProvider(providerName);
                return provider.SupportsModality(modality);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check modality support for provider: {ProviderName}", providerName);
                return false;
            }
        }
    }
}