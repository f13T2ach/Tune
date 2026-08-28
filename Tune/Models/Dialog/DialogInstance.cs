using System.Windows;
using System.Windows.Media;


namespace Tune.Models.Dialog
{

    /// <summary>
    /// 泛型对话框窗口，用于实现控件实例
    /// </summary>
    public class DialogInstance<TArg, TResult> : Window
    {
        private readonly IDialogContent<TArg, TResult> _content;

        /// <summary>参数</summary>
        public TArg Argument
        {
            get => _content.Argument;
            set => _content.Argument = value;
        }

        /// <summary>结果</summary>
        public TResult Result
        {
            get => _content.Result;
            set => _content.Result = value;
        }

        public new double Width
        {
            get => _content.Width;
            set => _content.Width = value;
        }

        public new double Height
        {
            get => _content.Height;
            set => _content.Height = value;
        }


        /// <summary>自定义关闭事件（在窗口关闭后触发）</summary>
        public event EventHandler<EventArgs>? DialogClosed;

        /// <summary>
        /// 构造窗口并注入内容控件
        /// </summary>
        public DialogInstance(IDialogContent<TArg, TResult> content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));

            // 设置窗口外观（可覆写）
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = Brushes.Transparent;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 将内容控件设为窗口内容
            this.Content = (UIElement)content;

            // 订阅内容控件的确认/取消事件，自动关闭窗口并传递结果
            content.ConfirmRequested += (s, e) => CloseWithResult(true);
            content.CancelRequested += (s, e) => CloseWithResult(false);

            this.Height = content.Height;
            this.Width = content.Width;

            // 窗口关闭后触发自定义事件
            this.Closed += (s, e) => DialogClosed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 显示对话框，返回用户操作结果
        /// </summary>
        public TResult ShowDialog(TArg argument)
        {
            this.Argument = argument;
            bool? dialogResult = base.ShowDialog();
            return this.Result; // 无论是否成功，返回内容控件设置的结果
        }

        private void CloseWithResult(bool success)
        {
            this.DialogResult = success;
            this.Close();
        }
    }
}
