using Tune.Models.Dialog;
using Tune.Models.Playlist;

namespace Tune.Generic.DialogGenerics
{
    /// <summary>
    /// Generic Implement of `DialogBase<string, PlaylistBase>`
    /// Xaml doesn't support generic class
    /// </summary>
    public class DialogBaseStringPlaylistBase : DialogBase<string, PlaylistBase>
    {
        public DialogBaseStringPlaylistBase() : base() { }
    }
}
