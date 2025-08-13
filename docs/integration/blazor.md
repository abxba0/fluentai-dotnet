# Blazor Applications Integration

This guide shows how to integrate FluentAI.NET into Blazor applications (both Blazor Server and Blazor WebAssembly).

## Blazor Server Integration

### 1. Create Project and Install Package

```bash
dotnet new blazorserver -n MyAIBlazorApp
cd MyAIBlazorApp
dotnet add package FluentAI.NET
```

### 2. Configure Services in Program.cs

```csharp
using FluentAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add FluentAI services
builder.Services.AddAiSdk(builder.Configuration)
    .AddOpenAiChatModel(builder.Configuration)
    .AddAnthropicChatModel(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

### 3. Add Configuration

Update `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "FluentAI": "Debug"
    }
  },
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-3.5-turbo",
    "MaxTokens": 1000,
    "RequestTimeout": "00:01:30"
  },
  "AllowedHosts": "*"
}
```

### 4. Create a Chat Component

Create `Components/Chat.razor`:

```razor
@page "/chat"
@using FluentAI.Abstractions
@using FluentAI.Abstractions.Models
@using FluentAI.Abstractions.Exceptions
@inject IChatModel ChatModel
@inject IJSRuntime JSRuntime

<PageTitle>AI Chat</PageTitle>

<h3>AI Chat Assistant</h3>

<div class="chat-container">
    <div class="messages-container" @ref="messagesContainer">
        @foreach (var message in messages)
        {
            <div class="message @(message.Role == ChatRole.User ? "user-message" : "ai-message")">
                <div class="message-content">
                    @if (message.Role == ChatRole.User)
                    {
                        <strong>You:</strong>
                    }
                    else
                    {
                        <strong>AI:</strong>
                    }
                    <span>@message.Content</span>
                </div>
                <div class="message-time">
                    @message.Timestamp.ToString("HH:mm:ss")
                </div>
            </div>
        }
        
        @if (isLoading)
        {
            <div class="message ai-message">
                <div class="message-content">
                    <strong>AI:</strong>
                    <span class="typing-indicator">
                        <span></span>
                        <span></span>
                        <span></span>
                    </span>
                </div>
            </div>
        }
    </div>

    <div class="input-container">
        <div class="input-group">
            <input @bind="currentMessage" 
                   @onkeypress="HandleKeyPress" 
                   placeholder="Type your message..." 
                   disabled="@isLoading" 
                   class="form-control" />
            <button @onclick="SendMessage" 
                    disabled="@(isLoading || string.IsNullOrWhiteSpace(currentMessage))" 
                    class="btn btn-primary">
                @if (isLoading)
                {
                    <span class="spinner-border spinner-border-sm" role="status"></span>
                }
                else
                {
                    <text>Send</text>
                }
            </button>
        </div>
        
        <div class="controls">
            <label>
                <input type="checkbox" @bind="useStreaming" disabled="@isLoading" />
                Enable Streaming
            </label>
            <button @onclick="ClearChat" disabled="@isLoading" class="btn btn-secondary btn-sm">
                Clear Chat
            </button>
        </div>
    </div>
</div>

@if (!string.IsNullOrEmpty(errorMessage))
{
    <div class="alert alert-danger mt-3">
        @errorMessage
    </div>
}

