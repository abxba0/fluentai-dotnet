// AiSdkException.cs
namespace Genius.Core.Abstractions.Exceptions;
public class AiSdkException : Exception
{
    public AiSdkException(string message) : base(message) { }
    public AiSdkException(string message, Exception innerException) : base(message, innerException) { }
}
