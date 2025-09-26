using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Services
{
    /// <summary>
    /// Base implementation for audio generation services.
    /// Actual provider implementations should override the core methods.
    /// </summary>
    public class AudioGenerationService : IAudioGenerationService
    {
        private readonly ILogger<AudioGenerationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioGenerationService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public AudioGenerationService(ILogger<AudioGenerationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public virtual string ProviderName => "DefaultAudioGenerationProvider";

        /// <inheritdoc />
        public virtual string DefaultModelName => "tts-1";

        /// <inheritdoc />
        public virtual async Task<AudioGenerationResponse> GenerateAsync(AudioGenerationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Generating audio for text: {Text}", request.Text);

            // Base implementation - should be overridden by providers
            await Task.CompletedTask;
            throw new NotImplementedException("Audio generation must be implemented by a specific provider.");
        }

        /// <inheritdoc />
        public virtual async Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Audio generation service configuration validation completed");
            await Task.CompletedTask;
        }
    }
}