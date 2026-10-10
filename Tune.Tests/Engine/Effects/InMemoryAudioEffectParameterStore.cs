using Tune.Engine.Effects;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Test double that keeps effect parameters in memory instead of user settings.</summary>
    internal sealed class InMemoryAudioEffectParameterStore : IAudioEffectParameterStore
    {
        private readonly Dictionary<string, double> _values = new Dictionary<string, double>();

        /// <summary>Gets the values written so far, keyed by parameter key.</summary>
        public IReadOnlyDictionary<string, double> Values => _values;

        /// <summary>Reads a value stored earlier.</summary>
        public bool TryLoad(string key, out double value) => _values.TryGetValue(key, out value);

        /// <summary>Stores a value, replacing any previous one.</summary>
        public void Save(string key, double value) => _values[key] = value;
    }
}
