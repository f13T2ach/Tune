using System.Globalization;

using Tune.Client.Controllers;
using Tune.Models.Playlist;
using Tune.Values.Static;
using Xunit;

namespace Tune.Tests.Client.Controllers
{
    /// <summary>Verifies the date formatting <see cref="ListController"/> feeds to the playlist header.</summary>
    public class ListControllerTests
    {
        #region ==== Missing Date ====

        /// <summary>Verifies that a playlist without a creation date shows the placeholder.</summary>
        [Fact]
        public void FormatCreateDate_WithoutPlaylist_ReturnsPlaceholder()
        {
            Assert.Equal(Strings.NO_CONTENT, ListController.FormatCreateDate(null));
        }

        /// <summary>Verifies that the unset creation date is treated as missing.</summary>
        [Fact]
        public void FormatCreateDate_WithUnsetDate_ReturnsPlaceholder()
        {
            var playlist = new CollectionPlaylist { CreateDateTime = DateTime.MinValue };

            Assert.Equal(Strings.NO_CONTENT, ListController.FormatCreateDate(playlist));
        }

        #endregion

        #region ==== Culture ====

        /// <summary>Verifies that the date follows the culture the app runs in.</summary>
        [Theory]
        [InlineData("zh-CN", "2026/8/30")]
        [InlineData("en-US", "8/30/2026")]
        public void FormatCreateDate_WithDate_UsesCurrentCulture(string cultureName, string expected)
        {
            CultureInfo original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                var playlist = new CollectionPlaylist { CreateDateTime = new DateTime(2026, 8, 30) };

                Assert.Equal(expected, ListController.FormatCreateDate(playlist));
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        #endregion
    }
}
