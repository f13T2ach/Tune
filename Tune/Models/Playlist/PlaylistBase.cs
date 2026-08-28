using Newtonsoft.Json.Linq;
using Tune.Models.Storage;
using Tune.Values.Enums;

namespace Tune.Models.Playlist
{
    /// <summary>
    /// Base class of Playlist's variant
    /// </summary>
    public abstract class PlaylistBase : JsonStorageObject
    {
        public abstract bool IsDeleted { get; set; }
        public abstract string ID { get; set; }
        public abstract string Name { get; set; }
        public abstract string Description { get; set; }
        public abstract List<Music.Music> MusicList { get; set; }
        public abstract bool isEmptyOrNotInited { get; set; }

        public abstract DateTime CreateDateTime { get; set; }

        /// <summary>
        /// Only Directory Type need this.
        /// This MIGHT BE NULL
        /// </summary>
        public abstract List<string>? SourceFrom { get; set; }     // Only Directory Type need this
                                                                   // In current version, we only need SourceFrom[0]
        public abstract EPlaylistType Type { get; }

        public override string? ToString()
        {
            return Name == null ? base.ToString() : Name;
        }
        
        public abstract void SavePlaylist();

        public override abstract JObject? ToJObject();

        
    }
}
