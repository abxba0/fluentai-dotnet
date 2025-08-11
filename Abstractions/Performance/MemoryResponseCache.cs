using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// In-memory implementation of response cache for chat responses.
    /// </summary>
    public class MemoryResponseCache : IResponseCache
    {
        private readonly ILogger<MemoryResponseCache> _logger;
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _defaultTtl;

        public MemoryResponseCache(ILogger<MemoryResponseCache> logger, TimeSpan? defaultTtl = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(30);
            
            // Run cleanup every 10 minutes
            _cleanupTimer = new Timer(async _ => await CleanupExpiredEntriesAsync(), null, 
                TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(10));
        }

        public Task<ChatResponse?> GetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null)
        {
            var key = GenerateCacheKey(messages, options);
            
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt > DateTimeOffset.UtcNow)
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", key[..Math.Min(key.Length, 16)] + "...");
                    return Task.FromResult<ChatResponse?>(entry.Response);
                }
                else
                {
                    // Remove expired entry
                    _cache.TryRemove(key, out _);
                    _logger.LogDebug("Cache entry expired and removed for key: {CacheKey}", key[..Math.Min(key.Length, 16)] + "...");
                }
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", key[..Math.Min(key.Length, 16)] + "...");
            return Task.FromResult<ChatResponse?>(null);
        }

        public Task SetAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options, ChatResponse response, TimeSpan? ttl = null)
        {
            var key = GenerateCacheKey(messages, options);
            var effectiveTtl = ttl ?? _defaultTtl;
            var expiresAt = DateTimeOffset.UtcNow.Add(effectiveTtl);

            var entry = new CacheEntry(response, expiresAt);
            _cache.AddOrUpdate(key, entry, (_, _) => entry);

            _logger.LogDebug("Cached response for key: {CacheKey}, expires at: {ExpiresAt}", 
                key[..Math.Min(key.Length, 16)] + "...", expiresAt);

            return Task.CompletedTask;
        }

        public Task CleanupExpiredEntriesAsync()
        {
            var now = DateTimeOffset.UtcNow;
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.ExpiresAt <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {ExpiredCount} expired cache entries", expiredKeys.Count);
            }

            return Task.CompletedTask;
        }

        private static string GenerateCacheKey(IEnumerable<ChatMessage> messages, ChatRequestOptions? options)
        {
            // Create a deterministic hash of the messages and options
            var keyData = new
            {
                Messages = messages.Select(m => new { m.Role, m.Content }).ToArray(),
                Options = options
            };

            var json = JsonSerializer.Serialize(keyData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
            return Convert.ToBase64String(hash);
        }

        private record CacheEntry(ChatResponse Response, DateTimeOffset ExpiresAt);

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
        }
    }
}