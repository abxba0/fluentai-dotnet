using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the contract for audio transcription AI services.
    /// </summary>
    public interface IAudioTranscriptionService : IAiService
    {
        /// <summary>
        /// Transcribes audio based on the provided request.
        /// </summary>
        /// <param name="request">The audio transcription request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the audio transcription response.</returns>
        Task<AudioTranscriptionResponse> TranscribeAsync(AudioTranscriptionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transcribes audio from a file path.
        /// </summary>
        /// <param name="filePath">The path to the audio file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the audio transcription response.</returns>
        Task<AudioTranscriptionResponse> TranscribeFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    }
}