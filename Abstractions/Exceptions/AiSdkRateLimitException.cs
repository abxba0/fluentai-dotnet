namespace FluentAI.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a rate limit is exceeded.
/// </summary>
public class AiSdkRateLimitException : AiSdkException
{
    /// <summary>
    /// Initializes a new instance of the AiSdkRateLimitException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public AiSdkRateLimitException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the AiSdkRateLimitException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public AiSdkRateLimitException(string message, Exception innerException) : base(message, innerException) { }
}