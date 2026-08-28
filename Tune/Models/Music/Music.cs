using System.IO;
using Tune.Helpers;
using Tune.Models.Lyric;
using Tune.Utils.MusicUtils;

namespace Tune.Models.Music
{
    /// <summary>
    /// Music Model
    /// Nullable: false
    /// </summary>
    public class Music
    {
        public MusicMetadata Metadata = new MusicMetadata();
        public LyricBase? Lyrics;   // can be null if lyrics unavailable
        public string MusicPath = String.Empty;
        // flags
        public bool isEmpty = true;

        public bool notExists = false;
        public bool isLyricsAvailable = false;

        /// <summary>
        /// Initializes a new **VAILD** instance of the Music class with the specified music path and retrieves its metadata.
        /// </summary>
        /// <param name="musicPath"></param>
        public Music(string musicPath)
        {
            if (!File.Exists(musicPath))
            {
                isEmpty = true;
                notExists = true;
                return;
            }

            Metadata = MetadataGetter.GetMetadata(musicPath);
            string lrcpath = PathConverters.GetLrcFilePath(musicPath);

            if (!String.IsNullOrEmpty(lrcpath))
            {
                Lyrics = new LrcLyric(lrcpath);
                isLyricsAvailable = true;
            }

            MusicPath = musicPath;
            isEmpty = false;
        }

        /// <summary>
        /// Initializes a new **EMPTY** instance of the Music class with default values.
        /// </summary>
        public Music()
        {
            isEmpty = true;
        }
    }
}
