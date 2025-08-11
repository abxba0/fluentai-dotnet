using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Performance;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Performance
{
    public class MemoryResponseCacheTests
    {
        private readonly MemoryResponseCache _cache;
        private readonly Mock<ILogger<MemoryResponseCache>> _mockLogger;

        public MemoryResponseCacheTests()
        {
            _mockLogger = new Mock<ILogger<MemoryResponseCache>>();
            _cache = new MemoryResponseCache(_mockLogger.Object, TimeSpan.FromMinutes(5));
        }

        [Fact]
        public async Task GetAsync_ForNonExistentKey_ShouldReturnNull()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };

            // Act
            var result = await _cache.GetAsync(messages);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SetAsync_ThenGetAsync_ShouldReturnCachedResponse()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var response = new ChatResponse("Hi there!", "test-model", "completed", new TokenUsage(5, 10));

            // Act
            await _cache.SetAsync(messages, null, response);
            var cachedResponse = await _cache.GetAsync(messages);

            // Assert
            Assert.NotNull(cachedResponse);
            Assert.Equal(response.Content, cachedResponse.Content);
            Assert.Equal(response.ModelId, cachedResponse.ModelId);
        }

        [Fact]
        public async Task GetAsync_AfterExpiration_ShouldReturnNull()
        {
            // Arrange
            var shortTtlCache = new MemoryResponseCache(_mockLogger.Object, TimeSpan.FromMilliseconds(50));
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var response = new ChatResponse("Hi there!", "test-model", "completed", new TokenUsage(5, 10));

            // Act
            await shortTtlCache.SetAsync(messages, null, response, TimeSpan.FromMilliseconds(10));
            await Task.Delay(20); // Wait for expiration
            var cachedResponse = await shortTtlCache.GetAsync(messages);

            // Assert
            Assert.Null(cachedResponse);
        }

        [Fact]
        public async Task DifferentMessages_ShouldHaveDifferentCacheKeys()
        {
            // Arrange
            var messages1 = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var messages2 = new[] { new ChatMessage(ChatRole.User, "Hi") };
            var response1 = new ChatResponse("Response 1", "test-model", "completed", new TokenUsage(5, 10));
            var response2 = new ChatResponse("Response 2", "test-model", "completed", new TokenUsage(3, 8));

            // Act
            await _cache.SetAsync(messages1, null, response1);
            await _cache.SetAsync(messages2, null, response2);

            var cached1 = await _cache.GetAsync(messages1);
            var cached2 = await _cache.GetAsync(messages2);

            // Assert
            Assert.NotNull(cached1);
            Assert.NotNull(cached2);
            Assert.Equal("Response 1", cached1.Content);
            Assert.Equal("Response 2", cached2.Content);
        }

        [Fact]
        public async Task CleanupExpiredEntriesAsync_ShouldRemoveExpiredEntries()
        {
            // Arrange
            var shortTtlCache = new MemoryResponseCache(_mockLogger.Object, TimeSpan.FromMilliseconds(10));
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var response = new ChatResponse("Hi there!", "test-model", "completed", new TokenUsage(5, 10));

            // Act
            await shortTtlCache.SetAsync(messages, null, response, TimeSpan.FromMilliseconds(10));
            await Task.Delay(20); // Wait for expiration
            await shortTtlCache.CleanupExpiredEntriesAsync();
            var cachedResponse = await shortTtlCache.GetAsync(messages);

            // Assert
            Assert.Null(cachedResponse);
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Act & Assert
            var exception = Record.Exception(() => _cache.Dispose());
            Assert.Null(exception);
        }
    }
}