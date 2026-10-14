using System.Globalization;

using Tune.Components.Slider;
using Xunit;

namespace Tune.Tests.Components.Slider
{
    /// <summary>Verifies the layout and text helpers of <see cref="EqFader"/>.</summary>
    public class EqFaderTests
    {
        #region ==== Frequency Text ====

        /// <summary>Verifies that frequencies below one kilohertz keep their number.</summary>
        [Theory]
        [InlineData(31, "31")]
        [InlineData(63, "63")]
        [InlineData(125, "125")]
        [InlineData(999, "999")]
        public void FormatFrequency_BelowOneKilo_ReturnsPlainNumber(int frequency, string expected)
        {
            Assert.Equal(expected, EqFader.FormatFrequency(frequency));
        }

        /// <summary>Verifies that larger frequencies are shown in kilohertz.</summary>
        [Theory]
        [InlineData(1000, "1k")]
        [InlineData(2000, "2k")]
        [InlineData(1200, "1.2k")]
        [InlineData(3150, "3.15k")]
        [InlineData(16000, "16k")]
        public void FormatFrequency_AtOrAboveOneKilo_ReturnsKiloHertz(int frequency, string expected)
        {
            WithCulture("en-US", () =>
                Assert.Equal(expected, EqFader.FormatFrequency(frequency)));
        }

        #endregion

        #region ==== Value Text ====

        /// <summary>Verifies that a gain is shown without trailing zeros.</summary>
        [Theory]
        [InlineData(0.0, "0")]
        [InlineData(3.0, "3")]
        [InlineData(-2.5, "-2.5")]
        [InlineData(1.25, "1.25")]
        public void FormatValue_WithGain_ReturnsShortNumber(double value, string expected)
        {
            WithCulture("en-US", () => Assert.Equal(expected, EqFader.FormatValue(value)));
        }

        /// <summary>Verifies that an unset gain stays visible as it is.</summary>
        [Fact]
        public void FormatValue_WithNotANumber_ReturnsNotANumber()
        {
            Assert.Equal("NaN", EqFader.FormatValue(double.NaN));
        }

        #endregion

        #region ==== Snapping ====

        /// <summary>Verifies that a gain lands on the nearest step.</summary>
        [Theory]
        [InlineData(2.4, 2.0)]
        [InlineData(2.6, 3.0)]
        [InlineData(-2.4, -2.0)]
        [InlineData(0.0, 0.0)]
        public void SnapValue_WithStepOne_LandsOnWholeStep(double value, double expected)
        {
            Assert.Equal(expected, EqFader.SnapValue(value, -12, 12, 1));
        }

        /// <summary>Verifies that a gain outside the range is pulled back into it.</summary>
        [Theory]
        [InlineData(50.0, 12.0)]
        [InlineData(-50.0, -12.0)]
        public void SnapValue_OutsideRange_ClampsToRange(double value, double expected)
        {
            Assert.Equal(expected, EqFader.SnapValue(value, -12, 12, 1));
        }

        /// <summary>Verifies a fractional step.</summary>
        [Theory]
        [InlineData(0.3, 0.5)]
        [InlineData(0.2, 0.0)]
        public void SnapValue_WithHalfStep_LandsOnHalfStep(double value, double expected)
        {
            Assert.Equal(expected, EqFader.SnapValue(value, 0, 1, 0.5));
        }

        /// <summary>Verifies that a useless step falls back to one.</summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(-3.0)]
        public void SnapValue_WithoutUsableStep_StillSnaps(double step)
        {
            Assert.Equal(2.0, EqFader.SnapValue(2.4, -12, 12, step));
        }

        #endregion

        #region ==== Position Mapping ====

        /// <summary>Verifies that the ends of the fader reach the ends of the range.</summary>
        [Fact]
        public void ValueFromPosition_AtBothEnds_ReachesBothLimits()
        {
            Assert.Equal(12.0, EqFader.ValueFromPosition(0, 494, -12, 12, 1));
            Assert.Equal(-12.0, EqFader.ValueFromPosition(494, 494, -12, 12, 1));
        }

        /// <summary>Verifies that the middle of the fader is the middle of the range.</summary>
        [Fact]
        public void ValueFromPosition_AtCentre_ReturnsCentreGain()
        {
            Assert.Equal(0.0, EqFader.ValueFromPosition(247, 494, -12, 12, 1));
        }

        /// <summary>Verifies that a gain is placed where the fader would read it back.</summary>
        [Theory]
        [InlineData(12.0)]
        [InlineData(6.0)]
        [InlineData(0.0)]
        [InlineData(-6.0)]
        [InlineData(-12.0)]
        public void ValueFromPosition_AfterPositionFromValue_ReturnsOriginalGain(double gain)
        {
            const double height = 494;

            double top = EqFader.PositionFromValue(gain, height, -12, 12);
            double centre = top + 4;

            Assert.Equal(gain, EqFader.ValueFromPosition(centre, height, -12, 12, 1));
        }

        /// <summary>Verifies the offset of the drag bar at the ends and the middle.</summary>
        [Fact]
        public void PositionFromValue_AtLimits_ReturnsTrackOffsets()
        {
            Assert.Equal(0.0, EqFader.PositionFromValue(12, 494, -12, 12));
            Assert.Equal(243.0, EqFader.PositionFromValue(0, 494, -12, 12));
            Assert.Equal(486.0, EqFader.PositionFromValue(-12, 494, -12, 12));
        }

        #endregion

        #region ==== Typed Gain ====

        /// <summary>Verifies the gains a user can type.</summary>
        [Theory]
        [InlineData("3", 3.0)]
        [InlineData("-2.5", -2.5)]
        [InlineData(" 4 ", 4.0)]
        public void TryParseValue_WithNumber_ReadsIt(string text, double expected)
        {
            WithCulture("en-US", () =>
            {
                Assert.True(EqFader.TryParseValue(text, out double value));
                Assert.Equal(expected, value);
            });
        }

        /// <summary>Verifies that text which is not a number is refused.</summary>
        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("--")]
        public void TryParseValue_WithoutNumber_ReturnsFalse(string text)
        {
            Assert.False(EqFader.TryParseValue(text, out _));
        }

        #endregion

        /// <summary>Runs an assertion under a fixed culture and restores the previous one.</summary>
        private static void WithCulture(string cultureName, Action assertion)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                assertion();
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }
    }
}
