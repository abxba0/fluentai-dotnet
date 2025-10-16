using System.Threading.Channels;

namespace FluentAI.Abstractions.Performance;

/// <summary>
/// Default implementation of backpressure control for streaming operations.
/// </summary>
public class DefaultBackpressureController : IBackpressureController
{
    private readonly BackpressureOptions _options;
    private readonly SemaphoreSlim _semaphore;
    private int _currentBufferSize;

    /// <summary>
    /// Initializes a new instance of the DefaultBackpressureController class.
    /// </summary>
    /// <param name="options">Backpressure configuration options.</param>
    public DefaultBackpressureController(BackpressureOptions? options = null)
    {
        _options = options ?? new BackpressureOptions();
        _semaphore = new SemaphoreSlim(_options.BufferCapacity, _options.BufferCapacity);
        _currentBufferSize = 0;
    }

    /// <inheritdoc/>
    public double BufferUtilization
    {
        get
        {
            var utilized = _options.BufferCapacity - _semaphore.CurrentCount;
            return (double)utilized / _options.BufferCapacity * 100.0;
        }
    }

    /// <inheritdoc/>
    public bool IsBackpressureActive => BufferUtilization >= (_options.BackpressureThreshold * 100.0);

    /// <inheritdoc/>
    public async ValueTask<bool> CanAcceptDataAsync(CancellationToken cancellationToken = default)
    {
        return await _semaphore.WaitAsync(0, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async ValueTask WaitForAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        Interlocked.Increment(ref _currentBufferSize);
    }

    /// <inheritdoc/>
    public void SignalDataConsumed()
    {
        _semaphore.Release();
        Interlocked.Decrement(ref _currentBufferSize);
    }

    /// <inheritdoc/>
    public BoundedChannelOptions GetChannelOptions()
    {
        return new BoundedChannelOptions(_options.BufferCapacity)
        {
            FullMode = _options.FullMode,
            SingleWriter = _options.SingleWriter,
            SingleReader = _options.SingleReader
        };
    }
}
