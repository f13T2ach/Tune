using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Tune.Client.Logger;
using Tune.Helpers;
using Tune.Helpers.JSONConverters;
using Tune.Models.Playlist;
using Tune.Models.Results;
using Tune.Utils.FileUtils;
using Tune.Values.Paths;

namespace Tune.Persistence.PlaylistStorage
{
    /// <summary>
    /// Provides file-based persistence for playlist data.
    /// </summary>
    public class PlaylistProvider : PersistenceProviderBase
    {
        private readonly string _playlistDirectory;

        private static readonly object _saveLock = new object();

        /// <summary>
        /// Only to be newed by `PlaylistService`
        /// </summary>
        public PlaylistProvider()
        {
            string env = EnvironmentUtils.GetEnvironmentDir();
            _playlistDirectory = Path.Combine(env, LocalPaths.LOCAL_PLAYLIST_N);
        }


        /// <summary>
        /// Loads all playlists from the storage directory.
        /// </summary>
        /// <returns>A result containing a list of successfully loaded playlists and any exceptions encountered.</returns>
        public ResultContentWithExceptions<List<PlaylistBase>> GetPlaylistBases()
        {
            var playlists = new List<PlaylistBase>();
            var exceptions = new List<Exception>();

            // Ensure directory exists
            if (!Directory.Exists(_playlistDirectory))
            {
                LoggerInstance.Instance.Warning($"Playlist directory does not exist: {_playlistDirectory}", nameof(GetPlaylistBases));
                return ResultContentWithExceptions<List<PlaylistBase>>.SuccessResult(new List<PlaylistBase>());
            }

            string[] filePaths = Directory.GetFiles(_playlistDirectory, "*.json");
            if (filePaths.Length == 0)
            {
                LoggerInstance.Instance.Warning($"No playlist JSON files found in: {_playlistDirectory}", nameof(GetPlaylistBases));
                return ResultContentWithExceptions<List<PlaylistBase>>.SuccessResult(new List<PlaylistBase>());
            }

            foreach (string filePath in filePaths)
            {
                // Read file
                var readResult = FileIOUtils.ReadFile(filePath);
                if (!readResult.Success)
                {
                    string msg = $"Failed to read file: {filePath}, learn more in log file";
                    LoggerHelper.LogError(msg, readResult.Exception, nameof(GetPlaylistBases));
                    exceptions.Add(new Exception(msg, readResult.Exception));
                    continue;
                }

                // Parse JSON
                try
                {
                    JObject json = JObject.Parse(readResult.Content);
                    var parseResult = PlaylistBaseConverter.FromJSONToBase(json);
                    if (!parseResult.Success)
                    {
                        string msg = $"Failed to parse playlist from file: {filePath}, learn more in log file";
                        LoggerHelper.LogError(msg, parseResult.Exception, nameof(GetPlaylistBases));
                        exceptions.Add(new Exception(msg, parseResult.Exception));
                        continue;
                    }

                    playlists.Add(parseResult.Content);
                }
                catch (Exception ex)
                {
                    string msg = $"Unexpected error while parsing file: {filePath}, learn more in log file";
                    LoggerHelper.LogError(msg, ex, nameof(GetPlaylistBases));
                    exceptions.Add(new Exception(msg, ex));
                }
            }

            // Return result
            if (exceptions.Count == 0)
                return ResultContentWithExceptions<List<PlaylistBase>>.SuccessResult(playlists);
            else
                return ResultContentWithExceptions<List<PlaylistBase>>.FailureResult(exceptions, playlists);
        }

