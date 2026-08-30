using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Tune.Engine
{
    public class AudioPlayerEngine : IAudioPlayerEngine, IDisposable
    {
        private WaveOut? _waveOut;
        private AudioFileReader? _audioReader;
        private DispatcherTimer? _progressTimer;
        private bool _isPlaying;
        private bool _isDisposed;

        public event EventHandler<double>? ProgressUpdated;

        // ==== Public Properties ====

        public double CurrentSeconds
        {
            get => _audioReader?.CurrentTime.TotalSeconds ?? 0;
            set
            {
                if (_audioReader == null) return;

                // Clamp value to valid range
                var target = Math.Clamp(value, 0, TotalSeconds);
                var targetTime = TimeSpan.FromSeconds(target);

                // Pause while seeking to avoid glitches if currently playing
                bool wasPlaying = IsPlaying;
                if (wasPlaying) Pause();

                _audioReader.CurrentTime = targetTime;
                Debug.WriteLine(_waveOut?.GetPosition());

                if (wasPlaying) Play(); // Resume playback

                OnPropertyChanged(nameof(CurrentSeconds));
            }
        }

        public double TotalSeconds => _audioReader?.TotalTime.TotalSeconds ?? 0;

        public float Volume
        {
            get => _audioReader?.Volume ?? 0f;
            set
            {
                if (_audioReader != null)
                {
                    _audioReader.Volume = Math.Clamp(value, 0f, 1f);
                    OnPropertyChanged(nameof(Volume));
                }
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    OnPropertyChanged(nameof(IsPlaying));
                }
            }
        }

        public bool IsFileLoaded => _audioReader != null;

        // ==== Constructor ====

        public AudioPlayerEngine()
        {
            _progressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };
            _progressTimer.Tick += (s, e) =>
            {
                if (IsPlaying && _audioReader != null)
                {
                    double current = CurrentSeconds;
                    OnPropertyChanged(nameof(CurrentSeconds));
                    ProgressUpdated?.Invoke(this, current);
                }
            };
        }

        // ==== Public Methods ====

        public void LoadFile(string filePath)
        {
            Unload();

            try
            {
                _audioReader = new AudioFileReader(filePath);
                _waveOut = new WaveOut();
                _waveOut.Init(_audioReader);
                _waveOut.PlaybackStopped += OnPlaybackStopped;

                OnPropertyChanged(nameof(CurrentSeconds));
                OnPropertyChanged(nameof(TotalSeconds));
                OnPropertyChanged(nameof(IsFileLoaded));
                OnPropertyChanged(nameof(IsPlaying));
            }
            catch (Exception ex)
            {
                // Log or handle as needed; rethrow or set error state
                Debug.WriteLine($"LoadFile failed: {ex.Message}");
                throw;
            }
        }

        public void Play()
        {
            if (_waveOut == null || _audioReader == null) return;

            // If at end, restart from beginning
            if (CurrentSeconds >= TotalSeconds - 0.1)
                CurrentSeconds = 0;

            _waveOut.Play();
            IsPlaying = true;
            _progressTimer?.Start();
        }

        public void Pause()
        {
            if (_waveOut == null) return;
            _waveOut.Pause();
            IsPlaying = false;
            _progressTimer?.Stop();
        }

        public void Stop()
        {
            if (_waveOut == null || _audioReader == null) return;

            _waveOut.Stop();
            _audioReader.CurrentTime = TimeSpan.Zero;

            IsPlaying = false;
            _progressTimer?.Stop();
            OnPropertyChanged(nameof(CurrentSeconds));
        }

        public void Unload()
        {
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }

            if (_audioReader != null)
            {
                _audioReader.Dispose();
                _audioReader = null;
            }

            IsPlaying = false;
            _progressTimer?.Stop();

            OnPropertyChanged(nameof(CurrentSeconds));
            OnPropertyChanged(nameof(TotalSeconds));
            OnPropertyChanged(nameof(IsFileLoaded));
        }

        // ==== IDisposable Implementation ====

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Unload(); // Already disposes waveOut and reader

            _progressTimer?.Stop();
            _progressTimer = null;

            // Ensure cleanup (Unload sets them to null, but safe guard)
            _waveOut?.Dispose();
            _audioReader?.Dispose();
            _waveOut = null;
            _audioReader = null;
        }

        // ==== Private Helpers ====

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            IsPlaying = false;
            _progressTimer?.Stop();
            OnPropertyChanged(nameof(CurrentSeconds));
            // Optionally raise event for end-of-track
        }

        // ==== INotifyPropertyChanged ====

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}