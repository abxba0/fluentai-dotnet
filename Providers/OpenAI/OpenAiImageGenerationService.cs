using Azure.AI.OpenAI;
using FluentAI.Abstractions;
using FluentAI.Abstractions.Exceptions;
using FluentAI.Abstractions.Models;
using FluentAI.Abstractions.Services;
using FluentAI.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FluentAI.Providers.OpenAI
{
    /// <summary>
    /// OpenAI implementation of the image generation service.
    /// Supports DALL-E 2 and DALL-E 3 models for image generation, editing, and variations.
    /// </summary>
    public class OpenAiImageGenerationService : ImageGenerationService
    {
        private readonly IOptionsMonitor<OpenAiOptions> _optionsMonitor;
        private readonly ILogger<OpenAiImageGenerationService> _logger;
        private Lazy<OpenAIClient>? _lazyClient;
        private readonly object _clientLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiImageGenerationService"/> class.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor for OpenAI configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public OpenAiImageGenerationService(
            IOptionsMonitor<OpenAiOptions> optionsMonitor, 
            ILogger<OpenAiImageGenerationService> logger) : base(logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override string ProviderName => "OpenAI";

        /// <inheritdoc />
        public override string DefaultModelName => "dall-e-3";

        /// <inheritdoc />
        public override async Task<ImageGenerationResponse> GenerateAsync(ImageGenerationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new ArgumentException("Prompt cannot be empty", nameof(request));
            }

            _logger.LogDebug("Generating image with OpenAI DALL-E: {Prompt}", request.Prompt);

            try
            {
                var client = GetOrCreateClient();
                var options = _optionsMonitor.CurrentValue;
                var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? DefaultModelName : request.ModelOverride;

                var imageGenerationOptions = new ImageGenerationOptions
                {
                    DeploymentName = model,
                    Prompt = request.Prompt,
                    ImageCount = request.NumberOfImages,
                    Size = ParseImageSize(request.Size),
                    Quality = ParseImageQuality(request.Quality),
                    Style = ParseImageStyle(request.Style)
                };

                var response = await client.GetImageGenerationsAsync(imageGenerationOptions, cancellationToken)
                    .ConfigureAwait(false);

                var generatedImages = response.Value.Data.Select(img => new GeneratedImage
                {
                    Url = img.Url?.ToString(),
                    Base64Data = img.Base64Data,
                    RevisedPrompt = img.RevisedPrompt
                }).ToList();

                _logger.LogInformation("Successfully generated {Count} image(s) with OpenAI", generatedImages.Count);

                return new ImageGenerationResponse
                {
                    Images = generatedImages,
                    RevisedPrompt = generatedImages.FirstOrDefault()?.RevisedPrompt ?? request.Prompt,
                    ModelUsed = model,
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating image with OpenAI: {Prompt}", request.Prompt);
                throw new AiSdkException($"OpenAI image generation failed: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public override async Task<ImageGenerationResponse> EditAsync(ImageEditRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            
            _logger.LogWarning("Image editing is not yet supported with the current Azure.AI.OpenAI SDK version");
            
            // Image editing requires OpenAI REST API or newer SDK version
            await Task.CompletedTask;
            throw new NotImplementedException("Image editing is not yet supported with the current Azure.AI.OpenAI SDK version. Please use the OpenAI REST API directly or upgrade the SDK.");
        }

        /// <inheritdoc />
        public override async Task<ImageGenerationResponse> CreateVariationAsync(ImageVariationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            
            _logger.LogWarning("Image variations are not yet supported with the current Azure.AI.OpenAI SDK version");
            
            // Image variations require OpenAI REST API or newer SDK version
            await Task.CompletedTask;
            throw new NotImplementedException("Image variations are not yet supported with the current Azure.AI.OpenAI SDK version. Please use the OpenAI REST API directly or upgrade the SDK.");
        }

        /// <inheritdoc />
        public override async Task ValidateConfigurationAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var options = _optionsMonitor.CurrentValue;
                
                if (string.IsNullOrWhiteSpace(options.ApiKey))
                {
                    throw new AiSdkConfigurationException("OpenAI API key is not configured");
                }

                _logger.LogInformation("OpenAI image generation service configuration validated successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI image generation service configuration validation failed");
                throw;
            }
        }

        private OpenAIClient GetOrCreateClient()
        {
            var options = _optionsMonitor.CurrentValue;
            
            lock (_clientLock)
            {
                if (_lazyClient == null)
                {
                    _lazyClient = new Lazy<OpenAIClient>(() => CreateClient(options));
                }
                
                return _lazyClient.Value;
            }
        }

        private OpenAIClient CreateClient(OpenAiOptions options)
        {
            if (options.IsAzureOpenAI && !string.IsNullOrWhiteSpace(options.Endpoint))
            {
                var endpoint = new Uri(options.Endpoint);
                return new OpenAIClient(endpoint, new Azure.AzureKeyCredential(options.ApiKey));
            }
            
            return new OpenAIClient(options.ApiKey);
        }

        private static ImageSize ParseImageSize(string? size)
        {
            return size?.ToLowerInvariant() switch
            {
                "256x256" => ImageSize.Size256x256,
                "512x512" => ImageSize.Size512x512,
                "1024x1024" => ImageSize.Size1024x1024,
                "1792x1024" => ImageSize.Size1792x1024,
                "1024x1792" => ImageSize.Size1024x1792,
                _ => ImageSize.Size1024x1024
            };
        }

        private static ImageGenerationQuality ParseImageQuality(string? quality)
        {
            return quality?.ToLowerInvariant() switch
            {
                "hd" => ImageGenerationQuality.Hd,
                "standard" => ImageGenerationQuality.Standard,
                _ => ImageGenerationQuality.Standard
            };
        }

        private static ImageGenerationStyle ParseImageStyle(string? style)
        {
            return style?.ToLowerInvariant() switch
            {
                "vivid" => ImageGenerationStyle.Vivid,
                "natural" => ImageGenerationStyle.Natural,
                _ => ImageGenerationStyle.Vivid
            };
        }


    }
}
