using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Tune.Components.Slider
{
    /// <summary>
    /// A slim slider whose handle sits on the tip of the filled part. The filled part is white at
    /// 0.6 and the rest at 0.15; both brighten while the value is being changed. While the handle is
    /// held the control is also "editing", which outside code can bind to or listen for.
    /// </summary>
    public partial class CustomSlider : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(double),
                typeof(CustomSlider),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueRangeChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(double),
                typeof(CustomSlider),
                new FrameworkPropertyMetadata(0.0, OnValueRangeChanged));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(double),
                typeof(CustomSlider),
                new FrameworkPropertyMetadata(100.0, OnValueRangeChanged));

        /// <summary>
        /// Only the handle changes the value by default, which suits places where the value should
        /// not be changed by a stray click. Set it to false to let the track respond as well.
        /// </summary>
        public static readonly DependencyProperty OnlyThumbEventProperty =
            DependencyProperty.Register(
                nameof(OnlyThumbEvent),
                typeof(bool),
                typeof(CustomSlider),
                new PropertyMetadata(true, (d, _) => ((CustomSlider)d).UpdateCursor()));

        /// <summary>True while the value is being changed, so the state can be bound to.</summary>
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
                nameof(IsEditing),
                typeof(bool),
                typeof(CustomSlider),
                new PropertyMetadata(false));

        /// <summary>Raised when the value starts or stops being changed.</summary>
        public static readonly RoutedEvent EditingChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(EditingChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(CustomSlider));

        private bool _isTrackingTrack;

        /// <summary>
        /// Shows a hand over the track only when the track answers the pointer. The cursor sits on
        /// the root because the two rectangles are kept out of hit testing, so a cursor set on them
        /// would never be reached.
        /// </summary>
        private void UpdateCursor() =>
            PART_Root.Cursor = OnlyThumbEvent ? Cursors.Arrow : Cursors.Hand;

        public CustomSlider()
        {
            InitializeComponent();

            Loaded += (_, _) => UpdateVisual();
            SizeChanged += (_, _) => UpdateVisual();

            // The default value does not run the property callback, so the cursor is set once here.
            UpdateCursor();

            // The handle's own drag handles the mouse itself and marks it handled, so these only
            // see presses that land on the track.
            PART_Root.MouseLeftButtonDown += OnTrackMouseDown;
            PART_Root.MouseMove += OnTrackMouseMove;
            PART_Root.MouseLeftButtonUp += OnTrackMouseUp;
        }

        /// <summary>Gets or sets the current value, between the minimum and the maximum.</summary>
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>Gets or sets the value at the left end.</summary>
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        /// <summary>Gets or sets the value at the right end.</summary>
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether only the handle changes the value. When this is
        /// false the track also answers the pointer.
        /// </summary>
        public bool OnlyThumbEvent
        {
            get => (bool)GetValue(OnlyThumbEventProperty);
            set => SetValue(OnlyThumbEventProperty, value);
        }

        /// <summary>Gets a value indicating whether the value is being changed.</summary>
        public bool IsEditing => (bool)GetValue(IsEditingProperty);

        /// <summary>Raised when the value starts or stops being changed.</summary>
        public event RoutedEventHandler EditingChanged
        {
            add => AddHandler(EditingChangedEvent, value);
            remove => RemoveHandler(EditingChangedEvent, value);
        }

        /// <summary>Gets how much of the track is filled, from zero to one.</summary>
        private double Ratio
        {
            get
            {
                double span = Maximum - Minimum;
                return span <= 0 ? 0 : Math.Clamp((Value - Minimum) / span, 0, 1);
            }
        }

        private static void OnValueRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
            ((CustomSlider)d).UpdateVisual();

        /// <summary>
        /// Puts the handle where the filled part ends and stops the fill under the middle of the
        /// handle, so the handle always sits on the tip rather than past it. Both the track and
        /// the fill are inset by half the handle, which makes the fill start at nothing and reach
        /// the end of the track, while the handle's centre stays on the fill's right edge.
        /// </summary>
        private void UpdateVisual()
        {
            double width = PART_Root.ActualWidth;
            double handleWidth = SliderThumb.ActualWidth;

            if (width <= 0) return;

            double inset = handleWidth / 2;
            double travel = Math.Max(0, width - handleWidth);

            // The handle's centre, which is also where the fill stops.
            double tip = inset + Ratio * travel;

            PART_Rest.Margin = new Thickness(inset, 0, inset, 0);
            PART_Fill.Margin = new Thickness(inset, 0, 0, 0);
            PART_Fill.Width = Math.Max(0, tip - inset);
            SliderThumb.Margin = new Thickness(Math.Max(0, tip - inset), 0, 0, 0);
        }



        /// <summary>Brightens both parts while the value is being changed, and puts them back after.</summary>
        private void SetDragging(bool dragging)
        {
            PART_Fill.Opacity = dragging ? 1.0 : 0.6;
            PART_Rest.Opacity = dragging ? 0.3 : 0.15;

            SetValue(IsEditingProperty, dragging);
            RaiseEvent(new RoutedEventArgs(EditingChangedEvent, this));
        }

        private void OnThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            Focus();
            SetDragging(true);
        }

        private void OnThumbDragCompleted(object sender, DragCompletedEventArgs e) => SetDragging(false);

        private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double travel = Math.Max(1, PART_Root.ActualWidth - SliderThumb.ActualWidth);

            Value = Math.Clamp(
                Value + e.HorizontalChange / travel * (Maximum - Minimum),
                Minimum,
                Maximum);
        }

        private void OnTrackMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (OnlyThumbEvent || e.LeftButton != MouseButtonState.Pressed) return;

            _isTrackingTrack = true;

            Focus();
            SetDragging(true);
            SetValueFromPoint(e.GetPosition(PART_Root));

            PART_Root.CaptureMouse();
            e.Handled = true;
        }

        private void OnTrackMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isTrackingTrack || e.LeftButton != MouseButtonState.Pressed) return;

            SetValueFromPoint(e.GetPosition(PART_Root));
            e.Handled = true;
        }

        private void OnTrackMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isTrackingTrack) return;

            _isTrackingTrack = false;

            PART_Root.ReleaseMouseCapture();
            SetDragging(false);
            e.Handled = true;
        }

        /// <summary>Sets the value from a point on the track, using the same geometry as the fill.</summary>
        private void SetValueFromPoint(Point point)
        {
            double handleWidth = SliderThumb.ActualWidth;
            double travel = Math.Max(1, PART_Root.ActualWidth - handleWidth);

            double ratio = Math.Clamp((point.X - handleWidth / 2) / travel, 0, 1);
            Value = Minimum + ratio * (Maximum - Minimum);
        }
    }
}
