namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a streaming token in RAG response generation.
/// </summary>
public class RagStreamToken
{
    /// <summary>
    /// Gets or sets the token content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token type.
    /// </summary>
    public StreamTokenType TokenType { get; set; } = StreamTokenType.Content;

    /// <summary>
    /// Gets or sets whether this is the final token in the stream.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets or sets metadata associated with this token.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Defines the type of streaming token.
/// </summary>
public enum StreamTokenType
{
    /// <summary>
    /// Regular content token.
    /// </summary>
    Content,

    /// <summary>
    /// Context information token.
    /// </summary>
    Context,

    /// <summary>
    /// Citation or source token.
    /// </summary>
    Citation,

    /// <summary>
    /// Metadata token.
    /// </summary>
    Metadata
}

/// <summary>
/// Represents the result of a document indexing operation.
/// </summary>
public class IndexingResult
{
    /// <summary>
    /// Gets or sets whether the indexing operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of documents successfully indexed.
    /// </summary>
    public int IndexedCount { get; set; }

    /// <summary>
    /// Gets or sets the number of document chunks created.
    /// </summary>
    public int ChunkCount { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during indexing.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the time taken for the indexing operation.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the indexing operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of a retrieval operation.
/// </summary>
public class RetrievalResult
{
    /// <summary>
    /// Gets or sets the retrieved document chunks.
    /// </summary>
    public IEnumerable<DocumentChunk> Chunks { get; set; } = Enumerable.Empty<DocumentChunk>();

    /// <summary>
    /// Gets or sets the query that was used for retrieval.
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the enhanced query if query enhancement was applied.
    /// </summary>
    public string? EnhancedQuery { get; set; }

    /// <summary>
    /// Gets or sets the time taken for the retrieval operation.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional metadata about the retrieval operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a citation or source reference.
/// </summary>
public class Citation
{
    /// <summary>
    /// Gets or sets the source document identifier.
    /// </summary>
    public string DocumentId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source URL if available.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the page number or section reference.
    /// </summary>
    public string? PageReference { get; set; }

    /// <summary>
    /// Gets or sets the relevance score for this citation.
    /// </summary>
    public double RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets additional citation metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents source information for a document chunk.
/// </summary>
public class SourceInfo
{
    /// <summary>
    /// Gets or sets the source document title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source URL.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the author of the source document.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the publication date.
    /// </summary>
    public DateTimeOffset? PublishedAt { get; set; }

    /// <summary>
    /// Gets or sets additional source metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Configuration options for document processing.
/// </summary>
public class ProcessingOptions
{
    /// <summary>
    /// Gets or sets whether to extract metadata from the document.
    /// Default is true.
    /// </summary>
    public bool ExtractMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to perform OCR on image-based documents.
    /// Default is false.
    /// </summary>
    public bool PerformOcr { get; set; } = false;

    /// <summary>
    /// Gets or sets the language for OCR processing.
    /// </summary>
    public string? OcrLanguage { get; set; }

    /// <summary>
    /// Gets or sets custom processing options.
    /// </summary>
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Configuration options for document chunking.
/// </summary>
public class ChunkingOptions
{
    /// <summary>
    /// Gets or sets the chunking strategy to use.
    /// </summary>
    public ChunkingStrategy Strategy { get; set; } = ChunkingStrategy.Semantic;

    /// <summary>
    /// Gets or sets the maximum size of each chunk in characters.
    /// Default is 1000.
    /// </summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the overlap between chunks in characters.
    /// Default is 200.
    /// </summary>
    public int Overlap { get; set; } = 200;

    /// <summary>
    /// Gets or sets custom chunking options.
    /// </summary>
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Defines the strategy for breaking documents into chunks.
/// </summary>
public enum ChunkingStrategy
{
    /// <summary>
    /// Fixed-size chunks with optional overlap.
    /// </summary>
    FixedSize,

    /// <summary>
    /// Semantic chunking based on content structure.
    /// </summary>
    Semantic,

    /// <summary>
    /// Sentence-based chunking.
    /// </summary>
    Sentence,

    /// <summary>
    /// Paragraph-based chunking.
    /// </summary>
    Paragraph
}

/// <summary>
/// Configuration options for embedding generation.
/// </summary>
public class EmbeddingOptions
{
    /// <summary>
    /// Gets or sets the embedding model to use.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the batch size for embedding generation.
    /// Default is 100.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets custom embedding options.
    /// </summary>
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

/// <summary>
/// Security settings for document access control.
/// </summary>
public class DocumentSecurity
{
    /// <summary>
    /// Gets or sets the access control list for this document.
    /// </summary>
    public IEnumerable<string> AllowedUsers { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the required roles for accessing this document.
    /// </summary>
    public IEnumerable<string> RequiredRoles { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets whether this document contains sensitive information.
    /// </summary>
    public bool IsSensitive { get; set; }

    /// <summary>
    /// Gets or sets additional security metadata.
    /// </summary>
    public Dictionary<string, object> SecurityMetadata { get; set; } = new();
}

/// <summary>
/// Information about a document type.
/// </summary>
public class DocumentTypeInfo
{
    /// <summary>
    /// Gets or sets the detected document type.
    /// </summary>
    public string DocumentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence level of the type detection (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets additional type information.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Information about an embedding model.
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of dimensions in the embedding vectors.
    /// </summary>
    public int Dimensions { get; set; }

    /// <summary>
    /// Gets or sets the maximum input token length.
    /// </summary>
    public int MaxInputTokens { get; set; }

    /// <summary>
    /// Gets or sets the model provider.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional model capabilities.
    /// </summary>
    public Dictionary<string, object> Capabilities { get; set; } = new();
}

/// <summary>
/// Represents the result of a processed document.
/// </summary>
public class ProcessedDocument
{
    /// <summary>
    /// Gets or sets the extracted text content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the extracted metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the detected language.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the processing timestamp.
    /// </summary>
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
}