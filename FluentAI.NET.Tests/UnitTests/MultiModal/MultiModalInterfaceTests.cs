using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Services;
using FluentAI.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MultiModal
{
    /// <summary>
    /// Unit tests for multi-modal interface validation and basic functionality.
    /// These tests ensure that the multi-modal interfaces are properly defined and can be used.
    /// </summary>
    public class MultiModalInterfaceTests
    {
        /// <summary>
        /// Verifies that all multi-modal service interfaces are properly defined.
        /// </summary>
        [Fact]
        public void MultiModalServiceInterfaces_AreProperlyDefined()
        {
            // Arrange & Act & Assert
            
            // Verify ITextGenerationService interface
            var textGenerationInterface = typeof(ITextGenerationService);
            Assert.True(textGenerationInterface.IsInterface);
            Assert.True(typeof(IAiService).IsAssignableFrom(textGenerationInterface));
            
            // Verify IImageAnalysisService interface
            var imageAnalysisInterface = typeof(IImageAnalysisService);
            Assert.True(imageAnalysisInterface.IsInterface);
            Assert.True(typeof(IAiService).IsAssignableFrom(imageAnalysisInterface));
            
            // Verify IImageGenerationService interface
            var imageGenerationInterface = typeof(IImageGenerationService);
            Assert.True(imageGenerationInterface.IsInterface);
            Assert.True(typeof(IAiService).IsAssignableFrom(imageGenerationInterface));
            
            // Verify IAudioTranscriptionService interface
            var audioTranscriptionInterface = typeof(IAudioTranscriptionService);
            Assert.True(audioTranscriptionInterface.IsInterface);
            Assert.True(typeof(IAiService).IsAssignableFrom(audioTranscriptionInterface));
            
            // Verify IAudioGenerationService interface
            var audioGenerationInterface = typeof(IAudioGenerationService);
            Assert.True(audioGenerationInterface.IsInterface);
            Assert.True(typeof(IAiService).IsAssignableFrom(audioGenerationInterface));
        }

        /// <summary>
        /// Verifies that TextGenerationService can be instantiated and properly wraps IChatModel.
        /// </summary>
        [Fact]
        public void TextGenerationService_CanBeInstantiated_WithIChatModel()
        {
            // Arrange
            var mockChatModel = new Mock<IChatModel>();
            var mockLogger = new Mock<ILogger<TextGenerationService>>();

            // Act
            var service = new TextGenerationService(mockChatModel.Object, mockLogger.Object);

            // Assert
            Assert.NotNull(service);
            Assert.Equal("DefaultTextProvider", service.ProviderName);
            Assert.Equal("gpt-3.5-turbo", service.DefaultModelName);
        }

        /// <summary>
        /// Verifies that multi-modal request and response models can be instantiated.
        /// </summary>
        [Fact]
        public void MultiModalModels_CanBeInstantiated()
        {
            // Arrange & Act
            var textRequest = new TextRequest
            {
                Prompt = "Test prompt",
                MaxTokens = 100,
                Temperature = 0.7f
            };

            var textResponse = new TextResponse
            {
                Content = "Test response",
                FinishReason = "stop",
                ModelUsed = "gpt-3.5-turbo",
                Provider = "OpenAI"
            };

            var imageAnalysisRequest = new ImageAnalysisRequest
            {
                Prompt = "Describe this image",
                ImageUrl = "https://example.com/image.jpg"
            };

            var imageAnalysisResponse = new ImageAnalysisResponse
            {
                Analysis = "This is a description of the image",
                ConfidenceScore = 0.95f
            };

            // Assert
            Assert.NotNull(textRequest);
            Assert.Equal("Test prompt", textRequest.Prompt);
            Assert.Equal(100, textRequest.MaxTokens);
            Assert.Equal(0.7f, textRequest.Temperature);

            Assert.NotNull(textResponse);
            Assert.Equal("Test response", textResponse.Content);
            Assert.Equal("stop", textResponse.FinishReason);

            Assert.NotNull(imageAnalysisRequest);
            Assert.Equal("Describe this image", imageAnalysisRequest.Prompt);
            Assert.Equal("https://example.com/image.jpg", imageAnalysisRequest.ImageUrl);

            Assert.NotNull(imageAnalysisResponse);
            Assert.Equal("This is a description of the image", imageAnalysisResponse.Analysis);
            Assert.Equal(0.95f, imageAnalysisResponse.ConfidenceScore);
        }

        /// <summary>
        /// Verifies that ModalityType enum contains all expected values.
        /// </summary>
        [Fact]
        public void ModalityType_ContainsAllExpectedValues()
        {
            // Arrange & Act
            var modalityTypes = Enum.GetValues<ModalityType>();

            // Assert
            Assert.Contains(ModalityType.TextGeneration, modalityTypes);
            Assert.Contains(ModalityType.ImageAnalysis, modalityTypes);
            Assert.Contains(ModalityType.ImageGeneration, modalityTypes);
            Assert.Contains(ModalityType.AudioTranscription, modalityTypes);
            Assert.Contains(ModalityType.AudioGeneration, modalityTypes);
            Assert.Equal(5, modalityTypes.Length);
        }

        /// <summary>
        /// Verifies that ModalitySupport class works correctly.
        /// </summary>
        [Fact]
        public void ModalitySupport_WorksCorrectly()
        {
            // Arrange
            var supportedModels = new[] { "gpt-4", "gpt-3.5-turbo" };

            // Act
            var modalitySupport = new ModalitySupport(ModalityType.TextGeneration, supportedModels)
            {
                SupportsStreaming = true,
                MaxInputSize = 4096,
                AdditionalCapabilities = new[] { "function-calling", "json-output" }
            };

            // Assert
            Assert.Equal(ModalityType.TextGeneration, modalitySupport.Modality);
            Assert.Equal(2, modalitySupport.SupportedModels.Count);
            Assert.Contains("gpt-4", modalitySupport.SupportedModels);
            Assert.Contains("gpt-3.5-turbo", modalitySupport.SupportedModels);
            Assert.True(modalitySupport.SupportsStreaming);
            Assert.Equal(4096, modalitySupport.MaxInputSize);
            Assert.NotNull(modalitySupport.AdditionalCapabilities);
            Assert.Contains("function-calling", modalitySupport.AdditionalCapabilities);
        }

        /// <summary>
        /// Verifies that MultiModalOptions configuration class works correctly.
        /// </summary>
        [Fact]
        public void MultiModalOptions_ConfigurationWorksCorrectly()
        {
            // Arrange & Act
            var options = new MultiModalOptions
            {
                DefaultStrategy = "Performance",
                Models = new MultiModalModelsOptions
                {
                    TextGeneration = new ModalityModelOptions
                    {
                        Primary = new ModelConfiguration
                        {
                            Provider = "OpenAI",
                            ModelName = "gpt-4",
                            MaxTokens = 4096,
                            Temperature = 0.7f
                        },
                        Fallback = new ModelConfiguration
                        {
                            Provider = "Anthropic",
                            ModelName = "claude-3-sonnet-20240229"
                        }
                    }
                }
            };

            // Assert
            Assert.Equal("Performance", options.DefaultStrategy);
            Assert.NotNull(options.Models);
            Assert.NotNull(options.Models.TextGeneration);
            Assert.NotNull(options.Models.TextGeneration.Primary);
            Assert.Equal("OpenAI", options.Models.TextGeneration.Primary.Provider);
            Assert.Equal("gpt-4", options.Models.TextGeneration.Primary.ModelName);
            Assert.Equal(4096, options.Models.TextGeneration.Primary.MaxTokens);
            Assert.Equal(0.7f, options.Models.TextGeneration.Primary.Temperature);
            
            Assert.NotNull(options.Models.TextGeneration.Fallback);
            Assert.Equal("Anthropic", options.Models.TextGeneration.Fallback.Provider);
            Assert.Equal("claude-3-sonnet-20240229", options.Models.TextGeneration.Fallback.ModelName);
        }
    }
}