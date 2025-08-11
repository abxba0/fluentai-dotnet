using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using FluentAI.Configuration;
using FluentAI.Providers.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.RateLimiting
{
    public class RateLimitingTests
    {
        private readonly Mock<ILogger<OpenAiChatModel>> _mockLogger;
        private readonly Mock<IOptionsMonitor<OpenAiOptions>> _mockOptionsMonitor;

        public RateLimitingTests()
        {
            _mockLogger = new Mock<ILogger<OpenAiChatModel>>();
            _mockOptionsMonitor = new Mock<IOptionsMonitor<OpenAiOptions>>();
        }

        [Fact]
        public async Task RateLimit_WithinLimit_AllowsRequests()
        {
            // Arrange
            var options = new OpenAiOptions
            {
                ApiKey = "test-key",
                Model = "gpt-3.5-turbo",
                PermitLimit = 5,
                WindowInSeconds = 10
            };
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(options);

            var chatModel = new OpenAiChatModel(_mockOptionsMonitor.Object, _mockLogger.Object);
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };

            // This test verifies that the rate limiter is created but doesn't make actual API calls
            // since we're testing the rate limiting logic in isolation
            
            // Act & Assert - Should not throw for rate limiting setup
            // The actual API call will fail due to no real API key, but rate limiting should pass
            try
            {
                await chatModel.GetResponseAsync(messages);
            }
            catch (AiSdkRateLimitException)
            {
                // Should not reach here with first request
                Assert.True(false, "Rate limiting should not trigger on first request");
            }
            catch (AiSdkException)
            {
                // Expected to fail on API call, but not on rate limiting
            }
        }

        [Fact]
        public async Task RateLimit_ExceedsLimit_ThrowsRateLimitException()
        {
            // Arrange
            var options = new OpenAiOptions
            {
                ApiKey = "test-key",
                Model = "gpt-3.5-turbo",
                PermitLimit = 2,
                WindowInSeconds = 60 // Large window to ensure we hit the limit
            };
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(options);

            var chatModel = new OpenAiChatModel(_mockOptionsMonitor.Object, _mockLogger.Object);
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };

            // Act - Make requests up to the limit
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    await chatModel.GetResponseAsync(messages);
                }
                catch (AiSdkException)
                {
                    // Expected to fail on API call, but not on rate limiting
                }
            }

            // Act & Assert - The next request should be rate limited
            await Assert.ThrowsAsync<AiSdkRateLimitException>(async () =>
            {
                await chatModel.GetResponseAsync(messages);
            });
        }

        [Fact]
        public async Task RateLimit_DisabledWhenNotConfigured_AllowsUnlimitedRequests()
        {
            // Arrange
            var options = new OpenAiOptions
            {
                ApiKey = "test-key",
                Model = "gpt-3.5-turbo"
                // No PermitLimit or WindowInSeconds configured
            };
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(options);

            var chatModel = new OpenAiChatModel(_mockOptionsMonitor.Object, _mockLogger.Object);
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };

            // Act & Assert - Should not throw for rate limiting
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    await chatModel.GetResponseAsync(messages);
                }
                catch (AiSdkRateLimitException)
                {
                    Assert.True(false, $"Rate limiting should be disabled but was triggered on request {i + 1}");
                }
                catch (AiSdkException)
                {
                    // Expected to fail on API call, but not on rate limiting
                }
            }
        }

        [Fact]
        public async Task RateLimit_StreamingMethod_AppliesRateLimit()
        {
            // Arrange
            var options = new OpenAiOptions
            {
                ApiKey = "test-key",
                Model = "gpt-3.5-turbo",
                PermitLimit = 1,
                WindowInSeconds = 60
            };
            _mockOptionsMonitor.Setup(x => x.CurrentValue).Returns(options);

            var chatModel = new OpenAiChatModel(_mockOptionsMonitor.Object, _mockLogger.Object);
            var messages = new[] { new ChatMessage(ChatRole.User, "Hello") };

            // Act - First streaming request should work (rate limiting wise)
            try
            {
                await foreach (var token in chatModel.StreamResponseAsync(messages))
                {
                    // Will fail on API call, but rate limiting should pass
                    break;
                }
            }
            catch (AiSdkException)
            {
                // Expected to fail on API call
            }

            // Act & Assert - Second streaming request should be rate limited
            await Assert.ThrowsAsync<AiSdkRateLimitException>(async () =>
            {
                await foreach (var token in chatModel.StreamResponseAsync(messages))
                {
                    break;
                }
            });
        }
    }
}