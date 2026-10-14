using System.IO;
using Newtonsoft.Json;
using Tune.Engine.Effects;
using Tune.Helpers;
using Tune.Utils.FileUtils;
using Tune.Values.Paths;

namespace Tune.Persistence.EqualizerStorage
{
    /// <summary>A named set of ten band gains.</summary>
    public sealed class EqualizerPreset
    {
        /// <summary>Gets or sets the name shown in the picker.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the gains in decibels, one per band, in display order.</summary>
        public List<double> Gains { get; set; } = new List<double>();

        /// <summary>Creates an empty preset, which is what the serializer needs.</summary>
        public EqualizerPreset()
        {
        }

        /// <summary>Creates a preset with the given name and gains.</summary>
        /// <param name="name">Name shown in the picker.</param>
        /// <param name="gains">Gains in decibels, in display order.</param>
        public EqualizerPreset(string name, IEnumerable<double> gains)
        {
            Name = name;
            Gains = new List<double>(gains);
        }

        /// <summary>Creates a preset with every band at zero.</summary>
        /// <param name="name">Name shown in the picker.</param>
        public static EqualizerPreset Flat(string name) =>
            new EqualizerPreset(name, TenBandEqualizerUnit.BandFrequencies.Select(_ => 0.0));

        /// <summary>Copies the preset, so later edits do not reach the stored one.</summary>
        public EqualizerPreset Copy() => new EqualizerPreset(Name, Gains);
    }

    /// <summary>
    /// Keeps the named equalizer presets in a JSON file next to the application,
    /// the same way playlists are kept next to it.
    /// </summary>
    public sealed class EqualizerPresetStore
    {
        private const string FileName = "equalizerpresets.json";

        private readonly List<EqualizerPreset> _presets = new List<EqualizerPreset>();

        /// <summary>Reads the stored presets, so a new store is ready to use.</summary>
        /// <param name="filePath">File to keep the presets in; defaults to the data folder.</param>
        public EqualizerPresetStore(string? filePath = null)
        {
            FilePath = filePath ??
                Path.Combine(EnvironmentUtils.GetEnvironmentDir(), LocalPaths.LOCAL_DATA_N, FileName);
            Load();
        }

        /// <summary>Gets the stored presets, in the order they were saved.</summary>
        public IReadOnlyList<EqualizerPreset> Presets => _presets;

        /// <summary>Gets the full path of the file the presets are kept in.</summary>
        public string FilePath { get; }

        /// <summary>
        /// Reads the presets from disk. A missing or unreadable file leaves the list empty
        /// rather than failing the page that uses it.
        /// </summary>
        public void Load()
        {
            _presets.Clear();

            try
            {
                if (!File.Exists(FilePath)) return;

                var loaded = JsonConvert.DeserializeObject<List<EqualizerPreset>>(File.ReadAllText(FilePath));
                if (loaded == null) return;

                _presets.AddRange(loaded.Where(preset => !string.IsNullOrWhiteSpace(preset.Name)));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to read '{FilePath}'", ex, nameof(Load));
            }
        }

        /// <summary>Writes the presets to disk.</summary>
        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_presets, Formatting.Indented));
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to write '{FilePath}'", ex, nameof(Save));
            }
        }

        /// <summary>Finds a preset by name, ignoring case.</summary>
        /// <param name="name">Name to look for.</param>
        public EqualizerPreset? Find(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;

            return _presets.FirstOrDefault(preset =>
                preset.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Adds a preset, or replaces the one that carries the same name.</summary>
        /// <param name="preset">Preset to store; it is copied, so later edits do not reach it.</param>
        public void AddOrReplace(EqualizerPreset preset)
        {
            EqualizerPreset? existing = Find(preset.Name);
            if (existing == null)
            {
                _presets.Add(preset.Copy());
                return;
            }

            existing.Gains = new List<double>(preset.Gains);
        }

        /// <summary>Removes a preset by name.</summary>
        /// <param name="name">Name of the preset to remove.</param>
        /// <returns>True when a preset was removed.</returns>
        public bool Remove(string? name)
        {
            EqualizerPreset? existing = Find(name);
            if (existing == null) return false;

            return _presets.Remove(existing);
        }

        /// <summary>Gets the preset names, in the order the picker shows them.</summary>
        public IReadOnlyList<string> Names => _presets.Select(preset => preset.Name).ToList();
    }
}
