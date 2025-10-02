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
    /// OpenAI implementation of the audio generation service.
    /// Supports text-to-speech with various voices and formats.
    /// </summary>
    public class OpenAiAudioGenerationService : AudioGenerationService
    {
        private readonly IOptionsMonitor<OpenAiOptions> _optionsMonitor;
        private readonly ILogger<OpenAiAudioGenerationService> _logger;
        private Lazy<OpenAIClient>? _lazyClient;
        private readonly object _clientLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiAudioGenerationService"/> class.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor for OpenAI configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public OpenAiAudioGenerationService(
            IOptionsMonitor<OpenAiOptions> optionsMonitor,
            ILogger<OpenAiAudioGenerationService> logger) : base(logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override string ProviderName => "OpenAI";

        /// <inheritdoc />
        public override string DefaultModelName => "tts-1";

        /// <inheritdoc />
        public override async Task<AudioGenerationResponse> GenerateAsync(AudioGenerationRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.Text))
            {
                throw new ArgumentException("Text cannot be empty", nameof(request));
            }

            _logger.LogDebug("Generating audio with OpenAI TTS: {Text}", request.Text.Substring(0, Math.Min(50, request.Text.Length)));

            try
            {
                var client = GetOrCreateClient();
                var options = _optionsMonitor.CurrentValue;
                var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? DefaultModelName : request.ModelOverride;

                var speechGenerationOptions = new SpeechGenerationOptions
                {
                    DeploymentName = model,
                    Input = request.Text,
                    Voice = ParseVoice(request.Voice),
                    ResponseFormat = ParseResponseFormat(request.ResponseFormat),
                    Speed = request.Speed
                };

                var response = await client.GenerateSpeechFromTextAsync(speechGenerationOptions, cancellationToken)
                    .ConfigureAwait(false);

                byte[] audioData;
                using (var memoryStream = new MemoryStream())
                {
                    await response.Value.ToStream().CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
                    audioData = memoryStream.ToArray();
                }

                var contentType = GetContentType(request.ResponseFormat);

                _logger.LogInformation("Successfully generated audio with OpenAI TTS, size: {Size} bytes", audioData.Length);

                return new AudioGenerationResponse
                {
                    AudioData = audioData,
                    ContentType = contentType,
                    Duration = 0, // OpenAI doesn't provide duration in response
                    Voice = request.Voice,
                    ModelUsed = model,
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio with OpenAI TTS");
                throw new AiSdkException($"OpenAI audio generation failed: {ex.Message}", ex);
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

                _logger.LogInformation("OpenAI audio generation service configuration validated successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI audio generation service configuration validation failed");
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

        private static SpeechVoice ParseVoice(string voice)
        {
            return voice?.ToLowerInvariant() switch
            {
                "alloy" => SpeechVoice.Alloy,
                "echo" => SpeechVoice.Echo,
                "fable" => SpeechVoice.Fable,
                "onyx" => SpeechVoice.Onyx,
                "nova" => SpeechVoice.Nova,
                "shimmer" => SpeechVoice.Shimmer,
                _ => SpeechVoice.Alloy
            };
        }

        private static SpeechGenerationResponseFormat ParseResponseFormat(string format)
        {
            return format?.ToLowerInvariant() switch
            {
                "mp3" => SpeechGenerationResponseFormat.Mp3,
                "opus" => SpeechGenerationResponseFormat.Opus,
                "aac" => SpeechGenerationResponseFormat.Aac,
                "flac" => SpeechGenerationResponseFormat.Flac,
                _ => SpeechGenerationResponseFormat.Mp3
            };
        }

        private static string GetContentType(string format)
        {
            return format?.ToLowerInvariant() switch
            {
                "mp3" => "audio/mpeg",
                "opus" => "audio/opus",
                "aac" => "audio/aac",
                "flac" => "audio/flac",
                _ => "audio/mpeg"
            };
        }
    }
}
