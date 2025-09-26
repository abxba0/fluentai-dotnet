using FluentAI.Abstractions.Models.Rag;

namespace FluentAI.Abstractions;

/// <summary>
/// Defines the contract for vector database operations.
/// </summary>
public interface IVectorDatabase
{
    /// <summary>
    /// Searches for similar vectors in the database.
    /// </summary>
    /// <param name="request">The vector search request with query vector and options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The search results with similar vectors and scores.</returns>
    Task<VectorSearchResult> SearchAsync(VectorSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts (inserts or updates) vectors in the database.
    /// </summary>
    /// <param name="vectors">The vectors to upsert with their metadata.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The upsert result with status information.</returns>
    Task<IndexResult> UpsertAsync(IEnumerable<Vector> vectors, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vector by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vector to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the vector was deleted, false if not found.</returns>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple vectors by their identifiers.
    /// </summary>
    /// <param name="ids">The unique identifiers of the vectors to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The deletion result with count of deleted vectors.</returns>
    Task<DeletionResult> DeleteManyAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the health and connectivity of the vector database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The health check result.</returns>
    Task<HealthCheckResult> HealthCheckAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about the vector database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Database statistics including vector count and storage usage.</returns>
    Task<DatabaseStats> GetStatsAsync(CancellationToken cancellationToken = default);
}