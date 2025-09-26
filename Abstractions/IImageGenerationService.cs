using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the contract for image generation AI services.
    /// </summary>
    public interface IImageGenerationService : IAiService
    {
        /// <summary>
        /// Generates images based on the provided request.
        /// </summary>
        /// <param name="request">The image generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image generation response.</returns>
        Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Edits an existing image based on the provided request.
        /// </summary>
        /// <param name="request">The image edit request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image generation response.</returns>
        Task<ImageGenerationResponse> EditAsync(ImageEditRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates variations of an existing image.
        /// </summary>
        /// <param name="request">The image variation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image generation response.</returns>
        Task<ImageGenerationResponse> CreateVariationAsync(ImageVariationRequest request, CancellationToken cancellationToken = default);
    }
}