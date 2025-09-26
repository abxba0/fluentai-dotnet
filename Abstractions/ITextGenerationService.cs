using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the contract for text generation AI services.
    /// </summary>
    public interface ITextGenerationService : IAiService
    {
        /// <summary>
        /// Generates text based on the provided request.
        /// </summary>
        /// <param name="request">The text generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task containing the text generation response.</returns>
        Task<TextResponse> GenerateAsync(TextRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams text generation response token-by-token.
        /// </summary>
        /// <param name="request">The text generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable of generated text tokens.</returns>
        IAsyncEnumerable<string> StreamAsync(TextRequest request, CancellationToken cancellationToken = default);
    }
}