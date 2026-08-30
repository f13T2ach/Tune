using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace Tune.Engine
{
    public class AudioPlayerEngine : IAudioPlayerEngine, IDisposable
    {

        // ===== Private Fields =====
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioReader;
        private DispatcherTimer? _progressTimer;
        private bool _isPlaying;
        private bool _isDisposed;

        public event EventHandler<double>? ProgressUpdated;

        // ===== Public Attributes =====

        /// <summary> Current Seconds </summary>
        public double CurrentSeconds
        {
            get => _audioReader?.CurrentTime.TotalSeconds ?? 0;
            set
            {
                if (_audioReader == null) return;
                // 避免无效值
                //var target = Math.Max(0, Math.Min(TotalSeconds, value));
                //var targetTime = TimeSpan.FromSeconds(target);

                // 如果正在播放，先暂停再跳转（防止卡顿/杂音）
                //bool wasPlaying = IsPlaying;
                //if (wasPlaying) Pause();

                _audioReader.CurrentTime = TimeSpan.FromSeconds(value);
                Debug.WriteLine(_waveOut?.GetPosition());
                //Debug.WriteLine(_audioReader.Position);

                //if (wasPlaying) Play();

                OnPropertyChanged(nameof(CurrentSeconds));
            }
        }

        /// <summary> Total Seconds </summary>
        public double TotalSeconds => _audioReader?.TotalTime.TotalSeconds ?? 0;

        /// <summary> Volume (0~1 Format) </summary>
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

        /// <summary> Is Playing </summary>
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

        /// <summary> Is File Loaded </summary>
        public bool IsFileLoaded => _audioReader != null;

        // ===== Constructor =====
        public AudioPlayerEngine()
        {
            _progressTimer = new DispatcherTimer();
            _progressTimer.Interval = TimeSpan.FromMilliseconds(50); // 高精度
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

        // ===== Public Methods =====

        /// <summary> Loda Music File </summary>
        public void LoadFile(string filePath)
        {
            // Release
            Unload();

            _audioReader = new AudioFileReader(filePath);
            _waveOut = new WaveOutEvent();
            _waveOut.Init(_audioReader);
            _waveOut.PlaybackStopped += OnPlaybackStopped;

            // Call every `PropertyChanged` event
            OnPropertyChanged(nameof(CurrentSeconds));
            OnPropertyChanged(nameof(TotalSeconds));
            OnPropertyChanged(nameof(IsFileLoaded));
            OnPropertyChanged(nameof(IsPlaying));
        }

        /// <summary> Play </summary>
        public void Play()
        {
            if (_waveOut == null || _audioReader == null) return;

            //_waveOut = new WaveOutEvent();
            //_waveOut.Init(_audioReader);
            //_waveOut.PlaybackStopped += OnPlaybackStopped;

            //_progressTimer?.Stop();
            /*
            if (CurrentSeconds >= TotalSeconds - 0.1)
            {
                CurrentSeconds = 0;
            }*/
            //CurrentSeconds = 8;

            _waveOut.Play();
            IsPlaying = true;
            _progressTimer?.Start();
        }

        /// <summary> Pause </summary>
        public void Pause()
        {
            if (_waveOut == null) return;
            _waveOut.Pause(); // Stack Overflow
            IsPlaying = false;
            _progressTimer.Stop();
        }

        /// <summary> Stop and Reset </summary>
        public void Stop()
        {
            if (_waveOut == null || _audioReader == null)
                return;


            _waveOut.Stop();

            // Back to first time
            _audioReader.CurrentTime = TimeSpan.Zero;


            IsPlaying = false;

            _progressTimer?.Stop();


            OnPropertyChanged(nameof(CurrentSeconds));
        }

        // Unload Music
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


            OnPropertyChanged(nameof(CurrentSeconds));
            OnPropertyChanged(nameof(TotalSeconds));
            OnPropertyChanged(nameof(IsFileLoaded));
        }

        /// <summary> Dispose resources </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            Unload();

            _progressTimer?.Stop();
            _progressTimer = null;

            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            _audioReader?.Dispose();
            _audioReader = null;
        }

        // ===== Private Helper Methods =====

        /// <summary> Playback Stopped Event Handler </summary>
        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            // 如果播放结束且非人为停止，更新UI状态
            IsPlaying = false;
            _progressTimer.Stop();

            // 如果播放到了结尾，可以触发一个事件供ViewModel处理（例如自动下一首）
            // 这里只更新状态
            OnPropertyChanged(nameof(CurrentSeconds));
        }

        // ===== INotifyPropertyChanged =====

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
