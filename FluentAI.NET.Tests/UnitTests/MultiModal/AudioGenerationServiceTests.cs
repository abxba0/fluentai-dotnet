using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Services;
using FluentAI.Configuration;
using FluentAI.Providers.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MultiModal
{
    /// <summary>
    /// Unit tests for audio generation service implementations.
    /// </summary>
    public class AudioGenerationServiceTests
    {
        private readonly Mock<ILogger<OpenAiAudioGenerationService>> _mockLogger;
        private readonly Mock<IOptionsMonitor<OpenAiOptions>> _mockOptions;
        private readonly OpenAiOptions _testOptions;

        public AudioGenerationServiceTests()
        {
            _mockLogger = new Mock<ILogger<OpenAiAudioGenerationService>>();
            _mockOptions = new Mock<IOptionsMonitor<OpenAiOptions>>();
            _testOptions = new OpenAiOptions
            {
                ApiKey = "test-api-key",
                Model = "tts-1"
            };
            _mockOptions.Setup(o => o.CurrentValue).Returns(_testOptions);
        }

        [Fact]
        public void OpenAiAudioGenerationService_CanBeInstantiated()
        {
            // Act
            var service = new OpenAiAudioGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.Equal("OpenAI", service.ProviderName);
            Assert.Equal("tts-1", service.DefaultModelName);
        }

        [Fact]
        public async Task GenerateAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiAudioGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.GenerateAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task GenerateAsync_WithEmptyText_ThrowsArgumentException()
        {
            // Arrange
            var service = new OpenAiAudioGenerationService(_mockOptions.Object, _mockLogger.Object);
            var request = new AudioGenerationRequest { Text = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GenerateAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfig_Succeeds()
        {
            // Arrange
            var service = new OpenAiAudioGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert - should not throw
            await service.ValidateConfigurationAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithMissingApiKey_ThrowsAiSdkConfigurationException()
        {
            // Arrange
            var invalidOptions = new OpenAiOptions { ApiKey = "" };
            _mockOptions.Setup(o => o.CurrentValue).Returns(invalidOptions);
            var service = new OpenAiAudioGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FluentAI.Abstractions.Exceptions.AiSdkConfigurationException>(() =>
                service.ValidateConfigurationAsync(CancellationToken.None));
        }

        [Fact]
        public void AudioGenerationRequest_CanBeCreated()
        {
            // Act
            var request = new AudioGenerationRequest
            {
                Text = "Hello, world!",
                Voice = "alloy",
                Speed = 1.0f,
                ResponseFormat = "mp3",
                VoiceParameters = new VoiceParameters
                {
                    Pitch = 0.5f,
                    Rate = 1.0f,
                    Volume = 0.8f,
                    Emphasis = 1.2f
                }
            };

            // Assert
            Assert.Equal("Hello, world!", request.Text);
            Assert.Equal("alloy", request.Voice);
            Assert.Equal(1.0f, request.Speed);
            Assert.Equal("mp3", request.ResponseFormat);
            Assert.NotNull(request.VoiceParameters);
            Assert.Equal(0.5f, request.VoiceParameters.Pitch);
            Assert.Equal(1.0f, request.VoiceParameters.Rate);
            Assert.Equal(0.8f, request.VoiceParameters.Volume);
            Assert.Equal(1.2f, request.VoiceParameters.Emphasis);
        }

        [Fact]
        public void AudioGenerationRequest_WithDifferentVoices_CreatesCorrectly()
        {
            // Arrange & Act
            var voices = new[] { "alloy", "echo", "fable", "onyx", "nova", "shimmer" };
            
            foreach (var voice in voices)
            {
                var request = new AudioGenerationRequest
                {
                    Text = "Test",
                    Voice = voice
                };

                // Assert
                Assert.Equal(voice, request.Voice);
            }
        }

        [Fact]
        public void AudioGenerationRequest_WithDifferentFormats_CreatesCorrectly()
        {
            // Arrange & Act
            var formats = new[] { "mp3", "opus", "aac", "flac" };
            
            foreach (var format in formats)
            {
                var request = new AudioGenerationRequest
                {
                    Text = "Test",
                    ResponseFormat = format
                };

                // Assert
                Assert.Equal(format, request.ResponseFormat);
            }
        }

        [Fact]
        public void AudioGenerationResponse_CanBeCreated()
        {
            // Act
            var response = new AudioGenerationResponse
            {
                AudioData = new byte[] { 1, 2, 3, 4, 5 },
                ContentType = "audio/mpeg",
                Duration = 5.5,
                SampleRate = 44100,
                Voice = "alloy",
                ModelUsed = "tts-1",
                Provider = "OpenAI"
            };

            // Assert
            Assert.NotNull(response.AudioData);
            Assert.Equal(5, response.AudioData.Length);
            Assert.Equal("audio/mpeg", response.ContentType);
            Assert.Equal(5.5, response.Duration);
            Assert.Equal(44100, response.SampleRate);
            Assert.Equal("alloy", response.Voice);
            Assert.Equal("tts-1", response.ModelUsed);
            Assert.Equal("OpenAI", response.Provider);
        }

        [Fact]
        public void VoiceParameters_CanBeCreated()
        {
            // Act
            var params_ = new VoiceParameters
            {
                Pitch = -2.5f,
                Rate = 1.5f,
                Volume = 0.7f,
                Emphasis = 1.3f
            };

            // Assert
            Assert.Equal(-2.5f, params_.Pitch);
            Assert.Equal(1.5f, params_.Rate);
            Assert.Equal(0.7f, params_.Volume);
            Assert.Equal(1.3f, params_.Emphasis);
        }

        [Fact]
        public void BaseService_ImplementsIAudioGenerationService()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<AudioGenerationService>>();
            var service = new AudioGenerationService(baseLogger.Object);

            // Assert
            Assert.IsAssignableFrom<IAudioGenerationService>(service);
        }

        [Fact]
        public async Task BaseService_GenerateAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<AudioGenerationService>>();
            var service = new AudioGenerationService(baseLogger.Object);
            var request = new AudioGenerationRequest { Text = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                service.GenerateAsync(request, CancellationToken.None));
        }

        [Fact]
        public void AudioGenerationRequest_WithModelOverride_UsesOverride()
        {
            // Act
            var request = new AudioGenerationRequest
            {
                Text = "Test",
                ModelOverride = "tts-1-hd"
            };

            // Assert
            Assert.Equal("tts-1-hd", request.ModelOverride);
        }

        [Fact]
        public void AudioGenerationRequest_WithSpeedBounds_CreatesCorrectly()
        {
            // Act
            var slowRequest = new AudioGenerationRequest
            {
                Text = "Test",
                Speed = 0.25f
            };
            
            var fastRequest = new AudioGenerationRequest
            {
                Text = "Test",
                Speed = 4.0f
            };

            // Assert
            Assert.Equal(0.25f, slowRequest.Speed);
            Assert.Equal(4.0f, fastRequest.Speed);
        }

        [Fact]
        public void AudioGenerationResponse_WithoutOptionalFields_CanBeCreated()
        {
            // Act
            var response = new AudioGenerationResponse
            {
                AudioData = new byte[] { 1, 2, 3 },
                ContentType = "audio/mpeg",
                Duration = 3.0,
                Voice = "alloy",
                ModelUsed = "tts-1",
                Provider = "OpenAI"
            };

            // Assert
            Assert.Null(response.SampleRate);
            Assert.NotNull(response.AudioData);
        }
    }
}
