namespace Tune.Engine.Effects
{
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
}
