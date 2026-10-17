using System;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Tune.Appearance.ExtendView
{
    /// <summary>
    /// The level meter, drawn entirely in code so every part keeps the proportions of the design.
    /// The whole scale is drawn dim in its three colours; the level is the bright bar that grows
    /// upwards from the floor and takes the colour of every zone it covers. A short strip under
    /// each bar tells the colour of the last sampling window's peak, and a window that ran past
    /// full scale turns the whole bar red as a warning.
    /// </summary>
    public class PeakMeterPanel : FrameworkElement
    {
        /// <summary>The proportion of the meter's frame, from the design: 115 wide by 310 tall.</summary>
        private const double FrameAspect = 310.0 / 115.0;

        /// <summary>The proportion of the clip strip, from the design: 109 wide by 34 tall.</summary>
        private const double ClipAspect = 14.0 / 109.0;
        private const double InnerOpacity = 0.45;
        private const double TickStepDecibels = 5;
        private const double BandDecibels = 2;

        private static readonly Brush ClipBrush = Frozen(Color.FromRgb(0xFF, 0x00, 0x00));
        private static readonly Brush WarningBrush = Frozen(Color.FromRgb(0xE5, 0xAF, 0x24));
        private static readonly Brush NormalBrush = Frozen(Color.FromRgb(0x37, 0xD2, 0x47));

        private static readonly double[] ScaleDecibels = [0, -6, -12, -18, -24, -30, -35, -42, -48, -54, -60];

        public static readonly DependencyProperty LeftLevelProperty = LevelProperty(nameof(LeftLevel));
        public static readonly DependencyProperty RightLevelProperty = LevelProperty(nameof(RightLevel));
        public static readonly DependencyProperty LeftAverageProperty = LevelProperty(nameof(LeftAverage));
        public static readonly DependencyProperty RightAverageProperty = LevelProperty(nameof(RightAverage));
        public static readonly DependencyProperty LeftPeakProperty = LevelProperty(nameof(LeftPeak));
        public static readonly DependencyProperty RightPeakProperty = LevelProperty(nameof(RightPeak));

        public static readonly DependencyProperty LeftWindowPeakProperty = LevelProperty(nameof(LeftWindowPeak));
        public static readonly DependencyProperty RightWindowPeakProperty = LevelProperty(nameof(RightWindowPeak));

        public static readonly DependencyProperty IsClippingProperty =
            DependencyProperty.Register(
                nameof(IsClipping),
                typeof(bool),
                typeof(PeakMeterPanel),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public double LeftLevel { get => (double)GetValue(LeftLevelProperty); set => SetValue(LeftLevelProperty, value); }

        public double RightLevel { get => (double)GetValue(RightLevelProperty); set => SetValue(RightLevelProperty, value); }

        public double LeftAverage { get => (double)GetValue(LeftAverageProperty); set => SetValue(LeftAverageProperty, value); }

        public double RightAverage { get => (double)GetValue(RightAverageProperty); set => SetValue(RightAverageProperty, value); }

        public double LeftPeak { get => (double)GetValue(LeftPeakProperty); set => SetValue(LeftPeakProperty, value); }

        public double RightPeak { get => (double)GetValue(RightPeakProperty); set => SetValue(RightPeakProperty, value); }

        public double LeftWindowPeak { get => (double)GetValue(LeftWindowPeakProperty); set => SetValue(LeftWindowPeakProperty, value); }

        public double RightWindowPeak { get => (double)GetValue(RightWindowPeakProperty); set => SetValue(RightWindowPeakProperty, value); }

        public bool IsClipping { get => (bool)GetValue(IsClippingProperty); set => SetValue(IsClippingProperty, value); }

        /// <inheritdoc/>
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (ActualWidth <= 0 || ActualHeight <= 0)
                return;

            double padding = 12;
            double scaleWidth = 68.2;
            double gap = 0;

            double byWidth = (ActualWidth - scaleWidth - padding * 2 - gap * 2) / 2;
            double byHeight = (ActualHeight - padding * 2 - gap)* 0.85 / (FrameAspect + ClipAspect);    // 0.85 is the bset ratio 
            double barWidth = Math.Max(4, Math.Min(byWidth, byHeight));
            double barHeight = barWidth * FrameAspect;
            double clipHeight = barWidth * ClipAspect;

            // Room is reserved above the bars for the channel letters
            double nameReserve = barWidth * 0.55;
            double contentHeight = nameReserve + barHeight + 14 + clipHeight;
            double top = (ActualHeight - contentHeight) / 2 + nameReserve;

            // The two bars and the scale are centred as one group instead of being pinned to the
            // two sides of the panel.
            double groupWidth = barWidth * 2 + scaleWidth + gap * 2 + 8;
            double leftX = Math.Max(padding, (ActualWidth - groupWidth) / 2 + 4);
            double rightX = leftX + barWidth + scaleWidth + gap * 2;

            drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

            DrawChannel(drawingContext, leftX, top, barWidth, barHeight, clipHeight, gap, "L", LeftLevel, LeftAverage, LeftPeak, LeftWindowPeak);
            DrawChannel(drawingContext, rightX, top, barWidth, barHeight, clipHeight, gap, "R", RightLevel, RightAverage, RightPeak, RightWindowPeak);

            DrawScale(drawingContext, leftX + barWidth + gap, rightX - gap, top, barHeight);

            drawingContext.Pop();
        }

        private void DrawChannel(
            DrawingContext drawingContext,
            double x,
            double top,
            double width,
            double height,
            double clipHeight,
            double gap,
            string name,
            double level,
            double average,
            double peak,
            double windowPeak)
        {
            var area = new Rect(x, top, width, height);

            if (IsClipping)
            {
                drawingContext.DrawRectangle(ClipBrush, null, area);
            }
            else
            {
                DrawSegments(drawingContext, area, InnerOpacity);
                // The whole scale is drawn again at full strength, clipped to the level, so each
                // colour keeps its own place instead of being stretched into the lit part.
                drawingContext.PushClip(new RectangleGeometry(
                    BandOf(area, level, PeakMeterScale.FloorDecibels)));
                DrawSegments(drawingContext, area, 1.0);
                drawingContext.Pop();
            }

            // The average is never drawn above the level, so it always sits inside the lit part
            // the way it does in the meters this follows.
            double averageShown = Math.Min(average, level);
            drawingContext.DrawRectangle(Brushes.Black, null, BandOf(area, averageShown, averageShown - BandDecibels));

            // Filled peak band: the highest level of the track, 2 dB tall, in the colour of the zone it sits in rather than white.
            drawingContext.DrawRectangle(ColorOf(peak), null, BandOf(area, peak - BandDecibels, peak));



            double boundary = Math.Round(BandOf(area, PeakMeterScale.WarningDecibels, PeakMeterScale.WarningDecibels).Top) + 0.5;
            drawingContext.DrawLine(
                new Pen(Brushes.White, 3),
                new Point(x - 8, boundary),
                new Point(x + width + 8, boundary));

            drawingContext.DrawRectangle(null, new Pen(Brushes.White, 3.5), new Rect(x - 1.75, top - 1.75, width + 3.5, height + 3.5));

            var clipArea = new Rect(x, top + height + 14, width, clipHeight);
            drawingContext.DrawRectangle(ColorOf(windowPeak), null, clipArea);
            drawingContext.DrawRectangle(null, new Pen(Brushes.White, 3.5), new Rect(clipArea.X - 1.75, clipArea.Y - 1.5, clipArea.Width + 3.5, clipArea.Height + 3.5));

            double nameX = name == "L" ? x - width * 0.3 : x + width * 0.95;
            DrawText(drawingContext, name, new Point(nameX, top - width * 0.45), width * 0.34, false, true);
        }

        private static void DrawSegments(DrawingContext drawingContext, Rect area, double opacity)
        {
            DrawColor(drawingContext, BandOf(area, 0, PeakMeterScale.ClipDecibels), ClipBrush, opacity);
            DrawColor(drawingContext, BandOf(area, PeakMeterScale.ClipDecibels, PeakMeterScale.WarningDecibels), WarningBrush, opacity);
            DrawColor(drawingContext, BandOf(area, PeakMeterScale.WarningDecibels, PeakMeterScale.FloorDecibels), NormalBrush, opacity);
        }

        private static Brush ColorOf(double decibels) =>
            PeakMeterScale.BandOf(decibels) switch
            {
                PeakMeterBand.Clip => ClipBrush,
                PeakMeterBand.Warning => WarningBrush,
                _ => NormalBrush
            };

        private static void DrawColor(DrawingContext drawingContext, Rect area, Brush brush, double opacity)
        {
            if (area.Height <= 0)
                return;

            if (opacity >= 1)
            {
                drawingContext.DrawRectangle(brush, null, area);
                return;
            }

            drawingContext.PushOpacity(opacity);
            drawingContext.DrawRectangle(brush, null, area);
            drawingContext.Pop();
        }

        private void DrawScale(DrawingContext drawingContext, double left, double right, double top, double height)
        {
            var area = new Rect(left, top, Math.Max(2, right - left), height);

            var pen = new Pen(Frozen(Colors.White, 0.45), 3);

            foreach (double decibels in ScaleDecibels)
            {
                double y = Math.Round(BandOf(area, decibels, decibels).Top) + 0.5;

                // A short mark on either side of the number; nothing is drawn inside the bars.
                drawingContext.DrawLine(pen, new Point(area.Left, y), new Point(area.Left + 9, y));
                drawingContext.DrawLine(pen, new Point(area.Right - 9, y), new Point(area.Right, y));

                DrawText(drawingContext, ((int)decibels).ToString(CultureInfo.InvariantCulture),
                    new Point(area.Left + area.Width / 2, y - area.Height * 0.07), area.Height * 0.099, false, true, true);
            }
        }

        private static Rect BandOf(Rect area, double from, double to)
        {
            double a = PeakMeterScale.FractionOf(from);
            double b = PeakMeterScale.FractionOf(to);

            return new Rect(
                area.Left,
                area.Top + Math.Min(a, b) * area.Height,
                area.Width,
                Math.Abs(b - a) * area.Height);
        }

        private void DrawText(
            DrawingContext drawingContext,
            string text,
            Point at,
            double size,
            bool rightAlign = false,
            bool bold = false,
            bool centre = false)
        {
            FontFamily family = TryFindResource("MainFont") as FontFamily
                ?? TextElement.GetFontFamily(this);

            var typeface = new Typeface(
                family,
                FontStyles.Normal,
                bold ? FontWeights.SemiBold : FontWeights.Normal,
                FontStretches.Normal);

            var formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                typeface,
                size,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            Point place = centre
                ? new Point(at.X - formatted.Width / 2, at.Y)
                : rightAlign
                    ? new Point(at.X - formatted.Width, at.Y)
                    : at;

            drawingContext.DrawText(formatted, place);
        }

        private static DependencyProperty LevelProperty(string name) =>
            DependencyProperty.Register(
                name,
                typeof(double),
                typeof(PeakMeterPanel),
                new FrameworkPropertyMetadata(-100.0, FrameworkPropertyMetadataOptions.AffectsRender));

        private static SolidColorBrush Frozen(Color color, double opacity = 1)
        {
            var brush = new SolidColorBrush(color) { Opacity = opacity };
            brush.Freeze();

            return brush;
        }
    }
}