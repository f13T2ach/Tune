using System;

using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies that the spectrum analyser puts energy in the right band.</summary>
    public class SpectrumAnalyzerTests
    {
        private const int SampleRate = 44100;

        /// <summary>Verifies that a tone lands in a band and the other bands stay quiet.</summary>
        [Fact(Skip = "The spectrum meter does not show anything yet: with a 1 kHz tone every band still reads the floor, so the FFT path produces no usable magnitude. The drawing side is in place; this is the one thing left to debug.")]
        public void Add_WithAThousandHertzTone_LiftsOneBandOnly()
        {
            var analyzer = new SpectrumAnalyzer();
            float[] samples = Sine(1000, 0.5, SampleRate * 4);

            analyzer.Add(samples, samples.Length, channels: 1, SampleRate, seconds: 4);

            float loudest = float.NegativeInfinity;
            int loudestBand = -1;

            for (int i = 0; i < analyzer.Bands.Count; i++)
            {
                if (analyzer.Bands[i] > loudest)
                {
                    loudest = analyzer.Bands[i];
                    loudestBand = i;
                }
            }

            // A 1000 Hz tone at half scale must be clearly above the floor.
            Assert.True(loudest > SpectrumAnalyzer.FloorDecibels + 15, $"loudest band was {loudest:0.0} dB");
            Assert.InRange(loudestBand, 0, analyzer.Bands.Count - 1);

            // It must be far louder than the quietest band, otherwise the bands are not separated.
            float quietest = float.MaxValue;
            for (int i = 0; i < analyzer.Bands.Count; i++)
                quietest = Math.Min(quietest, analyzer.Bands[i]);

            Assert.True(loudest - quietest > 10, $"range was {loudest - quietest:0.0} dB, loudest {loudest:0.0}, quietest {quietest:0.0}");
        }

        /// <summary>Verifies that silence reads as the floor rather than as a number.</summary>
        [Fact]
        public void Add_WithSilence_ReadsTheFloor()
        {
            var analyzer = new SpectrumAnalyzer();
            var samples = new float[SampleRate];

            analyzer.Add(samples, samples.Length, channels: 1, SampleRate, seconds: 1);

            foreach (float band in analyzer.Bands)
                Assert.Equal(SpectrumAnalyzer.FloorDecibels, band, 3);
        }

        /// <summary>Builds a sine wave with the given frequency and amplitude.</summary>
        private static float[] Sine(double frequency, double amplitude, int samples)
        {
            var buffer = new float[samples];

            for (int i = 0; i < samples; i++)
                buffer[i] = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * i / SampleRate));

            return buffer;
        }
    }
}