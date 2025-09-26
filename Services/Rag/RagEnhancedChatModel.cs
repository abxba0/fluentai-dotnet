using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Models.Rag;
using Microsoft.Extensions.Logging;
using RagRetrievalOptions = FluentAI.Abstractions.Models.Rag.RetrievalOptions;

namespace FluentAI.Services.Rag;

/// <summary>
/// A wrapper that enhances any chat model with RAG capabilities.
/// </summary>
public class RagEnhancedChatModel : IChatModelWithRag
{
    private readonly IChatModel _baseChatModel;
    private readonly IRagService _ragService;
    private readonly ILogger<RagEnhancedChatModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RagEnhancedChatModel"/> class.
    /// </summary>
    public RagEnhancedChatModel(
        IChatModel baseChatModel,
        IRagService ragService,
        ILogger<RagEnhancedChatModel> logger)
    {
        _baseChatModel = baseChatModel ?? throw new ArgumentNullException(nameof(baseChatModel));
        _ragService = ragService ?? throw new ArgumentNullException(nameof(ragService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public IRagService? RagService => _ragService;

    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, 
        ChatRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Delegate to base chat model for non-RAG requests
        return await _baseChatModel.GetResponseAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<string> StreamResponseAsync(
        IEnumerable<ChatMessage> messages, 
        ChatRequestOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Delegate to base chat model for non-RAG requests
        return _baseChatModel.StreamResponseAsync(messages, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ChatResponse> GetResponseWithContextAsync(
        IEnumerable<ChatMessage> messages, 
        RagContextOptions? ragOptions = null, 
        ChatRequestOptions? chatOptions = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Processing chat request with RAG context");

        try
        {
            ragOptions ??= new RagContextOptions();

            if (!ragOptions.AutoContextInjection)
            {
                // If auto-injection is disabled, just call the base model
                return await _baseChatModel.GetResponseAsync(messages, chatOptions, cancellationToken);
            }

            // Extract the user's latest message for context retrieval
            var userMessage = GetLatestUserMessage(messages);
            if (string.IsNullOrEmpty(userMessage))
            {
                _logger.LogWarning("No user message found for RAG context retrieval");
                return await _baseChatModel.GetResponseAsync(messages, chatOptions, cancellationToken);
            }

            // Retrieve relevant context
            var retrievalOptions = new RagRetrievalOptions
            {
                TopK = ragOptions.MaxContextChunks,
                SimilarityThreshold = ragOptions.SimilarityThreshold,
                Filters = ragOptions.ContextFilters,
                IncludeSources = ragOptions.IncludeCitations
            };

            var retrievalResult = await _ragService.RetrieveAsync(
                userMessage, 
                retrievalOptions, 
                cancellationToken);

            // Enhance messages with retrieved context
            var enhancedMessages = EnhanceMessagesWithContext(
                messages, 
                retrievalResult.Chunks, 
                ragOptions);

            // Get response from base model with enhanced context
            var response = await _baseChatModel.GetResponseAsync(
                enhancedMessages, 
                chatOptions, 
                cancellationToken);

            // Note: ChatResponse is a record without Metadata property in current FluentAI.NET
            // The metadata would need to be handled through additional response wrapping if needed

            _logger.LogDebug("RAG-enhanced response generated with {ChunkCount} context chunks", 
                retrievalResult.Chunks.Count());

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating RAG-enhanced response");
            
            // Fall back to base model on error
            return await _baseChatModel.GetResponseAsync(messages, chatOptions, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> StreamResponseWithContextAsync(
        IEnumerable<ChatMessage> messages,
        RagContextOptions? ragOptions = null,
        ChatRequestOptions? chatOptions = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Processing streaming chat request with RAG context");

        ragOptions ??= new RagContextOptions();

        if (!ragOptions.AutoContextInjection)
        {
            // If auto-injection is disabled, just call the base model
            await foreach (var token in _baseChatModel.StreamResponseAsync(messages, chatOptions, cancellationToken))
            {
                yield return token;
            }
            yield break;
        }

        // Extract the user's latest message for context retrieval
        var userMessage = GetLatestUserMessage(messages);
        if (string.IsNullOrEmpty(userMessage))
        {
            _logger.LogWarning("No user message found for RAG context retrieval");
            await foreach (var token in _baseChatModel.StreamResponseAsync(messages, chatOptions, cancellationToken))
            {
                yield return token;
            }
            yield break;
        }

        // Retrieve relevant context (this is outside the try block to avoid yield issues)
        RetrievalResult? retrievalResult = null;
        try
        {
            var retrievalOptions = new RagRetrievalOptions
            {
                TopK = ragOptions.MaxContextChunks,
                SimilarityThreshold = ragOptions.SimilarityThreshold,
                Filters = ragOptions.ContextFilters,
                IncludeSources = ragOptions.IncludeCitations
            };

            retrievalResult = await _ragService.RetrieveAsync(
                userMessage, 
                retrievalOptions, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving context for streaming response");
            // retrievalResult will remain null, handled below
        }

        // If retrieval failed, fall back to base model
        if (retrievalResult == null)
        {
            await foreach (var token in _baseChatModel.StreamResponseAsync(messages, chatOptions, cancellationToken))
            {
                yield return token;
            }
            yield break;
        }

        // Enhance messages with retrieved context
        var enhancedMessages = EnhanceMessagesWithContext(
            messages, 
            retrievalResult.Chunks, 
            ragOptions);

        // Stream response from base model with enhanced context
        await foreach (var token in _baseChatModel.StreamResponseAsync(
            enhancedMessages, 
            chatOptions, 
            cancellationToken))
        {
            yield return token;
        }

        _logger.LogDebug("RAG-enhanced streaming response completed with {ChunkCount} context chunks", 
            retrievalResult.Chunks.Count());
    }

    private static string GetLatestUserMessage(IEnumerable<ChatMessage> messages)
    {
        var userMessages = messages
            .Where(m => m.Role == ChatRole.User)
            .Select(m => m.Content)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToList();

        return userMessages.LastOrDefault() ?? string.Empty;
    }

    private static IEnumerable<ChatMessage> EnhanceMessagesWithContext(
        IEnumerable<ChatMessage> messages,
        IEnumerable<DocumentChunk> chunks,
        RagContextOptions ragOptions)
    {
        if (!chunks.Any())
        {
            return messages;
        }

        var contextContent = FormatContextContent(chunks, ragOptions.ContextTemplate);

        return ragOptions.InjectionStrategy switch
        {
            ContextInjectionStrategy.SystemMessage => InjectAsSystemMessage(messages, contextContent),
            ContextInjectionStrategy.AppendToUserMessage => AppendToUserMessage(messages, contextContent),
            ContextInjectionStrategy.SeparateContextMessage => InjectAsSeparateMessage(messages, contextContent),
            ContextInjectionStrategy.Automatic => InjectAsSystemMessage(messages, contextContent), // Default to system message
            _ => InjectAsSystemMessage(messages, contextContent)
        };
    }

    private static string FormatContextContent(IEnumerable<DocumentChunk> chunks, string? template)
    {
        if (!string.IsNullOrEmpty(template))
        {
            // Custom template formatting would go here
            // For now, use default formatting
        }

        var contextParts = chunks.Select(chunk =>
        {
            var source = chunk.Source?.Title ?? chunk.Source?.Url ?? chunk.DocumentId;
            return $"[Source: {source}]\n{chunk.Content}";
        });

        return string.Join("\n\n", contextParts);
    }

    private static IEnumerable<ChatMessage> InjectAsSystemMessage(
        IEnumerable<ChatMessage> messages,
        string contextContent)
    {
        var systemMessage = new ChatMessage(
            ChatRole.System,
            $"Use the following context to answer the user's question. If the context doesn't contain relevant information, say so clearly.\n\nContext:\n{contextContent}"
        );

        return new[] { systemMessage }.Concat(messages);
    }

    private static IEnumerable<ChatMessage> AppendToUserMessage(
        IEnumerable<ChatMessage> messages,
        string contextContent)
    {
        var messageList = messages.ToList();
        var lastUserMessageIndex = -1;

        // Find the last user message
        for (int i = messageList.Count - 1; i >= 0; i--)
        {
            if (messageList[i].Role == ChatRole.User)
            {
                lastUserMessageIndex = i;
                break;
            }
        }

        if (lastUserMessageIndex >= 0)
        {
            var originalMessage = messageList[lastUserMessageIndex];
            var enhancedContent = $"{originalMessage.Content}\n\nRelevant context:\n{contextContent}";
            
            messageList[lastUserMessageIndex] = new ChatMessage(originalMessage.Role, enhancedContent)
            {
                ToolCalls = originalMessage.ToolCalls,
                ToolCallId = originalMessage.ToolCallId
            };
        }

        return messageList;
    }

    private static IEnumerable<ChatMessage> InjectAsSeparateMessage(
        IEnumerable<ChatMessage> messages,
        string contextContent)
    {
        var messageList = messages.ToList();
        var contextMessage = new ChatMessage(
            ChatRole.User,
            $"Here is some relevant context that might help answer my question:\n\n{contextContent}"
        );

        // Insert context message before the last user message
        if (messageList.Count > 0 && messageList.Last().Role == ChatRole.User)
        {
            messageList.Insert(messageList.Count - 1, contextMessage);
        }
        else
        {
            messageList.Add(contextMessage);
        }

        return messageList;
    }

    private static IEnumerable<Citation> GenerateCitations(IEnumerable<DocumentChunk> chunks)
    {
        return chunks.Select(chunk => new Citation
        {
            DocumentId = chunk.DocumentId,
            Title = chunk.Source?.Title ?? chunk.DocumentId,
            Url = chunk.Source?.Url,
            PageReference = chunk.ChunkIndex.ToString(),
            RelevanceScore = chunk.RelevanceScore
        });
    }
}