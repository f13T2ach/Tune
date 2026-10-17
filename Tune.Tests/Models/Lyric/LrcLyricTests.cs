using System.IO;

using Tune.Models.Lyric;
using Xunit;

namespace Tune.Tests.Models.Lyric
{
    /// <summary>Verifies how lyrics that share a timestamp are read back.</summary>
    public class LrcLyricTests : IDisposable
    {
        private readonly string _filePath =
            Path.Combine(Path.GetTempPath(), $"tune-lrc-{Guid.NewGuid():N}.lrc");

        /// <summary>Removes the temporary lyric file of this test.</summary>
        public void Dispose()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        #region ==== Lines Sharing A Timestamp ====

        /// <summary>Verifies that every line of a timestamp is kept, in the order of the file.</summary>
        [Fact]
        public void GetLyricLinesAtTime_WithThreeLinesAtOneTimestamp_KeepsFileOrder()
        {
            LrcLyric lyric = Parse(
                "[00:01.000]original\n" +
                "[00:01.000]translation\n" +
                "[00:01.000]romanisation\n");

            Assert.Equal(
                new[] { "original", "translation", "romanisation" },
                lyric.GetLyricLinesAtTime(2, 3));
        }

        /// <summary>Verifies that the limit drops the lines past it, starting from the end.</summary>
        [Fact]
        public void GetLyricLinesAtTime_WithLimitTwo_DropsTheLastLine()
        {
            LrcLyric lyric = Parse(
                "[00:01.000]original\n" +
                "[00:01.000]translation\n" +
                "[00:01.000]romanisation\n");

            Assert.Equal(new[] { "original", "translation" }, lyric.GetLyricLinesAtTime(2, 2));
            Assert.Equal(new[] { "original" }, lyric.GetLyricLinesAtTime(2, 1));
        }

        /// <summary>Verifies that the lines of earlier timestamps are not carried along.</summary>
        [Fact]
        public void GetLyricLinesAtTime_WithSeveralTimestamps_AnswersTheCurrentGroup()
        {
            LrcLyric lyric = Parse(
                "[00:01.000]first\n" +
                "[00:01.000]first translated\n" +
                "[00:05.000]second\n" +
                "[00:05.000]second translated\n");

            Assert.Equal(new[] { "second", "second translated" }, lyric.GetLyricLinesAtTime(6, 3));
        }

        /// <summary>Verifies that a plain lyric still answers its single line.</summary>
        [Fact]
        public void GetLyricLinesAtTime_WithoutTranslation_AnswersTheLine()
        {
            LrcLyric lyric = Parse("[00:01.000]only line\n");

            Assert.Equal(new[] { "only line" }, lyric.GetLyricLinesAtTime(2, 3));
        }

        /// <summary>Verifies that nothing is shown before the first line or without a limit.</summary>
        [Fact]
        public void GetLyricLinesAtTime_OutsideTheLyricOrWithoutLimit_AnswersNothing()
        {
            LrcLyric lyric = Parse("[00:10.000]later\n");

            Assert.Empty(lyric.GetLyricLinesAtTime(1, 3));
            Assert.Empty(lyric.GetLyricLinesAtTime(11, 0));
        }

        #endregion

        /// <summary>Writes the given lyric text to a temporary file and parses it.</summary>
        private LrcLyric Parse(string content)
        {
            File.WriteAllText(_filePath, content);

            return new LrcLyric(_filePath);
        }
    }
}
