using FluentAI.Abstractions.Models;

namespace FluentAI.Abstractions.Exceptions
{
    /// <summary>
    /// Base exception for multi-modal AI operations.
    /// </summary>
    public class MultiModalException : AiSdkException
    {
        /// <summary>
        /// Gets the modality this exception relates to.
        /// </summary>
        public ModalityType Modality { get; }

        /// <summary>
        /// Gets the provider name this exception relates to.
        /// </summary>
        public string Provider { get; }

        /// <summary>
        /// Gets the model name this exception relates to.
        /// </summary>
        public string ModelName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiModalException"/> class.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="modelName">The model name.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MultiModalException(
            ModalityType modality,
            string provider,
            string modelName,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Modality = modality;
            Provider = provider ?? string.Empty;
            ModelName = modelName ?? string.Empty;
        }
    }

    /// <summary>
    /// Exception thrown when a requested modality is not supported by a provider.
    /// </summary>
    public class UnsupportedModalityException : MultiModalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedModalityException"/> class.
        /// </summary>
        /// <param name="modality">The unsupported modality.</param>
        /// <param name="provider">The provider name.</param>
        public UnsupportedModalityException(ModalityType modality, string provider)
            : base(modality, provider, string.Empty, $"Provider '{provider}' does not support modality '{modality}'")
        {
        }
    }

    /// <summary>
    /// Exception thrown when a requested model is not available.
    /// </summary>
    public class ModelNotAvailableException : MultiModalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelNotAvailableException"/> class.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="modelName">The unavailable model name.</param>
        /// <param name="availableModels">The list of available models.</param>
        public ModelNotAvailableException(
            ModalityType modality,
            string provider,
            string modelName,
            IEnumerable<string>? availableModels = null)
            : base(modality, provider, modelName,
                $"Model '{modelName}' is not available for {modality} on provider '{provider}'" +
                (availableModels != null ? $". Available models: {string.Join(", ", availableModels)}" : ""))
        {
            AvailableModels = availableModels?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets the list of available models for this modality and provider.
        /// </summary>
        public IReadOnlyList<string> AvailableModels { get; }
    }

    /// <summary>
    /// Exception thrown when input format is invalid for a modality.
    /// </summary>
    public class InvalidInputFormatException : MultiModalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputFormatException"/> class.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="modelName">The model name.</param>
        /// <param name="inputType">The invalid input type.</param>
        /// <param name="expectedFormats">The expected input formats.</param>
        public InvalidInputFormatException(
            ModalityType modality,
            string provider,
            string modelName,
            string inputType,
            IEnumerable<string>? expectedFormats = null)
            : base(modality, provider, modelName,
                $"Invalid input format '{inputType}' for {modality} on {provider}/{modelName}" +
                (expectedFormats != null ? $". Expected formats: {string.Join(", ", expectedFormats)}" : ""))
        {
            InputType = inputType;
            ExpectedFormats = expectedFormats?.ToList() ?? new List<string>();
        }

        /// <summary>
        /// Gets the invalid input type.
        /// </summary>
        public string InputType { get; }

        /// <summary>
        /// Gets the expected input formats.
        /// </summary>
        public IReadOnlyList<string> ExpectedFormats { get; }
    }

    /// <summary>
    /// Exception thrown when content violates policy rules.
    /// </summary>
    public class ContentPolicyViolationException : MultiModalException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPolicyViolationException"/> class.
        /// </summary>
        /// <param name="modality">The modality type.</param>
        /// <param name="provider">The provider name.</param>
        /// <param name="modelName">The model name.</param>
        /// <param name="violationType">The type of violation.</param>
        /// <param name="details">Additional details about the violation.</param>
        public ContentPolicyViolationException(
            ModalityType modality,
            string provider,
            string modelName,
            string violationType,
            string? details = null)
            : base(modality, provider, modelName,
                $"Content policy violation: {violationType}" +
                (!string.IsNullOrEmpty(details) ? $". Details: {details}" : ""))
        {
            ViolationType = violationType;
            Details = details;
        }

        /// <summary>
        /// Gets the type of policy violation.
        /// </summary>
        public string ViolationType { get; }

        /// <summary>
        /// Gets additional details about the violation.
        /// </summary>
        public string? Details { get; }
    }
}