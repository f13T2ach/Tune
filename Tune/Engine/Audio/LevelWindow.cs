using System;

namespace Tune.Engine.Audio
{
    /// <summary>What a closed sampling window reports: its average and its maximum, both in dB.</summary>
    /// <param name="AverageLeft">The time weighted mean of the left peaks of the window.</param>
    /// <param name="AverageRight">The time weighted mean of the right peaks of the window.</param>
    /// <param name="MaxLeft">The loudest left peak of the window.</param>
    /// <param name="MaxRight">The loudest right peak of the window.</param>
    public readonly record struct LevelWindowResult(
        double AverageLeft,
        double AverageRight,
        double MaxLeft,
        double MaxRight);

    /// <summary>
    /// Collects the peaks of the blocks that belong to one sampling window and reports them when
    /// the window closes. The bar does not wait for this: it follows every block. Only the average
    /// band and the clip indicator are decided by a window.
    /// </summary>
    /// <remarks>
    /// The window is measured in time rather than in blocks, because the output pulls whatever
    /// block size it likes. The average is the mean of the block peaks in decibels weighted by how
    /// long each block lasted; that is not an average of the samples themselves, but it is what the
    /// band is meant to show and it keeps the audio callback cheap.
    /// </remarks>
    public sealed class LevelWindow
    {
        /// <summary>
        /// How long the meter's sampling window lasts. Change this one value to make the average
        /// band and the clip indicator move faster or slower.
        /// </summary>
        public static readonly TimeSpan DefaultWindow = TimeSpan.FromSeconds(2);

        private readonly TimeSpan _window;

        private TimeSpan _elapsed;
        private double _sumLeft;
        private double _sumRight;
        private double _maxLeft = double.NegativeInfinity;
        private double _maxRight = double.NegativeInfinity;

        /// <param name="window">How long one sampling window lasts.</param>
        public LevelWindow(TimeSpan window)
        {
            if (window <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(window));

            _window = window;
        }

        /// <summary>Gets how long one sampling window lasts.</summary>
        public TimeSpan Window => _window;

        /// <summary>
        /// Adds one block. While the window is still open this answers null; on the block that
        /// completes it, it answers the window's average and maximum and starts a new window.
        /// </summary>
        /// <param name="peakLeft">The left peak of the block, in decibels below full scale.</param>
        /// <param name="peakRight">The right peak of the block, in decibels below full scale.</param>
        /// <param name="seconds">How long the block lasted.</param>
        public LevelWindowResult? Add(double peakLeft, double peakRight, double seconds)
        {
            if (seconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(seconds));

            _elapsed += TimeSpan.FromSeconds(seconds);
            _sumLeft += peakLeft * seconds;
            _sumRight += peakRight * seconds;
            _maxLeft = Math.Max(_maxLeft, peakLeft);
            _maxRight = Math.Max(_maxRight, peakRight);

            if (_elapsed < _window)
                return null;

            double total = _elapsed.TotalSeconds;

            var result = new LevelWindowResult(
                _sumLeft / total,
                _sumRight / total,
                _maxLeft,
                _maxRight);

            _elapsed = TimeSpan.Zero;
            _sumLeft = 0;
            _sumRight = 0;
            _maxLeft = double.NegativeInfinity;
            _maxRight = double.NegativeInfinity;

            return result;
        }
    }
}
