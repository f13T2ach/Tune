using System;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Engine.Audio;

namespace Tune.Client.Presenters
{
    /// <summary>
    /// The one place where the audio levels meet the interface. It takes what the engine reports on
    /// the audio thread, moves it to the interface thread and exposes it as plain observable values,
    /// so no control has to know anything about the player.
    /// </summary>
    /// <remarks>
    /// The level of the current block and the numbers of a sampling window move at different
    /// speeds on purpose: the bar follows every block, while the average band and the clip
    /// indicator only change when a window closes.
    /// </remarks>
    public partial class AudioLevelPresenter : ObservableObject
    {
        /// <summary>What silence reads as, so the meter never works with infinity.</summary>
        public const double SilenceDecibels = MeteringSampleProvider.SilenceDecibels;

        private readonly Dispatcher _dispatcher;

        private IAudioLevelSource? _source;

        [ObservableProperty]
        private double _leftLevel = SilenceDecibels;

        [ObservableProperty]
        private double _rightLevel = SilenceDecibels;

        [ObservableProperty]
        private double _leftAverage = SilenceDecibels;

        [ObservableProperty]
        private double _rightAverage = SilenceDecibels;

        [ObservableProperty]
        private double _leftPeak = SilenceDecibels;

        [ObservableProperty]
        private double _rightPeak = SilenceDecibels;

        [ObservableProperty]
        private bool _isClipping;

        /// <summary>Gets or sets the output ceiling in decibels; zero leaves the sound alone.</summary>
        [ObservableProperty]
        private double _outputLimitDecibels = Tune.Local.TuneSettings.Default.OutputLimitDb;

        /// <summary>Gets or sets a value indicating whether the output is limited.</summary>
        [ObservableProperty]
        private bool _isOutputLimited = Tune.Local.TuneSettings.Default.OutputLimitEnabled;

        /// <summary>The loudest left peak of the last sampling window, which colours the clip strip.</summary>
        [ObservableProperty]
        private double _leftWindowPeak = SilenceDecibels;

        /// <summary>The loudest right peak of the last sampling window.</summary>
        [ObservableProperty]
        private double _rightWindowPeak = SilenceDecibels;

        /// <summary>Gets the frames of the last block as one mono channel, for a scope or a spectrogram.</summary>
        public ReadOnlyMemory<float> Waveform { get; private set; } = ReadOnlyMemory<float>.Empty;

        /// <summary>Gets the sample rate of the stream the waveform came from.</summary>
        public int SampleRate { get; private set; }

        /// <summary>Gets the level of each spectrum band, in decibels, from low to high.</summary>
        [ObservableProperty]
        private IReadOnlyList<float> _spectrumBands = Array.Empty<float>();

