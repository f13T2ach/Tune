using System.IO;

using Tune.Assets.Localization;
using Tune.Engine.Effects;
using Tune.Helpers.UiState;
using Tune.Persistence.EqualizerStorage;
using Tune.Tests.Engine.Effects;
using Tune.ViewModel;
using Xunit;

namespace Tune.Tests.ViewModel
{
    /// <summary>Verifies the preset flow of <see cref="EqualizerPageViewModel"/>.</summary>
    public class EqualizerPageViewModelTests : IDisposable
    {
        private readonly string _filePath =
            Path.Combine(Path.GetTempPath(), $"tune-eq-{Guid.NewGuid():N}.json");

        /// <summary>Removes the temporary preset file of this test.</summary>
        public void Dispose()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        #region ==== The Empty Entry ====

        /// <summary>
        /// Verifies that the empty entry leaves the page where it starts: on a new
        /// equalizer, flat and with nothing selected.
        /// </summary>
        [Fact]
        public void EmptyEntry_WhenChosen_GoesBackToANewEqualizer()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain);
            view.Bands[2].Gain = 5;

            view.SelectedPresetName = LangStrings.EqEmpty;

            Assert.Equal(LangStrings.EqNewPreset, view.PresetName);
            Assert.All(view.Bands, band => Assert.Equal(0.0, band.Gain));
            Assert.Null(view.SelectedPresetName);
            Assert.False(view.CanRemove);
            Assert.True(view.IsSaveDisabled);
        }

        /// <summary>Verifies that the empty entry asks before it drops unsaved work.</summary>
        [Fact]
        public void EmptyEntry_WithDeclinedPrompt_KeepsTheGainsAndThePicker()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain, new EqualizerPresetStore(_filePath));
            view.PresetName = "Rock";
            view.Bands[1].Gain = 4;
            view.SavePresetCommand.Execute(null);
            Assert.True(view.CanRemove);

            view.Bands[1].Gain = -3;
            view.Prompt = (_, _) => false;
            view.SelectedPresetName = LangStrings.EqEmpty;

            Assert.True(chain.TenBand.IsEnabled);
            Assert.Equal(-3.0, view.Bands[1].Gain);
            Assert.Equal("Rock", view.SelectedPresetName);
        }

        /// <summary>Verifies that loading a preset after the empty entry brings its gains back.</summary>
        [Fact]
        public void LoadingPreset_AfterTheEmptyEntry_BringsTheGainsBack()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain, new EqualizerPresetStore(_filePath));
            view.PresetName = "Vocal";
            view.Bands[4].Gain = 6;
            view.SavePresetCommand.Execute(null);

            view.SelectedPresetName = LangStrings.EqEmpty;
            Assert.Equal(LangStrings.EqNewPreset, view.PresetName);

            view.SelectedPresetName = "Vocal";

            Assert.True(chain.TenBand.IsEnabled);
            Assert.Equal(6.0, view.Bands[4].Gain);
            Assert.True(view.CanRemove);
            Assert.Equal("Vocal", EqualizerScheme.Current.Name);
            Assert.Equal(EqualizerSchemeKind.TenBand, EqualizerScheme.Current.Kind);
        }

        #endregion

        #region ==== Saving ====

        /// <summary>Verifies that a change after saving makes the save button available again.</summary>
        [Fact]
        public void Save_ThenChangingAgain_EnablesSavingAgain()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain, new EqualizerPresetStore(_filePath));
            view.PresetName = "Live";

            view.Bands[0].Gain = 3;
            Assert.False(view.IsSaveDisabled);

            view.SavePresetCommand.Execute(null);
            Assert.True(view.IsSaveDisabled);

            view.Bands[0].Gain = 5;
            Assert.False(view.IsSaveDisabled);
        }

        /// <summary>Verifies that a new equalizer flattens the bands and keeps the stored ones.</summary>
        [Fact]
        public void NewPreset_WithStoredPreset_KeepsItButFlattensTheBands()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain, new EqualizerPresetStore(_filePath));
            view.PresetName = "Warm";
            view.Bands[7].Gain = -5;
            view.SavePresetCommand.Execute(null);

            view.NewPresetCommand.Execute(null);

            Assert.All(view.Bands, band => Assert.Equal(0.0, band.Gain));
            Assert.Contains("Warm", view.PresetNames);
            Assert.False(view.CanRemove);
        }

        /// <summary>
        /// Verifies that saving a preset that was changed does not look like a request to
        /// load something: rebuilding the picker must not be taken for a user's choice.
        /// </summary>
        [Fact]
        public void Save_AfterLoadingAndChanging_AsksToSaveOnly()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain, new EqualizerPresetStore(_filePath));
            view.PresetName = "Rock";
            view.Bands[1].Gain = 4;
            view.SavePresetCommand.Execute(null);

            int prompts = 0;
            view.Prompt = (_, _) => { prompts++; return true; };

            view.Bands[1].Gain = -3;
            view.SavePresetCommand.Execute(null);

            Assert.Equal(1, prompts);                                 // the save question only
            Assert.True(chain.TenBand.IsEnabled);                     // not switched off
            Assert.Equal(-3.0, view.Bands[1].Gain);                   // not flattened
            Assert.Equal("Rock", view.SelectedPresetName);            // picker kept its preset
            Assert.True(view.IsSaveDisabled);
        }

        /// <summary>Verifies that a cleared picker is not the same as the empty entry.</summary>
        [Fact]
        public void ClearingThePicker_DoesNotSwitchTheEqualizerOff()
        {
            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            EqualizerPageViewModel view = Create(chain);
            view.Bands[5].Gain = 4;

            view.SelectedPresetName = null;

            Assert.True(chain.TenBand.IsEnabled);
            Assert.Equal(4.0, view.Bands[5].Gain);
        }

        /// <summary>Verifies that the page comes back on the scheme it was left on.</summary>
        [Fact]
        public void Constructor_AfterASchemeWasUsed_LoadsItAgain()
        {
            var store = new EqualizerPresetStore(_filePath);
            store.AddOrReplace(new EqualizerPreset("Rock", new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }));
            store.Save();

            UiStateStore.Set(EqualizerPageViewModel.SchemeKey, "Rock");
            try
            {
                var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
                EqualizerPageViewModel view = Create(chain, store);

                Assert.Equal("Rock", view.SelectedPresetName);
                Assert.Equal(3.0, view.Bands[2].Gain);
                Assert.True(view.CanRemove);
            }
            finally
            {
                UiStateStore.Set(EqualizerPageViewModel.SchemeKey, string.Empty);
            }
        }

        /// <summary>
        /// Verifies that a chain which already runs a restored scheme does not look like
        /// unsaved work when the page opens, which used to raise a pointless question.
        /// </summary>
        [Fact]
        public void Constructor_AfterTheChainRestoredAScheme_AsksNothing()
        {
            var store = new EqualizerPresetStore(_filePath);
            var preset = new EqualizerPreset("Bass", new double[] { 6, 3, 1, 0, 0, 0, 0, 0, 0, 0 });
            store.AddOrReplace(preset);
            store.Save();

            var chain = new AudioEffectChain(new InMemoryAudioEffectParameterStore());
            chain.TenBand.ApplyGains(preset.Gains);   // what the application does at startup

            UiStateStore.Set(EqualizerPageViewModel.SchemeKey, "Bass");
            int prompts = 0;
            EqualizerPageViewModel.PromptOverride = (_, _) => { prompts++; return true; };

            try
            {
                var view = new EqualizerPageViewModel(chain, store);

                Assert.Equal(0, prompts);
                Assert.True(view.IsSaveDisabled);
                Assert.Equal("Bass", view.SelectedPresetName);
            }
            finally
            {
                EqualizerPageViewModel.PromptOverride = null;
                UiStateStore.Set(EqualizerPageViewModel.SchemeKey, string.Empty);
            }
        }

        #endregion

        /// <summary>Builds a page view model over the given chain, confirming every prompt.</summary>
        private static EqualizerPageViewModel Create(AudioEffectChain chain, EqualizerPresetStore? store = null)
        {
            var view = new EqualizerPageViewModel(chain, store ?? new EqualizerPresetStore());
            view.Prompt = (_, _) => true;

            return view;
        }
    }
}