        /// <summary>
        /// Saves a single playlist to disk.
        /// IF YOU ARE GOING TO USE THIS DIRECTLY, GO FUCK YOUR SELF.
        /// </summary>
        /// <param name="item">The playlist to save.</param>
        /// <returns>A result indicating success or failure (with exceptions).</returns>
        public ResultWithExceptions SavePlaylist(PlaylistBase item)
        {
            ResultContentWithException<bool> result;
            lock (_saveLock)   // lock at single operation
            {
                if (!Directory.Exists(_playlistDirectory))
                    Directory.CreateDirectory(_playlistDirectory);
                result = SavePlaylistItem(item, nameof(SavePlaylist));
            }
            if (result.Success)
                return ResultWithExceptions.SuccessResult();
            else
                return ResultWithExceptions.FailureResult(result.Exception ?? new Exception($"Failed to save playlist '{item.ID}', learn more in log file"));
        }

        /// <summary>
        /// Saves multiple playlists to disk. Continues processing even if individual items fail.
        /// </summary>
        /// <param name="list">The list of playlists to save.</param>
        /// <returns>A result containing all exceptions that occurred, or success if none.</returns>
        public ResultWithExceptions SavePlaylists(List<PlaylistBase> list)
        {
            var exceptions = new List<Exception>();
            var failedIds = new List<string>();
            lock (_saveLock)   // lock
            {
                if(!Directory.Exists(_playlistDirectory))
                    Directory.CreateDirectory(_playlistDirectory);
                foreach (PlaylistBase item in list)
                {
                    ResultContentWithException<bool> result = SavePlaylistItem(item, nameof(SavePlaylists));
                    if (!result.Success)
                    {
                        exceptions.Add(result.Exception ?? new Exception($"Unknown exception occurred in processing:{item.ID}, learn more in log file"));
                        failedIds.Add(item.ID);
                    }
                }
            }

            if (exceptions.Count == 0)
                return ResultWithExceptions.SuccessResult();

            string combinedMessage = $"Failed to save {exceptions.Count} playlist(s): {string.Join(", ", failedIds)}";
            return ResultWithExceptions.FailureResult(exceptions, combinedMessage);
        }

        /// <summary>
        /// Attempts to save a single playlist item to disk.
        /// </summary>
        /// <param name="item">The playlist to save.</param>
        /// <param name="methodName">The calling method name (for logging).</param>
        private ResultContentWithException<bool> SavePlaylistItem(PlaylistBase item, string methodName)
        {

            try
            {
                //it's alright because we already checked it.
                //Directory.CreateDirectory(_playlistDirectory);

                JObject? json = item.ToJObject();
                if (json == null)
                    throw new InvalidOperationException($"ToJObject() returned null for playlist '{item.ID}'");

                string filePath = Path.Combine(_playlistDirectory, $"{item.ID}.json");
                string jsonString = json.ToString(Newtonsoft.Json.Formatting.Indented);
                FileIOUtils.WriteToFile(filePath, jsonString);

                return ResultContentWithException<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                // directory existence should keep `true`
                LoggerHelper.LogError($"Failed to save playlist '{item.ID}', directory existence: {Directory.Exists(_playlistDirectory)}", ex, methodName);
                return ResultContentWithException<bool>.FailureResult(ex);
            }

        }

        /// <summary>
        /// Deletes a playlist file from disk.
        /// </summary>
        /// <param name="id">The playlist ID to delete.</param>
        /// <returns>A result indicating success or failure (with exceptions).</returns>
        public ResultWithExceptions DeletePlaylist(string id)
        {
            lock (_saveLock)
            {
                try
                {
                    string filePath = Path.Combine(_playlistDirectory, $"{id}.json");
                    if (!File.Exists(filePath))
                    {
                        // not exists, success
                        return ResultWithExceptions.SuccessResult();
                    }

                    File.Delete(filePath);
                    LoggerInstance.Instance.Info($"Deleted playlist file: {filePath}", nameof(DeletePlaylist));
                    return ResultWithExceptions.SuccessResult();
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"Failed to delete playlist '{id}'", ex, nameof(DeletePlaylist));
                    return ResultWithExceptions.FailureResult(ex);
                }
            }
        }


        public ResultWithExceptions DeletePlaylist(PlaylistBase item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return DeletePlaylist(item.ID);
        }
    }
}