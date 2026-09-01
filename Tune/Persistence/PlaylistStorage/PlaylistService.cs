using System.Collections.Concurrent;
using Tune.Models.Playlist;
using Tune.Models.Results;
using Tune.Services.Interfaces;
using Tune.Values.Enums;

namespace Tune.Persistence.PlaylistStorage
{
    public class PlaylistService : IAsyncLoadable
    {
        private readonly PlaylistProvider _provider;
        private readonly ConcurrentDictionary<string, PlaylistBase> _cache;
        private bool _isInitialized = false;
        public bool IsLoaded => _isInitialized;

        public ELoadableServicesTags tag => ELoadableServicesTags.PlaylistServiceTag;

        private readonly object _initLock = new object();

        public PlaylistService(PlaylistProvider provider)
        {
            _provider = provider;
            _cache = new ConcurrentDictionary<string, PlaylistBase>();
        }

        /// <summary>
        /// Async init
        /// </summary>
        /// <returns>A task representing the initialization operation.</returns>
        public async Task<ResultWithExceptions> LoadAsync()
        {
            // Offload the synchronous work to a thread-pool thread to avoid blocking the UI.
            return await Task.Run(() => Initialize());
        }

        /// <summary>
        /// Initialize to sync with the local playlists
        /// </summary>
        /// <returns></returns>
        public ResultWithExceptions Initialize()
        {
            lock (_initLock)
            {
                if (_isInitialized)
                    return ResultWithExceptions.SuccessResult();

                var loadResult = _provider.GetPlaylistBases();

                // Cleanup cache and rebuild the cache
                _cache.Clear();
                if (loadResult.Content == null)
                    return ResultWithExceptions.FailureResult(new ArgumentNullException("_provider.GetPlaylistBases() is null, find the logs above this line"));

                foreach (var playlist in loadResult.Content)
                {
                    _cache.TryAdd(playlist.ID, playlist);
                }

                _isInitialized = true;

                if (!loadResult.Success)
                    return ResultWithExceptions.FailureResult(loadResult.Exceptions ?? new[] { new Exception("PlaylistService.Initialize() failed and get null loadResult.Exceptions") }, Assets.Localization.LangStrings.PartOfPlaylistNotInited);

                return ResultWithExceptions.SuccessResult();
            }
        }

        /// <summary>
        /// Get all playlists from memory
        /// </summary>
        /// <returns></returns>
        public List<PlaylistBase> GetAllPlaylists()
        {
            return _cache.Values.ToList();
        }

        /// <summary>
        /// Get single playlist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PlaylistBase? GetPlaylistById(string id)
        {
            _cache.TryGetValue(id, out var playlist);
            return playlist;
        }

        /// <summary>
        /// Save playlist to app & local
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ResultWithExceptions SavePlaylist(PlaylistBase item)
        {
            // local first
            var saveResult = _provider.SavePlaylist(item);
            if (!saveResult.Success)
                return saveResult;

            _cache.AddOrUpdate(item.ID, item, (key, old) => item);
            return ResultWithExceptions.SuccessResult();
        }

        /// <summary>
        /// Delete single playlist
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ResultWithExceptions DeletePlaylist(string id)
        {

            var deleteResult = _provider.DeletePlaylist(id);
            if (!deleteResult.Success)
                return deleteResult;

            _cache.TryRemove(id, out _);
            return ResultWithExceptions.SuccessResult();
        }

        /// <summary>
        /// save all to local
        /// DO NOT USE IT AT EXIT: 
        /// It might be risky when the system is shutting down.
        /// </summary>
        /// <returns></returns>
        public ResultWithExceptions SaveAll()
        {
            var all = _cache.Values.ToList();
            return _provider.SavePlaylists(all);
        }
    }
}
