using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies the band layout of <see cref="TenBandEqualizerUnit"/>.</summary>
    public class TenBandEqualizerUnitTests
    {
        #region ==== Bands ====

        /// <summary>Verifies that the ten bands sit on the standard octave centres.</summary>
        [Fact]
        public void BandFrequencies_Always_CoverTenOctaveBands()
        {
            Assert.Equal(10, TenBandEqualizerUnit.BandFrequencies.Length);
            Assert.Equal(31.5, TenBandEqualizerUnit.BandFrequencies[0]);
            Assert.Equal(1000.0, TenBandEqualizerUnit.BandFrequencies[5]);
            Assert.Equal(16000.0, TenBandEqualizerUnit.BandFrequencies[9]);
        }

        /// <summary>Verifies that a new unit has one flat band per frequency.</summary>
        [Fact]
        public void Bands_OnNewUnit_AreFlatAndInDisplayOrder()
        {
            var unit = new TenBandEqualizerUnit();

            Assert.Equal(TenBandEqualizerUnit.BandFrequencies.Length, unit.Bands.Count);
            for (int i = 0; i < unit.Bands.Count; i++)
            {
                Assert.Equal(TenBandEqualizerUnit.BandFrequencies[i], unit.Bands[i].Frequency);
                Assert.Equal(0.0, unit.Bands[i].Gain);
            }
        }

        #endregion

        #region ==== Gains ====

        /// <summary>Verifies that applying a preset reaches every band.</summary>
        [Fact]
        public void ApplyGains_WithTenValues_ReachesEveryBand()
        {
            var unit = new TenBandEqualizerUnit();
            double[] gains = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            unit.ApplyGains(gains);

            Assert.Equal(gains, unit.CaptureGains());
        }

        /// <summary>Verifies that a short preset leaves the remaining bands alone.</summary>
        [Fact]
        public void ApplyGains_WithFewerValues_KeepsTheOtherBands()
        {
            var unit = new TenBandEqualizerUnit();

            unit.ApplyGains(new[] { -3.0, -6.0 });

            Assert.Equal(-3.0, unit.Bands[0].Gain);
            Assert.Equal(-6.0, unit.Bands[1].Gain);
            Assert.Equal(0.0, unit.Bands[2].Gain);
        }

        /// <summary>Verifies that an empty preset changes nothing.</summary>
        [Fact]
        public void ApplyGains_WithoutValues_ChangesNothing()
        {
            var unit = new TenBandEqualizerUnit();
            unit.Bands[4].Gain = 5;

            unit.ApplyGains(null);

            Assert.Equal(5.0, unit.Bands[4].Gain);
        }

        /// <summary>Verifies that the two directions of the gain list match.</summary>
        [Fact]
        public void CaptureGains_AfterChangingOneBand_ReportsThatBand()
        {
            var unit = new TenBandEqualizerUnit();

            unit.Bands[7].Gain = -9.5;

            double[] gains = unit.CaptureGains();
            Assert.Equal(-9.5, gains[7]);
            Assert.Equal(10, gains.Length);
        }

        #endregion
    }
}
