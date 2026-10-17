using Tune.Appearance.ExtendView;
using Xunit;

namespace Tune.Tests.Components.Content
{
    /// <summary>Verifies the scale of the level meter, which decides colour and position.</summary>
    public class PeakMeterScaleTests
    {
        #region ==== Bands ====

        /// <summary>Verifies the red band, which runs from full scale down to the clipping line.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1.99)]
        [InlineData(-2)]
        public void BandOf_InTheTopBand_IsClip(double decibels) =>
            Assert.Equal(PeakMeterBand.Clip, PeakMeterScale.BandOf(decibels));

        /// <summary>Verifies the orange band, which runs from the clipping line to the warning line.</summary>
        [Theory]
        [InlineData(-2.01)]
        [InlineData(-8)]
        [InlineData(-14)]
        public void BandOf_BelowTheClippingLine_IsWarning(double decibels) =>
            Assert.Equal(PeakMeterBand.Warning, PeakMeterScale.BandOf(decibels));

        /// <summary>Verifies the green band, which runs from the warning line down to the floor.</summary>
        [Theory]
        [InlineData(-14.01)]
        [InlineData(-30)]
        [InlineData(-60)]
        public void BandOf_BelowTheWarningLine_IsNormal(double decibels) =>
            Assert.Equal(PeakMeterBand.Normal, PeakMeterScale.BandOf(decibels));

        /// <summary>Verifies that below the floor there is nothing to draw.</summary>
        [Fact]
        public void BandOf_BelowTheFloor_IsSilent() =>
            Assert.Equal(PeakMeterBand.Silent, PeakMeterScale.BandOf(-60.01));

        #endregion

        #region ==== Position ====

        /// <summary>Verifies that full scale sits at the top and the floor at the bottom.</summary>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-30, 0.5)]
        [InlineData(-60, 1)]
        public void FractionOf_OnTheScale_ReturnsThePosition(double decibels, double expected) =>
            Assert.Equal(expected, PeakMeterScale.FractionOf(decibels), 6);

        /// <summary>Verifies that levels outside the scale are held at its ends.</summary>
        [Fact]
        public void FractionOf_OutsideTheScale_IsHeldAtTheEnds()
        {
            Assert.Equal(0, PeakMeterScale.FractionOf(6));
            Assert.Equal(1, PeakMeterScale.FractionOf(-120));
        }

        #endregion
    }
}
