using NAudio.Wave;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Test double that plays back a fixed block of samples once.</summary>
    internal sealed class FakeSampleProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _position;

        /// <summary>Creates a source over the given samples.</summary>
        public FakeSampleProvider(float[] samples, int sampleRate = 44100, int channels = 2)
        {
            _samples = samples;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
        }

        /// <summary>Gets the format reported to the effect chain.</summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>Copies as many remaining samples as the buffer accepts.</summary>
        public int Read(Span<float> buffer)
        {
            int count = Math.Min(_samples.Length - _position, buffer.Length);
            _samples.AsSpan(_position, count).CopyTo(buffer);
            _position += count;
            return count;
        }
    }
}
