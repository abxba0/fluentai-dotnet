namespace FluentAI.Abstractions.Models;

/// <summary>
/// Defines the supported AI modalities.
/// </summary>
public enum ModalityType
{
    /// <summary>
    /// Text generation and conversation.
    /// </summary>
    TextGeneration,

    /// <summary>
    /// Image analysis and understanding.
    /// </summary>
    ImageAnalysis,

    /// <summary>
    /// Image generation and creation.
    /// </summary>
    ImageGeneration,

    /// <summary>
    /// Audio transcription and speech-to-text.
    /// </summary>
    AudioTranscription,

    /// <summary>
    /// Audio generation and text-to-speech.
    /// </summary>
    AudioGeneration
}

/// <summary>
/// Describes the support level for a specific modality.
/// </summary>
public class ModalitySupport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModalitySupport"/> class.
    /// </summary>
    /// <param name="modality">The modality type.</param>
    /// <param name="supportedModels">The supported model names.</param>
    public ModalitySupport(ModalityType modality, IEnumerable<string> supportedModels)
    {
        Modality = modality;
        SupportedModels = supportedModels.ToList();
    }

    /// <summary>
    /// Gets the modality type.
    /// </summary>
    public ModalityType Modality { get; }

    /// <summary>
    /// Gets the list of supported model names for this modality.
    /// </summary>
    public IReadOnlyList<string> SupportedModels { get; }

    /// <summary>
    /// Gets or sets whether streaming is supported for this modality.
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// Gets or sets additional capabilities supported by this modality.
    /// </summary>
    public IEnumerable<string>? AdditionalCapabilities { get; set; }

    /// <summary>
    /// Gets or sets the maximum input size supported (in bytes for binary data, characters for text).
    /// </summary>
    public long? MaxInputSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum output size supported.
    /// </summary>
    public long? MaxOutputSize { get; set; }
}