        public AudioLevelPresenter()
        {
            // The presenter is built on the interface thread, so this is the thread to post back to.
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Takes the levels from the given source, replacing any earlier one. Passing null stops
        /// the listening, which is what an unloaded player should do.
        /// </summary>
        public void Attach(IAudioLevelSource? source, Tune.Engine.Effects.IAudioEffectChain? chain = null)
        {
            if (_source != null)
                _source.LevelReported -= OnLevelReported;

            _source = source;

            if (_source != null)
                _source.LevelReported += OnLevelReported;

            // The chain is what can actually limit, so the two settings are pushed to it here and
            // whenever they change.
            _chain = chain ?? _source as Tune.Engine.Effects.IAudioEffectChain;

            if (_chain != null)
            {
                _chain.OutputLimitDecibels = OutputLimitDecibels;
                _chain.IsOutputLimited = IsOutputLimited;
            }
        }

        private Tune.Engine.Effects.IAudioEffectChain? _chain;

        partial void OnOutputLimitDecibelsChanged(double value)
        {
            if (_chain != null) _chain.OutputLimitDecibels = value;

            Tune.Local.TuneSettings.Default.OutputLimitDb = value;
            Tune.Local.TuneSettings.Default.Save();
        }

        partial void OnIsOutputLimitedChanged(bool value)
        {
            if (_chain != null) _chain.IsOutputLimited = value;

            Tune.Local.TuneSettings.Default.OutputLimitEnabled = value;
            Tune.Local.TuneSettings.Default.Save();
        }

        /// <summary>Forgets the highest level so far, which belongs to the track being played.</summary>
        public void ResetPeaks()
        {
            LeftPeak = SilenceDecibels;
            RightPeak = SilenceDecibels;

            LeftLevel = SilenceDecibels;
            RightLevel = SilenceDecibels;

            _leftPeaks.Clear();
            _rightPeaks.Clear();
        }

        /// <summary>How fast the bar falls back, in decibels per second.</summary>
        private const double ReleaseDecibelsPerSecond = 26;

        /// <summary>How fast the loudness mark falls back, slower so it does not twitch.</summary>
        private const double LoudnessReleaseDecibelsPerSecond = 8;

        /// <summary>
        /// How long the bar takes to move most of the way to a new peak, in seconds. Shorter is
        /// punchier but twitchier; this is about the pace a hardware meter moves at.
        /// </summary>
        private const double AttackSeconds = 0.03;

        /// <summary>How long a peak stays on the marker before it is forgotten, like OBS's top dot.</summary>
        private static readonly TimeSpan PeakHold = TimeSpan.FromSeconds(20);

        private readonly List<(DateTime Time, double Value)> _leftPeaks = new();
        private readonly List<(DateTime Time, double Value)> _rightPeaks = new();


        /// <summary>
        /// Adds the peak of a window and forgets the ones older than the hold time, so the marker
        /// follows the loudest moment of the last twenty seconds rather than of the whole track.
        /// </summary>
        private static double RollingPeak(List<(DateTime Time, double Value)> peaks, DateTime now, double value)
        {
            peaks.Add((now, value));
            peaks.RemoveAll(entry => now - entry.Time > PeakHold);

            double best = SilenceDecibels;
            foreach ((DateTime _, double held) in peaks)
                best = Math.Max(best, held);

            return best;
        }

        private DateTime _lastUpdate = DateTime.UtcNow;

        /// <summary>
        /// Rises towards the incoming peak and falls at a steady rate. The rise is smoothed rather
        /// than instant: neighbouring block peaks differ by a few decibels, and jumping straight to
        /// each of them is what makes the bar twitch at the top.
        /// </summary>
        /// <param name="shown">The level on screen now.</param>
        /// <param name="target">The level that just arrived.</param>
        /// <param name="seconds">How long since the last update, taken once for both sides.</param>
        /// <param name="release">How fast it falls, in decibels per second.</param>
        private static double Smooth(double shown, double target, double seconds, double release)
        {
            if (target >= shown)
            {
                // An exponential approach with a short time constant: quick enough to follow a hit,
                // slow enough that neighbouring blocks average out.
                double rise = 1 - Math.Exp(-seconds / AttackSeconds);

                return shown + (target - shown) * rise;
            }

            return Math.Max(target, shown - release * seconds);
        }

        /// <summary>Puts everything back to silence when the player lets go of the audio.</summary>
        private void OnLevelsReset(object? sender, EventArgs e)
        {
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () =>
            {
                LeftLevel = SilenceDecibels;
                RightLevel = SilenceDecibels;
                LeftAverage = SilenceDecibels;
                RightAverage = SilenceDecibels;
                LeftWindowPeak = SilenceDecibels;
                RightWindowPeak = SilenceDecibels;
                IsClipping = false;
                SpectrumBands = Array.Empty<float>();

                ResetPeaks();
            });
        }

        private void OnLevelReported(object? sender, AudioLevelEventArgs e)
        {
            // Report priority lets several blocks of a frame collapse into one update.
            _dispatcher.BeginInvoke(DispatcherPriority.Render, () => Apply(e));
        }

        private void Apply(AudioLevelEventArgs e)
        {
            // The elapsed time is taken once for both sides: it used to be taken inside Smooth,
            // which made the second call always see zero seconds and left the right bar stuck.
            DateTime now = DateTime.UtcNow;
            double seconds = Math.Max(0, (now - _lastUpdate).TotalSeconds);
            _lastUpdate = now;

            // A rise is followed at once, while a fall is limited to a fixed rate per second. That
            // is what keeps the bar from twitching the way a raw per block value does.
            // Like the meter this follows: the bar is the peak, which falls away at a fixed rate,
            // while the mark inside it is the live loudness, so it moves with the sound instead of
            // only once per sampling window.
            Waveform = e.Frames;
            SampleRate = e.SampleRate;

            if (e.Bands.Count > 0)
            {
                // A fresh array each time: the analyser mutates its own, and a binding only reacts
                // when the value it sees actually changes.
                var copy = new float[e.Bands.Count];

                for (int i = 0; i < copy.Length; i++)
                    copy[i] = e.Bands[i];

                SpectrumBands = copy;
            }

            LeftLevel = Smooth(LeftLevel, e.PeakLeft, seconds, ReleaseDecibelsPerSecond);
            RightLevel = Smooth(RightLevel, e.PeakRight, seconds, ReleaseDecibelsPerSecond);

            LeftAverage = Smooth(LeftAverage, e.Left, seconds, LoudnessReleaseDecibelsPerSecond);
            RightAverage = Smooth(RightAverage, e.Right, seconds, LoudnessReleaseDecibelsPerSecond);

            if (!e.Window.HasValue)
                return;

            LevelWindowResult window = e.Window.Value;



            LeftWindowPeak = window.MaxLeft;
            RightWindowPeak = window.MaxRight;

            LeftPeak = RollingPeak(_leftPeaks, now, window.MaxLeft);
            RightPeak = RollingPeak(_rightPeaks, now, window.MaxRight);

            // Clipping means the window reached full scale, not that it entered the red band:
            // mastered music sits in that band all the time without ever clipping.
            // Reaching full scale is normal for a mastered track; only going past it means the
            // signal would have clipped without the limiter.
            IsClipping = Math.Max(window.MaxLeft, window.MaxRight) > 0;
        }
    }
}
