# FluentAI.NET Web API Template

A ready-to-use ASP.NET Core Web API template with FluentAI.NET integrated.

## Features

- ✅ RESTful API endpoints
- ✅ Swagger/OpenAPI documentation
- ✅ Streaming support
- ✅ Multiple AI providers
- ✅ Rate limiting
- ✅ Error handling

## Endpoints

### POST /api/chat
Send a chat message and receive a response.

**Request:**
```json
{
  "message": "What is machine learning?"
}
```

**Response:**
```json
{
  "message": "Machine learning is...",
  "modelId": "gpt-4",
  "tokensUsed": 150
}
```

### POST /api/chat/stream
Send a chat message and receive a streaming response.

**Request:**
```json
{
  "message": "Write a poem about AI"
}
```

**Response:** Text stream

## Getting Started

### 1. Configure API Keys

```bash
export OPENAI_API_KEY="your-key"
export ANTHROPIC_API_KEY="your-key"
export GOOGLE_API_KEY="your-key"
```

### 2. Run the API

```bash
dotnet run
```

Navigate to `https://localhost:5001/swagger` to view the API documentation.

### 3. Test Endpoints

#### Using curl

```bash
# Chat endpoint
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, AI!"}'

# Streaming endpoint
curl -X POST https://localhost:5001/api/chat/stream \
  -H "Content-Type: application/json" \
  -d '{"message": "Tell me a story"}' \
  --no-buffer
```

#### Using PowerShell

```powershell
# Chat endpoint
Invoke-RestMethod -Uri https://localhost:5001/api/chat `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"message": "Hello, AI!"}'
```

## Customization

### Add Authentication

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* configure */ });

app.UseAuthentication();
```

### Add CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

### Add Controllers

Create `Controllers/ChatController.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatModel _chatModel;

    public ChatController(IChatModel chatModel)
    {
        _chatModel = chatModel;
    }

    [HttpPost]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var messages = new[]
        {
            new ChatMessage(ChatRole.User, request.Message)
        };

        var response = await _chatModel.GetResponseAsync(messages);
        return Ok(new ChatResponse
        {
            Message = response.Content,
            ModelId = response.ModelId
        });
    }
}
```

## Production Deployment

### Docker

Create `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["*.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluentAI.Templates.WebApi.dll"]
```

### Azure App Service

```bash
az webapp up --name my-fluentai-api --resource-group my-rg
```

### Configuration

Set environment variables in your hosting environment:

```
OPENAI_API_KEY=your-key
ANTHROPIC_API_KEY=your-key
GOOGLE_API_KEY=your-key
```

## Security Best Practices

1. **Never commit API keys** - Use environment variables or Azure Key Vault
2. **Enable HTTPS** - Always use HTTPS in production
3. **Add authentication** - Protect your endpoints
4. **Rate limiting** - Prevent abuse
5. **Input validation** - Validate all user inputs
6. **Error handling** - Don't expose sensitive information in errors

## Performance Tips

1. **Use caching** - Cache frequent responses
2. **Enable compression** - Reduce response sizes
3. **Use async/await** - Improve throughput
4. **Connection pooling** - Reuse HTTP connections
5. **Monitor metrics** - Track performance and usage

## Documentation

- [FluentAI.NET Documentation](https://github.com/abxba0/fluentai-dotnet/tree/main/docs)
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core)
- [OpenAPI Specification](https://swagger.io/specification/)

## Support

- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Contributing Guide](https://github.com/abxba0/fluentai-dotnet/blob/main/CONTRIBUTING.md)
