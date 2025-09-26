using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Models.Rag;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentAI.Services.Rag;

/// <summary>
/// Default implementation of the RAG service.
/// </summary>
public class DefaultRagService : IRagService
{
    private readonly IVectorDatabase _vectorDatabase;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly IDocumentProcessor _documentProcessor;
    private readonly IChatModel _chatModel;
    private readonly ILogger<DefaultRagService> _logger;
    private readonly RagOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultRagService"/> class.
    /// </summary>
    public DefaultRagService(
        IVectorDatabase vectorDatabase,
        IEmbeddingGenerator embeddingGenerator,
        IDocumentProcessor documentProcessor,
        IChatModel chatModel,
        ILogger<DefaultRagService> logger,
        IOptions<RagOptions> options)
    {
        _vectorDatabase = vectorDatabase ?? throw new ArgumentNullException(nameof(vectorDatabase));
        _embeddingGenerator = embeddingGenerator ?? throw new ArgumentNullException(nameof(embeddingGenerator));
        _documentProcessor = documentProcessor ?? throw new ArgumentNullException(nameof(documentProcessor));
        _chatModel = chatModel ?? throw new ArgumentNullException(nameof(chatModel));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async Task<RagResponse> QueryAsync(RagRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing RAG query: {Query}", request.Query);
        var startTime = DateTimeOffset.UtcNow;
        
        try
        {
            // Step 1: Retrieve relevant context
            var retrievalResult = await RetrieveAsync(
                request.Query, 
                request.RetrievalOptions, 
                cancellationToken);

            // Step 2: Enhance messages with context
            var enhancedMessages = await EnhanceMessagesWithContext(
                request.Messages, 
                retrievalResult.Chunks, 
                cancellationToken);

            // Step 3: Generate response using chat model
            var chatResponse = await _chatModel.GetResponseAsync(
                enhancedMessages, 
                request.GenerationOptions, 
                cancellationToken);

            // Step 4: Build RAG response
            var ragResponse = new RagResponse
            {
                Content = chatResponse.Content,
                RetrievedContext = retrievalResult.Chunks,
                ConfidenceScore = CalculateConfidenceScore(retrievalResult.Chunks),
                ModelUsed = chatResponse.ModelId,
                Provider = "DefaultRagService",
                TokenUsage = chatResponse.Usage,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Citations = GenerateCitations(retrievalResult.Chunks),
                Metadata = new Dictionary<string, object>
                {
                    ["RetrievalTime"] = retrievalResult.ProcessingTime,
                    ["ChunkCount"] = retrievalResult.Chunks.Count()
                }
            };

            _logger.LogInformation("RAG query completed successfully in {ProcessingTime}ms", 
                ragResponse.ProcessingTime.TotalMilliseconds);

            return ragResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing RAG query: {Query}", request.Query);
            throw;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<RagStreamToken> StreamQueryAsync(
        RagRequest request, 
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing streaming RAG query: {Query}", request.Query);

        // First retrieve context
        var retrievalResult = await RetrieveAsync(
            request.Query, 
            request.RetrievalOptions, 
            cancellationToken);

        // Yield context information first
        yield return new RagStreamToken
        {
            Content = $"Retrieved {retrievalResult.Chunks.Count()} relevant context chunks",
            TokenType = StreamTokenType.Context,
            Metadata = new Dictionary<string, object>
            {
                ["ChunkCount"] = retrievalResult.Chunks.Count(),
                ["RetrievalTime"] = retrievalResult.ProcessingTime.TotalMilliseconds
            }
        };

        // Enhance messages with context
        var enhancedMessages = await EnhanceMessagesWithContext(
            request.Messages, 
            retrievalResult.Chunks, 
            cancellationToken);

        // Stream the chat response
        await foreach (var token in _chatModel.StreamResponseAsync(
            enhancedMessages, 
            request.GenerationOptions, 
            cancellationToken))
        {
            yield return new RagStreamToken
            {
                Content = token,
                TokenType = StreamTokenType.Content
            };
        }

        // Yield final completion token with citations
        yield return new RagStreamToken
        {
            Content = "",
            TokenType = StreamTokenType.Citation,
            IsComplete = true,
            Metadata = new Dictionary<string, object>
            {
                ["Citations"] = GenerateCitations(retrievalResult.Chunks)
            }
        };
    }

    /// <inheritdoc />
    public async Task<IndexingResult> IndexDocumentAsync(
        DocumentIndexRequest request, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Indexing document: {DocumentId}", request.Id);
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Step 1: Process the document
            var processedDocument = await _documentProcessor.ProcessAsync(
                request.Document, 
                request.ProcessingOptions, 
                cancellationToken);

            // Step 2: Chunk the document
            var chunks = await _documentProcessor.ChunkDocumentAsync(
                processedDocument, 
                request.ChunkingOptions, 
                cancellationToken);

            var chunkList = chunks.ToList();

            // Step 3: Generate embeddings for chunks
            var texts = chunkList.Select(c => c.Content).ToList();
            var embeddingResult = await _embeddingGenerator.GenerateEmbeddingsAsync(
                texts, 
                request.EmbeddingOptions, 
                cancellationToken);

            // Step 4: Create vectors for indexing
            var vectors = chunkList
                .Zip(embeddingResult.Embeddings, (chunk, embedding) => new Vector
                {
                    Id = chunk.Id,
                    Values = embedding.Vector,
                    Metadata = new Dictionary<string, object>
                    {
                        ["DocumentId"] = request.Id,
                        ["Content"] = chunk.Content,
                        ["ChunkIndex"] = chunk.ChunkIndex,
                        ["Title"] = chunk.Title ?? "",
                        ["Source"] = chunk.Source?.Url ?? ""
                    }
                })
                .ToList();

            // Step 5: Upsert vectors to database
            var indexResult = await _vectorDatabase.UpsertAsync(vectors, cancellationToken);

            var result = new IndexingResult
            {
                Success = indexResult.Success,
                IndexedCount = indexResult.Success ? 1 : 0,
                ChunkCount = chunkList.Count,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Errors = indexResult.Errors
            };

            _logger.LogInformation("Document indexing completed: {DocumentId}, Success: {Success}, Chunks: {ChunkCount}", 
                request.Id, result.Success, result.ChunkCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error indexing document: {DocumentId}", request.Id);
            return new IndexingResult
            {
                Success = false,
                Errors = new[] { ex.Message },
                ProcessingTime = DateTimeOffset.UtcNow - startTime
            };
        }
    }

    /// <inheritdoc />
    public async Task<IndexingResult> IndexDocumentsAsync(
        IEnumerable<DocumentIndexRequest> requests, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Batch indexing {DocumentCount} documents", requests.Count());
        var startTime = DateTimeOffset.UtcNow;

        var results = new List<IndexingResult>();
        var errors = new List<string>();

        foreach (var request in requests)
        {
            try
            {
                var result = await IndexDocumentAsync(request, cancellationToken);
                results.Add(result);
                
                if (!result.Success)
                {
                    errors.AddRange(result.Errors);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in batch indexing document: {DocumentId}", request.Id);
                errors.Add($"Document {request.Id}: {ex.Message}");
                results.Add(new IndexingResult { Success = false, Errors = new[] { ex.Message } });
            }
        }

        return new IndexingResult
        {
            Success = results.All(r => r.Success),
            IndexedCount = results.Count(r => r.Success),
            ChunkCount = results.Sum(r => r.ChunkCount),
            ProcessingTime = DateTimeOffset.UtcNow - startTime,
            Errors = errors
        };
    }

    /// <inheritdoc />
    public async Task<RetrievalResult> RetrieveAsync(
        string query, 
        FluentAI.Abstractions.Models.Rag.RetrievalOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving context for query: {Query}", query);
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            options ??= _options.Retrieval != null ? new FluentAI.Abstractions.Models.Rag.RetrievalOptions
            {
                TopK = _options.Retrieval.TopK,
                SimilarityThreshold = _options.Retrieval.SimilarityThreshold,
                ReRanking = _options.Retrieval.ReRanking,
                QueryEnhancement = _options.Retrieval.QueryEnhancement
            } : new FluentAI.Abstractions.Models.Rag.RetrievalOptions();

            // Generate query embedding
            var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(
                query, 
                null, 
                cancellationToken);

            // Search vector database
            var searchRequest = new VectorSearchRequest
            {
                QueryVector = queryEmbedding.Embeddings.First().Vector,
                TopK = options.TopK,
                MinScore = options.SimilarityThreshold,
                Filters = options.Filters,
                IncludeMetadata = true
            };

            var searchResult = await _vectorDatabase.SearchAsync(searchRequest, cancellationToken);

            // Convert to document chunks
            var chunks = searchResult.Matches.Select(match => new DocumentChunk
            {
                Id = match.Id,
                DocumentId = match.Metadata.GetValueOrDefault("DocumentId")?.ToString() ?? "",
                Content = match.Metadata.GetValueOrDefault("Content")?.ToString() ?? "",
                Title = match.Metadata.GetValueOrDefault("Title")?.ToString(),
                RelevanceScore = match.Score,
                ChunkIndex = int.TryParse(match.Metadata.GetValueOrDefault("ChunkIndex")?.ToString(), out var idx) ? idx : 0,
                Source = new SourceInfo
                {
                    Url = match.Metadata.GetValueOrDefault("Source")?.ToString()
                }
            }).ToList();

            return new RetrievalResult
            {
                Chunks = chunks,
                Query = query,
                ProcessingTime = DateTimeOffset.UtcNow - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["SearchStrategy"] = options.SearchStrategy.ToString(),
                    ["TotalMatches"] = chunks.Count
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for query: {Query}", query);
            throw;
        }
    }

    private async Task<IEnumerable<ChatMessage>> EnhanceMessagesWithContext(
        IEnumerable<ChatMessage> messages, 
        IEnumerable<DocumentChunk> chunks, 
        CancellationToken cancellationToken)
    {
        if (!chunks.Any())
        {
            return messages;
        }

        var contextContent = string.Join("\n\n", chunks.Select(c => 
            $"[Source: {c.Source?.Title ?? c.DocumentId}]\n{c.Content}"));

        var systemMessage = new ChatMessage(
            ChatRole.System,
            $"Use the following context to answer the user's question. If the context doesn't contain relevant information, say so.\n\nContext:\n{contextContent}"
        );

        return new[] { systemMessage }.Concat(messages);
    }

    private static double CalculateConfidenceScore(IEnumerable<DocumentChunk> chunks)
    {
        if (!chunks.Any())
        {
            return 0.0;
        }

        var scores = chunks.Select(c => c.RelevanceScore);
        return scores.Average();
    }

    private static IEnumerable<Citation> GenerateCitations(IEnumerable<DocumentChunk> chunks)
    {
        return chunks.Select(chunk => new Citation
        {
            DocumentId = chunk.DocumentId,
            Title = chunk.Source?.Title ?? chunk.DocumentId,
            Url = chunk.Source?.Url,
            RelevanceScore = chunk.RelevanceScore
        });
    }
}