using FluentAI.Abstractions.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// Provides caching capabilities for chat responses to improve performance.
    /// </summary>
    public interface IResponseCache
    {
        /// <summary>
        /// Gets a cached response for the given request.
        /// </summary>
        /// <param name="messages">The chat messages.</param>
        /// <param name="options">The request options.</param>
        /// <returns>The cached response if found, null otherwise.</returns>
        Task<ChatResponse?> GetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null);

        /// <summary>
        /// Stores a response in the cache.
        /// </summary>
        /// <param name="messages">The chat messages.</param>
        /// <param name="options">The request options.</param>
        /// <param name="response">The response to cache.</param>
        /// <param name="ttl">Time to live for the cached entry.</param>
        Task SetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options, ChatResponse response, TimeSpan? ttl = null);

        /// <summary>
        /// Removes expired entries from the cache.
        /// </summary>
        Task CleanupExpiredEntriesAsync();
    }
}