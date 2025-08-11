using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Failover
{
    public class FailoverTests
    {
        private readonly Mock<IChatModel> _mockPrimaryProvider;
        private readonly Mock<IChatModel> _mockFallbackProvider;
        private readonly Mock<ILogger<FailoverChatModel>> _mockLogger;
        private readonly FailoverChatModel _failoverChatModel;

        public FailoverTests()
        {
            _mockPrimaryProvider = new Mock<IChatModel>();
            _mockFallbackProvider = new Mock<IChatModel>();
            _mockLogger = new Mock<ILogger<FailoverChatModel>>();
            _failoverChatModel = new FailoverChatModel(_mockPrimaryProvider.Object, _mockFallbackProvider.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetResponseAsync_PrimarySucceeds_FallbackNotCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var expectedResponse = new ChatResponse("Response from primary", "model-1", "completed", new TokenUsage(10, 20));
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _failoverChatModel.GetResponseAsync(messages);

            // Assert
            Assert.Equal(expectedResponse, result);
            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatRequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetResponseAsync_PrimaryFailsWithRetriableError_FallbackCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var fallbackResponse = new ChatResponse("Response from fallback", "model-2", "completed", new TokenUsage(10, 20));
            var retriableException = new AiSdkException("Service unavailable", new Azure.RequestFailedException(503, "Service unavailable"));
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(retriableException);
            _mockFallbackProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ReturnsAsync(fallbackResponse);

            // Act
            var result = await _failoverChatModel.GetResponseAsync(messages);

            // Assert
            Assert.Equal(fallbackResponse, result);
            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_PrimaryFailsWithNonRetriableError_FallbackNotCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var nonRetriableException = new AiSdkConfigurationException("Invalid API key");
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(nonRetriableException);

            // Act & Assert
            await Assert.ThrowsAsync<AiSdkConfigurationException>(async () =>
            {
                await _failoverChatModel.GetResponseAsync(messages);
            });

            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatRequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetResponseAsync_PrimaryFailsWithRateLimitError_FallbackCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var fallbackResponse = new ChatResponse("Response from fallback", "model-2", "completed", new TokenUsage(10, 20));
            var rateLimitException = new AiSdkRateLimitException("Rate limit exceeded");
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(rateLimitException);
            _mockFallbackProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ReturnsAsync(fallbackResponse);

            // Act
            var result = await _failoverChatModel.GetResponseAsync(messages);

            // Assert
            Assert.Equal(fallbackResponse, result);
            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_PrimaryFailsWithTimeoutError_FallbackCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var fallbackResponse = new ChatResponse("Response from fallback", "model-2", "completed", new TokenUsage(10, 20));
            var timeoutException = new TimeoutException("Request timed out");
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(timeoutException);
            _mockFallbackProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ReturnsAsync(fallbackResponse);

            // Act
            var result = await _failoverChatModel.GetResponseAsync(messages);

            // Assert
            Assert.Equal(fallbackResponse, result);
            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_BothFail_ThrowsFallbackException()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var primaryException = new AiSdkException("Service unavailable", new Azure.RequestFailedException(503, "Service unavailable"));
            var fallbackException = new AiSdkException("Fallback also failed");
            
            _mockPrimaryProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(primaryException);
            _mockFallbackProvider.Setup(x => x.GetResponseAsync(messages, null, default))
                .ThrowsAsync(fallbackException);

            // Act & Assert
            await Assert.ThrowsAsync<AiSdkException>(async () =>
            {
                await _failoverChatModel.GetResponseAsync(messages);
            });

            _mockPrimaryProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
            _mockFallbackProvider.Verify(x => x.GetResponseAsync(messages, null, default), Times.Once);
        }

        [Fact]
        public async Task StreamResponseAsync_PrimarySucceeds_FallbackNotCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var primaryTokens = new[] { "Hello", " from", " primary" };
            
            _mockPrimaryProvider.Setup(x => x.StreamResponseAsync(messages, null, default))
                .Returns(primaryTokens.ToAsyncEnumerable());

            // Act
            var result = new List<string>();
            await foreach (var token in _failoverChatModel.StreamResponseAsync(messages))
            {
                result.Add(token);
            }

            // Assert
            Assert.Equal(primaryTokens, result);
            _mockPrimaryProvider.Verify(x => x.StreamResponseAsync(messages, null, default), Times.AtLeastOnce);
            _mockFallbackProvider.Verify(x => x.StreamResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatRequestOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task StreamResponseAsync_PrimaryFailsWithRetriableError_FallbackCalled()
        {
            // Arrange
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };
            var fallbackTokens = new[] { "Hello", " from", " fallback" };
            var retriableException = new AiSdkException("Service unavailable", new Azure.RequestFailedException(503, "Service unavailable"));
            
            _mockPrimaryProvider.Setup(x => x.StreamResponseAsync(messages, null, default))
                .Returns(ThrowAsync<string>(retriableException));
            _mockFallbackProvider.Setup(x => x.StreamResponseAsync(messages, null, default))
                .Returns(fallbackTokens.ToAsyncEnumerable());

            // Act
            var result = new List<string>();
            await foreach (var token in _failoverChatModel.StreamResponseAsync(messages))
            {
                result.Add(token);
            }

            // Assert
            Assert.Equal(fallbackTokens, result);
            _mockFallbackProvider.Verify(x => x.StreamResponseAsync(messages, null, default), Times.Once);
        }

        private static async IAsyncEnumerable<T> ThrowAsync<T>(Exception exception)
        {
            await Task.Yield();
            throw exception;
            #pragma warning disable CS0162 // Unreachable code detected
            yield break;
            #pragma warning restore CS0162 // Unreachable code detected
        }
    }

    // Extension method to convert arrays to IAsyncEnumerable for testing
    public static class TestExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                await Task.Yield();
                yield return item;
            }
        }
    }
}