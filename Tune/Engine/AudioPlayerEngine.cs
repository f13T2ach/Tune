using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using Tune.Engine.Effects;

namespace Tune.Engine
{
    public class AudioPlayerEngine : IAudioPlayerEngine, IDisposable
    {
        private readonly IAudioEffectChain _effectChain;

        /// <summary>Stored AudioApi value that selects the WaveOut device.</summary>
        internal const string WaveOutApiName = "waveout";

        private IWavePlayer? _output;
        private AudioFileReader? _audioReader;
        private DispatcherTimer? _progressTimer;
        private bool _isPlaying;
        private bool _isDisposed;

        public event EventHandler<double>? ProgressUpdated;

        /// <summary>
        /// Only be triggered when the music has played to its end (not when the user stops the music)
        /// </summary>
        public event EventHandler? PlaybackEnded;

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

                // A filter must not carry its state across a jump, otherwise the
                // equalizer rings for a moment at the new position.
                _effectChain.Reset();

                Debug.WriteLine(_audioReader?.CurrentTime);

                if (wasPlaying) Play(); // Resume playback

                OnPropertyChanged(nameof(CurrentSeconds));
            }
        }

        public double TotalSeconds => _audioReader?.TotalTime.TotalSeconds ?? 0;

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

        /// <summary>
        /// Initializes the engine with the effect chain every file is played through.
        /// </summary>
        /// <param name="effectChain">The chain that owns the volume and the effect stages.</param>
        public AudioPlayerEngine(IAudioEffectChain effectChain)
        {
            _effectChain = effectChain;

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
                _output = CreateOutputDevice();

                // Play through the effect chain instead of handing the raw reader to the device.
                var processor = _effectChain.CreateProcessor(_audioReader);

                // Outside the whole chain, so the limiter covers the effects and the volume too.
                if (Tune.Local.TuneSettings.Default.AudioLimiter)
                    processor = new LimitingSampleProvider(processor);

                _output.Init(new SampleToWaveProvider(processor));
                _output.PlaybackStopped += OnPlaybackStopped;

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
            if (_output == null || _audioReader == null) return;

            // If at end, restart from beginning
            if (CurrentSeconds >= TotalSeconds - 0.1)
                CurrentSeconds = 0;

            _output.Play();
            IsPlaying = true;
            _progressTimer?.Start();
        }

        public void Pause()
        {
            if (_output == null) return;
            _output.Pause();
            IsPlaying = false;
            _progressTimer?.Stop();
        }

        public void Stop()
        {
            if (_output == null || _audioReader == null) return;

            _output.Stop();
            _audioReader.CurrentTime = TimeSpan.Zero;

            IsPlaying = false;
            _progressTimer?.Stop();
            OnPropertyChanged(nameof(CurrentSeconds));
        }

        public void Unload()
        {
            if (_output != null)
            {
                _output.PlaybackStopped -= OnPlaybackStopped;
                _output.Stop();
                _output.Dispose();
                _output = null;
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
            _output?.Dispose();
            _audioReader?.Dispose();
            _output = null;
            _audioReader = null;
        }

        // ==== Private Helpers ====

        /// <summary>
        /// Creates the output device the stored API asks for. The choice is read while
        /// loading a file, so changing it only takes effect after a restart.
        /// </summary>
        internal static IWavePlayer CreateOutputDevice()
        {
            if (UseWaveOut(Tune.Local.TuneSettings.Default.AudioApi))
                return new WaveOut();

            // WasapiOut is marked obsolete in favour of WasapiPlayerBuilder, but the builder
            // only builds asynchronously while this runs during a synchronous file load.
#pragma warning disable CS0618
            return new WasapiOut();
#pragma warning restore CS0618
        }

        /// <summary>True when the stored API name asks for the WaveOut device.</summary>
        internal static bool UseWaveOut(string? apiName) =>
            string.Equals(apiName, WaveOutApiName, StringComparison.OrdinalIgnoreCase);
        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            // End when the music has played to its end
            bool naturalEnd = e.Exception == null
                              && _audioReader != null
                              // tolerate 200ms difference to avoid floating point precision issues
                              // TODO: this might be reported as a bug (e.g. issue: play next song when i clicked STOP button)
                              && _audioReader.CurrentTime >= _audioReader.TotalTime - TimeSpan.FromMilliseconds(200);

            IsPlaying = false;
            _progressTimer?.Stop();
            OnPropertyChanged(nameof(CurrentSeconds));

            if (naturalEnd)
            {
                PlaybackEnded?.Invoke(this, EventArgs.Empty);
            }
        }



        // ==== INotifyPropertyChanged ====

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}