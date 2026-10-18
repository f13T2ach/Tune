using System.Collections.Generic;
using System;
using System.Linq;

using NAudio.Wave;
using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies that the metering provider actually hands spectrum bands to its listeners.</summary>
    public class MeteringBandsTests
    {
        /// <summary>Verifies that a tone reaches the event as a loud band.</summary>
        [Fact]
        public void Read_WithATone_ReportsBandsWithAPeak()
        {
            float[] samples = Sine(1000, 0.5, 44100 * 2);
            var meter = new MeteringSampleProvider(new Block(samples));

            IReadOnlyList<float>? bands = null;
            ReadOnlyMemory<float> frames = ReadOnlyMemory<float>.Empty;
            int sampleRate = 0;

            meter.LevelReported += (_, e) =>
            {
                bands = e.Bands;
                frames = e.Frames;
                sampleRate = e.SampleRate;
            };

            while (meter.Read(new float[4096], 0, 4096) > 0) { }

            Assert.NotNull(bands);
            Assert.True(bands!.Count > 0, "no bands were reported");

            float loudest = bands.Max();

            Assert.True(loudest > SpectrumAnalyzer.FloorDecibels + 15, $"loudest band {loudest:0.0} dB");

            // The same event has to carry what a scope or a spectrogram needs.
            Assert.True(frames.Length > 0, "no frames were reported");
            Assert.Equal(44100, sampleRate);
        }

        private static float[] Sine(double frequency, double amplitude, int count)
        {
            var buffer = new float[count];

            for (int i = 0; i < count; i++)
                buffer[i] = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * i / 44100));

            return buffer;
        }

        /// <summary>A provider that hands out one long block and the given samples.</summary>
        private sealed class Block(float[] samples) : ISampleProvider
        {
            private bool _read;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 1);

            public int Read(Span<float> buffer)
            {
                if (_read) return 0;

                _read = true;
                int count = Math.Min(samples.Length, buffer.Length);
                samples.AsSpan(0, count).CopyTo(buffer);

                return count;
            }

            public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        }
    }
}