using System;
using System.Collections.Generic;

using NAudio.Wave;
using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies what the metering provider measures without changing the audio.</summary>
    public class MeteringSampleProviderTests
    {
        #region ==== The Peaks ====

        /// <summary>Verifies that the two channels are measured apart.</summary>
        [Fact]
        public void Read_WithTwoChannels_MeasuresEachSide()
        {
            var source = new SingleBlockProvider([1.0f, 0.25f], channels: 2, sampleRate: 1000);
            var meter = new MeteringSampleProvider(source);
            AudioLevelEventArgs? reported = null;
            meter.LevelReported += (_, e) => reported = e;

            meter.Read(new float[2], 0, 2);

            Assert.NotNull(reported);
            Assert.Equal(0, reported!.Left, 3);
            Assert.Equal(-12.041, reported.Right, 3);
        }

        /// <summary>Verifies that a mono source is measured on both sides.</summary>
        [Fact]
        public void Read_WithOneChannel_MeasuresBothSides()
        {
            var source = new SingleBlockProvider([0.5f], channels: 1, sampleRate: 1000);
            var meter = new MeteringSampleProvider(source);
            AudioLevelEventArgs? reported = null;
            meter.LevelReported += (_, e) => reported = e;

            meter.Read(new float[1], 0, 1);

            Assert.NotNull(reported);
            Assert.Equal(-6.020600, reported!.Left, 6);
            Assert.Equal(reported.Left, reported.Right, 6);
        }

        /// <summary>Verifies that silence lands on the floor rather than at infinity.</summary>
        [Fact]
        public void Read_WithSilence_UsesTheSilenceFloor()
        {
            var source = new SingleBlockProvider([0f, 0f], channels: 2, sampleRate: 1000);
            var meter = new MeteringSampleProvider(source);
            AudioLevelEventArgs? reported = null;
            meter.LevelReported += (_, e) => reported = e;

            meter.Read(new float[2], 0, 2);

            Assert.NotNull(reported);
            Assert.Equal(MeteringSampleProvider.SilenceDecibels, reported!.Left, 6);
        }

        #endregion

        #region ==== The Window ====

        /// <summary>Verifies that a window is reported once enough audio has gone by.</summary>
        [Fact]
        public void Read_UntilTheWindowCloses_ReportsItOnce()
        {
            // 2000 frames a second, so 400 frames last a fifth of a second, which is the window.
            var samples = new float[800];
            for (int i = 0; i < samples.Length; i += 2)
            {
                samples[i] = 0.5f;
                samples[i + 1] = 0.5f;
            }

            var source = new SingleBlockProvider(samples, channels: 2, sampleRate: 2000);
            var meter = new MeteringSampleProvider(source, TimeSpan.FromSeconds(0.2));
            var windows = new List<LevelWindowResult>();

            meter.LevelReported += (_, e) =>
            {
                if (e.Window.HasValue) windows.Add(e.Window.Value);
            };

            meter.Read(new float[800], 0, 800);

            Assert.Single(windows);
        }

        #endregion

        /// <summary>Verifies that the provider passes the audio through unchanged.</summary>
        [Fact]
        public void Read_PassesTheAudioThrough()
        {
            var source = new SingleBlockProvider([0.1f, -0.2f], channels: 2, sampleRate: 1000);
            var meter = new MeteringSampleProvider(source);
            var buffer = new float[4];

            int read = meter.Read(buffer, 0, 4);

            Assert.Equal(2, read);
            Assert.Equal(0.1f, buffer[0]);
            Assert.Equal(-0.2f, buffer[1]);
        }

        /// <summary>A provider that hands out the same block once.</summary>
        private sealed class SingleBlockProvider(float[] samples, int channels, int sampleRate) : ISampleProvider
        {
            private bool _read;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);

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
