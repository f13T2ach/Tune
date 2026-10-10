using System.Globalization;

using Tune.Helpers.ValueConverters;
using Xunit;

namespace Tune.Tests.Helpers.ValueConverters
{
    /// <summary>Verifies the behavior of <see cref="SecondsToTimeConverter"/>.</summary>
    public class SecondsToTimeConverterTests
    {
        private readonly SecondsToTimeConverter _converter = new SecondsToTimeConverter();

        #region ==== Formatting ====

        /// <summary>Verifies that a duration below one hour uses the minute-second format.</summary>
        [Theory]
        [InlineData(0.0, "00:00")]
        [InlineData(65.0, "01:05")]
        [InlineData(3599.0, "59:59")]
        public void Convert_WithDurationBelowOneHour_ReturnsMinuteSecondFormat(double seconds, string expected)
        {
            Assert.Equal(expected, Format(seconds));
        }

        /// <summary>Verifies that a duration of at least one hour uses the hour format.</summary>
        [Theory]
        [InlineData(3600.0, "01:00:00")]
        [InlineData(3661.0, "01:01:01")]
        public void Convert_WithDurationOfAtLeastOneHour_ReturnsHourFormat(double seconds, string expected)
        {
            Assert.Equal(expected, Format(seconds));
        }

        #endregion

        #region ==== Parsing ====

        /// <summary>Verifies that the minute-second format is parsed back into seconds.</summary>
        [Fact]
        public void ConvertBack_WithMinuteSecondFormat_ReturnsSeconds()
        {
            Assert.Equal(65.0, Parse("01:05"));
        }

        /// <summary>Verifies that the hour format is parsed back into seconds.</summary>
        [Fact]
        public void ConvertBack_WithHourFormat_ReturnsSeconds()
        {
            Assert.Equal(3661.0, Parse("01:01:01"));
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
        public void Convert_WithNullValue_ReturnsZeroText()
        {
            Assert.Equal("00:00", Format(null!));
        }

        /// <summary>Verifies that a non-numeric value renders as zero.</summary>
        [Fact]
        public void Convert_WithNonNumericValue_ReturnsZeroText()
        {
            Assert.Equal("00:00", Format("abc"));
        }

        /// <summary>Verifies that a negative value is clamped to zero.</summary>
        [Fact]
        public void Convert_WithNegativeValue_ReturnsZeroText()
        {
            Assert.Equal("00:00", Format(-5.0));
        }

        /// <summary>Verifies that a not-a-number value renders as zero.</summary>
        [Fact]
        public void Convert_WithNotANumber_ReturnsZeroText()
        {
            Assert.Equal("00:00", Format(double.NaN));
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
