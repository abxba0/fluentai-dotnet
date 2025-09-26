using FluentAI.Abstractions;
using FluentAI.Abstractions.Models.Rag;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace FluentAI.Services.Rag;

/// <summary>
/// Default implementation of the document processor.
/// </summary>
public class DefaultDocumentProcessor : IDocumentProcessor
{
    private readonly ILogger<DefaultDocumentProcessor> _logger;
    
    private static readonly string[] SupportedFormats = { "txt", "text", "plain", "md", "markdown", "html" };

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultDocumentProcessor"/> class.
    /// </summary>
    public DefaultDocumentProcessor(ILogger<DefaultDocumentProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ProcessedDocument> ProcessAsync(
        DocumentInput input, 
        ProcessingOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Processing document: {FileName}", input.FileName ?? "Unknown");

        options ??= new ProcessingOptions();

        try
        {
            string content;
            
            // Extract text content based on input type
            if (!string.IsNullOrEmpty(input.Content))
            {
                content = input.Content;
            }
            else if (input.BinaryContent != null)
            {
                content = await ExtractTextFromBinary(input.BinaryContent, input.MimeType, cancellationToken);
            }
            else
            {
                throw new ArgumentException("No content provided in document input");
            }

            // Clean and normalize content
            content = CleanText(content);

            // Extract metadata if requested
            var metadata = new Dictionary<string, object>();
            if (options.ExtractMetadata)
            {
                metadata = await ExtractMetadata(content, input, cancellationToken);
            }

            // Copy input metadata
            foreach (var kvp in input.Metadata)
            {
                metadata[kvp.Key] = kvp.Value;
            }

            var processedDocument = new ProcessedDocument
            {
                Content = content,
                Title = input.Title ?? ExtractTitleFromContent(content),
                Metadata = metadata,
                Language = input.Language ?? DetectLanguage(content)
            };

            _logger.LogDebug("Document processed successfully: {Length} characters, Title: {Title}", 
                content.Length, processedDocument.Title);

            return processedDocument;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing document: {FileName}", input.FileName);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DocumentChunk>> ChunkDocumentAsync(
        ProcessedDocument document, 
        ChunkingOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Chunking document: {Title}", document.Title);

        options ??= new ChunkingOptions();

        try
        {
            var chunks = new List<DocumentChunk>();

            switch (options.Strategy)
            {
                case ChunkingStrategy.FixedSize:
                    chunks = ChunkByFixedSize(document, options);
                    break;
                case ChunkingStrategy.Semantic:
                    chunks = await ChunkBySemantic(document, options, cancellationToken);
                    break;
                case ChunkingStrategy.Sentence:
                    chunks = ChunkBySentence(document, options);
                    break;
                case ChunkingStrategy.Paragraph:
                    chunks = ChunkByParagraph(document, options);
                    break;
                default:
                    chunks = ChunkByFixedSize(document, options);
                    break;
            }

            _logger.LogDebug("Document chunked into {ChunkCount} chunks", chunks.Count);

            return chunks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error chunking document: {Title}", document.Title);
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerable<string> GetSupportedFormats()
    {
        return SupportedFormats;
    }

    /// <inheritdoc />
    public DocumentTypeInfo DetectDocumentType(DocumentInput input)
    {
        var mimeType = input.MimeType?.ToLowerInvariant();
        var fileName = input.FileName?.ToLowerInvariant();
        var extension = fileName != null ? Path.GetExtension(fileName).TrimStart('.') : null;

        // Check by MIME type first
        if (!string.IsNullOrEmpty(mimeType))
        {
            return mimeType switch
            {
                "text/plain" => new DocumentTypeInfo { DocumentType = "text", Confidence = 1.0, MimeType = mimeType },
                "text/markdown" => new DocumentTypeInfo { DocumentType = "markdown", Confidence = 1.0, MimeType = mimeType },
                "text/html" => new DocumentTypeInfo { DocumentType = "html", Confidence = 1.0, MimeType = mimeType },
                _ => DetectByExtension(extension)
            };
        }

        // Fall back to extension detection
        return DetectByExtension(extension);
    }

    private static DocumentTypeInfo DetectByExtension(string? extension)
    {
        return extension switch
        {
            "txt" => new DocumentTypeInfo { DocumentType = "text", Confidence = 0.9, MimeType = "text/plain" },
            "md" => new DocumentTypeInfo { DocumentType = "markdown", Confidence = 0.9, MimeType = "text/markdown" },
            "html" or "htm" => new DocumentTypeInfo { DocumentType = "html", Confidence = 0.9, MimeType = "text/html" },
            _ => new DocumentTypeInfo { DocumentType = "unknown", Confidence = 0.0 }
        };
    }

    private async Task<string> ExtractTextFromBinary(byte[] binaryContent, string? mimeType, CancellationToken cancellationToken)
    {
        // For now, assume text-based content and decode as UTF-8
        // In a production implementation, you would add support for PDF, DOCX, etc.
        try
        {
            return Encoding.UTF8.GetString(binaryContent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decode binary content as UTF-8, trying other encodings");
            
            // Try other encodings
            var encodings = new[] { Encoding.ASCII, Encoding.Unicode, Encoding.UTF32 };
            
            foreach (var encoding in encodings)
            {
                try
                {
                    return encoding.GetString(binaryContent);
                }
                catch
                {
                    // Continue to next encoding
                }
            }
            
            throw new NotSupportedException($"Unable to extract text from binary content with MIME type: {mimeType}");
        }
    }

    private static string CleanText(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        // Remove excessive whitespace
        content = Regex.Replace(content, @"\s+", " ");
        
        // Remove HTML tags if present
        content = Regex.Replace(content, @"<[^>]+>", "");
        
        // Normalize line endings
        content = content.Replace("\r\n", "\n").Replace("\r", "\n");
        
        return content.Trim();
    }

    private async Task<Dictionary<string, object>> ExtractMetadata(
        string content, 
        DocumentInput input, 
        CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, object>
        {
            ["CharacterCount"] = content.Length,
            ["WordCount"] = CountWords(content),
            ["LineCount"] = content.Split('\n').Length
        };

        // Add input metadata
        if (!string.IsNullOrEmpty(input.Author))
            metadata["Author"] = input.Author;
        
        if (input.CreatedAt.HasValue)
            metadata["CreatedAt"] = input.CreatedAt.Value;
        
        if (input.ModifiedAt.HasValue)
            metadata["ModifiedAt"] = input.ModifiedAt.Value;
        
        if (input.Tags.Any())
            metadata["Tags"] = input.Tags.ToArray();
        
        if (!string.IsNullOrEmpty(input.SourceUrl))
            metadata["SourceUrl"] = input.SourceUrl;

        return metadata;
    }

    private static string? ExtractTitleFromContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            return null;

        // Try to extract title from first line if it looks like a heading
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0)
        {
            var firstLine = lines[0].Trim();
            
            // Check for markdown heading
            if (firstLine.StartsWith("#"))
            {
                return firstLine.TrimStart('#').Trim();
            }
            
            // Check if first line is significantly shorter than average (likely a title)
            if (firstLine.Length < 100 && lines.Length > 1)
            {
                var avgLineLength = lines.Skip(1).Take(5).Average(l => l.Length);
                if (firstLine.Length < avgLineLength * 0.7)
                {
                    return firstLine;
                }
            }
        }

        return null;
    }

    private static string DetectLanguage(string content)
    {
        // Simple language detection - in production you'd use a proper language detection library
        // For now, assume English
        return "en";
    }

    private static int CountWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        return content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private List<DocumentChunk> ChunkByFixedSize(ProcessedDocument document, ChunkingOptions options)
    {
        var chunks = new List<DocumentChunk>();
        var content = document.Content;
        var position = 0;
        var chunkIndex = 0;

        while (position < content.Length)
        {
            var chunkSize = Math.Min(options.ChunkSize, content.Length - position);
            var chunkContent = content.Substring(position, chunkSize);

            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                DocumentId = document.Title ?? "unknown",
                Content = chunkContent,
                ChunkIndex = chunkIndex,
                StartPosition = position,
                EndPosition = position + chunkSize,
                DocumentMetadata = new Dictionary<string, object>(document.Metadata)
            };

            chunks.Add(chunk);

            // Move position with overlap consideration
            position += chunkSize - options.Overlap;
            chunkIndex++;
        }

        return chunks;
    }

    private async Task<List<DocumentChunk>> ChunkBySemantic(ProcessedDocument document, ChunkingOptions options, CancellationToken cancellationToken)
    {
        // For now, fall back to paragraph-based chunking for semantic chunking
        // In a production implementation, you would use NLP libraries for true semantic chunking
        return ChunkByParagraph(document, options);
    }

    private List<DocumentChunk> ChunkBySentence(ProcessedDocument document, ChunkingOptions options)
    {
        var sentences = SplitIntoSentences(document.Content);
        var chunks = new List<DocumentChunk>();
        var currentChunk = new StringBuilder();
        var chunkIndex = 0;
        var startPosition = 0;

        foreach (var sentence in sentences)
        {
            if (currentChunk.Length + sentence.Length > options.ChunkSize && currentChunk.Length > 0)
            {
                // Create chunk from current content
                var chunkContent = currentChunk.ToString().Trim();
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = document.Title ?? "unknown",
                    Content = chunkContent,
                    ChunkIndex = chunkIndex,
                    StartPosition = startPosition,
                    EndPosition = startPosition + chunkContent.Length,
                    DocumentMetadata = new Dictionary<string, object>(document.Metadata)
                };

                chunks.Add(chunk);
                chunkIndex++;
                startPosition += chunkContent.Length;
                currentChunk.Clear();
            }

            currentChunk.Append(sentence).Append(" ");
        }

        // Add final chunk if there's remaining content
        if (currentChunk.Length > 0)
        {
            var chunkContent = currentChunk.ToString().Trim();
            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                DocumentId = document.Title ?? "unknown",
                Content = chunkContent,
                ChunkIndex = chunkIndex,
                StartPosition = startPosition,
                EndPosition = startPosition + chunkContent.Length,
                DocumentMetadata = new Dictionary<string, object>(document.Metadata)
            };

            chunks.Add(chunk);
        }

        return chunks;
    }

