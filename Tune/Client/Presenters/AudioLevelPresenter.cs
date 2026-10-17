using System;
using System.Windows.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Appearance.ExtendView;
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

        /// <summary>The loudest left peak of the last sampling window, which colours the clip strip.</summary>
        [ObservableProperty]
        private double _leftWindowPeak = SilenceDecibels;

        /// <summary>The loudest right peak of the last sampling window.</summary>
        [ObservableProperty]
        private double _rightWindowPeak = SilenceDecibels;

        public AudioLevelPresenter()
        {
            // The presenter is built on the interface thread, so this is the thread to post back to.
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Takes the levels from the given source, replacing any earlier one. Passing null stops
        /// the listening, which is what an unloaded player should do.
        /// </summary>
        public void Attach(IAudioLevelSource? source)
        {
            if (_source != null)
                _source.LevelReported -= OnLevelReported;

            _source = source;

            if (_source != null)
                _source.LevelReported += OnLevelReported;
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
        /// Follows a rise at once and drops back at a steady rate, so a loud block does not throw
        /// the bar up and the next quiet one drop it again.
        /// </summary>
        /// <param name="shown">The level on screen now.</param>
        /// <param name="target">The level that just arrived.</param>
        /// <param name="seconds">How long since the last update, taken once for both sides.</param>
        private static double Smooth(double shown, double target, double seconds, double release) =>
            target >= shown
                ? target
                : Math.Max(target, shown - release * seconds);

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
            IsClipping = Math.Max(window.MaxLeft, window.MaxRight) >= 0;
        }
    }
}
