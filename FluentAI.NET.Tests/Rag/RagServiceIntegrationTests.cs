using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Models.Rag;
using FluentAI.Configuration;
using FluentAI.Services.Rag;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FluentAI.NET.Tests.Rag;

public class RagServiceIntegrationTests
{
    private readonly Mock<ILogger<DefaultRagService>> _mockLogger;
    private readonly Mock<IChatModel> _mockChatModel;
    private readonly InMemoryVectorDatabase _vectorDatabase;
    private readonly DefaultDocumentProcessor _documentProcessor;
    private readonly Mock<IEmbeddingGenerator> _mockEmbeddingGenerator;
    private readonly DefaultRagService _ragService;

    public RagServiceIntegrationTests()
    {
        _mockLogger = new Mock<ILogger<DefaultRagService>>();
        _mockChatModel = new Mock<IChatModel>();
        
        var vectorDbLogger = new Mock<ILogger<InMemoryVectorDatabase>>();
        _vectorDatabase = new InMemoryVectorDatabase(vectorDbLogger.Object);
        
        var docProcessorLogger = new Mock<ILogger<DefaultDocumentProcessor>>();
        _documentProcessor = new DefaultDocumentProcessor(docProcessorLogger.Object);
        
        _mockEmbeddingGenerator = new Mock<IEmbeddingGenerator>();
        
        var ragOptions = new RagOptions
        {
            Retrieval = new FluentAI.Configuration.RetrievalOptions
            {
                TopK = 5,
                SimilarityThreshold = 0.1
            }
        };

        var options = Options.Create(ragOptions);
        
        _ragService = new DefaultRagService(
            _vectorDatabase,
            _mockEmbeddingGenerator.Object,
            _documentProcessor,
            _mockChatModel.Object,
            _mockLogger.Object,
            options);
    }

    [Fact]
    public async Task IndexDocumentAsync_ShouldIndexDocument_WhenValidDocumentProvided()
    {
        // Arrange
        var request = new DocumentIndexRequest
        {
            Id = "test-doc-1",
            Document = new DocumentInput
            {
                Content = "This is a test document with some content for indexing. It should be processed and chunked properly.",
                Title = "Test Document"
            },
            ChunkingOptions = new ChunkingOptions
            {
                Strategy = ChunkingStrategy.FixedSize,
                ChunkSize = 50,
                Overlap = 10
            }
        };

        // Setup embedding generator to return mock embeddings
        _mockEmbeddingGenerator
            .Setup(x => x.GenerateEmbeddingsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbeddingResult
            {
                Embeddings = new[]
                {
                    new Embedding { Text = "chunk1", Vector = new[] { 1.0f, 0.0f, 0.0f } },
                    new Embedding { Text = "chunk2", Vector = new[] { 0.0f, 1.0f, 0.0f } }
                }
            });

        // Act
        var result = await _ragService.IndexDocumentAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.IndexedCount);
        Assert.True(result.ChunkCount > 0);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task RetrieveAsync_ShouldReturnRelevantChunks_WhenDocumentsIndexed()
    {
        // Arrange - Index a document first
        await IndexTestDocument();

        // Setup embedding generator for query
        _mockEmbeddingGenerator
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<EmbeddingRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbeddingResult
            {
                Embeddings = new[]
                {
                    new Embedding { Text = "test query", Vector = new[] { 1.0f, 0.0f, 0.0f } }
                }
            });

        var options = new FluentAI.Abstractions.Models.Rag.RetrievalOptions
        {
            TopK = 3,
            SimilarityThreshold = 0.1
        };

        // Act
        var result = await _ragService.RetrieveAsync("test query", options);