<style>
    .chat-container {
        max-width: 800px;
        margin: 0 auto;
        border: 1px solid #dee2e6;
        border-radius: 8px;
        overflow: hidden;
    }

    .messages-container {
        height: 400px;
        overflow-y: auto;
        padding: 1rem;
        background-color: #f8f9fa;
    }

    .message {
        margin-bottom: 1rem;
        padding: 0.75rem;
        border-radius: 8px;
        max-width: 80%;
    }

    .user-message {
        background-color: #007bff;
        color: white;
        margin-left: auto;
    }

    .ai-message {
        background-color: white;
        border: 1px solid #dee2e6;
    }

    .message-content {
        margin-bottom: 0.25rem;
    }

    .message-time {
        font-size: 0.75rem;
        opacity: 0.7;
    }

    .input-container {
        padding: 1rem;
        background-color: white;
        border-top: 1px solid #dee2e6;
    }

    .input-group {
        display: flex;
        gap: 0.5rem;
        margin-bottom: 0.5rem;
    }

    .input-group input {
        flex: 1;
    }

    .controls {
        display: flex;
        justify-content: space-between;
        align-items: center;
        font-size: 0.875rem;
    }

    .typing-indicator {
        display: inline-flex;
        gap: 2px;
    }

    .typing-indicator span {
        width: 6px;
        height: 6px;
        border-radius: 50%;
        background-color: #6c757d;
        animation: typing 1.4s infinite ease-in-out;
    }

    .typing-indicator span:nth-child(1) { animation-delay: -0.32s; }
    .typing-indicator span:nth-child(2) { animation-delay: -0.16s; }

    @keyframes typing {
        0%, 80%, 100% { transform: scale(0); }
        40% { transform: scale(1); }
    }
</style>

@code {
    private List<ChatMessageModel> messages = new();
    private string currentMessage = "";
    private bool isLoading = false;
    private bool useStreaming = false;
    private string errorMessage = "";
    private ElementReference messagesContainer;

    protected override void OnInitialized()
    {
        // Add welcome message
        messages.Add(new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = "Hello! I'm your AI assistant. How can I help you today?",
            Timestamp = DateTime.Now
        });
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(currentMessage) || isLoading)
            return;

        var userMessage = currentMessage.Trim();
        currentMessage = "";
        errorMessage = "";

        // Add user message
        messages.Add(new ChatMessageModel
        {
            Role = ChatRole.User,
            Content = userMessage,
            Timestamp = DateTime.Now
        });

        isLoading = true;
        StateHasChanged();
        await ScrollToBottom();

        try
        {
            var chatMessages = messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList();

            if (useStreaming)
            {
                await HandleStreamingResponse(chatMessages);
            }
            else
            {
                await HandleRegularResponse(chatMessages);
            }
        }
        catch (AiSdkRateLimitException)
        {
            errorMessage = "Rate limit exceeded. Please wait a moment and try again.";
        }
        catch (AiSdkException ex)
        {
            errorMessage = $"AI service error: {ex.Message}";
        }
        catch (Exception ex)
        {
            errorMessage = $"An unexpected error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
            await ScrollToBottom();
        }
    }

    private async Task HandleRegularResponse(List<ChatMessage> chatMessages)
    {
        var response = await ChatModel.GetResponseAsync(chatMessages);
        
        messages.Add(new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = response.Content,
            Timestamp = DateTime.Now
        });
    }

    private async Task HandleStreamingResponse(List<ChatMessage> chatMessages)
    {
        var aiMessage = new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = "",
            Timestamp = DateTime.Now
        };
        
        messages.Add(aiMessage);
        
        await foreach (var token in ChatModel.StreamResponseAsync(chatMessages))
        {
            aiMessage.Content += token;
            StateHasChanged();
            await ScrollToBottom();
            await Task.Delay(10); // Small delay for visual effect
        }
    }

    private async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private void ClearChat()
    {
        messages.Clear();
        messages.Add(new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = "Hello! I'm your AI assistant. How can I help you today?",
            Timestamp = DateTime.Now
        });
        errorMessage = "";
        StateHasChanged();
    }

    private async Task ScrollToBottom()
    {
        await Task.Delay(50); // Allow DOM to update
        await JSRuntime.InvokeVoidAsync("scrollToBottom", messagesContainer);
    }

    private class ChatMessageModel
    {
        public ChatRole Role { get; set; }
        public string Content { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
```

### 5. Add JavaScript for Scrolling

Add to `wwwroot/js/site.js` or create a new file:

```javascript
window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};
```

### 6. Update Navigation

Add to `Shared/NavMenu.razor`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="chat">
        <span class="oi oi-chat" aria-hidden="true"></span> AI Chat
    </NavLink>
</div>
```

## Blazor WebAssembly Integration

### 1. Create Project Structure

```bash
dotnet new blazorwasm -n MyAIBlazorWasm
cd MyAIBlazorWasm
dotnet add package Microsoft.Extensions.Http
```

**Note**: FluentAI.NET cannot run directly in Blazor WebAssembly due to CORS restrictions and security limitations. You need a server-side API.

### 2. Create API Client Service

```csharp
using System.Text.Json;

public interface IChatApiClient
{
    Task<ChatApiResponse> SendMessageAsync(string message);
    IAsyncEnumerable<string> StreamMessageAsync(string message);
}

public class ChatApiClient : IChatApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ChatApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<ChatApiResponse> SendMessageAsync(string message)
    {
        var request = new { Message = message };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("api/chat/message", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ChatApiResponse>(responseJson, _jsonOptions)!;
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string message)
    {
        var request = new { Message = message };
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync("api/chat/stream", content, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!string.IsNullOrEmpty(line))
                yield return line;
        }
    }
}

public record ChatApiResponse(string Message, string ModelId, TokenUsageModel TokenUsage);
public record TokenUsageModel(int InputTokens, int OutputTokens, int TotalTokens);
```

### 3. Register Services in Program.cs (Blazor WASM)

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP client for API calls
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri("https://your-api-server.com/") 
});

