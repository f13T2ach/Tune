using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Tests.Helpers.Mock;
using Xunit;

namespace Tune.Tests.Models.Playlist
{
    /// <summary>Verifies what adding music to a collection playlist does.</summary>
    public class CollectionPlaylistTests
    {
        #region ==== Adding Music ====

        /// <summary>Verifies that a file already in the list is not listed a second time.</summary>
        [Fact]
        public void AddMuisc_WithTheSamePath_KeepsOneEntry()
        {
            var playlist = new CollectionPlaylist();
            Music first = Create("probe.mp3");
            Music again = Create("probe.mp3");

            playlist.AddMuisc(first);
            playlist.AddMuisc(again);

            Assert.Single(playlist.MusicList);
            Assert.Same(first, playlist.MusicList[0]);
        }

        /// <summary>Verifies that the path is compared without regard to letter case.</summary>
        [Fact]
        public void AddMuisc_WithTheSamePathInOtherCase_KeepsOneEntry()
        {
            var playlist = new CollectionPlaylist();

            playlist.AddMuisc(Create("Probe.MP3"));
            playlist.AddMuisc(Create("probe.mp3"));

            Assert.Single(playlist.MusicList);
        }

        /// <summary>Verifies that different files are both kept, in the order they arrived.</summary>
        [Fact]
        public void AddMuisc_WithDifferentPaths_KeepsBoth()
        {
            var playlist = new CollectionPlaylist();
            Music first = Create("one.mp3");
            Music second = Create("two.mp3");

            playlist.AddMuisc(first);
            playlist.AddMuisc(second);

            Assert.Equal(2, playlist.MusicList.Count);
            Assert.Same(first, playlist.MusicList[0]);
            Assert.Same(second, playlist.MusicList[1]);
        }

        /// <summary>Verifies that nothing is added when there is nothing to add.</summary>
        [Fact]
        public void AddMuisc_WithoutMusic_ChangesNothing()
        {
            var playlist = new CollectionPlaylist();

            playlist.AddMuisc(null!);

            Assert.Empty(playlist.MusicList);
        }

        #endregion

        /// <summary>Creates a song that carries the given path, which is what is compared.</summary>
        private static Music Create(string musicPath)
        {
            Music music = MusicTestFactory.CreateFake("Probe", "Tune");
            music.MusicPath = musicPath;

            return music;
        }
    }
}
