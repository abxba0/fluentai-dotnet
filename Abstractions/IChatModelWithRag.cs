using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Models.Rag;

namespace FluentAI.Abstractions;

/// <summary>
/// Extends the core chat model interface with RAG (Retrieval Augmented Generation) capabilities.
/// </summary>
public interface IChatModelWithRag : IChatModel
{
    /// <summary>
    /// Gets a complete response from the model with RAG context injection.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="ragOptions">Optional RAG context configuration.</param>
    /// <param name="chatOptions">Optional chat request configuration.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A chat response enhanced with retrieved context.</returns>
    Task<ChatResponse> GetResponseWithContextAsync(
        IEnumerable<ChatMessage> messages, 
        RagContextOptions? ragOptions = null,
        ChatRequestOptions? chatOptions = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Streams a response from the model with RAG context injection.
    /// </summary>
    /// <param name="messages">The conversation messages.</param>
    /// <param name="ragOptions">Optional RAG context configuration.</param>
    /// <param name="chatOptions">Optional chat request configuration.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of streaming tokens with context metadata.</returns>
    IAsyncEnumerable<string> StreamResponseWithContextAsync(
        IEnumerable<ChatMessage> messages,
        RagContextOptions? ragOptions = null,
        ChatRequestOptions? chatOptions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the RAG service associated with this chat model.
    /// </summary>
    IRagService? RagService { get; }
}