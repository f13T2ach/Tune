using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Services.FileDialog;
using Tune.Services.Interfaces;

namespace Tune.ViewModel
{
    // **TODO**
    // 1.
    public partial class MainViewModel : ObservableObject
    {
        // ==== Instance ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicPlaybackController _playback;
        private readonly MusicLyricPresenter _musicLyric;
        private readonly MusicDisplayPresenter _musicDisplay;

        // ==== System Services & Resources ====
        private readonly ITimeService _timeService;   // DI
        private readonly IAudioDeviceService _audioDeviceService;    // DI
        private readonly IFileDialogService _fileDialogService; // DI

        // ==== Instance Resolver for Binding ====
        public PlayerController PlayerControl => _playerController;
        public ListController ListController => _listController;
        public MusicPlaybackController Playback => _playback;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;

        /// <summary>
        /// The music currently loaded into the player.
        /// Forwarded from the shared MusicPlaybackController so that existing
        /// XAML bindings (e.g. MusicDisplay) keep working without modification.
        /// </summary>
        public Music CurrentMusic => _playback.CurrentMusic;

        // ==== System Services ====
        public DateTime CurrentTime => _timeService.CurrentTime;
        public string CurrentDeviceName => _audioDeviceService.DeviceName;


        public MainViewModel(
        IFileDialogService fileDialogService,
        ITimeService timeService,
        IAudioDeviceService audioDeviceService,
        PlayerController playerController,
        ListController listController,
        MusicPlaybackController playback,
        MusicDisplayPresenter musicDisplay,
        MusicLyricPresenter musicLyric)
        {
            _fileDialogService = fileDialogService;
            _timeService = timeService;
            _audioDeviceService = audioDeviceService;
            _playerController = playerController;
            _listController = listController;
            _playback = playback;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;

            // Events registration
            _audioDeviceService.DeviceChanged += OnAudioDeviceChanged;
            _timeService.TimeChanged += OnTimeChanged;

            // Forward CurrentMusic changes from the shared controller
            // so that bindings on MainViewModel stay reactive.
            _playback.PropertyChanged += OnPlaybackPropertyChanged;
        }

        /// <summary>
        /// Forward CurrentMusic change notification from MusicPlaybackController.
        /// </summary>
        private void OnPlaybackPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicPlaybackController.CurrentMusic))
            {
                OnPropertyChanged(nameof(CurrentMusic));
            }
        }

        /// <summary>
        /// Event of Time Changed:
        /// Update UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTimeChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentTime));
        }


        /// <summary>
        /// Event of Audio Device Changed: 
        /// Update UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAudioDeviceChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentDeviceName));
        }

        #region ==== Commands for UI Binding ====
        [RelayCommand] private void Play() => _playerController.Play();
        [RelayCommand] private void Pause() => _playerController.Pause();
        [RelayCommand] private void Seek(double seconds) => _playerController.Seek(seconds);
        [RelayCommand] private void TimelineStartDragging() => _playerController.BeginDrag();

        [RelayCommand]
        private void Stop() => _playback.Stop();

        /// <summary>
        /// OpenFile and Play
        /// </summary>
        [RelayCommand]
        private void OpenFile()
        {
            var filePath = _fileDialogService.OpenMusicFile();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            var result = _playback.PlayMusic(new Music(filePath));
            if (!result.IsSucceeded)
            {
                return;
            }
            _playerController.Play();
        }

        /// <summary>
        /// Unload the current playing music(STOP MUSIC)
        /// Behaviour: load a empty Music();
        /// </summary>
        [RelayCommand]
        public void UnloadCurrentMusic() => _playback.Unload();

        [RelayCommand] public void SetNoLoop() => _listController.OnSetNoLoop();
        [RelayCommand] public void SetLoopStatus() => _listController.OnSetLoopStatus();
        //[RelayCommand] public void SetPlaylist() => _listController.OnSetPlaylist();
        [RelayCommand] public void SetPlaylist() => WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(PlaylistsPage)));
        [RelayCommand] public void NavigateToPlaylist() => WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(CurrentPlaylist)));
        [RelayCommand] public void NavigateToSoundEffect() => WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(SoundEffect)));
        #endregion

        /// <summary>
        /// Dispose this Page
        /// **NOTE**: DO NOT UNLOAD OR DISPOSE HERE
        ///           As _player is global instance (DI).
        /// </summary>
        public void Dispose()
        {
            _audioDeviceService.DeviceChanged -= OnAudioDeviceChanged;
            _timeService.TimeChanged -= OnTimeChanged;
            _playback.PropertyChanged -= OnPlaybackPropertyChanged;
        }


    }
}