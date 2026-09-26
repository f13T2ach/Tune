using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Assets.Localization;
using Tune.Components.UI;
using Tune.Helpers.DialogExtentions;
using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Values.Enums;
using Tune.Values.Static;

namespace Tune.Client.Controllers
{
    public partial class ListController : ObservableObject
    {
        // ==== Playback State ====
        private int _currentIndex = -1;
        private readonly List<Music> _playHistory = new();

        // ==== Observable Properties ====
        [ObservableProperty]
        private PlaylistBase? _currentPlaylist;

        [ObservableProperty]
        private EListPlayStatus _listPlayStatus = EListPlayStatus.NoLoop;

        [ObservableProperty]
        private bool _isPlaylistNull = true;

        [ObservableProperty]
        private bool _isPlaylistEmpty = false;  // Behaviour: Null -> false, Empty -> true, Non-empty -> false

        [ObservableProperty]
        private string _currentPlaylistName = string.Empty;

        [ObservableProperty]
        private string _currentPlaylistNameDisplay = Strings.NO_CONTENT;

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
            // Update display content
            CurrentPlaylistNameDisplay = newValue?.Name ?? Strings.NO_CONTENT;

            if (newValue != null)
            {
                IsPlaylistEmpty = newValue.MusicList.Count == 0;
            }
            else
            {
                IsPlaylistEmpty = false;
            }
        }

        // ==== Playlist Switching Logic ====

        /// <summary>
        /// Set the current playlist. Passing <c>null</c> clears the current playlist
        /// and toggles <see cref="isPlaylistNull"/> to <c>true</c>.
        /// THIS WOULD NOT AUTOMATICALLY CHANGE THE `ListplayStatus` PROPERTY!!!
        /// </summary>
        public void SetPlaylist(PlaylistBase? playlist)
        {
            CurrentPlaylist = playlist;
            // Switching the list starts a fresh playback session.
            _currentIndex = -1;
            _playHistory.Clear();
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

        public void OnSetTargetLoop(EListPlayStatus status)
        {
            ListPlayStatus = status;
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
        // ============== Setter ==============
        /// <summary>
        /// Mark a song as the current one: update the internal index and push it
        /// into the history. Call this whenever a song actually starts playing
        /// (e.g. user clicks a track, playlist playback begins, next/prev resolved).
        /// Empty or unlisted songs are ignored for the index update but still
        /// won't crash; the history dedup rule is shared with <see cref="RecordPlayedSong"/>.
        /// </summary>
        public void SetCurrentSong(Music music)
        {
            if (music == null || music.isEmpty)
                return;

            if (CurrentPlaylist?.MusicList != null)
            {
                int index = CurrentPlaylist.MusicList.IndexOf(music);
                if (index >= 0)
                    _currentIndex = index;
            }

            RecordPlayedSong(music);
        }

        // ============== Getter ==============
        private void GetLoopStatusFromSettings()
        {
            OnSetNoLoop(); // TODO: Load the loop status from settings if needed. Currently defaults to NoLoop.
        }

        public bool IsNoLoopStatus() => ListPlayStatus == EListPlayStatus.NoLoop;

        /// <summary>
        /// Return the next song according to the current loop mode.
        /// Returns <c>null</c> when there is no next song (e.g. NoLoop at the end).
        /// SingleLoop returns the current song itself.
        /// </summary>
        public Music? GetNextSong()
        {
            if (CurrentPlaylist?.MusicList == null || CurrentPlaylist.MusicList.Count == 0)
                return null;

            int count = CurrentPlaylist.MusicList.Count;

            // First call (or right after the list was switched): start from the top.
            if (_currentIndex < 0)
            {
                _currentIndex = 0;
                return CurrentPlaylist.MusicList[0];
            }

            // SingleLoop repeats the current song; the "next" is itself.
            if (ListPlayStatus == EListPlayStatus.SingleLoop)
                return CurrentPlaylist.MusicList[_currentIndex];

            int nextIndex;
            switch (ListPlayStatus)
            {
                case EListPlayStatus.NoLoop:
                    nextIndex = _currentIndex + 1;
                    if (nextIndex >= count)
                        return null; // End of list: no next song.
                    break;

                case EListPlayStatus.ListLoop:
                    nextIndex = (_currentIndex + 1) % count;
                    break;

                case EListPlayStatus.RandomListPlay:
                    if (count == 1)
                    {
                        nextIndex = 0;
                    }
                    else
                    {
                        do { nextIndex = Random.Shared.Next(count); }
                        while (nextIndex == _currentIndex);
                    }
                    break;

                default:
                    return null;
            }

            _currentIndex = nextIndex;
            return CurrentPlaylist.MusicList[nextIndex];
        }

        /// <summary>
        /// Return the previously played song using the play history.
        /// Returns <c>null</c> when there is no history to go back to.
        /// </summary>
        public Music? GetLastSong()
        {
            if (CurrentPlaylist?.MusicList == null || CurrentPlaylist.MusicList.Count == 0)
                return null;

            // Nothing recorded yet.
            if (_playHistory.Count == 0)
                return null;

            // The current song is the most recent record; remove it so that
            // repeated "last" calls keep moving backwards.
            _playHistory.RemoveAt(_playHistory.Count - 1);

            if (_playHistory.Count == 0)
                return null;

            var target = _playHistory[^1];

            int index = CurrentPlaylist.MusicList.IndexOf(target);
            if (index >= 0)
                _currentIndex = index;

            return target;
        }

        /// <summary>
        /// Record a played song into the history.
        /// Consecutive duplicates are ignored so that pressing "last" once
        /// returns to the song before the current one.
        /// </summary>
        public void RecordPlayedSong(Music music)
        {
            if (music == null)
                return;

            // Skip if the same song is already the most recent record.
            if (_playHistory.Count > 0 && _playHistory[^1] == music)
                return;

            _playHistory.Add(music);
        }


    }
}