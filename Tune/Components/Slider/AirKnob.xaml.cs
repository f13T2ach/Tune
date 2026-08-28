using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tune.Local; // 确保 TuneSettings 可用

namespace Tune.Components
{
    public partial class AirKnob
    {
        private const double MaxAngle = 223.0;
        private Point _currentEndPoint;
        private bool _isDragging;
        private double _dragStartAngle;
        private double _dragStartRawAngle;

        // ========== 动画状态 ==========
        private double _animStartValue;
        private double _animEndValue;
        private double _animDuration = 400; // ms
        public double AnimationDelay { get; set; } = 200; // 动画前摇 ms
        private System.Diagnostics.Stopwatch _animationWatch = new();
        private bool _isAnimating;
        private bool _animationStarted;

        // ========== 设置加载标志 ==========
        private bool _isLoading = false;

        // ========== 新增依赖属性：设置键名 ==========
        public static readonly DependencyProperty SettingsKeyProperty =
            DependencyProperty.Register(
                nameof(SettingsKey),
                typeof(string),
                typeof(AirKnob),
                new PropertyMetadata(null));

        public string SettingsKey
        {
            get => (string)GetValue(SettingsKeyProperty);
            set => SetValue(SettingsKeyProperty, value);
        }

        public AirKnob()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            SizeChanged += (s, e) => UpdateFan();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 从设置加载初始值（如果指定了 SettingsKey）
            if (!string.IsNullOrEmpty(SettingsKey))
            {
                _isLoading = true;
                object settingValue = TuneSettings.Default[SettingsKey];
                if (settingValue is double doubleValue)
                {
                    Value = doubleValue;
                }
                else if (settingValue is int intValue)
                {
                    Value = intValue;
                }
                // 其他类型可扩展
                _isLoading = false;
            }
            UpdateFan();
        }

