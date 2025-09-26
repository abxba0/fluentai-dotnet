using System.Threading;
using System.Threading.Tasks;

namespace FluentAI.Abstractions
{
    /// <summary>
    /// Base interface for all AI services in the FluentAI.NET SDK.
    /// </summary>
    public interface IAiService
    {
        /// <summary>
        /// Gets the name of the provider implementing this service.
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Gets the default model name used by this service.
        /// </summary>
        string DefaultModelName { get; }

        /// <summary>
        /// Validates that the service is properly configured and ready for use.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task representing the validation operation.</returns>
        Task ValidateConfigurationAsync(CancellationToken cancellationToken = default);
    }
}