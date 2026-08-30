using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Engine;
using Tune.Helpers.FormatConverters;
using Tune.Local;
using Tune.Models.Messages;
using Tune.Models.Music;
using Tune.Models.Results;
using Tune.Services.FileDialog;
using Tune.Services.Interfaces;

namespace Tune.ViewModel
{
    // **TODO**
    // 1. The _waveOut cant response immediately (depends on the buffer size???).
    public partial class MainViewModel : ObservableObject
    {
        // ==== Instance ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicLyricPresenter _musicLyric;
        private readonly MusicDisplayPresenter _musicDisplay;

        [ObservableProperty]
        private Music _currentMusic = new Music();  // Music Instance

        // ==== System Services & Resources ====
        private readonly ITimeService _timeService;   // DI
        private readonly IAudioDeviceService _audioDeviceService;    // DI
        private readonly IFileDialogService _fileDialogService; // DI

        // ==== Event Handlers ====
        private readonly EventHandler<double> _progressHandler;     // Progress Event Handler

        // ==== Instance Resolver for Binding ====
        public PlayerController PlayerControl => _playerController;
        public ListController ListController => _listController;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;   // 若需要公开

        // ==== System Services ===
        public DateTime CurrentTime => _timeService.CurrentTime;
        public string CurrentDeviceName => _audioDeviceService.DeviceName;


        public MainViewModel(
        IFileDialogService fileDialogService,
        ITimeService timeService,
        IAudioDeviceService audioDeviceService,
        PlayerController playerController,
        ListController listController,
        MusicDisplayPresenter musicDisplay,
        MusicLyricPresenter musicLyric)
        {
            _fileDialogService = fileDialogService;
            _timeService = timeService;
            _audioDeviceService = audioDeviceService;
            _playerController = playerController;
            _listController = listController;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;

            // Events registration
            _audioDeviceService.DeviceChanged += OnAudioDeviceChanged;
            _timeService.TimeChanged += OnTimeChanged;
            //_playerController.PropertyChanged += OnPlayerControllerPropertyChanged;

            _progressHandler = (_, seconds) => _musicLyric.UpdateLyricsBySecond(seconds);
            _playerController.ProgressUpdated += _progressHandler;
            _playerController.TotalSecondsChanged += (_, _) => _musicDisplay.SetTotalSeconds(_playerController.TotalSeconds);

            // Update
            UpdateDisplayProperties();
            _musicLyric.UpdateLyricsBySecond(0);
        }

        /// <summary>
        /// Execute the functions of updating UI properties: 
        /// `UpdateMetadata()` is to update the info of music.
        /// `UpdateTotalSeconds()` is to update the total sec of music.
        /// `UpdateCurrentLyrics(0)` is to reset the music queue to 0.
        ///  **NOTE**: This would **RESET** the playing progress
        /// </summary>
        private void UpdateDisplayProperties()
        {
            UpdateMetadata();
            UpdateTotalSeconds();
        }

        /// <summary>
        /// Update Music Intance (Unload the _player first) : 
        /// 1. Instance is EMPTY
        ///    Set IsPlaying = false , set current lyric data null
        /// 2. Instance is VALID
        ///    Load it to player, load to current lyric data
        ///    set volume.
        /// Afterwards, Update and Reset UI.
        /// </summary>
        /// <param name="music"></param>
        private PlayerControllerLoadResult UpdateMusic(Music music)
        {
            if (music == null)
                return new PlayerControllerLoadResult(false);
            PlayerControllerLoadResult result;

            if (!music.isEmpty)
            {
                double volume =
                    UIVolumeToDoubleVolume.Convert(
                        TuneSettings.Default.PlayerVolume);
                result = _playerController.Load(music.MusicPath, volume);
            }
            else
            {
                _playerController.Unload();
                result = new PlayerControllerLoadResult(true);
            }
            if (!result.IsSucceeded)
            {
                return result;
            }

            CurrentMusic = music;
            MusicLyric.UpdateLyric(music.Lyrics);   // It would handle lyric load failure

            MusicDisplay.Update(music);
            MusicLyric.UpdateLyricsBySecond(0);
            UpdateDisplayProperties();
            return result;
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
        private void Stop()
        {
            _playerController.Stop();
            MusicLyric.UpdateLyricsBySecond(0);
        }

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
            var result = UpdateMusic(new Music(filePath));
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
        public void UnloadCurrentMusic()
        {
            CurrentMusic = new Music();
            UpdateMusic(CurrentMusic);
        }

        [RelayCommand] public void SetNoLoop() => _listController.OnSetNoLoop();
        [RelayCommand] public void SetLoopStatus() => _listController.OnSetLoopStatus();
        [RelayCommand] public void SetPlaylist() => _listController.OnSetPlaylist();
        [RelayCommand] public void NavigateToPlaylist() =>WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(CurrentPlaylist)));
        #endregion

        #region ==== UI Update Methods ====
        /// <summary>
        /// Update (from Music Instance) metadatas to UI
        /// </summary>
        private void UpdateMetadata() => MusicDisplay.Update(CurrentMusic);

        /// <summary>
        /// Update total seconds
        /// </summary>
        private void UpdateTotalSeconds() => MusicDisplay.SetTotalSeconds(_playerController.TotalSeconds);
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
            _playerController.ProgressUpdated -= _progressHandler;
            //_playerController.PropertyChanged -= OnPlayerControllerPropertyChanged;
        }


    }
}