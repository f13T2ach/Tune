using Tune.Helpers.FormatConverters;
using Xunit;

namespace Tune.Tests.Helpers.FormatConverters
{
    /// <summary>Verifies the behavior of <see cref="UIMixToDoubleMix"/>.</summary>
    public class UIMixToDoubleMixTests
    {
        #region ==== Forward Conversion ====

        /// <summary>Verifies that a UI mix value is turned into its normalized factor.</summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(25.0, 0.5)]
        [InlineData(50.0, 1.0)]
        [InlineData(-50.0, -1.0)]
        public void Convert_WithUiMix_ReturnsFactor(double uiMix, double expected)
        {
            Assert.Equal(expected, UIMixToDoubleMix.Convert(uiMix));
        }

        #endregion

        #region ==== Reverse Conversion ====

        /// <summary>Verifies that a normalized factor is turned back into the UI scale.</summary>
        [Theory]
        [InlineData(0.0, 0.0)]
        [InlineData(0.5, 25.0)]
        [InlineData(1.0, 50.0)]
        [InlineData(-1.0, -50.0)]
        public void ReverseConvert_WithFactor_ReturnsUiMix(double factor, double expected)
        {
            Assert.Equal(expected, UIMixToDoubleMix.ReverseConvert(factor));
        }

        #endregion

        #region ==== Round Trip ====

        /// <summary>Verifies that the two conversions undo each other.</summary>
        [Theory]
        [InlineData(-50.0)]
        [InlineData(0.0)]
        [InlineData(37.5)]
        [InlineData(50.0)]
        public void ReverseConvert_AfterConvert_ReturnsOriginalUiMix(double uiMix)
        {
            double factor = UIMixToDoubleMix.Convert(uiMix);

            Assert.Equal(uiMix, UIMixToDoubleMix.ReverseConvert(factor));
        }

        #endregion
    }
}
