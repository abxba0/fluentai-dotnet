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
    /// OpenAI implementation of the image analysis service.
    /// Uses GPT-4 Vision for image understanding, OCR, and object detection.
    /// </summary>
    public class OpenAiImageAnalysisService : ImageAnalysisService
    {
        private readonly IOptionsMonitor<OpenAiOptions> _optionsMonitor;
        private readonly ILogger<OpenAiImageAnalysisService> _logger;
        private Lazy<OpenAIClient>? _lazyClient;
        private readonly object _clientLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiImageAnalysisService"/> class.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor for OpenAI configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public OpenAiImageAnalysisService(
            IOptionsMonitor<OpenAiOptions> optionsMonitor,
            ILogger<OpenAiImageAnalysisService> logger) : base(logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override string ProviderName => "OpenAI";

        /// <inheritdoc />
        public override string DefaultModelName => "gpt-4-vision-preview";

        /// <inheritdoc />
        public override async Task<ImageAnalysisResponse> AnalyzeAsync(ImageAnalysisRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                throw new ArgumentException("Prompt cannot be empty", nameof(request));
            }

            if (string.IsNullOrWhiteSpace(request.ImageUrl) && (request.ImageData == null || request.ImageData.Length == 0))
            {
                throw new ArgumentException("Either ImageUrl or ImageData must be provided", nameof(request));
            }

            _logger.LogDebug("Analyzing image with OpenAI GPT-4 Vision: {Prompt}", request.Prompt);

            try
            {
                var client = GetOrCreateClient();
                var options = _optionsMonitor.CurrentValue;
                var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? DefaultModelName : request.ModelOverride;

                // For GPT-4 Vision, we need to format the message with image URL or base64
                var chatMessages = new List<ChatRequestMessage>();
                
                string imageContent;
                if (!string.IsNullOrWhiteSpace(request.ImageUrl))
                {
                    imageContent = request.ImageUrl;
                }
                else if (request.ImageData != null && request.ImageData.Length > 0)
                {
                    var base64Image = Convert.ToBase64String(request.ImageData);
                    var mimeType = GetMimeType(request.ImageFormat);
                    imageContent = $"data:{mimeType};base64,{base64Image}";
                }
                else
                {
                    throw new ArgumentException("Either ImageUrl or ImageData must be provided");
                }

                // Build message with image reference
                var messageContent = $"{request.Prompt}\n\nImage: {imageContent}";
                chatMessages.Add(new ChatRequestUserMessage(messageContent));

                var chatOptions = new ChatCompletionsOptions
                {
                    DeploymentName = model,
                    MaxTokens = request.MaxTokens ?? 4096
                };

                foreach (var message in chatMessages)
                {
                    chatOptions.Messages.Add(message);
                }

                var response = await client.GetChatCompletionsAsync(chatOptions, cancellationToken)
                    .ConfigureAwait(false);

                var analysisText = response.Value.Choices[0].Message.Content;
                
                _logger.LogInformation("Successfully analyzed image with OpenAI GPT-4 Vision");

                return new ImageAnalysisResponse
                {
                    Analysis = analysisText,
                    ConfidenceScore = null, // OpenAI doesn't provide confidence scores
                    DetectedObjects = null, // Would need to parse from response text if requested
                    ExtractedText = null, // Would need to parse from response text if OCR was requested
                    ModelUsed = model,
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing image with OpenAI: {Prompt}", request.Prompt);
                throw new AiSdkException($"OpenAI image analysis failed: {ex.Message}", ex);
            }
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

                _logger.LogInformation("OpenAI image analysis service configuration validated successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI image analysis service configuration validation failed");
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

        private static string GetMimeType(string? imageFormat)
        {
            return imageFormat?.ToLowerInvariant() switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "gif" => "image/gif",
                "webp" => "image/webp",
                _ => "image/jpeg"
            };
        }
    }
}
