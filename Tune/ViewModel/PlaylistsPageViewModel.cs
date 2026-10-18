using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Tune.Client.Controllers;
using Tune.Components.UI;
using Tune.Helpers;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.NewDialog;
using Tune.Models.Playlist;
using Tune.Persistence.PlaylistStorage;
using Tune.Helpers.DialogExtentions;

namespace Tune.ViewModel
{
    public partial class PlaylistsPageViewModel : ObservableObject
    {
        private readonly ListController _listController;
        private readonly MusicPlaybackController _playback;
        private readonly PlaylistService? _playlistService;

        /// <summary>
        /// Explicitly bound playlist passed at construction time.
        /// When null, falls back to ListController.CurrentPlaylist.
        /// </summary>
        private readonly PlaylistBase? _explicitPlaylist;

        // ==== Public Properties ====
        public ListController ListController => _listController;
        public MusicPlaybackController Playback => _playback;

        /// <summary>
        /// The playlist displayed by this view model.
        /// Uses the explicitly provided playlist when available,
        /// otherwise tracks ListController.CurrentPlaylist.
        /// </summary>
        private PlaylistBase? SourcePlaylist => _explicitPlaylist ?? _listController.CurrentPlaylist;

        /// <summary>
        /// Observable music list for the displayed playlist.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Music> _musicList = new();

        /// <summary>
        /// The playlists shown by the page, with the empty cover that creates one kept last.
        /// </summary>
        public ObservableCollection<PlaylistBase> Playlists { get; } = new();

        /// <summary>
        /// Attempts to play the specified playlist.
        /// It switches the current playlist, sets the loop mode to list loop,
        /// selects the first playable track, and starts playback when playback succeeds.
        /// </summary>
        /// <param name="playlist">
        /// The playlist to play. Null, uninitialized, empty, or all-missing playlists will not start playback.
        /// </param>
        /// <remarks>
        /// The current playlist and loop mode are changed before verifying that a playable track exists.
        /// If invalid playlists should not change playback state, validation should be moved before SetPlaylist.
        /// </remarks>
        [RelayCommand]
        private void PlayPlaylist(PlaylistBase? playlist)
        {
            // Ignore a null playlist.
            if (playlist is null) return;

            // The empty cover is the way to stop: nothing plays, and the player falls back to
            // the history playlist so the user is not left with no playlist at all.
            if (playlist.isEmptyOrNotInited)
            {
                _listController.ClearPlaylist();
                _playback?.Unload();
                return;
            }

            // Get the playlist's music list.
            var musicList = playlist.MusicList;
            if (musicList is null) return;

            // Switch the current playlist and set the loop mode to list loop.
            _listController.SetPlaylist(playlist);
            _listController.OnSetTargetLoop(Values.Enums.EListPlayStatus.ListLoop);

            // Do not continue if the playlist has no tracks.
            if (musicList.Count == 0)
                return;

            // Find the first track that still exists.
            var first = musicList.FirstOrDefault(m => !m.notExists);
            if (first is null) return;

            // Ask the playback service to play/load the selected track.
            var result = _playback.PlayMusic(first);
            if (result.IsSucceeded)
            {
                // Start/resume playback after a successful PlayMusic request.
                _playback.Play();
            }
        }


        /// <summary>
        /// Opens a playlist in the list page. That page follows the current playlist, so
        /// it shows the one that was clicked, and its back button leaves the player alone.
        /// </summary>
        /// <param name="playlist">The playlist to open.</param>
        [RelayCommand]
        private void OpenPlaylist(PlaylistBase? playlist)
        {
            if (playlist is null) return;

            // The empty cover is the way to a new playlist, not a playlist to look at.
            if (playlist.isEmptyOrNotInited)
            {
                CreatePlaylist();
                return;
            }

            _listController.SetPlaylist(playlist);
            WeakReferenceMessenger.Default.Send(new NavigationMessage(PageKind.CurrentPlaylist));

            // That navigation builds the list page. Tell it where its back button belongs,
            // so leaving it comes back here instead of going to the main page.
            CurrentPlaylistViewModel.ReturnPageKind = PageKind.Playlists;
        }

