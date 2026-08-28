using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Models.Results;
using Tune.Values.Enums;

namespace Tune.Helpers.JSONConverters
{
    public static class PlaylistBaseConverter
    {
        public static JObject? FromCollectionPlaylistToJSON(CollectionPlaylist arg)
        {
            var jObject = new JObject
            {
                ["ID"] = arg.ID,
                ["Name"] = arg.Name,
                ["Description"] = arg.Description,
                ["Type"] = arg.Type.ToString(),               // 或 (int)Type 取决于需求
                ["IsDeleted"] = arg.IsDeleted,
                ["isEmptyOrNotInited"] = arg.isEmptyOrNotInited
            };

            // SourceFrom 可能为 null，显式处理
            if (arg.SourceFrom != null)
                jObject["SourceFrom"] = new JArray(arg.SourceFrom);
            else
                jObject["SourceFrom"] = null;             // 或者不添加此字段

            // 序列化音乐列表
            var musicArray = new JArray();
            foreach (var music in arg.MusicList)
            {
                if (music != null)
                {
                    // 假设 Music 类也实现了 ToJObject()
                    var musicPath = music.MusicPath;
                    musicArray.Add(musicPath);
                }
                else
                {
                    musicArray.Add(JValue.CreateNull());
                }
            }
            jObject["MusicList"] = musicArray;

            return jObject;
        }

        /// <summary>
        /// Converts a JSON object to a CollectionPlaylist instance.
        /// Skips null elements in MusicList.
        /// </summary>
        /// <param name="collectionPlaylistRaw">The JObject representing the playlist.</param>
        /// <returns>A result containing the playlist or an exception.</returns>
        public static ResultContentWithException<PlaylistBase> FromJSONToCollectionPlaylist(JObject collectionPlaylistRaw)
        {
            try
            {
                // Safely read required fields, using null-conditional and null-coalescing
                string id = collectionPlaylistRaw["ID"]?.Value<string>() ?? string.Empty;
                string name = collectionPlaylistRaw["Name"]?.Value<string>() ?? string.Empty;
                string description = collectionPlaylistRaw["Description"]?.Value<string>() ?? string.Empty;

                var playlist = new CollectionPlaylist(id, name, description);

                // Read optional boolean fields with explicit null check to avoid Value<bool> on null token
                JToken? isDeletedToken = collectionPlaylistRaw["IsDeleted"];
                if (isDeletedToken != null && isDeletedToken.Type == JTokenType.Boolean)
                    playlist.IsDeleted = isDeletedToken.Value<bool>();

                JToken? isEmptyToken = collectionPlaylistRaw["isEmptyOrNotInited"];
                if (isEmptyToken != null && isEmptyToken.Type == JTokenType.Boolean)
                    playlist.isEmptyOrNotInited = isEmptyToken.Value<bool>();

                // Read SourceFrom (can be null or array)
                JToken? sourceToken = collectionPlaylistRaw["SourceFrom"];
                if (sourceToken != null && sourceToken.Type == JTokenType.Array)
                {
                    var sourceList = new List<string>();
                    foreach (JToken item in (JArray)sourceToken)
                    {
                        // Only add non-null string values
                        if (item.Type == JTokenType.String)
                        {
                            string? value = item.Value<string>();
                            if (!string.IsNullOrEmpty(value))
                                sourceList.Add(value);
                        }
                    }
                    playlist.SourceFrom = sourceList;
                }
                else
                {
                    playlist.SourceFrom = null; // explicit null if missing or not an array
                }

                // Read MusicList (array of music paths) - skip null elements
                JToken? musicListToken = collectionPlaylistRaw["MusicList"];
                if (musicListToken != null && musicListToken.Type == JTokenType.Array)
                {
                    var musicList = new List<Music>();
                    foreach (JToken item in (JArray)musicListToken)
                    {
                        // Skip null tokens
                        if (item.Type == JTokenType.Null)
                            continue;

                        // Only process string values
                        if (item.Type == JTokenType.String)
                        {
                            string? path = item.Value<string>();
                            if (!string.IsNullOrEmpty(path))
                            {
                                var music = new Music(path);
                                musicList.Add(music);
                            }
                        }
                        // Optionally handle other types? Ignored for now.
                    }
                    playlist.MusicList = musicList;
                }
                else
                {
                    playlist.MusicList = new List<Music>(); // empty list if missing or invalid
                }

                return ResultContentWithException<PlaylistBase>.SuccessResult(playlist);
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError("Unable to prase collection playlist", ex);
                return ResultContentWithException<PlaylistBase>.FailureResult(ex, "Failed to deserialize CollectionPlaylist from JSON.");
            }
        }

        /// <summary>
        /// Converts a JSON object to the appropriate PlaylistBase subclass by inspecting the "Type" field.
        /// THIS MIGHT THROW EXCEPTION!!!
        /// </summary>
        /// <param name="raw">The JObject representing the playlist.</param>
        /// <returns>A result containing the playlist or an exception.</returns>
        public static ResultContentWithException<PlaylistBase> FromJSONToBase(JObject raw)
        {
            try
            {
                if (raw == null)
                    throw new ArgumentNullException(nameof(raw), "JSON object cannot be null.");

                // Safely read the "Type" field
                JToken? typeToken = raw["Type"];
                if (typeToken == null)
                    throw new InvalidOperationException("Missing 'Type' field in JSON.");

                // Ensure it's a string
                string? typeString = typeToken.Value<string>();
                if (string.IsNullOrEmpty(typeString))
                    throw new InvalidOperationException("'Type' field is empty or not a valid string.");

                // Compare case-insensitively
                // ============= 在此处判断类型 =============
                if (string.Equals(typeString, EPlaylistType.Collection.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return FromJSONToCollectionPlaylist(raw);
                }
                // ============= 在此处判断类型 =============
                else
                {
                    throw new NotSupportedException($"Playlist type '{typeString}' is not supported for deserialization.");
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError("Unable to prase", ex);
                return ResultContentWithException<PlaylistBase>.FailureResult(ex, "Failed to deserialize PlaylistBase from JSON.");
            }
        }

    }
}
