using FluentAI.Abstractions.Models;
using FluentAI.Configuration;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Interface for selecting models and providers based on configuration and strategies.
    /// </summary>
    public interface IModelSelector
    {
        /// <summary>
        /// Selects the best provider and model for a specific modality based on the configured strategy.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <param name="strategy">The selection strategy (Performance, CostOptimized, Balanced).</param>
        /// <returns>A model selection result containing provider and model information.</returns>
        ModelSelectionResult SelectModel(ModalityType modality, string? strategy = null);

        /// <summary>
        /// Gets the fallback model configuration for a specific modality.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <returns>The fallback model selection result, or null if no fallback is configured.</returns>
        ModelSelectionResult? GetFallbackModel(ModalityType modality);

        /// <summary>
        /// Validates that the required models are configured for the specified modalities.
        /// </summary>
        /// <param name="requiredModalities">The modalities that must be configured.</param>
        /// <returns>A validation result with any configuration issues.</returns>
        ConfigurationValidationResult ValidateConfiguration(IEnumerable<ModalityType> requiredModalities);
    }

    /// <summary>
    /// Result of model selection containing provider and model information.
    /// </summary>
    public class ModelSelectionResult
    {
        /// <summary>
        /// Gets or sets the selected provider name.
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the selected model name.
        /// </summary>
        public string ModelName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model configuration.
        /// </summary>
        public ModelConfiguration? Configuration { get; set; }

        /// <summary>
        /// Gets or sets the modality this selection is for.
        /// </summary>
        public ModalityType Modality { get; set; }

        /// <summary>
        /// Gets or sets whether this is a fallback selection.
        /// </summary>
        public bool IsFallback { get; set; }
    }

    /// <summary>
    /// Result of configuration validation.
    /// </summary>
    public class ConfigurationValidationResult
    {
        /// <summary>
        /// Gets or sets whether the configuration is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation errors.
        /// </summary>
        public IList<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the validation warnings.
        /// </summary>
        public IList<string> Warnings { get; set; } = new List<string>();
    }
}