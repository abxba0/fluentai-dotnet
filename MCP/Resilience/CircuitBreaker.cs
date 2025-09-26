using Microsoft.Extensions.Logging;

namespace FluentAI.MCP.Resilience;

/// <summary>
/// Circuit breaker pattern implementation for MCP connections to prevent cascade failures.
/// </summary>
public class CircuitBreaker : IDisposable
{
    private readonly string _name;
    private readonly int _failureThreshold;
    private readonly TimeSpan _timeout;
    private readonly ILogger<CircuitBreaker> _logger;
    private readonly object _lockObject = new();
    
    private CircuitBreakerState _state = CircuitBreakerState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the CircuitBreaker class.
    /// </summary>
    /// <param name="name">The name of the circuit breaker for logging.</param>
    /// <param name="failureThreshold">Number of failures before opening the circuit.</param>
    /// <param name="timeout">Time to wait before attempting to close the circuit.</param>
    /// <param name="logger">Logger instance.</param>
    public CircuitBreaker(
        string name, 
        int failureThreshold, 
        TimeSpan timeout, 
        ILogger<CircuitBreaker> logger)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _failureThreshold = failureThreshold > 0 ? failureThreshold : throw new ArgumentException("Failure threshold must be positive", nameof(failureThreshold));
        _timeout = timeout > TimeSpan.Zero ? timeout : throw new ArgumentException("Timeout must be positive", nameof(timeout));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current state of the circuit breaker.
    /// </summary>
    public CircuitBreakerState State
    {
        get
        {
            lock (_lockObject)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets the current failure count.
    /// </summary>
    public int FailureCount
    {
        get
        {
            lock (_lockObject)
            {
                return _failureCount;
            }
        }
    }

    /// <summary>
    /// Executes an operation through the circuit breaker.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is open.</exception>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        CheckState();

        try
        {
            var result = await operation();
            OnSuccess();
            return result;
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Executes an operation through the circuit breaker without a return value.
    /// </summary>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <exception cref="CircuitBreakerOpenException">Thrown when the circuit breaker is open.</exception>
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        CheckState();

        try
        {
            await operation();
            OnSuccess();
        }
        catch (Exception ex)
        {
            OnFailure(ex);
            throw;
        }
    }

    /// <summary>
    /// Manually resets the circuit breaker to closed state.
    /// </summary>
    public void Reset()
    {
        lock (_lockObject)
        {
            _state = CircuitBreakerState.Closed;
            _failureCount = 0;
            _lastFailureTime = DateTime.MinValue;
            
            _logger.LogInformation("Circuit breaker {Name} has been manually reset", _name);
        }
    }

    /// <summary>
    /// Event raised when the circuit breaker state changes.
    /// </summary>
    public event EventHandler<CircuitBreakerStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the CircuitBreaker and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _disposed = true;
        }
    }

    private void CheckState()
    {
        lock (_lockObject)
        {
            switch (_state)
            {
                case CircuitBreakerState.Closed:
                    // Normal operation
                    break;

                case CircuitBreakerState.Open:
                    // Check if timeout has passed to attempt half-open
                    if (DateTime.UtcNow - _lastFailureTime >= _timeout)
                    {
                        ChangeState(CircuitBreakerState.HalfOpen);
                        _logger.LogInformation("Circuit breaker {Name} transitioning to half-open after timeout", _name);
                    }
                    else
                    {
                        throw new CircuitBreakerOpenException(_name, _timeout - (DateTime.UtcNow - _lastFailureTime));
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // Allow one attempt to test if service is recovered
                    break;
            }
        }
    }

    private void OnSuccess()
    {
        lock (_lockObject)
        {
            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Service appears to be recovered
                ChangeState(CircuitBreakerState.Closed);
                _failureCount = 0;
                _logger.LogInformation("Circuit breaker {Name} closed after successful half-open attempt", _name);
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount > 0)
            {
                // Reset failure count on successful operation
                _failureCount = 0;
                _logger.LogDebug("Circuit breaker {Name} failure count reset after success", _name);
            }
        }
    }

    private void OnFailure(Exception exception)
    {
        lock (_lockObject)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            _logger.LogWarning(exception, "Circuit breaker {Name} recorded failure {FailureCount}/{Threshold}", 
                _name, _failureCount, _failureThreshold);

            if (_state == CircuitBreakerState.HalfOpen)
            {
                // Half-open attempt failed, go back to open
                ChangeState(CircuitBreakerState.Open);
                _logger.LogWarning("Circuit breaker {Name} opened after failed half-open attempt", _name);
            }
            else if (_state == CircuitBreakerState.Closed && _failureCount >= _failureThreshold)
            {
                // Threshold exceeded, open the circuit
                ChangeState(CircuitBreakerState.Open);
                _logger.LogError("Circuit breaker {Name} opened after {FailureCount} failures", _name, _failureCount);
            }
        }
    }

    private void ChangeState(CircuitBreakerState newState)
    {
        var previousState = _state;
        _state = newState;

        try
        {
            StateChanged?.Invoke(this, new CircuitBreakerStateChangedEventArgs
            {
                Name = _name,
                PreviousState = previousState,
                CurrentState = newState,
                FailureCount = _failureCount,
                LastFailureTime = _lastFailureTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error raising state change event for circuit breaker {Name}", _name);
        }
    }
}

/// <summary>
/// Represents the state of a circuit breaker.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed, allowing normal operation.
    /// </summary>
    Closed,

    /// <summary>
    /// Circuit is open, preventing operation due to failures.
    /// </summary>
    Open,

    /// <summary>
    /// Circuit is half-open, allowing limited attempts to test recovery.
    /// </summary>
    HalfOpen
}

/// <summary>
/// Event arguments for circuit breaker state changes.
/// </summary>
public class CircuitBreakerStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the name of the circuit breaker.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the previous state.
    /// </summary>
    public CircuitBreakerState PreviousState { get; init; }

    /// <summary>
    /// Gets the current state.
    /// </summary>
    public CircuitBreakerState CurrentState { get; init; }

    /// <summary>
    /// Gets the current failure count.
    /// </summary>
    public int FailureCount { get; init; }

    /// <summary>
    /// Gets the timestamp of the last failure.
    /// </summary>
    public DateTime LastFailureTime { get; init; }
}

/// <summary>
/// Exception thrown when a circuit breaker is open and prevents operation execution.
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    /// <summary>
    /// Gets the name of the circuit breaker.
    /// </summary>
    public string CircuitBreakerName { get; }

    /// <summary>
    /// Gets the remaining time until the circuit breaker will attempt to half-open.
    /// </summary>
    public TimeSpan RemainingTimeout { get; }

    /// <summary>
    /// Initializes a new instance of the CircuitBreakerOpenException class.
    /// </summary>
    /// <param name="circuitBreakerName">The name of the circuit breaker.</param>
    /// <param name="remainingTimeout">The remaining timeout duration.</param>
    public CircuitBreakerOpenException(string circuitBreakerName, TimeSpan remainingTimeout)
        : base($"Circuit breaker '{circuitBreakerName}' is open. Retry in {remainingTimeout.TotalSeconds:F1} seconds.")
    {
        CircuitBreakerName = circuitBreakerName;
        RemainingTimeout = remainingTimeout;
    }
}