using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Services
{
    /// <summary>
    /// Base implementation for image analysis services.
    /// Actual provider implementations should override the core methods.
    /// </summary>
    public class ImageAnalysisService : IImageAnalysisService
    {
        private readonly ILogger<ImageAnalysisService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageAnalysisService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public ImageAnalysisService(ILogger<ImageAnalysisService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public virtual string ProviderName => "DefaultImageAnalysisProvider";

        /// <inheritdoc />
        public virtual string DefaultModelName => "gpt-4-vision-preview";

        /// <inheritdoc />
        public virtual async Task<ImageAnalysisResponse> AnalyzeAsync(ImageAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Analyzing image with prompt: {Prompt}", request.Prompt);

            // Base implementation - should be overridden by providers
            await Task.CompletedTask;
            throw new NotImplementedException("Image analysis must be implemented by a specific provider.");
        }

        /// <inheritdoc />
        public virtual async Task<ImageAnalysisResponse> AnalyzeFromUrlAsync(string imageUrl, string prompt, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(imageUrl);
            ArgumentException.ThrowIfNullOrEmpty(prompt);

            var request = new ImageAnalysisRequest
            {
                ImageUrl = imageUrl,
                Prompt = prompt
            };

            return await AnalyzeAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task<ImageAnalysisResponse> AnalyzeFromBytesAsync(byte[] imageData, string prompt, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(imageData);
            ArgumentException.ThrowIfNullOrEmpty(prompt);

            var request = new ImageAnalysisRequest
            {
                ImageData = imageData,
                Prompt = prompt
            };

            return await AnalyzeAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Image analysis service configuration validation completed");
            await Task.CompletedTask;
        }
    }
}