using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// Default implementation of performance monitor for AI operations.
    /// </summary>
    public class DefaultPerformanceMonitor : IPerformanceMonitor
    {
        private readonly ILogger<DefaultPerformanceMonitor> _logger;
        private readonly ConcurrentDictionary<string, OperationStatsBuilder> _operationStats = new();
        private readonly ConcurrentDictionary<string, long> _counters = new();

        public DefaultPerformanceMonitor(ILogger<DefaultPerformanceMonitor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable StartOperation(string operationName)
        {
            return new OperationTimer(operationName, this, _logger);
        }

        public void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null)
        {
            var tagsStr = tags?.Any() == true 
                ? string.Join(",", tags.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                : string.Empty;

            _logger.LogDebug("Performance metric recorded: {MetricName}={Value} {Tags}", 
                metricName, value, tagsStr);
        }

        public void IncrementCounter(string counterName, int increment = 1, Dictionary<string, string>? tags = null)
        {
            _counters.AddOrUpdate(counterName, increment, (key, existingValue) => existingValue + increment);

            var tagsStr = tags?.Any() == true 
                ? string.Join(",", tags.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                : string.Empty;

            _logger.LogDebug("Performance counter incremented: {CounterName}+{Increment} (Total: {Total}) {Tags}", 
                counterName, increment, _counters[counterName], tagsStr);
        }

        public OperationStats? GetOperationStats(string operationName)
        {
            return _operationStats.TryGetValue(operationName, out var statsBuilder) 
                ? statsBuilder.Build() 
                : null;
        }

        internal void RecordOperationExecution(string operationName, double durationMs, bool success)
        {
            var statsBuilder = _operationStats.GetOrAdd(operationName, _ => new OperationStatsBuilder(operationName));
            statsBuilder.RecordExecution(durationMs, success);
        }

        private class OperationTimer : IDisposable
        {
            private readonly string _operationName;
            private readonly DefaultPerformanceMonitor _monitor;
            private readonly ILogger _logger;
            private readonly Stopwatch _stopwatch;
            private bool _disposed;

            public OperationTimer(string operationName, DefaultPerformanceMonitor monitor, ILogger logger)
            {
                _operationName = operationName;
                _monitor = monitor;
                _logger = logger;
                _stopwatch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _stopwatch.Stop();
                    var durationMs = _stopwatch.Elapsed.TotalMilliseconds;
                    
                    // Assume success unless explicitly marked as failure
                    _monitor.RecordOperationExecution(_operationName, durationMs, true);
                    
                    _logger.LogDebug("Operation {OperationName} completed in {DurationMs}ms", 
                        _operationName, durationMs);
                    
                    _disposed = true;
                }
            }
        }

        private class OperationStatsBuilder
        {
            private readonly string _operationName;
            private readonly object _lock = new();
            private long _executionCount;
            private double _totalExecutionTimeMs;
            private double _minExecutionTimeMs = double.MaxValue;
            private double _maxExecutionTimeMs;
            private DateTimeOffset _firstExecution;
            private DateTimeOffset _lastExecution;
            private long _failedExecutions;

            public OperationStatsBuilder(string operationName)
            {
                _operationName = operationName;
                _firstExecution = DateTimeOffset.UtcNow;
            }

            public void RecordExecution(double durationMs, bool success)
            {
                lock (_lock)
                {
                    _executionCount++;
                    _totalExecutionTimeMs += durationMs;
                    _minExecutionTimeMs = Math.Min(_minExecutionTimeMs, durationMs);
                    _maxExecutionTimeMs = Math.Max(_maxExecutionTimeMs, durationMs);
                    _lastExecution = DateTimeOffset.UtcNow;

                    if (!success)
                    {
                        _failedExecutions++;
                    }
                }
            }

            public OperationStats Build()
            {
                lock (_lock)
                {
                    return new OperationStats
                    {
                        OperationName = _operationName,
                        ExecutionCount = _executionCount,
                        AverageExecutionTimeMs = _executionCount > 0 ? _totalExecutionTimeMs / _executionCount : 0,
                        MinExecutionTimeMs = _minExecutionTimeMs == double.MaxValue ? 0 : _minExecutionTimeMs,
                        MaxExecutionTimeMs = _maxExecutionTimeMs,
                        TotalExecutionTimeMs = _totalExecutionTimeMs,
                        FirstExecution = _firstExecution,
                        LastExecution = _lastExecution,
                        FailedExecutions = _failedExecutions
                    };
                }
            }
        }
    }
}