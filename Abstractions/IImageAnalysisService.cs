using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the contract for image analysis AI services.
    /// </summary>
    public interface IImageAnalysisService : IAiService
    {
        /// <summary>
        /// Analyzes an image based on the provided request.
        /// </summary>
        /// <param name="request">The image analysis request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image analysis response.</returns>
        Task<ImageAnalysisResponse> AnalyzeAsync(ImageAnalysisRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes an image from a URL with a prompt.
        /// </summary>
        /// <param name="imageUrl">The URL of the image to analyze.</param>
        /// <param name="prompt">The analysis prompt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image analysis response.</returns>
        Task<ImageAnalysisResponse> AnalyzeFromUrlAsync(string imageUrl, string prompt, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes an image from byte data with a prompt.
        /// </summary>
        /// <param name="imageData">The image data as bytes.</param>
        /// <param name="prompt">The analysis prompt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the image analysis response.</returns>
        Task<ImageAnalysisResponse> AnalyzeFromBytesAsync(byte[] imageData, string prompt, CancellationToken cancellationToken = default);
    }
}