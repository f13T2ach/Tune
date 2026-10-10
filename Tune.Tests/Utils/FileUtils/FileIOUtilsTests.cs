using System.IO;
using System.Text;

using Tune.Models.Results;
using Tune.Utils.FileUtils;
using Xunit;

namespace Tune.Tests.Utils.FileUtils
{
    /// <summary>Verifies the behavior of <see cref="FileIOUtils"/>.</summary>
    public class FileIOUtilsTests : IDisposable
    {
        private readonly string _root;

        /// <summary>Creates an isolated temporary directory for the test.</summary>
        public FileIOUtilsTests()
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

        #region ==== Write ====

        /// <summary>Verifies that writing creates the file with the given content.</summary>
        [Fact]
        public void WriteToFile_WithValidPath_CreatesFileWithContent()
        {
            string path = Path.Combine(_root, "write.txt");

            ResultWithException result = FileIOUtils.WriteToFile(path, "payload");

            Assert.True(result.Success);
            Assert.Equal("payload", File.ReadAllText(path));
        }

        /// <summary>Verifies that writing honours the requested encoding.</summary>
        [Fact]
        public void WriteToFile_WithEncoding_WritesWithThatEncoding()
        {
            string path = Path.Combine(_root, "encoded.txt");

            FileIOUtils.WriteToFile(path, "payload", Encoding.Unicode);

            Assert.Equal("payload", File.ReadAllText(path, Encoding.Unicode));
        }

        /// <summary>Verifies that an empty path fails instead of throwing.</summary>
        [Fact]
        public void WriteToFile_WithEmptyPath_ReportsFailure()
        {
            ResultWithException result = FileIOUtils.WriteToFile(string.Empty, "payload");

            Assert.False(result.Success);
            Assert.IsType<ArgumentNullException>(result.Exception);
        }

        /// <summary>Verifies that a null content fails instead of throwing.</summary>
        [Fact]
        public void WriteToFile_WithNullContent_ReportsFailure()
        {
            ResultWithException result = FileIOUtils.WriteToFile(Path.Combine(_root, "write.txt"), null!);

            Assert.False(result.Success);
            Assert.IsType<ArgumentNullException>(result.Exception);
        }

        #endregion

        #region ==== Read ====

        /// <summary>Verifies that reading returns the content that was written before.</summary>
        [Fact]
        public void ReadFile_AfterWrite_ReturnsWrittenContent()
        {
            string path = Path.Combine(_root, "roundtrip.txt");
            FileIOUtils.WriteToFile(path, "payload");

            ResultContentWithException<string> result = FileIOUtils.ReadFile(path);

            Assert.True(result.Success);
            Assert.Equal("payload", result.Content);
        }

        /// <summary>Verifies that reading a missing file fails instead of throwing.</summary>
        [Fact]
        public void ReadFile_WithMissingFile_ReportsFailure()
        {
            ResultContentWithException<string> result = FileIOUtils.ReadFile(Path.Combine(_root, "missing.txt"));

            Assert.False(result.Success);
            Assert.IsType<FileNotFoundException>(result.Exception);
        }

        /// <summary>Verifies that an empty path fails instead of throwing.</summary>
        [Fact]
        public void ReadFile_WithEmptyPath_ReportsFailure()
        {
            ResultContentWithException<string> result = FileIOUtils.ReadFile(string.Empty);

            Assert.False(result.Success);
            Assert.IsType<ArgumentNullException>(result.Exception);
        }

        #endregion

        #region ==== Modify ====

        /// <summary>Verifies that modifying replaces the whole content of an existing file.</summary>
        [Fact]
        public void ModifyFile_WithExistingFile_ReplacesContent()
        {
            string path = Path.Combine(_root, "modify.txt");
            FileIOUtils.WriteToFile(path, "before");

            ResultWithException result = FileIOUtils.ModifyFile(path, "after");

            Assert.True(result.Success);
            Assert.Equal("after", File.ReadAllText(path));
        }

        /// <summary>Verifies that modifying a missing file fails instead of throwing.</summary>
        [Fact]
        public void ModifyFile_WithMissingFile_ReportsFailure()
        {
            ResultWithException result = FileIOUtils.ModifyFile(Path.Combine(_root, "missing.txt"), "after");

            Assert.False(result.Success);
            Assert.IsType<FileNotFoundException>(result.Exception);
        }

        #endregion

        #region ==== Delete ====

        /// <summary>Verifies that deleting removes an existing file.</summary>
        [Fact]
        public void DeleteFile_WithExistingFile_RemovesIt()
        {
            string path = Path.Combine(_root, "delete.txt");
            FileIOUtils.WriteToFile(path, "payload");

            ResultWithException result = FileIOUtils.DeleteFile(path);

            Assert.True(result.Success);
            Assert.False(File.Exists(path));
        }

        /// <summary>Verifies that deleting a missing file fails instead of throwing.</summary>
        [Fact]
        public void DeleteFile_WithMissingFile_ReportsFailure()
        {
            ResultWithException result = FileIOUtils.DeleteFile(Path.Combine(_root, "missing.txt"));

            Assert.False(result.Success);
            Assert.IsType<FileNotFoundException>(result.Exception);
        }

        #endregion
    }
}
