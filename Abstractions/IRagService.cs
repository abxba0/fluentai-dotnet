using FluentAI.Abstractions.Models.Rag;

namespace FluentAI.Abstractions;

/// <summary>
/// Defines the core contract for a Retrieval Augmented Generation (RAG) service.
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Executes a RAG query by retrieving relevant context and generating a response.
    /// </summary>
    /// <param name="request">The RAG request containing query and options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A RAG response with generated content and retrieved context.</returns>
    Task<RagResponse> QueryAsync(RagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams a RAG response token-by-token for real-time applications.
    /// </summary>
    /// <param name="request">The RAG request containing query and options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An async enumerable of streaming tokens.</returns>
    IAsyncEnumerable<RagStreamToken> StreamQueryAsync(RagRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes a single document into the knowledge base.
    /// </summary>
    /// <param name="request">The document indexing request.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The indexing result with status and metadata.</returns>
    Task<IndexingResult> IndexDocumentAsync(DocumentIndexRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes multiple documents into the knowledge base in batch.
    /// </summary>
    /// <param name="requests">The collection of document indexing requests.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The batch indexing result with overall status.</returns>
    Task<IndexingResult> IndexDocumentsAsync(IEnumerable<DocumentIndexRequest> requests, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves relevant document context without generating a response.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="options">Optional retrieval configuration.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The retrieved context chunks.</returns>
    Task<RetrievalResult> RetrieveAsync(string query, RetrievalOptions? options = null, CancellationToken cancellationToken = default);
}