using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAudio.Effects;
using NAudio.Wave;
using Tune.Helpers;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// One effect of the rack: its generic parameters, the bypass and mix every
    /// <see cref="AudioEffect"/> provides, and the commands that reorder or drop it.
    /// </summary>
    public sealed class EffectSlot : ObservableObject
    {
        private readonly AudioEffect? _effect;
        private bool _bypass;

        /// <summary>
        /// Initializes a slot for one effect of the rack.
        /// </summary>
        /// <param name="name">The label shown on the node.</param>
        /// <param name="effect">The effect the slot wraps.</param>
        /// <param name="remove">Removes the slot from the chain.</param>
        /// <param name="moveUp">Moves the slot one place earlier.</param>
        /// <param name="moveDown">Moves the slot one place later.</param>
        public EffectSlot(
            string name,
            IAudioEffect effect,
            Action<EffectSlot> remove,
            Action<EffectSlot> moveUp,
            Action<EffectSlot> moveDown)
        {
            Name = name;
            Effect = effect;
            _effect = effect as AudioEffect;

            var parameters = new List<EffectParameterSlot>();
            if (effect is IParameterized parameterized)
            {
                foreach (EffectParameter parameter in parameterized.Parameters)
                {
                    var editor = new EffectParameterSlot(parameter);
                    editor.PropertyChanged += OnParameterChanged;
                    parameters.Add(editor);
                }
            }

            Parameters = parameters;

            RemoveCommand = new RelayCommand(() => remove(this));
            MoveUpCommand = new RelayCommand(() => moveUp(this));
            MoveDownCommand = new RelayCommand(() => moveDown(this));
        }

        /// <summary>
        /// Gets the label shown on the node.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the effect the slot wraps.
        /// </summary>
        public IAudioEffect Effect { get; }

        /// <summary>
        /// Gets the parameters the effect exposes, in the order the effect declares them.
        /// </summary>
        public IReadOnlyList<EffectParameterSlot> Parameters { get; }

        /// <summary>
        /// Gets a value indicating whether the effect can be mixed dry against wet.
        /// </summary>
        public bool HasMix => _effect != null;

        /// <summary>
        /// Gets a value indicating whether the effect accepts the loaded format.
        /// An unsupported slot stays out of the audio path.
        /// </summary>
        public bool IsSupported { get; private set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the user switched the effect off.
        /// </summary>
        public bool Bypass
        {
            get => _bypass;
            set
            {
                if (SetProperty(ref _bypass, value))
                    ApplyBypass(suppressed: false);
            }
        }

        /// <summary>
        /// Gets or sets the dry/wet mix of the effect.
        /// </summary>
        public double Mix
        {
            get => _effect?.Mix ?? 1.0;
            set
            {
                if (_effect == null)
                    return;

                _effect.Mix = (float)value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the one line summary shown under the node name.
        /// </summary>
        public string Subtitle
        {
            get
            {
                foreach (EffectParameterSlot parameter in Parameters)
                {
                    if (parameter.ShowMeter)
                        continue;

                    string value = parameter.DisplayValue;
                    return string.IsNullOrEmpty(value) ? parameter.Name : $"{parameter.Name} {value}";
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the command that drops the effect from the chain.
        /// </summary>
        public IRelayCommand RemoveCommand { get; }

        /// <summary>
        /// Gets the command that moves the effect one place earlier.
        /// </summary>
        public IRelayCommand MoveUpCommand { get; }

        /// <summary>
        /// Gets the command that moves the effect one place later.
        /// </summary>
        public IRelayCommand MoveDownCommand { get; }

        /// <summary>
        /// Tells the effect which format the audio has.
        /// An effect that refuses the format is bypassed and kept out of the path,
        /// so one unsupported effect cannot break playback.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        public void Configure(WaveFormat format)
        {
            try
            {
                Effect.Configure(format);
                IsSupported = true;
            }
            catch (Exception ex)
            {
                IsSupported = false;
                LoggerHelper.LogError($"Effect '{Name}' cannot process '{format}'", ex, nameof(Configure));
            }

            ApplyBypass(suppressed: false);
        }

        /// <summary>
        /// Re-applies the effective bypass, so the chain can switch every effect off
        /// without losing the user's own choice for this node.
        /// </summary>
        /// <param name="suppressed">True while the chain master switch is off.</param>
        public void ApplyBypass(bool suppressed)
        {
            if (_effect != null)
                _effect.Bypass = suppressed || _bypass || !IsSupported;

            OnPropertyChanged(nameof(Bypass));
        }

        /// <summary>
        /// Pushes meter values to the UI. Called on a timer by the page.
        /// </summary>
        public void RefreshMeters()
        {
            foreach (EffectParameterSlot parameter in Parameters)
            {
                if (parameter.ShowMeter)
                    parameter.Refresh();
            }
        }

        private void OnParameterChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Subtitle));
        }
    }
}
