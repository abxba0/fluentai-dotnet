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
    /// Unit tests for audio transcription service implementations.
    /// </summary>
    public class AudioTranscriptionServiceTests
    {
        private readonly Mock<ILogger<OpenAiAudioTranscriptionService>> _mockLogger;
        private readonly Mock<IOptionsMonitor<OpenAiOptions>> _mockOptions;
        private readonly OpenAiOptions _testOptions;

        public AudioTranscriptionServiceTests()
        {
            _mockLogger = new Mock<ILogger<OpenAiAudioTranscriptionService>>();
            _mockOptions = new Mock<IOptionsMonitor<OpenAiOptions>>();
            _testOptions = new OpenAiOptions
            {
                ApiKey = "test-api-key",
                Model = "whisper-1"
            };
            _mockOptions.Setup(o => o.CurrentValue).Returns(_testOptions);
        }

        [Fact]
        public void OpenAiAudioTranscriptionService_CanBeInstantiated()
        {
            // Act
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.Equal("OpenAI", service.ProviderName);
            Assert.Equal("whisper-1", service.DefaultModelName);
        }

        [Fact]
        public async Task TranscribeAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.TranscribeAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task TranscribeAsync_WithEmptyAudioData_ThrowsArgumentException()
        {
            // Arrange
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);
            var request = new AudioTranscriptionRequest { AudioData = Array.Empty<byte>() };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.TranscribeAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task TranscribeFromFileAsync_WithNullFilePath_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.TranscribeFromFileAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task TranscribeFromFileAsync_WithNonExistentFile_ThrowsFileNotFoundException()
        {
            // Arrange
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);
            var nonExistentPath = "/tmp/nonexistent-audio-file.mp3";

            // Act & Assert
            await Assert.ThrowsAsync<FileNotFoundException>(() =>
                service.TranscribeFromFileAsync(nonExistentPath, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfig_Succeeds()
        {
            // Arrange
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert - should not throw
            await service.ValidateConfigurationAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithMissingApiKey_ThrowsAiSdkConfigurationException()
        {
            // Arrange
            var invalidOptions = new OpenAiOptions { ApiKey = "" };
            _mockOptions.Setup(o => o.CurrentValue).Returns(invalidOptions);
            var service = new OpenAiAudioTranscriptionService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FluentAI.Abstractions.Exceptions.AiSdkConfigurationException>(() =>
                service.ValidateConfigurationAsync(CancellationToken.None));
        }

        [Fact]
        public void AudioTranscriptionRequest_CanBeCreated()
        {
            // Act
            var request = new AudioTranscriptionRequest
            {
                AudioData = new byte[] { 1, 2, 3, 4, 5 },
                FilePath = "/path/to/audio.mp3",
                Language = "en",
                ResponseFormat = "json",
                Temperature = 0.7f,
                Prompt = "This is about technology"
            };

            // Assert
            Assert.NotNull(request.AudioData);
            Assert.Equal(5, request.AudioData.Length);
            Assert.Equal("/path/to/audio.mp3", request.FilePath);
            Assert.Equal("en", request.Language);
            Assert.Equal("json", request.ResponseFormat);
            Assert.Equal(0.7f, request.Temperature);
            Assert.Equal("This is about technology", request.Prompt);
        }

        [Fact]
        public void AudioTranscriptionRequest_WithDifferentLanguages_CreatesCorrectly()
        {
            // Arrange & Act
            var languages = new[] { "en", "es", "fr", "de", "auto" };
            
            foreach (var language in languages)
            {
                var request = new AudioTranscriptionRequest
                {
                    AudioData = new byte[] { 1, 2, 3 },
                    Language = language
                };

                // Assert
                Assert.Equal(language, request.Language);
            }
        }

        [Fact]
        public void AudioTranscriptionRequest_WithDifferentFormats_CreatesCorrectly()
        {
            // Arrange & Act
            var formats = new[] { "json", "text", "srt", "verbose_json", "vtt" };
            
            foreach (var format in formats)
            {
                var request = new AudioTranscriptionRequest
                {
                    AudioData = new byte[] { 1, 2, 3 },
                    ResponseFormat = format
                };

                // Assert
                Assert.Equal(format, request.ResponseFormat);
            }
        }

        [Fact]
        public void AudioTranscriptionResponse_CanBeCreated()
        {
            // Act
            var response = new AudioTranscriptionResponse
            {
                Text = "This is the transcribed text",
                DetectedLanguage = "en",
                ConfidenceScore = 0.95f,
                AudioDuration = 30.5,
                Words = new List<TranscriptionWord>
                {
                    new TranscriptionWord
                    {
                        Word = "hello",
                        StartTime = 0.0,
                        EndTime = 0.5,
                        Confidence = 0.98f
                    }
                },
                Segments = new List<TranscriptionSegment>
                {
                    new TranscriptionSegment
                    {
                        Id = 0,
                        Text = "Hello world",
                        StartTime = 0.0,
                        EndTime = 1.0,
                        Temperature = 0.7f,
                        AvgLogProb = -0.5f,
                        CompressionRatio = 1.2f,
                        NoSpeechProb = 0.01f
                    }
                },
                ModelUsed = "whisper-1",
                Provider = "OpenAI"
            };

            // Assert
            Assert.Equal("This is the transcribed text", response.Text);
            Assert.Equal("en", response.DetectedLanguage);
            Assert.Equal(0.95f, response.ConfidenceScore);
            Assert.Equal(30.5, response.AudioDuration);
            Assert.NotNull(response.Words);
            Assert.Single(response.Words);
            Assert.Equal("hello", response.Words.First().Word);
            Assert.NotNull(response.Segments);
            Assert.Single(response.Segments);
            Assert.Equal("Hello world", response.Segments.First().Text);
            Assert.Equal("whisper-1", response.ModelUsed);
            Assert.Equal("OpenAI", response.Provider);
        }

        [Fact]
        public void TranscriptionWord_CanBeCreated()
        {
            // Act
            var word = new TranscriptionWord
            {
                Word = "example",
                StartTime = 1.5,
                EndTime = 2.0,
                Confidence = 0.92f
            };

            // Assert
            Assert.Equal("example", word.Word);
            Assert.Equal(1.5, word.StartTime);
            Assert.Equal(2.0, word.EndTime);
            Assert.Equal(0.92f, word.Confidence);
        }

        [Fact]
        public void TranscriptionSegment_CanBeCreated()
        {
            // Act
            var segment = new TranscriptionSegment
            {
                Id = 1,
                Text = "This is a segment",
                StartTime = 5.0,
                EndTime = 10.0,
                Temperature = 0.8f,
                AvgLogProb = -0.3f,
                CompressionRatio = 1.5f,
                NoSpeechProb = 0.05f
            };

            // Assert
            Assert.Equal(1, segment.Id);
            Assert.Equal("This is a segment", segment.Text);
            Assert.Equal(5.0, segment.StartTime);
            Assert.Equal(10.0, segment.EndTime);
            Assert.Equal(0.8f, segment.Temperature);
            Assert.Equal(-0.3f, segment.AvgLogProb);
            Assert.Equal(1.5f, segment.CompressionRatio);
            Assert.Equal(0.05f, segment.NoSpeechProb);
        }

        [Fact]
        public void BaseService_ImplementsIAudioTranscriptionService()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<AudioTranscriptionService>>();
            var service = new AudioTranscriptionService(baseLogger.Object);

            // Assert
            Assert.IsAssignableFrom<IAudioTranscriptionService>(service);
        }

        [Fact]
        public async Task BaseService_TranscribeAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<AudioTranscriptionService>>();
            var service = new AudioTranscriptionService(baseLogger.Object);
            var request = new AudioTranscriptionRequest { AudioData = new byte[] { 1, 2, 3 } };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                service.TranscribeAsync(request, CancellationToken.None));
        }

        [Fact]
        public void AudioTranscriptionRequest_WithModelOverride_UsesOverride()
        {
            // Act
            var request = new AudioTranscriptionRequest
            {
                AudioData = new byte[] { 1, 2, 3 },
                ModelOverride = "whisper-large"
            };

            // Assert
            Assert.Equal("whisper-large", request.ModelOverride);
        }

        [Fact]
        public void AudioTranscriptionRequest_DefaultLanguage_IsAuto()
        {
            // Act
            var request = new AudioTranscriptionRequest
            {
                AudioData = new byte[] { 1, 2, 3 }
            };

            // Assert
            Assert.Equal("auto", request.Language);
        }

        [Fact]
        public void AudioTranscriptionRequest_DefaultResponseFormat_IsJson()
        {
            // Act
            var request = new AudioTranscriptionRequest
            {
                AudioData = new byte[] { 1, 2, 3 }
            };

            // Assert
            Assert.Equal("json", request.ResponseFormat);
        }

        [Fact]
        public void AudioTranscriptionResponse_WithMinimalData_CanBeCreated()
        {
            // Act
            var response = new AudioTranscriptionResponse
            {
                Text = "Simple transcription",
                ModelUsed = "whisper-1",
                Provider = "OpenAI"
            };

            // Assert
            Assert.Equal("Simple transcription", response.Text);
            Assert.Null(response.DetectedLanguage);
            Assert.Null(response.ConfidenceScore);
            Assert.Null(response.Words);
            Assert.Null(response.Segments);
            Assert.Null(response.AudioDuration);
        }
    }
}
