using FluentAI.Abstractions.MCP;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FluentAI.MCP.Adapters;

/// <summary>
/// OpenAI-specific tool schema adapter for converting MCP tools to OpenAI function calling format.
/// </summary>
public class OpenAiToolAdapter : IToolSchemaAdapter
{
    private readonly ILogger<OpenAiToolAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the OpenAiToolAdapter class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public OpenAiToolAdapter(ILogger<OpenAiToolAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string ProviderId => "OpenAI";

    /// <inheritdoc />
    public bool CanAdapt(ToolSchema mcpSchema)
    {
        ArgumentNullException.ThrowIfNull(mcpSchema);

        // OpenAI can adapt most tool schemas as long as they have a name
        var canAdapt = !string.IsNullOrEmpty(mcpSchema.Name);
        
        _logger.LogDebug("OpenAI adapter can adapt tool {ToolName}: {CanAdapt}", 
            mcpSchema.Name, canAdapt);

        return canAdapt;
    }

    /// <inheritdoc />
    public ProviderToolSchema AdaptSchema(ToolSchema mcpSchema)
    {
        ArgumentNullException.ThrowIfNull(mcpSchema);

        if (!CanAdapt(mcpSchema))
            throw new ArgumentException($"Cannot adapt tool schema: {mcpSchema.Name}");

        _logger.LogDebug("Adapting MCP tool {ToolName} to OpenAI function schema", mcpSchema.Name);

        var openAiSchema = new OpenAiFunctionSchema
        {
            Name = mcpSchema.Name,
            Description = CreateSafeDescription(mcpSchema.Description),
            OriginalSchema = mcpSchema
        };

        // Convert MCP input schema to OpenAI parameters format
        if (mcpSchema.InputSchema != null)
        {
            openAiSchema.Parameters = ConvertToOpenAiParameters(mcpSchema.InputSchema);
        }
        else
        {
            // Provide empty parameters schema if none exists
            openAiSchema.Parameters = JsonSerializer.SerializeToDocument(new
            {
                type = "object",
                properties = new { },
                required = new string[0]
            });
        }

        _logger.LogDebug("Successfully adapted tool {ToolName} to OpenAI format", mcpSchema.Name);
        return openAiSchema;
    }

    /// <inheritdoc />
    public ProviderToolCall AdaptToolCall(ToolCall toolCall)
    {
        ArgumentNullException.ThrowIfNull(toolCall);

        _logger.LogDebug("Adapting MCP tool call {ToolName} to OpenAI format", toolCall.ToolName);

        var openAiCall = new OpenAiFunctionCall
        {
            CallId = toolCall.CallId,
            ToolName = toolCall.ToolName,
            Metadata = new Dictionary<string, object>(toolCall.Metadata)
        };

        // Convert parameters to JSON string format expected by OpenAI
        if (toolCall.Parameters != null)
        {
            openAiCall.Arguments = toolCall.Parameters.RootElement.GetRawText();
        }
        else
        {
            openAiCall.Arguments = "{}";
        }

        _logger.LogDebug("Successfully adapted tool call {ToolName} to OpenAI format", toolCall.ToolName);
        return openAiCall;
    }

    /// <inheritdoc />
    public ToolResult AdaptResult(ProviderToolResult providerResult)
    {
        ArgumentNullException.ThrowIfNull(providerResult);

        if (providerResult is not OpenAiFunctionResult openAiResult)
            throw new ArgumentException("Provider result must be OpenAiFunctionResult for OpenAI adapter");

        _logger.LogDebug("Adapting OpenAI function result to MCP format");

        var mcpResult = new ToolResult
        {
            CallId = openAiResult.CallId,
            IsSuccess = openAiResult.IsSuccess,
            Metadata = new Dictionary<string, object>(openAiResult.Metadata)
        };

        if (openAiResult.IsSuccess && !string.IsNullOrEmpty(openAiResult.Content))
        {
            // Wrap the content in a simple JSON structure
            var resultContent = new
            {
                content = openAiResult.Content,
                type = "text"
            };
            mcpResult.Content = JsonSerializer.SerializeToDocument(resultContent);
        }
        else if (!string.IsNullOrEmpty(openAiResult.Error))
        {
            mcpResult.Error = new ToolError
            {
                Code = "execution_error",
                Message = openAiResult.Error
            };
        }

        _logger.LogDebug("Successfully adapted OpenAI result to MCP format");
        return mcpResult;
    }

    private string CreateSafeDescription(string? description)
    {
        // Ensure we have a description for OpenAI, as it helps with function calling
        return string.IsNullOrEmpty(description) 
            ? "MCP tool function" 
            : description.Length > 1000 
                ? description.Substring(0, 997) + "..." 
                : description;
    }

    private JsonDocument ConvertToOpenAiParameters(JsonDocument mcpInputSchema)
    {
        try
        {
            // If the MCP schema is already in JSON Schema format, we can use it directly
            // OpenAI expects JSON Schema for function parameters
            var root = mcpInputSchema.RootElement;

            // Check if it looks like a valid JSON Schema
            if (root.TryGetProperty("type", out var typeProperty) && 
                typeProperty.GetString() == "object")
            {
                return mcpInputSchema;
            }

            // If it's not a standard JSON Schema, wrap it as an object schema
            var wrappedSchema = new
            {
                type = "object",
                properties = new
                {
                    input = mcpInputSchema.RootElement
                },
                required = new[] { "input" }
            };

            return JsonSerializer.SerializeToDocument(wrappedSchema);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert MCP input schema to OpenAI format, using fallback");

            // Fallback to a generic object schema
            var fallbackSchema = new
            {
                type = "object",
                properties = new { },
                required = new string[0]
            };

            return JsonSerializer.SerializeToDocument(fallbackSchema);
        }
    }
}