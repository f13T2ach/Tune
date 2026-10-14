using System.IO;

using Tune.Helpers.UiState;
using Xunit;

namespace Tune.Tests.Helpers.UiState
{
    /// <summary>Verifies how <see cref="UiStateStore"/> remembers values.</summary>
    public class UiStateStoreTests
    {
        private const string Key = "tab.probe";

        /// <summary>Verifies that a stored value is read back.</summary>
        [Fact]
        public void Set_ThenGet_ReturnsTheValue()
        {
            UiStateStore.Set(Key, "3");

            Assert.Equal("3", UiStateStore.Get(Key));
        }

        /// <summary>Verifies that an unknown key reads as empty rather than failing.</summary>
        [Fact]
        public void Get_WithoutAValue_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, UiStateStore.Get("tab.never-written"));
        }

        /// <summary>Verifies that an empty value forgets the key.</summary>
        [Fact]
        public void Set_WithEmptyValue_ForgetsTheKey()
        {
            UiStateStore.Set(Key, "2");
            UiStateStore.Set(Key, string.Empty);

            Assert.Equal(string.Empty, UiStateStore.Get(Key));
        }

        /// <summary>Verifies that the state survives a reload from disk.</summary>
        [Fact]
        public void Set_ThenReadingTheFile_KeepsTheValue()
        {
            UiStateStore.Set(Key, "1");

            Assert.True(File.Exists(UiStateStore.FilePath));
            Assert.Contains("tab.probe", File.ReadAllText(UiStateStore.FilePath));
        }
    }
}
