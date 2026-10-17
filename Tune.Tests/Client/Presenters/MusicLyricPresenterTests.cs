using System.IO;

using Tune.Client.Presenters;
using Tune.Models.Lyric;
using Xunit;

namespace Tune.Tests.Client.Presenters
{
    /// <summary>Verifies which lyric lines the presenter hands to the view.</summary>
    public class MusicLyricPresenterTests : IDisposable
    {
        private readonly string _filePath =
            Path.Combine(Path.GetTempPath(), $"tune-lrc-{Guid.NewGuid():N}.lrc");

        private const string ThreeLines =
            "[00:01.000]original\n" +
            "[00:01.000]translation\n" +
            "[00:01.000]romanisation\n";

        private const string TwoLines =
            "[00:01.000]original\n" +
            "[00:01.000]translation\n";

        /// <summary>Removes the temporary lyric file of this test.</summary>
        public void Dispose()
        {
            if (File.Exists(_filePath)) File.Delete(_filePath);
        }

        #region ==== The Secondary Line ====

        /// <summary>
        /// Verifies that the secondary line is the translation by default, and that it is the
        /// only line under the main one even when the timestamp holds a third line.
        /// </summary>
        [Fact]
        public void UpdateLyricsBySecond_ByDefault_ShowsTheTranslation()
        {
            var presenter = new MusicLyricPresenter();
            presenter.UpdateLyric(Parse(ThreeLines));

            presenter.UpdateLyricsBySecond(2);

            Assert.Equal("original", presenter.Lyric);
            Assert.Equal(new[] { "translation" }, presenter.ExtraLines);
        }

        /// <summary>Verifies that the third line replaces the translation when it is asked for.</summary>
        [Fact]
        public void UpdateLyricsBySecond_WithTheThirdLineChosen_ShowsTheThirdLine()
        {
            var presenter = new MusicLyricPresenter { SecondaryIsThirdLine = true };
            presenter.UpdateLyric(Parse(ThreeLines));

            presenter.UpdateLyricsBySecond(2);

            Assert.Equal("original", presenter.Lyric);
            Assert.Equal(new[] { "romanisation" }, presenter.ExtraLines);
        }

        /// <summary>
        /// Verifies that the translation is shown when the third line was chosen but the
        /// timestamp does not have one.
        /// </summary>
        [Fact]
        public void UpdateLyricsBySecond_WithTheThirdLineChosenButMissing_ShowsTheTranslation()
        {
            var presenter = new MusicLyricPresenter { SecondaryIsThirdLine = true };
            presenter.UpdateLyric(Parse(TwoLines));

            presenter.UpdateLyricsBySecond(2);

            Assert.Equal("original", presenter.Lyric);
            Assert.Equal(new[] { "translation" }, presenter.ExtraLines);
        }

        #endregion

        #region ==== How Many Lines ====

        /// <summary>Verifies that a limit of one leaves the secondary line out.</summary>
        [Fact]
        public void UpdateLyricsBySecond_WithLimitOne_ShowsTheMainLineOnly()
        {
            var presenter = new MusicLyricPresenter { LineLimit = 1 };
            presenter.UpdateLyric(Parse(ThreeLines));

            presenter.UpdateLyricsBySecond(2);

            Assert.Equal("original", presenter.Lyric);
            Assert.Empty(presenter.ExtraLines);
        }

        /// <summary>Verifies that the secondary line is cleared when there is nothing to show.</summary>
        [Fact]
        public void UpdateLyricsBySecond_WithoutLyric_ClearsTheSecondaryLine()
        {
            var presenter = new MusicLyricPresenter();
            presenter.UpdateLyric(Parse(TwoLines));
            presenter.UpdateLyricsBySecond(2);

            presenter.UpdateLyric(null);
            presenter.UpdateLyricsBySecond(2);

            Assert.Empty(presenter.ExtraLines);
        }

        /// <summary>Verifies that a limit outside the allowed range is clamped.</summary>
        [Fact]
        public void LineLimit_OutsideTheAllowedRange_IsClamped()
        {
            var presenter = new MusicLyricPresenter();

            presenter.LineLimit = 99;
            Assert.Equal(MusicLyricPresenter.MaxLines, presenter.LineLimit);

            presenter.LineLimit = 0;
            Assert.Equal(1, presenter.LineLimit);
        }

        #endregion

        #region ==== The Position Readout ====

        /// <summary>
        /// Verifies the readout counts entries rather than lines, so a timestamp that holds a
        /// translation still counts as one entry.
        /// </summary>
        [Fact]
        public void UpdateLyricsBySecond_Always_ReportsTheCurrentEntry()
        {
            var presenter = new MusicLyricPresenter();
            presenter.UpdateLyric(Parse(
                "[00:01.000]first\n" +
                "[00:01.000]first translated\n" +
                "[00:05.000]second\n" +
                "[00:05.000]second translated\n"));

            presenter.UpdateLyricsBySecond(2);
            Assert.Equal("1/2", presenter.LyricPosition);

            presenter.UpdateLyricsBySecond(6);
            Assert.Equal("2/2", presenter.LyricPosition);
        }

        /// <summary>Verifies the placeholder before the first line and without a lyric at all.</summary>
        [Fact]
        public void UpdateLyricsBySecond_WithoutACurrentEntry_ShowsThePlaceholder()
        {
            var presenter = new MusicLyricPresenter();
            presenter.UpdateLyric(Parse("[00:10.000]later\n"));

            presenter.UpdateLyricsBySecond(1);
            Assert.Equal($"{Tune.Values.Static.Strings.NO_CONTENT}/{Tune.Values.Static.Strings.NO_CONTENT}", presenter.LyricPosition);

            presenter.UpdateLyric(null);
            presenter.UpdateLyricsBySecond(11);
            Assert.Equal($"{Tune.Values.Static.Strings.NO_CONTENT}/{Tune.Values.Static.Strings.NO_CONTENT}", presenter.LyricPosition);
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
