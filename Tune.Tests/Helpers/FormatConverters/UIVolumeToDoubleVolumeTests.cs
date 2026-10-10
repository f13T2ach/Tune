using Tune.Helpers.FormatConverters;
using Xunit;

namespace Tune.Tests.Helpers.FormatConverters
{
    /// <summary>Verifies the behavior of <see cref="UIVolumeToDoubleVolume"/>.</summary>
    public class UIVolumeToDoubleVolumeTests
    {
        #region ==== Forward Conversion ====

        /// <summary>Verifies that a UI volume is turned into its fractional equivalent.</summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(50.0, 0.5)]
        [InlineData(100.0, 1.0)]
        [InlineData(150.0, 1.5)]
        public void Convert_WithUiVolume_ReturnsFraction(double uiVolume, double expected)
        {
            Assert.Equal(expected, UIVolumeToDoubleVolume.Convert(uiVolume));
        }

        #endregion

        #region ==== Reverse Conversion ====

        /// <summary>Verifies that a fractional volume is turned back into the UI scale.</summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(0.5, 50.0)]
        [InlineData(1.0, 100.0)]
        public void ReverseConvert_WithFraction_ReturnsUiVolume(double fraction, double expected)
        {
            Assert.Equal(expected, UIVolumeToDoubleVolume.ReverseConvert(fraction));
        }

        #endregion

        #region ==== Round Trip ====

        /// <summary>Verifies that the two conversions undo each other.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(37.5)]
        [InlineData(100.0)]
        public void ReverseConvert_AfterConvert_ReturnsOriginalUiVolume(double uiVolume)
        {
            double fraction = UIVolumeToDoubleVolume.Convert(uiVolume);

            Assert.Equal(uiVolume, UIVolumeToDoubleVolume.ReverseConvert(fraction));
        }

        #endregion
    }
}
