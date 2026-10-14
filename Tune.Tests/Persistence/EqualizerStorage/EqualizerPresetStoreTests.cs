using System.IO;

using Tune.Persistence.EqualizerStorage;
using Xunit;

namespace Tune.Tests.Persistence.EqualizerStorage
{
    /// <summary>Verifies how <see cref="EqualizerPresetStore"/> keeps its presets.</summary>
    public class EqualizerPresetStoreTests : IDisposable
    {
        private readonly string _filePath =
            Path.Combine(Path.GetTempPath(), $"tune-eq-{Guid.NewGuid():N}.json");

        /// <summary>Removes the temporary preset file of this test.</summary>
        public void Dispose()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        #region ==== Reading and Writing ====

        /// <summary>Verifies that a missing file leaves the store empty instead of failing.</summary>
        [Fact]
        public void Presets_WithoutFile_AreEmpty()
        {
            var store = new EqualizerPresetStore(_filePath);

            Assert.Empty(store.Presets);
        }

        /// <summary>Verifies that saved presets survive a reload.</summary>
        [Fact]
        public void Presets_AfterSaveAndReload_AreKept()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(new EqualizerPreset("Rock", new double[] { 4, 3, 2, 1, 0, -1, -2, -3, -4, -5 }));
            store.Save();

            var reloaded = new EqualizerPresetStore(_filePath);

            Assert.Single(reloaded.Presets);
            Assert.Equal("Rock", reloaded.Presets[0].Name);
            Assert.Equal(4.0, reloaded.Presets[0].Gains[0]);
            Assert.Equal(-5.0, reloaded.Presets[0].Gains[9]);
        }

        #endregion

        #region ==== Managing ====

        /// <summary>Verifies that a preset can be found by name whatever its casing.</summary>
        [Fact]
        public void Find_WithDifferentCasing_ReturnsThePreset()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(new EqualizerPreset("Vocal", new double[10]));

            Assert.NotNull(store.Find("vocal"));
            Assert.Null(store.Find("missing"));
            Assert.Null(store.Find(""));
        }

        /// <summary>Verifies that saving over a name replaces the gains instead of adding a copy.</summary>
        [Fact]
        public void AddOrReplace_WithKnownName_ReplacesTheGains()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(new EqualizerPreset("Flat", new double[10]));
            store.AddOrReplace(new EqualizerPreset("Flat", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }));

            Assert.Single(store.Presets);
            Assert.Equal(1.0, store.Find("Flat")!.Gains[0]);
        }

        /// <summary>Verifies that the stored preset is a copy, so later edits do not reach it.</summary>
        [Fact]
        public void AddOrReplace_ThenEditingSource_DoesNotChangeStoredPreset()
        {
            var store = new EqualizerPresetStore(_filePath);
            var preset = new EqualizerPreset("Live", new double[10]);
            store.AddOrReplace(preset);

            preset.Gains[0] = 9;

            Assert.Equal(0.0, store.Find("Live")!.Gains[0]);
        }

        /// <summary>Verifies removal.</summary>
        [Fact]
        public void Remove_WithKnownName_DropsThePreset()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(EqualizerPreset.Flat("Studio"));

            Assert.True(store.Remove("studio"));
            Assert.Empty(store.Presets);
            Assert.False(store.Remove("studio"));
        }

        /// <summary>Verifies the names the picker shows.</summary>
        [Fact]
        public void Names_WithSeveralPresets_KeepTheOrderTheyWereSavedIn()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(EqualizerPreset.Flat("One"));
            store.AddOrReplace(EqualizerPreset.Flat("Two"));

            Assert.Equal(new[] { "One", "Two" }, store.Names);
        }

        /// <summary>Verifies that a flat preset has one zero per band.</summary>
        [Fact]
        public void Flat_Always_HasOneZeroPerBand()
        {
            EqualizerPreset preset = EqualizerPreset.Flat("Flat");

            Assert.Equal(10, preset.Gains.Count);
            Assert.All(preset.Gains, gain => Assert.Equal(0.0, gain));
        }

        #endregion
    }
}
