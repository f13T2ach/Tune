using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Tune.Appearance.ExtendView
{
    public class SpectrumMeterPanel : FrameworkElement
    {
        /// <summary>The bottom of the scale, in decibels below full scale.</summary>
        public const double FloorDecibels = -60;

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

            double gap = 4;
            double lineWidth = Math.Max(1, (width - gap * (barCount - 1)) / barCount);

            for (int band = 0; band < barCount; band++)
            {
                double x = band * (lineWidth + gap);
                double level = levels != null && band < levels.Count ? levels[band] : FloorDecibels;

                // How much of the height the band covers: the floor is 0, full scale is 1.
                double lit = Math.Clamp((level - FloorDecibels) / -FloorDecibels, 0, 1);
                double lineHeight = lit * height;

                if (lineHeight <= 0)
                    continue;

                drawingContext.DrawRectangle(
                    LitBrush,
                    null,
                    new Rect(x, height - lineHeight, lineWidth, lineHeight));
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