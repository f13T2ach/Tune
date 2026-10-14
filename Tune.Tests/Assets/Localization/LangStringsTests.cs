using System.Collections;
using System.Globalization;
using System.Resources;

using Tune.Assets.Localization;
using Xunit;

namespace Tune.Tests.Assets.Localization
{
    /// <summary>Verifies that the two languages define the same set of strings.</summary>
    public class LangStringsTests
    {
        #region ==== Key Coverage ====

        /// <summary>
        /// Verifies that the Chinese resources define every key of the neutral ones.
        /// Lookup deliberately skips parents, otherwise a missing key would be hidden
        /// by the fallback to the neutral language.
        /// </summary>
        [Fact]
        public void Resources_WithChineseCulture_DefineEveryNeutralKey()
        {
            ResourceSet? neutral = LangStrings.ResourceManager.GetResourceSet(
                CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true);
            ResourceSet? chinese = LangStrings.ResourceManager.GetResourceSet(
                new CultureInfo("zh-CN"), createIfNotExists: true, tryParents: false);

            Assert.NotNull(neutral);
            Assert.NotNull(chinese);

            var missing = new List<string>();
            foreach (DictionaryEntry entry in neutral)
            {
                string key = (string)entry.Key;
                if (chinese.GetString(key) == null)
                    missing.Add(key);
            }

            Assert.Empty(missing);
        }

        /// <summary>Verifies that no string is left empty in either language.</summary>
        [Fact]
        public void Resources_WithEitherCulture_DefineNoEmptyString()
        {
            ResourceSet? neutral = LangStrings.ResourceManager.GetResourceSet(
                CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: true);
            ResourceSet? chinese = LangStrings.ResourceManager.GetResourceSet(
                new CultureInfo("zh-CN"), createIfNotExists: true, tryParents: false);

            Assert.NotNull(neutral);
            Assert.NotNull(chinese);

            var empty = new List<string>();
            foreach (DictionaryEntry entry in neutral)
            {
                string key = (string)entry.Key;
                if (string.IsNullOrWhiteSpace(neutral.GetString(key)) ||
                    string.IsNullOrWhiteSpace(chinese.GetString(key)))
                    empty.Add(key);
            }

            Assert.Empty(empty);
        }

        #endregion
    }
}