    private List<DocumentChunk> ChunkByParagraph(ProcessedDocument document, ChunkingOptions options)
    {
        var paragraphs = document.Content.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<DocumentChunk>();
        var currentChunk = new StringBuilder();
        var chunkIndex = 0;
        var position = 0;

        foreach (var paragraph in paragraphs)
        {
            if (currentChunk.Length + paragraph.Length > options.ChunkSize && currentChunk.Length > 0)
            {
                // Create chunk from current content
                var chunkContent = currentChunk.ToString().Trim();
                var chunk = new DocumentChunk
                {
                    Id = Guid.NewGuid().ToString(),
                    DocumentId = document.Title ?? "unknown",
                    Content = chunkContent,
                    ChunkIndex = chunkIndex,
                    StartPosition = position - chunkContent.Length,
                    EndPosition = position,
                    DocumentMetadata = new Dictionary<string, object>(document.Metadata)
                };

                chunks.Add(chunk);
                chunkIndex++;
                currentChunk.Clear();
            }

            currentChunk.Append(paragraph).Append("\n\n");
            position += paragraph.Length + 2;
        }

        // Add final chunk if there's remaining content
        if (currentChunk.Length > 0)
        {
            var chunkContent = currentChunk.ToString().Trim();
            var chunk = new DocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                DocumentId = document.Title ?? "unknown",
                Content = chunkContent,
                ChunkIndex = chunkIndex,
                StartPosition = position - chunkContent.Length,
                EndPosition = position,
                DocumentMetadata = new Dictionary<string, object>(document.Metadata)
            };

            chunks.Add(chunk);
        }

        return chunks;
    }

    private static IEnumerable<string> SplitIntoSentences(string content)
    {
        // Simple sentence splitting - in production you'd use a proper NLP library
        var sentences = Regex.Split(content, @"(?<=[.!?])\s+")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim());

        return sentences;
    }
}