using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tune.Local;

namespace Tune.Components
{
    public partial class AirKnob
    {
        private const double MaxAngle = 223.0;
        private const double StrokeHalfWidth = 1.5;
        private const double DefaultAnimationDuration = 400; // ms
        private const double NearEndPointThreshold = 20.0;

        private Point _currentEndPoint;
        private bool _isDragging;
        private double _dragStartAngle;
        private double _dragStartRawAngle;

        // ========== 动画状态 ==========
        private double _animStartValue;
        private double _animEndValue;
        private double _animDuration = DefaultAnimationDuration;
        public double AnimationDelay { get; set; } = 200;
        private readonly Stopwatch _animationWatch = new();
        private bool _isAnimating;
        private bool _animationStarted;
        private bool _isLoading;

        // ========== 依赖属性 ==========
        public static readonly DependencyProperty SettingsKeyProperty =
            DependencyProperty.Register(nameof(SettingsKey), typeof(string), typeof(AirKnob),
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
            if (!string.IsNullOrEmpty(SettingsKey))
            {
                _isLoading = true;
                if (TuneSettings.Default[SettingsKey] is double d)
                    Value = d;
                else if (TuneSettings.Default[SettingsKey] is int i)
                    Value = i;
                _isLoading = false;
            }
            UpdateFan();
        }

