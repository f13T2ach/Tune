namespace Tune.Tests.Engine.Effects
{
    /// <summary>Test helper that builds sine waves and measures the amplitude of one frequency.</summary>
    internal static class SineSignal
    {
        /// <summary>Builds a mono sine wave with the given frequency and amplitude.</summary>
        public static float[] Create(double frequency, double amplitude, int sampleRate, int frameCount)
        {
            var samples = new float[frameCount];

            for (int n = 0; n < frameCount; n++)
                samples[n] = (float)(amplitude * Math.Sin(2 * Math.PI * frequency * n / sampleRate));

            return samples;
        }

        /// <summary>
        /// Measures the amplitude of one frequency over the second half of the buffer,
        /// which skips the settling transient of a filter. The measurement is exact for
        /// frequencies that are an even number of hertz, because the second half then
        /// holds a whole number of cycles.
        /// </summary>
        public static double MeasureAmplitude(float[] samples, double frequency, int sampleRate)
        {
            int start = samples.Length / 2;
            int count = samples.Length - start;
            double real = 0;
            double imaginary = 0;

            for (int n = 0; n < count; n++)
            {
                double angle = 2 * Math.PI * frequency * (start + n) / sampleRate;
                real += samples[start + n] * Math.Cos(angle);
                imaginary -= samples[start + n] * Math.Sin(angle);
            }

            return 2.0 * Math.Sqrt(real * real + imaginary * imaginary) / count;
        }
    }
}