builder.Services.AddScoped<IChatApiClient, ChatApiClient>();

await builder.Build().RunAsync();
```

### 4. Use the API Client in Blazor Component

```razor
@page "/chat"
@inject IChatApiClient ChatApi
@inject IJSRuntime JSRuntime

<!-- Similar UI as Blazor Server but using ChatApi instead of ChatModel -->

@code {
    private async Task HandleRegularResponse(string message)
    {
        var response = await ChatApi.SendMessageAsync(message);
        
        messages.Add(new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = response.Message,
            Timestamp = DateTime.Now
        });
    }

    private async Task HandleStreamingResponse(string message)
    {
        var aiMessage = new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            Content = "",
            Timestamp = DateTime.Now
        };
        
        messages.Add(aiMessage);
        
        await foreach (var token in ChatApi.StreamMessageAsync(message))
        {
            aiMessage.Content += token;
            StateHasChanged();
            await ScrollToBottom();
            await Task.Delay(10);
        }
    }
}
```

## Advanced Features

### Chat History Service

```csharp
public interface IChatHistoryService
{
    Task SaveConversationAsync(string userId, List<ChatMessageModel> messages);
    Task<List<ChatMessageModel>> LoadConversationAsync(string userId);
    Task<List<ConversationSummary>> GetConversationHistoryAsync(string userId);
}

public class LocalStorageChatHistoryService : IChatHistoryService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageChatHistoryService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SaveConversationAsync(string userId, List<ChatMessageModel> messages)
    {
        var key = $"chat_history_{userId}_{DateTime.Now:yyyy-MM-dd}";
        var json = JsonSerializer.Serialize(messages);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task<List<ChatMessageModel>> LoadConversationAsync(string userId)
    {
        var key = $"chat_history_{userId}_{DateTime.Now:yyyy-MM-dd}";
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
        
        if (string.IsNullOrEmpty(json))
            return new List<ChatMessageModel>();
            
        return JsonSerializer.Deserialize<List<ChatMessageModel>>(json) ?? new List<ChatMessageModel>();
    }

    public async Task<List<ConversationSummary>> GetConversationHistoryAsync(string userId)
    {
        // Implementation to get conversation summaries
        var conversations = new List<ConversationSummary>();
        
        for (int i = 0; i < 7; i++) // Last 7 days
        {
            var date = DateTime.Now.AddDays(-i);
            var key = $"chat_history_{userId}_{date:yyyy-MM-dd}";
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            
            if (!string.IsNullOrEmpty(json))
            {
                var messages = JsonSerializer.Deserialize<List<ChatMessageModel>>(json);
                if (messages?.Any() == true)
                {
                    conversations.Add(new ConversationSummary
                    {
                        Date = date,
                        MessageCount = messages.Count,
                        Preview = messages.FirstOrDefault(m => m.Role == ChatRole.User)?.Content ?? ""
                    });
                }
            }
        }
        
        return conversations;
    }
}

