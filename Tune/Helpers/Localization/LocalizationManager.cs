using System;
using System.Globalization;

namespace Tune.Helpers.Localization
{
    /// <summary>
    /// Decides which language the interface uses and applies it to the current thread.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>Setting value that keeps whichever language the system reports.</summary>
        public const string SystemLanguage = "auto";

        /// <summary>
        /// Resolves the stored setting into the culture name to apply.
        /// </summary>
        /// <param name="language">Stored value: "auto", a culture name such as "zh-CN", or empty.</param>
        /// <returns>The culture name to apply, or null to keep the system language.</returns>
        public static string? ResolveCultureName(string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return null;

            string candidate = language.Trim();
            if (candidate.Equals(SystemLanguage, StringComparison.OrdinalIgnoreCase))
                return null;

            try
            {
                // predefinedOnly keeps a typo such as "zh_CN" from silently becoming a
                // custom culture with no resources, which would drop the user to English.
                return CultureInfo.GetCultureInfo(candidate, predefinedOnly: true).Name;
            }
            catch (ArgumentException)
            {
                // An unknown value is not worth failing the start up over.
                return null;
            }
        }

        /// <summary>
        /// Applies the stored language, or keeps the system language when it is set to "auto".
        /// The chosen language also drives formatting, so dates and numbers follow it.
        /// </summary>
        public static void ApplySavedLanguage()
        {
            string? cultureName = ResolveCultureName(Local.TuneSettings.Default.Language);
            if (cultureName == null)
                return;

            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
