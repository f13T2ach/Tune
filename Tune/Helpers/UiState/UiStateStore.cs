using System.IO;
using Newtonsoft.Json;
using Tune.Helpers;
using Tune.Utils.FileUtils;
using Tune.Values.Paths;

namespace Tune.Helpers.UiState
{
    /// <summary>
    /// Remembers small pieces of interface state - the page a toolbar had selected, the
    /// tab a tab bar was on - in the data folder of the application, the way playlists
    /// and equalizer presets are kept.
    /// </summary>
    public static class UiStateStore
    {
        private const string FileName = "uistate.json";

        /// <summary>Gets the full path of the file the state is kept in.</summary>
        public static string FilePath =>
            Path.Combine(EnvironmentUtils.GetEnvironmentDir(), LocalPaths.LOCAL_DATA_N, FileName);

        private static readonly Dictionary<string, string> Values = Load();

        /// <summary>Reads a remembered value.</summary>
        /// <param name="key">Name the value was stored under.</param>
        /// <returns>The value, or an empty string when nothing was stored.</returns>
        public static string Get(string key)
        {
            lock (Values)
            {
                return Values.TryGetValue(key, out string? value) ? value : string.Empty;
            }
        }

        /// <summary>Remembers a value and writes the file.</summary>
        /// <param name="key">Name to store the value under.</param>
        /// <param name="value">The value; an empty one forgets the key.</param>
        public static void Set(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            lock (Values)
            {
                if (string.IsNullOrEmpty(value))
                    Values.Remove(key);
                else
                    Values[key] = value;

                Save();
            }
        }

        private static Dictionary<string, string> Load()
        {
            try
            {
                if (!File.Exists(FilePath)) return new Dictionary<string, string>();

                var loaded = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(FilePath));

                return loaded ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to read '{FilePath}'", ex, nameof(Load));

                return new Dictionary<string, string>();
            }
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Values, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to write '{FilePath}'", ex, nameof(Save));
            }
        }
    }
}
