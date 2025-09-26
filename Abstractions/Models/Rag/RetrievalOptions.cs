namespace FluentAI.Abstractions.Models.Rag;

/// <summary>
/// Configuration options for document retrieval in RAG operations.
/// </summary>
public class RetrievalOptions
{
    /// <summary>
    /// Gets or sets the maximum number of document chunks to retrieve.
    /// Default is 5.
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum similarity threshold for retrieved chunks (0.0 to 1.0).
    /// Default is 0.7.
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets whether to enable re-ranking of retrieved results.
    /// Default is true.
    /// </summary>
    public bool ReRanking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enhance the query before retrieval.
    /// Default is true.
    /// </summary>
    public bool QueryEnhancement { get; set; } = true;

    /// <summary>
    /// Gets or sets the search strategy to use.
    /// </summary>
    public SearchStrategy SearchStrategy { get; set; } = SearchStrategy.Semantic;

    /// <summary>
    /// Gets or sets filters to apply during retrieval.
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum character length for combined retrieved context.
    /// Default is 4000.
    /// </summary>
    public int MaxContextLength { get; set; } = 4000;

    /// <summary>
    /// Gets or sets whether to include source information in results.
    /// Default is true.
    /// </summary>
    public bool IncludeSources { get; set; } = true;
}

/// <summary>
/// Defines the search strategy for document retrieval.
/// </summary>
public enum SearchStrategy
{
    /// <summary>
    /// Semantic search using vector similarity.
    /// </summary>
    Semantic,

    /// <summary>
    /// Keyword-based search.
    /// </summary>
    Keyword,

    /// <summary>
    /// Hybrid search combining semantic and keyword approaches.
    /// </summary>
    Hybrid
}