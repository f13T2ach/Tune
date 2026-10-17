using System;

using Tune.Engine.Audio;
using Xunit;

namespace Tune.Tests.Engine.Audio
{
    /// <summary>Verifies the sampling window that decides the average and the clip indicator.</summary>
    public class LevelWindowTests
    {
        private static readonly TimeSpan HalfSecond = TimeSpan.FromSeconds(0.5);

        #region ==== The Window ====

        /// <summary>Verifies that the window says nothing until its time has run out.</summary>
        [Fact]
        public void Add_InsideTheWindow_AnswersNothing()
        {
            var window = new LevelWindow(OneSecond);

            Assert.Null(window.Add(-10, -20, 0.6));
            Assert.Null(window.Add(-20, -30, 0.3));
        }

        /// <summary>Verifies the average and the maximum of a completed window.</summary>
        [Fact]
        public void Add_WhenTheWindowCloses_AnswersTheAverageAndTheMaximum()
        {
            var window = new LevelWindow(OneSecond);

            window.Add(-10, -40, 0.5);

            // This block brings the window to exactly one second, so it closes here.
            LevelWindowResult? result = window.Add(-20, -20, 0.5);

            Assert.NotNull(result);

            Assert.Equal(-15, result!.Value.AverageLeft, 6);
            Assert.Equal(-30, result.Value.AverageRight, 6);

            Assert.Equal(-10, result.Value.MaxLeft, 6);
            Assert.Equal(-20, result.Value.MaxRight, 6);
        }

        /// <summary>Verifies that the average takes the block lengths into account.</summary>
        [Fact]
        public void Add_WithUnevenBlocks_WeightsTheAverageByTime()
        {
            var window = new LevelWindow(OneSecond);

            window.Add(-10, -10, 0.75);
            LevelWindowResult? result = window.Add(-30, -30, 0.25);

            Assert.NotNull(result);
            Assert.Equal(-15, result!.Value.AverageLeft, 6);
        }

        /// <summary>Verifies that a new window starts empty after one closes.</summary>
        [Fact]
        public void Add_AfterAWindowCloses_StartsANewOne()
        {
            var window = new LevelWindow(HalfSecond);
            window.Add(-10, -10, 0.5);

            Assert.Null(window.Add(-50, -50, 0.2));

            LevelWindowResult? result = window.Add(-30, -30, 0.3);

            Assert.NotNull(result);

            // Only the blocks of the new window count, weighted by their lengths.
            Assert.Equal(-38, result!.Value.AverageLeft, 6);
            Assert.Equal(-30, result.Value.MaxLeft, 6);
        }

        /// <summary>Verifies that a block longer than the window closes it straight away.</summary>
        [Fact]
        public void Add_WithABlockLongerThanTheWindow_AnswersAtOnce()
        {
            var window = new LevelWindow(HalfSecond);

            LevelWindowResult? result = window.Add(-5, -7, 3);

            Assert.NotNull(result);
            Assert.Equal(-5, result!.Value.MaxLeft, 6);
        }

        #endregion

        /// <summary>Verifies that a window has to last something.</summary>
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_WithoutTime_Throws(int seconds) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelWindow(TimeSpan.FromSeconds(seconds)));

        /// <summary>Verifies that a block has to last something.</summary>
        [Fact]
        public void Add_WithoutTime_Throws() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelWindow(OneSecond).Add(-1, -1, 0));

        /// <summary>Verifies that the default window is the two seconds the meter uses.</summary>
        [Fact]
        public void DefaultWindow_IsTwoSeconds() =>
            Assert.Equal(TimeSpan.FromSeconds(2), LevelWindow.DefaultWindow);

        private static TimeSpan OneSecond => TimeSpan.FromSeconds(1);
    }
}
