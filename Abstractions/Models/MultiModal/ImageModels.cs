namespace FluentAI.Abstractions.Models;

/// <summary>
/// Request for image analysis operations.
/// </summary>
public class ImageAnalysisRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the analysis prompt.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image data as bytes.
    /// </summary>
    public byte[]? ImageData { get; set; }

    /// <summary>
    /// Gets or sets the URL of the image to analyze.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the format of the image (e.g., "jpeg", "png").
    /// </summary>
    public string? ImageFormat { get; set; }

    /// <summary>
    /// Gets or sets the detail level for analysis ("low", "high", "auto").
    /// </summary>
    public string DetailLevel { get; set; } = "auto";

    /// <summary>
    /// Gets or sets the maximum number of tokens for the analysis response.
    /// </summary>
    public int? MaxTokens { get; set; }
}

/// <summary>
/// Response from image analysis operations.
/// </summary>
public class ImageAnalysisResponse : MultiModalResponse
{
    /// <summary>
    /// Gets or sets the analysis result text.
    /// </summary>
    public string Analysis { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score of the analysis (0.0 to 1.0).
    /// </summary>
    public float? ConfidenceScore { get; set; }

    /// <summary>
    /// Gets or sets detected objects in the image.
    /// </summary>
    public IEnumerable<DetectedObject>? DetectedObjects { get; set; }

    /// <summary>
    /// Gets or sets extracted text from the image (OCR).
    /// </summary>
    public string? ExtractedText { get; set; }
}

/// <summary>
/// Request for image generation operations.
/// </summary>
public class ImageGenerationRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the text prompt for image generation.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image size (e.g., "1024x1024", "512x512").
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets the quality level ("standard", "hd").
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Gets or sets the style ("vivid", "natural").
    /// </summary>
    public string? Style { get; set; }

    /// <summary>
    /// Gets or sets the number of images to generate.
    /// </summary>
    public int NumberOfImages { get; set; } = 1;

    /// <summary>
    /// Gets or sets the response format ("url", "b64_json").
    /// </summary>
    public string ResponseFormat { get; set; } = "url";
}

/// <summary>
/// Request for image editing operations.
/// </summary>
public class ImageEditRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the original image data.
    /// </summary>
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the mask image data for inpainting.
    /// </summary>
    public byte[]? MaskData { get; set; }

    /// <summary>
    /// Gets or sets the prompt describing the desired edit.
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the image size for the output.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets the number of edited images to generate.
    /// </summary>
    public int NumberOfImages { get; set; } = 1;
}

/// <summary>
/// Request for image variation operations.
/// </summary>
public class ImageVariationRequest : MultiModalRequest
{
    /// <summary>
    /// Gets or sets the source image data.
    /// </summary>
    public byte[] ImageData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the image size for variations.
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// Gets or sets the number of variations to generate.
    /// </summary>
    public int NumberOfImages { get; set; } = 1;

    /// <summary>
    /// Gets or sets the response format ("url", "b64_json").
    /// </summary>
    public string ResponseFormat { get; set; } = "url";
}

/// <summary>
/// Response from image generation operations.
/// </summary>
public class ImageGenerationResponse : MultiModalResponse
{
    /// <summary>
    /// Gets or sets the generated images.
    /// </summary>
    public IEnumerable<GeneratedImage> Images { get; set; } = Array.Empty<GeneratedImage>();

    /// <summary>
    /// Gets or sets the revised prompt that was actually used.
    /// </summary>
    public string RevisedPrompt { get; set; } = string.Empty;
}

/// <summary>
/// Represents a detected object in an image.
/// </summary>
public class DetectedObject
{
    /// <summary>
    /// Gets or sets the name/label of the detected object.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidence score for this detection (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; set; }

    /// <summary>
    /// Gets or sets the bounding box coordinates (x, y, width, height).
    /// </summary>
    public BoundingBox? BoundingBox { get; set; }
}

/// <summary>
/// Represents a bounding box for object detection.
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// Gets or sets the X coordinate of the top-left corner.
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate of the top-left corner.
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the bounding box.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the bounding box.
    /// </summary>
    public int Height { get; set; }
}

/// <summary>
/// Represents a generated image.
/// </summary>
public class GeneratedImage
{
    /// <summary>
    /// Gets or sets the URL of the generated image.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the base64-encoded image data.
    /// </summary>
    public string? Base64Data { get; set; }

    /// <summary>
    /// Gets or sets the revised prompt used for this image.
    /// </summary>
    public string? RevisedPrompt { get; set; }
}