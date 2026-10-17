using System;

using NAudio.Wave;

namespace Tune.Engine.Audio
{
    /// <summary>
    /// The last thing between the user layer and the device. It is fixed on purpose: a safety
    /// limiter that is not configurable, and the meter that tells whether the user layer ran the
    /// signal past full scale.
    /// </summary>
    /// <remarks>
    /// The chain is: user layer -> this stage -> device. The meter is placed before the safety
    /// limiter, so a red meter means the signal really did reach full scale rather than showing
    /// samples the limiter has already flattened.
    /// </remarks>
    public sealed class SystemOutputStage : ISampleProvider, IAudioLevelSource, IDisposable
    {
        /// <summary>The ceiling the safety limiter holds, a little below full scale.</summary>
        public const double SafetyCeilingDecibels = -0.3;

        private readonly MeteringSampleProvider _meter;
        private readonly OutputLimiterProvider _limiter;

        /// <param name="source">The user layer, already processed.</param>
        public SystemOutputStage(ISampleProvider source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            _meter = new MeteringSampleProvider(source);
            _limiter = new OutputLimiterProvider(_meter) { CeilingDecibels = SafetyCeilingDecibels };
            _meter.LevelReported += OnMeterReported;
        }

        /// <inheritdoc/>
        public WaveFormat WaveFormat => _limiter.WaveFormat;

        /// <inheritdoc/>
        public event EventHandler<AudioLevelEventArgs>? LevelReported;

        /// <inheritdoc/>
        public int Read(Span<float> buffer) => _limiter.Read(buffer);

        /// <summary>Reads a slice of an array, which is the form older callers use.</summary>
        public int Read(float[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        /// <summary>Stops forwarding the meter, so a discarded stage goes quiet.</summary>
        public void Dispose() => _meter.LevelReported -= OnMeterReported;

        private void OnMeterReported(object? sender, AudioLevelEventArgs e) =>
            LevelReported?.Invoke(this, e);
    }
}