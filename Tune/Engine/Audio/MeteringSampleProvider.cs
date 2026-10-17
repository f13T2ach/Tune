using System;

using NAudio.Wave;

namespace Tune.Engine.Audio
{
    /// <summary>What one block measured, and the window that closed with it when one did.</summary>
    public sealed class AudioLevelEventArgs : EventArgs
    {
        public AudioLevelEventArgs(
            double left,
            double right,
            double peakLeft,
            double peakRight,
            LevelWindowResult? window)
        {
            Left = left;
            Right = right;
            PeakLeft = peakLeft;
            PeakRight = peakRight;
            Window = window;
        }

        /// <summary>Gets the left peak of the block, in decibels below full scale.</summary>
        public double Left { get; }

        /// <summary>Gets the right peak of the block, in decibels below full scale.</summary>
        public double Right { get; }

        /// <summary>Gets the left peak of the block, which is what clipping is judged on.</summary>
        public double PeakLeft { get; }

        /// <summary>Gets the right peak of the block, which is what clipping is judged on.</summary>
        public double PeakRight { get; }

        /// <summary>Gets the closed sampling window, or null while the window is still open.</summary>
        public LevelWindowResult? Window { get; }
    }

    /// <summary>
    /// Sits in the audio path, measures every block and passes it on untouched. The peaks are
    /// reported per block so the bar can follow the sound, while the average and the maximum are
    /// only reported when a sampling window closes.
    /// </summary>
    public sealed class MeteringSampleProvider : ISampleProvider, IAudioLevelSource
    {
        /// <summary>Reported for silence, which has no decibel value of its own.</summary>
        public const double SilenceDecibels = -100;

        private readonly ISampleProvider _source;
        private readonly LevelWindow _window;

        /// <param name="source">The provider to measure; it is read through unchanged.</param>
        /// <param name="window">How long a sampling window lasts, or null for the meter's default.</param>
        public MeteringSampleProvider(ISampleProvider source, TimeSpan? window = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _window = new LevelWindow(window ?? LevelWindow.DefaultWindow);
        }

        /// <summary>Gets the format of the source, which is read through unchanged.</summary>
        public WaveFormat WaveFormat => _source.WaveFormat;

        /// <summary>Gets how long one sampling window lasts.</summary>
        public TimeSpan Window => _window.Window;

        /// <inheritdoc/>
        public event EventHandler<AudioLevelEventArgs>? LevelReported;

        /// <inheritdoc/>
        public int Read(Span<float> buffer)
        {
            int read = _source.Read(buffer);

            if (read <= 0)
                return read;

            int channels = Math.Max(1, WaveFormat.Channels);
            double sumLeft = 0;
            double sumRight = 0;
            double peakLeft = 0;
            double peakRight = 0;
            int frames = 0;

            for (int i = 0; i < read; i += channels)
            {
                double left = buffer[i];

                // A mono source is heard on both sides, so both sides get the same sample.
                double right = channels > 1 && i + 1 < read ? buffer[i + 1] : left;

                peakLeft = Math.Max(peakLeft, Math.Abs(left));
                peakRight = Math.Max(peakRight, Math.Abs(right));

                sumLeft += left * left;
                sumRight += right * right;
                frames++;
            }

            // The bar shows the loudness of the block, which is why a bass note no longer throws it
            // to the top; the peak is kept for the clip indicator and the peak marker.
            double rmsLeft = frames == 0 ? 0 : Math.Sqrt(sumLeft / frames);
            double rmsRight = frames == 0 ? 0 : Math.Sqrt(sumRight / frames);

            double seconds = (double)read / channels / WaveFormat.SampleRate;

            LevelWindowResult? closed = _window.Add(ToDecibels(peakLeft), ToDecibels(peakRight), seconds);
            LevelReported?.Invoke(this, new AudioLevelEventArgs(
                ToDecibels(rmsLeft),
                ToDecibels(rmsRight),
                ToDecibels(peakLeft),
                ToDecibels(peakRight),
                closed));
            return read;
        }

        /// <summary>Reads a slice of an array, which is the form older callers use.</summary>
        public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <summary>Turns a linear peak into decibels, keeping silence at a floor far below the scale.</summary>
        /// <param name="peak">The linear peak, from zero to one.</param>
        public static double ToDecibels(double peak) =>
            peak <= 0 ? SilenceDecibels : 20 * Math.Log10(peak);
    }
}
