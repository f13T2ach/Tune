using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Assets.Localization;
using Tune.Components.UI;
using Tune.Helpers.DialogExtentions;
using Tune.Models.Playlist;
using Tune.Values.Enums;
using Tune.Values.Static;

namespace Tune.Client.Controllers
{
    public partial class ListController : ObservableObject
    {
        private EListPlayStatus ListPlayStatus { get; set; }

        // ==== Observable Properties ====
        [ObservableProperty]
        private PlaylistBase? _currentPlaylist;

        [ObservableProperty]
        private bool _isPlaylistNull = true;

        [ObservableProperty]
        private bool _isPlaylistEmpty = false;  // Behaviour: Null -> false, Empty -> true, Non-empty -> false

        [ObservableProperty]
        private string _currentPlaylistName = string.Empty;

        [ObservableProperty]
        private string _currentPlaylistDescription = string.Empty;

        [ObservableProperty]
        private int _currentPlaylistCount;

        [ObservableProperty]
        private EPlaylistType _currentPlaylistType;

        [ObservableProperty]
        private string? _listIconSource;

        public ListController()
        {
            GetLoopStatusFromSettings();
        }

        /// <summary>
        /// When CurrentPlaylist Changed, update private properties.
        /// THIS (to change_currentPlaylist) SHOULD BE CALLED AT LAST!!!(AFTER SAVING PLAYLIST(IF IT'S NEEDED TO SAVE))
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnCurrentPlaylistChanged(PlaylistBase? oldValue, PlaylistBase? newValue)
        {
            CurrentPlaylistName = newValue?.Name ?? string.Empty;
            CurrentPlaylistDescription = newValue?.Description ?? string.Empty;
            CurrentPlaylistCount = newValue?.MusicList?.Count ?? 0;
            CurrentPlaylistType = newValue?.Type ?? EPlaylistType.Empty;
            IsPlaylistNull = newValue == null;
            if(newValue != null)
            {
                IsPlaylistEmpty = newValue.MusicList.Count == 0;
            }
            else
            {
                IsPlaylistEmpty = false;
            }
        }

        // ============== Playlist Switching Logic ==============

        /// <summary>
        /// Set the current playlist. Passing <c>null</c> clears the current playlist
        /// and toggles <see cref="isPlaylistNull"/> to <c>true</c>.
        /// </summary>
        public void SetPlaylist(PlaylistBase? playlist)
        {
            // NOTE: Unlike the previous version, null is now a valid input.
            // Assigning null triggers OnCurrentPlaylistChanged and broadcasts the
            // "null playlist" state to all bindings (name/description/count/type).
            CurrentPlaylist = playlist;
            // Additional logic (e.g. persist the last playlist to settings) may go here.
        }

        /// <summary>
        /// Show the playlist selector dialog and apply the user's choice.
        /// The dialog also exposes a <c>LoadNullPlaylist</c> event, which is raised
        /// when the user explicitly picks the "None" entry and confirms.
        /// </summary>
        public void OnSetPlaylist()
        {
            var selector = new SelectPlaylist();

            // Subscribe to the "None" option. When the user confirms it, clear the
            // current playlist so the UI switches to the null-playlist state.
            void HandleLoadNullPlaylist(object? sender, EventArgs e)
            {
                SetPlaylist(null);
            }

            selector.LoadNullPlaylist += HandleLoadNullPlaylist;

            try
            {
                var selected = selector.ShowSelectPlaylistDialog(LangStrings.SelectPlaylistToPlay);

                // If the user confirmed a real playlist, apply it.
                if (selected != null)
                {
                    SetPlaylist(selected);
                }
            }
            finally
            {
                // Always unsubscribe to avoid keeping a reference to the dialog.
                selector.LoadNullPlaylist -= HandleLoadNullPlaylist;
            }
        }

        public void OnSetTargetPlaylist(PlaylistBase playlist)
        {
            SetPlaylist(playlist);
        }

        // ============== Loop Mode ==============
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