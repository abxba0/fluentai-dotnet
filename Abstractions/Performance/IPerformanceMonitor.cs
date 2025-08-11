using System.Diagnostics;

namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// Provides performance monitoring and metrics collection for AI operations.
    /// </summary>
    public interface IPerformanceMonitor
    {
        /// <summary>
        /// Starts timing an operation.
        /// </summary>
        /// <param name="operationName">Name of the operation being timed.</param>
        /// <returns>A disposable timer that automatically records the duration when disposed.</returns>
        IDisposable StartOperation(string operationName);

        /// <summary>
        /// Records a custom metric value.
        /// </summary>
        /// <param name="metricName">Name of the metric.</param>
        /// <param name="value">Value to record.</param>
        /// <param name="tags">Optional tags for categorization.</param>
        void RecordMetric(string metricName, double value, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Increments a counter metric.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        /// <param name="increment">Amount to increment by (default: 1).</param>
        /// <param name="tags">Optional tags for categorization.</param>
        void IncrementCounter(string counterName, int increment = 1, Dictionary<string, string>? tags = null);

        /// <summary>
        /// Gets performance statistics for a specific operation.
        /// </summary>
        /// <param name="operationName">Name of the operation.</param>
        /// <returns>Performance statistics or null if no data available.</returns>
        OperationStats? GetOperationStats(string operationName);
    }
}