using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Effects;
using NAudio.Wave;
using Tune.Helpers;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The ten band graphic equalizer of the chain: one peaking filter per octave
    /// between 31.5 Hz and 16 kHz. Like the three band one it is a fixed processor
    /// rather than a rack entry, so a loaded preset can never be reshaped by the rack.
    /// </summary>
    public sealed class TenBandEqualizerUnit : ObservableObject
    {
        /// <summary>Centre frequencies of the ten bands, in the order the UI shows them.</summary>
        public static readonly double[] BandFrequencies =
        {
            31.5, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000
        };

        /// <summary>Width of every band, the usual value for an octave equalizer.</summary>
        private const float BandQuality = 1.4f;

        private readonly List<TenBandEqualizerBand> _bands = new List<TenBandEqualizerBand>();
        private readonly EqualizerBand[] _filters;
        private readonly Equalizer _equalizer;
        private readonly AudioEffect[] _effects;

        private bool _isConfigured;
        private bool _isEnabled = true;

        /// <summary>Creates the unit with ten flat bands.</summary>
        public TenBandEqualizerUnit()
        {
            _filters = new EqualizerBand[BandFrequencies.Length];
            for (int i = 0; i < BandFrequencies.Length; i++)
            {
                var band = new TenBandEqualizerBand(BandFrequencies[i]);
                band.GainChanged += OnBandGainChanged;
                _bands.Add(band);

                _filters[i] = EqualizerBand.Peaking((float)BandFrequencies[i], BandQuality, 0);
            }

            _equalizer = new Equalizer(_filters);
            _effects = new AudioEffect[] { _equalizer };
        }

        /// <summary>Gets the ten bands, in the order the UI shows them.</summary>
        public IReadOnlyList<TenBandEqualizerBand> Bands => _bands;

        /// <summary>Gets the audio effects of the unit, in processing order.</summary>
        public IReadOnlyList<AudioEffect> Effects => _effects;

        /// <summary>
        /// Gets a value indicating whether the unit accepts the loaded format.
        /// An unsupported unit stays out of the audio path.
        /// </summary>
        public bool IsSupported { get; private set; } = true;

        /// <summary>Gets the largest gain of a single band, in decibels.</summary>
        public double MaxDecibels { get; } = 12.0;

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

        /// <summary>Reads the ten gains, in the order the UI shows them.</summary>
        public double[] CaptureGains()
        {
            var gains = new double[_bands.Count];
            for (int i = 0; i < _bands.Count; i++)
                gains[i] = _bands[i].Gain;

            return gains;
        }

        /// <summary>Applies ten gains at once, which is how a preset is loaded.</summary>
        /// <param name="gains">The gains; missing entries keep the band where it is.</param>
        public void ApplyGains(IReadOnlyList<double>? gains)
        {
            if (gains == null) return;

            for (int i = 0; i < _bands.Count && i < gains.Count; i++)
                _bands[i].Gain = gains[i];
        }

        /// <summary>Tells the equalizer which format the audio has.</summary>
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
                LoggerHelper.LogError($"The ten band equalizer cannot process '{format}'", ex, nameof(Configure));
            }

            ApplyBypass();

            // The filters can only be applied once the sample rate is known.
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

        private void OnBandGainChanged(object? sender, PropertyChangedEventArgs e)
        {
            ApplyBands();
        }

        /// <summary>
        /// Pushes the ten gains into the equalizer and re-applies them.
        /// The framework crossfades from the old response, so retuning does not click.
        /// </summary>
        private void ApplyBands()
        {
            for (int i = 0; i < _bands.Count; i++)
                _filters[i].GainDb = (float)Math.Clamp(_bands[i].Gain, -MaxDecibels, MaxDecibels);

            if (_isConfigured)
                _equalizer.Update();
        }

        private void ApplyBypass()
        {
            bool bypass = !_isEnabled || !IsSupported;

            foreach (AudioEffect effect in _effects)
                effect.Bypass = bypass;
        }
    }

    /// <summary>One band of the ten band equalizer: a centre frequency and a gain.</summary>
    public sealed partial class TenBandEqualizerBand : ObservableObject
    {
        [ObservableProperty]
        private double _gain;

        /// <summary>Creates a band at the given centre frequency.</summary>
        /// <param name="frequency">Centre frequency of the band, in hertz.</param>
        public TenBandEqualizerBand(double frequency)
        {
            Frequency = frequency;
        }

        /// <summary>Gets the centre frequency of the band, in hertz.</summary>
        public double Frequency { get; }

        /// <summary>Raised when the gain changed and the filter has to be retuned.</summary>
        public event EventHandler<PropertyChangedEventArgs>? GainChanged;

        partial void OnGainChanged(double value) =>
            GainChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Gain)));
    }
}
