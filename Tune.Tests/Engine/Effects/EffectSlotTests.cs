using NAudio.Effects;

using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies the behavior of <see cref="EffectSlot"/>.</summary>
    public class EffectSlotTests
    {
        #region ==== Bypass ====

        /// <summary>Verifies that a fresh node is not bypassed.</summary>
        [Fact]
        public void Bypass_WithNewSlot_IsFalse()
        {
            EffectSlot slot = CreateSlot();

            Assert.False(slot.Bypass);
            Assert.False(Effect(slot).Bypass);
        }

        /// <summary>Verifies that switching a node off bypasses its effect.</summary>
        [Fact]
        public void Bypass_WhenSet_BypassesTheEffect()
        {
            EffectSlot slot = CreateSlot();

            slot.Bypass = true;

            Assert.True(Effect(slot).Bypass);
        }

        /// <summary>
        /// Verifies that the chain master switch bypasses a node without losing the
        /// choice the user made for that node.
        /// </summary>
        [Fact]
        public void ApplyBypass_WhenSuppressedAndReleased_KeepsTheUserChoice()
        {
            EffectSlot slot = CreateSlot();

            slot.ApplyBypass(suppressed: true);
            Assert.True(Effect(slot).Bypass);
            Assert.False(slot.Bypass);

            slot.ApplyBypass(suppressed: false);
            Assert.False(Effect(slot).Bypass);
        }

        #endregion

        #region ==== Parameters ====

        /// <summary>Verifies that the parameters of the effect are collected for the editor.</summary>
        [Fact]
        public void Parameters_WithGainEffect_IsNotEmpty()
        {
            Assert.NotEmpty(CreateSlot().Parameters);
        }

        /// <summary>Verifies that the node carries a one line summary of its first parameter.</summary>
        [Fact]
        public void Subtitle_WithGainEffect_NamesTheFirstParameter()
        {
            EffectSlot slot = CreateSlot();

            Assert.False(string.IsNullOrWhiteSpace(slot.Subtitle));
            Assert.Contains(slot.Parameters[0].Name, slot.Subtitle);
        }

        #endregion

        /// <summary>Creates a slot over a fresh gain effect with no chain behind it.</summary>
        private static EffectSlot CreateSlot()
        {
            return new EffectSlot("Gain", new GainEffect(), _ => { }, _ => { }, _ => { });
        }

        /// <summary>Returns the effect of a slot as the base class that owns Bypass.</summary>
        private static AudioEffect Effect(EffectSlot slot)
        {
            return Assert.IsAssignableFrom<AudioEffect>(slot.Effect);
        }
    }
}
