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
    /// Unit tests for image generation service implementations.
    /// </summary>
    public class ImageGenerationServiceTests
    {
        private readonly Mock<ILogger<OpenAiImageGenerationService>> _mockLogger;
        private readonly Mock<IOptionsMonitor<OpenAiOptions>> _mockOptions;
        private readonly OpenAiOptions _testOptions;

        public ImageGenerationServiceTests()
        {
            _mockLogger = new Mock<ILogger<OpenAiImageGenerationService>>();
            _mockOptions = new Mock<IOptionsMonitor<OpenAiOptions>>();
            _testOptions = new OpenAiOptions
            {
                ApiKey = "test-api-key",
                Model = "dall-e-3"
            };
            _mockOptions.Setup(o => o.CurrentValue).Returns(_testOptions);
        }

        [Fact]
        public void OpenAiImageGenerationService_CanBeInstantiated()
        {
            // Act
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.Equal("OpenAI", service.ProviderName);
            Assert.Equal("dall-e-3", service.DefaultModelName);
        }

        [Fact]
        public async Task GenerateAsync_WithNullRequest_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                service.GenerateAsync(null!, CancellationToken.None));
        }

        [Fact]
        public async Task GenerateAsync_WithEmptyPrompt_ThrowsArgumentException()
        {
            // Arrange
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);
            var request = new ImageGenerationRequest { Prompt = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                service.GenerateAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task EditAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);
            var request = new ImageEditRequest
            {
                ImageData = new byte[] { 1, 2, 3 },
                Prompt = "Edit this image"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => 
                service.EditAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateVariationAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);
            var request = new ImageVariationRequest
            {
                ImageData = new byte[] { 1, 2, 3 }
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => 
                service.CreateVariationAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithValidConfig_Succeeds()
        {
            // Arrange
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert - should not throw
            await service.ValidateConfigurationAsync(CancellationToken.None);
        }

        [Fact]
        public async Task ValidateConfigurationAsync_WithMissingApiKey_ThrowsAiSdkConfigurationException()
        {
            // Arrange
            var invalidOptions = new OpenAiOptions { ApiKey = "" };
            _mockOptions.Setup(o => o.CurrentValue).Returns(invalidOptions);
            var service = new OpenAiImageGenerationService(_mockOptions.Object, _mockLogger.Object);

            // Act & Assert
            await Assert.ThrowsAsync<FluentAI.Abstractions.Exceptions.AiSdkConfigurationException>(() =>
                service.ValidateConfigurationAsync(CancellationToken.None));
        }

        [Fact]
        public void ImageGenerationRequest_CanBeCreated()
        {
            // Act
            var request = new ImageGenerationRequest
            {
                Prompt = "A beautiful sunset",
                Size = "1024x1024",
                Quality = "hd",
                Style = "vivid",
                NumberOfImages = 1,
                ResponseFormat = "url"
            };

            // Assert
            Assert.Equal("A beautiful sunset", request.Prompt);
            Assert.Equal("1024x1024", request.Size);
            Assert.Equal("hd", request.Quality);
            Assert.Equal("vivid", request.Style);
            Assert.Equal(1, request.NumberOfImages);
            Assert.Equal("url", request.ResponseFormat);
        }

        [Fact]
        public void ImageGenerationResponse_CanBeCreated()
        {
            // Act
            var response = new ImageGenerationResponse
            {
                Images = new List<GeneratedImage>
                {
                    new GeneratedImage
                    {
                        Url = "https://example.com/image.png",
                        RevisedPrompt = "Revised prompt"
                    }
                },
                RevisedPrompt = "Revised prompt",
                ModelUsed = "dall-e-3",
                Provider = "OpenAI"
            };

            // Assert
            Assert.NotNull(response.Images);
            Assert.Single(response.Images);
            Assert.Equal("https://example.com/image.png", response.Images.First().Url);
            Assert.Equal("Revised prompt", response.RevisedPrompt);
            Assert.Equal("dall-e-3", response.ModelUsed);
            Assert.Equal("OpenAI", response.Provider);
        }

        [Fact]
        public void ImageEditRequest_CanBeCreated()
        {
            // Act
            var request = new ImageEditRequest
            {
                ImageData = new byte[] { 1, 2, 3, 4 },
                MaskData = new byte[] { 5, 6, 7, 8 },
                Prompt = "Edit the image",
                Size = "512x512",
                NumberOfImages = 2
            };

            // Assert
            Assert.NotNull(request.ImageData);
            Assert.Equal(4, request.ImageData.Length);
            Assert.NotNull(request.MaskData);
            Assert.Equal(4, request.MaskData.Length);
            Assert.Equal("Edit the image", request.Prompt);
            Assert.Equal("512x512", request.Size);
            Assert.Equal(2, request.NumberOfImages);
        }

        [Fact]
        public void ImageVariationRequest_CanBeCreated()
        {
            // Act
            var request = new ImageVariationRequest
            {
                ImageData = new byte[] { 1, 2, 3 },
                Size = "1024x1024",
                NumberOfImages = 3,
                ResponseFormat = "b64_json"
            };

            // Assert
            Assert.NotNull(request.ImageData);
            Assert.Equal(3, request.ImageData.Length);
            Assert.Equal("1024x1024", request.Size);
            Assert.Equal(3, request.NumberOfImages);
            Assert.Equal("b64_json", request.ResponseFormat);
        }

        [Fact]
        public void GeneratedImage_CanBeCreated()
        {
            // Act
            var image = new GeneratedImage
            {
                Url = "https://example.com/image.png",
                Base64Data = "base64encodeddata",
                RevisedPrompt = "Revised prompt text"
            };

            // Assert
            Assert.Equal("https://example.com/image.png", image.Url);
            Assert.Equal("base64encodeddata", image.Base64Data);
            Assert.Equal("Revised prompt text", image.RevisedPrompt);
        }

        [Fact]
        public void ImageGenerationRequest_WithModelOverride_UsesOverride()
        {
            // Act
            var request = new ImageGenerationRequest
            {
                Prompt = "Test",
                ModelOverride = "dall-e-2"
            };

            // Assert
            Assert.Equal("dall-e-2", request.ModelOverride);
        }

        [Fact]
        public void BaseService_ImplementsIImageGenerationService()
        {
            // Arrange
            var service = new ImageGenerationService(_mockLogger.Object);

            // Assert
            Assert.IsAssignableFrom<IImageGenerationService>(service);
        }

        [Fact]
        public async Task BaseService_GenerateAsync_ThrowsNotImplementedException()
        {
            // Arrange
            var baseLogger = new Mock<ILogger<ImageGenerationService>>();
            var service = new ImageGenerationService(baseLogger.Object);
            var request = new ImageGenerationRequest { Prompt = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                service.GenerateAsync(request, CancellationToken.None));
        }
    }
}
