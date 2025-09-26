using FluentAI.Abstractions.Models.Rag;

namespace FluentAI.Abstractions;

/// <summary>
/// Defines the contract for processing documents for RAG indexing.
/// </summary>
public interface IDocumentProcessor
{
    /// <summary>
    /// Processes a document by extracting text and metadata.
    /// </summary>
    /// <param name="input">The document input containing content and metadata.</param>
    /// <param name="options">Optional processing configuration.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The processed document with extracted text and enriched metadata.</returns>
    Task<ProcessedDocument> ProcessAsync(DocumentInput input, ProcessingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Chunks a processed document into smaller, searchable segments.
    /// </summary>
    /// <param name="document">The processed document to chunk.</param>
    /// <param name="options">Optional chunking configuration.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The document chunks ready for embedding and indexing.</returns>
    Task<IEnumerable<DocumentChunk>> ChunkDocumentAsync(ProcessedDocument document, ChunkingOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the supported document formats for processing.
    /// </summary>
    /// <returns>A collection of supported file extensions and MIME types.</returns>
    IEnumerable<string> GetSupportedFormats();

    /// <summary>
    /// Detects the document type from content or metadata.
    /// </summary>
    /// <param name="input">The document input to analyze.</param>
    /// <returns>The detected document type and confidence level.</returns>
    DocumentTypeInfo DetectDocumentType(DocumentInput input);
}