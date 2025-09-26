# Multi-Modal AI Support in FluentAI.NET

FluentAI.NET now supports multi-modal AI operations while maintaining full backward compatibility with existing `IChatModel` usage. This guide shows you how to get started with text generation, image analysis, image generation, and audio processing.

## Quick Start

### 1. Basic Setup (Backward Compatible)

Your existing FluentAI.NET code continues to work unchanged:

```csharp
// Existing code - no changes needed
services.AddFluentAI()
    .AddOpenAI(config => config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .UseDefaultProvider("OpenAI");

// Your existing IChatModel usage works exactly the same
var chatModel = serviceProvider.GetRequiredService<IChatModel>();
var response = await chatModel.GetResponseAsync(messages);
```

### 2. Adding Multi-Modal Support

Add multi-modal capabilities to your existing setup:

```csharp
services.AddFluentAI()
    .AddOpenAI(config => config.ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
    .UseDefaultProvider("OpenAI")
    .AddMultiModalSupport(configuration);  // <- Add this line
```

### 3. Configuration

Add multi-modal configuration to your `appsettings.json`:

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI",
    "MultiModal": {
      "DefaultStrategy": "Performance",
      "Models": {
        "TextGeneration": {
          "Primary": {
            "Provider": "OpenAI",
            "ModelName": "gpt-4",
            "MaxTokens": 4096,
            "Temperature": 0.7
          }
        },
        "ImageAnalysis": {
          "Primary": {
            "Provider": "OpenAI",
            "ModelName": "gpt-4-vision-preview",
            "DetailLevel": "high"
          }
        },
        "ImageGeneration": {
          "Primary": {
            "Provider": "OpenAI",
            "ModelName": "dall-e-3",
            "Quality": "hd",
            "Size": "1024x1024"
          }
        }
      }
    }
  }
}
```

## Using Multi-Modal Services

### Text Generation

```csharp
public class MyService
{
    private readonly ITextGenerationService _textService;

    public MyService(ITextGenerationService textService)
    {
        _textService = textService;
    }

    public async Task<string> GenerateText()
    {
        var request = new TextRequest
        {
            Prompt = "Explain quantum computing",
            MaxTokens = 500,
            Temperature = 0.7f,
            SystemMessage = "You are an expert science communicator."
        };

        var response = await _textService.GenerateAsync(request);
        return response.Content;
    }
}
```

### Image Analysis

```csharp
public class ImageAnalysisService
{
    private readonly IImageAnalysisService _imageService;

    public async Task<string> AnalyzeImage(string imageUrl)
    {
        var request = new ImageAnalysisRequest
        {
            ImageUrl = imageUrl,
            Prompt = "Describe what you see in this image",
            DetailLevel = "high"
        };

        var response = await _imageService.AnalyzeAsync(request);
        return response.Analysis;
    }

    public async Task<string> AnalyzeImageFromBytes(byte[] imageData)
    {
        var response = await _imageService.AnalyzeFromBytesAsync(
            imageData, 
            "What objects can you identify in this image?"
        );
        return response.Analysis;
    }
}
```

### Image Generation

```csharp
public class ImageCreationService
{
    private readonly IImageGenerationService _imageService;

    public async Task<string> CreateImage(string prompt)
    {
        var request = new ImageGenerationRequest
        {
            Prompt = prompt,
            Size = "1024x1024",
            Quality = "hd",
            Style = "vivid",
            NumberOfImages = 1
        };

        var response = await _imageService.GenerateAsync(request);
        return response.Images.First().Url;
    }
}
```

### Audio Transcription

```csharp
public class AudioService
{
    private readonly IAudioTranscriptionService _audioService;

    public async Task<string> TranscribeAudio(string audioFilePath)
    {
        var response = await _audioService.TranscribeFromFileAsync(audioFilePath);
        return response.Text;
    }

    public async Task<string> TranscribeAudioBytes(byte[] audioData)
    {
        var request = new AudioTranscriptionRequest
        {
            AudioData = audioData,
            Language = "en",
            ResponseFormat = "json"
        };

        var response = await _audioService.TranscribeAsync(request);
        return response.Text;
    }
}
```

## Key Benefits

### 🔄 Zero Breaking Changes
- Existing `IChatModel` code works unchanged
- Seamless upgrade path
- Backward compatibility guaranteed

### 🎯 Unified Interface
- All services implement `IAiService`
- Consistent error handling
- Standardized configuration

### 🔧 Configuration-Driven
- Primary and fallback models
- Environment-specific overrides
- Provider abstraction

### 🚀 Provider Agnostic
- Easy provider switching
- Automatic failover
- Consistent API across providers

### 📊 Enhanced Monitoring
- Processing time tracking
- Token usage monitoring
- Performance metrics

## Next Steps

- [Configuration Guide](configuration.md) - Detailed configuration options
- [Provider Support](providers.md) - Supported providers and capabilities
- [Error Handling](error-handling.md) - Comprehensive error handling
- [Advanced Usage](advanced-usage.md) - Advanced patterns and best practices

## Migration from Existing Code

Your existing FluentAI.NET code requires **no changes**. Simply add multi-modal support alongside your existing setup:

```csharp
// Before (still works)
services.AddFluentAI()
    .AddOpenAI(config => config.ApiKey = apiKey)
    .UseDefaultProvider("OpenAI");

// After (adds multi-modal support)
services.AddFluentAI()
    .AddOpenAI(config => config.ApiKey = apiKey)
    .UseDefaultProvider("OpenAI")
    .AddMultiModalSupport(configuration);  // New capability added
```

Both your existing `IChatModel` and new multi-modal services will work side by side.