using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    /// <summary>
    /// 可点击的文本块，支持悬停/按下时的透明度反馈，并公开 Click 事件。
    /// </summary>
    public class ClickableTextBlock : TextBlock
    {
        private double? _originalOpacity;
        private bool _isMouseOver;
        private bool _isPressed;

        /// <summary>
        /// 单击事件（路由事件，冒泡方式）
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClickableTextBlock));

        public ClickableTextBlock()
        {
            this.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 单击事件处理程序
        /// </summary>
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _isPressed = true;
            UpdateOpacity();
            CaptureMouse();   // 确保在控件外松开也能感知
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (_isPressed)
            {
                _isPressed = false;
                UpdateOpacity();
                ReleaseMouseCapture();

                // 仅当鼠标仍在控件上时触发 Click（标准按钮行为）
                if (IsMouseOver)
                {
                    RaiseEvent(new RoutedEventArgs(ClickEvent));
                }
            }
            e.Handled = true;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _isMouseOver = true;
            UpdateOpacity();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isMouseOver = false;
            UpdateOpacity();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            // 如果失去鼠标捕获（例如点击后拖出控件），取消按下状态
            if (_isPressed)
            {
                _isPressed = false;
                UpdateOpacity();
            }
        }

        /// <summary>
        /// 更新透明度：当鼠标悬停或按下时，变为原始值的 0.75 倍，否则恢复原始值。
        /// </summary>
        private void UpdateOpacity()
        {
            // 首次进入状态时记录当前透明度（确保外部初始值被保留）
            if (!_originalOpacity.HasValue)
            {
                _originalOpacity = Opacity;
            }

            bool shouldDim = _isMouseOver || _isPressed;
            Opacity = shouldDim ? _originalOpacity.Value * 0.75 : _originalOpacity.Value;
        }
    }
}