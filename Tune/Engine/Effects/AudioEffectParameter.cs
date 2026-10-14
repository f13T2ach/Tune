using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Helpers;
using Tune.Local;

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
        /// Gets the short label shown on the UI, such as "VOL" or "BASS".
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

    /// <summary>
    /// Persistence seam for effect parameters.
    /// Keeping it an interface lets the chain run against plain memory in tests.
    /// </summary>
    public interface IAudioEffectParameterStore
    {
        /// <summary>
        /// Tries to read a persisted value.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The stored value, or zero when nothing was stored.</param>
        /// <returns><c>true</c> when a value was found.</returns>
        bool TryLoad(string key, out double value);

        /// <summary>
        /// Persists a value.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The value to store.</param>
        void Save(string key, double value);
    }

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

    /// <summary>
    /// Stores effect parameters in the user settings file.
    /// Every parameter key must exist in TuneSettings, otherwise the write is refused.
    /// </summary>
    internal sealed class TuneSettingsParameterStore : IAudioEffectParameterStore
    {
        /// <inheritdoc />
        public bool TryLoad(string key, out double value)
        {
            value = 0;

            try
            {
                object? stored = TuneSettings.Default[key];
                if (stored == null || stored == DBNull.Value)
                    return false;

                value = Convert.ToDouble(stored);
                return true;
            }
            catch (Exception ex)
            {
                // A missing key is not fatal, the caller falls back to the default value.
                LoggerHelper.LogError($"Unable to read effect parameter '{key}'", ex, nameof(TryLoad));
                return false;
            }
        }

        /// <inheritdoc />
        public void Save(string key, double value)
        {
            try
            {
                TuneSettings.Default[key] = value;
                TuneSettings.Default.Save();
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Unable to save effect parameter '{key}'", ex, nameof(Save));
            }
        }
    }
}
