using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Services
{
    /// <summary>
    /// Base implementation for audio transcription services.
    /// Actual provider implementations should override the core methods.
    /// </summary>
    public class AudioTranscriptionService : IAudioTranscriptionService
    {
        private readonly ILogger<AudioTranscriptionService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AudioTranscriptionService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        public AudioTranscriptionService(ILogger<AudioTranscriptionService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public virtual string ProviderName => "DefaultAudioTranscriptionProvider";

        /// <inheritdoc />
        public virtual string DefaultModelName => "whisper-1";

        /// <inheritdoc />
        public virtual async Task<AudioTranscriptionResponse> TranscribeAsync(AudioTranscriptionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            _logger.LogDebug("Transcribing audio data");

            // Base implementation - should be overridden by providers
            await Task.CompletedTask;
            throw new NotImplementedException("Audio transcription must be implemented by a specific provider.");
        }

        /// <inheritdoc />
        public virtual async Task<AudioTranscriptionResponse> TranscribeFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrEmpty(filePath);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Audio file not found: {filePath}");
            }

            _logger.LogDebug("Transcribing audio file: {FilePath}", filePath);

            // Read the file and create a request
            var audioData = await File.ReadAllBytesAsync(filePath, cancellationToken);
            var request = new AudioTranscriptionRequest
            {
                AudioData = audioData,
                FilePath = filePath
            };

            return await TranscribeAsync(request, cancellationToken);
        }

        /// <inheritdoc />
        public virtual async Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Audio transcription service configuration validation completed");
            await Task.CompletedTask;
        }
    }
}