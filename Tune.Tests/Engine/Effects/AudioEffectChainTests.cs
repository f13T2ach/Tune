using NAudio.Effects;
using NAudio.Wave;

using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies the behavior of <see cref="AudioEffectChain"/>.</summary>
    public class AudioEffectChainTests
    {
        private const int SampleRate = 44100;
        private const int FrameCount = 44100;
        private const int SteadyStateFrames = 16384;
        private const double InputAmplitude = 0.5;

        #region ==== Composition ====

        /// <summary>Verifies that the rack starts empty.</summary>
        [Fact]
        public void Slots_WithDefaultChain_IsEmpty()
        {
            Assert.Empty(CreateChain().Slots);
        }

        /// <summary>Verifies that the equalizer is a fixed processor and is not offered as a rack entry.</summary>
        [Fact]
        public void AvailableEffects_WithDefaultChain_DoesNotOfferTheEqualizer()
        {
            AudioEffectChain chain = CreateChain();

            Assert.NotEmpty(chain.AvailableEffects);
            Assert.DoesNotContain("Equalizer", chain.AvailableEffects);
            Assert.DoesNotContain("EQ", chain.AvailableEffects);
        }

        /// <summary>
        /// Verifies the invariant that keeps the rack from conflicting with the knobs:
        /// adding every catalog entry never changes or duplicates the parameters the
        /// main page binds to, because those belong to the fixed equalizer.
        /// </summary>
        [Fact]
        public void AddEffect_WithEveryCatalogEntry_LeavesTheKnobParametersUntouched()
        {
            AudioEffectChain chain = CreateChain();
            List<IAudioEffectParameter> before = KnobParameters(chain);

            for (int i = 0; i < chain.AvailableEffects.Count; i++)
            {
                chain.SelectedEffectIndex = i;
                chain.AddEffectCommand.Execute(null);
            }

            List<IAudioEffectParameter> after = KnobParameters(chain);

            Assert.Equal(chain.AvailableEffects.Count, chain.Slots.Count);
            Assert.Equal(before.Count, after.Count);
            Assert.Equal(after.Count, after.Distinct().Count());
            Assert.Equal(before, after);
        }

        /// <summary>Verifies that the level and the equalizer keep their UI ranges.</summary>
        [Fact]
        public void Knobs_WithDefaultChain_UseTheUiRanges()
        {
            AudioEffectChain chain = CreateChain();

            Assert.Equal(0, chain.Volume.MinValue);
            Assert.Equal(100, chain.Volume.MaxValue);
            Assert.Equal(50, chain.Volume.Value);
            Assert.Equal(
                new[] { "BASS", "MID", "TREBLE", "EQ" },
                chain.Equalizer.Parameters.Select(parameter => parameter.DisplayName));
        }

        /// <summary>Verifies that the effects are on by default.</summary>
        [Fact]
        public void IsEnabled_WithDefaultChain_IsTrue()
        {
            Assert.True(CreateChain().IsEnabled);
        }

        /// <summary>Verifies that an added node becomes the selected one.</summary>
        [Fact]
        public void AddEffect_WithDefaultChain_SelectsTheNewNode()
        {
            AudioEffectChain chain = CreateChain();
            chain.SelectedEffectIndex = IndexOf(chain, "Gain");

            chain.AddEffectCommand.Execute(null);

            EffectSlot slot = Assert.Single(chain.Slots);
            Assert.Same(slot, chain.SelectedSlot);
            Assert.Equal("Gain", slot.Name);
        }

        /// <summary>Verifies that a node can be moved and dropped again.</summary>
        [Fact]
        public void MoveDown_ThenRemove_ReordersAndDropsTheNode()
        {
            AudioEffectChain chain = CreateChain();
            Add(chain, "Gain");
            Add(chain, "Pan");
            EffectSlot first = chain.Slots[0];

            first.MoveDownCommand.Execute(null);
            Assert.Same(first, chain.Slots[1]);

            first.RemoveCommand.Execute(null);
            Assert.Single(chain.Slots);
            Assert.Equal("Pan", chain.Slots[0].Name);
            Assert.Same(chain.Slots[0], chain.SelectedSlot);
        }

        #endregion

        #region ==== Processing ====

        /// <summary>Verifies that reading through the processor applies the volume.</summary>
        [Fact]
        public void Read_ThroughProcessor_AppliesTheVolume()
        {
            AudioEffectChain chain = CreateChain();
            var source = new FakeSampleProvider(new[] { 1.0f, 1.0f, 1.0f });
            float[] buffer = new float[3];

            int read = chain.CreateProcessor(source).Read(buffer);

            Assert.Equal(3, read);
            Assert.Equal(new[] { 0.5f, 0.5f, 0.5f }, buffer);
        }

        /// <summary>Verifies that the processor reports the format of the source it wraps.</summary>
        [Fact]
        public void Read_ThroughProcessor_KeepsTheSourceFormat()
        {
            AudioEffectChain chain = CreateChain();
            var source = new FakeSampleProvider(new[] { 1.0f }, sampleRate: 48000, channels: 1);

            Assert.Equal(source.WaveFormat, chain.CreateProcessor(source).WaveFormat);
        }

        /// <summary>Verifies that the equalizer shapes the audio while the chain is on.</summary>
        [Fact]
        public void Read_WithEqualizerBoost_LiftsTheLowBand()
        {
            AudioEffectChain chain = CreateChain();

            // Unity level, so the measurement shows the equalizer alone.
            chain.Volume.Value = 100;
            chain.Equalizer.Bass.Value = 50;

            Assert.InRange(MeasureGain(chain, 30), 3.5, 4.1);
        }

        /// <summary>Verifies that switching the chain off bypasses the effects but keeps the volume.</summary>
        [Fact]
        public void Read_WithChainDisabled_BypassesTheEffectsButKeepsTheVolume()
        {
            AudioEffectChain chain = CreateChain();
            chain.Equalizer.Bass.Value = 50;
            chain.IsEnabled = false;

            Assert.InRange(MeasureGain(chain, 30), 0.49, 0.51);
        }

        /// <summary>
        /// Verifies that a rack edit reaches the running processor, so an effect added
        /// while audio plays is heard on the next block instead of the next file.
        /// </summary>
        [Fact]
        public void Read_AfterAddingANode_HearsTheNewEffect()
        {
            AudioEffectChain chain = CreateChain();
            chain.Volume.Value = 100;
            var source = new FakeSampleProvider(CreateConstantBlock(), SampleRate, channels: 1);
            float[] buffer = new float[SteadyStateFrames];

            ISampleProvider processor = chain.CreateProcessor(source);

            chain.SelectedEffectIndex = IndexOf(chain, "Gain");
            chain.AddEffectCommand.Execute(null);
            var gain = Assert.IsType<GainEffect>(chain.Slots[0].Effect);
            gain.LinearGain = 2f;

            processor.Read(buffer);

            Assert.Equal(2.0, (double)buffer[^1], tolerance: 1e-4);
        }

        /// <summary>Verifies that a bypassed node of the rack is skipped by the audio path.</summary>
        [Fact]
        public void Read_WithBypassedNode_LeavesTheSignalAlone()
        {
            AudioEffectChain chain = CreateChain();
            chain.Volume.Value = 100;
            chain.SelectedEffectIndex = IndexOf(chain, "Gain");
            chain.AddEffectCommand.Execute(null);
            var gain = Assert.IsType<GainEffect>(chain.Slots[0].Effect);
            gain.LinearGain = 2f;
            chain.Slots[0].Bypass = true;
            var source = new FakeSampleProvider(CreateConstantBlock(), SampleRate, channels: 1);
            float[] buffer = new float[SteadyStateFrames];

            chain.CreateProcessor(source).Read(buffer);

            Assert.Equal(1.0, (double)buffer[^1], tolerance: 1e-4);
        }

        #endregion

        #region ==== Reset ====

        /// <summary>Verifies that resetting clears the effect state and leaves the chain working.</summary>
        [Fact]
        public void Reset_AfterProcessing_KeepsTheChainWorking()
        {
            AudioEffectChain chain = CreateChain();
            chain.CreateProcessor(new FakeSampleProvider(new float[64], SampleRate, channels: 1)).Read(new float[64]);

            chain.Reset();

            Assert.InRange(MeasureGain(chain, 1000), 0.49, 0.51);
        }

        #endregion

        /// <summary>Creates a chain over an in-memory store.</summary>
        private static AudioEffectChain CreateChain()
        {
            return new AudioEffectChain(new InMemoryAudioEffectParameterStore());
        }

        /// <summary>Adds the catalog entry with the given label to the rack.</summary>
        private static void Add(AudioEffectChain chain, string name)
        {
            chain.SelectedEffectIndex = IndexOf(chain, name);
            chain.AddEffectCommand.Execute(null);
        }

        /// <summary>Finds the catalog index of an effect by its label.</summary>
        private static int IndexOf(AudioEffectChain chain, string name)
        {
            int index = chain.AvailableEffects.ToList().IndexOf(name);
            Assert.True(index >= 0, $"The catalog does not offer '{name}'.");
            return index;
        }

        /// <summary>Collects the parameters the main page knobs bind to.</summary>
        private static List<IAudioEffectParameter> KnobParameters(AudioEffectChain chain)
        {
            var parameters = new List<IAudioEffectParameter> { chain.Volume };
            parameters.AddRange(chain.Equalizer.Parameters);
            return parameters;
        }

        /// <summary>Builds a constant mono block, used to read a settled gain.</summary>
        private static float[] CreateConstantBlock()
        {
            var buffer = new float[SteadyStateFrames];
            Array.Fill(buffer, 1.0f);
            return buffer;
        }

        /// <summary>Runs a mono sine through the whole chain and returns its gain.</summary>
        private static double MeasureGain(AudioEffectChain chain, double frequency)
        {
            float[] input = SineSignal.Create(frequency, InputAmplitude, SampleRate, FrameCount);
            var source = new FakeSampleProvider(input, SampleRate, channels: 1);
            float[] output = new float[input.Length];

            chain.CreateProcessor(source).Read(output);

            return SineSignal.MeasureAmplitude(output, frequency, SampleRate) / InputAmplitude;
        }
    }
}
