using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Components.UI;
using Tune.Helpers;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.NewDialog;
using Tune.Models.Playlist;
using Tune.Persistence.PlaylistStorage;

namespace Tune.ViewModel
{
    /// <summary>
    /// ViewModel for PlaylistsPage UserControl.
    /// </summary>
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

        public ObservableCollection<PlaylistBase> Playlists => _playlistService?.GetAllPlaylists() ?? new ObservableCollection<PlaylistBase>();

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

            // Ignore playlists that are empty or not initialized.
            if (playlist.isEmptyOrNotInited) return;

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


        // ==== Navigation ====
        [RelayCommand]
        public void NavigateToMain() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Main)));

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

            LoadMusicList();
        }

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