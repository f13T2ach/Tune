using System;

using NAudio.Wave;

using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies that the limiter holds peaks under its ceiling and leaves the rest alone.</summary>
    public class LimitingSampleProviderTests
    {
        #region ==== Peak Handling ====

        /// <summary>Verifies that a signal above full scale is brought under the ceiling.</summary>
        [Fact]
        public void Read_WithLoudSignal_KeepsThePeakUnderTheCeiling()
        {
            var limiter = new LimitingSampleProvider(new ConstantSampleProvider(2f));
            var buffer = new float[64];

            limiter.Read(buffer);

            Assert.All(buffer, sample =>
                Assert.True(Math.Abs(sample) <= LimitingSampleProvider.Ceiling));
        }

        /// <summary>Verifies that a quiet signal passes through untouched.</summary>
        [Fact]
        public void Read_WithQuietSignal_ChangesNothing()
        {
            var limiter = new LimitingSampleProvider(new ConstantSampleProvider(0.5f));
            var buffer = new float[64];

            limiter.Read(buffer);

            Assert.All(buffer, sample => Assert.Equal(0.5f, sample));
        }

        /// <summary>Verifies that a signal sitting exactly at the ceiling is left alone.</summary>
        [Fact]
        public void Read_AtTheCeiling_ChangesNothing()
        {
            var limiter = new LimitingSampleProvider(
                new ConstantSampleProvider(LimitingSampleProvider.Ceiling));
            var buffer = new float[16];

            limiter.Read(buffer);

            Assert.All(buffer, sample => Assert.Equal(LimitingSampleProvider.Ceiling, sample));
        }

        /// <summary>Verifies that a silent block stays silent.</summary>
        [Fact]
        public void Read_WithSilence_StaysSilent()
        {
            var limiter = new LimitingSampleProvider(new ConstantSampleProvider(0f));
            var buffer = new float[16];

            limiter.Read(buffer);

            Assert.All(buffer, sample => Assert.Equal(0f, sample));
        }

        #endregion

        /// <summary>Repeats one value, so a peak is easy to reason about.</summary>
        private sealed class ConstantSampleProvider : ISampleProvider
        {
            private readonly float _value;

            public ConstantSampleProvider(float value) => _value = value;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

            public int Read(Span<float> buffer)
            {
                buffer.Fill(_value);

                return buffer.Length;
            }
        }
    }
}
