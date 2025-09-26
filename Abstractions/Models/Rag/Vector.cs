namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Represents a vector with its identifier and metadata for storage and retrieval.
/// </summary>
public class Vector
{
    /// <summary>
    /// Gets or sets the unique identifier for this vector.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vector values (embedding).
    /// </summary>
    public float[] Values { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the metadata associated with this vector.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the namespace or collection this vector belongs to.
    /// </summary>
    public string? Namespace { get; set; }

    /// <summary>
    /// Gets or sets the sparse vector values for hybrid search scenarios.
    /// </summary>
    public SparseVector? SparseValues { get; set; }
}

/// <summary>
/// Represents sparse vector data for hybrid search capabilities.
/// </summary>
public class SparseVector
{
    /// <summary>
    /// Gets or sets the indices of non-zero values.
    /// </summary>
    public int[] Indices { get; set; } = Array.Empty<int>();

    /// <summary>
    /// Gets or sets the non-zero values.
    /// </summary>
    public float[] Values { get; set; } = Array.Empty<float>();
}