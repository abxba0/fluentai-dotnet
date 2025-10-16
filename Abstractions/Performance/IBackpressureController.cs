using System.Threading.Channels;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Provides backpressure control for streaming operations to prevent overwhelming consumers.
/// </summary>
public interface IBackpressureController
{
    /// <summary>
    /// Gets the current buffer utilization as a percentage (0-100).
    /// </summary>
    double BufferUtilization { get; }

    /// <summary>
    /// Gets whether backpressure is currently active.
    /// </summary>
    bool IsBackpressureActive { get; }

    /// <summary>
    /// Checks if the system can accept more data without overwhelming the consumer.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if more data can be accepted.</returns>
    ValueTask<bool> CanAcceptDataAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Waits until backpressure is relieved and the system can accept more data.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    ValueTask WaitForAvailabilityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Signals that data has been consumed and buffer space is available.
    /// </summary>
    void SignalDataConsumed();

    /// <summary>
    /// Gets configuration for bounded channels with backpressure support.
    /// </summary>
    /// <returns>Bounded channel options configured for backpressure.</returns>
    BoundedChannelOptions GetChannelOptions();
}

/// <summary>
/// Configuration options for backpressure control.
/// </summary>
public class BackpressureOptions
{
    /// <summary>
    /// Maximum buffer size before backpressure is applied. Default is 1000.
    /// </summary>
    public int BufferCapacity { get; set; } = 1000;

    /// <summary>
    /// Buffer utilization threshold (0-1) at which backpressure becomes active. Default is 0.8.
    /// </summary>
    public double BackpressureThreshold { get; set; } = 0.8;

    /// <summary>
    /// Behavior when buffer is full. Default is Wait.
    /// </summary>
    public BoundedChannelFullMode FullMode { get; set; } = BoundedChannelFullMode.Wait;

    /// <summary>
    /// Whether to allow single-writer optimization. Default is true.
    /// </summary>
    public bool SingleWriter { get; set; } = true;

    /// <summary>
    /// Whether to allow single-reader optimization. Default is true.
    /// </summary>
    public bool SingleReader { get; set; } = true;
}
