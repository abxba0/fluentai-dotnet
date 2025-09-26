using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Models.Rag;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace FluentAI.Services.Rag;

/// <summary>
/// OpenAI implementation of embedding generation.
/// </summary>
public class OpenAiEmbeddingGenerator : IEmbeddingGenerator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAiEmbeddingGenerator> _logger;
    private readonly FluentAI.Configuration.EmbeddingOptions _options;
    private readonly OpenAiOptions _openAiOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiEmbeddingGenerator"/> class.
    /// </summary>
    public OpenAiEmbeddingGenerator(
        HttpClient httpClient,
        ILogger<OpenAiEmbeddingGenerator> logger,
        IOptions<FluentAI.Configuration.EmbeddingOptions> options,
        IOptions<OpenAiOptions> openAiOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _openAiOptions = openAiOptions?.Value ?? throw new ArgumentNullException(nameof(openAiOptions));

        ConfigureHttpClient();
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> GenerateEmbeddingsAsync(
        IEnumerable<string> texts, 
        EmbeddingRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var textList = texts.ToList();
        _logger.LogDebug("Generating embeddings for {TextCount} texts", textList.Count);

        var startTime = DateTimeOffset.UtcNow;
        var batchSize = options?.BatchSize ?? _options.BatchSize;
        var model = options?.Model ?? _options.Model;

        try
        {
            var allEmbeddings = new List<Embedding>();
            var totalTokens = 0;

            // Process in batches
            for (int i = 0; i < textList.Count; i += batchSize)
            {
                var batch = textList.Skip(i).Take(batchSize).ToList();
                var batchResult = await GenerateBatchEmbeddings(batch, model, cancellationToken);
                
                // Update indices to be global across batches
                foreach (var embedding in batchResult.Embeddings)
                {
                    embedding.Index = allEmbeddings.Count;
                    allEmbeddings.Add(embedding);
                }

                if (batchResult.TokenUsage != null)
                {
                    totalTokens += batchResult.TokenUsage.TotalTokens;
                }
            }

            var result = new EmbeddingResult
            {
                Embeddings = allEmbeddings,
                ModelUsed = model,
                Provider = "OpenAI",
                TokenUsage = new TokenUsage(totalTokens, 0),
                ProcessingTime = DateTimeOffset.UtcNow - startTime
            };

            _logger.LogDebug("Generated {EmbeddingCount} embeddings in {ProcessingTime}ms using {TotalTokens} tokens", 
                allEmbeddings.Count, result.ProcessingTime.TotalMilliseconds, totalTokens);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embeddings for {TextCount} texts", textList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<EmbeddingResult> GenerateEmbeddingAsync(
        string text, 
        EmbeddingRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        return await GenerateEmbeddingsAsync(new[] { text }, options, cancellationToken);
    }

    /// <inheritdoc />
    public ModelInfo GetModelInfo()
    {
        var model = _options.Model;
        
        // Return model information based on known OpenAI models
        return model switch
        {
            "text-embedding-ada-002" => new ModelInfo
            {
                Name = model,
                Dimensions = 1536,
                MaxInputTokens = 8191,
                Provider = "OpenAI",
                Capabilities = new Dictionary<string, object>
                {
                    ["SupportsLargeText"] = true,
                    ["CostPerToken"] = 0.0001
                }
            },
            "text-embedding-3-small" => new ModelInfo
            {
                Name = model,
                Dimensions = 1536,
                MaxInputTokens = 8191,
                Provider = "OpenAI",
                Capabilities = new Dictionary<string, object>
                {
                    ["SupportsLargeText"] = true,
                    ["CostPerToken"] = 0.00002
                }
            },
            "text-embedding-3-large" => new ModelInfo
            {
                Name = model,
                Dimensions = 3072,
                MaxInputTokens = 8191,
                Provider = "OpenAI",
                Capabilities = new Dictionary<string, object>
                {
                    ["SupportsLargeText"] = true,
                    ["CostPerToken"] = 0.00013
                }
            },
            _ => new ModelInfo
            {
                Name = model,
                Dimensions = 1536, // Default assumption
                MaxInputTokens = 8191,
                Provider = "OpenAI"
            }
        };
    }

    private void ConfigureHttpClient()
    {
        var apiKey = _openAiOptions.ApiKey ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured");
        }

        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var baseUrl = _openAiOptions.IsAzureOpenAI && !string.IsNullOrEmpty(_openAiOptions.Endpoint)
            ? _openAiOptions.Endpoint
            : "https://api.openai.com";

        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    private async Task<EmbeddingResult> GenerateBatchEmbeddings(
        List<string> texts, 
        string model, 
        CancellationToken cancellationToken)
    {
        var request = new
        {
            input = texts,
            model = model,
            encoding_format = "float"
        };

        var endpoint = _openAiOptions.IsAzureOpenAI 
            ? $"/openai/deployments/{model}/embeddings?api-version=2023-05-15"
            : "/v1/embeddings";

        var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonResponse = JsonDocument.Parse(responseContent);

        var embeddings = new List<Embedding>();
        var dataArray = jsonResponse.RootElement.GetProperty("data");

        foreach (var item in dataArray.EnumerateArray())
        {
            var embeddingArray = item.GetProperty("embedding");
            var vector = new float[embeddingArray.GetArrayLength()];
            
            int i = 0;
            foreach (var value in embeddingArray.EnumerateArray())
            {
                vector[i++] = value.GetSingle();
            }

            var index = item.GetProperty("index").GetInt32();
            embeddings.Add(new Embedding
            {
                Text = texts[index],
                Vector = vector,
                Index = index
            });
        }

        // Parse usage information
        TokenUsage? tokenUsage = null;
        if (jsonResponse.RootElement.TryGetProperty("usage", out var usageElement))
        {
            var promptTokens = usageElement.GetProperty("prompt_tokens").GetInt32();
            tokenUsage = new TokenUsage(promptTokens, 0);
        }

        return new EmbeddingResult
        {
            Embeddings = embeddings.OrderBy(e => e.Index),
            ModelUsed = model,
            Provider = "OpenAI",
            TokenUsage = tokenUsage
        };
    }
}