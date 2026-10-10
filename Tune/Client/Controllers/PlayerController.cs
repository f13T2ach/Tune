using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tune.Engine;
using Tune.Models.Results;

namespace Tune.Client.Controllers
{
    public partial class PlayerController : ObservableObject
    {
        private readonly IAudioPlayerEngine _engine;

        //private bool _wasPlayingBeforeDrag;

        public event EventHandler? EnginePlaybackToEnd;

        public PlayerController(IAudioPlayerEngine engine)
        {
            _engine = engine;
            _engine.PropertyChanged += OnEnginePropertyChanged;
            _engine.ProgressUpdated += OnEngineProgressUpdated;
            _engine.PlaybackEnded += OnEnginePlaybackToEnd;
        }

        // ==== Observable Properties ====
        [ObservableProperty]
        private bool _isPlaying;

        [ObservableProperty]
        private double _currentSeconds;

        [ObservableProperty]
        private double _totalSeconds;

        [RelayCommand]
        private void TogglePlayPause()
        {
            if (_engine.IsFileLoaded == false)
            {
                IsPlaying = false;
                return;
            }
            if (IsPlaying) Pause();
            else Play();
        }

        public double RemainingSeconds => TotalSeconds - CurrentSeconds;
        public bool IsFileLoaded => _engine.IsFileLoaded;

        // ==== Events ====
        public event EventHandler<double>? ProgressUpdated;
        public event EventHandler? TotalSecondsChanged;

        // ==== Control Methods ====
        /// <summary>
        /// Load Controller to play.
        /// The output level lives in the effect chain, so loading no longer sets a volume.
        /// </summary>
        /// <param name="filePath">Music Path</param>
        public PlayerControllerLoadResult Load(string filePath)
        {
            _engine.Unload();
            try
            {
                _engine.LoadFile(filePath);
            }
            catch (Exception ex)
            {
                return new PlayerControllerLoadResult(false, ex);
            }
            return new PlayerControllerLoadResult(true);
        }

        /// <summary>
        /// Unload
        /// </summary>
        public void Unload()
        {
            _engine.Unload();
            IsPlaying = false;
            CurrentSeconds = 0;
            TotalSeconds = 0;
        }

        /// <summary>
        /// Play
        /// </summary>
        public void Play()
        {
            if (!_engine.IsFileLoaded)
            {
                return;
            }

            _engine.Play();
        }

        /// <summary>
        /// Pause
        /// </summary>
        public void Pause()
        {
            if (!_engine.IsFileLoaded) return;
            _engine.Pause();
        }

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            _engine.Stop();
            IsPlaying = false;
            CurrentSeconds = 0;
        }



        /// <summary>
        /// Change the progress of music
        /// Excecute after the progress has been changed and the timeline has been released.
        /// </summary>
        /// <param name="seconds"></param>
        public void Seek(double seconds)
        {
            seconds = Math.Clamp(seconds, 0, TotalSeconds);
            //_engine.Pause();
            _engine.CurrentSeconds = seconds;
            // Auto Play.
            // **NOTE**: We can delete this to let user manually play after adjusting the progress
            _engine.Play();

            ProgressUpdated?.Invoke(this, seconds);
        }



        /// <summary>
        /// The event of Timeline start dragging
        /// This would pause the timeline.
        /// However, this is not the best solution: 
        /// 
        /// We can set a flag to remind the time line
        /// not to change itself by _player when the 
        /// timeline is been dragging. 
        /// </summary>
        public void BeginDrag() => _engine.Pause();

        // ==== Events Receiver ====
        private void OnEnginePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AudioPlayerEngine.IsPlaying):
                    IsPlaying = _engine.IsPlaying;
                    break;
                case nameof(AudioPlayerEngine.CurrentSeconds):
                    CurrentSeconds = _engine.CurrentSeconds;
                    OnPropertyChanged(nameof(RemainingSeconds));
                    break;
                case nameof(AudioPlayerEngine.TotalSeconds):
                    TotalSeconds = _engine.TotalSeconds;
                    OnPropertyChanged(nameof(RemainingSeconds));
                    TotalSecondsChanged?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private void OnEngineProgressUpdated(object? sender, double seconds)
        {
            // Sync CurrentSeconds
            CurrentSeconds = seconds;
            OnPropertyChanged(nameof(RemainingSeconds));
            ProgressUpdated?.Invoke(this, seconds);
        }

        partial void OnCurrentSecondsChanged(double value)
        {
            // `_player.CurrentSeconds = value;` is meaningless here.
            OnPropertyChanged(nameof(RemainingSeconds));
        }

        private void OnEnginePlaybackToEnd(object? sender, EventArgs e)
        {
            // Forward to UI (e.g. switch to the next song, change the button back to the playing state, etc.)
            EnginePlaybackToEnd?.Invoke(this, EventArgs.Empty);

        }


        public void Dispose()
        {
            _engine.PropertyChanged -= OnEnginePropertyChanged;
            _engine.ProgressUpdated -= OnEngineProgressUpdated;
        }
    }
}