        // ========== 依赖属性 ==========
        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(AirKnob),
                new PropertyMetadata(0.0, OnValueParamsChanged));

        public double YDiff
        {
            get => (double)GetValue(YDiffProperty);
            set => SetValue(YDiffProperty, value);
        }
        public static readonly DependencyProperty YDiffProperty =
            DependencyProperty.Register(nameof(YDiff), typeof(double), typeof(AirKnob),
                new PropertyMetadata(0.0, OnValueParamsChanged));

        public double RDiff
        {
            get => (double)GetValue(RDiffProperty);
            set => SetValue(RDiffProperty, value);
        }
        public static readonly DependencyProperty RDiffProperty =
            DependencyProperty.Register(nameof(RDiff), typeof(double), typeof(AirKnob),
                new PropertyMetadata(0.0, OnValueParamsChanged));

        public double Damping
        {
            get => (double)GetValue(DampingProperty);
            set => SetValue(DampingProperty, value);
        }
        public static readonly DependencyProperty DampingProperty =
            DependencyProperty.Register(nameof(Damping), typeof(double), typeof(AirKnob),
                new PropertyMetadata(0.0));

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(AirKnob),
                new PropertyMetadata(100.0, OnValueParamsChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(AirKnob),
                new PropertyMetadata(0.0, OnValueChanged));

        private static void OnValueParamsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirKnob knob && knob.IsLoaded)
            {
                knob.UpdateFan();
            }
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirKnob knob && knob.IsLoaded)
            {
                knob.UpdateFan();
                knob.ValueTextBlock.Text = knob.Value.ToString("0");

                // ===== 自动保存逻辑（非拖动、非动画、非加载时） =====
                if (!string.IsNullOrEmpty(knob.SettingsKey) &&
                    !knob._isLoading &&
                    !knob._isDragging &&
                    !knob._isAnimating)
                {
                    knob.SaveToSettings();
                }
            }
        }

        // ========== 保存到设置 ==========
        private void SaveToSettings()
        {
            if (string.IsNullOrEmpty(SettingsKey)) return;
            TuneSettings.Default[SettingsKey] = Value;
            TuneSettings.Default.Save();
        }

        // ========== 角度计算（保持不变） ==========
        private double ComputeAngle()
        {
            if (MaxValue <= MinValue) return 0;
            double fraction = (Value - MinValue) / (MaxValue - MinValue);
            return Math.Clamp(fraction * MaxAngle, 0, MaxAngle);
        }

        private double AngleToValue(double angle)
        {
            angle = Math.Clamp(angle, 0, MaxAngle);
            if (MaxValue <= MinValue) return MinValue;
            return MinValue + (angle / MaxAngle) * (MaxValue - MinValue);
        }

        private double ComputeAngleFromPoint(Point point)
        {
            double size = Math.Min(ActualWidth, ActualHeight);
            double radius = size / 2.0 - 3;
            Point center = new Point(ActualWidth / 2.0, ActualHeight / 2.0 - YDiff);
            Vector v = point - center;
            double radians = Math.Atan2(v.Y, v.X);
            double angle = radians * 180.0 / Math.PI;
            if (angle < 0) angle += 360.0;
            return angle;
        }

        // ========== 扇形绘制（保持不变） ==========
        private void UpdateFan()
        {
            double angle = ComputeAngle();
            if (ActualWidth < 1 || ActualHeight < 1)
            {
                FanFillPath.Data = null;
                FanStrokePath.Data = null;
                return;
            }

            double size = Math.Min(ActualWidth, ActualHeight);
            double radius = size / 2.0 - 3 - RDiff;
            double strokeHalf = 1.5;
            double innerRadius = radius - strokeHalf;
            Point center = new Point(ActualWidth / 2.0, ActualHeight / 2.0 - YDiff);
            Point startInner = new Point(center.X + innerRadius, center.Y);

            if (angle <= 0)
            {
                FanFillPath.Data = null;
                var baseLineGeo = new PathGeometry();
                var lineFigure = new PathFigure() { StartPoint = center };
                lineFigure.Segments.Add(new LineSegment(startInner, true));
                baseLineGeo.Figures.Add(lineFigure);
                FanStrokePath.Data = baseLineGeo;
                _currentEndPoint = startInner;
                return;
            }

            double rad = angle * Math.PI / 180.0;
            Point endOuter = new Point(center.X + radius * Math.Cos(rad),
                                       center.Y + radius * Math.Sin(rad));
            Point endInner = new Point(center.X + innerRadius * Math.Cos(rad),
                                       center.Y + innerRadius * Math.Sin(rad));

            _currentEndPoint = endOuter;

            bool isLargeArc = angle > 180;
            var fillFigure = new PathFigure(
                center,
                new List<PathSegment>
                {
                    new LineSegment(new Point(center.X + radius, center.Y), true),
                    new ArcSegment(endOuter, new Size(radius, radius), 0,
                                   isLargeArc, SweepDirection.Clockwise, true)
                }, true);
            var geometry = new PathGeometry(new List<PathFigure> { fillFigure });
            geometry.Freeze();
            FanFillPath.Data = geometry;

            var borderGeometry = new PathGeometry();
            var radius1 = new PathFigure() { StartPoint = center };
            radius1.Segments.Add(new LineSegment(startInner, true));
            borderGeometry.Figures.Add(radius1);

            var radius2 = new PathFigure() { StartPoint = center };
            radius2.Segments.Add(new LineSegment(endInner, true));
            borderGeometry.Figures.Add(radius2);

            var arcFigure = new PathFigure() { StartPoint = startInner };
            arcFigure.Segments.Add(
                new ArcSegment(endInner, new Size(innerRadius, innerRadius), 0,
                               isLargeArc, SweepDirection.Clockwise, true));
            borderGeometry.Figures.Add(arcFigure);
            borderGeometry.Freeze();
            FanStrokePath.Data = borderGeometry;
            ValueTextBlock.Text = ((int)Value).ToString("D2");

            if (_isDragging)
                UpdateDotPosition();
        }

        // ========== 白点定位 ==========
        private void UpdateDotPosition()
        {
            double halfSize = DragDot.Width / 2.0;
            Canvas.SetLeft(DragDot, _currentEndPoint.X - halfSize);
            Canvas.SetTop(DragDot, _currentEndPoint.Y - halfSize);
        }

        // ========== 动画 ==========
        public void AnimateValueTo(double targetValue)
        {
            if (_isDragging)
                return;

            targetValue = Math.Clamp(targetValue, MinValue, MaxValue);
            if (Math.Abs(Value - targetValue) < 0.01)
                return;

            _animStartValue = Value;
            _animEndValue = targetValue;
            _animationWatch.Restart();
            _animationStarted = false;
            _isAnimating = true;

            CompositionTarget.Rendering -= AnimationRendering;
            CompositionTarget.Rendering += AnimationRendering;
        }

        private void AnimationRendering(object? sender, EventArgs e)
        {
            if (!_isAnimating)
                return;

            double elapsed = _animationWatch.Elapsed.TotalMilliseconds;

            if (!_animationStarted)
            {
                if (elapsed < AnimationDelay)
                {
                    Value = _animStartValue;
                    return;
                }
                _animationStarted = true;
                _animationWatch.Restart();
                return;
            }

            double progress = Math.Clamp(
                _animationWatch.Elapsed.TotalMilliseconds / _animDuration,
                0, 1);
            double easedProgress = progress * progress * (3 - 2 * progress);
            Value = _animStartValue + (_animEndValue - _animStartValue) * easedProgress;

            if (progress >= 1)
            {
                Value = _animEndValue;
                _isAnimating = false;
                _animationWatch.Stop();
                CompositionTarget.Rendering -= AnimationRendering;
                // 动画结束，保存一次（因为最后一次 Value 赋值会触发 OnValueChanged）
            }
        }

        private void StopAnimationAndJumpToEnd()
        {
            if (_isAnimating)
            {
                _isAnimating = false;
                _animationWatch.Stop();
                CompositionTarget.Rendering -= AnimationRendering;
                Value = _animEndValue; // 触发保存（因为 _isAnimating = false）
            }
        }

        // ========== 鼠标交互 ==========
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StopAnimationAndJumpToEnd();

            ValueTextBlock.Visibility = Visibility.Visible;
            ValueTextBlock.Text = ((int)Value).ToString("D2");

            Point pos = e.GetPosition(this);
            if (IsNearEndPoint(pos, _currentEndPoint, threshold: 20))
            {
                _isDragging = true;
                _dragStartAngle = ComputeAngle();
                _dragStartRawAngle = ComputeAngleFromPoint(pos);

                DragDot.Visibility = Visibility.Visible;
                UpdateDotPosition();

                CaptureMouse();
                e.Handled = true;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point pos = e.GetPosition(this);
                double rawAngle = ComputeAngleFromPoint(pos);

                double delta = rawAngle - _dragStartRawAngle;
                delta = ((delta + 180) % 360 + 360) % 360 - 180;

                double effectiveDelta = delta / (1.0 + Damping);
                double effectiveAngle = _dragStartAngle + effectiveDelta;
                effectiveAngle = Math.Clamp(effectiveAngle, 0, MaxAngle);

                double newValue = AngleToValue(effectiveAngle);
                double change = Math.Abs(newValue - Value);
                double maxAllowedChange = (MaxValue - MinValue) / 2.0;

                if (change <= maxAllowedChange)
                {
                    Value = newValue; // 拖动过程中会触发 OnValueChanged，但 _isDragging 为 true，不保存
                }
                else
                {
                    _dragStartAngle = ComputeAngle();
                    _dragStartRawAngle = rawAngle;
                }
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ValueTextBlock.Visibility = Visibility.Collapsed;
            DragDot.Visibility = Visibility.Collapsed;

            if (_isDragging)
            {
                // 拖动结束，保存最终值
                if (!string.IsNullOrEmpty(SettingsKey))
                    SaveToSettings();

                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private bool IsNearEndPoint(Point p, Point endPoint, double threshold = 20)
        {
            return (p - endPoint).LengthSquared < threshold * threshold;
        }
    }
}