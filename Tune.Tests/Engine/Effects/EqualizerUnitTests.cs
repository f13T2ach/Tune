using NAudio.Effects;
using NAudio.Wave;

using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies the behavior of <see cref="EqualizerUnit"/>.</summary>
    public class EqualizerUnitTests
    {
        private const int SampleRate = 44100;
        private const int FrameCount = 44100;
        private const double InputAmplitude = 0.5;

        #region ==== Frequency Response ====

        /// <summary>Verifies that a flat equalizer leaves every frequency untouched.</summary>
        [Theory]
        [InlineData(30.0)]
        [InlineData(1000.0)]
        [InlineData(16000.0)]
        public void Configure_WithFlatBands_LeavesTheSignalUntouched(double frequency)
        {
            EqualizerUnit unit = CreateUnit();

            Assert.Equal(1.0, MeasureGain(unit, frequency), tolerance: 0.02);
        }

        /// <summary>Verifies that the bass knob lifts the low band and not the high band.</summary>
        [Fact]
        public void Configure_WithBassBoost_LiftsTheLowBandOnly()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Bass.Value = 50;

            Assert.InRange(MeasureGain(unit, 30), 3.5, 4.1);
            Assert.Equal(1.0, MeasureGain(unit, 16000), tolerance: 0.05);
        }

        /// <summary>Verifies that the bass knob cuts the low band.</summary>
        [Fact]
        public void Configure_WithBassCut_LowersTheLowBand()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Bass.Value = -50;

            Assert.InRange(MeasureGain(unit, 30), 0.23, 0.29);
        }

        /// <summary>Verifies that the mid knob acts on its centre frequency and stays narrow.</summary>
        [Fact]
        public void Configure_WithMidBoost_LiftsTheCentreFrequencyOnly()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Mid.Value = 50;

            Assert.InRange(MeasureGain(unit, 1000), 3.8, 4.15);
            Assert.Equal(1.0, MeasureGain(unit, 30), tolerance: 0.05);
            Assert.Equal(1.0, MeasureGain(unit, 16000), tolerance: 0.05);
        }

        /// <summary>Verifies that the treble knob lifts the high band and not the low band.</summary>
        [Fact]
        public void Configure_WithTrebleBoost_LiftsTheHighBandOnly()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Treble.Value = 50;

            Assert.InRange(MeasureGain(unit, 16000), 2.5, 4.1);
            Assert.Equal(1.0, MeasureGain(unit, 30), tolerance: 0.05);
        }

        /// <summary>Verifies that the trim scales the whole output.</summary>
        [Fact]
        public void Configure_WithTrimBoost_ScalesEveryFrequency()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Trim.Value = 50;

            Assert.InRange(MeasureGain(unit, 1000), 3.8, 4.15);
        }

        /// <summary>Verifies that a band moved after the format is known still takes effect.</summary>
        [Fact]
        public void Bass_AfterConfigure_LiftsTheLowBand()
        {
            EqualizerUnit unit = CreateUnit();
            unit.Configure(CreateFormat());

            unit.Bass.Value = 50;

            Assert.InRange(MeasureGain(unit, 30), 3.5, 4.1);
        }

        #endregion

        #region ==== Composition ====

        /// <summary>Verifies that the unit owns its three bands and the output trim, in order.</summary>
        [Fact]
        public void Parameters_WithDefaultUnit_HoldsBandsAndTrim()
        {
            EqualizerUnit unit = CreateUnit();

            Assert.Equal(
                new[] { "BASS", "MID", "TREBLE", "EQ" },
                unit.Parameters.Select(parameter => parameter.DisplayName));
        }

        /// <summary>Verifies that the bands and the trim both sit on the audio path.</summary>
        [Fact]
        public void Effects_WithDefaultUnit_HoldsTheEqualizerAndTheTrim()
        {
            EqualizerUnit unit = CreateUnit();

            Assert.Equal(2, unit.Effects.Count);
            Assert.IsType<Equalizer>(unit.Effects[0]);
            Assert.IsType<GainEffect>(unit.Effects[1]);
        }

        /// <summary>Verifies that the unit accepts a mono format.</summary>
        [Fact]
        public void Configure_WithMonoFormat_IsSupported()
        {
            EqualizerUnit unit = CreateUnit();

            unit.Configure(CreateFormat());

            Assert.True(unit.IsSupported);
        }

        #endregion

        /// <summary>Creates a unit over an in-memory store.</summary>
        private static EqualizerUnit CreateUnit()
        {
            return new EqualizerUnit(new InMemoryAudioEffectParameterStore());
        }

        /// <summary>Creates the mono format used by every measurement.</summary>
        private static WaveFormat CreateFormat()
        {
            return WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);
        }

        /// <summary>Runs a mono sine through the unit and returns the gain at that frequency.</summary>
        private static double MeasureGain(EqualizerUnit unit, double frequency)
        {
            unit.Configure(CreateFormat());

            float[] buffer = SineSignal.Create(frequency, InputAmplitude, SampleRate, FrameCount);
            Span<float> block = buffer;

            foreach (AudioEffect effect in unit.Effects)
                effect.Process(block);

            return SineSignal.MeasureAmplitude(buffer, frequency, SampleRate) / InputAmplitude;
        }
    }
}