public record ConversationSummary
{
    public DateTime Date { get; init; }
    public int MessageCount { get; init; }
    public string Preview { get; init; } = "";
}
```

### Component State Management

```csharp
public class ChatState
{
    public List<ChatMessageModel> Messages { get; set; } = new();
    public bool IsLoading { get; set; }
    public bool UseStreaming { get; set; }
    public string ErrorMessage { get; set; } = "";

    public event Action? OnStateChanged;

    public void SetLoading(bool loading)
    {
        IsLoading = loading;
        NotifyStateChanged();
    }

    public void SetError(string error)
    {
        ErrorMessage = error;
        NotifyStateChanged();
    }

    public void AddMessage(ChatMessageModel message)
    {
        Messages.Add(message);
        NotifyStateChanged();
    }

    public void ClearMessages()
    {
        Messages.Clear();
        ErrorMessage = "";
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();
}
```

### Custom Chat Input Component

```razor
@* Components/ChatInput.razor *@
<div class="chat-input-container">
    <div class="input-group">
        <textarea @bind="Message" 
                  @onkeydown="HandleKeyDown"
                  @oninput="OnInput"
                  placeholder="Type your message..."
                  disabled="@IsDisabled"
                  rows="@(Message.Contains('\n') ? Math.Min(Message.Split('\n').Length, 5) : 1)"
                  class="form-control auto-resize"></textarea>
        <button @onclick="OnSendClick" 
                disabled="@(IsDisabled || string.IsNullOrWhiteSpace(Message))" 
                class="btn btn-primary">
            @if (IsLoading)
            {
                <span class="spinner-border spinner-border-sm"></span>
            }
            else
            {
                <i class="oi oi-arrow-right"></i>
            }
        </button>
    </div>
</div>

@code {
    [Parameter] public string Message { get; set; } = "";
    [Parameter] public EventCallback<string> MessageChanged { get; set; }
    [Parameter] public EventCallback<string> OnSend { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool IsDisabled { get; set; }

    private async Task OnSendClick()
    {
        if (!string.IsNullOrWhiteSpace(Message) && !IsDisabled)
        {
            await OnSend.InvokeAsync(Message);
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey && !IsDisabled)
        {
            e.PreventDefault();
            await OnSendClick();
        }
    }

    private async Task OnInput(ChangeEventArgs e)
    {
        Message = e.Value?.ToString() ?? "";
        await MessageChanged.InvokeAsync(Message);
    }
}
```

## Performance Optimization

### 1. Virtual Scrolling for Large Chat History

```csharp
// Use Microsoft.AspNetCore.Components.Web.Virtualization
<Virtualize Items="messages" Context="message">
    <div class="message @(message.Role == ChatRole.User ? "user-message" : "ai-message")">
        <!-- Message content -->
    </div>
</Virtualize>
```

### 2. Message Chunking

```csharp
private const int MaxMessagesInMemory = 100;

private void AddMessage(ChatMessageModel message)
{
    messages.Add(message);
    
    // Keep only recent messages in memory
    if (messages.Count > MaxMessagesInMemory)
    {
        var toRemove = messages.Count - MaxMessagesInMemory;
        messages.RemoveRange(0, toRemove);
    }
}
```

### 3. Debounced Input

```csharp
private Timer? _debounceTimer;

private void OnInputChanged(string value)
{
    _debounceTimer?.Dispose();
    _debounceTimer = new Timer(_ =>
    {
        InvokeAsync(() =>
        {
            // Handle input change
            StateHasChanged();
        });
    }, null, 300, Timeout.Infinite);
}
```

## Next Steps

- Explore [WPF/WinUI integration](wpf-winui.md) for desktop applications
- Learn about [performance optimization](common-patterns.md#performance-optimization)
- Check [security considerations](common-patterns.md#security) for web applications
- Review the [troubleshooting guide](troubleshooting.md)