using Tune.Helpers;
using Xunit;

namespace Tune.Tests.Helpers
{
    /// <summary>Verifies the behavior of <see cref="StringUtils"/>.</summary>
    public class StringUtilsTests
    {
        #region ==== Remove Empty Lines ====

        /// <summary>Verifies that a null input is returned unchanged.</summary>
        [Fact]
        public void RemoveEmptyLines_WithNullInput_ReturnsNull()
        {
            Assert.Null(StringUtils.RemoveEmptyLines(null!));
        }

        /// <summary>Verifies that an empty input is returned unchanged.</summary>
        [Fact]
        public void RemoveEmptyLines_WithEmptyInput_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, StringUtils.RemoveEmptyLines(string.Empty));
        }

        /// <summary>Verifies that blank lines are dropped while the remaining lines keep their order.</summary>
        [Fact]
        public void RemoveEmptyLines_WithBlankLines_DropsThemAndKeepsOrder()
        {
            string result = StringUtils.RemoveEmptyLines("first\n\n   \n\t\nsecond");

            Assert.Equal("first\nsecond", result);
        }

        /// <summary>Verifies that every common line ending is normalised to a line feed.</summary>
        [Fact]
        public void RemoveEmptyLines_WithMixedLineEndings_NormalisesToLineFeed()
        {
            string result = StringUtils.RemoveEmptyLines("a\r\nb\rc\nd");

            Assert.Equal("a\nb\nc\nd", result);
        }

        /// <summary>Verifies that a whitespace-only input collapses to an empty string.</summary>
        [Fact]
        public void RemoveEmptyLines_WithOnlyWhitespace_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, StringUtils.RemoveEmptyLines("  \n\t\n  "));
        }

        /// <summary>Verifies that the spaces surrounding a kept line are preserved.</summary>
        [Fact]
        public void RemoveEmptyLines_WithPaddedLine_KeepsSurroundingSpaces()
        {
            Assert.Equal(" padded ", StringUtils.RemoveEmptyLines(" padded \n\n"));
        }

        #endregion

        #region ==== Fill Empty Lyric Text ====

        /// <summary>Verifies that a timestamp-only line receives an invisible filler character.</summary>
        [Fact]
        public void FillEmptyLyricText_WithTimestampOnly_AppendsInvisibleFiller()
        {
            string result = StringUtils.FillEmptyLyricText("[00:00.00]");

            Assert.Equal("[00:00.00]\u200E", result);
        }

        /// <summary>Verifies that a line that already has text is left untouched.</summary>
        [Fact]
        public void FillEmptyLyricText_WithExistingText_LeavesLineUnchanged()
        {
            string result = StringUtils.FillEmptyLyricText("[00:00.00]hello");

            Assert.Equal("[00:00.00]hello", result);
        }

        /// <summary>Verifies that only the empty lines of a multi-line lyric are filled.</summary>
        [Fact]
        public void FillEmptyLyricText_WithinMultipleLines_FillsOnlyEmptyLines()
        {
            string result = StringUtils.FillEmptyLyricText("[00:01.00]\n[00:02.00]text");

            Assert.Equal("[00:01.00]\u200E\n[00:02.00]text", result);
        }

        /// <summary>Verifies that a line without a closing bracket is left untouched.</summary>
        [Fact]
        public void FillEmptyLyricText_WithoutClosingBracket_LeavesLineUnchanged()
        {
            string result = StringUtils.FillEmptyLyricText("no tag here");

            Assert.Equal("no tag here", result);
        }

        /// <summary>Verifies that a closing bracket inside a line is ignored.</summary>
        [Fact]
        public void FillEmptyLyricText_WithBracketInsideLine_LeavesLineUnchanged()
        {
            string result = StringUtils.FillEmptyLyricText("a]b");

            Assert.Equal("a]b", result);
        }

        /// <summary>Verifies that a null input is returned unchanged.</summary>
        [Fact]
        public void FillEmptyLyricText_WithNullInput_ReturnsNull()
        {
            Assert.Null(StringUtils.FillEmptyLyricText(null!));
        }

        #endregion
    }
}
