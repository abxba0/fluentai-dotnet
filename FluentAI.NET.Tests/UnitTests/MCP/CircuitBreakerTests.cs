using FluentAI.MCP.Resilience;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.MCP;

/// <summary>
/// Unit tests for CircuitBreaker functionality.
/// </summary>
public class CircuitBreakerTests
{
    private readonly Mock<ILogger<CircuitBreaker>> _mockLogger;

    public CircuitBreakerTests()
    {
        _mockLogger = new Mock<ILogger<CircuitBreaker>>();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeCorrectly()
    {
        // Act
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
        Assert.Equal(0, circuitBreaker.FailureCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithInvalidFailureThreshold_ShouldThrowArgumentException(int invalidThreshold)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CircuitBreaker("test", invalidThreshold, TimeSpan.FromSeconds(10), _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithZeroTimeout_ShouldThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new CircuitBreaker("test", 3, TimeSpan.Zero, _mockLogger.Object));
    }

    [Fact]
    public async Task ExecuteAsync_WithSuccessfulOperation_ShouldReturnResult()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);
        var expectedResult = "success";

        // Act
        var result = await circuitBreaker.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            return expectedResult;
        });

        // Assert
        Assert.Equal(expectedResult, result);
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
        Assert.Equal(0, circuitBreaker.FailureCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithFailingOperation_ShouldIncrementFailureCount()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            circuitBreaker.ExecuteAsync(() => Task.FromException(new InvalidOperationException("Test error"))));

        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
        Assert.Equal(1, circuitBreaker.FailureCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithRepeatedFailures_ShouldOpenCircuit()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 2, TimeSpan.FromSeconds(10), _mockLogger.Object);
        var stateChangedEvents = new List<CircuitBreakerStateChangedEventArgs>();
        circuitBreaker.StateChanged += (sender, args) => stateChangedEvents.Add(args);

        // Act - Cause failures to exceed threshold
        for (int i = 0; i < 2; i++)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                circuitBreaker.ExecuteAsync(() => Task.FromException(new InvalidOperationException("Test error"))));
        }

        // Assert
        Assert.Equal(CircuitBreakerState.Open, circuitBreaker.State);
        Assert.Equal(2, circuitBreaker.FailureCount);
        Assert.Single(stateChangedEvents);
        Assert.Equal(CircuitBreakerState.Open, stateChangedEvents[0].CurrentState);
    }

    [Fact]
    public async Task ExecuteAsync_WithOpenCircuit_ShouldThrowCircuitBreakerOpenException()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 1, TimeSpan.FromSeconds(10), _mockLogger.Object);

        // Cause circuit to open
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            circuitBreaker.ExecuteAsync(() => Task.FromException(new InvalidOperationException("Test error"))));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CircuitBreakerOpenException>(() =>
            circuitBreaker.ExecuteAsync(() => Task.FromResult("test")));

        Assert.Equal("test", exception.CircuitBreakerName);
        Assert.True(exception.RemainingTimeout.TotalSeconds > 0);
    }

    [Fact]
    public void Reset_ShouldResetCircuitToClosedState()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 1, TimeSpan.FromSeconds(10), _mockLogger.Object);
        
        // Force circuit to open by causing failure
        circuitBreaker.ExecuteAsync(() => Task.FromException(new InvalidOperationException("Test error")))
            .ContinueWith(_ => { }); // Ignore the exception

        // Act
        circuitBreaker.Reset();

        // Assert
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
        Assert.Equal(0, circuitBreaker.FailureCount);
    }

    [Fact]
    public async Task ExecuteAsync_VoidOperation_ShouldWorkCorrectly()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);
        var operationExecuted = false;

        // Act
        await circuitBreaker.ExecuteAsync(async () =>
        {
            await Task.Delay(1);
            operationExecuted = true;
        });

        // Assert
        Assert.True(operationExecuted);
        Assert.Equal(CircuitBreakerState.Closed, circuitBreaker.State);
    }

    [Fact]
    public void Dispose_ShouldDisposeCleanly()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);

        // Act & Assert - Should not throw
        circuitBreaker.Dispose();
    }

    [Fact]
    public async Task ExecuteAsync_WithNullOperation_ShouldThrowArgumentNullException()
    {
        // Arrange
        var circuitBreaker = new CircuitBreaker("test", 3, TimeSpan.FromSeconds(10), _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            circuitBreaker.ExecuteAsync<string>(null!));
    }
}