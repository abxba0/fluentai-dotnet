namespace FluentAI.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when there's an error in AI SDK operations.
/// </summary>
public class AiSdkException : Exception
{
    /// <summary>
    /// Initializes a new instance of the AiSdkException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public AiSdkException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the AiSdkException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public AiSdkException(string message, Exception innerException) : base(message, innerException) { }
}
