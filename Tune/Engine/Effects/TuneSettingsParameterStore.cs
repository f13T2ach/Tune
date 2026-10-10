using Tune.Helpers;
using Tune.Local;

namespace Tune.Engine.Effects
{
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
