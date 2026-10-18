using System;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// Splits the signal into logarithmically spaced bands by running one band-pass filter per band
    /// and measuring the loudness of each. It is meant for a display, so the readings are in
    /// decibels, the bands are smoothed, and nothing here allocates once it is built.
    /// </summary>
    /// <remarks>
    /// A filter bank is used rather than an FFT because the numbers are easy to reason about: a tone
    /// at the centre of a band reads about its own level, and a tone far away reads the floor. That
    /// is what the tests check.
    /// </remarks>
    public sealed class SpectrumAnalyzer
    {
        /// <summary>The bottom of the scale a band can read.</summary>
        public const double FloorDecibels = -60;

        /// <summary>How many samples one measurement covers.</summary>
        private const int BlockLength = 1024;

        private readonly Band[] _bands;
        private readonly float[] _latest;
        private readonly double[] _sums;
        private readonly double _releasePerSecond;

        private int _position;

        /// <param name="bandCount">How many bands to split the spectrum into.</param>
        /// <param name="releasePerSecond">How fast a band falls back, in decibels per second.</param>
        /// <param name="sampleRate">The sample rate the stream runs at.</param>
        public SpectrumAnalyzer(int bandCount = 16, double releasePerSecond = 40, int sampleRate = 44100)
        {
            if (bandCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(bandCount));

            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleRate));

            _bands = new Band[bandCount];
            _latest = new float[bandCount];
            _sums = new double[bandCount];
            _releasePerSecond = releasePerSecond;

            for (int i = 0; i < bandCount; i++)
            {
                // 40 Hz up to 16 kHz, spaced logarithmically, which is how the bands are drawn.
                double low = 40 * Math.Pow(16000.0 / 40, (double)i / bandCount);
                double high = 40 * Math.Pow(16000.0 / 40, (double)(i + 1) / bandCount);
                double centre = Math.Sqrt(low * high);

                _bands[i] = new Band(centre / sampleRate, 1.4);
                _latest[i] = (float)FloorDecibels;
            }
        }

        /// <summary>Gets the level of each band, in decibels below full scale, from low to high.</summary>
        public IReadOnlyList<float> Bands => _latest;

        /// <summary>
        /// Adds a block of interleaved samples and measures the bands once a whole measurement window
        /// has been filled.
        /// </summary>
        /// <param name="buffer">The samples that were just read.</param>
        /// <param name="read">How many of them are valid.</param>
        /// <param name="channels">How many channels the buffer interleaves.</param>
        /// <param name="sampleRate">The sample rate of the stream.</param>
        /// <param name="seconds">How long the block lasted.</param>
        public void Add(ReadOnlySpan<float> buffer, int read, int channels, int sampleRate, double seconds)
        {
            int frames = Math.Max(1, channels);

            for (int i = 0; i < read; i += frames)
            {
                // One channel is enough for a display; the meter is about the music, not the sides.
                float value = buffer[i];

                for (int band = 0; band < _bands.Length; band++)
                {
                    double filtered = _bands[band].Process(value);
                    _sums[band] += filtered * filtered;
                }

                if (++_position >= BlockLength)
                {
                    Measure();
                    _position = 0;
                }
            }

            Reveal(seconds);
        }

        /// <summary>Turns the accumulated energy of each band into a level in decibels.</summary>
        private void Measure()
        {
            for (int band = 0; band < _bands.Length; band++)
            {
                double rms = Math.Sqrt(_sums[band] / BlockLength);

                _sums[band] = 0;

                // The filter passes a tone at about its own amplitude, so the RMS is already a fair
                // level; the factor of two puts a full scale sine near zero decibels.
                _bands[band].Level = ToDecibels(rms * 2);
            }
        }

        /// <summary>Moves what is shown towards what was measured, falling back at a steady rate.</summary>
        private void Reveal(double seconds)
        {
            double fall = _releasePerSecond * seconds;

            for (int i = 0; i < _bands.Length; i++)
            {
                double shown = _bands[i].Level;

                if (shown > _latest[i])
                    _latest[i] = (float)shown;
                else
                    _latest[i] = (float)Math.Max(shown, _latest[i] - fall);
            }
        }

        private static double ToDecibels(double magnitude) =>
            magnitude <= 0
                ? FloorDecibels
                : Math.Clamp(20 * Math.Log10(magnitude), FloorDecibels, 0);

        /// <summary>One band-pass filter and the level it last measured.</summary>
        private struct Band
        {
            private readonly double _b0;
            private readonly double _b2;
            private readonly double _a1;
            private readonly double _a2;

            private double _x1;
            private double _x2;
            private double _y1;
            private double _y2;

            public Band(double centre, double q)
            {
                double w0 = 2 * Math.PI * Math.Clamp(centre, 0.001, 0.45);
                double alpha = Math.Sin(w0) / (2 * q);
                double a0 = 1 + alpha;

                // Constant peak gain: a tone at the centre comes out at its own amplitude.
                _b0 = alpha / a0;
                _b2 = -alpha / a0;
                _a1 = -2 * Math.Cos(w0) / a0;
                _a2 = (1 - alpha) / a0;

                _x1 = _x2 = _y1 = _y2 = 0;
                Level = FloorDecibels;
            }

            /// <summary>Gets or sets the level this band last measured, in decibels.</summary>
            public double Level { get; set; }

            /// <summary>Runs one sample through the filter and answers the filtered sample.</summary>
            public double Process(double input)
            {
                double output = _b0 * input + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

                _x2 = _x1;
                _x1 = input;
                _y2 = _y1;
                _y1 = output;

                return output;
            }
        }
    }
}