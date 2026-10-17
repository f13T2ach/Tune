using System;

namespace Tune.Appearance.ExtendView
{
    /// <summary>Which colour band a level falls into, from the loudest down.</summary>
    public enum PeakMeterBand
    {
        /// <summary>Below the floor, so nothing is shown.</summary>
        Silent,

        /// <summary>Below the warning line, drawn green.</summary>
        Normal,

        /// <summary>Between the warning line and the clipping line, drawn orange.</summary>
        Warning,

        /// <summary>At or above the clipping line, drawn red.</summary>
        Clip
    }

    /// <summary>
    /// The scale of the level meter: 0 dB at the top and -60 dB at the bottom, divided into red
    /// from 0 to -2, orange from -2 to -14 and green below that.
    /// </summary>
    public static class PeakMeterScale
    {
        /// <summary>The bottom of the scale in decibels.</summary>
        public const double FloorDecibels = -60;

        /// <summary>Where red gives way to orange.</summary>
        public const double ClipDecibels = -2;

        /// <summary>Where orange gives way to green.</summary>
        public const double WarningDecibels = -14;

        /// <summary>Gets the band a level falls into.</summary>
        /// <param name="decibels">The level, in decibels below full scale.</param>
        public static PeakMeterBand BandOf(double decibels) =>
            decibels >= ClipDecibels ? PeakMeterBand.Clip
            : decibels >= WarningDecibels ? PeakMeterBand.Warning
            : decibels >= FloorDecibels ? PeakMeterBand.Normal
            : PeakMeterBand.Silent;

        /// <summary>
        /// Gets how far down the bar a level sits: zero at the top, one at the floor. Anything at or
        /// below the floor sits at the bottom, anything above full scale at the top.
        /// </summary>
        /// <param name="decibels">The level, in decibels below full scale.</param>
        public static double FractionOf(double decibels)
        {
            double clamped = Math.Clamp(decibels, FloorDecibels, 0);

            return (0 - clamped) / (0 - FloorDecibels);
        }
    }
}
