using System;

using NAudio.Wave;
using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies the fixed system layer: the safety limiter and the meter in front of it.</summary>
    public class SystemOutputStageTests
    {
        private const float SafetyCeiling = 0.96605f;   // -0.3 dBFS

        /// <summary>Verifies that the safety limiter holds a full scale signal below full scale.</summary>
        [Fact]
        public void Read_AtFullScale_IsHeldUnderTheCeiling()
        {
            var stage = new SystemOutputStage(new Block([1f, -1f]));
            var buffer = new float[2];

            stage.Read(buffer, 0, 2);

            Assert.Equal(SafetyCeiling, buffer[0], 4);
            Assert.Equal(-SafetyCeiling, buffer[1], 4);
        }

        /// <summary>Verifies that quiet audio passes through the layer untouched.</summary>
        [Fact]
        public void Read_WellUnderTheCeiling_IsUnchanged()
        {
            var stage = new SystemOutputStage(new Block([0.25f, -0.25f]));
            var buffer = new float[2];

            stage.Read(buffer, 0, 2);

            Assert.Equal(0.25f, buffer[0]);
            Assert.Equal(-0.25f, buffer[1]);
        }

        /// <summary>
        /// Verifies that the meter sees the signal before the limiter, so full scale still reads as
        /// zero decibels and a red meter means the user layer really reached full scale.
        /// </summary>
        [Fact]
        public void Read_AtFullScale_ReportsZeroDecibels()
        {
            var stage = new SystemOutputStage(new Block([1f, 1f]));
            AudioLevelEventArgs? reported = null;
            stage.LevelReported += (_, e) => reported = e;

            stage.Read(new float[2], 0, 2);

            Assert.NotNull(reported);
            Assert.Equal(0, reported!.Left, 3);
            Assert.Equal(0, reported.Right, 3);
        }

        /// <summary>Verifies that a disposed stage stops reporting levels.</summary>
        [Fact]
        public void Dispose_ThenReading_StopsReporting()
        {
            var stage = new SystemOutputStage(new Block([1f, 1f]));
            int reports = 0;
            stage.LevelReported += (_, _) => reports++;

            stage.Read(new float[2], 0, 2);
            stage.Dispose();
            stage.Read(new float[2], 0, 2);

            Assert.Equal(1, reports);
        }

        /// <summary>A provider that hands out one fixed block.</summary>
        private sealed class Block(float[] samples) : ISampleProvider
        {
            private bool _read;

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

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