namespace FluentAI.Abstractions.Models;

/// <summary>
/// Request for audio transcription operations.
/// </summary>
public class AudioTranscriptionRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the audio data as bytes.
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the file path to the audio file.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Gets or sets the language code for transcription (e.g., "en", "es", "auto").
    /// </summary>
    public string Language { get; set; } = "auto";

    /// <summary>
    /// Gets or sets the response format ("json", "text", "srt", "verbose_json", "vtt").
    /// </summary>
    public string ResponseFormat { get; set; } = "json";

    /// <summary>
    /// Gets or sets the sampling temperature for transcription (0.0 to 1.0).
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets an optional text prompt to guide the transcription style.
    /// </summary>
    public string? Prompt { get; set; }
}

/// <summary>
/// Response from audio transcription operations.
/// </summary>
public class AudioTranscriptionResponse : MultiModalResponse
{
    /// <summary>
    /// Gets or sets the transcribed text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detected language of the audio.
    /// </summary>
    public string? DetectedLanguage { get; set; }

    /// <summary>
    /// Gets or sets the confidence score of the transcription (0.0 to 1.0).
    /// </summary>
    public float? ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets word-level timestamps and confidence scores.
    /// </summary>
    public IEnumerable<TranscriptionWord>? Words { get; set; }

    /// <summary>
    /// Gets or sets segment-level transcription data.
    /// </summary>
    public IEnumerable<TranscriptionSegment>? Segments { get; set; }

    /// <summary>
    /// Gets or sets the duration of the processed audio in seconds.
    /// </summary>
    public double? AudioDuration { get; set; }
}

/// <summary>
/// Request for audio generation operations.
/// </summary>
public class AudioGenerationRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the text to convert to speech.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the voice to use for generation.
    /// </summary>
    public string Voice { get; set; } = "alloy";

    /// <summary>
    /// Gets or sets the speed of the generated audio (0.25 to 4.0).
    /// </summary>
    public float Speed { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the response format ("mp3", "opus", "aac", "flac").
    /// </summary>
    public string ResponseFormat { get; set; } = "mp3";

    /// <summary>
    /// Gets or sets additional voice parameters.
    /// </summary>
    public VoiceParameters? VoiceParameters { get; set; }
}

/// <summary>
/// Response from audio generation operations.
/// </summary>
public class AudioGenerationResponse : MultiModalResponse
{
    /// <summary>
    /// Gets or sets the generated audio data as bytes.
    /// </summary>
    public byte[] AudioData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the content type of the audio data.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration of the generated audio in seconds.
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// Gets or sets the sample rate of the generated audio.
    /// </summary>
    public int? SampleRate { get; set; }

    /// <summary>
    /// Gets or sets the voice that was used for generation.
    /// </summary>
    public string Voice { get; set; } = string.Empty;
}

/// <summary>
/// Represents a transcribed word with timing information.
/// </summary>
public class TranscriptionWord
{
    /// <summary>
    /// Gets or sets the transcribed word.
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the word in seconds.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the word in seconds.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Gets or sets the confidence score for this word (0.0 to 1.0).
    /// </summary>
    public float? Confidence { get; set; }
}

/// <summary>
/// Represents a transcription segment with timing information.
/// </summary>
public class TranscriptionSegment
{
    /// <summary>
    /// Gets or sets the segment ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the text content of the segment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the segment in seconds.
    /// </summary>
    public double StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the segment in seconds.
    /// </summary>
    public double EndTime { get; set; }

    /// <summary>
    /// Gets or sets the temperature used for this segment.
    /// </summary>
    public float? Temperature { get; set; }

    /// <summary>
    /// Gets or sets the average log probability for this segment.
    /// </summary>
    public float? AvgLogProb { get; set; }

    /// <summary>
    /// Gets or sets the compression ratio for this segment.
    /// </summary>
    public float? CompressionRatio { get; set; }

    /// <summary>
    /// Gets or sets whether this segment has no speech.
    /// </summary>
    public float? NoSpeechProb { get; set; }
}

/// <summary>
/// Voice parameters for audio generation.
/// </summary>
public class VoiceParameters
{
    /// <summary>
    /// Gets or sets the pitch adjustment (-12 to 12 semitones).
    /// </summary>
    public float? Pitch { get; set; }

    /// <summary>
    /// Gets or sets the emphasis level (0.0 to 2.0).
    /// </summary>
    public float? Emphasis { get; set; }

    /// <summary>
    /// Gets or sets the speaking rate multiplier (0.5 to 2.0).
    /// </summary>
    public float? Rate { get; set; }

    /// <summary>
    /// Gets or sets the volume level (0.0 to 1.0).
    /// </summary>
    public float? Volume { get; set; }
}