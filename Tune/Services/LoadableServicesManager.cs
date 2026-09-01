using Tune.Helpers;
using Tune.Services.Interfaces;
using Tune.Values.Enums;

namespace Tune.Services
{
    /// <summary>
    /// Centralized manager for loading all registered IAsyncLoadable services.
    /// Provides progress tracking and status queries for other modules.
    /// </summary>
    public static class LoadableServicesManager
    {
        private static readonly List<IAsyncLoadable> _services = new List<IAsyncLoadable>();
        private static readonly object _lock = new object();
        private static int _loadedCount = 0;
        private static int _totalCount = 0;

        /// <summary>
        /// Registers a loadable service.
        /// </summary>
        public static void Register(IAsyncLoadable service)
        {
            foreach (var item in _services)
            {
                if (item.tag == service.tag)
                    throw new Exception($"Already exists same servie tag '{service.tag.ToString()}' in {nameof(LoadableServicesManager)}");
            }

            lock (_lock)
            {
                _services.Add(service);
                _totalCount = _services.Count;
            }
        }

        /// <summary>
        /// Asynchronously loads all registered services.
        /// Each service is loaded independently; failures are logged but do not stop others.
        /// </summary>
        /// <returns>A task that completes when all services have been processed.</returns>
        public static async Task LoadAllAsync()
        {
            List<IAsyncLoadable> snapshot;
            lock (_lock)
            {
                snapshot = _services.ToList();
                _loadedCount = 0;
            }

            // Load each service in parallel (or sequentially if dependencies exist)
            var tasks = snapshot.Select(async service =>
            {
                try
                {
                    var result = await service.LoadAsync();
                    if (result.Success)
                    {
                        lock (_lock) _loadedCount++;
                    }
                    else
                    {
                        // Log failure but continue
                        // You may also capture exceptions in a global collection
                    }
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError(ex, nameof(LoadAllAsync));
                }
            });

            await Task.WhenAll(tasks);
        }


        /// <summary>
        /// Find instance by tag. Null if not exists
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <returns></returns>
        public static IAsyncLoadable? GetLoadableInstance(ELoadableServicesTags tag)
        {
            foreach (var service in _services)
            {
                if (service.tag == tag) return service;
            }
            return null;
        }
        /// <summary>
        /// Gets the total number of registered services.
        /// </summary>
        public static int TotalCount => _totalCount;

        /// <summary>
        /// Gets the number of services that have successfully loaded.
        /// </summary>
        public static int LoadedCount => _loadedCount;

        /// <summary>
        /// Gets the loading progress as a percentage (0.0 to 1.0).
        /// </summary>
        public static double Progress => _totalCount == 0 ? 1.0 : (double)_loadedCount / _totalCount;

        /// <summary>
        /// Gets a value indicating whether all registered services have been loaded successfully.
        /// </summary>
        public static bool AllLoaded => _loadedCount == _totalCount && _totalCount > 0;
    }
}