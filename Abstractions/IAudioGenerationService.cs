using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the contract for audio generation AI services.
    /// </summary>
    public interface IAudioGenerationService : IAiService
    {
        /// <summary>
        /// Generates audio based on the provided request.
        /// </summary>
        /// <param name="request">The audio generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the audio generation response.</returns>
        Task<AudioGenerationResponse> GenerateAsync(AudioGenerationRequest request, CancellationToken cancellationToken = default);
    }
}