namespace Tune.Helpers.FormatConverters
{
    /// <summary>
    /// Converts a bipolar UI mix value (-50..50) to a normalized factor (-1..1).
    /// The signed counterpart of <see cref="UIVolumeToDoubleVolume"/>.
    /// </summary>
    internal static class UIMixToDoubleMix
    {
        /// <summary>
        /// The UI value that maps to a factor of one.
        /// </summary>
        public const double UiRange = 50.0;

        /// <summary>
        /// Converts a UI mix value to its normalized factor.
        /// </summary>
        /// <param name="uiMix">The UI value, usually between -50 and 50.</param>
        /// <returns>The factor, usually between -1 and 1.</returns>
        public static double Convert(double uiMix)
        {
            return uiMix / UiRange;
        }

        /// <summary>
        /// Converts a normalized factor back to its UI mix value.
        /// </summary>
        /// <param name="mix">The factor, usually between -1 and 1.</param>
        /// <returns>The UI value, usually between -50 and 50.</returns>
        public static double ReverseConvert(double mix)
        {
            return mix * UiRange;
        }
    }
}
