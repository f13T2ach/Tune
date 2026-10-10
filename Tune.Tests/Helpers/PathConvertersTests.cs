using System.IO;

using Tune.Helpers;
using Xunit;

namespace Tune.Tests.Helpers
{
    /// <summary>Verifies the behavior of <see cref="PathConverters"/>.</summary>
    public class PathConvertersTests : IDisposable
    {
        private readonly string _root;

        /// <summary>Creates an isolated temporary directory for the test.</summary>
        public PathConvertersTests()
        {
            _root = Path.Combine(Path.GetTempPath(), "tune-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        /// <summary>Removes the temporary directory created for the test.</summary>
        public void Dispose()
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }

        #region ==== Resolution ====

        /// <summary>Verifies that a lyric file next to the audio file is found.</summary>
        [Fact]
        public void GetLrcFilePath_WithMatchingLyricFile_ReturnsLyricPath()
        {
            string audio = CreateAudioFile("song.mp3");
            string lyric = Path.Combine(_root, "song.lrc");
            File.WriteAllText(lyric, "[00:00.00]text");

            Assert.Equal(lyric, PathConverters.GetLrcFilePath(audio));
        }

        /// <summary>Verifies that a lyric file with an upper-case extension is found.</summary>
        [Fact]
        public void GetLrcFilePath_WithUpperCaseExtension_ReturnsLyricPath()
        {
            string audio = CreateAudioFile("song.mp3");
            File.WriteAllText(Path.Combine(_root, "song.LRC"), "[00:00.00]text");

            Assert.Equal(Path.Combine(_root, "song.lrc"), PathConverters.GetLrcFilePath(audio));
        }

        /// <summary>Verifies that an empty string is returned when no lyric file exists.</summary>
        [Fact]
        public void GetLrcFilePath_WithoutLyricFile_ReturnsEmpty()
        {
            string audio = CreateAudioFile("song.mp3");

            Assert.Equal(string.Empty, PathConverters.GetLrcFilePath(audio));
        }

        /// <summary>Verifies that a bare file name is resolved against the current directory.</summary>
        [Fact]
        public void GetLrcFilePath_WithBareFileName_ReturnsEmptyWhenNoLyricExists()
        {
            Assert.Equal(string.Empty, PathConverters.GetLrcFilePath("tune-tests-bare-name.mp3"));
        }

        #endregion

        #region ==== Edge Cases ====

        /// <summary>Verifies that a null path is rejected.</summary>
        [Fact]
        public void GetLrcFilePath_WithNullPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => PathConverters.GetLrcFilePath(null!));
        }

        /// <summary>Verifies that a whitespace path is rejected.</summary>
        [Fact]
        public void GetLrcFilePath_WithWhitespacePath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => PathConverters.GetLrcFilePath("   "));
        }

        #endregion

        /// <summary>Creates an empty audio file with the given name inside the temporary directory.</summary>
        private string CreateAudioFile(string fileName)
        {
            string path = Path.Combine(_root, fileName);
            File.WriteAllText(path, string.Empty);
            return path;
        }
    }
}
