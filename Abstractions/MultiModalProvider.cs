using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Abstract base class for multi-modal AI providers.
    /// </summary>
    public abstract class MultiModalProvider
    {
        /// <summary>
        /// Gets the name of this provider.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the modalities supported by this provider.
        /// </summary>
        public abstract IEnumerable<ModalitySupport> SupportedModalities { get; }

        /// <summary>
        /// Determines if this provider supports the specified modality.
        /// </summary>
        /// <param name="modality">The modality to check.</param>
        /// <returns>True if the modality is supported; otherwise, false.</returns>
        public virtual bool SupportsModality(ModalityType modality) =>
            SupportedModalities.Any(s => s.Modality == modality);

        /// <summary>
        /// Gets the supported models for a specific modality.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <returns>A list of supported model names.</returns>
        public virtual IEnumerable<string> GetSupportedModels(ModalityType modality) =>
            SupportedModalities
                .Where(s => s.Modality == modality)
                .SelectMany(s => s.SupportedModels);

        /// <summary>
        /// Executes a multi-modal request.
        /// </summary>
        /// <typeparam name="TRequest">The request type.</typeparam>
        /// <typeparam name="TResponse">The response type.</typeparam>
        /// <param name="request">The request to execute.</param>
        /// <param name="modality">The modality type.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the response.</returns>
        public abstract Task<TResponse> ExecuteAsync<TRequest, TResponse>(
            TRequest request,
            ModalityType modality,
            CancellationToken cancellationToken = default)
            where TRequest : MultiModalRequest
            where TResponse : MultiModalResponse;

        /// <summary>
        /// Validates the provider configuration.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the validation operation.</returns>
        public virtual Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the default model name for a specific modality.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <returns>The default model name, or null if not supported.</returns>
        public virtual string? GetDefaultModel(ModalityType modality) =>
            SupportedModalities
                .FirstOrDefault(s => s.Modality == modality)?
                .SupportedModels
                .FirstOrDefault();
    }
}