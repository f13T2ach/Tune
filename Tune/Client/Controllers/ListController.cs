using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using Tune.Assets.Localization;
using Tune.Components.UI;
using Tune.Helpers;
using Tune.Helpers.DialogExtentions;
using Tune.Models.Playlist;
using Tune.Values.Config;
using Tune.Values.Enums;
using Tune.Values.Static;

namespace Tune.Client.Controllers
{
    public partial class ListController : ObservableObject
    {
        private EListPlayStatus ListPlayStatus { get; set; }

        // ==== observal ====
        [ObservableProperty]
        private PlaylistBase? _currentPlaylist;

        /// <summary>
        /// When CurrentPlaylist Changed, update private properties.
        /// THIS SHOULD BE CALLED AT LAST!!!(AFTER SAVING PLAYLIST(IF IT'S NEEDED TO SAVE))
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnCurrentPlaylistChanged(PlaylistBase? oldValue, PlaylistBase? newValue)
        {
            CurrentPlaylistName = newValue?.Name ?? string.Empty;
            CurrentPlaylistDescription = newValue?.Description ?? string.Empty;
            CurrentPlaylistCount = newValue?.MusicList?.Count ?? 0;
            CurrentPlaylistType = newValue?.Type ?? EPlaylistType.Empty;
        }

        // 辅助绑定属性（只读）
        [ObservableProperty]
        private string _currentPlaylistName = string.Empty;

        [ObservableProperty]
        private string _currentPlaylistDescription = string.Empty;

        [ObservableProperty]
        private int _currentPlaylistCount;

        [ObservableProperty]
        private EPlaylistType _currentPlaylistType;

        // 原有的 Icon 属性保持不变
        [ObservableProperty]
        private string? _listIconSource;

        public ListController()
        {
            GetLoopStatusFromSettings();
        }

        // ---------- 播放列表切换逻辑 ----------
        public void SetPlaylist(PlaylistBase? playlist)
        {
            if (playlist == null) return;
            CurrentPlaylist = playlist;
            // 可添加额外逻辑，如保存上次播放列表到设置
        }

        public void OnSetPlaylist()
        {
            var selector = new SelectPlaylist();
            var selected = selector.ShowSelectPlaylistDialog(LangStrings.SelectPlaylistToPlay);
            if (selected != null)
            {
                SetPlaylist(selected);
            }
        }

        public void OnSetTargetPlaylist(PlaylistBase playlist)
        {
            SetPlaylist(playlist);
        }

        // ---------- 循环模式切换（已有）----------
        public void OnSetNoLoop()
        {
            ListPlayStatus = EListPlayStatus.NoLoop;
            ListIconSource = Paths.LISTPLAY_ICON_PATH_DICT[ListPlayStatus];
        }

        public void OnSetLoopStatus()
        {
            EListPlayStatus nextStatus = ListPlayStatus switch
            {
                EListPlayStatus.NoLoop => EListPlayStatus.ListLoop,
                EListPlayStatus.ListLoop => EListPlayStatus.SingleLoop,
                EListPlayStatus.SingleLoop => EListPlayStatus.RandomListPlay,
                EListPlayStatus.RandomListPlay => EListPlayStatus.ListLoop,
                _ => EListPlayStatus.NoLoop
            };

            ListPlayStatus = nextStatus;
            ListIconSource = Paths.LISTPLAY_ICON_PATH_DICT[nextStatus];
        }

        private void GetLoopStatusFromSettings()
        {
            OnSetNoLoop(); // 实际应从设置读取
        }
    }
}