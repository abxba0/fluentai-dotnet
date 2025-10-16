using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using FluentAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FluentAI services
builder.Services.AddAiSdk(builder.Configuration);
builder.Services.AddOpenAiChatModel(builder.Configuration);
builder.Services.AddAnthropicChatModel(builder.Configuration);
builder.Services.AddGoogleGeminiChatModel(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Minimal API endpoints
app.MapPost("/api/chat", async (ChatRequest request, IChatModel chatModel) =>
{
    try
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, request.Message)
        };

        var response = await chatModel.GetResponseAsync(messages);
        
        return Results.Ok(new ChatResponse
        {
            Message = response.Content,
            ModelId = response.ModelId,
            TokensUsed = response.Usage?.TotalTokens ?? 0
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("Chat")
.WithOpenApi();

app.MapPost("/api/chat/stream", async (ChatRequest request, IChatModel chatModel) =>
{
    try
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, request.Message)
        };

        return Results.Stream(async stream =>
        {
            await foreach (var token in chatModel.StreamResponseAsync(messages))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(token);
                await stream.WriteAsync(bytes);
                await stream.FlushAsync();
            }
        }, "text/plain");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
})
.WithName("ChatStream")
.WithOpenApi();

app.Run();

// Request/Response models
public record ChatRequest(string Message);
public record ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public int TokensUsed { get; set; }
}
