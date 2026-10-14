using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Effects;
using NAudio.Wave;
using Tune.Helpers;
using Tune.Helpers.FormatConverters;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The three band equalizer of the chain.
    /// It is a fixed processor rather than a rack entry: the low, mid and high
    /// parameters are its own bands, so the rack can never reshape the knobs the
    /// main page binds to.
    /// </summary>
    public sealed class EqualizerUnit : ObservableObject
    {
        private const double LowFrequency = 120.0;
        private const double MidFrequency = 1000.0;
        private const double HighFrequency = 6000.0;
        private const float MidQuality = 0.7f;
        private const float ShelfSlope = 1.0f;
        private const double MaxBandDecibels = 12.0;
        private const double MaxTrimDecibels = 12.0;

        private readonly List<IAudioEffectParameter> _parameters = new List<IAudioEffectParameter>();
        private readonly List<AudioEffect> _effects = new List<AudioEffect>();
        private readonly EqualizerBand _lowBand;
        private readonly EqualizerBand _midBand;
        private readonly EqualizerBand _highBand;
        private readonly Equalizer _equalizer;
        private readonly GainEffect _trim;

        private bool _isEnabled = true;
        private bool _isConfigured;

        /// <summary>
        /// Initializes the unit with its four parameters and restores them from the store.
        /// </summary>
        /// <param name="store">The store the parameters are read from and written to.</param>
        public EqualizerUnit(IAudioEffectParameterStore store)
        {
            Bass = new AudioEffectParameter(store, "Bass", "BASS", -50, 50, 0);
            Mid = new AudioEffectParameter(store, "Mid", "MID", -50, 50, 0);
            Treble = new AudioEffectParameter(store, "Treble", "TREBLE", -50, 50, 0);
            Trim = new AudioEffectParameter(store, "EqTrim", "EQ", -50, 50, 0);

            _lowBand = EqualizerBand.LowShelf((float)LowFrequency, ShelfSlope, 0);
            _midBand = EqualizerBand.Peaking((float)MidFrequency, MidQuality, 0);
            _highBand = EqualizerBand.HighShelf((float)HighFrequency, ShelfSlope, 0);
            _equalizer = new Equalizer(new[] { _lowBand, _midBand, _highBand });
            _trim = new GainEffect();

            _parameters.Add(Bass);
            _parameters.Add(Mid);
            _parameters.Add(Treble);
            _parameters.Add(Trim);

            _effects.Add(_equalizer);
            _effects.Add(_trim);

            Bass.PropertyChanged += OnBandChanged;
            Mid.PropertyChanged += OnBandChanged;
            Treble.PropertyChanged += OnBandChanged;
            Trim.PropertyChanged += OnTrimChanged;

            ApplyBands();
            ApplyTrim();
        }

        /// <summary>
        /// Gets the three bands and the output trim, in the order the UI shows them.
        /// </summary>
        public IReadOnlyList<IAudioEffectParameter> Parameters => _parameters;

        /// <summary>
        /// Gets the audio effects of the unit, in processing order.
        /// </summary>
        public IReadOnlyList<AudioEffect> Effects => _effects;

        /// <summary>
        /// Gets a value indicating whether the unit accepts the loaded format.
        /// An unsupported unit stays out of the audio path.
        /// </summary>
        public bool IsSupported { get; private set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the equalizer is applied.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (SetProperty(ref _isEnabled, value))
                    ApplyBypass();
            }
        }

        /// <summary>
        /// Gets the low shelf amount, on the -50..50 UI scale.
        /// </summary>
        public IAudioEffectParameter Bass { get; }

        /// <summary>
        /// Gets the mid bell amount, on the -50..50 UI scale.
        /// </summary>
        public IAudioEffectParameter Mid { get; }

        /// <summary>
        /// Gets the high shelf amount, on the -50..50 UI scale.
        /// </summary>
        public IAudioEffectParameter Treble { get; }

        /// <summary>
        /// Gets the output trim of the whole equalizer, on the -50..50 UI scale.
        /// </summary>
        public IAudioEffectParameter Trim { get; }

        /// <summary>
        /// Tells the equalizer which format the audio has, and applies the bands.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        public void Configure(WaveFormat format)
        {
            try
            {
                foreach (AudioEffect effect in _effects)
                    effect.Configure(format);

                IsSupported = true;
            }
            catch (Exception ex)
            {
                IsSupported = false;
                LoggerHelper.LogError($"The equalizer cannot process '{format}'", ex, nameof(Configure));
            }

            ApplyBypass();

            // The bands can only be applied once the sample rate is known.
            _isConfigured = true;
            ApplyBands();
        }

        /// <summary>
        /// Clears the filter state, so a jump in playback does not ring.
        /// </summary>
        public void Reset()
        {
            foreach (AudioEffect effect in _effects)
                effect.Reset();
        }

        private void OnBandChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioEffectParameter.Value))
                ApplyBands();
        }

        private void OnTrimChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioEffectParameter.Value))
                ApplyTrim();
        }

        /// <summary>
        /// Pushes the three band amounts into the equalizer and re-applies them.
        /// The framework crossfades from the old response, so retuning does not click.
        /// </summary>
        private void ApplyBands()
        {
            _lowBand.GainDb = ToDecibels(Bass, MaxBandDecibels);
            _midBand.GainDb = ToDecibels(Mid, MaxBandDecibels);
            _highBand.GainDb = ToDecibels(Treble, MaxBandDecibels);

            if (_isConfigured)
                _equalizer.Update();
        }

        private void ApplyTrim()
        {
            _trim.GainDb = ToDecibels(Trim, MaxTrimDecibels);
        }

        private void ApplyBypass()
        {
            bool bypass = !_isEnabled || !IsSupported;

            foreach (AudioEffect effect in _effects)
                effect.Bypass = bypass;
        }

        private static float ToDecibels(IAudioEffectParameter parameter, double maxDecibels)
        {
            return (float)(UIMixToDoubleMix.Convert(parameter.Value) * maxDecibels);
        }
    }
}
