using FluentAI.Abstractions.Models.Rag;
using FluentAI.Services.Rag;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluentAI.NET.Tests.Rag;

public class DefaultDocumentProcessorTests
{
    private readonly Mock<ILogger<DefaultDocumentProcessor>> _mockLogger;
    private readonly DefaultDocumentProcessor _processor;

    public DefaultDocumentProcessorTests()
    {
        _mockLogger = new Mock<ILogger<DefaultDocumentProcessor>>();
        _processor = new DefaultDocumentProcessor(_mockLogger.Object);
    }

    [Fact]
    public async Task ProcessAsync_ShouldExtractContent_WhenTextProvided()
    {
        // Arrange
        var input = new DocumentInput
        {
            Content = "This is a test document with some content.",
            Title = "Test Document",
            Author = "Test Author"
        };

        // Act
        var result = await _processor.ProcessAsync(input);

        // Assert
        Assert.Equal("This is a test document with some content.", result.Content);
        Assert.Equal("Test Document", result.Title);
        Assert.True(result.Metadata.ContainsKey("Author"));
        Assert.Equal("Test Author", result.Metadata["Author"]);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCleanHtmlContent_WhenHtmlProvided()
    {
        // Arrange
        var input = new DocumentInput
        {
            Content = "<html><body><h1>Title</h1><p>This is a paragraph with <strong>bold</strong> text.</p></body></html>"
        };

        // Act
        var result = await _processor.ProcessAsync(input);

        // Assert
        Assert.DoesNotContain("<", result.Content);
        Assert.DoesNotContain(">", result.Content);
        Assert.Contains("Title", result.Content);
        Assert.Contains("paragraph", result.Content);
        Assert.Contains("bold", result.Content);
    }

    [Fact]
    public async Task ChunkDocumentAsync_ShouldCreateChunks_WhenUsingFixedSizeStrategy()
    {
        // Arrange
        var document = new ProcessedDocument
        {
            Content = "This is a long document that should be split into multiple chunks. " +
                     "Each chunk should be roughly the same size based on the chunk size parameter. " +
                     "The processor should handle overlapping content between chunks properly.",
            Title = "Test Document"
        };

        var options = new ChunkingOptions
        {
            Strategy = ChunkingStrategy.FixedSize,
            ChunkSize = 50,
            Overlap = 10
        };

        // Act
        var chunks = await _processor.ChunkDocumentAsync(document, options);
        var chunkList = chunks.ToList();

        // Assert
        Assert.True(chunkList.Count > 1);
        Assert.All(chunkList, chunk =>
        {
            Assert.NotEmpty(chunk.Id);
            Assert.Equal("Test Document", chunk.DocumentId);
            Assert.True(chunk.Content.Length <= 50);
            Assert.True(chunk.ChunkIndex >= 0);
        });
    }

    [Fact]
    public async Task ChunkDocumentAsync_ShouldCreateChunks_WhenUsingParagraphStrategy()
    {
        // Arrange
        var document = new ProcessedDocument
        {
            Content = "First paragraph with some content.\n\n" +
                     "Second paragraph with different content.\n\n" +
                     "Third paragraph with more content.",
            Title = "Test Document"
        };

        var options = new ChunkingOptions
        {
            Strategy = ChunkingStrategy.Paragraph,
            ChunkSize = 100,
            Overlap = 0
        };

        // Act
        var chunks = await _processor.ChunkDocumentAsync(document, options);
        var chunkList = chunks.ToList();

        // Assert
        Assert.True(chunkList.Count >= 1);
        Assert.All(chunkList, chunk =>
        {
            Assert.NotEmpty(chunk.Content);
            Assert.Contains("paragraph", chunk.Content);
        });
    }

    [Fact]
    public void GetSupportedFormats_ShouldReturnExpectedFormats()
    {
        // Act
        var formats = _processor.GetSupportedFormats();

        // Assert
        Assert.Contains("txt", formats);
        Assert.Contains("md", formats);
        Assert.Contains("html", formats);
    }

    [Fact]
    public void DetectDocumentType_ShouldDetectTextType_WhenTextMimeType()
    {
        // Arrange
        var input = new DocumentInput
        {
            Content = "Plain text content",
            MimeType = "text/plain"
        };

        // Act
        var typeInfo = _processor.DetectDocumentType(input);

        // Assert
        Assert.Equal("text", typeInfo.DocumentType);
        Assert.Equal(1.0, typeInfo.Confidence);
        Assert.Equal("text/plain", typeInfo.MimeType);
    }

    [Fact]
    public void DetectDocumentType_ShouldDetectMarkdownType_WhenMarkdownExtension()
    {
        // Arrange
        var input = new DocumentInput
        {
            Content = "# Markdown content",
            FileName = "test.md"
        };

        // Act
        var typeInfo = _processor.DetectDocumentType(input);

        // Assert
        Assert.Equal("markdown", typeInfo.DocumentType);
        Assert.Equal(0.9, typeInfo.Confidence);
    }

    [Fact]
    public async Task ProcessAsync_ShouldIncludeMetadata_WhenExtractMetadataEnabled()
    {
        // Arrange
        var input = new DocumentInput
        {
            Content = "Test content with multiple words and sentences.",
            Tags = new[] { "test", "document" },
            SourceUrl = "https://example.com/test"
        };

        var options = new ProcessingOptions
        {
            ExtractMetadata = true
        };

        // Act
        var result = await _processor.ProcessAsync(input, options);

        // Assert
        Assert.True(result.Metadata.ContainsKey("CharacterCount"));
        Assert.True(result.Metadata.ContainsKey("WordCount"));
        Assert.True(result.Metadata.ContainsKey("LineCount"));
        Assert.True(result.Metadata.ContainsKey("Tags"));
        Assert.True(result.Metadata.ContainsKey("SourceUrl"));
        
        Assert.True((int)result.Metadata["CharacterCount"] > 0);
        Assert.True((int)result.Metadata["WordCount"] > 0);
        Assert.Equal(new[] { "test", "document" }, result.Metadata["Tags"]);
        Assert.Equal("https://example.com/test", result.Metadata["SourceUrl"]);
    }

    [Fact]
    public async Task ChunkDocumentAsync_ShouldMaintainDocumentMetadata_InChunks()
    {
        // Arrange
        var document = new ProcessedDocument
        {
            Content = "This is test content that will be chunked.",
            Title = "Test Document",
            Metadata = new Dictionary<string, object>
            {
                ["Author"] = "Test Author",
                ["CreatedDate"] = "2024-01-01"
            }
        };

        var options = new ChunkingOptions
        {
            Strategy = ChunkingStrategy.FixedSize,
            ChunkSize = 20,
            Overlap = 5
        };

        // Act
        var chunks = await _processor.ChunkDocumentAsync(document, options);
        var chunkList = chunks.ToList();

        // Assert
        Assert.All(chunkList, chunk =>
        {
            Assert.True(chunk.DocumentMetadata.ContainsKey("Author"));
            Assert.True(chunk.DocumentMetadata.ContainsKey("CreatedDate"));
            Assert.Equal("Test Author", chunk.DocumentMetadata["Author"]);
            Assert.Equal("2024-01-01", chunk.DocumentMetadata["CreatedDate"]);
        });
    }
}