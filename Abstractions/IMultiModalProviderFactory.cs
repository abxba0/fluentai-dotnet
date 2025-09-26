using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Factory interface for creating multi-modal providers.
    /// </summary>
    public interface IMultiModalProviderFactory
    {
        /// <summary>
        /// Creates a provider instance by name.
        /// </summary>
        /// <param name="providerName">The name of the provider.</param>
        /// <returns>The multi-modal provider instance.</returns>
        MultiModalProvider CreateProvider(string providerName);

        /// <summary>
        /// Gets all available provider names.
        /// </summary>
        /// <returns>A collection of provider names.</returns>
        IEnumerable<string> GetAvailableProviders();

        /// <summary>
        /// Gets providers that support a specific modality.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <returns>A collection of provider names that support the modality.</returns>
        IEnumerable<string> GetProvidersForModality(ModalityType modality);

        /// <summary>
        /// Checks if a provider supports a specific modality.
        /// </summary>
        /// <param name="providerName">The provider name.</param>
        /// <param name="modality">The modality type.</param>
        /// <returns>True if the provider supports the modality; otherwise, false.</returns>
        bool SupportsModality(string providerName, ModalityType modality);
    }
}