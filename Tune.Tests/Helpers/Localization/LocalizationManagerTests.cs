using Tune.Helpers.Localization;
using Xunit;

namespace Tune.Tests.Helpers.Localization
{
    /// <summary>Verifies the behavior of <see cref="LocalizationManager"/>.</summary>
    public class LocalizationManagerTests
    {
        #region ==== System Language ====

        /// <summary>Verifies that the auto value keeps whatever language the system reports.</summary>
        [Theory]
        [InlineData("auto")]
        [InlineData("AUTO")]
        [InlineData(" auto ")]
        public void ResolveCultureName_WithAuto_ReturnsNull(string language)
        {
            Assert.Null(LocalizationManager.ResolveCultureName(language));
        }

        /// <summary>Verifies that a missing setting keeps the system language.</summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ResolveCultureName_WithoutValue_ReturnsNull(string? language)
        {
            Assert.Null(LocalizationManager.ResolveCultureName(language));
        }

        #endregion

        #region ==== Explicit Language ====

        /// <summary>Verifies that a culture name is accepted and returned in its canonical form.</summary>
        [Theory]
        [InlineData("en", "en")]
        [InlineData("EN-US", "en-US")]
        [InlineData("zh-CN", "zh-CN")]
        [InlineData(" zh-cn ", "zh-CN")]
        public void ResolveCultureName_WithKnownCulture_ReturnsCanonicalName(string language, string expected)
        {
            Assert.Equal(expected, LocalizationManager.ResolveCultureName(language));
        }

        /// <summary>Verifies that an unknown value falls back to the system language.</summary>
        [Theory]
        [InlineData("not-a-culture")]
        [InlineData("!!")]
        public void ResolveCultureName_WithUnknownCulture_ReturnsNull(string language)
        {
            Assert.Null(LocalizationManager.ResolveCultureName(language));
        }

        #endregion
    }
}
