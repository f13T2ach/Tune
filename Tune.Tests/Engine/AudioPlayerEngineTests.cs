using Tune.Engine;
using Xunit;

namespace Tune.Tests.Engine
{
    /// <summary>Verifies how <see cref="AudioPlayerEngine"/> picks the audio output API.</summary>
    public class AudioPlayerEngineTests
    {
        #region ==== Api Name ====

        /// <summary>Verifies that only the stored WaveOut name selects the WaveOut device.</summary>
        [Theory]
        [InlineData("waveout", true)]
        [InlineData("WaveOut", true)]
        [InlineData("WAVEOUT", true)]
        [InlineData(" waveout ", false)]
        [InlineData("wasapi", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("unknown", false)]
        public void UseWaveOut_WithStoredName_ReturnsExpected(string? apiName, bool expected)
        {
            Assert.Equal(expected, AudioPlayerEngine.UseWaveOut(apiName));
        }

        #endregion
    }
}
