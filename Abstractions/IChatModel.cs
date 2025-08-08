using FluentAI.Abstractions.Models;
using System.Runtime.CompilerServices;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Defines the core contract for a generative AI chat model.
    /// </summary>
    public interface IChatModel
    {
        /// <summary>
        /// Gets a complete response from the model for a given series of messages.
        /// </summary>
        Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Streams a response from the model token-by-token.
        /// </summary>
        IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default);
    }
}