namespace FluentAI.Abstractions.Performance
{
    /// <summary>
    /// Represents performance statistics for an operation.
    /// </summary>
    public record OperationStats
    {
        /// <summary>
        /// Gets the name of the operation.
        /// </summary>
        public string OperationName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the total number of times the operation was executed.
        /// </summary>
        public long ExecutionCount { get; init; }

        /// <summary>
        /// Gets the average execution time in milliseconds.
        /// </summary>
        public double AverageExecutionTimeMs { get; init; }

        /// <summary>
        /// Gets the minimum execution time in milliseconds.
        /// </summary>
        public double MinExecutionTimeMs { get; init; }

        /// <summary>
        /// Gets the maximum execution time in milliseconds.
        /// </summary>
        public double MaxExecutionTimeMs { get; init; }

        /// <summary>
        /// Gets the total execution time in milliseconds.
        /// </summary>
        public double TotalExecutionTimeMs { get; init; }

        /// <summary>
        /// Gets the timestamp of the first execution.
        /// </summary>
        public DateTimeOffset FirstExecution { get; init; }

        /// <summary>
        /// Gets the timestamp of the last execution.
        /// </summary>
        public DateTimeOffset LastExecution { get; init; }

        /// <summary>
        /// Gets the number of failed executions.
        /// </summary>
        public long FailedExecutions { get; init; }

        /// <summary>
        /// Gets the success rate as a percentage.
        /// </summary>
        public double SuccessRate => ExecutionCount > 0 ? ((double)(ExecutionCount - FailedExecutions) / ExecutionCount) * 100.0 : 0.0;
    }
}