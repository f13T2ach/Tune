using NAudio.Effects;
using NAudio.Wave;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// Sample source that pushes every read block through the effects of the chain,
    /// in order. One instance belongs to one loaded file.
    /// </summary>
    internal sealed class EffectChainSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private volatile AudioEffect[] _effects;

        /// <summary>
        /// Initializes the node for one loaded file.
        /// </summary>
        /// <param name="source">The sample source to read from.</param>
        /// <param name="effects">The effects to apply, already in processing order.</param>
        public EffectChainSampleProvider(ISampleProvider source, AudioEffect[] effects)
        {
            _source = source;
            _effects = effects;
        }

        /// <inheritdoc />
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>
        /// Swaps the effect list in one assignment, so the reader always sees a
        /// complete chain and the rack can be edited while audio plays.
        /// </summary>
        /// <param name="effects">The effects to apply, in processing order.</param>
        public void SetEffects(AudioEffect[] effects)
        {
            _effects = effects;
        }

        /// <inheritdoc />
        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            if (read <= 0)
                return read;

            Span<float> block = buffer[..read];
            AudioEffect[] effects = _effects;

            // Indexed loop on purpose: this runs on the audio thread and must not allocate.
            for (int i = 0; i < effects.Length; i++)
                effects[i].Process(block);

            return read;
        }
    }
}
