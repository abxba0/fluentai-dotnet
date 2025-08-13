namespace FluentAI.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when there's a configuration error in the AI SDK.
/// </summary>
public class AiSdkConfigurationException : AiSdkException
{
    /// <summary>
    /// Initializes a new instance of the AiSdkConfigurationException class with a specified error message.
    /// </summary>
    /// <param name="message">The configuration error message.</param>
    public AiSdkConfigurationException(string message) : base(message) { }
    
    /// <summary>
    /// Initializes a new instance of the AiSdkConfigurationException class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The configuration error message.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public AiSdkConfigurationException(string message, Exception innerException) : base(message, innerException) { }
}