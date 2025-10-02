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
    /// Unit tests for image analysis service implementations.
    /// </summary>
    public class ImageAnalysisServiceTests
    {
        private readonly Mock<ILogger<OpenAiImageAnalysisService>> _mockLogger;
        private readonly Mock<IOptionsMonitor<OpenAiOptions>> _mockOptions;
        private readonly OpenAiOptions _testOptions;

        public ImageAnalysisServiceTests()
        {
            _mockLogger = new Mock<ILogger<OpenAiImageAnalysisService>>();
            _mockOptions = new Mock<IOptionsMonitor<OpenAiOptions>>();
            _testOptions = new OpenAiOptions
            {
                ApiKey = "test-api-key",
                Model = "gpt-4-vision-preview"
            };
            _mockOptions.Setup(o => o.CurrentValue).Returns(_testOptions);
        }

        [Fact]
        public void OpenAiImageAnalysisService_CanBeInstantiated()
        {
            // Act
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.Equal("OpenAI", service.ProviderName);
            Assert.Equal("gpt-4-vision-preview", service.DefaultModelName);
        }

        [Fact]
        public async Task AnalyzeAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AnalyzeAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeAsync_WithEmptyPrompt_ThrowsArgumentException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);
            var request = new ImageAnalysisRequest
            {
                Prompt = "",
                ImageUrl = "https://example.com/image.jpg"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AnalyzeAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeAsync_WithNoImageData_ThrowsArgumentException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);
            var request = new ImageAnalysisRequest
            {
                Prompt = "Describe this image"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AnalyzeAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeFromUrlAsync_WithNullUrl_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AnalyzeFromUrlAsync(null!, "Describe", CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeFromUrlAsync_WithNullPrompt_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AnalyzeFromUrlAsync("https://example.com/image.jpg", null!, CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeFromBytesAsync_WithNullBytes_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AnalyzeFromBytesAsync(null!, "Describe", CancellationToken.None));
        }

        [Fact]
        public async Task AnalyzeFromBytesAsync_WithNullPrompt_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);
            var bytes = new byte[] { 1, 2, 3 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                service.AnalyzeFromBytesAsync(bytes, null!, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfig_Succeeds()
        {
            // Arrange
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert - should not throw
            await service.ValidateConfigurationAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithMissingApiKey_ThrowsAiSdkConfigurationException()
        {
            // Arrange
            var invalidOptions = new OpenAiOptions { ApiKey = "" };
            _mockOptions.Setup(o => o.CurrentValue).Returns(invalidOptions);
            var service = new OpenAiImageAnalysisService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FluentAI.Abstractions.Exceptions.AiSdkConfigurationException>(() =>
                service.ValidateConfigurationAsync(CancellationToken.None));
        }

        [Fact]
        public void ImageAnalysisRequest_WithUrl_CanBeCreated()
        {
            // Act
            var request = new ImageAnalysisRequest
            {
                Prompt = "Describe this image",
                ImageUrl = "https://example.com/image.jpg",
                DetailLevel = "high",
                MaxTokens = 1000
            };

            // Assert
            Assert.Equal("Describe this image", request.Prompt);
            Assert.Equal("https://example.com/image.jpg", request.ImageUrl);
            Assert.Equal("high", request.DetailLevel);
            Assert.Equal(1000, request.MaxTokens);
        }

        [Fact]
        public void ImageAnalysisRequest_WithBytes_CanBeCreated()
        {
            // Act
            var imageData = new byte[] { 1, 2, 3, 4, 5 };
            var request = new ImageAnalysisRequest
            {
                Prompt = "What's in this image?",
                ImageData = imageData,
                ImageFormat = "jpeg",
                DetailLevel = "auto"
            };

            // Assert
            Assert.Equal("What's in this image?", request.Prompt);
            Assert.NotNull(request.ImageData);
            Assert.Equal(5, request.ImageData.Length);
            Assert.Equal("jpeg", request.ImageFormat);
            Assert.Equal("auto", request.DetailLevel);
        }

        [Fact]
        public void ImageAnalysisResponse_CanBeCreated()
        {
            // Act
            var response = new ImageAnalysisResponse
            {
                Analysis = "This image shows a sunset",
                ConfidenceScore = 0.95f,
                ExtractedText = "Some text found",
                DetectedObjects = new List<DetectedObject>
                {
                    new DetectedObject
                    {
                        Name = "person",
                        Confidence = 0.9f,
                        BoundingBox = new BoundingBox { X = 10, Y = 20, Width = 100, Height = 200 }
                    }
                },
                ModelUsed = "gpt-4-vision-preview",
                Provider = "OpenAI"
            };

            // Assert
            Assert.Equal("This image shows a sunset", response.Analysis);
            Assert.Equal(0.95f, response.ConfidenceScore);
            Assert.Equal("Some text found", response.ExtractedText);
            Assert.NotNull(response.DetectedObjects);
            Assert.Single(response.DetectedObjects);
            Assert.Equal("person", response.DetectedObjects.First().Name);
            Assert.Equal("gpt-4-vision-preview", response.ModelUsed);
            Assert.Equal("OpenAI", response.Provider);
        }

        [Fact]
        public void DetectedObject_CanBeCreated()
        {
            // Act
            var obj = new DetectedObject
            {
                Name = "car",
                Confidence = 0.85f,
                BoundingBox = new BoundingBox
                {
                    X = 50,
                    Y = 100,
                    Width = 200,
                    Height = 150
                }
            };

            // Assert
            Assert.Equal("car", obj.Name);
            Assert.Equal(0.85f, obj.Confidence);
            Assert.NotNull(obj.BoundingBox);
            Assert.Equal(50, obj.BoundingBox.X);
            Assert.Equal(100, obj.BoundingBox.Y);
            Assert.Equal(200, obj.BoundingBox.Width);
            Assert.Equal(150, obj.BoundingBox.Height);
        }

        [Fact]
        public void BoundingBox_CanBeCreated()
        {
            // Act
            var box = new BoundingBox
            {
                X = 10,
                Y = 20,
                Width = 30,
                Height = 40
            };

            // Assert
            Assert.Equal(10, box.X);
            Assert.Equal(20, box.Y);
            Assert.Equal(30, box.Width);
            Assert.Equal(40, box.Height);
        }

        [Fact]
        public void BaseService_ImplementsIImageAnalysisService()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<ImageAnalysisService>>();
            var service = new ImageAnalysisService(baseLogger.Object);

            // Assert
            Assert.IsAssignableFrom<IImageAnalysisService>(service);
        }

        [Fact]
        public async Task BaseService_AnalyzeAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<ImageAnalysisService>>();
            var service = new ImageAnalysisService(baseLogger.Object);
            var request = new ImageAnalysisRequest
            {
                Prompt = "Test",
                ImageUrl = "https://example.com/image.jpg"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                service.AnalyzeAsync(request, CancellationToken.None));
        }

        [Fact]
        public void ImageAnalysisRequest_WithModelOverride_UsesOverride()
        {
            // Act
            var request = new ImageAnalysisRequest
            {
                Prompt = "Analyze",
                ImageUrl = "https://example.com/image.jpg",
                ModelOverride = "gpt-4o"
            };

            // Assert
            Assert.Equal("gpt-4o", request.ModelOverride);
        }
    }
}
