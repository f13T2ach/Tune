using System;

using NAudio.Wave;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// Holds the signal under a ceiling on its way to the device. The chain adds gain, so a loud
    /// patch would clip; this turns the peaks down instead of letting them overrun. It sits
    /// outside the whole chain, so both the effects and the volume fader are covered.
    /// </summary>
    internal sealed class LimitingSampleProvider : ISampleProvider
    {
        /// <summary>The peak the signal is held under, a little below full scale.</summary>
        internal const float Ceiling = 0.97f;

        /// <summary>How fast the reduction lets go, per block; roughly 23 ms of recovery.</summary>
        private const float Release = 0.001f;

        private readonly ISampleProvider _source;
        private float _reduction = 1f;

        /// <summary>
        /// Initializes the limiter for one loaded file.
        /// </summary>
        /// <param name="source">The sample source to read from and protect.</param>
        public LimitingSampleProvider(ISampleProvider source)
        {
            _source = source;
        }

        /// <inheritdoc />
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <inheritdoc />
        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            if (read <= 0)
                return read;

            Span<float> block = buffer[..read];

            // Indexed loop on purpose: this runs on the audio thread and must not allocate.
            float peak = 0f;
            for (int i = 0; i < block.Length; i++)
            {
                float magnitude = Math.Abs(block[i]);
                if (magnitude > peak) peak = magnitude;
            }

            // A peak that is too high is turned down at once. The level is let back up slowly, so
            // it does not jump about from one block to the next.
            float wanted = peak > Ceiling ? Ceiling / peak : 1f;
            _reduction = wanted < _reduction
                ? wanted
                : _reduction + ((wanted - _reduction) * Release);

            for (int i = 0; i < block.Length; i++)
                block[i] *= _reduction;

            return read;
        }
    }
}
