using FluentAI.Dashboard.Components;
using FluentAI.Dashboard.Services;
using FluentAI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add FluentAI services (optional - dashboard works with simulated data if not configured)
try
{
    builder.Services.AddAiSdk(builder.Configuration);
    builder.Services.AddOpenAiChatModel(builder.Configuration);
    builder.Services.AddAnthropicChatModel(builder.Configuration);
    builder.Services.AddGoogleGeminiChatModel(builder.Configuration);
}
catch
{
    // If FluentAI configuration fails, continue with simulated data
    Console.WriteLine("FluentAI services not configured. Dashboard will use simulated data.");
}

// Add MetricsCollector as singleton
builder.Services.AddSingleton<MetricsCollector>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