        // Assert
        Assert.NotEmpty(result.Chunks);
        Assert.Equal("test query", result.Query);
        Assert.All(result.Chunks, chunk =>
        {
            Assert.NotEmpty(chunk.Content);
            Assert.NotEmpty(chunk.DocumentId);
        });
    }

    [Fact]
    public async Task QueryAsync_ShouldReturnRagResponse_WhenQueryProcessed()
    {
        // Arrange - Index a document first
        await IndexTestDocument();

        // Setup embedding generator for query
        _mockEmbeddingGenerator
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<EmbeddingRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbeddingResult
            {
                Embeddings = new[]
                {
                    new Embedding { Text = "test query", Vector = new[] { 1.0f, 0.0f, 0.0f } }
                }
            });

        // Setup chat model response
        _mockChatModel
            .Setup(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ChatResponse("Generated response based on context", "test-model", "completed", new TokenUsage(50, 25)));

        var request = new RagRequest
        {
            Query = "What is this document about?",
            Messages = new[]
            {
                new ChatMessage(ChatRole.User, "What is this document about?")
            }
        };

        // Act
        var result = await _ragService.QueryAsync(request);

        // Assert
        Assert.Equal("Generated response based on context", result.Content);
        Assert.NotEmpty(result.RetrievedContext);
        Assert.True(result.ConfidenceScore >= 0);
        Assert.Equal("test-model", result.ModelUsed);
        Assert.NotNull(result.TokenUsage);
    }

    [Fact]
    public async Task StreamQueryAsync_ShouldReturnStreamingTokens_WhenQueryProcessed()
    {
        // Arrange - Index a document first
        await IndexTestDocument();

        // Setup embedding generator for query
        _mockEmbeddingGenerator
            .Setup(x => x.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<EmbeddingRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbeddingResult
            {
                Embeddings = new[]
                {
                    new Embedding { Text = "test query", Vector = new[] { 1.0f, 0.0f, 0.0f } }
                }
            });

        // Setup chat model streaming response
        _mockChatModel
            .Setup(x => x.StreamResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatRequestOptions>(), It.IsAny<CancellationToken>()))
            .Returns(CreateAsyncEnumerable(new[] { "Generated", " streaming", " response" }));

        var request = new RagRequest
        {
            Query = "What is this document about?",
            Messages = new[]
            {
                new ChatMessage(ChatRole.User, "What is this document about?")
            }
        };

        // Act
        var tokens = new List<RagStreamToken>();
        await foreach (var token in _ragService.StreamQueryAsync(request))
        {
            tokens.Add(token);
        }

        // Assert
        Assert.NotEmpty(tokens);
        
        // Should have context token at the beginning
        Assert.Contains(tokens, t => t.TokenType == StreamTokenType.Context);
        
        // Should have content tokens
        Assert.Contains(tokens, t => t.TokenType == StreamTokenType.Content);
        
        // Should have completion token at the end
        Assert.Contains(tokens, t => t.TokenType == StreamTokenType.Citation && t.IsComplete);
    }

    private async Task IndexTestDocument()
    {
        var request = new DocumentIndexRequest
        {
            Id = "test-doc",
            Document = new DocumentInput
            {
                Content = "This is a comprehensive test document about artificial intelligence and machine learning. " +
                         "It contains information about various AI concepts, algorithms, and applications.",
                Title = "AI Test Document"
            },
            ChunkingOptions = new ChunkingOptions
            {
                Strategy = ChunkingStrategy.FixedSize,
                ChunkSize = 50,
                Overlap = 10
            }
        };

        _mockEmbeddingGenerator
            .Setup(x => x.GenerateEmbeddingsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<EmbeddingRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmbeddingResult
            {
                Embeddings = new[]
                {
                    new Embedding { Text = "chunk1", Vector = new[] { 1.0f, 0.0f, 0.0f } },
                    new Embedding { Text = "chunk2", Vector = new[] { 0.0f, 1.0f, 0.0f } },
                    new Embedding { Text = "chunk3", Vector = new[] { 0.0f, 0.0f, 1.0f } }
                }
            });

        await _ragService.IndexDocumentAsync(request);
    }

    private static async IAsyncEnumerable<string> CreateAsyncEnumerable(IEnumerable<string> items)
    {
        foreach (var item in items)
        {
            yield return item;
        }
    }
}