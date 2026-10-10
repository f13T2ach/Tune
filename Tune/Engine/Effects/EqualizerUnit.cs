using System.ComponentModel;
using NAudio.Effects;
using Tune.Helpers.FormatConverters;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The three band equalizer of the chain.
    /// The low, mid and high parameters are the bands of this equalizer itself,
    /// so they shape the tone instead of stacking a second tone control on top of it.
    /// </summary>
    public sealed class EqualizerUnit : AudioEffectUnit
    {
        private const double LowFrequency = 120.0;
        private const double MidFrequency = 1000.0;
        private const double HighFrequency = 6000.0;
        private const float MidQuality = 0.7f;
        private const float ShelfSlope = 1.0f;
        private const double MaxBandDecibels = 12.0;
        private const double MaxTrimDecibels = 12.0;

        private readonly EqualizerBand _lowBand;
        private readonly EqualizerBand _midBand;
        private readonly EqualizerBand _highBand;
        private readonly Equalizer _equalizer;
        private readonly GainEffect _trim;
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

            AddParameter(Bass);
            AddParameter(Mid);
            AddParameter(Treble);
            AddParameter(Trim);

            AddEffect(_equalizer);
            AddEffect(_trim);

            Bass.PropertyChanged += OnBandChanged;
            Mid.PropertyChanged += OnBandChanged;
            Treble.PropertyChanged += OnBandChanged;
            Trim.PropertyChanged += OnTrimChanged;

            ApplyBands();
            ApplyTrim();
        }

        /// <inheritdoc />
        public override string Id => "equalizer";

        /// <inheritdoc />
        public override string DisplayName => "EQ";

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

        /// <inheritdoc />
        protected override void OnConfigured()
        {
            // The bands can only be applied once the sample rate is known.
            _isConfigured = true;
            ApplyBands();
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

        private static float ToDecibels(IAudioEffectParameter parameter, double maxDecibels)
        {
            return (float)(UIMixToDoubleMix.Convert(parameter.Value) * maxDecibels);
        }
    }
}
