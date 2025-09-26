namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for RAG (Retrieval Augmented Generation) functionality.
/// </summary>
public class RagOptions
{
    /// <summary>
    /// Gets or sets the vector database configuration.
    /// </summary>
    public VectorDatabaseOptions? VectorDatabase { get; set; }

    /// <summary>
    /// Gets or sets the embedding generation configuration.
    /// </summary>
    public EmbeddingOptions? Embedding { get; set; }

    /// <summary>
    /// Gets or sets the document processing configuration.
    /// </summary>
    public DocumentProcessingOptions? DocumentProcessing { get; set; }

    /// <summary>
    /// Gets or sets the retrieval configuration.
    /// </summary>
    public RetrievalOptions? Retrieval { get; set; }

    /// <summary>
    /// Gets or sets the security configuration.
    /// </summary>
    public RagSecurityOptions? Security { get; set; }

    /// <summary>
    /// Gets or sets the performance configuration.
    /// </summary>
    public RagPerformanceOptions? Performance { get; set; }
}

/// <summary>
/// Configuration options for vector database providers.
/// </summary>
public class VectorDatabaseOptions
{
    /// <summary>
    /// Gets or sets the vector database provider name (e.g., "Pinecone", "Weaviate", "Chroma").
    /// </summary>
    public string Provider { get; set; } = "InMemory";

    /// <summary>
    /// Gets or sets the connection string or configuration for the vector database.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();

    /// <summary>
    /// Gets or sets the connection pooling options.
    /// </summary>
    public ConnectionPoolingOptions? ConnectionPooling { get; set; }

    /// <summary>
    /// Gets or sets the health check configuration.
    /// </summary>
    public HealthCheckOptions? HealthCheck { get; set; }
}

/// <summary>
/// Configuration options for embedding generation.
/// </summary>
public class EmbeddingOptions
{
    /// <summary>
    /// Gets or sets the embedding provider (e.g., "OpenAI", "Cohere", "HuggingFace").
    /// </summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>
    /// Gets or sets the embedding model to use.
    /// </summary>
    public string Model { get; set; } = "text-embedding-ada-002";

    /// <summary>
    /// Gets or sets the batch size for embedding generation.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets caching options for embeddings.
    /// </summary>
    public CachingOptions? Caching { get; set; }

    /// <summary>
    /// Gets or sets additional provider-specific configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Configuration options for document processing.
/// </summary>
public class DocumentProcessingOptions
{
    /// <summary>
    /// Gets or sets the chunking strategy (e.g., "Semantic", "FixedSize", "Sentence").
    /// </summary>
    public string ChunkingStrategy { get; set; } = "Semantic";

    /// <summary>
    /// Gets or sets the chunk size in characters.
    /// </summary>
    public int ChunkSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the overlap between chunks in characters.
    /// </summary>
    public int Overlap { get; set; } = 200;

    /// <summary>
    /// Gets or sets the supported document formats.
    /// </summary>
    public IEnumerable<string> SupportedFormats { get; set; } = new[] { "txt", "html", "md" };

    /// <summary>
    /// Gets or sets additional processing configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Configuration options for document retrieval.
/// </summary>
public class RetrievalOptions
{
    /// <summary>
    /// Gets or sets the maximum number of chunks to retrieve.
    /// </summary>
    public int TopK { get; set; } = 5;

    /// <summary>
    /// Gets or sets the minimum similarity threshold (0.0 to 1.0).
    /// </summary>
    public double SimilarityThreshold { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets whether to enable re-ranking of results.
    /// </summary>
    public bool ReRanking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable query enhancement.
    /// </summary>
    public bool QueryEnhancement { get; set; } = true;

    /// <summary>
    /// Gets or sets additional retrieval configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Configuration options for RAG security features.
/// </summary>
public class RagSecurityOptions
{
    /// <summary>
    /// Gets or sets whether to enable access control for documents.
    /// </summary>
    public bool AccessControl { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable content filtering.
    /// </summary>
    public bool ContentFiltering { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable audit logging.
    /// </summary>
    public bool AuditLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets additional security configuration.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Configuration options for RAG performance optimization.
/// </summary>
public class RagPerformanceOptions
{
    /// <summary>
    /// Gets or sets caching options for RAG operations.
    /// </summary>
    public CachingOptions? Caching { get; set; }

    /// <summary>
    /// Gets or sets monitoring options.
    /// </summary>
    public MonitoringOptions? Monitoring { get; set; }

    /// <summary>
    /// Gets or sets rate limiting options.
    /// </summary>
    public RateLimitingOptions? RateLimiting { get; set; }
}

/// <summary>
/// Configuration options for connection pooling.
/// </summary>
public class ConnectionPoolingOptions
{
    /// <summary>
    /// Gets or sets the maximum number of connections in the pool.
    /// </summary>
    public int MaxConnections { get; set; } = 10;

    /// <summary>
    /// Gets or sets the connection timeout in seconds.
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the idle timeout in seconds.
    /// </summary>
    public int IdleTimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Configuration options for health checks.
/// </summary>
public class HealthCheckOptions
{
    /// <summary>
    /// Gets or sets whether health checks are enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the health check interval in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the health check timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;
}

/// <summary>
/// Configuration options for caching.
/// </summary>
public class CachingOptions
{
    /// <summary>
    /// Gets or sets whether caching is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of cached items.
    /// </summary>
    public int MaxCacheSize { get; set; } = 1000;
}

/// <summary>
/// Configuration options for monitoring.
/// </summary>
public class MonitoringOptions
{
    /// <summary>
    /// Gets or sets whether monitoring is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the metrics collection interval in seconds.
    /// </summary>
    public int MetricsIntervalSeconds { get; set; } = 30;
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    /// Gets or sets whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of requests per window.
    /// </summary>
    public int RequestsPerWindow { get; set; } = 100;

    /// <summary>
    /// Gets or sets the time window in seconds.
    /// </summary>
    public int WindowSeconds { get; set; } = 60;
}