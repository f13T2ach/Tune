using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using Tune.Appearance;
using Tune.Assets.Localization;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Engine;
using Tune.Helpers.Localization;
using Tune.Local;
using Tune.Models.Messages;
using Tune.Models.ViewModelImpl;
using Tune.Values.Static;

namespace Tune.ViewModel
{
    /// <summary>
    /// Backs the settings page: categories on the left, the settings of the selected
    /// category on the right, and a save button that writes the settings to disk.
    /// </summary>
    public partial class SettingsPageViewModel : ObservableObject
    {
        /// <summary>Stored AudioApi value that selects the WASAPI device.</summary>
        private const string WasapiApiName = "wasapi";

        /// <summary>Language as it is stored on disk, used to tell whether there are changes.</summary>
        private string _savedLanguage = string.Empty;

        /// <summary>Audio API as it is stored on disk, used to tell whether there are changes.</summary>
        private string _savedAudioApi = string.Empty;

        /// <summary>Equalizer band minimum as it is stored on disk.</summary>
        private string _savedBandMinimum = string.Empty;

        /// <summary>Equalizer band maximum as it is stored on disk.</summary>
        private string _savedBandMaximum = string.Empty;

        /// <summary>Equalizer band step as it is stored on disk.</summary>
        private string _savedBandStep = string.Empty;

        // ==== Categories ====

        public ObservableCollection<string> Categories { get; } = new()
        {
            LangStrings.CategoryGeneral,
            LangStrings.CategoryAudio,
            LangStrings.CategoryEffect,
            LangStrings.CategoryAbout
        };

        [ObservableProperty]
        private int _selectedCategoryIndex;

        public bool IsGeneralSelected => SelectedCategoryIndex == 0;
        public bool IsAudioSelected => SelectedCategoryIndex == 1;
        public bool IsEffectSelected => SelectedCategoryIndex == 2;
        public bool IsAboutSelected => SelectedCategoryIndex == 3;

        // ==== General settings ====

        public ObservableCollection<LanguageOption> Languages { get; } = new()
        {
            new LanguageOption(LocalizationManager.SystemLanguage, LangStrings.LanguageSystem),
            new LanguageOption("zh-CN", "中文"),
            new LanguageOption("en", "English")
        };

        [ObservableProperty]
        private LanguageOption _selectedLanguage;

        // ==== Audio settings ====

        public ObservableCollection<AudioApiOption> AudioApis { get; } = new()
        {
            new AudioApiOption(WasapiApiName, "WASAPI"),
            new AudioApiOption(AudioPlayerEngine.WaveOutApiName, "WaveOut")
        };

        [ObservableProperty]
        private AudioApiOption _selectedAudioApi;

        // ==== Effect settings ====

        /// <summary>Gets or sets the smallest gain an equalizer band accepts, as typed.</summary>
        [ObservableProperty]
        private string _bandMinimumText = string.Empty;

        /// <summary>Gets or sets the largest gain an equalizer band accepts, as typed.</summary>
        [ObservableProperty]
        private string _bandMaximumText = string.Empty;

        /// <summary>Gets or sets the gain an equalizer band moves by, as typed.</summary>
        [ObservableProperty]
        private string _bandStepText = string.Empty;

        // ==== Save state ====

        [ObservableProperty]
        private string _saveHint = string.Empty;

        public bool IsSaveDisabled => !HasChanges();

        // ==== About ====

        public string AppName => Strings.APPLICATION_NAME;

        public string VersionText =>
            Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? Strings.NO_CONTENT;

        public string RuntimeText =>
            System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

        // ==== Constructor ====

        public SettingsPageViewModel(
            PlayerController playerController,
            MusicPlaybackController playback,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric)
        {
            // Show the stored values, falling back to the first entry when one is unknown.
            _savedLanguage = TuneSettings.Default.Language;
            _savedAudioApi = TuneSettings.Default.AudioApi;

            _selectedLanguage = Languages.FirstOrDefault(option =>
                option.Value.Equals(_savedLanguage, StringComparison.OrdinalIgnoreCase))
                ?? Languages[0];

            _selectedAudioApi = AudioApis.FirstOrDefault(option =>
                option.Value.Equals(_savedAudioApi, StringComparison.OrdinalIgnoreCase))
                ?? AudioApis[0];

            // Numbers are shown in the invariant form they are stored in.
            _savedBandMinimum = BandMinimumText = Format(TuneSettings.Default.EffectEqBandMinimum);
            _savedBandMaximum = BandMaximumText = Format(TuneSettings.Default.EffectEqBandMaximum);
            _savedBandStep = BandStepText = Format(TuneSettings.Default.EffectEqBandStep);
        }

