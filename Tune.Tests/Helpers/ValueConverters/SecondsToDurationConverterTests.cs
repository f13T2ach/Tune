using System.Globalization;

using Tune.Helpers.ValueConverters;
using Xunit;

namespace Tune.Tests.Helpers.ValueConverters
{
    /// <summary>Verifies the behavior of <see cref="SecondsToDurationConverter"/>.</summary>
    public class SecondsToDurationConverterTests
    {
        private readonly SecondsToDurationConverter _converter = new SecondsToDurationConverter();

        #region ==== Formatting ====

        /// <summary>Verifies that a duration below one hour uses the apostrophe format.</summary>
        [Theory]
        [InlineData(0.0, "0'00''")]
        [InlineData(59.0, "0'59''")]
        [InlineData(65.0, "1'05''")]
        [InlineData(225.0, "3'45''")]
        public void StaticConvert_WithDurationBelowOneHour_ReturnsApostropheFormat(double seconds, string expected)
        {
            Assert.Equal(expected, SecondsToDurationConverter.StaticConvert(seconds));
        }

        /// <summary>Verifies that a duration of at least one hour uses the hour format.</summary>
        [Theory]
        [InlineData(3600.0, "01:00:00")]
        [InlineData(3725.0, "01:02:05")]
        public void StaticConvert_WithDurationOfAtLeastOneHour_ReturnsHourFormat(double seconds, string expected)
        {
            Assert.Equal(expected, SecondsToDurationConverter.StaticConvert(seconds));
        }

        /// <summary>Verifies that the instance conversion matches the static conversion.</summary>
        [Fact]
        public void Convert_WithDuration_MatchesStaticConvert()
        {
            Assert.Equal(SecondsToDurationConverter.StaticConvert(225.0), Format(225.0));
        }

        #endregion

        #region ==== Parsing ====

        /// <summary>Verifies that the apostrophe format is parsed back into seconds.</summary>
        [Theory]
        [InlineData("1'05''", 65.0)]
        [InlineData("3'45''", 225.0)]
        public void ConvertBack_WithApostropheFormat_ReturnsSeconds(string text, double expected)
        {
            Assert.Equal(expected, Parse(text));
        }

        /// <summary>Verifies that the hour format is parsed back into seconds.</summary>
        [Theory]
        [InlineData("01:05", 65.0)]
        [InlineData("01:02:05", 3725.0)]
        public void ConvertBack_WithColonFormat_ReturnsSeconds(string text, double expected)
        {
            Assert.Equal(expected, Parse(text));
        }

        /// <summary>Verifies that an empty text is parsed as zero.</summary>
        [Fact]
        public void ConvertBack_WithEmptyText_ReturnsZero()
        {
            Assert.Equal(0.0, Parse(string.Empty));
        }

        /// <summary>Verifies that unparsable text is reported as zero.</summary>
        [Fact]
        public void ConvertBack_WithUnparsableText_ReturnsZero()
        {
            Assert.Equal(0.0, Parse("abc"));
        }

        #endregion

        #region ==== Edge Cases ====

        /// <summary>Verifies that a null value renders as zero.</summary>
        [Fact]
        public void StaticConvert_WithNullValue_ReturnsZeroText()
        {
            Assert.Equal("0'00''", SecondsToDurationConverter.StaticConvert(null!));
        }

        /// <summary>Verifies that a non-numeric value renders as zero.</summary>
        [Fact]
        public void StaticConvert_WithNonNumericValue_ReturnsZeroText()
        {
            Assert.Equal("0'00''", SecondsToDurationConverter.StaticConvert("abc"));
        }

        /// <summary>Verifies that a negative value is clamped to zero.</summary>
        [Fact]
        public void StaticConvert_WithNegativeValue_ReturnsZeroText()
        {
            Assert.Equal("0'00''", SecondsToDurationConverter.StaticConvert(-1.0));
        }

        /// <summary>Verifies that a not-a-number value renders as zero.</summary>
        [Fact]
        public void StaticConvert_WithNotANumber_ReturnsZeroText()
        {
            Assert.Equal("0'00''", SecondsToDurationConverter.StaticConvert(double.NaN));
        }

        /// <summary>Verifies that a value that is not text is parsed as zero.</summary>
        [Fact]
        public void ConvertBack_WithNonTextValue_ReturnsZero()
        {
            object result = _converter.ConvertBack(5, typeof(double), null!, CultureInfo.InvariantCulture);

            Assert.Equal(0.0, (double)result);
        }

        #endregion

        /// <summary>Formats the given number of seconds through the converter under test.</summary>
        private string Format(object value)
        {
            return (string)_converter.Convert(value, typeof(string), null!, CultureInfo.InvariantCulture);
        }

        /// <summary>Parses the given text through the converter under test.</summary>
        private double Parse(string text)
        {
            return (double)_converter.ConvertBack(text, typeof(double), null!, CultureInfo.InvariantCulture);
        }
    }
}
