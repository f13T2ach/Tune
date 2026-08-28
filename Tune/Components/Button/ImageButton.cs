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

        // 处理左键按下事件
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
                e.Handled = true; // 防止事件继续冒泡
            }
        }
    }
}