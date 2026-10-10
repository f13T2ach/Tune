using System.IO;

using Tune.Utils.FileUtils;
using Xunit;

namespace Tune.Tests.Utils.FileUtils
{
    /// <summary>Verifies the behavior of <see cref="DirectoryUtils"/>.</summary>
    public class DirectoryUtilsTests : IDisposable
    {
        private readonly string _root;

        /// <summary>Creates an isolated temporary directory for the test.</summary>
        public DirectoryUtilsTests()
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

        #region ==== Listing ====

        /// <summary>Verifies that the files of an empty directory are reported as an empty array.</summary>
        [Fact]
        public void GetFilesInDirectory_WithEmptyDirectory_ReturnsEmptyArray()
        {
            Assert.Empty(DirectoryUtils.GetFilesInDirectory(_root));
        }

        /// <summary>Verifies that only the files directly inside the directory are returned.</summary>
        [Fact]
        public void GetFilesInDirectory_WithNestedFiles_ReturnsTopLevelFilesOnly()
        {
            File.WriteAllText(Path.Combine(_root, "a.txt"), "a");
            File.WriteAllText(Path.Combine(_root, "b.txt"), "b");
            string nested = Path.Combine(_root, "nested");
            Directory.CreateDirectory(nested);
            File.WriteAllText(Path.Combine(nested, "c.txt"), "c");

            string[] result = DirectoryUtils.GetFilesInDirectory(_root);

            Assert.Equal(2, result.Length);
            Assert.All(result, path => Assert.Equal(_root, Path.GetDirectoryName(path)));
        }

        #endregion

        #region ==== Edge Cases ====

        /// <summary>Verifies that a null path is rejected.</summary>
        [Fact]
        public void GetFilesInDirectory_WithNullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DirectoryUtils.GetFilesInDirectory(null!));
        }

        /// <summary>Verifies that an empty path is rejected.</summary>
        [Fact]
        public void GetFilesInDirectory_WithEmptyPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => DirectoryUtils.GetFilesInDirectory(string.Empty));
        }

        /// <summary>Verifies that a missing directory is reported.</summary>
        [Fact]
        public void GetFilesInDirectory_WithMissingDirectory_ThrowsDirectoryNotFoundException()
        {
            string missing = Path.Combine(_root, "missing");

            Assert.Throws<DirectoryNotFoundException>(() => DirectoryUtils.GetFilesInDirectory(missing));
        }

        #endregion
    }
}
