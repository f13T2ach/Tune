using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Components.UI;
using Tune.Models.Dialog;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Values.Static;

namespace Tune.ViewModel
{
    /// <summary>
    /// ViewModel for CurrentPlaylist UserControl.
    /// </summary>
    public partial class CurrentPlaylistViewModel : ObservableObject
    {
        private readonly ListController _listController;
        private readonly MusicPlaybackController _playback;

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

        // ==== MusicItem Commands ====
        [RelayCommand]
        private void PlayNext(Music? music)
        {
            if (music == null) return;
            // TODO: Insert this music as the next track in the play queue.
        }

        [RelayCommand]
        private void AddToPlaylist(Music? music)
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

            var playlist = SourcePlaylist;
            if (playlist == null) return;

            if (!MusicList.Remove(music)) return;

            playlist.MusicList.Remove(music);
            playlist.SavePlaylist();

            // Sync ListController state only when this is the active playlist.
            if (ReferenceEquals(playlist, _listController.CurrentPlaylist))
            {
                _listController.CurrentPlaylistCount = MusicList.Count;
                _listController.IsPlaylistEmpty = MusicList.Count == 0;
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

            // TODO: Switch ListController.CurrentPlaylist to SourcePlaylist before playing.

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
            var playlist = SourcePlaylist;
            if (playlist == null) return;

            var control = new AddMusic(_playback.CurrentMusic);
            var window = new DialogInstance<object?, Music?>(control);
            var result = window.ShowDialog(null);
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
        public CurrentPlaylistViewModel(
            ListController listController,
            MusicPlaybackController playback,
            PlaylistBase? playlist = null)
        {
            _listController = listController;
            _playback = playback;
            _explicitPlaylist = playlist;

            _listController.PropertyChanged += OnListControllerPropertyChanged;

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