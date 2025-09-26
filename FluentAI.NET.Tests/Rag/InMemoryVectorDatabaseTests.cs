using FluentAI.Abstractions.Models.Rag;
using FluentAI.Services.Rag;
using Microsoft.Extensions.Logging;
using Moq;

namespace FluentAI.NET.Tests.Rag;

public class InMemoryVectorDatabaseTests
{
    private readonly Mock<ILogger<InMemoryVectorDatabase>> _mockLogger;
    private readonly InMemoryVectorDatabase _vectorDatabase;

    public InMemoryVectorDatabaseTests()
    {
        _mockLogger = new Mock<ILogger<InMemoryVectorDatabase>>();
        _vectorDatabase = new InMemoryVectorDatabase(_mockLogger.Object);
    }

    [Fact]
    public async Task UpsertAsync_ShouldAddVectors_WhenValidVectorsProvided()
    {
        // Arrange
        var vectors = new[]
        {
            new Vector
            {
                Id = "test1",
                Values = new[] { 1.0f, 2.0f, 3.0f },
                Metadata = new Dictionary<string, object> { ["content"] = "Test content 1" }
            },
            new Vector
            {
                Id = "test2", 
                Values = new[] { 4.0f, 5.0f, 6.0f },
                Metadata = new Dictionary<string, object> { ["content"] = "Test content 2" }
            }
        };

        // Act
        var result = await _vectorDatabase.UpsertAsync(vectors);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.UpsertedCount);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnSimilarVectors_WhenVectorsExist()
    {
        // Arrange
        var vectors = new[]
        {
            new Vector
            {
                Id = "test1",
                Values = new[] { 1.0f, 0.0f, 0.0f },
                Metadata = new Dictionary<string, object> { ["content"] = "Test content 1" }
            },
            new Vector
            {
                Id = "test2",
                Values = new[] { 0.0f, 1.0f, 0.0f },
                Metadata = new Dictionary<string, object> { ["content"] = "Test content 2" }
            }
        };

        await _vectorDatabase.UpsertAsync(vectors);

        var searchRequest = new VectorSearchRequest
        {
            QueryVector = new[] { 1.0f, 0.0f, 0.0f }, // Should match test1 exactly
            TopK = 1,
            MinScore = 0.0,
            IncludeMetadata = true
        };

        // Act
        var result = await _vectorDatabase.SearchAsync(searchRequest);

        // Assert
        Assert.Single(result.Matches);
        var match = result.Matches.First();
        Assert.Equal("test1", match.Id);
        Assert.True(match.Score > 0.9); // Should be very close to 1.0 (perfect match)
        Assert.Equal("Test content 1", match.Metadata["content"]);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveVector_WhenVectorExists()
    {
        // Arrange
        var vector = new Vector
        {
            Id = "test1",
            Values = new[] { 1.0f, 2.0f, 3.0f },
            Metadata = new Dictionary<string, object> { ["content"] = "Test content" }
        };

        await _vectorDatabase.UpsertAsync(new[] { vector });

        // Act
        var result = await _vectorDatabase.DeleteAsync("test1");

        // Assert
        Assert.True(result);

        // Verify vector is gone
        var searchRequest = new VectorSearchRequest
        {
            QueryVector = new[] { 1.0f, 2.0f, 3.0f },
            TopK = 10,
            MinScore = 0.0
        };

        var searchResult = await _vectorDatabase.SearchAsync(searchRequest);
        Assert.Empty(searchResult.Matches);
    }

    [Fact]
    public async Task HealthCheckAsync_ShouldReturnHealthy_Always()
    {
        // Act
        var result = await _vectorDatabase.HealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
        Assert.True(result.Details.ContainsKey("VectorCount"));
    }

    [Fact]
    public async Task GetStatsAsync_ShouldReturnCorrectStats_WhenVectorsExist()
    {
        // Arrange
        var vectors = new[]
        {
            new Vector { Id = "test1", Values = new[] { 1.0f, 2.0f, 3.0f } },
            new Vector { Id = "test2", Values = new[] { 4.0f, 5.0f, 6.0f } }
        };

        await _vectorDatabase.UpsertAsync(vectors);

        // Act
        var stats = await _vectorDatabase.GetStatsAsync();

        // Assert
        Assert.Equal(2, stats.VectorCount);
        Assert.Equal(3, stats.Dimensions);
        Assert.True(stats.StorageUsedBytes > 0);
    }

    [Fact]
    public async Task SearchAsync_ShouldApplyFilters_WhenFiltersProvided()
    {
        // Arrange
        var vectors = new[]
        {
            new Vector
            {
                Id = "test1",
                Values = new[] { 1.0f, 0.0f, 0.0f },
                Metadata = new Dictionary<string, object> { ["category"] = "A", ["content"] = "Content A" }
            },
            new Vector
            {
                Id = "test2",
                Values = new[] { 1.0f, 0.0f, 0.0f }, // Same vector but different category
                Metadata = new Dictionary<string, object> { ["category"] = "B", ["content"] = "Content B" }
            }
        };

        await _vectorDatabase.UpsertAsync(vectors);

        var searchRequest = new VectorSearchRequest
        {
            QueryVector = new[] { 1.0f, 0.0f, 0.0f },
            TopK = 10,
            MinScore = 0.0,
            Filters = new Dictionary<string, object> { ["category"] = "A" },
            IncludeMetadata = true
        };

        // Act
        var result = await _vectorDatabase.SearchAsync(searchRequest);

        // Assert
        Assert.Single(result.Matches);
        Assert.Equal("test1", result.Matches.First().Id);
        Assert.Equal("Content A", result.Matches.First().Metadata["content"]);
    }
}