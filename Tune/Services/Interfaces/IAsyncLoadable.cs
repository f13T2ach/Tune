using Tune.Models.Results;
using Tune.Values.Enums;

namespace Tune.Services.Interfaces
{
    /// <summary>
    /// Represents a service that can be asynchronously loaded during application startup.
    /// </summary>
    public interface IAsyncLoadable
    {
        /// <summary>
        /// Asynchronously loads/initializes the service.
        /// </summary>
        /// <returns>A result indicating success or failure with exceptions.</returns>
        Task<ResultWithExceptions> LoadAsync();

        /// <summary>
        /// Gets a value indicating whether the service has been successfully loaded.
        /// </summary>
        bool IsLoaded { get; }

        ELoadableServicesTags tag { get; }
    }
}
