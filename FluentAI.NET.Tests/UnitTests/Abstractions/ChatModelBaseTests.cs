using FluentAI.Abstractions;
using FluentAI.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Abstractions;

/// <summary>
/// Unit tests for ChatModelBase abstract class following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate ChatModelBase provides correct base functionality for chat models
/// EXPECTED BEHAVIOR: Base class provides validation, retry logic, and common functionality
/// METRICS: Correctness (must be 100% accurate), error handling (proper exceptions), retry logic behavior
/// </summary>
public class ChatModelBaseTests
{
    private readonly Mock<ILogger> _mockLogger;

    public ChatModelBaseTests()
    {
        _mockLogger = new Mock<ILogger>();
    }

    // TEST #1: Normal case - constructor with valid logger
    [Fact]
    public void Constructor_WithValidLogger_CreatesInstance()
    {
        // INPUT: Valid ILogger instance
        // EXPECTED: ChatModelBase instance created successfully
        
        var chatModel = new TestChatModel(_mockLogger.Object);
        
        Assert.NotNull(chatModel);
    }

    // TEST #2: Error case - constructor with null logger
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // INPUT: Null logger parameter
        // EXPECTED: ArgumentNullException should be thrown
        
        Assert.Throws<ArgumentNullException>(() => new TestChatModel(null!));
    }

    // TEST #3: Normal case - ValidateMessages with valid input
    [Fact]
    public void ValidateMessages_WithValidMessages_ReturnsValidatedList()
    {
        // INPUT: List of valid ChatMessage instances
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "Hello"),
            new(ChatRole.Assistant, "Hi there!")
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: Returns validated list with same messages
        var result = chatModel.TestValidateMessages(messages, 1000);

        Assert.Equal(2, result.Count);
        Assert.Equal(messages[0], result[0]);
        Assert.Equal(messages[1], result[1]);
    }

    // TEST #4: Error case - ValidateMessages with null messages
    [Fact]
    public void ValidateMessages_WithNullMessages_ThrowsArgumentNullException()
    {
        // INPUT: Null messages collection
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentNullException should be thrown
        Assert.Throws<ArgumentNullException>(() => chatModel.TestValidateMessages(null!, 1000));
    }

    // TEST #5: Error case - ValidateMessages with empty messages
    [Fact]
    public void ValidateMessages_WithEmptyMessages_ThrowsArgumentException()
    {
        // INPUT: Empty messages collection
        var messages = new List<ChatMessage>();
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentException should be thrown
        Assert.Throws<ArgumentException>(() => chatModel.TestValidateMessages(messages, 1000));
    }

    // TEST #6: Error case - ValidateMessages with null message in collection
    [Fact]
    public void ValidateMessages_WithNullMessageInCollection_ThrowsArgumentException()
    {
        // INPUT: Messages collection containing null element
        var messages = new List<ChatMessage?>
        {
            new(ChatRole.User, "Hello"),
            null,
            new(ChatRole.Assistant, "Hi")
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentException should be thrown
        Assert.Throws<ArgumentException>(() => chatModel.TestValidateMessages(messages!, 1000));
    }

    // TEST #7: Error case - ValidateMessages with empty content
    [Fact]
    public void ValidateMessages_WithEmptyContent_ThrowsArgumentException()
    {
        // INPUT: Message with empty content
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, ""),
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentException should be thrown
        Assert.Throws<ArgumentException>(() => chatModel.TestValidateMessages(messages, 1000));
    }

    // TEST #8: Error case - ValidateMessages with whitespace content
    [Fact]
    public void ValidateMessages_WithWhitespaceContent_ThrowsArgumentException()
    {
        // INPUT: Message with whitespace-only content
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "   "),
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentException should be thrown
        Assert.Throws<ArgumentException>(() => chatModel.TestValidateMessages(messages, 1000));
    }

    // TEST #9: Edge case - ValidateMessages at size limit
    [Fact]
    public void ValidateMessages_AtSizeLimit_ReturnsValidatedList()
    {
        // INPUT: Messages that exactly reach the size limit
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "12345"), // 5 characters
            new(ChatRole.Assistant, "67890") // 5 characters, total = 10
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: Should not throw, exactly at limit
        var result = chatModel.TestValidateMessages(messages, 10);
        
        Assert.Equal(2, result.Count);
    }

    // TEST #10: Error case - ValidateMessages exceeding size limit
    [Fact]
    public void ValidateMessages_ExceedingSizeLimit_ThrowsArgumentException()
    {
        // INPUT: Messages that exceed the size limit
        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, "12345"), // 5 characters
            new(ChatRole.Assistant, "678901") // 6 characters, total = 11
        };
        var chatModel = new TestChatModel(_mockLogger.Object);

        // EXPECTED: ArgumentException should be thrown
        Assert.Throws<ArgumentException>(() => chatModel.TestValidateMessages(messages, 10));
    }

    // TEST #11: Normal case - ExecuteWithRetryAsync succeeds on first try
    [Fact]
    public async Task ExecuteWithRetryAsync_SucceedsOnFirstTry_ReturnsResult()
    {
        // INPUT: Operation that succeeds immediately
        var chatModel = new TestChatModel(_mockLogger.Object);
        var expectedResult = "Success";

        // EXPECTED: Returns result without retries
        var result = await chatModel.TestExecuteWithRetryAsync(
            () => Task.FromResult(expectedResult),
            3,
            ex => true,
            CancellationToken.None);

        Assert.Equal(expectedResult, result);
    }

    // TEST #12: Normal case - ExecuteWithRetryAsync succeeds after retries
    [Fact]
    public async Task ExecuteWithRetryAsync_SucceedsAfterRetries_ReturnsResult()
    {
        // INPUT: Operation that fails twice then succeeds
        var chatModel = new TestChatModel(_mockLogger.Object);
        var callCount = 0;
        var expectedResult = "Success";

        Func<Task<string>> operation = () =>
        {
            callCount++;
            if (callCount <= 2)
                throw new InvalidOperationException("Temporary failure");
            return Task.FromResult(expectedResult);
        };

        // EXPECTED: Returns result after retries
        var result = await chatModel.TestExecuteWithRetryAsync(
            operation,
            3,
            ex => ex is InvalidOperationException,
            CancellationToken.None);

        Assert.Equal(expectedResult, result);
        Assert.Equal(3, callCount); // Should have been called 3 times
    }

    // TEST #13: Error case - ExecuteWithRetryAsync exceeds max retries
    [Fact]
    public async Task ExecuteWithRetryAsync_ExceedsMaxRetries_ThrowsLastException()
    {
        // INPUT: Operation that always fails
        var chatModel = new TestChatModel(_mockLogger.Object);
        var callCount = 0;
        var expectedException = new InvalidOperationException("Always fails");

        Func<Task<string>> operation = () =>
        {
            callCount++;
            throw expectedException;
        };

        // EXPECTED: Should throw the last exception after max retries
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => chatModel.TestExecuteWithRetryAsync(
                operation,
                2,
                ex => ex is InvalidOperationException,
                CancellationToken.None));

        Assert.Same(expectedException, actualException);
        Assert.Equal(3, callCount); // Initial call + 2 retries = 3 total calls
    }

    // TEST #14: Error case - ExecuteWithRetryAsync with non-retriable error
    [Fact]
    public async Task ExecuteWithRetryAsync_WithNonRetriableError_ThrowsImmediately()
    {
        // INPUT: Operation that throws non-retriable error
        var chatModel = new TestChatModel(_mockLogger.Object);
        var callCount = 0;
        var expectedException = new ArgumentException("Non-retriable");

        Func<Task<string>> operation = () =>
        {
            callCount++;
            throw expectedException;
        };

        // EXPECTED: Should throw immediately without retries
        var actualException = await Assert.ThrowsAsync<ArgumentException>(
            () => chatModel.TestExecuteWithRetryAsync(
                operation,
                3,
                ex => ex is InvalidOperationException, // Only InvalidOperationException is retriable
                CancellationToken.None));

        Assert.Same(expectedException, actualException);
        Assert.Equal(1, callCount); // Should only be called once
    }

    // TEST #15: Error case - ExecuteWithRetryAsync with cancellation
    [Fact]
    public async Task ExecuteWithRetryAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // INPUT: Cancelled cancellation token
        var chatModel = new TestChatModel(_mockLogger.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task<string>> operation = () =>
        {
            throw new InvalidOperationException("Should not reach here");
        };

        // EXPECTED: Should throw OperationCanceledException (or derived TaskCanceledException)
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => chatModel.TestExecuteWithRetryAsync(
                operation,
                3,
                ex => true,
                cts.Token));
    }
}

/// <summary>
/// Test implementation of ChatModelBase for testing protected methods.
/// </summary>
internal class TestChatModel : ChatModelBase
{
    public TestChatModel(ILogger logger) : base(logger)
    {
    }

    public override Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("This is a test implementation");
    }

    public override IAsyncEnumerable<string> StreamResponseAsync(IEnumerable<ChatMessage> messages, ChatRequestOptions? options = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("This is a test implementation");
    }

    // Expose protected methods for testing
    public List<ChatMessage> TestValidateMessages(IEnumerable<ChatMessage> messages, long maxRequestSize)
    {
        return ValidateMessages(messages, maxRequestSize);
    }

    public Task<T> TestExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries,
        Func<Exception, bool> isRetriableError,
        CancellationToken cancellationToken)
    {
        return ExecuteWithRetryAsync(operation, maxRetries, isRetriableError, cancellationToken);
    }
}