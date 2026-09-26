using Tune.Models.Music;

namespace Tune.Tests.Helpers.Mock
{
    /// <summary>
    /// Factory for creating fake Music instances used only in tests / design time.
    /// </summary>
    public static class MusicTestFactory
    {
        /// <summary>
        /// Creates a non-empty fake Music with the given title / artist.
        /// </summary>
        public static Music CreateFake(string title, string artist)
        {
            // Bypass the file-based constructor by using the empty constructor,
            // then manually mark it as valid and fill in the metadata.
            var music = new Music
            {
                isEmpty = false,
                notExists = true,
                isLyricsAvailable = false,
                MusicPath = $@"C:\Fake\{title}.mp3",
                Metadata = new MusicMetadata
                {
                    Title = title,
                    Artist = artist,
                    Album = "Test Album",
                },
                Lyrics = null
            };

            return music;
        }
    }
}