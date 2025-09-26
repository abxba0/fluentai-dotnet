namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents input document data for processing and indexing.
/// </summary>
public class DocumentInput
{
    /// <summary>
    /// Gets or sets the document content as text.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the document content as binary data.
    /// </summary>
    public byte[]? BinaryContent { get; set; }

    /// <summary>
    /// Gets or sets the document title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the original filename.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the document.
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// Gets or sets the document author.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the document creation date.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the document last modified date.
    /// </summary>
    public DateTimeOffset? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets document tags for categorization.
    /// </summary>
    public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the document language (ISO 639-1 code).
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Gets or sets the source URL if the document was retrieved from the web.
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// Gets or sets additional custom metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}