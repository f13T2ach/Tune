using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Tune.Components.Slider
{
    /// <summary>
    /// One band of a graphic equalizer: a vertical fader that pulls green above the
    /// centre and red below it, with the gain on top and the centre frequency below.
    /// </summary>
    public partial class EqFader : UserControl
    {
        /// <summary>Height of the bar the user drags, in pixels.</summary>
        private const double ThumbHeight = 8;

        private bool _isDragging;

        // ==== Dependency properties ====

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(EqFader),
                new FrameworkPropertyMetadata(0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(EqFader),
                new PropertyMetadata(0.0, OnRangeChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(EqFader),
                new PropertyMetadata(0.0, OnRangeChanged));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(nameof(Step), typeof(double), typeof(EqFader),
                new PropertyMetadata(1.0, OnRangeChanged));

        public static readonly DependencyProperty FrequencyProperty =
            DependencyProperty.Register(nameof(Frequency), typeof(double), typeof(EqFader),
                new PropertyMetadata(0.0, OnFrequencyChanged));

        /// <summary>Gain of this band.</summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>Gain at the bottom of the fader.</summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>Gain at the top of the fader.</summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>Gain the fader moves by while dragging.</summary>
        public double Step
        {
            get { return (double)GetValue(StepProperty); }
            set { SetValue(StepProperty, value); }
        }

        /// <summary>Centre frequency of this band, shown below the fader.</summary>
        public double Frequency
        {
            get { return (double)GetValue(FrequencyProperty); }
            set { SetValue(FrequencyProperty, value); }
        }

        // ==== Constructor ====

        public EqFader()
        {
            InitializeComponent();

            // Defaults come from the settings; a caller can still override them in XAML.
            SetCurrentValue(MinimumProperty, Local.TuneSettings.Default.EffectEqBandMinimum);
            SetCurrentValue(MaximumProperty, Local.TuneSettings.Default.EffectEqBandMaximum);
            SetCurrentValue(StepProperty, Local.TuneSettings.Default.EffectEqBandStep);

            UpdateValueText();
            UpdateFrequencyText();
            UpdateGeometry();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fader = (EqFader)d;
            fader.UpdateValueText();
            fader.UpdateGeometry();
        }

        private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EqFader)d).UpdateGeometry();
        }

        private static void OnFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((EqFader)d).UpdateFrequencyText();
        }

        // ==== Layout ====

        private void OnTrackAreaSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGeometry();
        }

        /// <summary>Places the fill and the drag bar for the current gain.</summary>
        private void UpdateGeometry()
        {
            double height = PART_TrackArea.ActualHeight;
            if (height <= 0) return;

            double thumbTop = PositionFromValue(Value, height, Minimum, Maximum);
            PART_ThumbBar.Margin = new Thickness(0, thumbTop, 0, 0);

            // The fill runs from the centre of the fader to the centre of the drag bar.
            double valueCentre = thumbTop + ThumbHeight / 2;
            double middle = height / 2;
            double top = Math.Min(valueCentre, middle);
            double bottom = Math.Max(valueCentre, middle);

            PART_FillBar.Margin = new Thickness(0, top, 0, 0);
            PART_FillBar.Height = Math.Max(0, bottom - top);
            PART_FillBar.Fill = (System.Windows.Media.Brush)FindResource(
                Value < 0 ? "NegativeFillBrush" : "PositiveFillBrush");

            // The centre marker is only worth showing once the bar has left it.
            PART_CenterBar.Visibility = Math.Abs(Value) > 1e-9 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateValueText()
        {
            PART_ValueText.Text = FormatValue(Value);
        }

        private void UpdateFrequencyText()
        {
            PART_FrequencyText.Text = FormatFrequency(Frequency);
        }

        // ==== Dragging ====

        private void OnTrackAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            PART_TrackArea.CaptureMouse();
            ApplyPosition(e.GetPosition(PART_TrackArea).Y);
            e.Handled = true;
        }

        private void OnTrackAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            ApplyPosition(e.GetPosition(PART_TrackArea).Y);
        }

        private void OnTrackAreaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging) return;

            _isDragging = false;
            PART_TrackArea.ReleaseMouseCapture();
            e.Handled = true;
        }

        /// <summary>Moves the gain to whatever the pointer position asks for.</summary>
        private void ApplyPosition(double y)
        {
            Value = ValueFromPosition(y, PART_TrackArea.ActualHeight, Minimum, Maximum, Step);
        }

        // ==== Editing the gain by hand ====

        private void OnValueTextClicked(object sender, MouseButtonEventArgs e)
        {
            PART_ValueInput.Text = FormatValue(Value);
            PART_ValueText.Visibility = Visibility.Collapsed;
            PART_ValueInput.Visibility = Visibility.Visible;

            // The input box owns a text box, so focus has to be handed to it once it is visible.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox? box = FindChild<TextBox>(PART_ValueInput);
                if (box == null) return;

                box.Focus();
                box.SelectAll();
            }), DispatcherPriority.Input);

            // Clicking outside the box does not always move keyboard focus, so the end of the
            // edit is watched at the window instead of relying on LostFocus alone.
            Window? window = Window.GetWindow(this);
            if (window != null)
            {
                window.PreviewMouseDown -= OnAnywhereMouseDown;
                window.PreviewMouseDown += OnAnywhereMouseDown;
            }

            e.Handled = true;
        }

        /// <summary>Finishes the edit when the user presses anywhere outside the input box.</summary>
        private void OnAnywhereMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PART_ValueInput.Visibility != Visibility.Visible) return;
            if (PART_ValueInput.IsMouseOver) return;

            if (sender is Window window) window.PreviewMouseDown -= OnAnywhereMouseDown;

            CommitValueInput();
        }

        private void OnValueInputLostFocus(object sender, RoutedEventArgs e)
        {
            CommitValueInput();
        }

        private void OnValueInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                CommitValueInput();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                EndValueEditing();
                e.Handled = true;
            }
        }

        /// <summary>Takes the typed gain, or keeps the old one when it cannot be read.</summary>
        private void CommitValueInput()
        {
            if (PART_ValueInput.Visibility != Visibility.Visible) return;

            ApplyTypedValue(PART_ValueInput.Text);
            EndValueEditing();
        }

        /// <summary>Applies a gain typed as text, snapped and clamped like a drag would be.</summary>
        internal void ApplyTypedValue(string? text)
        {
            if (TryParseValue(text, out double typed))
                Value = SnapValue(typed, Minimum, Maximum, Step);
        }

        private void EndValueEditing()
        {
            PART_ValueInput.Visibility = Visibility.Collapsed;
            PART_ValueText.Visibility = Visibility.Visible;
            UpdateValueText();
        }

        /// <summary>Reads a typed gain, accepting either the current or the invariant separator.</summary>
        internal static bool TryParseValue(string? text, out double value)
        {
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return true;

            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        // ==== Pure layout and text helpers ====

        /// <summary>Formats a gain for the readout above the fader.</summary>
        internal static string FormatValue(double value) =>
            value.ToString("0.##", CultureInfo.CurrentCulture);

        /// <summary>
        /// Formats a centre frequency: values below one kilohertz keep their number,
        /// larger ones are shown in kilohertz, with decimals where they are needed.
        /// </summary>
        internal static string FormatFrequency(double frequency)
        {
            if (Math.Abs(frequency) < 1000)
                return frequency.ToString("0.##", CultureInfo.CurrentCulture);

            return (frequency / 1000.0).ToString("0.##", CultureInfo.CurrentCulture) + "k";
        }

        /// <summary>Moves a gain onto the nearest step and inside the range.</summary>
        internal static double SnapValue(double value, double minimum, double maximum, double step)
        {
            double low = Math.Min(minimum, maximum);
            double high = Math.Max(minimum, maximum);
            double stride = step > 0 ? step : 1.0;

            double snapped = low + Math.Round((value - low) / stride) * stride;

            // The rounding above can leave a tiny remainder behind.
            return Math.Clamp(Math.Round(snapped, 6), low, high);
        }

        /// <summary>Turns a pointer position inside the fader into a gain.</summary>
        internal static double ValueFromPosition(double y, double height, double minimum, double maximum, double step)
        {
            double usable = Math.Max(1.0, height - ThumbHeight);
            double ratio = 1.0 - Math.Clamp((y - ThumbHeight / 2) / usable, 0.0, 1.0);

            return SnapValue(minimum + ratio * (maximum - minimum), minimum, maximum, step);
        }

        /// <summary>Turns a gain into the offset of the top edge of the drag bar.</summary>
        internal static double PositionFromValue(double value, double height, double minimum, double maximum)
        {
            double usable = Math.Max(1.0, height - ThumbHeight);
            double span = maximum - minimum;
            double ratio = span == 0 ? 0.0 : Math.Clamp((value - minimum) / span, 0.0, 1.0);

            return usable * (1.0 - ratio);
        }

        /// <summary>Finds the first child of the given type below an element.</summary>
        private static T? FindChild<T>(DependencyObject root) where T : DependencyObject
        {
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T typed) return typed;

                T? nested = FindChild<T>(child);
                if (nested != null) return nested;
            }

            return null;
        }
    }
}
