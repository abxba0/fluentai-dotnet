# HuggingFace Chat Completions Endpoint Support

This document demonstrates how to use the new HuggingFace chat completions endpoint support that was added to resolve the issue where token usage was returning 0 values.

## Problem Solved

The original issue was that when using the HuggingFace router chat completions endpoint:
```
https://router.huggingface.co/v1/chat/completions
```

The FluentAI.NET package was:
1. Sending requests in the wrong format (inference API format instead of OpenAI format)
2. Not extracting token usage from the OpenAI-compatible responses
3. Always returning `Input=0, Output=0` for token usage

## Solution

The package now automatically detects when you're using a chat completions endpoint and:
1. **Formats requests correctly** using OpenAI-compatible structure
2. **Extracts proper token usage** from the response
3. **Maintains backward compatibility** with existing inference API usage

## Configuration

### For Chat Completions Endpoint (NEW)

```json
{
  "HuggingFace": {
    "ApiKey": "hf_your_token_here",
    "ModelId": "https://router.huggingface.co/v1/chat/completions",
    "MaxRetries": 2,
    "RequestTimeout": "00:02:00"
  }
}
```

### Usage with Request Options

```csharp
var messages = new List<ChatMessage>
{
    new ChatMessage(ChatRole.User, "Hello! Please introduce yourself briefly.")
};

var requestOptions = new HuggingFaceRequestOptions
{
    Model = "openai/gpt-oss-20b",  // NEW: Specify the model for chat completions
    Temperature = 0.7f,
    MaxNewTokens = 1000
};

var response = await chatModel.GetResponseAsync(messages, requestOptions, CancellationToken.None);

// Now returns proper values instead of Input=0, Output=0
Console.WriteLine($"Model ID: {response.ModelId}");
Console.WriteLine($"Finish Reason: {response.FinishReason}");
Console.WriteLine($"Token Usage: Input={response.Usage.InputTokens}, Output={response.Usage.OutputTokens}");
```

## Request Format Changes

### Chat Completions Endpoint (Automatic Detection)
When `ModelId` contains "/chat/completions", the package sends:
```json
{
  "messages": [
    {"role": "user", "content": "Hello! Please introduce yourself briefly."}
  ],
  "model": "openai/gpt-oss-20b",
  "temperature": 0.7,
  "max_tokens": 1000,
  "stream": false
}
```

### Traditional Inference API (Backward Compatible)
When `ModelId` does NOT contain "/chat/completions", the package still sends:
```json
{
  "inputs": "User: Hello! Please introduce yourself briefly.\nAssistant: ",
  "parameters": {
    "temperature": 0.7,
    "max_new_tokens": 1000
  },
  "stream": false
}
```

## Response Processing

### Chat Completions Response (NEW)
The package now correctly extracts from OpenAI-compatible responses:
```json
{
  "choices": [
    {
      "message": {
        "content": "Hello! I'm an AI assistant created by Anthropic..."
      },
      "finish_reason": "stop"
    }
  ],
  "model": "openai/gpt-oss-20b",
  "usage": {
    "prompt_tokens": 15,
    "completion_tokens": 12,
    "total_tokens": 27
  }
}
```

### Traditional Inference Response (Unchanged)
Still handles traditional responses correctly:
```json
[
  {
    "generated_text": "User: Hello!\nAssistant: Hello! How can I help you today?"
  }
]
```

## Key Benefits

1. **Automatic Detection**: No breaking changes - works based on endpoint URL
2. **Proper Token Usage**: Extracts actual token counts from chat completions responses
3. **Model Flexibility**: Can specify different models per request
4. **Backward Compatibility**: Existing inference API usage continues to work
5. **Parameter Mapping**: Correctly maps parameters for each endpoint type

## Migration Guide

If you're currently using the inference API and want to switch to chat completions:

1. **Update your configuration** to use the chat completions endpoint URL
2. **Add the Model property** to your HuggingFaceRequestOptions
3. **No code changes required** - the package handles the rest automatically

That's it! The package will automatically detect the endpoint type and format requests/responses accordingly.