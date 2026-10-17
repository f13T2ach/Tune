using System;

using NAudio.Wave;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// Keeps the output under a ceiling by turning the level down rather than by flattening the
    /// waveform. The gain drops at once when a frame is too loud and comes back slowly afterwards,
    /// so the shape of the sound is kept and only its level is held.
    /// </summary>
    /// <remarks>
    /// This is still a limiter without look-ahead, so a very fast transient is held back slightly
    /// late; what it avoids is the hard clipping that turns loud passages into buzz.
    /// </remarks>
    public sealed class OutputLimiterProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;

        private float _ceiling = 1f;
        private double _gain = 1;

        /// <param name="source">The provider to limit; it is read through.</param>
        public OutputLimiterProvider(ISampleProvider source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        /// <inheritdoc/>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Gets or sets a value indicating whether the output is limited. It is on by default, and
        /// at a ceiling of zero it lets everything through.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets how long the gain takes to come back after it was turned down, in seconds.
        /// Longer is smoother but pumps; shorter is punchier but more audible.
        /// </summary>
        public double ReleaseSeconds { get; set; } = 0.2;

        /// <summary>
        /// Gets or sets the ceiling in decibels below full scale. Zero means full scale, so nothing
        /// is touched; anything lower turns the level down whenever it would pass the ceiling.
        /// </summary>
        public double CeilingDecibels
        {
            get => 20 * Math.Log10(_ceiling);
            set => _ceiling = (float)Math.Pow(10, Math.Min(0, value) / 20);
        }

        /// <inheritdoc/>
        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            if (!IsEnabled || read <= 0)
                return read;

            int channels = Math.Max(1, WaveFormat.Channels);
            double release = Math.Exp(-1.0 / (Math.Max(0.01, ReleaseSeconds) * WaveFormat.SampleRate));

            for (int i = 0; i < read; i += channels)
            {
                // One gain for the whole frame, so the stereo image is left alone.
                double peak = 0;

                for (int c = 0; c < channels && i + c < read; c++)
                    peak = Math.Max(peak, Math.Abs(buffer[i + c]));

                double target = peak <= _ceiling ? 1.0 : _ceiling / peak;

                // Attack is immediate, release is exponential, and both are applied before the
                // samples are written, so the level never jumps between them.
                _gain = target < _gain ? target : target + (_gain - target) * release;

                for (int c = 0; c < channels && i + c < read; c++)
                    buffer[i + c] = (float)(buffer[i + c] * _gain);
            }

            return read;
        }

        /// <summary>Reads a slice of an array, which is the form older callers use.</summary>
        public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    }
}