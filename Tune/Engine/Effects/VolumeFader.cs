using System.ComponentModel;
using NAudio.Effects;
using NAudio.Wave;
using Tune.Helpers.FormatConverters;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The output level of the chain.
    /// It is deliberately not an effect unit: it always runs last and is never
    /// bypassed, so switching the effects off keeps the volume control working.
    /// </summary>
    internal sealed class VolumeFader
    {
        private readonly IAudioEffectParameter _volume;

        /// <summary>
        /// Initializes the fader and applies the stored volume.
        /// </summary>
        /// <param name="volume">The volume parameter on the 0..100 UI scale.</param>
        public VolumeFader(IAudioEffectParameter volume)
        {
            _volume = volume;
            Gain = new GainEffect();

            volume.PropertyChanged += OnVolumeChanged;

            Apply();
        }

        /// <summary>
        /// Gets the effect that applies the level.
        /// </summary>
        public GainEffect Gain { get; }

        /// <summary>
        /// Tells the fader which format the audio has.
        /// The framework refuses to process an effect that was never configured.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        public void Configure(WaveFormat format)
        {
            Gain.Configure(format);
        }

        private void OnVolumeChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioEffectParameter.Value))
                Apply();
        }

        private void Apply()
        {
            Gain.LinearGain = (float)UIVolumeToDoubleVolume.Convert(_volume.Value);
        }
    }
}
