# FluentAI.NET Multi-Modal Services Guide

This guide covers the use of multi-modal AI services in FluentAI.NET, including image generation, image analysis, audio generation, and audio transcription.

## Table of Contents

- [Overview](#overview)
- [Image Generation](#image-generation)
- [Image Analysis](#image-analysis)
- [Audio Generation (Text-to-Speech)](#audio-generation-text-to-speech)
- [Audio Transcription (Speech-to-Text)](#audio-transcription-speech-to-text)
- [Configuration](#configuration)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)

## Overview

FluentAI.NET provides a unified interface for working with multi-modal AI services. Currently supported:

- **Image Generation**: Create images from text prompts (DALL-E 2/3)
- **Image Analysis**: Analyze images with vision models (GPT-4 Vision)
- **Audio Generation**: Convert text to speech with customizable voices
- **Audio Transcription**: Convert speech to text with Whisper

All services follow a consistent pattern:
1. Create a request object with your parameters
2. Call the service's async method
3. Process the response

## Image Generation

### Basic Usage

```csharp
using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Providers.OpenAI;
using Microsoft.Extensions.Options;

// Create the service
var options = Options.Create(new OpenAiOptions { ApiKey = "your-api-key" });
var logger = loggerFactory.CreateLogger<OpenAiImageGenerationService>();
var imageService = new OpenAiImageGenerationService(options, logger);

// Generate an image
var request = new ImageGenerationRequest
{
    Prompt = "A serene mountain landscape at sunset with vibrant colors",
    Size = "1024x1024",
    Quality = "hd",
    Style = "vivid",
    NumberOfImages = 1
};

var response = await imageService.GenerateAsync(request);

// Access the generated image
foreach (var image in response.Images)
{
    Console.WriteLine($"Image URL: {image.Url}");
    Console.WriteLine($"Revised Prompt: {image.RevisedPrompt}");
    
    // Or access base64 data if ResponseFormat was "b64_json"
    if (!string.IsNullOrEmpty(image.Base64Data))
    {
        var imageBytes = Convert.FromBase64String(image.Base64Data);
        await File.WriteAllBytesAsync("generated-image.png", imageBytes);
    }
}
```

### Advanced Options

```csharp
var request = new ImageGenerationRequest
{
    Prompt = "A futuristic city with flying cars",
    Size = "1792x1024",        // Options: 256x256, 512x512, 1024x1024, 1792x1024, 1024x1792
    Quality = "hd",             // Options: "standard", "hd"
    Style = "vivid",            // Options: "vivid", "natural"
    NumberOfImages = 1,         // Max depends on model
    ResponseFormat = "url",     // Options: "url", "b64_json"
    ModelOverride = "dall-e-3"  // Override default model
};
```

## Image Analysis

### Analyze Image from URL

```csharp
using FluentAI.Providers.OpenAI;

var analysisService = new OpenAiImageAnalysisService(options, logger);

// Analyze from URL
var response = await analysisService.AnalyzeFromUrlAsync(
    "https://example.com/image.jpg",
    "What objects do you see in this image? Provide details."
);

Console.WriteLine($"Analysis: {response.Analysis}");
Console.WriteLine($"Confidence: {response.ConfidenceScore}");

// Check for detected objects
if (response.DetectedObjects != null)
{
    foreach (var obj in response.DetectedObjects)
    {
        Console.WriteLine($"- {obj.Name} (confidence: {obj.Confidence})");
        if (obj.BoundingBox != null)
        {
            Console.WriteLine($"  Location: ({obj.BoundingBox.X}, {obj.BoundingBox.Y})");
        }
    }
}
```

### Analyze Image from Bytes

```csharp
// Load image from file
var imageBytes = await File.ReadAllBytesAsync("photo.jpg");

// Analyze from bytes
var response = await analysisService.AnalyzeFromBytesAsync(
    imageBytes,
    "Describe this image in detail. What is the main subject?"
);

Console.WriteLine(response.Analysis);
```

### Advanced Analysis

```csharp
var request = new ImageAnalysisRequest
{
    Prompt = "List all text visible in this image",
    ImageUrl = "https://example.com/document.jpg",
    DetailLevel = "high",      // Options: "low", "high", "auto"
    MaxTokens = 2000,          // Control response length
    ImageFormat = "jpeg"       // Hint for base64 data
};

var response = await analysisService.AnalyzeAsync(request);

// Access extracted text (if requested in prompt)
if (!string.IsNullOrEmpty(response.ExtractedText))
{
    Console.WriteLine($"OCR Text: {response.ExtractedText}");
}
```

## Audio Generation (Text-to-Speech)

### Basic TTS

```csharp
using FluentAI.Providers.OpenAI;

var audioService = new OpenAiAudioGenerationService(options, logger);

var request = new AudioGenerationRequest
{
    Text = "Hello! Welcome to FluentAI text-to-speech service.",
    Voice = "alloy",           // Options: alloy, echo, fable, onyx, nova, shimmer
    Speed = 1.0f,              // Range: 0.25 to 4.0
    ResponseFormat = "mp3"     // Options: mp3, opus, aac, flac
};

var response = await audioService.GenerateAsync(request);

// Save audio to file
await File.WriteAllBytesAsync("output.mp3", response.AudioData);

Console.WriteLine($"Generated {response.AudioData.Length} bytes of {response.ContentType}");
Console.WriteLine($"Duration: {response.Duration} seconds");
Console.WriteLine($"Voice: {response.Voice}");
```

### Voice Customization

```csharp
var request = new AudioGenerationRequest
{
    Text = "This is a demonstration of voice customization.",
    Voice = "nova",
    Speed = 1.25f,  // Slightly faster
    ResponseFormat = "mp3",
    VoiceParameters = new VoiceParameters
    {
        Pitch = 0.5f,      // Range: -12 to 12 semitones
        Rate = 1.2f,       // Range: 0.5 to 2.0
        Volume = 0.9f,     // Range: 0.0 to 1.0
        Emphasis = 1.3f    // Range: 0.0 to 2.0
    }
};

var response = await audioService.GenerateAsync(request);
```

### Available Voices

| Voice | Characteristics |
|-------|----------------|
| **alloy** | Balanced, neutral tone |
| **echo** | Warm, friendly |
| **fable** | Expressive, storytelling |
| **onyx** | Deep, authoritative |
| **nova** | Energetic, upbeat |
| **shimmer** | Soft, calming |

## Audio Transcription (Speech-to-Text)

### Transcribe from File

```csharp
using FluentAI.Providers.OpenAI;

var transcriptionService = new OpenAiAudioTranscriptionService(options, logger);

// Transcribe from file path
var response = await transcriptionService.TranscribeFromFileAsync("meeting-recording.mp3");

Console.WriteLine($"Transcription: {response.Text}");
Console.WriteLine($"Language: {response.DetectedLanguage}");
Console.WriteLine($"Duration: {response.AudioDuration} seconds");
```

### Transcribe with Options

```csharp
var audioBytes = await File.ReadAllBytesAsync("interview.wav");

var request = new AudioTranscriptionRequest
{
    AudioData = audioBytes,
    Language = "en",               // Specify language or use "auto"
    ResponseFormat = "verbose_json", // Options: json, text, srt, verbose_json, vtt
    Temperature = 0.0f,            // For deterministic results
    Prompt = "This is a technical interview about software engineering."
};

var response = await transcriptionService.TranscribeAsync(request);

// Access word-level details (when using verbose_json)
if (response.Words != null)
{
    foreach (var word in response.Words)
    {
        Console.WriteLine($"{word.Word} ({word.StartTime:F2}s - {word.EndTime:F2}s, confidence: {word.Confidence:P})");
    }
}

// Access segment details
if (response.Segments != null)
{
    foreach (var segment in response.Segments)
    {
        Console.WriteLine($"Segment {segment.Id}: {segment.Text}");
        Console.WriteLine($"  Time: {segment.StartTime:F2}s - {segment.EndTime:F2}s");
        Console.WriteLine($"  Avg Log Prob: {segment.AvgLogProb:F4}");
    }
}
```

### Supported Languages

Whisper supports 99+ languages. Common examples:
- `"en"` - English
- `"es"` - Spanish
- `"fr"` - French
- `"de"` - German
- `"zh"` - Chinese
- `"ja"` - Japanese
- `"auto"` - Auto-detect

### Response Formats

| Format | Description | Use Case |
|--------|-------------|----------|
| `json` | Simple JSON with text | Basic transcription |
| `verbose_json` | Detailed JSON with timestamps, words, segments | Analysis, subtitles |
| `text` | Plain text only | Simple use cases |
| `srt` | SubRip subtitle format | Video subtitles |
| `vtt` | WebVTT subtitle format | Web video subtitles |

## Configuration

### Using Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using FluentAI.Configuration;
using FluentAI.Providers.OpenAI;

var services = new ServiceCollection();

// Configure OpenAI options
services.Configure<OpenAiOptions>(configuration.GetSection("OpenAI"));

// Register services
services.AddSingleton<OpenAiImageGenerationService>();
services.AddSingleton<OpenAiImageAnalysisService>();
services.AddSingleton<OpenAiAudioGenerationService>();
services.AddSingleton<OpenAiAudioTranscriptionService>();

// Build and resolve
var serviceProvider = services.BuildServiceProvider();
var imageService = serviceProvider.GetRequiredService<OpenAiImageGenerationService>();
```

### Configuration in appsettings.json

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "Model": "gpt-4-vision-preview",
    "IsAzureOpenAI": false,
    "Endpoint": "",
    "RequestTimeout": "00:05:00",
    "MaxRetries": 3
  }
}
```

### Azure OpenAI Configuration

```json
{
  "OpenAI": {
    "ApiKey": "your-azure-key",
    "IsAzureOpenAI": true,
    "Endpoint": "https://your-resource.openai.azure.com/",
    "Model": "gpt-4-vision"
  }
}
```

## Error Handling

### Common Exceptions

```csharp
using FluentAI.Abstractions.Exceptions;

try
{
    var response = await imageService.GenerateAsync(request);
}
catch (AiSdkConfigurationException ex)
{
    // Configuration error (missing API key, invalid endpoint, etc.)
    Console.WriteLine($"Configuration error: {ex.Message}");
}
catch (AiSdkException ex)
{
    // General AI SDK error
    Console.WriteLine($"AI SDK error: {ex.Message}");
}
catch (ArgumentException ex)
{
    // Invalid request parameters
    Console.WriteLine($"Invalid request: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    // File not found (for transcription from file)
    Console.WriteLine($"File not found: {ex.Message}");
}
```

### Validation

All services validate their inputs:

```csharp
// These will throw ArgumentException/ArgumentNullException
var request = new ImageGenerationRequest { Prompt = "" };  // Empty prompt
var request = new AudioTranscriptionRequest { AudioData = null };  // Null audio data
await transcriptionService.TranscribeFromFileAsync("/nonexistent/file.mp3");  // File not found
```

## Best Practices

### 1. Use Cancellation Tokens

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

try
{
    var response = await imageService.GenerateAsync(request, cts.Token);
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled or timed out");
}
```

### 2. Validate Configuration Early

```csharp
// Validate configuration before making requests
await imageService.ValidateConfigurationAsync();
await audioService.ValidateConfigurationAsync();
```

### 3. Handle Large Files Efficiently

```csharp
// For audio transcription with large files
using var fileStream = File.OpenRead("large-audio.mp3");
using var memoryStream = new MemoryStream();
await fileStream.CopyToAsync(memoryStream);

var request = new AudioTranscriptionRequest
{
    AudioData = memoryStream.ToArray(),
    FilePath = "large-audio.mp3"
};
```

### 4. Use Appropriate Image Sizes

```csharp
// DALL-E 3 supports: 1024x1024, 1792x1024, 1024x1792
// DALL-E 2 supports: 256x256, 512x512, 1024x1024

var request = new ImageGenerationRequest
{
    Prompt = "A landscape",
    Size = "1792x1024",  // Wide format for landscapes
    ModelOverride = "dall-e-3"
};
```

### 5. Optimize Prompt Quality

```csharp
// Good: Detailed, specific prompts
var goodPrompt = "A photorealistic image of a golden retriever puppy playing in a sunny park, " +
                 "with green grass, blue sky, and a red ball in the foreground";

// Less effective: Vague prompts
var vaguePrompt = "A dog";

var request = new ImageGenerationRequest
{
    Prompt = goodPrompt,
    Quality = "hd",
    Style = "natural"
};
```

### 6. Handle Costs Appropriately

```csharp
// Monitor token usage and costs
var response = await imageService.GenerateAsync(request);

if (response.TokenUsage != null)
{
    Console.WriteLine($"Prompt tokens: {response.TokenUsage.PromptTokens}");
    Console.WriteLine($"Completion tokens: {response.TokenUsage.CompletionTokens}");
    Console.WriteLine($"Total tokens: {response.TokenUsage.TotalTokens}");
}
```

### 7. Implement Retry Logic

```csharp
int maxRetries = 3;
int retryCount = 0;

while (retryCount < maxRetries)
{
    try
    {
        var response = await audioService.GenerateAsync(request);
        break;  // Success
    }
    catch (AiSdkException ex) when (retryCount < maxRetries - 1)
    {
        retryCount++;
        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
        await Task.Delay(delay);
    }
}
```

## Complete Examples

### End-to-End Image Generation Pipeline

```csharp
using FluentAI.Providers.OpenAI;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Options;

public class ImageGenerationPipeline
{
    private readonly OpenAiImageGenerationService _imageService;
    
    public ImageGenerationPipeline(IOptions<OpenAiOptions> options, ILogger<OpenAiImageGenerationService> logger)
    {
        _imageService = new OpenAiImageGenerationService(options, logger);
    }
    
    public async Task<string> GenerateAndSaveImageAsync(string prompt, string outputPath)
    {
        // Validate configuration
        await _imageService.ValidateConfigurationAsync();
        
        // Create request
        var request = new ImageGenerationRequest
        {
            Prompt = prompt,
            Size = "1024x1024",
            Quality = "hd",
            Style = "vivid",
            ResponseFormat = "url"
        };
        
        // Generate image
        var response = await _imageService.GenerateAsync(request);
        
        // Download and save
        if (response.Images.Any())
        {
            var firstImage = response.Images.First();
            if (!string.IsNullOrEmpty(firstImage.Url))
            {
                using var httpClient = new HttpClient();
                var imageBytes = await httpClient.GetByteArrayAsync(firstImage.Url);
                await File.WriteAllBytesAsync(outputPath, imageBytes);
                
                return firstImage.RevisedPrompt ?? prompt;
            }
        }
        
        throw new Exception("No image generated");
    }
}
```

### Audio Transcription with Subtitles

```csharp
public class SubtitleGenerator
{
    private readonly OpenAiAudioTranscriptionService _transcriptionService;
    
    public async Task<string> GenerateSrtSubtitlesAsync(string audioFilePath)
    {
        var request = new AudioTranscriptionRequest
        {
            AudioData = await File.ReadAllBytesAsync(audioFilePath),
            ResponseFormat = "srt",
            Language = "auto"
        };
        
        var response = await _transcriptionService.TranscribeAsync(request);
        
        var srtPath = Path.ChangeExtension(audioFilePath, ".srt");
        await File.WriteAllTextAsync(srtPath, response.Text);
        
        return srtPath;
    }
}
```

## Additional Resources

- [FluentAI.NET Main Documentation](README.md)
- [Configuration Guide](configuration.md)
- [API Reference](API-Reference.md)
- [OpenAI API Documentation](https://platform.openai.com/docs)
- [DALL-E Guide](https://platform.openai.com/docs/guides/images)
- [Whisper Guide](https://platform.openai.com/docs/guides/speech-to-text)

## Support

For issues, questions, or contributions:
- GitHub Issues: https://github.com/abxba0/fluentai-dotnet/issues
- Discussions: https://github.com/abxba0/fluentai-dotnet/discussions
