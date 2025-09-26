using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;

namespace FluentAI.Examples.ConsoleApp.Services;

/// <summary>
/// Demonstrates multi-modal AI capabilities and how they integrate with existing FluentAI.NET functionality.
/// </summary>
public class MultiModalDemoService
{
    private readonly IChatModel _chatModel;
    private readonly ITextGenerationService? _textGenerationService;
    private readonly IImageAnalysisService? _imageAnalysisService;
    private readonly IImageGenerationService? _imageGenerationService;
    private readonly IAudioTranscriptionService? _audioTranscriptionService;
    private readonly IAudioGenerationService? _audioGenerationService;
    private readonly ILogger<MultiModalDemoService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiModalDemoService"/> class.
    /// </summary>
    /// <param name="chatModel">The traditional chat model (for backward compatibility).</param>
    /// <param name="textGenerationService">The multi-modal text generation service.</param>
    /// <param name="imageAnalysisService">The image analysis service.</param>
    /// <param name="imageGenerationService">The image generation service.</param>
    /// <param name="audioTranscriptionService">The audio transcription service.</param>
    /// <param name="audioGenerationService">The audio generation service.</param>
    /// <param name="logger">The logger instance.</param>
    public MultiModalDemoService(
        IChatModel chatModel,
        ITextGenerationService? textGenerationService,
        IImageAnalysisService? imageAnalysisService,
        IImageGenerationService? imageGenerationService,
        IAudioTranscriptionService? audioTranscriptionService,
        IAudioGenerationService? audioGenerationService,
        ILogger<MultiModalDemoService> logger)
    {
        _chatModel = chatModel ?? throw new ArgumentNullException(nameof(chatModel));
        _textGenerationService = textGenerationService;
        _imageAnalysisService = imageAnalysisService;
        _imageGenerationService = imageGenerationService;
        _audioTranscriptionService = audioTranscriptionService;
        _audioGenerationService = audioGenerationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Runs a comprehensive demonstration of multi-modal capabilities.
    /// </summary>
    public async Task RunMultiModalDemo()
    {
        Console.WriteLine("🚀 FluentAI.NET Multi-Modal Capabilities Demo");
        Console.WriteLine("=============================================");
        Console.WriteLine();

        await ShowBackwardCompatibility();
        await ShowMultiModalTextGeneration();
        await ShowImageAnalysisCapabilities();
        await ShowImageGenerationCapabilities();
        await ShowAudioTranscriptionCapabilities();
        await ShowAudioGenerationCapabilities();
        await ShowMultiModalIntegration();
    }

    /// <summary>
    /// Demonstrates that existing IChatModel functionality continues to work.
    /// </summary>
    private async Task ShowBackwardCompatibility()
    {
        Console.WriteLine("✅ Backward Compatibility");
        Console.WriteLine("   ─────────────────────");
        Console.WriteLine();

        Console.WriteLine("FluentAI.NET maintains full backward compatibility.");
        Console.WriteLine("Your existing IChatModel code continues to work unchanged:");
        Console.WriteLine();

        try
        {
            // This is the traditional way of using FluentAI.NET
            var messages = new[]
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                new ChatMessage(ChatRole.User, "Hello! How are you?")
            };

            Console.WriteLine("🔄 Using traditional IChatModel interface...");
            var response = await _chatModel.GetResponseAsync(messages);
            
            Console.WriteLine($"✓ Response: {TruncateText(response.Content, 100)}");
            Console.WriteLine($"✓ Model: {response.ModelId}");
            Console.WriteLine($"✓ Tokens Used: {response.Usage.TotalTokens}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("💡 This existing code works exactly as before!");
        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates the new multi-modal text generation service.
    /// </summary>
    private async Task ShowMultiModalTextGeneration()
    {
        Console.WriteLine("🎯 Multi-Modal Text Generation");
        Console.WriteLine("   ───────────────────────────");
        Console.WriteLine();

        if (_textGenerationService == null)
        {
            Console.WriteLine("❌ Text generation service not available (not configured)");
            Console.WriteLine();
            return;
        }

        Console.WriteLine("New multi-modal approach with enhanced capabilities:");
        Console.WriteLine();

        try
        {
            var request = new TextRequest
            {
                Prompt = "Explain quantum computing in simple terms",
                MaxTokens = 200,
                Temperature = 0.7f,
                SystemMessage = "You are an expert science communicator."
            };

            Console.WriteLine("🔄 Using new ITextGenerationService interface...");
            var response = await _textGenerationService.GenerateAsync(request);
            
            Console.WriteLine($"✓ Response: {TruncateText(response.Content, 150)}");
            Console.WriteLine($"✓ Provider: {response.Provider}");
            Console.WriteLine($"✓ Model: {response.ModelUsed}");
            Console.WriteLine($"✓ Processing Time: {response.ProcessingTime.TotalMilliseconds:F0}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates image analysis capabilities.
    /// </summary>
    private async Task ShowImageAnalysisCapabilities()
    {
        Console.WriteLine("🖼️ Image Analysis Capabilities");
        Console.WriteLine("   ──────────────────────────");
        Console.WriteLine();

        if (_imageAnalysisService == null)
        {
            Console.WriteLine("❌ Image analysis service not available (not configured)");
        }
        else
        {
            Console.WriteLine("🔍 Image Analysis Features:");
            Console.WriteLine($"   • Provider: {_imageAnalysisService.ProviderName}");
            Console.WriteLine($"   • Default Model: {_imageAnalysisService.DefaultModelName}");
            Console.WriteLine("   • Analyze images from URLs");
            Console.WriteLine("   • Analyze images from byte arrays");
            Console.WriteLine("   • Extract text (OCR)");
            Console.WriteLine("   • Object detection");
            Console.WriteLine("   • Scene understanding");
            Console.WriteLine();
            Console.WriteLine("📝 Example usage:");
            Console.WriteLine("   var request = new ImageAnalysisRequest");
            Console.WriteLine("   {");
            Console.WriteLine("       ImageUrl = \"https://example.com/image.jpg\",");
            Console.WriteLine("       Prompt = \"Describe what you see in this image\"");
            Console.WriteLine("   };");
            Console.WriteLine("   var result = await service.AnalyzeAsync(request);");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates image generation capabilities.
    /// </summary>
    private async Task ShowImageGenerationCapabilities()
    {
        Console.WriteLine("🎨 Image Generation Capabilities");
        Console.WriteLine("   ─────────────────────────────");
        Console.WriteLine();

        if (_imageGenerationService == null)
        {
            Console.WriteLine("❌ Image generation service not available (not configured)");
        }
        else
        {
            Console.WriteLine("🎨 Image Generation Features:");
            Console.WriteLine($"   • Provider: {_imageGenerationService.ProviderName}");
            Console.WriteLine($"   • Default Model: {_imageGenerationService.DefaultModelName}");
            Console.WriteLine("   • Generate images from text prompts");
            Console.WriteLine("   • Edit existing images with masks");
            Console.WriteLine("   • Create variations of images");
            Console.WriteLine("   • Multiple output formats (URL, base64)");
            Console.WriteLine("   • Quality and style controls");
            Console.WriteLine();
            Console.WriteLine("📝 Example usage:");
            Console.WriteLine("   var request = new ImageGenerationRequest");
            Console.WriteLine("   {");
            Console.WriteLine("       Prompt = \"A futuristic city at sunset\",");
            Console.WriteLine("       Size = \"1024x1024\",");
            Console.WriteLine("       Quality = \"hd\"");
            Console.WriteLine("   };");
            Console.WriteLine("   var result = await service.GenerateAsync(request);");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates audio transcription capabilities.
    /// </summary>
    private async Task ShowAudioTranscriptionCapabilities()
    {
        Console.WriteLine("🎤 Audio Transcription Capabilities");
        Console.WriteLine("   ─────────────────────────────────");
        Console.WriteLine();

        if (_audioTranscriptionService == null)
        {
            Console.WriteLine("❌ Audio transcription service not available (not configured)");
        }
        else
        {
            Console.WriteLine("🎤 Audio Transcription Features:");
            Console.WriteLine($"   • Provider: {_audioTranscriptionService.ProviderName}");
            Console.WriteLine($"   • Default Model: {_audioTranscriptionService.DefaultModelName}");
            Console.WriteLine("   • Transcribe from audio files");
            Console.WriteLine("   • Transcribe from byte arrays");
            Console.WriteLine("   • Automatic language detection");
            Console.WriteLine("   • Word-level timestamps");
            Console.WriteLine("   • Multiple output formats (JSON, SRT, VTT)");
            Console.WriteLine();
            Console.WriteLine("📝 Example usage:");
            Console.WriteLine("   var result = await service.TranscribeFromFileAsync(\"audio.mp3\");");
            Console.WriteLine("   Console.WriteLine($\"Transcript: {result.Text}\");");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates audio generation capabilities.
    /// </summary>
    private async Task ShowAudioGenerationCapabilities()
    {
        Console.WriteLine("🔊 Audio Generation Capabilities");
        Console.WriteLine("   ──────────────────────────────");
        Console.WriteLine();

        if (_audioGenerationService == null)
        {
            Console.WriteLine("❌ Audio generation service not available (not configured)");
        }
        else
        {
            Console.WriteLine("🔊 Audio Generation Features:");
            Console.WriteLine($"   • Provider: {_audioGenerationService.ProviderName}");
            Console.WriteLine($"   • Default Model: {_audioGenerationService.DefaultModelName}");
            Console.WriteLine("   • Text-to-speech conversion");
            Console.WriteLine("   • Multiple voice options");
            Console.WriteLine("   • Speed control");
            Console.WriteLine("   • Multiple output formats (MP3, WAV, etc.)");
            Console.WriteLine();
            Console.WriteLine("📝 Example usage:");
            Console.WriteLine("   var request = new AudioGenerationRequest");
            Console.WriteLine("   {");
            Console.WriteLine("       Text = \"Hello, this is a test of text-to-speech\",");
            Console.WriteLine("       Voice = \"alloy\",");
            Console.WriteLine("       Speed = 1.0f");
            Console.WriteLine("   };");
            Console.WriteLine("   var result = await service.GenerateAsync(request);");
        }

        Console.WriteLine();
    }

    /// <summary>
    /// Demonstrates how different modalities can work together.
    /// </summary>
    private async Task ShowMultiModalIntegration()
    {
        Console.WriteLine("🔗 Multi-Modal Integration");
        Console.WriteLine("   ────────────────────────");
        Console.WriteLine();

        Console.WriteLine("💡 Key Benefits of Multi-Modal Approach:");
        Console.WriteLine("   ✅ Unified interface (IAiService) for all modalities");
        Console.WriteLine("   ✅ Consistent configuration and provider abstraction");
        Console.WriteLine("   ✅ Automatic failover between providers");
        Console.WriteLine("   ✅ Provider-agnostic application code");
        Console.WriteLine("   ✅ Comprehensive error handling and logging");
        Console.WriteLine("   ✅ Performance monitoring and caching");
        Console.WriteLine("   ✅ Security and content filtering");
        Console.WriteLine();

        Console.WriteLine("🚀 Example Multi-Modal Workflow:");
        Console.WriteLine("   1. Generate an image from text prompt");
        Console.WriteLine("   2. Analyze the generated image");
        Console.WriteLine("   3. Create audio description of the analysis");
        Console.WriteLine("   4. Generate follow-up text based on the analysis");
        Console.WriteLine();

        Console.WriteLine("📊 Configuration-Driven Model Selection:");
        Console.WriteLine("   • Primary models for optimal performance");
        Console.WriteLine("   • Fallback models for reliability");
        Console.WriteLine("   • Environment-specific overrides");
        Console.WriteLine("   • Cost optimization strategies");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Truncates text to a specified maximum length.
    /// </summary>
    /// <param name="text">The text to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <returns>The truncated text.</returns>
    private static string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }
}