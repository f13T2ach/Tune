using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    /// <summary>
    /// 具有命令绑定功能的 Image 控件，左键点击触发 Command。
    /// </summary>
    public class ImageButton : Image, ICommandSource
    {
        // 依赖属性定义
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

        // 公共属性
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

        // 内部状态
        private bool _isHovering;
        private bool _isPressed;
        private double? _baseOpacity;

        private static void OnClickEffectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageButton button)
                button.UpdateClickEffect();
        }

        /// <summary>
        /// 根据悬停 / 按下状态应用或恢复透明度。
        /// </summary>
        private void UpdateClickEffect()
        {
            bool active = ClickEffect && (_isHovering || _isPressed);

            if (active)
            {
                // 仅在首次应用时记录原始透明度，避免递归生效
                _baseOpacity ??= Opacity;
                Opacity = _baseOpacity.Value * 0.75;
            }
            else if (_baseOpacity.HasValue)
            {
                Opacity = _baseOpacity.Value;
                _baseOpacity = null;
            }
        }

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
            // 鼠标离开控件时，无论是否仍按住，都视为释放效果
            _isPressed = false;
            UpdateClickEffect();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _isPressed = true;
            UpdateClickEffect();

            // 捕获鼠标，确保在控件外释放也能收到 MouseUp
            CaptureMouse();

            base.OnMouseLeftButtonDown(e);

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
                e.Handled = true; // 防止事件继续冒泡
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            _isPressed = false;
            UpdateClickEffect();

            if (IsMouseCaptured)
                ReleaseMouseCapture();

            base.OnMouseLeftButtonUp(e);
        }
    }
}