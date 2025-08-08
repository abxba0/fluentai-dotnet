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
}