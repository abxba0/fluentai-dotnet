namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a request to search for similar vectors.
/// </summary>
public class VectorSearchRequest
{
    /// <summary>
    /// Gets or sets the query vector to search with.
    /// </summary>
    public float[] QueryVector { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the sparse query vector for hybrid search.
    /// </summary>
    public SparseVector? SparseQueryVector { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// Default is 10.
    /// </summary>
    public int TopK { get; set; } = 10;

    /// <summary>
    /// Gets or sets the minimum similarity score threshold.
    /// </summary>
    public double MinScore { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets filters to apply to the search.
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Gets or sets the namespace to search within.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets whether to include vector values in the response.
    /// Default is false.
    /// </summary>
    public bool IncludeValues { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include metadata in the response.
    /// Default is true.
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;
}

/// <summary>
/// Represents the result of a vector search operation.
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// Gets or sets the matching vectors with their scores.
    /// </summary>
    public IEnumerable<VectorMatch> Matches { get; set; } = Enumerable.Empty<VectorMatch>();

    /// <summary>
    /// Gets or sets the namespace that was searched.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the time taken for the search operation.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional search metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a vector match from a search operation.
/// </summary>
public class VectorMatch
{
    /// <summary>
    /// Gets or sets the unique identifier of the matching vector.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the similarity score (0.0 to 1.0).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Gets or sets the vector values if requested.
    /// </summary>
    public float[]? Values { get; set; }

    /// <summary>
    /// Gets or sets the sparse vector values if applicable.
    /// </summary>
    public SparseVector? SparseValues { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with this vector.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of an index operation.
/// </summary>
public class IndexResult
{
    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of vectors that were upserted.
    /// </summary>
    public int UpsertedCount { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during the operation.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the time taken for the operation.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional operation metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of a deletion operation.
/// </summary>
public class DeletionResult
{
    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the number of vectors that were deleted.
    /// </summary>
    public int DeletedCount { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during the operation.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    /// <summary>
    /// Gets or sets the time taken for the operation.
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }

    /// <summary>
    /// Gets or sets additional operation metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the result of a health check operation.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Gets or sets whether the database is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Gets or sets the health status message.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the response time for the health check.
    /// </summary>
    public TimeSpan ResponseTime { get; set; }

    /// <summary>
    /// Gets or sets additional health information.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp of the health check.
    /// </summary>
    public DateTimeOffset CheckedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents database statistics.
/// </summary>
public class DatabaseStats
{
    /// <summary>
    /// Gets or sets the total number of vectors in the database.
    /// </summary>
    public long VectorCount { get; set; }

    /// <summary>
    /// Gets or sets the total storage used in bytes.
    /// </summary>
    public long StorageUsedBytes { get; set; }

    /// <summary>
    /// Gets or sets the number of namespaces.
    /// </summary>
    public int NamespaceCount { get; set; }

    /// <summary>
    /// Gets or sets the vector dimensions.
    /// </summary>
    public int Dimensions { get; set; }

    /// <summary>
    /// Gets or sets additional database statistics.
    /// </summary>
    public Dictionary<string, object> AdditionalStats { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when the statistics were collected.
    /// </summary>
    public DateTimeOffset CollectedAt { get; set; } = DateTimeOffset.UtcNow;
}