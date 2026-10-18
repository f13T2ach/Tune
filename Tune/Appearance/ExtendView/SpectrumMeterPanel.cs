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
            IReadOnlyList<float>? levels = Levels;

            if (width <= 0 || height <= 0 || levels == null || levels.Count == 0)
                return;

            double gap = 2;
            double barWidth = Math.Max(2, (width - gap * (levels.Count - 1)) / levels.Count);
            double segmentHeight = height / Segments;

            for (int band = 0; band < levels.Count; band++)
            {
                double x = band * (barWidth + gap);
                double lit = (FloorDecibels - levels[band]) / -FloorDecibels;
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