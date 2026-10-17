using System;

using NAudio.Wave;
using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies that the limiter clamps the output without changing the quiet parts.</summary>
    public class OutputLimiterProviderTests
    {
        #region ==== Clamping ====

        /// <summary>Verifies that samples above the ceiling are pulled down to it.</summary>
        [Fact]
        public void Read_AboveTheCeiling_IsClamped()
        {
            var limiter = new OutputLimiterProvider(new Block([1f, -1f])) { CeilingDecibels = -6 };
            var buffer = new float[2];

            limiter.Read(buffer, 0, 2);

            // -6 dB is about 0.5012 in linear terms.
            Assert.Equal(0.5011872f, buffer[0], 6);
            Assert.Equal(-0.5011872f, buffer[1], 6);
        }

        /// <summary>Verifies that samples under the ceiling are left alone.</summary>
        [Fact]
        public void Read_BelowTheCeiling_IsUnchanged()
        {
            var limiter = new OutputLimiterProvider(new Block([0.25f, -0.25f])) { CeilingDecibels = -6 };
            var buffer = new float[2];

            limiter.Read(buffer, 0, 2);

            Assert.Equal(0.25f, buffer[0]);
            Assert.Equal(-0.25f, buffer[1]);
        }

        /// <summary>Verifies that a ceiling of zero lets full scale through.</summary>
        [Fact]
        public void Read_WithFullScaleCeiling_ClampsNothing()
        {
            var limiter = new OutputLimiterProvider(new Block([1f, -1f]));
            var buffer = new float[2];

            limiter.Read(buffer, 0, 2);

            Assert.Equal(1f, buffer[0]);
            Assert.Equal(-1f, buffer[1]);
        }

        /// <summary>Verifies that turning it off lets everything through.</summary>
        [Fact]
        public void Read_WhenTurnedOff_ClampsNothing()
        {
            var limiter = new OutputLimiterProvider(new Block([1f, -1f]))
            {
                CeilingDecibels = -12,
                IsEnabled = false
            };

            var buffer = new float[2];
            limiter.Read(buffer, 0, 2);

            Assert.Equal(1f, buffer[0]);
            Assert.Equal(-1f, buffer[1]);
        }

        /// <summary>Verifies the round trip between decibels and the ceiling.</summary>
        [Fact]
        public void CeilingDecibels_AfterSetting_ReadsBack()
        {
            var limiter = new OutputLimiterProvider(new Block([0f]));

            limiter.CeilingDecibels = -12;

            Assert.Equal(-12, limiter.CeilingDecibels, 4);
        }

        #endregion

        /// <summary>A provider that hands out one fixed block.</summary>
        private sealed class Block(float[] samples) : ISampleProvider
        {
            private bool _read;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(1000, 2);

            public int Read(Span<float> buffer)
            {
                if (_read) return 0;

                _read = true;
                samples.AsSpan().CopyTo(buffer);

                return samples.Length;
            }

            public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        }
    }
}
