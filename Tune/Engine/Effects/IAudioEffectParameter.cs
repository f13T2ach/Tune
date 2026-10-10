using System.ComponentModel;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// A single adjustable value owned by the effect chain.
    /// The value is expressed on the UI scale, so a knob can bind directly to one.
    /// </summary>
    public interface IAudioEffectParameter : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the key used to persist the value.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the short label shown on the UI, such as "VOL" or "A".
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets the lowest accepted value.
        /// </summary>
        double MinValue { get; }

        /// <summary>
        /// Gets the highest accepted value.
        /// </summary>
        double MaxValue { get; }

        /// <summary>
        /// Gets the value used when nothing has been persisted yet.
        /// </summary>
        double DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether the range crosses zero,
        /// meaning the control trims instead of attenuating.
        /// </summary>
        bool IsBipolar { get; }

        /// <summary>
        /// Gets or sets the current value, clamped to the range.
        /// </summary>
        double Value { get; set; }
    }
}
