using CommunityToolkit.Mvvm.ComponentModel;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// An effect parameter that clamps its value and writes it through to the store.
    /// The stored value is loaded once on construction.
    /// </summary>
    public class AudioEffectParameter : ObservableObject, IAudioEffectParameter
    {
        private readonly IAudioEffectParameterStore _store;
        private double _value;

        /// <summary>
        /// Initializes a parameter, restoring the persisted value when one exists.
        /// </summary>
        /// <param name="store">The store the value is read from and written to.</param>
        /// <param name="key">The persistence key.</param>
        /// <param name="displayName">The short label shown on the UI.</param>
        /// <param name="minValue">The lowest accepted value.</param>
        /// <param name="maxValue">The highest accepted value.</param>
        /// <param name="defaultValue">The value used when nothing was persisted.</param>
        public AudioEffectParameter(
            IAudioEffectParameterStore store,
            string key,
            string displayName,
            double minValue,
            double maxValue,
            double defaultValue)
        {
            _store = store;
            Key = key;
            DisplayName = displayName;
            MinValue = minValue;
            MaxValue = maxValue;
            DefaultValue = defaultValue;

            double restored = store.TryLoad(key, out double stored) ? stored : defaultValue;
            _value = Math.Clamp(restored, minValue, maxValue);
        }

        /// <inheritdoc />
        public string Key { get; }

        /// <inheritdoc />
        public string DisplayName { get; }

        /// <inheritdoc />
        public double MinValue { get; }

        /// <inheritdoc />
        public double MaxValue { get; }

        /// <inheritdoc />
        public double DefaultValue { get; }

        /// <inheritdoc />
        public bool IsBipolar => MinValue < 0 && MaxValue > 0;

        /// <inheritdoc />
        public double Value
        {
            get => _value;
            set
            {
                double clamped = Math.Clamp(value, MinValue, MaxValue);
                if (SetProperty(ref _value, clamped))
                    _store.Save(Key, clamped);
            }
        }
    }
}
