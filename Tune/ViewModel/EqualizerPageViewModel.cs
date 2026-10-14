using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Tune.Assets.Localization;
using Tune.Components.UI;
using Tune.Engine.Effects;
using Tune.Helpers.UiState;
using Tune.Persistence.EqualizerStorage;

namespace Tune.ViewModel
{
    /// <summary>
    /// Backs the ten band equalizer page: it shows the ten bands of the chain and
    /// saves, removes and creates the named presets those bands are loaded from.
    /// </summary>
    public partial class EqualizerPageViewModel : ObservableObject
    {
        private readonly TenBandEqualizerUnit _unit;
        private readonly EqualizerPresetStore _store;
        private readonly double[] _flat;

        /// <summary>Gains the page last agreed with the store on.</summary>
        private double[] _baseline;

        /// <summary>Name of the loaded preset, kept apart from the picker's own value.</summary>
        private string? _loadedName;

        /// <summary>Set while the picker is moved by the page instead of by the user.</summary>
        private bool _suppressPresetChange;

        /// <summary>
        /// Initializes the page with the bands of the chain and the stored presets.
        /// </summary>
        /// <param name="chain">The chain that owns the ten band equalizer.</param>
        /// <param name="store">The store the presets are read from and written to.</param>
        public EqualizerPageViewModel(IAudioEffectChain chain, EqualizerPresetStore store)
        {
            _unit = chain.TenBand;
            _store = store;
            _flat = new double[_unit.Bands.Count];

            // The page agrees with whatever the chain is already running: the composition
            // root may have restored a scheme before this page was ever opened.
            _baseline = _unit.CaptureGains();

            PresetNames = new ObservableCollection<string>(_store.Names);
            RefreshPresetNames();

            // Come back on the scheme the user left the page with.
            string lastName = UiStateStore.Get(SchemeKey);
            if (!string.IsNullOrEmpty(lastName))
                SelectedPresetName = lastName;

            foreach (TenBandEqualizerBand band in _unit.Bands)
                band.PropertyChanged += OnBandChanged;
        }

        /// <summary>Gets the ten bands, in the order the chart shows them.</summary>
        public IReadOnlyList<TenBandEqualizerBand> Bands => _unit.Bands;

        /// <summary>Gets the names of the stored presets, in the order they were saved.</summary>
        public ObservableCollection<string> PresetNames { get; }

        /// <summary>Gets the largest gain of a band, used by the faders and the axis.</summary>
        public double MaxGain => _unit.MaxDecibels;

        /// <summary>Gets the smallest gain of a band, used by the faders and the axis.</summary>
        public double MinGain => -_unit.MaxDecibels;

        /// <summary>Entry of the picker that switches the equalizer off.</summary>
        private string EmptyEntry => LangStrings.EqEmpty;

        /// <summary>Interface state key the last used scheme is remembered under.</summary>
        internal const string SchemeKey = "equalizer.scheme";

        /// <summary>
        /// Asks the user to confirm something. A test replaces it, so the whole preset
        /// flow can be driven without opening a modal dialog - including the questions
        /// asked while the page is being built.
        /// </summary>
        internal static Func<string, string, bool>? PromptOverride { get; set; }

        /// <summary>Asks the user to confirm something on behalf of this page.</summary>
        internal Func<string, string, bool> Prompt { get; set; } =
            (title, message) => PromptOverride?.Invoke(title, message) ?? AskUserDialog.Ask(title, message);

        /// <summary>Gets the axis labels of the chart.</summary>
        public string MaxGainText => $"{MaxGain:0} dB";

        /// <summary>Gets the middle axis label of the chart.</summary>
        public string MiddleGainText => "0 dB";

        /// <summary>Gets the bottom axis label of the chart.</summary>
        public string MinGainText => $"{MinGain:0} dB";

        // ==== Name and preset ====

        [ObservableProperty]
        private string? _selectedPresetName;

        [ObservableProperty]
        private string _presetName = LangStrings.EqNewPreset;

        [ObservableProperty]
        private bool _isEditingName;

        [ObservableProperty]
        private string _nameDraft = string.Empty;

        /// <summary>True while the bands differ from the loaded preset.</summary>
        public bool HasChanges => !SameGains(_unit.CaptureGains(), _baseline);

        /// <summary>True while a stored preset is loaded, which is what removing needs.</summary>
        public bool CanRemove =>
            !string.IsNullOrWhiteSpace(SelectedPresetName) &&
            !SelectedPresetName.Equals(EmptyEntry, StringComparison.Ordinal);

        /// <summary>True when saving would write the same bands again.</summary>
        public bool IsSaveDisabled => !HasChanges;

        /// <summary>True when there is no stored preset to remove.</summary>
        public bool IsRemoveDisabled => !CanRemove;

        /// <summary>True while the name is shown as a label, which is the normal state.</summary>
        public bool IsNotEditingName => !IsEditingName;

        // ==== Commands ====

        /// <summary>Turns the title into an input box.</summary>
        [RelayCommand]
        private void BeginEditName()
        {
            NameDraft = PresetName;
            IsEditingName = true;
        }

