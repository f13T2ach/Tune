using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    /// <summary>
    /// 具有命令绑定功能的 Image 控件，左键点击触发 Command 与 Click 事件。
    /// </summary>
    public class ImageButton : Image, ICommandSource
    {
        // ============ 依赖属性 ============
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(ImageButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(ImageButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(ImageButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ClickEffectProperty =
            DependencyProperty.Register(nameof(ClickEffect), typeof(bool), typeof(ImageButton),
                new PropertyMetadata(false, OnClickEffectChanged));

        // ============ Click 路由事件 ============
        /// <summary>点击事件（冒泡）。可在 XAML 中通过 Click="Handler" 使用。</summary>
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(
                nameof(Click),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(ImageButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        // ============ 公共属性 ============
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public IInputElement CommandTarget
        {
            get => (IInputElement)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        /// <summary>
        /// 是否启用鼠标悬停/按下透明度效果（默认为 false）。
        /// </summary>
        public bool ClickEffect
        {
            get => (bool)GetValue(ClickEffectProperty);
            set => SetValue(ClickEffectProperty, value);
        }

        // ============ 内部状态 ============
        private bool _isHovering;
        private bool _isPressed;
        private double? _baseOpacity;

        private static void OnClickEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageButton button)
                button.UpdateClickEffect();
        }

        private void UpdateClickEffect()
        {
            bool active = ClickEffect && (_isHovering || _isPressed);

            if (active)
            {
                _baseOpacity ??= Opacity;
                Opacity = _baseOpacity.Value * 0.75;
            }
            else if (_baseOpacity.HasValue)
            {
                Opacity = _baseOpacity.Value;
                _baseOpacity = null;
            }
        }

        // ============ 鼠标事件 ============
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovering = true;
            UpdateClickEffect();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovering = false;
            _isPressed = false;
            UpdateClickEffect();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _isPressed = true;
            UpdateClickEffect();

            CaptureMouse();
            base.OnMouseLeftButtonDown(e);

            // 1) 命令执行（若可执行）
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
                e.Handled = true;
            }

            // 2) 触发 Click 路由事件（独立于命令，且不依赖 e.Handled）
            OnClick();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _isPressed = false;
            UpdateClickEffect();

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            base.OnMouseLeftButtonUp(e);
        }

        /// <summary>触发 Click 路由事件，子类可重写以扩展。</summary>
        protected virtual void OnClick()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        }
    }
}