# FluentAI.NET Visual Debugging Dashboard

Real-time monitoring and visualization dashboard for FluentAI.NET AI operations.

## Features

### ✅ Implemented
- **Real-time Token Usage Graphs** - Track token consumption across requests
- **Performance Metrics** - Monitor response times and request throughput
- **Cache Statistics** - Visualize cache hit/miss rates with interactive charts
- **Cost Tracking** - Estimate and monitor API costs in real-time
- **Memory Monitoring** - Track application memory usage
- **Success/Failure Rates** - Monitor request success rates and error tracking
- **Auto-refresh** - Dashboard updates automatically every 2 seconds
- **Test Interface** - Send test requests to populate metrics

## Getting Started

### Prerequisites
- .NET 8.0 or later
- (Optional) Valid API keys for AI providers (works with simulated data without keys)

### Running the Dashboard

#### Option 1: Run from Source

```bash
cd Tools/FluentAI.Dashboard
dotnet run
```

Then navigate to `https://localhost:5001` or `http://localhost:5000`

#### Option 2: Build and Run

```bash
cd Tools/FluentAI.Dashboard
dotnet build
dotnet run --no-build
```

### Configuration

Configure AI providers in `appsettings.json`:

```json
{
  "AiSdk": {
    "DefaultProvider": "OpenAI"
  },
  "OpenAI": {
    "Model": "gpt-4",
    "MaxTokens": 2000
  }
}
```

Set API keys via environment variables:
```bash
export OPENAI_API_KEY="your-key"
export ANTHROPIC_API_KEY="your-key"
export GOOGLE_API_KEY="your-key"
```

**Note:** The dashboard works with simulated data if API keys are not configured.

## Usage

### Dashboard Page

Navigate to `/dashboard` to view:
- Total requests and success rate
- Token usage over time
- Response time metrics
- Cache hit/miss statistics
- Memory usage
- Cost estimates

### Test Requests Page

Navigate to `/test` to:
- Send individual test requests
- Send batch requests (10 at a time)
- Simulate cache hits/misses
- Choose different models
- View request responses

## Metrics Collected

| Metric | Description |
|--------|-------------|
| Total Requests | Number of AI requests made |
| Success Rate | Percentage of successful requests |
| Total Tokens | Total tokens consumed across all requests |
| Estimated Cost | Approximate API costs based on token usage |
| Cache Hit Rate | Percentage of requests served from cache |
| Response Time | Time taken to receive responses (ms) |
| Memory Usage | Application memory consumption (MB) |

## Development

### Project Structure

```
FluentAI.Dashboard/
├── Components/
│   ├── Pages/
│   │   ├── Dashboard.razor      # Main metrics dashboard
│   │   ├── TestRequests.razor   # Test interface
│   │   └── Home.razor            # Landing page
│   └── Layout/
│       ├── MainLayout.razor      # Main layout
│       └── NavMenu.razor         # Navigation menu
├── Services/
│   └── MetricsCollector.cs       # Metrics collection service
├── Program.cs                    # Application startup
└── appsettings.json              # Configuration
```

### Adding Custom Metrics

Extend the `MetricsCollector` class to track additional metrics:

```csharp
public void RecordCustomMetric(string name, double value)
{
    // Your implementation
}
```

## Troubleshooting

### "FluentAI services not configured"
- This is normal if API keys are not set
- Dashboard will use simulated data
- To use real data, configure API keys in environment variables

### Dashboard not updating
- Check that MetricsCollector is registered as a singleton
- Verify auto-refresh timer is running
- Check browser console for errors

### No metrics showing
- Send test requests from the `/test` page
- Click "Send 10 Requests" to populate initial data
- Use "Refresh" button to manually update

## Technologies Used

- **Blazor Server** - Interactive web UI with real-time updates
- **Bootstrap 5** - Responsive UI components
- **Chart.js** - Future charting library (prepared)
- **FluentAI.NET** - AI model integration

## Future Enhancements

- [ ] Interactive charts with Chart.js/Blazorise.Charts
- [ ] Export metrics to CSV/JSON
- [ ] Historical data storage
- [ ] Alert thresholds and notifications
- [ ] Multi-provider comparison charts
- [ ] Custom dashboard layouts
- [ ] WebSocket real-time updates

## Support

- [FluentAI.NET Documentation](../../docs/)
- [GitHub Issues](https://github.com/abxba0/fluentai-dotnet/issues)
- [Examples](../../Examples/)

## License

Same as FluentAI.NET - MIT License
