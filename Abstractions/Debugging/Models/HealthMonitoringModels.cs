namespace FluentAI.Abstractions.Debugging.Models
{
    /// <summary>
    /// Context for system health monitoring.
    /// </summary>
    public record HealthMonitoringContext
    {
        /// <summary>
        /// Gets or sets the monitoring configuration.
        /// </summary>
        public HealthMonitoringConfiguration Configuration { get; init; } = new();

        /// <summary>
        /// Gets or sets the components to monitor.
        /// </summary>
        public IReadOnlyList<string> ComponentsToMonitor { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the monitoring duration.
        /// </summary>
        public TimeSpan MonitoringDuration { get; init; } = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Gets or sets the baseline metrics for comparison.
        /// </summary>
        public BaselineMetrics BaselineMetrics { get; init; } = new();

        /// <summary>
        /// Gets or sets the alerting thresholds.
        /// </summary>
        public AlertingThresholds AlertingThresholds { get; init; } = new();
    }

    /// <summary>
    /// Configuration for health monitoring.
    /// </summary>
    public record HealthMonitoringConfiguration
    {
        /// <summary>
        /// Gets or sets the monitoring interval.
        /// </summary>
        public TimeSpan MonitoringInterval { get; init; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the metrics to collect.
        /// </summary>
        public IReadOnlyList<HealthMetricType> MetricsToCollect { get; init; } = Array.Empty<HealthMetricType>();

        /// <summary>
        /// Gets or sets whether to perform active health checks.
        /// </summary>
        public bool PerformActiveHealthChecks { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to monitor for new issues.
        /// </summary>
        public bool MonitorForNewIssues { get; init; } = true;

        /// <summary>
        /// Gets or sets the log analysis configuration.
        /// </summary>
        public LogAnalysisConfiguration LogAnalysis { get; init; } = new();
    }

    /// <summary>
    /// Configuration for log analysis during health monitoring.
    /// </summary>
    public record LogAnalysisConfiguration
    {
        /// <summary>
        /// Gets or sets whether to analyze error logs.
        /// </summary>
        public bool AnalyzeErrorLogs { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to analyze warning logs.
        /// </summary>
        public bool AnalyzeWarningLogs { get; init; } = true;

        /// <summary>
        /// Gets or sets the log patterns to watch for.
        /// </summary>
        public IReadOnlyList<string> WatchPatterns { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the log sources to monitor.
        /// </summary>
        public IReadOnlyList<string> LogSources { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Result of system health monitoring.
    /// </summary>
    public record SystemHealthResult
    {
        /// <summary>
        /// Gets or sets the overall health status.
        /// </summary>
        public HealthStatus OverallHealth { get; init; }

        /// <summary>
        /// Gets or sets the health score (0-100).
        /// </summary>
        public int HealthScore { get; init; }

        /// <summary>
        /// Gets or sets the component health results.
        /// </summary>
        public IReadOnlyList<ComponentHealthResult> ComponentHealth { get; init; } = Array.Empty<ComponentHealthResult>();

        /// <summary>
        /// Gets or sets the collected metrics.
        /// </summary>
        public HealthMetricsCollection CollectedMetrics { get; init; } = new();

        /// <summary>
        /// Gets or sets the detected anomalies.
        /// </summary>
        public IReadOnlyList<HealthAnomaly> DetectedAnomalies { get; init; } = Array.Empty<HealthAnomaly>();

        /// <summary>
        /// Gets or sets the triggered alerts.
        /// </summary>
        public IReadOnlyList<HealthAlert> TriggeredAlerts { get; init; } = Array.Empty<HealthAlert>();

        /// <summary>
        /// Gets or sets newly discovered issues.
        /// </summary>
        public IReadOnlyList<string> NewIssuesDiscovered { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the monitoring summary.
        /// </summary>
        public HealthMonitoringSummary MonitoringSummary { get; init; } = new();
    }

    /// <summary>
    /// Health result for a specific component.
    /// </summary>
    public record ComponentHealthResult
    {
        /// <summary>
        /// Gets or sets the component name.
        /// </summary>
        public string ComponentName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the health status of the component.
        /// </summary>
        public HealthStatus HealthStatus { get; init; }

        /// <summary>
        /// Gets or sets the component health score (0-100).
        /// </summary>
        public int HealthScore { get; init; }

        /// <summary>
        /// Gets or sets the component-specific metrics.
        /// </summary>
        public Dictionary<string, double> Metrics { get; init; } = new();

        /// <summary>
        /// Gets or sets the issues detected for this component.
        /// </summary>
        public IReadOnlyList<string> DetectedIssues { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the last successful health check timestamp.
        /// </summary>
        public DateTimeOffset LastSuccessfulCheck { get; init; }

        /// <summary>
        /// Gets or sets recommendations for this component.
        /// </summary>
        public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Collection of health metrics.
    /// </summary>
    public record HealthMetricsCollection
    {
        /// <summary>
        /// Gets or sets the performance metrics.
        /// </summary>
        public PerformanceHealthMetrics Performance { get; init; } = new();

        /// <summary>
        /// Gets or sets the resource usage metrics.
        /// </summary>
        public ResourceUsageMetrics ResourceUsage { get; init; } = new();

        /// <summary>
        /// Gets or sets the error rate metrics.
        /// </summary>
        public ErrorRateMetrics ErrorRates { get; init; } = new();

        /// <summary>
        /// Gets or sets the availability metrics.
        /// </summary>
        public AvailabilityMetrics Availability { get; init; } = new();

        /// <summary>
        /// Gets or sets custom metrics.
        /// </summary>
        public Dictionary<string, double> CustomMetrics { get; init; } = new();
    }

    /// <summary>
    /// Performance health metrics.
    /// </summary>
    public record PerformanceHealthMetrics
    {
        /// <summary>
        /// Gets or sets the average response time in milliseconds.
        /// </summary>
        public double AverageResponseTimeMs { get; init; }

        /// <summary>
        /// Gets or sets the 95th percentile response time in milliseconds.
        /// </summary>
        public double P95ResponseTimeMs { get; init; }

        /// <summary>
        /// Gets or sets the throughput (requests per second).
        /// </summary>
        public double ThroughputRps { get; init; }

        /// <summary>
        /// Gets or sets the transaction completion rate.
        /// </summary>
        public double TransactionCompletionRate { get; init; }
    }

    /// <summary>
    /// Resource usage metrics.
    /// </summary>
    public record ResourceUsageMetrics
    {
        /// <summary>
        /// Gets or sets the CPU usage percentage.
        /// </summary>
        public double CpuUsagePercent { get; init; }

        /// <summary>
        /// Gets or sets the memory usage percentage.
        /// </summary>
        public double MemoryUsagePercent { get; init; }

        /// <summary>
        /// Gets or sets the disk usage percentage.
        /// </summary>
        public double DiskUsagePercent { get; init; }

        /// <summary>
        /// Gets or sets the network I/O utilization.
        /// </summary>
        public double NetworkIoUtilization { get; init; }

        /// <summary>
        /// Gets or sets the thread pool utilization.
        /// </summary>
        public double ThreadPoolUtilization { get; init; }
    }

    /// <summary>
    /// Error rate metrics.
    /// </summary>
    public record ErrorRateMetrics
    {
        /// <summary>
        /// Gets or sets the error rate percentage.
        /// </summary>
        public double ErrorRatePercent { get; init; }

        /// <summary>
        /// Gets or sets the exception rate per minute.
        /// </summary>
        public double ExceptionRatePerMinute { get; init; }

        /// <summary>
        /// Gets or sets the timeout rate percentage.
        /// </summary>
        public double TimeoutRatePercent { get; init; }

        /// <summary>
        /// Gets or sets the failure rate for critical operations.
        /// </summary>
        public double CriticalOperationFailureRate { get; init; }
    }

    /// <summary>
    /// Availability metrics.
    /// </summary>
    public record AvailabilityMetrics
    {
        /// <summary>
        /// Gets or sets the uptime percentage.
        /// </summary>
        public double UptimePercent { get; init; }

        /// <summary>
        /// Gets or sets the service availability percentage.
        /// </summary>
        public double ServiceAvailabilityPercent { get; init; }

        /// <summary>
        /// Gets or sets the mean time to recovery (MTTR) in minutes.
        /// </summary>
        public double MeanTimeToRecoveryMinutes { get; init; }

        /// <summary>
        /// Gets or sets the mean time between failures (MTBF) in hours.
        /// </summary>
        public double MeanTimeBetweenFailuresHours { get; init; }
    }

    /// <summary>
    /// Represents a health anomaly detected during monitoring.
    /// </summary>
    public record HealthAnomaly
    {
        /// <summary>
        /// Gets or sets the anomaly type.
        /// </summary>
        public AnomalyType AnomalyType { get; init; }

        /// <summary>
        /// Gets or sets the component where the anomaly was detected.
        /// </summary>
        public string Component { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the anomaly.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the severity of the anomaly.
        /// </summary>
        public IssueSeverity Severity { get; init; }

        /// <summary>
        /// Gets or sets when the anomaly was first detected.
        /// </summary>
        public DateTimeOffset FirstDetected { get; init; }

        /// <summary>
        /// Gets or sets the baseline value for comparison.
        /// </summary>
        public double BaselineValue { get; init; }

        /// <summary>
        /// Gets or sets the current value that triggered the anomaly.
        /// </summary>
        public double CurrentValue { get; init; }

        /// <summary>
        /// Gets or sets the deviation percentage from baseline.
        /// </summary>
        public double DeviationPercent { get; init; }
    }

    /// <summary>
    /// Represents a health alert triggered during monitoring.
    /// </summary>
    public record HealthAlert
    {
        /// <summary>
        /// Gets or sets the alert level.
        /// </summary>
        public AlertLevel AlertLevel { get; init; }

        /// <summary>
        /// Gets or sets the alert message.
        /// </summary>
        public string AlertMessage { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the component that triggered the alert.
        /// </summary>
        public string Component { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the metric that triggered the alert.
        /// </summary>
        public string TriggeringMetric { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the threshold that was breached.
        /// </summary>
        public double BreachedThreshold { get; init; }

        /// <summary>
        /// Gets or sets the actual value that breached the threshold.
        /// </summary>
        public double ActualValue { get; init; }

        /// <summary>
        /// Gets or sets when the alert was triggered.
        /// </summary>
        public DateTimeOffset TriggeredAt { get; init; }

        /// <summary>
        /// Gets or sets recommended actions for this alert.
        /// </summary>
        public IReadOnlyList<string> RecommendedActions { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Summary of health monitoring session.
    /// </summary>
    public record HealthMonitoringSummary
    {
        /// <summary>
        /// Gets or sets the monitoring start time.
        /// </summary>
        public DateTimeOffset MonitoringStartTime { get; init; }

        /// <summary>
        /// Gets or sets the monitoring end time.
        /// </summary>
        public DateTimeOffset MonitoringEndTime { get; init; }

        /// <summary>
        /// Gets or sets the total monitoring duration.
        /// </summary>
        public TimeSpan TotalMonitoringDuration { get; init; }

        /// <summary>
        /// Gets or sets the number of health checks performed.
        /// </summary>
        public int HealthChecksPerformed { get; init; }

        /// <summary>
        /// Gets or sets the number of metrics collected.
        /// </summary>
        public int MetricsCollected { get; init; }

        /// <summary>
        /// Gets or sets the number of anomalies detected.
        /// </summary>
        public int AnomaliesDetected { get; init; }

        /// <summary>
        /// Gets or sets the number of alerts triggered.
        /// </summary>
        public int AlertsTriggered { get; init; }

        /// <summary>
        /// Gets or sets the overall health trend.
        /// </summary>
        public HealthTrend OverallHealthTrend { get; init; }
    }

    /// <summary>
    /// Baseline metrics for comparison during monitoring.
    /// </summary>
    public record BaselineMetrics
    {
        /// <summary>
        /// Gets or sets the baseline performance metrics.
        /// </summary>
        public PerformanceHealthMetrics BaselinePerformance { get; init; } = new();

        /// <summary>
        /// Gets or sets the baseline resource usage.
        /// </summary>
        public ResourceUsageMetrics BaselineResourceUsage { get; init; } = new();

        /// <summary>
        /// Gets or sets the baseline error rates.
        /// </summary>
        public ErrorRateMetrics BaselineErrorRates { get; init; } = new();
    }

    /// <summary>
    /// Alerting thresholds for health monitoring.
    /// </summary>
    public record AlertingThresholds
    {
        /// <summary>
        /// Gets or sets the CPU usage threshold for alerts.
        /// </summary>
        public double CpuUsageThreshold { get; init; } = 80.0;

        /// <summary>
        /// Gets or sets the memory usage threshold for alerts.
        /// </summary>
        public double MemoryUsageThreshold { get; init; } = 85.0;

        /// <summary>
        /// Gets or sets the error rate threshold for alerts.
        /// </summary>
        public double ErrorRateThreshold { get; init; } = 5.0;

        /// <summary>
        /// Gets or sets the response time threshold for alerts.
        /// </summary>
        public double ResponseTimeThresholdMs { get; init; } = 5000.0;

        /// <summary>
        /// Gets or sets custom thresholds.
        /// </summary>
        public Dictionary<string, double> CustomThresholds { get; init; } = new();
    }

    /// <summary>
    /// Workflow execution summary models.
    /// </summary>
    public record WorkflowExecutionSummary
    {
        /// <summary>
        /// Gets or sets the total execution time.
        /// </summary>
        public TimeSpan TotalExecutionTime { get; init; }

        /// <summary>
        /// Gets or sets the number of phases completed.
        /// </summary>
        public int PhasesCompleted { get; init; }

        /// <summary>
        /// Gets or sets the execution status.
        /// </summary>
        public WorkflowExecutionStatus Status { get; init; }

        /// <summary>
        /// Gets or sets any errors encountered during execution.
        /// </summary>
        public IReadOnlyList<string> ExecutionErrors { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Parallel execution settings.
    /// </summary>
    public record ParallelExecutionSettings
    {
        /// <summary>
        /// Gets or sets whether to enable parallel execution.
        /// </summary>
        public bool EnableParallelExecution { get; init; } = true;

        /// <summary>
        /// Gets or sets the maximum degree of parallelism.
        /// </summary>
        public int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;
    }

    /// <summary>
    /// Workflow quality gates.
    /// </summary>
    public record WorkflowQualityGates
    {
        /// <summary>
        /// Gets or sets the minimum health score required.
        /// </summary>
        public int MinimumHealthScore { get; init; } = 70;

        /// <summary>
        /// Gets or sets the maximum critical issues allowed.
        /// </summary>
        public int MaximumCriticalIssues { get; init; } = 0;

        /// <summary>
        /// Gets or sets whether to fail on security issues.
        /// </summary>
        public bool FailOnSecurityIssues { get; init; } = true;
    }

    /// <summary>
    /// Reporting preferences for the workflow.
    /// </summary>
    public record ReportingPreferences
    {
        /// <summary>
        /// Gets or sets the report verbosity level.
        /// </summary>
        public AnalysisVerbosity ReportVerbosity { get; init; } = AnalysisVerbosity.Normal;

        /// <summary>
        /// Gets or sets whether to include detailed analysis.
        /// </summary>
        public bool IncludeDetailedAnalysis { get; init; } = true;

        /// <summary>
        /// Gets or sets whether to include code samples in reports.
        /// </summary>
        public bool IncludeCodeSamples { get; init; } = false;
    }

    /// <summary>
    /// Quality gate results.
    /// </summary>
    public record QualityGateResults
    {
        /// <summary>
        /// Gets or sets whether all quality gates passed.
        /// </summary>
        public bool AllGatesPassed { get; init; }

        /// <summary>
        /// Gets or sets the individual gate results.
        /// </summary>
        public Dictionary<string, bool> GateResults { get; init; } = new();

        /// <summary>
        /// Gets or sets failure reasons for failed gates.
        /// </summary>
        public IReadOnlyList<string> FailureReasons { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Change impact assessment.
    /// </summary>
    public record ChangeImpactAssessment
    {
        /// <summary>
        /// Gets or sets the overall impact level.
        /// </summary>
        public IssueImpact OverallImpact { get; init; }

        /// <summary>
        /// Gets or sets the affected areas.
        /// </summary>
        public IReadOnlyList<string> AffectedAreas { get; init; } = Array.Empty<string>();

        /// <summary>
        /// Gets or sets the risk level of the change.
        /// </summary>
        public IssueSeverity RiskLevel { get; init; }
    }

    /// <summary>
    /// Health status enumeration.
    /// </summary>
    public enum HealthStatus
    {
        /// <summary>
        /// System is healthy.
        /// </summary>
        Healthy,

        /// <summary>
        /// System has minor issues but is functional.
        /// </summary>
        Warning,

        /// <summary>
        /// System has significant issues.
        /// </summary>
        Unhealthy,

        /// <summary>
        /// System is critically impaired.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Health metric types.
    /// </summary>
    public enum HealthMetricType
    {
        /// <summary>
        /// Performance metrics.
        /// </summary>
        Performance,

        /// <summary>
        /// Resource usage metrics.
        /// </summary>
        ResourceUsage,

        /// <summary>
        /// Error rate metrics.
        /// </summary>
        ErrorRates,

        /// <summary>
        /// Availability metrics.
        /// </summary>
        Availability,

        /// <summary>
        /// Custom application metrics.
        /// </summary>
        Custom
    }

    /// <summary>
    /// Anomaly types.
    /// </summary>
    public enum AnomalyType
    {
        /// <summary>
        /// Performance degradation.
        /// </summary>
        PerformanceDegradation,

        /// <summary>
        /// Resource spike.
        /// </summary>
        ResourceSpike,

        /// <summary>
        /// Error rate increase.
        /// </summary>
        ErrorRateIncrease,

        /// <summary>
        /// Availability drop.
        /// </summary>
        AvailabilityDrop,

        /// <summary>
        /// Unusual pattern detected.
        /// </summary>
        UnusualPattern
    }

    /// <summary>
    /// Alert levels.
    /// </summary>
    public enum AlertLevel
    {
        /// <summary>
        /// Information alert.
        /// </summary>
        Info,

        /// <summary>
        /// Warning alert.
        /// </summary>
        Warning,

        /// <summary>
        /// Error alert.
        /// </summary>
        Error,

        /// <summary>
        /// Critical alert.
        /// </summary>
        Critical
    }

    /// <summary>
    /// Health trends.
    /// </summary>
    public enum HealthTrend
    {
        /// <summary>
        /// Health is improving.
        /// </summary>
        Improving,

        /// <summary>
        /// Health is stable.
        /// </summary>
        Stable,

        /// <summary>
        /// Health is degrading.
        /// </summary>
        Degrading,

        /// <summary>
        /// Health trend is unknown.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Workflow execution status.
    /// </summary>
    public enum WorkflowExecutionStatus
    {
        /// <summary>
        /// Workflow completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// Workflow completed with warnings.
        /// </summary>
        SuccessWithWarnings,

        /// <summary>
        /// Workflow failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Workflow was cancelled.
        /// </summary>
        Cancelled,

        /// <summary>
        /// Workflow timed out.
        /// </summary>
        TimedOut
    }
}