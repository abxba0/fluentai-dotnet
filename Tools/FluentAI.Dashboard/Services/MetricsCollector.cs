using System.Collections.Concurrent;

namespace FluentAI.Dashboard.Services;

/// <summary>
/// Collects and aggregates metrics for dashboard visualization
/// </summary>
public class MetricsCollector
{
    private readonly ConcurrentQueue<MetricDataPoint> _tokenUsageHistory = new();
    private readonly ConcurrentQueue<MetricDataPoint> _responseTimeHistory = new();
    private readonly ConcurrentDictionary<string, int> _cacheStats = new();
    private readonly object _lock = new();
    
    private int _totalRequests;
    private int _successfulRequests;
    private int _failedRequests;
    private double _totalTokensUsed;
    private double _estimatedCost;

    public event EventHandler? MetricsUpdated;

    public void RecordRequest(string modelId, int tokens, double responseTimeMs, bool success, double cost = 0)
    {
        lock (_lock)
        {
            _totalRequests++;
            if (success)
            {
                _successfulRequests++;
                _totalTokensUsed += tokens;
                _estimatedCost += cost;
                
                _tokenUsageHistory.Enqueue(new MetricDataPoint
                {
                    Timestamp = DateTime.UtcNow,
                    Value = tokens,
                    Label = modelId
                });

                _responseTimeHistory.Enqueue(new MetricDataPoint
                {
                    Timestamp = DateTime.UtcNow,
                    Value = responseTimeMs,
                    Label = modelId
                });

                // Keep only last 100 data points
                while (_tokenUsageHistory.Count > 100)
                    _tokenUsageHistory.TryDequeue(out _);
                while (_responseTimeHistory.Count > 100)
                    _responseTimeHistory.TryDequeue(out _);
            }
            else
            {
                _failedRequests++;
            }
        }

        MetricsUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void RecordCacheHit(bool hit)
    {
        var key = hit ? "hits" : "misses";
        _cacheStats.AddOrUpdate(key, 1, (_, count) => count + 1);
        MetricsUpdated?.Invoke(this, EventArgs.Empty);
    }

    public DashboardMetrics GetCurrentMetrics()
    {
        lock (_lock)
        {
            return new DashboardMetrics
            {
                TotalRequests = _totalRequests,
                SuccessfulRequests = _successfulRequests,
                FailedRequests = _failedRequests,
                SuccessRate = _totalRequests > 0 ? (_successfulRequests / (double)_totalRequests) * 100 : 0,
                TotalTokensUsed = (long)_totalTokensUsed,
                EstimatedCost = _estimatedCost,
                CacheHits = _cacheStats.GetValueOrDefault("hits", 0),
                CacheMisses = _cacheStats.GetValueOrDefault("misses", 0),
                TokenUsageHistory = _tokenUsageHistory.ToList(),
                ResponseTimeHistory = _responseTimeHistory.ToList(),
                MemoryUsedMB = GC.GetTotalMemory(false) / 1024.0 / 1024.0
            };
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _totalRequests = 0;
            _successfulRequests = 0;
            _failedRequests = 0;
            _totalTokensUsed = 0;
            _estimatedCost = 0;
            _tokenUsageHistory.Clear();
            _responseTimeHistory.Clear();
            _cacheStats.Clear();
        }
        MetricsUpdated?.Invoke(this, EventArgs.Empty);
    }
}

public class DashboardMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double SuccessRate { get; set; }
    public long TotalTokensUsed { get; set; }
    public double EstimatedCost { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double CacheHitRate => (CacheHits + CacheMisses) > 0 
        ? (CacheHits / (double)(CacheHits + CacheMisses)) * 100 
        : 0;
    public List<MetricDataPoint> TokenUsageHistory { get; set; } = new();
    public List<MetricDataPoint> ResponseTimeHistory { get; set; } = new();
    public double MemoryUsedMB { get; set; }
}

public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public string Label { get; set; } = string.Empty;
}
