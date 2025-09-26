using FluentAI.Abstractions;
using FluentAI.Abstractions.Models.Rag;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FluentAI.Services.Rag;

/// <summary>
/// In-memory implementation of a vector database for development and testing.
/// </summary>
public class InMemoryVectorDatabase : IVectorDatabase
{
    private readonly ILogger<InMemoryVectorDatabase> _logger;
    private readonly ConcurrentDictionary<string, Vector> _vectors = new();
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryVectorDatabase"/> class.
    /// </summary>
    public InMemoryVectorDatabase(ILogger<InMemoryVectorDatabase> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<VectorSearchResult> SearchAsync(VectorSearchRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Searching vectors with TopK: {TopK}, MinScore: {MinScore}", request.TopK, request.MinScore);
        
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            var matches = new List<VectorMatch>();

            // Filter by namespace if specified
            var vectors = string.IsNullOrEmpty(request.Namespace) 
                ? _vectors.Values 
                : _vectors.Values.Where(v => v.Namespace == request.Namespace);

            // Apply metadata filters
            if (request.Filters.Any())
            {
                vectors = vectors.Where(v => MatchesFilters(v, request.Filters));
            }

            // Calculate similarities and find matches
            foreach (var vector in vectors)
            {
                var similarity = CalculateCosineSimilarity(request.QueryVector, vector.Values);
                
                if (similarity >= request.MinScore)
                {
                    var match = new VectorMatch
                    {
                        Id = vector.Id,
                        Score = similarity,
                        Values = request.IncludeValues ? vector.Values : null,
                        SparseValues = vector.SparseValues,
                        Metadata = request.IncludeMetadata ? vector.Metadata : new Dictionary<string, object>()
                    };

                    matches.Add(match);
                }
            }

            // Sort by score descending and take top K
            var topMatches = matches
                .OrderByDescending(m => m.Score)
                .Take(request.TopK)
                .ToList();

            var result = new VectorSearchResult
            {
                Matches = topMatches,
                Namespace = request.Namespace,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["TotalVectors"] = vectors.Count(),
                    ["ScoreThreshold"] = request.MinScore
                }
            };

            _logger.LogDebug("Vector search completed: {MatchCount} matches found in {ProcessingTime}ms", 
                topMatches.Count, result.ProcessingTime.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during vector search");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IndexResult> UpsertAsync(IEnumerable<Vector> vectors, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Upserting {VectorCount} vectors", vectors.Count());
        
        var startTime = DateTimeOffset.UtcNow;
        var upsertedCount = 0;
        var errors = new List<string>();

        try
        {
            lock (_lockObject)
            {
                foreach (var vector in vectors)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(vector.Id))
                        {
                            errors.Add("Vector ID cannot be null or empty");
                            continue;
                        }

                        if (vector.Values == null || vector.Values.Length == 0)
                        {
                            errors.Add($"Vector {vector.Id} has no values");
                            continue;
                        }

                        _vectors.AddOrUpdate(vector.Id, vector, (key, oldValue) => vector);
                        upsertedCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error upserting vector {vector.Id}: {ex.Message}");
                    }
                }
            }

            var result = new IndexResult
            {
                Success = errors.Count == 0,
                UpsertedCount = upsertedCount,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Errors = errors
            };

            _logger.LogDebug("Vector upsert completed: {UpsertedCount} vectors upserted, {ErrorCount} errors", 
                upsertedCount, errors.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during vector upsert");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting vector: {VectorId}", id);

        try
        {
            var removed = _vectors.TryRemove(id, out _);
            
            _logger.LogDebug("Vector deletion result: {VectorId} - {Result}", id, removed ? "Success" : "Not found");
            
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vector: {VectorId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<DeletionResult> DeleteManyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting {IdCount} vectors", ids.Count());
        
        var startTime = DateTimeOffset.UtcNow;
        var deletedCount = 0;
        var errors = new List<string>();

        try
        {
            foreach (var id in ids)
            {
                try
                {
                    if (_vectors.TryRemove(id, out _))
                    {
                        deletedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error deleting vector {id}: {ex.Message}");
                }
            }

            var result = new DeletionResult
            {
                Success = errors.Count == 0,
                DeletedCount = deletedCount,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Errors = errors
            };

            _logger.LogDebug("Vector deletion completed: {DeletedCount} vectors deleted, {ErrorCount} errors", 
                deletedCount, errors.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk vector deletion");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Simple health check - just verify we can access the data structure
            var vectorCount = _vectors.Count;
            var responseTime = DateTimeOffset.UtcNow - startTime;

            return new HealthCheckResult
            {
                IsHealthy = true,
                Status = "Healthy",
                ResponseTime = responseTime,
                Details = new Dictionary<string, object>
                {
                    ["VectorCount"] = vectorCount,
                    ["DatabaseType"] = "InMemory"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            return new HealthCheckResult
            {
                IsHealthy = false,
                Status = $"Unhealthy: {ex.Message}",
                ResponseTime = DateTimeOffset.UtcNow - startTime,
                Details = new Dictionary<string, object>
                {
                    ["Error"] = ex.Message,
                    ["DatabaseType"] = "InMemory"
                }
            };
        }
    }

    /// <inheritdoc />
    public async Task<DatabaseStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var vectors = _vectors.Values.ToList();
            var dimensions = vectors.FirstOrDefault()?.Values.Length ?? 0;
            var namespaces = vectors.Select(v => v.Namespace).Where(n => !string.IsNullOrEmpty(n)).Distinct().Count();

            // Estimate storage size (rough calculation)
            var storageBytes = vectors.Sum(v => 
                sizeof(float) * v.Values.Length + // Vector values
                v.Id.Length * sizeof(char) + // ID string
                EstimateMetadataSize(v.Metadata)); // Metadata

            return new DatabaseStats
            {
                VectorCount = vectors.Count,
                StorageUsedBytes = storageBytes,
                NamespaceCount = namespaces,
                Dimensions = dimensions,
                AdditionalStats = new Dictionary<string, object>
                {
                    ["DatabaseType"] = "InMemory",
                    ["AverageMetadataSize"] = vectors.Count > 0 ? vectors.Average(v => EstimateMetadataSize(v.Metadata)) : 0
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database stats");
            throw;
        }
    }

    private static bool MatchesFilters(Vector vector, Dictionary<string, object> filters)
    {
        foreach (var filter in filters)
        {
            if (!vector.Metadata.TryGetValue(filter.Key, out var value))
            {
                return false;
            }

            // Simple equality check - in production you'd support more complex filtering
            if (!Equals(value, filter.Value))
            {
                return false;
            }
        }

        return true;
    }

    private static double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same dimensions");
        }

        double dotProduct = 0;
        double norm1 = 0;
        double norm2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            norm1 += vector1[i] * vector1[i];
            norm2 += vector2[i] * vector2[i];
        }

        if (norm1 == 0 || norm2 == 0)
        {
            return 0;
        }

        return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }

    private static long EstimateMetadataSize(Dictionary<string, object> metadata)
    {
        long size = 0;
        
        foreach (var kvp in metadata)
        {
            // Rough estimation of memory usage
            size += kvp.Key.Length * sizeof(char);
            
            if (kvp.Value is string str)
            {
                size += str.Length * sizeof(char);
            }
            else
            {
                // Rough estimation for other types
                size += 50; // Average estimate
            }
        }

        return size;
    }
}