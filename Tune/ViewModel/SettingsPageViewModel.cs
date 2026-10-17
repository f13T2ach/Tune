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

        /// <summary>Lyric lines per timestamp as stored on disk.</summary>
        private int _savedLineLimit;

        /// <summary>Gets or sets which line is shown under the main lyric: translation or third line.</summary>
        [ObservableProperty]
        private string _selectedSecondaryLyric = TuneSettings.Default.SecondaryLyric;

        /// <summary>Secondary lyric as it is stored on disk, used to tell whether there are changes.</summary>
        private string _savedSecondaryLyric = TuneSettings.Default.SecondaryLyric;

        /// <summary>Gets or sets whether the limiter protects the output from clipping.</summary>
        [ObservableProperty]
        private bool _audioLimiter = TuneSettings.Default.AudioLimiter;

        /// <summary>Limiter state as it is stored on disk, used to tell whether there are changes.</summary>
        private bool _savedAudioLimiter = TuneSettings.Default.AudioLimiter;

        /// <summary>
        /// Gets or sets which secondary lyric option is selected, counted from zero, because a
        /// combo box index is zero based and the stored value is not.
        /// </summary>
        public int SelectedSecondaryLyricIndex
        {
            get => string.Equals(SelectedSecondaryLyric, "third", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            set => SelectedSecondaryLyric = value == 1 ? "third" : "translation";
        }

        /// <summary>The line limit as a combo box index, which counts from zero.</summary>
        public int SelectedLineLimitIndex
        {
            get => SelectedLineLimit - 1;
            set => SelectedLineLimit = value + 1;
        }

        /// <summary>The lyric presenter, so a saved line limit takes effect right away.</summary>
        private readonly MusicLyricPresenter _musicLyric;

        // ==== Categories ====

        public ObservableCollection<string> Categories { get; } = new()
        {
            LangStrings.CategoryGeneral,
            LangStrings.CategoryAudio,
            LangStrings.CategoryEffect,
            LangStrings.CategoryLyric,
            LangStrings.CategoryAbout,
        };

        [ObservableProperty]
        private int _selectedCategoryIndex;

        public bool IsGeneralSelected => SelectedCategoryIndex == 0;
        public bool IsAudioSelected => SelectedCategoryIndex == 1;
        public bool IsEffectSelected => SelectedCategoryIndex == 2;
        public bool IsLyricSelected => SelectedCategoryIndex == 3;
        public bool IsAboutSelected => SelectedCategoryIndex == 4;

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
        private int _selectedLineLimit;

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
            _musicLyric = musicLyric;
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
            _savedLineLimit = SelectedLineLimit = TuneSettings.Default.LyricLineLimit;
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

        partial void OnSelectedLineLimitChanged(int value)

        {

            OnPropertyChanged(nameof(SelectedLineLimitIndex));

        }


        partial void OnSelectedCategoryIndexChanged(int value)

        {
            OnPropertyChanged(nameof(IsGeneralSelected));
            OnPropertyChanged(nameof(IsAudioSelected));
            OnPropertyChanged(nameof(IsEffectSelected));
            OnPropertyChanged(nameof(IsLyricSelected));
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
            TextDiffers(BandStepText, _savedBandStep) ||
            SelectedLineLimit != _savedLineLimit ||
            !string.Equals(SelectedSecondaryLyric, _savedSecondaryLyric, StringComparison.OrdinalIgnoreCase) ||
            AudioLimiter != _savedAudioLimiter;

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

            TuneSettings.Default.LyricLineLimit = SelectedLineLimit;

            TuneSettings.Default.SecondaryLyric = SelectedSecondaryLyric;
            _savedSecondaryLyric = SelectedSecondaryLyric;

            TuneSettings.Default.AudioLimiter = AudioLimiter;
            _savedAudioLimiter = AudioLimiter;
            _musicLyric.SecondaryIsThirdLine =
                string.Equals(SelectedSecondaryLyric, "third", StringComparison.OrdinalIgnoreCase);

            _musicLyric.LineLimit = SelectedLineLimit;

            TuneSettings.Default.Save();



            _savedLanguage = SelectedLanguage.Value;
            _savedAudioApi = SelectedAudioApi.Value;
            _savedBandMinimum = BandMinimumText = Format(TuneSettings.Default.EffectEqBandMinimum);
            _savedBandMaximum = BandMaximumText = Format(TuneSettings.Default.EffectEqBandMaximum);
            _savedBandStep = BandStepText = Format(TuneSettings.Default.EffectEqBandStep);
            _savedLineLimit = SelectedLineLimit = TuneSettings.Default.LyricLineLimit;

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
