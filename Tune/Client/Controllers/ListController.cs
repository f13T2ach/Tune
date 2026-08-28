
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using Tune.Components.UI;
using Tune.Helpers;
using Tune.Helpers.DialogExtentions;
using Tune.Models.Playlist;
using Tune.Values.Config;
using Tune.Values.Enums;
using Tune.Values.Static;

namespace Tune.Client.Controllers
{
    /// <summary>
    /// List Play Controller
    /// The middle layer between UI and player
    ///         ----------------
    ///             UI Layer
    ///         ----------------
    ///          ListPlayController
    /// (This provides UI Properties and Events and conatins inner logicss)
    ///         ----------------
    ///             Engine
    ///         ----------------
    /// </summary>
    public partial class ListController : ObservableObject
    {
        private EListPlayStatus ListPlayStatus { get; set; }

        [ObservableProperty]
        public string? _listIconSource;

        public void OnSetNoLoop()
        {
            ListPlayStatus = EListPlayStatus.NoLoop;
            ListIconSource = Paths.LISTPLAY_ICON_PATH_DICT[ListPlayStatus];
        }

        public void OnSetLoopStatus()
        {
            // Get the current status accroding to last status
            EListPlayStatus nextStatus = ListPlayStatus switch
            {
                EListPlayStatus.NoLoop => EListPlayStatus.ListLoop,        // Switch to ListLoop(Enable loop)
                EListPlayStatus.ListLoop => EListPlayStatus.SingleLoop,
                EListPlayStatus.SingleLoop => EListPlayStatus.RandomListPlay,
                EListPlayStatus.RandomListPlay => EListPlayStatus.ListLoop, // Back to the head: ListLoop
                _ => EListPlayStatus.NoLoop
            };

            // Update
            ListPlayStatus = nextStatus;
            ListIconSource = Paths.LISTPLAY_ICON_PATH_DICT[nextStatus];
        }

        public void OnSetPlaylist()
        {
            var selector = new SelectPlaylist();
            var selected = selector.ShowSelectPlaylistDialog("请选择歌单");
        }

        public ListController()
        {
            GetLoopStatusFromSettings();
        }

        private void GetLoopStatusFromSettings()
        {
            OnSetNoLoop();
        }
    }
}
