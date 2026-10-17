using System;

using NAudio.Wave;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// Keeps every sample under a ceiling so the output never runs past full scale. It is a hard
    /// limiter with no look-ahead, which is enough to stop the level from overshooting; it is not
    /// a mastering limiter and will round off the loudest peaks.
    /// </summary>
    public sealed class OutputLimiterProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;

        private float _ceiling = 1f;

        /// <param name="source">The provider to limit; it is read through and clamped.</param>
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
        /// Gets or sets the ceiling in decibels below full scale. Zero means full scale, so nothing
        /// is touched; anything lower clamps the samples.
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

            for (int i = 0; i < read; i++)
            {
                if (buffer[i] > _ceiling) buffer[i] = _ceiling;
                else if (buffer[i] < -_ceiling) buffer[i] = -_ceiling;
            }

            return read;
        }

        /// <summary>Reads a slice of an array, which is the form older callers use.</summary>
        public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
    }
}
