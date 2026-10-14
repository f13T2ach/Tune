using Tune.Client.Controllers;
using Tune.Models.Playlist;
using Xunit;

namespace Tune.Tests.Client.Controllers
{
    /// <summary>Verifies how the hidden history playlist is recognised.</summary>
    public class ListControllerHistoryTests
    {
        /// <summary>
        /// Verifies that the playlist carrying the reserved identity is the history one, no
        /// matter what it is called in the current language.
        /// </summary>
        [Fact]
        public void IsHistory_WithTheReservedIdentity_IsTrue()
        {
            var history = new CollectionPlaylist { ID = ListController.HistoryPlaylistID };

            Assert.True(ListController.IsHistory(history));
        }

        /// <summary>Verifies that an ordinary playlist is not mistaken for the history one.</summary>
        [Fact]
        public void IsHistory_WithAnOrdinaryPlaylist_IsFalse()
        {
            Assert.False(ListController.IsHistory(new CollectionPlaylist("id", "Rock")));
            Assert.False(ListController.IsHistory(null));
        }
    }
}