        // ========== 依赖属性定义 ==========
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
                knob.UpdateFan();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AirKnob knob && knob.IsLoaded)
            {
                knob.UpdateFan();
                knob.ValueTextBlock.Text = knob.Value.ToString("0");
                knob.TrySaveToSettings();
            }
        }

        // ========== 保存逻辑 ==========
        private void TrySaveToSettings()
        {
            if (string.IsNullOrEmpty(SettingsKey)) return;
            if (_isLoading || _isDragging || _isAnimating) return;
            SaveToSettings();
        }

        private void SaveToSettings()
        {
            TuneSettings.Default[SettingsKey] = Value;
            TuneSettings.Default.Save();
        }

        // ========== 角度计算 ==========
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
            Point center = new(ActualWidth / 2.0, ActualHeight / 2.0 - YDiff);
            Vector v = point - center;
            double angle = Math.Atan2(v.Y, v.X) * 180.0 / Math.PI;
            if (angle < 0) angle += 360.0;
            return angle;
        }

        // ========== 核心绘制（重构后） ==========
        private void UpdateFan()
        {
            if (!ValidateSize()) return;

            var geo = BuildGeometry();
            ApplyGeometry(geo);

            UpdateValueText();
            if (_isDragging)
                UpdateDotPosition();
        }

        private bool ValidateSize()
        {
            if (ActualWidth < 1 || ActualHeight < 1)
            {
                FanFillPath.Data = null;
                FanStrokePath.Data = null;
                return false;
            }
            return true;
        }

        private (PathGeometry fill, PathGeometry stroke, Point endPoint) BuildGeometry()
        {
            double angle = ComputeAngle();
            double size = Math.Min(ActualWidth, ActualHeight);
            double radius = size / 2.0 - 3 - RDiff;
            double innerRadius = radius - StrokeHalfWidth;
            Point center = new(ActualWidth / 2.0, ActualHeight / 2.0 - YDiff);

            if (angle <= 0)
                return BuildZeroAngleGeometry(center, innerRadius);

            return BuildPositiveAngleGeometry(center, radius, innerRadius, angle);
        }

        private (PathGeometry fill, PathGeometry stroke, Point endPoint) BuildZeroAngleGeometry(Point center, double innerRadius)
        {
            Point startInner = new(center.X + innerRadius, center.Y);
            var lineFig = new PathFigure { StartPoint = center };
            lineFig.Segments.Add(new LineSegment(startInner, true));
            var stroke = new PathGeometry();
            stroke.Figures.Add(lineFig);
            return (null, stroke, startInner);
        }

        private (PathGeometry fill, PathGeometry stroke, Point endPoint) BuildPositiveAngleGeometry(
            Point center, double radius, double innerRadius, double angleDeg)
        {
            double rad = angleDeg * Math.PI / 180.0;
            Point endOuter = new(
                center.X + radius * Math.Cos(rad),
                center.Y + radius * Math.Sin(rad));
            Point endInner = new(
                center.X + innerRadius * Math.Cos(rad),
                center.Y + innerRadius * Math.Sin(rad));

            bool isLargeArc = angleDeg > 180;

            // --- 填充扇形 ---
            var fillFigure = new PathFigure { StartPoint = center };
            fillFigure.Segments.Add(new LineSegment(new Point(center.X + radius, center.Y), true));
            fillFigure.Segments.Add(new ArcSegment(endOuter, new Size(radius, radius), 0,
                isLargeArc, SweepDirection.Clockwise, true));
            fillFigure.IsClosed = true;
            var fillGeometry = new PathGeometry(new List<PathFigure> { fillFigure });
            fillGeometry.Freeze();

            // --- 描边 ---
            var strokeGeometry = new PathGeometry();

            // 半径1
            var radius1Fig = new PathFigure { StartPoint = center };
            radius1Fig.Segments.Add(new LineSegment(new Point(center.X + innerRadius, center.Y), true));
            strokeGeometry.Figures.Add(radius1Fig);

            // 半径2
            var radius2Fig = new PathFigure { StartPoint = center };
            radius2Fig.Segments.Add(new LineSegment(endInner, true));
            strokeGeometry.Figures.Add(radius2Fig);

            // 内弧
            var arcFig = new PathFigure { StartPoint = new Point(center.X + innerRadius, center.Y) };
            arcFig.Segments.Add(new ArcSegment(endInner, new Size(innerRadius, innerRadius), 0,
                isLargeArc, SweepDirection.Clockwise, true));
            strokeGeometry.Figures.Add(arcFig);
            strokeGeometry.Freeze();

            return (fillGeometry, strokeGeometry, endOuter);
        }

        private void ApplyGeometry((PathGeometry fill, PathGeometry stroke, Point endPoint) geo)
        {
            FanFillPath.Data = geo.fill;
            FanStrokePath.Data = geo.stroke;
            _currentEndPoint = geo.endPoint;
        }

        private void UpdateValueText()
        {
            ValueTextBlock.Text = ((int)Value).ToString("D2");
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
            if (_isDragging) return;

            targetValue = Math.Clamp(targetValue, MinValue, MaxValue);
            if (Math.Abs(Value - targetValue) < 0.01) return;

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
            if (!_isAnimating) return;

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

            double progress = Math.Clamp(elapsed / _animDuration, 0, 1);
            double eased = progress * progress * (3 - 2 * progress);
            Value = _animStartValue + (_animEndValue - _animStartValue) * eased;

            if (progress >= 1)
            {
                Value = _animEndValue;
                _isAnimating = false;
                _animationWatch.Stop();
                CompositionTarget.Rendering -= AnimationRendering;
                // 最后 Value 变更会触发 TrySaveToSettings（因 _isAnimating 已 false）
            }
        }

        private void StopAnimationAndJumpToEnd()
        {
            if (_isAnimating)
            {
                _isAnimating = false;
                _animationWatch.Stop();
                CompositionTarget.Rendering -= AnimationRendering;
                Value = _animEndValue; // 触发保存逻辑
            }
        }

        // ========== 鼠标交互 ==========
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StopAnimationAndJumpToEnd();

            ValueTextBlock.Visibility = Visibility.Visible;
            ValueTextBlock.Text = ((int)Value).ToString("D2");

            Point pos = e.GetPosition(this);
            if (IsNearEndPoint(pos, _currentEndPoint))
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
            if (!_isDragging) return;

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
                Value = newValue;
            else
            {
                _dragStartAngle = ComputeAngle();
                _dragStartRawAngle = rawAngle;
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ValueTextBlock.Visibility = Visibility.Collapsed;
            DragDot.Visibility = Visibility.Collapsed;

            if (_isDragging)
            {
                SaveToSettings(); // 拖动结束保存
                _isDragging = false;
                ReleaseMouseCapture();
            }
        }

        private bool IsNearEndPoint(Point p, Point endPoint, double threshold = NearEndPointThreshold)
        {
            return (p - endPoint).LengthSquared < threshold * threshold;
        }
    }
}