        /// <summary>Formats a stored number so it can be typed back unchanged.</summary>
        private static string Format(double value) =>
            value.ToString("0.###", CultureInfo.InvariantCulture);

        /// <summary>Reads a number the user typed, in either separator style.</summary>
        private static bool TryRead(string? text, out double value) =>
            double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        /// <summary>True when what was typed no longer matches what is stored.</summary>
        private static bool TextDiffers(string? text, string saved) =>
            !TryRead(text, out double typed) ||
            !TryRead(saved, out double stored) ||
            Math.Abs(typed - stored) > 1e-9;

        partial void OnSelectedCategoryIndexChanged(int value)
        {
            OnPropertyChanged(nameof(IsGeneralSelected));
            OnPropertyChanged(nameof(IsAudioSelected));
            OnPropertyChanged(nameof(IsEffectSelected));
            OnPropertyChanged(nameof(IsAboutSelected));
        }

        partial void OnBandMinimumTextChanged(string value) => RefreshSaveState();

        partial void OnBandMaximumTextChanged(string value) => RefreshSaveState();

        partial void OnBandStepTextChanged(string value) => RefreshSaveState();

        partial void OnSelectedLanguageChanged(LanguageOption value) => RefreshSaveState();

        partial void OnSelectedAudioApiChanged(AudioApiOption value) => RefreshSaveState();

        private bool HasChanges() =>
            !string.Equals(_savedLanguage, SelectedLanguage.Value, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(_savedAudioApi, SelectedAudioApi.Value, StringComparison.OrdinalIgnoreCase) ||
            TextDiffers(BandMinimumText, _savedBandMinimum) ||
            TextDiffers(BandMaximumText, _savedBandMaximum) ||
            TextDiffers(BandStepText, _savedBandStep);

        private void RefreshSaveState()
        {
            if (HasChanges())
            {
                // A new change makes an earlier confirmation stale.
                SaveHint = string.Empty;
            }

            OnPropertyChanged(nameof(IsSaveDisabled));
            SaveCommand.NotifyCanExecuteChanged();
        }

        // ==== Commands for UI Binding ====

        [RelayCommand(CanExecute = nameof(HasChanges))]
        private void Save()
        {
            TuneSettings.Default.Language = SelectedLanguage.Value;
            TuneSettings.Default.AudioApi = SelectedAudioApi.Value;

            // An unusable range is ignored, the stored one stays in force.
            if (TryRead(BandMinimumText, out double minimum) &&
                TryRead(BandMaximumText, out double maximum) &&
                TryRead(BandStepText, out double step) &&
                minimum < maximum && step > 0)
            {
                TuneSettings.Default.EffectEqBandMinimum = minimum;
                TuneSettings.Default.EffectEqBandMaximum = maximum;
                TuneSettings.Default.EffectEqBandStep = step;
            }

            TuneSettings.Default.Save();

            _savedLanguage = SelectedLanguage.Value;
            _savedAudioApi = SelectedAudioApi.Value;
            _savedBandMinimum = BandMinimumText = Format(TuneSettings.Default.EffectEqBandMinimum);
            _savedBandMaximum = BandMaximumText = Format(TuneSettings.Default.EffectEqBandMaximum);
            _savedBandStep = BandStepText = Format(TuneSettings.Default.EffectEqBandStep);

            SaveHint = LangStrings.Saved;
            RefreshSaveState();
        }

        [RelayCommand]
        private void NavigateToMain() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Main)));
    }

    /// <summary>One selectable language.</summary>
    public sealed record LanguageOption(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    /// <summary>One selectable audio output API.</summary>
    public sealed record AudioApiOption(string Value, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }
}