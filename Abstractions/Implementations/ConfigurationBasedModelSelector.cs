using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentAI.Abstractions.Implementations
{
    /// <summary>
    /// Model selector that uses configuration to determine the best model for each modality.
    /// </summary>
    public class ConfigurationBasedModelSelector : IModelSelector
    {
        private readonly MultiModalOptions _options;
        private readonly IMultiModalProviderFactory _providerFactory;
        private readonly ILogger<ConfigurationBasedModelSelector> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationBasedModelSelector"/> class.
        /// </summary>
        /// <param name="options">The multi-modal options.</param>
        /// <param name="providerFactory">The provider factory.</param>
        /// <param name="logger">The logger instance.</param>
        public ConfigurationBasedModelSelector(
            IOptions<MultiModalOptions> options,
            IMultiModalProviderFactory providerFactory,
            ILogger<ConfigurationBasedModelSelector> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _providerFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public ModelSelectionResult SelectModel(ModalityType modality, string? strategy = null)
        {
            strategy ??= _options.DefaultStrategy;

            _logger.LogDebug("Selecting model for modality: {Modality} with strategy: {Strategy}", modality, strategy);

            var modalityConfig = GetModalityConfiguration(modality);
            
            if (modalityConfig?.Primary != null)
            {
                var primaryResult = CreateSelectionResult(modalityConfig.Primary, modality, false);
                if (IsProviderAvailable(primaryResult.ProviderName, modality))
                {
                    _logger.LogDebug("Selected primary model: {Provider}/{Model} for {Modality}", 
                        primaryResult.ProviderName, primaryResult.ModelName, modality);
                    return primaryResult;
                }
            }

            // Try fallback if primary is not available
            if (modalityConfig?.Fallback != null)
            {
                var fallbackResult = CreateSelectionResult(modalityConfig.Fallback, modality, true);
                if (IsProviderAvailable(fallbackResult.ProviderName, modality))
                {
                    _logger.LogDebug("Selected fallback model: {Provider}/{Model} for {Modality}", 
                        fallbackResult.ProviderName, fallbackResult.ModelName, modality);
                    return fallbackResult;
                }
            }

            // Last resort: try to find any available provider for this modality
            var availableProviders = _providerFactory.GetProvidersForModality(modality);
            var firstAvailable = availableProviders.FirstOrDefault();
            
            if (firstAvailable != null)
            {
                var provider = _providerFactory.CreateProvider(firstAvailable);
                var defaultModel = provider.GetDefaultModel(modality);
                
                if (!string.IsNullOrEmpty(defaultModel))
                {
                    _logger.LogWarning("Using default provider configuration for {Modality}: {Provider}/{Model}", 
                        modality, firstAvailable, defaultModel);
                    
                    return new ModelSelectionResult
                    {
                        ProviderName = firstAvailable,
                        ModelName = defaultModel,
                        Modality = modality,
                        IsFallback = true,
                        Configuration = new ModelConfiguration
                        {
                            Provider = firstAvailable,
                            ModelName = defaultModel
                        }
                    };
                }
            }

            throw new InvalidOperationException($"No available provider found for modality: {modality}");
        }

        /// <inheritdoc />
        public ModelSelectionResult? GetFallbackModel(ModalityType modality)
        {
            var modalityConfig = GetModalityConfiguration(modality);
            
            if (modalityConfig?.Fallback != null)
            {
                return CreateSelectionResult(modalityConfig.Fallback, modality, true);
            }

            return null;
        }

        /// <inheritdoc />
        public ConfigurationValidationResult ValidateConfiguration(IEnumerable<ModalityType> requiredModalities)
        {
            var result = new ConfigurationValidationResult { IsValid = true };

            foreach (var modality in requiredModalities)
            {
                try
                {
                    var selection = SelectModel(modality);
                    _logger.LogDebug("Configuration validation passed for {Modality}: {Provider}/{Model}", 
                        modality, selection.ProviderName, selection.ModelName);
                }
                catch (Exception ex)
                {
                    result.IsValid = false;
                    result.Errors.Add($"No valid configuration found for modality {modality}: {ex.Message}");
                }
            }

            // Check for warnings
            if (_options.Models.TextGeneration?.Primary == null)
            {
                result.Warnings.Add("No primary text generation model configured");
            }

            return result;
        }

        /// <summary>
        /// Gets the modality configuration from options.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <returns>The modality configuration, or null if not found.</returns>
        private ModalityModelOptions? GetModalityConfiguration(ModalityType modality)
        {
            return modality switch
            {
                ModalityType.TextGeneration => _options.Models.TextGeneration,
                ModalityType.ImageAnalysis => _options.Models.ImageAnalysis,
                ModalityType.ImageGeneration => _options.Models.ImageGeneration,
                ModalityType.AudioTranscription => _options.Models.AudioTranscription,
                ModalityType.AudioGeneration => _options.Models.AudioGeneration,
                _ => null
            };
        }

        /// <summary>
        /// Creates a model selection result from a model configuration.
        /// </summary>
        /// <param name="config">The model configuration.</param>
        /// <param name="modality">The modality type.</param>
        /// <param name="isFallback">Whether this is a fallback selection.</param>
        /// <returns>The model selection result.</returns>
        private static ModelSelectionResult CreateSelectionResult(ModelConfiguration config, ModalityType modality, bool isFallback)
        {
            return new ModelSelectionResult
            {
                ProviderName = config.Provider,
                ModelName = config.ModelName,
                Configuration = config,
                Modality = modality,
                IsFallback = isFallback
            };
        }

        /// <summary>
        /// Checks if a provider is available and supports the modality.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="modality">The modality type.</param>
        /// <returns>True if the provider is available; otherwise, false.</returns>
        private bool IsProviderAvailable(string providerName, ModalityType modality)
        {
            try
            {
                return _providerFactory.SupportsModality(providerName, modality);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider {ProviderName} is not available for {Modality}", providerName, modality);
                return false;
            }
        }
    }
}