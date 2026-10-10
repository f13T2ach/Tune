using NAudio.Effects;

namespace Tune.Engine.Effects
{
    /// <summary>
    /// The effects the rack can add to the chain.
    /// Only effects that expose a generic <see cref="IParameterized"/> surface are
    /// listed, because the parameter panel builds its editors from those parameters.
    /// The equalizer is deliberately absent: it is a fixed processor of the chain,
    /// not something the user stacks.
    /// </summary>
    public static class EffectCatalog
    {
        /// <summary>
        /// One addable effect: the label shown to the user and a factory for it.
        /// </summary>
        /// <param name="Name">The label shown in the picker and on the node.</param>
        /// <param name="Create">Creates a fresh instance of the effect.</param>
        public sealed record Entry(string Name, Func<IAudioEffect> Create);

        /// <summary>
        /// Gets every effect that can be added to the rack, in menu order.
        /// </summary>
        public static IReadOnlyList<Entry> Entries { get; } = new Entry[]
        {
            new Entry("Gain", () => new GainEffect()),
            new Entry("Pan", () => new PanEffect()),
            new Entry("Stereo Width", () => new StereoWidthEffect()),
            new Entry("DC Blocker", () => new DcBlockerEffect()),
            new Entry("Mono Maker", () => new MonoMakerEffect()),
            new Entry("Compressor", () => new CompressorEffect()),
            new Entry("Limiter", () => new LimiterEffect()),
            new Entry("Gate", () => new GateEffect()),
            new Entry("Transient Shaper", () => new TransientShaperEffect()),
            new Entry("De-Esser", () => new DeEsserEffect()),
            new Entry("Saturation", () => new SaturationEffect()),
            new Entry("Bit Crusher", () => new BitCrusherEffect()),
            new Entry("Delay", () => new DelayEffect()),
            new Entry("Chorus", () => new ChorusEffect()),
            new Entry("Flanger", () => new FlangerEffect()),
            new Entry("Phaser", () => new PhaserEffect()),
            new Entry("Tremolo", () => new TremoloEffect()),
            new Entry("Reverb (Freeverb)", () => new ReverbEffect()),
            new Entry("Reverb (FDN)", () => new FdnReverbEffect()),
            new Entry("Pitch Shift", () => new PitchShiftEffect()),
            new Entry("AGC", () => new AutomaticGainControlEffect()),
            new Entry("Noise Suppression", () => new NoiseSuppressionEffect()),
            new Entry("Comfort Noise", () => new ComfortNoiseEffect())
        };
    }
}
