namespace Tune.Helpers.FormatConverters
{
    class UIVolumeToDoubleVolume
    {
        /// <summary>
        /// Convert UI volume (0-100) to double volume (0.0-1.0)
        /// </summary>
        /// <param name="uiVolume">UI Volumes</param>
        /// <returns></returns>
        public static double Convert(double uiVolume)
        {
            return uiVolume / 100.0;
        }

        /// <summary>
        /// Convert double volume (0.0-1.0) to UI volume (0-100) 
        /// </summary>
        /// <param name="doubleVolume">Double Volumes</param>
        /// <returns></returns>
        public static double ReverseConvert(double doubleVolume)
        {
            return doubleVolume * 100.0;
        }
    }
}
