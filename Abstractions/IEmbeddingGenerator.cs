using FluentAI.Abstractions.Models.Rag;

namespace FluentAI.Abstractions;

/// <summary>
/// Defines the contract for generating text embeddings.
/// </summary>
public interface IEmbeddingGenerator
{
    /// <summary>
    /// Generates embeddings for multiple texts in batch.
    /// </summary>
    /// <param name="texts">The texts to generate embeddings for.</param>
    /// <param name="options">Optional embedding generation options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The embedding results with vectors and metadata.</returns>
    Task<EmbeddingResult> GenerateEmbeddingsAsync(IEnumerable<string> texts, EmbeddingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an embedding for a single text.
    /// </summary>
    /// <param name="text">The text to generate an embedding for.</param>
    /// <param name="options">Optional embedding generation options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The embedding result with vector and metadata.</returns>
    Task<EmbeddingResult> GenerateEmbeddingAsync(string text, EmbeddingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about the embedding model being used.
    /// </summary>
    /// <returns>Information about the embedding model including dimensions and capabilities.</returns>
    ModelInfo GetModelInfo();
}