        /// <summary>Takes the typed name, or keeps the old one when it is empty.</summary>
        [RelayCommand]
        private void ConfirmNameEdit()
        {
            if (!string.IsNullOrWhiteSpace(NameDraft))
                PresetName = NameDraft.Trim();

            IsEditingName = false;
        }

        /// <summary>Leaves the name as it was.</summary>
        [RelayCommand]
        private void CancelNameEdit()
        {
            IsEditingName = false;
        }

        /// <summary>Stores the bands under the current name, after asking the user.</summary>
        [RelayCommand(CanExecute = nameof(HasChanges))]
        private void SavePreset()
        {
            if (!Prompt(LangStrings.EqSave, LangStrings.EqConfirmSave))
                return;

            _store.AddOrReplace(new EqualizerPreset(PresetName, _unit.CaptureGains()));
            _store.Save();

            RefreshPresetNames();
            LoadName(PresetName);
            _loadedName = PresetName;
            AcceptChanges();
        }

        /// <summary>Removes the loaded preset, after asking the user.</summary>
        [RelayCommand(CanExecute = nameof(CanRemove))]
        private void RemovePreset()
        {
            if (string.IsNullOrWhiteSpace(SelectedPresetName)) return;
            if (!Prompt(LangStrings.EqRemove, LangStrings.EqConfirmRemove)) return;

            _store.Remove(SelectedPresetName);
            _store.Save();

            RefreshPresetNames();
            StartNewPreset(askFirst: false);
        }

        /// <summary>Starts a flat equalizer, asking first when there is unsaved work.</summary>
        [RelayCommand]
        private void NewPreset() => StartNewPreset(askFirst: true);

        partial void OnIsEditingNameChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotEditingName));
        }

        // ==== Loading a preset from the picker ====

        partial void OnSelectedPresetNameChanged(string? value)
        {
            if (_suppressPresetChange) return;

            // The empty entry is how the equalizer is switched off. A cleared picker is
            // not that choice, it only means nothing is selected.
            if (value != null && value.Equals(EmptyEntry, StringComparison.Ordinal))
            {
                if (HasChanges && !Prompt(LangStrings.EqLoad, LangStrings.EqConfirmLoad))
                {
                    LoadName(_loadedName);
                    return;
                }

                // No scheme at all is the state the page starts in: editing a new one.
                StartNewPreset(askFirst: false);
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                // The picker was cleared by the page itself, nothing to load.
                RefreshState();
                return;
            }

            EqualizerPreset? preset = _store.Find(value);
            if (preset == null)
            {
                // The picker was cleared by the page itself, nothing to load.
                _loadedName = null;
                RefreshState();
                return;
            }

            if (HasChanges && !Prompt(LangStrings.EqLoad, LangStrings.EqConfirmLoad))
            {
                // Put the picker back on the preset that is still loaded.
                LoadName(_loadedName);
                return;
            }

            _unit.IsEnabled = true;
            _unit.ApplyGains(preset.Gains);
            _loadedName = preset.Name;
            PresetName = preset.Name;
            UiStateStore.Set(SchemeKey, preset.Name);
            EqualizerScheme.Current.Set(EqualizerSchemeKind.TenBand, preset.Name);
            AcceptChanges();
        }

        // ==== Internals ====

        private void StartNewPreset(bool askFirst)
        {
            if (askFirst && HasChanges && !Prompt(LangStrings.EqNew, LangStrings.EqConfirmNew))
                return;

            _unit.IsEnabled = true;
            _unit.ApplyGains(_flat);
            _loadedName = null;
            PresetName = LangStrings.EqNewPreset;
            UiStateStore.Set(SchemeKey, string.Empty);
            EqualizerScheme.Current.Clear();
            LoadName(null);
            AcceptChanges();
        }

        /// <summary>The bands now match what the page shows as saved.</summary>
        private void AcceptChanges()
        {
            _baseline = _unit.CaptureGains();
            RefreshState();
        }

        private void OnBandChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TenBandEqualizerBand.Gain))
                RefreshState();
        }

        private void RefreshPresetNames()
        {
            // Rebuilding the list makes the picker drop its selection and write that
            // back, which must not be mistaken for a choice the user made.
            _suppressPresetChange = true;
            try
            {
                PresetNames.Clear();

                // The empty entry is always first, so the equalizer can be switched off.
                PresetNames.Add(EmptyEntry);
                foreach (string name in _store.Names)
                    PresetNames.Add(name);
            }
            finally
            {
                _suppressPresetChange = false;
            }
        }

        /// <summary>Moves the picker without loading anything.</summary>
        private void LoadName(string? name)
        {
            _suppressPresetChange = true;
            SelectedPresetName = name;
            _suppressPresetChange = false;
        }

        private void RefreshState()
        {
            OnPropertyChanged(nameof(HasChanges));
            OnPropertyChanged(nameof(CanRemove));
            OnPropertyChanged(nameof(IsSaveDisabled));
            OnPropertyChanged(nameof(IsRemoveDisabled));
            SavePresetCommand.NotifyCanExecuteChanged();
            RemovePresetCommand.NotifyCanExecuteChanged();
        }

        private static bool SameGains(IReadOnlyList<double> left, IReadOnlyList<double> right)
        {
            if (left.Count != right.Count) return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (Math.Abs(left[i] - right[i]) > 1e-6) return false;
            }

            return true;
        }
    }
}