        /// <summary>
        /// Creates a playlist through the same dialog the rest of the application uses, and
        /// opens it once it exists. This is what the empty cover at the end of the list
        /// runs.
        /// </summary>
        [RelayCommand]
        private void CreatePlaylist()
        {
            var created = CreatePlaylistDialog();
            if (created is null) return;

            // The new playlist has to be in the list that the page shows, and the empty
            // cover has to stay last.
            RefreshPlaylists();
            OpenPlaylist(created);
        }

        /// <summary>
        /// Runs the new playlist dialog. A test replaces it, so the routing to it can be
        /// checked without opening a modal dialog.
        /// </summary>
        internal Func<PlaylistBase?> CreatePlaylistDialog { get; set; } = ShowCreatePlaylistDialog;

        private static PlaylistBase? ShowCreatePlaylistDialog()
        {
            var creator = new CreatePlaylist();

            return creator.ShowCreatePlaylistDialog(Tune.Assets.Localization.LangStrings.CreateNewPlaylisyTitle);
        }

        // ==== Navigation ====
        [RelayCommand]
        public void NavigateToMain() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage(PageKind.Main));

        [RelayCommand]
        public void AddMusicClicked()
        {
            var playlist = SourcePlaylist;
            if (playlist == null) return;

            /*
             * Old dialog
            var control = new AddMusic(_playback.CurrentMusic);
            var window = new DialogInstance<object?, Music?>(control);
            var result = window.ShowDialog(null);*/
            var dlg = new AddMusic(_playback.CurrentMusic);
            var result = dlg.ShowDialog() ? dlg.Result : null;
            if (result == null || result.notExists) return;

            playlist.AddMuisc(result);
            playlist.SavePlaylist();

            LoadMusicList();

            // Sync ListController state only when this is the active playlist.
            if (ReferenceEquals(playlist, _listController.CurrentPlaylist))
            {
                _listController.CurrentPlaylistCount = MusicList.Count;
                _listController.IsPlaylistEmpty = MusicList.Count == 0;
            }
        }

        // ==== Constructor ====
        public PlaylistsPageViewModel(
            ListController listController,
            MusicPlaybackController playback,
            PlaylistBase? playlist = null)
        {
            _listController = listController;
            _playback = playback;
            _explicitPlaylist = playlist;

            _listController.PropertyChanged += OnListControllerPropertyChanged;
            _playlistService = LoadableServicesHelper.GetPlaylistService();

            RefreshPlaylists();
            LoadMusicList();
        }

        /// <summary>
        /// Rebuilds the shown playlists and appends the empty cover, which is why it is
        /// always the last item no matter how many playlists there are.
        /// </summary>
        private void RefreshPlaylists()
        {
            Playlists.Clear();

            if (_playlistService != null)
            {
                foreach (PlaylistBase playlist in _playlistService.GetAllPlaylists())
                {
                    // The history playlist has its own button and is never listed here.
                    if (ListController.IsHistory(playlist)) continue;

                    Playlists.Add(playlist);
                }
            }

            Playlists.Add(CreateEmptyCover());
        }

        /// <summary>
        /// The cover at the end of the list. It is an empty playlist on purpose: opening it
        /// starts a new playlist, and playing it leaves the player with no playlist at all.
        /// </summary>
        private static PlaylistBase CreateEmptyCover() => new CollectionPlaylist
        {
            Name = Tune.Assets.Localization.LangStrings.PlaylistEmptyCoverName,
            Description = Tune.Assets.Localization.LangStrings.NewFromCover
        };

        private void OnListControllerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Only track ListController.CurrentPlaylist when no explicit playlist was provided.
            if (_explicitPlaylist == null && e.PropertyName == nameof(ListController.CurrentPlaylist))
            {
                LoadMusicList();
            }
        }

        private void LoadMusicList()
        {
            var playlist = SourcePlaylist;
            MusicList = playlist?.MusicList != null
                ? new ObservableCollection<Music>(playlist.MusicList)
                : new ObservableCollection<Music>();
        }

        public void Dispose()
        {
            _listController.PropertyChanged -= OnListControllerPropertyChanged;
        }
    }
}