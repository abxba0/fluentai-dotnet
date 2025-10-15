using FluentAI.Configuration;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Configuration;

public class RagOptionsTests
{
    [Fact]
    public void RagOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new RagOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.VectorDatabase);
        Assert.Null(options.Embedding);
        Assert.Null(options.DocumentProcessing);
        Assert.Null(options.Retrieval);
    }

    [Fact]
    public void RagOptions_SetVectorDatabase_ShouldUpdateValue()
    {
        // Arrange
        var options = new RagOptions();
        var vectorDb = new VectorDatabaseOptions { Provider = "Pinecone" };

        // Act
        options.VectorDatabase = vectorDb;

        // Assert
        Assert.Same(vectorDb, options.VectorDatabase);
        Assert.Equal("Pinecone", options.VectorDatabase.Provider);
    }

    [Fact]
    public void RagOptions_SetEmbedding_ShouldUpdateValue()
    {
        // Arrange
        var options = new RagOptions();
        var embedding = new EmbeddingOptions { Model = "text-embedding-3-large" };

        // Act
        options.Embedding = embedding;

        // Assert
        Assert.Same(embedding, options.Embedding);
        Assert.Equal("text-embedding-3-large", options.Embedding.Model);
    }

    [Fact]
    public void RagOptions_SetDocumentProcessing_ShouldUpdateValue()
    {
        // Arrange
        var options = new RagOptions();
        var docProcessing = new DocumentProcessingOptions { ChunkSize = 2000 };

        // Act
        options.DocumentProcessing = docProcessing;

        // Assert
        Assert.Same(docProcessing, options.DocumentProcessing);
        Assert.Equal(2000, options.DocumentProcessing.ChunkSize);
    }

    [Fact]
    public void RagOptions_SetRetrieval_ShouldUpdateValue()
    {
        // Arrange
        var options = new RagOptions();
        var retrieval = new RetrievalOptions { TopK = 10 };

        // Act
        options.Retrieval = retrieval;

        // Assert
        Assert.Same(retrieval, options.Retrieval);
        Assert.Equal(10, options.Retrieval.TopK);
    }

    [Fact]
    public void DocumentProcessingOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new DocumentProcessingOptions();

        // Assert
        Assert.Equal("Semantic", options.ChunkingStrategy);
        Assert.Equal(1000, options.ChunkSize);
        Assert.Equal(200, options.Overlap);
    }

    [Fact]
    public void EmbeddingOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new EmbeddingOptions();

        // Assert
        Assert.Equal("OpenAI", options.Provider);
        Assert.Equal("text-embedding-ada-002", options.Model);
        Assert.Equal(100, options.BatchSize);
    }

    [Fact]
    public void RetrievalOptions_DefaultConstructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var options = new RetrievalOptions();

        // Assert
        Assert.Equal(5, options.TopK);
        Assert.Equal(0.7, options.SimilarityThreshold);
        Assert.True(options.ReRanking);
        Assert.True(options.QueryEnhancement);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void DocumentProcessingOptions_SetChunkSize_WithVariousValues_ShouldUpdateCorrectly(int chunkSize)
    {
        // Arrange
        var options = new DocumentProcessingOptions();

        // Act
        options.ChunkSize = chunkSize;

        // Assert
        Assert.Equal(chunkSize, options.ChunkSize);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(20)]
    public void RetrievalOptions_SetTopK_WithVariousValues_ShouldUpdateCorrectly(int topK)
    {
        // Arrange
        var options = new RetrievalOptions();

        // Act
        options.TopK = topK;

        // Assert
        Assert.Equal(topK, options.TopK);
    }
}
