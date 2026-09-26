using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows;
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
        // Observable mirror of _cache, exposed for UI binding.
        private readonly ObservableCollection<PlaylistBase> _playlists;
        private bool _isInitialized = false;
        public bool IsLoaded => _isInitialized;

        public ELoadableServicesTags tag => ELoadableServicesTags.PlaylistServiceTag;

        private readonly object _initLock = new object();

        public PlaylistService(PlaylistProvider provider)
        {
            _provider = provider;
            _cache = new ConcurrentDictionary<string, PlaylistBase>();
            _playlists = new ObservableCollection<PlaylistBase>();
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

                var loadedPlaylists = loadResult.Content.ToList();
                foreach (var playlist in loadedPlaylists)
                {
                    _cache.TryAdd(playlist.ID, playlist);
                }

                // Rebuild the observable collection in a single UI-thread dispatch.
                RunOnUIThread(() =>
                {
                    _playlists.Clear();
                    foreach (var playlist in loadedPlaylists)
                        _playlists.Add(playlist);
                });

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
        public ObservableCollection<PlaylistBase> GetAllPlaylists()
        {
            return _playlists;
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

            // Keep the observable collection in sync so bound UI updates immediately.
            RunOnUIThread(() =>
            {
                var index = IndexOfPlaylistById(item.ID);
                if (index >= 0)
                    _playlists[index] = item;
                else
                    _playlists.Add(item);
            });

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

            // Keep the observable collection in sync so bound UI updates immediately.
            RunOnUIThread(() =>
            {
                var index = IndexOfPlaylistById(id);
                if (index >= 0)
                    _playlists.RemoveAt(index);
            });

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

        // Find the index of a playlist in the observable collection by its ID.
        private int IndexOfPlaylistById(string id)
        {
            for (int i = 0; i < _playlists.Count; i++)
            {
                if (_playlists[i].ID == id)
                    return i;
            }
            return -1;
        }

        // Send an action onto the UI thread when a dispatcher is available.
        private static void RunOnUIThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }
    }
}