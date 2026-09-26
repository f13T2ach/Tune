using Tune.Models.Dialog;
using Tune.Models.Music;
using Tune.Models.Playlist;

namespace Tune.Generic.DialogGenerics
{
    public class DialogBaseMusicPlaylistBasePlaylistBase : DialogBase<(Music, PlaylistBase?), PlaylistBase>
    {
        public DialogBaseMusicPlaylistBasePlaylistBase() : base() { }
    }
}
