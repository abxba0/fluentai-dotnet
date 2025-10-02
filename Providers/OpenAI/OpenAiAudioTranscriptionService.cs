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
    /// OpenAI implementation of the audio transcription service.
    /// Uses Whisper models for speech-to-text transcription.
    /// </summary>
    public class OpenAiAudioTranscriptionService : AudioTranscriptionService
    {
        private readonly IOptionsMonitor<OpenAiOptions> _optionsMonitor;
        private readonly ILogger<OpenAiAudioTranscriptionService> _logger;
        private Lazy<OpenAIClient>? _lazyClient;
        private readonly object _clientLock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenAiAudioTranscriptionService"/> class.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor for OpenAI configuration.</param>
        /// <param name="logger">The logger instance.</param>
        public OpenAiAudioTranscriptionService(
            IOptionsMonitor<OpenAiOptions> optionsMonitor,
            ILogger<OpenAiAudioTranscriptionService> logger) : base(logger)
        {
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public override string ProviderName => "OpenAI";

        /// <inheritdoc />
        public override string DefaultModelName => "whisper-1";

        /// <inheritdoc />
        public override async Task<AudioTranscriptionResponse> TranscribeAsync(AudioTranscriptionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.AudioData == null || request.AudioData.Length == 0)
            {
                throw new ArgumentException("Audio data cannot be empty", nameof(request));
            }

            _logger.LogDebug("Transcribing audio with OpenAI Whisper, size: {Size} bytes", request.AudioData.Length);

            try
            {
                var client = GetOrCreateClient();
                var options = _optionsMonitor.CurrentValue;
                var model = string.IsNullOrWhiteSpace(request.ModelOverride) ? DefaultModelName : request.ModelOverride;

                using var audioStream = new MemoryStream(request.AudioData);
                var fileName = !string.IsNullOrWhiteSpace(request.FilePath) 
                    ? Path.GetFileName(request.FilePath) 
                    : "audio.wav";

                var transcriptionOptions = new AudioTranscriptionOptions
                {
                    DeploymentName = model,
                    AudioData = BinaryData.FromStream(audioStream),
                    Filename = fileName,
                    ResponseFormat = ParseResponseFormat(request.ResponseFormat),
                    Temperature = request.Temperature
                };

                // Set language if not auto
                if (!string.IsNullOrWhiteSpace(request.Language) && request.Language.ToLowerInvariant() != "auto")
                {
                    transcriptionOptions.Language = request.Language;
                }

                // Set prompt if provided
                if (!string.IsNullOrWhiteSpace(request.Prompt))
                {
                    transcriptionOptions.Prompt = request.Prompt;
                }

                var response = await client.GetAudioTranscriptionAsync(transcriptionOptions, cancellationToken)
                    .ConfigureAwait(false);

                var transcription = response.Value;

                _logger.LogInformation("Successfully transcribed audio with OpenAI Whisper");

                return new AudioTranscriptionResponse
                {
                    Text = transcription.Text,
                    DetectedLanguage = transcription.Language,
                    AudioDuration = transcription.Duration?.TotalSeconds,
                    ModelUsed = model,
                    Provider = ProviderName,
                    // Map segments if available (only in verbose_json format)
                    Segments = transcription.Segments?.Select(s => new TranscriptionSegment
                    {
                        Id = s.Id,
                        Text = s.Text,
                        StartTime = s.Start.TotalSeconds,
                        EndTime = s.End.TotalSeconds,
                        Temperature = s.Temperature,
                        AvgLogProb = s.AverageLogProbability,
                        CompressionRatio = s.CompressionRatio,
                        NoSpeechProb = s.NoSpeechProbability
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transcribing audio with OpenAI Whisper");
                throw new AiSdkException($"OpenAI audio transcription failed: {ex.Message}", ex);
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

                _logger.LogInformation("OpenAI audio transcription service configuration validated successfully");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OpenAI audio transcription service configuration validation failed");
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

        private static AudioTranscriptionFormat ParseResponseFormat(string format)
        {
            return format?.ToLowerInvariant() switch
            {
                "json" => AudioTranscriptionFormat.Simple,
                "verbose_json" => AudioTranscriptionFormat.Verbose,
                "text" => AudioTranscriptionFormat.Simple,
                "srt" => AudioTranscriptionFormat.Srt,
                "vtt" => AudioTranscriptionFormat.Vtt,
                _ => AudioTranscriptionFormat.Simple
            };
        }
    }
}
