using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tune.Components
{
    public partial class ExitButton : UserControl
    {
        private const double CollapsedWidth = 10d;
        private const double ExpandedWidth = 55d;

        /// <summary>当前是否处于展开状态，用于过滤重复鼠标事件。</summary>
        private bool _isExpanded;

        /// <summary>
        /// 动画代号。每次播放动画自增一次，旧动画的 Completed 回调
        /// 只有在代号仍相等（即自己仍是最新动画）时才允许生效。
        /// </summary>
        private int _animationToken;

        static ExitButton()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(ExitButton),
                new FrameworkPropertyMetadata(Brushes.White));
        }

        public ExitButton()
        {
            InitializeComponent();
        }

        #region ExitCommand
        public static readonly DependencyProperty ExitCommandProperty =
            DependencyProperty.Register(nameof(ExitCommand), typeof(ICommand), typeof(ExitButton),
                new PropertyMetadata(null));

        public ICommand? ExitCommand
        {
            get => (ICommand?)GetValue(ExitCommandProperty);
            set => SetValue(ExitCommandProperty, value);
        }
        #endregion

        #region CommandParameter
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ExitButton),
                new PropertyMetadata(null));

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }
        #endregion

        #region Click 路由事件
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ExitButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
        #endregion

        private void EscapeBorder_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (_isExpanded) return;
            _isExpanded = true;

            LeaveImage.Visibility = Visibility.Visible;
            AnimateWidth(ExpandedWidth, TimeSpan.FromSeconds(0.5), null);
        }

        private void EscapeBorder_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!_isExpanded) return;
            _isExpanded = false;

            AnimateWidth(CollapsedWidth, TimeSpan.FromSeconds(0.2), () =>
            {
                if (!_isExpanded)
                    LeaveImage.Visibility = Visibility.Hidden;
            });
        }

        private void EscapeBorder_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));

            ICommand? command = ExitCommand;
            if (command is not null && command.CanExecute(CommandParameter))
                command.Execute(CommandParameter);

            e.Handled = true;
        }

        /// <summary>
        /// 播放宽度动画。
        /// <para>1. 用 <see cref="DependencyObject.GetValue"/> 取当前实际宽度作为 From，
        ///       保证打断时从当前位置平滑过渡，不跳变。</para>
        /// <para>2. 用 token 校验保证只有最新一次动画的 Completed 会落地终值，
        ///       旧动画的 Completed 直接忽略，避免误清动画或瞬间跳到旧终值。</para>
        /// </summary>
        private void AnimateWidth(double to, TimeSpan duration, Action? onCompleted)
        {
            int token = ++_animationToken;

            // 取当前显示宽度（含动画中间值），而不是 Width 基础属性
            double from = (double)EscapeBorder.GetValue(WidthProperty);

            // 已在目标位置：直接落值即可，无需动画
            if (Math.Abs(from - to) < 0.5)
            {
                if (token == _animationToken)
                {
                    EscapeBorder.BeginAnimation(WidthProperty, null);
                    EscapeBorder.Width = to;
                    onCompleted?.Invoke();
                }
                return;
            }

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(duration),
                EasingFunction = new BounceEase
                {
                    Bounces = 0,
                    EasingMode = EasingMode.EaseIn,
                    Bounciness = 0
                }
            };

            animation.Completed += (_, _) =>
            {
                // 关键：只有自己仍是最新一次动画时才落地。
                // 若已被新动画取代，直接放弃，避免把新动画顶掉造成瞬间跳变。
                if (token != _animationToken) return;

                EscapeBorder.BeginAnimation(WidthProperty, null);
                EscapeBorder.Width = to;
                onCompleted?.Invoke();
            };

            // 默认 HandoffBehavior.SnapshotAndReplace：
            // 新动画会立刻接管，旧动画被替换，且新动画从 From（当前值）开始，过渡平滑。
            EscapeBorder.BeginAnimation(WidthProperty, animation);
        }
    }
}