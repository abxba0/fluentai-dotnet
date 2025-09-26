namespace FluentAI.Configuration;

/// <summary>
/// Configuration options for multi-modal AI services.
/// </summary>
public class MultiModalOptions
{
    /// <summary>
    /// Gets or sets the default strategy for model selection.
    /// </summary>
    public string DefaultStrategy { get; set; } = "Performance";

    /// <summary>
    /// Gets or sets the model configurations for each modality.
    /// </summary>
    public MultiModalModelsOptions Models { get; set; } = new();

    /// <summary>
    /// Gets or sets environment-specific configuration overrides.
    /// </summary>
    public Dictionary<string, Dictionary<string, object>>? EnvironmentOverrides { get; set; }
}

/// <summary>
/// Model configurations for different modalities.
/// </summary>
public class MultiModalModelsOptions
{
    /// <summary>
    /// Gets or sets the text generation model configuration.
    /// </summary>
    public ModalityModelOptions? TextGeneration { get; set; }

    /// <summary>
    /// Gets or sets the image analysis model configuration.
    /// </summary>
    public ModalityModelOptions? ImageAnalysis { get; set; }

    /// <summary>
    /// Gets or sets the image generation model configuration.
    /// </summary>
    public ModalityModelOptions? ImageGeneration { get; set; }

    /// <summary>
    /// Gets or sets the audio transcription model configuration.
    /// </summary>
    public ModalityModelOptions? AudioTranscription { get; set; }

    /// <summary>
    /// Gets or sets the audio generation model configuration.
    /// </summary>
    public ModalityModelOptions? AudioGeneration { get; set; }
}

/// <summary>
/// Configuration options for a specific modality.
/// </summary>
public class ModalityModelOptions
{
    /// <summary>
    /// Gets or sets the primary model configuration.
    /// </summary>
    public ModelConfiguration? Primary { get; set; }

    /// <summary>
    /// Gets or sets the fallback model configuration.
    /// </summary>
    public ModelConfiguration? Fallback { get; set; }
}

/// <summary>
/// Configuration for a specific model.
/// </summary>
public class ModelConfiguration
{
    /// <summary>
    /// Gets or sets the provider name.
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum tokens for requests.
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Gets or sets the default temperature.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the request timeout.
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }

    /// <summary>
    /// Gets or sets the detail level (for image analysis).
    /// </summary>
    public string? DetailLevel { get; set; }

    /// <summary>
    /// Gets or sets the quality setting (for image generation).
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Gets or sets the size setting (for image generation).
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets the style setting (for image generation).
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets the voice setting (for audio generation).
    /// </summary>
    public string? Voice { get; set; }

    /// <summary>
    /// Gets or sets the speed setting (for audio generation).
    /// </summary>
    public float? Speed { get; set; }

    /// <summary>
    /// Gets or sets the response format.
    /// </summary>
    public string? ResponseFormat { get; set; }

    /// <summary>
    /// Gets or sets additional provider-specific parameters.
    /// </summary>
    public Dictionary<string, object>? AdditionalParameters { get; set; }
}