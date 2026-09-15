using Newtonsoft.Json.Linq;
using Tune.Assets.Localization;
using Tune.Helpers.JSONConverters;
using Tune.Persistence.PlaylistStorage;
using Tune.Values.Enums;

namespace Tune.Models.Playlist
{
    /// <summary>
    /// CollectionPlaylist: The Playlist that formed by user adding music
    /// Nullable: false
    /// </summary>
    public class CollectionPlaylist : PlaylistBase
    {
        #region ==== Properties ====
        public override EPlaylistType Type => EPlaylistType.Collection;

        private string _name = string.Empty;
        public override string Name { get => _name; set => _name = value; }

        private string _description = string.Empty;
        public override string Description { get => _description; set => _description = value; }

        private List<Music.Music> _musicList = new List<Music.Music>();
        public override List<Music.Music> MusicList { get => _musicList; set => _musicList = value; }

        private bool _isEmptyOrNotInited = true;
        public override bool isEmptyOrNotInited { get => _isEmptyOrNotInited; set => _isEmptyOrNotInited = value; }

        private List<string>? _sourceFrom = null;       // CollectionPlaylist doesnt have source
        public override List<string>? SourceFrom { get => _sourceFrom; set => _sourceFrom = value; }

        private string _id = string.Empty;
        public override string ID { get => _id; set => _id = value; }

        private bool _isDeleted = false;
        public override bool IsDeleted { get => _isDeleted; set => _isDeleted = value; }

        private DateTime _createDateTime = DateTime.MinValue;
        public override DateTime CreateDateTime { get => _createDateTime; set => _createDateTime = value; }
        #endregion

        /// <summary>
        /// This is used to new a empty instance
        /// It would not init any properties
        /// ONLY USED TO TEST OR DEBUG.
        /// </summary>
        public CollectionPlaylist()
        {
            // Do nothing
        }

        /// <summary>
        /// Init a non-empty instance
        /// THIS WOULD SET EMPTY FLAG TO FALSE!!!
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public CollectionPlaylist(string id, string name, string description = "")
        {
            ID = id;
            Name = string.IsNullOrEmpty(name) ? LangStrings.NoName : name;
            Description = string.IsNullOrEmpty(description) ? LangStrings.NoDescription : description;
            isEmptyOrNotInited = false;
            CreateDateTime = DateTime.Now;
        }

        public void Delete()
        {
            isEmptyOrNotInited = true;
            IsDeleted = true;
        }

        public Music.Music? GetMusicFromIndex(int index)
        {
            return MusicList.ElementAtOrDefault(index);
        }

        public void AddListMusicToList(List<Music.Music> list)
        {
            throw new NotImplementedException();
        }

        public override void SavePlaylist()
        {
            PlaylistProvider provider = new PlaylistProvider();
            provider.SavePlaylist(this);
        }

        /// <summary>
        /// This would **not auto save** playlist after adding music to list.
        /// </summary>
        /// <param name="music"></param>
        public override void AddMuisc(Music.Music music)
        {
            MusicList.Add(music);
            // SavePlaylist();
        }

        public override JObject? ToJObject()
        {
            return PlaylistBaseConverter.FromCollectionPlaylistToJSON(this);
        }
    }
}
