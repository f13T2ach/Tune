using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Windows;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Components.UI;
using Tune.Helpers;
using Tune.Models.Dialog;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.Playlist;
using Tune.Persistence.PlaylistStorage;
using Tune.Values.Static;

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

        // ==== MusicItem Commands ====
        [RelayCommand]
        private void PlayPlaylist(PlaylistBase? playlist)
        {
            if (playlist is null) return;
            if (playlist.isEmptyOrNotInited) return;

            var musicList = playlist.MusicList;
            if (musicList is null || musicList.Count == 0) return;

            // 第一首可用的音乐（跳过 notExists）
            var first = musicList.FirstOrDefault(m => !m.notExists);
            if (first is null) return;

            // 切换当前歌单
            _listController.CurrentPlaylist = playlist;
            _listController.CurrentPlaylistCount = musicList.Count;
            _listController.IsPlaylistEmpty = false;

            // 播放
            var result = _playback.PlayMusic(first);
            if (result.IsSucceeded)
            {
                _playback.Play();
            }
        }
        /*
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
        }*/

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