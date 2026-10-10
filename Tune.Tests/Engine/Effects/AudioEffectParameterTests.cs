using Tune.Engine.Effects;
using Xunit;

namespace Tune.Tests.Engine.Effects
{
    /// <summary>Verifies the behavior of <see cref="AudioEffectParameter"/>.</summary>
    public class AudioEffectParameterTests
    {
        #region ==== Construction ====

        /// <summary>Verifies that a persisted value is restored on construction.</summary>
        [Fact]
        public void Constructor_WithPersistedValue_RestoresIt()
        {
            var store = new InMemoryAudioEffectParameterStore();
            store.Save("Bass", 20);

            var parameter = new AudioEffectParameter(store, "Bass", "A", -50, 50, 0);

            Assert.Equal(20, parameter.Value);
        }

        /// <summary>Verifies that the default value is used when nothing was persisted.</summary>
        [Fact]
        public void Constructor_WithoutPersistedValue_UsesDefault()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "Bass", "A", -50, 50, 7);

            Assert.Equal(7, parameter.Value);
        }

        /// <summary>Verifies that a persisted value outside the range is clamped.</summary>
        [Fact]
        public void Constructor_WithOutOfRangeValue_ClampsToRange()
        {
            var store = new InMemoryAudioEffectParameterStore();
            store.Save("Bass", 900);

            var parameter = new AudioEffectParameter(store, "Bass", "A", -50, 50, 0);

            Assert.Equal(50, parameter.Value);
        }

        #endregion

        #region ==== Value ====

        /// <summary>Verifies that a value above the range is clamped to the maximum.</summary>
        [Fact]
        public void Value_AboveRange_ClampsToMaximum()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "Bass", "A", -50, 50, 0);

            parameter.Value = 80;

            Assert.Equal(50, parameter.Value);
        }

        /// <summary>Verifies that a value below the range is clamped to the minimum.</summary>
        [Fact]
        public void Value_BelowRange_ClampsToMinimum()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "Bass", "A", -50, 50, 0);

            parameter.Value = -80;

            Assert.Equal(-50, parameter.Value);
        }

        /// <summary>Verifies that a new value is written through to the store.</summary>
        [Fact]
        public void Value_WhenChanged_PersistsTheClampedValue()
        {
            var store = new InMemoryAudioEffectParameterStore();
            var parameter = new AudioEffectParameter(store, "Bass", "A", -50, 50, 0);

            parameter.Value = 12.5;

            Assert.Equal(12.5, store.Values["Bass"]);
        }

        /// <summary>Verifies that a value change raises a notification.</summary>
        [Fact]
        public void Value_WhenChanged_RaisesPropertyChanged()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "Bass", "A", -50, 50, 0);
            string? changed = null;
            parameter.PropertyChanged += (_, e) => changed = e.PropertyName;

            parameter.Value = 10;

            Assert.Equal(nameof(IAudioEffectParameter.Value), changed);
        }

        #endregion

        #region ==== Shape ====

        /// <summary>Verifies that a range crossing zero is reported as bipolar.</summary>
        [Fact]
        public void IsBipolar_WithRangeCrossingZero_IsTrue()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "Bass", "A", -50, 50, 0);

            Assert.True(parameter.IsBipolar);
        }

        /// <summary>Verifies that a range starting at zero is not bipolar.</summary>
        [Fact]
        public void IsBipolar_WithRangeStartingAtZero_IsFalse()
        {
            var parameter = new AudioEffectParameter(new InMemoryAudioEffectParameterStore(), "PlayerVolume", "VOL", 0, 100, 50);

            Assert.False(parameter.IsBipolar);
        }

        #endregion
    }
}
