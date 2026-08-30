using Tune.Components.UI;
using Tune.Models.Dialog;
using Tune.Models.Playlist;

namespace Tune.Helpers.DialogExtentions
{

    public static class PlaylistDialogExtentions
    {
        /// <summary>
        /// 显示选择歌单对话框
        /// </summary>
        public static PlaylistBase? ShowSelectPlaylistDialog(
            this SelectPlaylist control,
            string description = "选择歌单")
        {
            var window = new DialogInstance<string?, PlaylistBase?>(control);
            return window.ShowDialog(description);
        }

        /// <summary>
        /// 显示创建歌单对话框，返回新创建的歌单（取消时返回 null）
        /// </summary>
        public static PlaylistBase? ShowCreatePlaylistDialog(
            this CreatePlaylist control,
            string description = "新建歌单")
        {
            control.Argument = description;
            var window = new DialogInstance<string?, PlaylistBase?>(control);
            return window.ShowDialog(description);
        }
    }
}