using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Components.UI;
using Tune.Models.Dialog;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Tests.Helpers.Mock;
using Tune.Values.Static;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace Tune.ViewModel
{
    /// <summary>
    /// ViewModel for CurrentPlaylist UserControl.
    /// Shares PlayerController, ListController and Presenters with MainViewModel.
    /// </summary>
    public partial class CurrentPlaylistViewModel : ObservableObject
    {
        // ==== Shared ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicPlaybackController _playback;
        private readonly MusicDisplayPresenter _musicDisplay;
        private readonly MusicLyricPresenter _musicLyric;

        /// <summary>
        /// Explicitly bound playlist passed at construction time.
        /// When null, this view model falls back to ListController.CurrentPlaylist.
        /// </summary>
        private readonly PlaylistBase? _explicitPlaylistBase;

        // ==== Public Properties ====
        public PlayerController PlayerController => _playerController;
        public ListController ListController => _listController;
        public MusicPlaybackController Playback => _playback;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;

        /// <summary>
        /// The playlist displayed by this view model.
        /// Uses the explicitly provided playlist when available,
        /// otherwise tracks ListController.CurrentPlaylistBase.
        /// </summary>
        private PlaylistBase? SourcePlaylistBase => _explicitPlaylistBase ?? _listController.CurrentPlaylist;

        /// <summary>
        /// Observable music list for the current playlist.
        /// Kept in sync with ListController.CurrentPlaylistBase.MusicList.
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<Music> _musicList = new();

        // ==== Player Commands ====
        [RelayCommand]
        private void Seek(double seconds) => _playerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => _playerController.BeginDrag();

        // ==== MusicItem Commands ====
        [RelayCommand]
        private void PlayNext(Music? music)
        {
            if (music == null) return;
            // TODO: Insert this music as the next track in the play queue.
        }

        [RelayCommand]
        private void AddToPlaylistBase(Music? music)
        {
            if (music == null) return;
            // TODO: Show playlist selector and add the music to the chosen playlist.
        }

        [RelayCommand]
        private void More(Music? music)
        {
            if (music == null) return;
            // TODO: Show detailed information about this music.
        }

        [RelayCommand]
        private void Delete(Music? music)
        {
            if (music == null) return;

            var playlist = SourcePlaylistBase;
            if (playlist == null) return;

            // Remove from observable collection (UI updates automatically)
            if (MusicList.Remove(music))
            {
                // Also remove from the underlying playlist model
                playlist.MusicList.Remove(music);
                playlist.SavePlaylist();

                // Update ListController state only when this is the active playlist,
                // so bindings (count, empty flag) refresh consistently.
                if (ReferenceEquals(playlist, _listController.CurrentPlaylist))
                {
                    _listController.CurrentPlaylistCount = MusicList.Count;
                    _listController.IsPlaylistEmpty = MusicList.Count == 0;
                }
            }
        }

        [RelayCommand]
        private void ClickItem(Music? music)
        {
            if (music == null) return;

            if (music.notExists)
            {
                MessageBox.Show("Music not exists, unable to play it.", Strings.APPLICATION_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // TODO: Switch ListController.CurrentPlaylist to SourcePlaylist before playing,
            // so that global bindings stay aligned with this view model.

            // Unified entry point: switch the current playing music through
            // the shared MusicPlaybackController so every page stays in sync.
            var result = _playback.PlayMusic(music);
            if (result.IsSucceeded)
            {
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
            var playlist = SourcePlaylistBase;
            if (playlist != null)
            {
                var control = new AddMusic(_playback.CurrentMusic);
                var window = new DialogInstance<object?, Music?>(control);
                var result = window.ShowDialog(null);
                if (result != null && !result.notExists)
                {
                    playlist.AddMuisc(result);
                    playlist.SavePlaylist();

                    // Refresh the observable collection
                    LoadMusicList();

                    // Keep ListController state in sync so bindings refresh immediately
                    if (ReferenceEquals(playlist, _listController.CurrentPlaylist))
                    {
                        _listController.CurrentPlaylistCount = MusicList.Count;
                        _listController.IsPlaylistEmpty = MusicList.Count == 0;
                    }
                }
                ;
            }
        }

        // ==== Constructor ====
        public CurrentPlaylistViewModel(
            PlayerController playerController,
            ListController listController,
            MusicPlaybackController playback,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric,
            PlaylistBase? playlist = null)
        {
            _playerController = playerController;
            _listController = listController;
            _playback = playback;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;
            _explicitPlaylistBase = playlist;

            // Keep MusicList in sync when the current playlist changes
            _listController.PropertyChanged += OnListControllerPropertyChanged;

            // Initial load
            LoadMusicList();
        }

        private void OnListControllerPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Only track ListController.CurrentPlaylist when no explicit playlist was provided.
            if (_explicitPlaylistBase == null && e.PropertyName == nameof(ListController.CurrentPlaylist))
            {
                LoadMusicList();
            }
        }

        private void LoadMusicList()
        {
            var playlist = SourcePlaylistBase;
            if (playlist?.MusicList != null)
            {
                MusicList = new ObservableCollection<Music>(playlist.MusicList);
            }
            else
            {
                MusicList = new ObservableCollection<Music>();
            }
        }

        public void Dispose()
        {
            _listController.PropertyChanged -= OnListControllerPropertyChanged;
        }
    }
}