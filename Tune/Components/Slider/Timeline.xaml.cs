using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    public partial class Timeline : UserControl
    {
        // ========== 依赖属性 ==========

        // 依赖属性
        /*
        public static readonly DependencyProperty DragCompletedCommandProperty =
            DependencyProperty.Register(nameof(DragCompletedCommand), typeof(ICommand), typeof(Timeline), new PropertyMetadata(null));

        public ICommand DragCompletedCommand
        {
            get => (ICommand)GetValue(DragCompletedCommandProperty);
            set => SetValue(DragCompletedCommandProperty, value);
        }*/


        public static readonly DependencyProperty CurrentSecondsProperty =
            DependencyProperty.Register(
                nameof(CurrentSeconds),
                typeof(double),
                typeof(Timeline),
                new PropertyMetadata(0.0, OnCurrentSecondsChanged));

        public double CurrentSeconds
        {
            get => (double)GetValue(CurrentSecondsProperty);
            set => SetValue(CurrentSecondsProperty, value);
        }

        public static readonly DependencyProperty TotalSecondsProperty =
            DependencyProperty.Register(
                nameof(TotalSeconds),
                typeof(double),
                typeof(Timeline),
                new PropertyMetadata(1.0, OnTotalSecondsChanged)); // 默认1避免除零

        public double TotalSeconds
        {
            get => (double)GetValue(TotalSecondsProperty);
            set => SetValue(TotalSecondsProperty, value);
        }

        // ========== 路由事件（可选） ==========
        public static readonly RoutedEvent CurrentSecondsChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(CurrentSecondsChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<double>),
                typeof(Timeline));

        public event RoutedPropertyChangedEventHandler<double> CurrentSecondsChanged
        {
            add => AddHandler(CurrentSecondsChangedEvent, value);
            remove => RemoveHandler(CurrentSecondsChangedEvent, value);
        }

        public static readonly RoutedEvent DragStartedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(DragStarted),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(Timeline));

        public event RoutedEventHandler DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        public static readonly RoutedEvent DragCompletedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(DragCompleted),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(Timeline));

        public event RoutedEventHandler DragCompleted
        {
            add => AddHandler(DragCompletedEvent, value);
            remove => RemoveHandler(DragCompletedEvent, value);
        }

        // ========== 私有状态 ==========
        private bool _isDragging;
        private double _startCurrentSeconds;   // 拖动开始时的当前秒数
        private Point _startMousePos;          // 拖动开始时鼠标相对于控件的位置

        public Timeline()
        {
            InitializeComponent();
            this.SizeChanged += (s, e) => UpdateRedLinePosition();
        }

        // ========== 属性变更回调 ==========
        private static void OnCurrentSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Timeline)d;
            double oldValue = (double)e.OldValue;
            double newValue = (double)e.NewValue;
            control.UpdateRedLinePosition();

            // 触发路由事件（可选）
            var args = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue)
            {
                RoutedEvent = CurrentSecondsChangedEvent
            };
            control.RaiseEvent(args);
        }

        private static void OnTotalSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Timeline)d;
            control.UpdateRedLinePosition();
        }

        // ========== 更新红色竖线位置 ==========
        private void UpdateRedLinePosition()
        {
            if (RedLine == null || ActualWidth <= 3 || TotalSeconds <= 0)
            {
                RedLine.Margin = new Thickness(0, 0, 0, 0);
                return;
            }

            double ratio = Math.Max(0, Math.Min(1, CurrentSeconds / TotalSeconds));
            double maxLeft = ActualWidth - 3;
            double left = ratio * maxLeft;
            RedLine.Margin = new Thickness(left, 0, 0, 0);
        }

        // ========== 鼠标事件处理 ==========
        private void OnDragAreaMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _isDragging = true;
            _startMousePos = e.GetPosition(this);
            _startCurrentSeconds = CurrentSeconds;

            // 根据点击位置直接跳转
            double ratio = CalculateRatioFromPoint(_startMousePos);
            double newSeconds = ratio * TotalSeconds;
            CurrentSeconds = newSeconds;
            _startCurrentSeconds = newSeconds; // 更新起始值，使后续拖动连续

            PART_DragArea.CaptureMouse();
            RaiseEvent(new RoutedEventArgs(DragStartedEvent, this));
            e.Handled = true;
        }

        private void OnDragAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || e.LeftButton != MouseButtonState.Pressed) return;

            Point currentPos = e.GetPosition(this);
            double deltaX = currentPos.X - _startMousePos.X;
            if (ActualWidth > 3 && TotalSeconds > 0)
            {
                double deltaRatio = deltaX / (ActualWidth - 3);
                double newSeconds = _startCurrentSeconds + deltaRatio * TotalSeconds;
                newSeconds = Math.Max(0, Math.Min(TotalSeconds, newSeconds));
                CurrentSeconds = newSeconds; // 实时更新绑定值
            }
            e.Handled = true;
        }

        private void OnDragAreaMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                PART_DragArea.ReleaseMouseCapture();
                RaiseEvent(new RoutedEventArgs(DragCompletedEvent, this));
                e.Handled = true;
            }
        }

        private void OnDragAreaMouseLeave(object sender, MouseEventArgs e)
        {
            // 可选择：鼠标离开控件时是否结束拖动（这里保留，若需启用可取消注释）
            // if (_isDragging)
            // {
            //     _isDragging = false;
            //     PART_DragArea.ReleaseMouseCapture();
            //     RaiseEvent(new RoutedEventArgs(DragCompletedEvent, this));
            // }
        }

        // 辅助：计算鼠标位置对应的比例（0~1）
        private double CalculateRatioFromPoint(Point point)
        {
            if (ActualWidth <= 3) return 0;
            double x = Math.Max(0, Math.Min(ActualWidth - 3, point.X));
            return x / (ActualWidth - 3);
        }
    }
}