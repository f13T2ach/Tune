using CommunityToolkit.Mvvm.Messaging;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Models.Messages;
using Tune.Models.Playlist;
using Tune.ViewModel;
using Xunit;

namespace Tune.Tests.ViewModel
{
    /// <summary>Verifies what the playlists page does with the empty cover.</summary>
    public class PlaylistsPageViewModelTests : IDisposable
    {
        private readonly List<Type> _navigated = new();

        /// <summary>Listens to where the page asks the application to go.</summary>
        public PlaylistsPageViewModelTests()
        {
            WeakReferenceMessenger.Default.Register<NavigationMessage>(
                this, (_, message) => _navigated.Add(message.PageType));
        }

        /// <summary>Stops listening.</summary>
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }

        #region ==== The Empty Cover ====

        /// <summary>
        /// Verifies that the cover at the end of the list is an empty playlist, which is
        /// what makes opening it start a new one and playing it clear the player.
        /// </summary>
        [Fact]
        public void Playlists_Always_EndWithTheEmptyCover()
        {
            var view = new PlaylistsPageViewModel(new ListController(), null!);

            PlaylistBase last = Assert.IsType<CollectionPlaylist>(view.Playlists[^1]);

            Assert.True(last.isEmptyOrNotInited);
        }

        /// <summary>Verifies that playing the empty cover falls back to the history playlist.</summary>
        [Fact]
        public void PlayPlaylist_WithTheEmptyCover_FallsBackToTheHistoryPlaylist()
        {
            var controller = new ListController();
            var view = new PlaylistsPageViewModel(controller, null!);
            var empty = new CollectionPlaylist { Name = "Empty" };

            view.PlayPlaylistCommand.Execute(empty);

            Assert.True(ListController.IsHistory(controller.CurrentPlaylist));
        }

        #endregion

        #region ==== Opening A Playlist ====

        /// <summary>
        /// Verifies that clicking a cover makes that playlist the current one and opens the
        /// list page, which follows the current playlist.
        /// </summary>
        [Fact]
        public void OpenPlaylist_WithAPlaylist_MakesItCurrentAndOpensTheListPage()
        {
            var controller = new ListController();
            var view = new PlaylistsPageViewModel(controller, null!);
            var playlist = new CollectionPlaylist("id", "Rock");

            view.OpenPlaylistCommand.Execute(playlist);

            Assert.Same(playlist, controller.CurrentPlaylist);
            Assert.Contains(typeof(CurrentPlaylist), _navigated);
            Assert.Equal(typeof(PlaylistsPage), CurrentPlaylistViewModel.ReturnPageType);
        }

        /// <summary>
        /// Verifies that the empty cover asks for a new playlist instead of becoming the
        /// current one, and that a real playlist is simply opened.
        /// </summary>
        [Fact]
        public void OpenPlaylist_OnlyTheEmptyCover_AsksForANewPlaylist()
        {
            var controller = new ListController();
            var view = new PlaylistsPageViewModel(controller, null!);
            bool asked = false;
            view.CreatePlaylistDialog = () => { asked = true; return null; };

            view.OpenPlaylistCommand.Execute(new CollectionPlaylist("id", "Rock"));
            Assert.False(asked);

            view.OpenPlaylistCommand.Execute(new CollectionPlaylist { Name = "Empty" });
            Assert.True(asked);
        }

        #endregion
    }
}
