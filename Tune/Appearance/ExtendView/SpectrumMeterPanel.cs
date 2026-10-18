using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Tune.Appearance.ExtendView
{
    /// <summary>
    /// Shows one bar per spectrum band. Each bar is made of separated segments and only the
    /// segments up to the band's level are lit, which is what makes the level readable at a glance.
    /// </summary>
    public class SpectrumMeterPanel : FrameworkElement
    {
        /// <summary>The bottom of the scale, in decibels below full scale.</summary>
        public const double FloorDecibels = -60;

        /// <summary>How many segments one bar is divided into.</summary>
        private const int Segments = 24;

        private static readonly Brush LitBrush = Frozen(Color.FromRgb(0x3C, 0x9B, 0xF0));
        private static readonly Brush UnlitBrush = Frozen(Color.FromRgb(0x2A, 0x2A, 0x2A));

        /// <summary>The level of each band, in decibels, from low to high.</summary>
        public static readonly DependencyProperty LevelsProperty =
            DependencyProperty.Register(
                nameof(Levels),
                typeof(IReadOnlyList<float>),
                typeof(SpectrumMeterPanel),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Gets or sets the level of each band, in decibels, from low to high.</summary>
        public IReadOnlyList<float>? Levels
        {
            get => (IReadOnlyList<float>?)GetValue(LevelsProperty);
            set => SetValue(LevelsProperty, value);
        }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double width = ActualWidth;
            double height = ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            // The grid is drawn even before any level has arrived, so an empty meter can be told
            // apart from a meter that was never rendered.
            IReadOnlyList<float>? levels = Levels;
            int barCount = levels == null || levels.Count == 0 ? 16 : levels.Count;

            double gap = 2;
            double barWidth = Math.Max(2, (width - gap * (barCount - 1)) / barCount);
double segmentHeight = height / Segments;

            // Temporary probe: one lit square at the top of the first bar as soon as any level has
            // arrived. It tells "no data" apart from "all bands are at the floor"; remove it once the
            // feed is confirmed.
            if (levels != null && levels.Count > 0)
            {
                var probe = new Rect(0, 0, barWidth, segmentHeight * 0.6);
                drawingContext.DrawRectangle(LitBrush, null, probe);
            }

            for (int band = 0; band < barCount; band++)
            {
                double x = band * (barWidth + gap);
                double level = levels != null && band < levels.Count ? levels[band] : FloorDecibels;
                double lit = (FloorDecibels - level) / -FloorDecibels;
                int litSegments = (int)Math.Round(Math.Clamp(lit, 0, 1) * Segments);

                for (int segment = 0; segment < Segments; segment++)
                {
                    // The bottom segment sits at the bottom of the control, so the bars grow upwards.
                    double y = height - (segment + 1) * segmentHeight;

                    var rect = new Rect(x, y + segmentHeight * 0.2, barWidth, segmentHeight * 0.6);
                    drawingContext.DrawRectangle(segment < litSegments ? LitBrush : UnlitBrush, null, rect);
                }
            }
        }

        private static SolidColorBrush Frozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();

            return brush;
        }
    }
}