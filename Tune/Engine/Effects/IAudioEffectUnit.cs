using NAudio.Effects;
using NAudio.Wave;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// One effect of the chain together with the parameters it owns.
    /// A parameter belongs to exactly one unit, so two units can never shape
    /// the same control and the audio order follows the units.
    /// </summary>
    public interface IAudioEffectUnit
    {
        /// <summary>
        /// Gets the stable identifier of the unit, used for diagnostics.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the label shown on the UI.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the unit is applied.
        /// The transition is ramped by the audio framework, so it does not click.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets a value indicating whether the unit can process the loaded format.
        /// An unsupported unit stays out of the audio path entirely.
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// Gets the parameters owned by this unit.
        /// </summary>
        IReadOnlyList<IAudioEffectParameter> Parameters { get; }

        /// <summary>
        /// Gets the audio effects of this unit, in the order they are applied.
        /// </summary>
        IReadOnlyList<AudioEffect> Effects { get; }

        /// <summary>
        /// Tells the unit which format the audio has, so it can build its filters.
        /// </summary>
        /// <param name="format">The format of the loaded file.</param>
        void Configure(WaveFormat format);
    }
}
