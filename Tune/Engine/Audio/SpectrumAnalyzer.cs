using System;

using NAudio.Dsp;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// Splits the signal into bands with an FFT and keeps the level of each band in decibels. It is
    /// meant for a display: the bands are spaced logarithmically, each one takes the loudest bin it
    /// covers, and the reading falls back slowly instead of jumping.
    /// </summary>
    public sealed class SpectrumAnalyzer
    {
        /// <summary>The bottom of the scale a band can read.</summary>
        public const double FloorDecibels = -60;

        private const int FftLength = 1024;
        private const int FftOrder = 10;    // 2^10 == FftLength

        private readonly Complex[] _fft = new Complex[FftLength];
        private readonly float[] _samples = new float[FftLength];
        private readonly double[] _bands;
        private readonly double _releasePerSecond;

        private int _position;
        private float[] _latest;

        /// <param name="bandCount">How many bands to split the spectrum into.</param>
        /// <param name="releasePerSecond">How fast a band falls back, in decibels per second.</param>
        public SpectrumAnalyzer(int bandCount = 16, double releasePerSecond = 40)
        {
            if (bandCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(bandCount));

            _bands = new double[bandCount];
            _latest = new float[bandCount];
            _releasePerSecond = releasePerSecond;

            for (int i = 0; i < _bands.Length; i++)
            {
                _bands[i] = FloorDecibels;
                _latest[i] = (float)FloorDecibels;
            }
        }

        /// <summary>Gets the level of each band, in decibels below full scale, from low to high.</summary>
        public IReadOnlyList<float> Bands => _latest;

        /// <summary>
        /// Adds a block of interleaved samples. The bands are only recalculated each time a whole
        /// FFT window has been filled, which is what keeps this cheap enough for the audio thread.
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
                // One mono sum is enough for a display.
                float value = buffer[i];

                _samples[_position++] = value;

                if (_position >= FftLength)
                {
                    Analyse(sampleRate);
                    _position = 0;
                }
            }

            Reveal(seconds);
        }

        /// <summary>Fills the FFT, measures each band's loudest bin and stores it in decibels.</summary>
        private void Analyse(int sampleRate)
        {
            // A Hann window keeps the edges of the window from smearing energy into other bands.
            for (int i = 0; i < FftLength; i++)
            {
                double window = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (FftLength - 1));

                _fft[i].X = (float)(_samples[i] * window);
                _fft[i].Y = 0;
            }

            FastFourierTransform.FFT(true, FftOrder, _fft);

            double binWidth = (double)sampleRate / FftLength;
            int half = FftLength / 2;

            for (int band = 0; band < _bands.Length; band++)
            {
                double low = BandEdge(band, _bands.Length, binWidth, half);
                double high = BandEdge(band + 1, _bands.Length, binWidth, half);

                int from = Math.Max(1, (int)Math.Floor(low / binWidth));
                int to = Math.Max(from, (int)Math.Ceiling(high / binWidth));
                to = Math.Min(to, half - 1);

                double peak = 0;

                for (int bin = from; bin <= to; bin++)
                {
                    double real = _fft[bin].X;
                    double imaginary = _fft[bin].Y;

                    peak = Math.Max(peak, Math.Sqrt(real * real + imaginary * imaginary));
                }

// The FFT is not normalised: its magnitudes grow with the window length, so they
                // have to be scaled back to full scale before they mean anything in decibels.
                _bands[band] = ToDecibels(peak / FftLength);
            }
        }

        /// <summary>
        /// The frequency where a band starts. The edges are spaced logarithmically from about 40 Hz
        /// so the low end gets the room it needs to be readable.
        /// </summary>
        private static double BandEdge(int index, int bandCount, double binWidth, int half)
        {
            double lowest = 40;
            double highest = Math.Max(lowest * 2, half * binWidth);

            return lowest * Math.Pow(highest / lowest, (double)index / bandCount);
        }

        /// <summary>Moves what is shown towards what was measured, falling back at a steady rate.</summary>
        private void Reveal(double seconds)
        {
            double fall = _releasePerSecond * seconds;

            for (int i = 0; i < _bands.Length; i++)
            {
                double shown = Math.Max(_bands[i], FloorDecibels);

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
    }
}
