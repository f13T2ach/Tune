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
        public DialogInstance(IDialogContent<TArg, TResult>? content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            Content = (UIElement)content;

            // 先订阅
            content.ConfirmRequested += (s, e) => CloseWithResult(true);
            content.CancelRequested += (s, e) => CloseWithResult(false);
            Closed += (s, e) => DialogClosed?.Invoke(this, EventArgs.Empty);

            Height = content.Height;
            Width = content.Width;

            // 再初始化（此时 RaiseCancel 已经能打到订阅者）
            (content as IInitializableDialog)?.Initialize();
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

        public void ForceClose()
        {
            this.Close();
        }

        private bool? _pendingResult;

        private void CloseWithResult(bool success)
        {
            if (IsLoaded || IsVisible)
            {
                try
                {
                    DialogResult = success;
                    Close();
                }
                catch
                {
                    _pendingResult = success;
                    Loaded += OnFirstLoaded;
                }
            }
            else
            {
                // 窗口还没显示：先记住，等 Loaded 再关
                _pendingResult = success;
                Loaded += OnFirstLoaded;
            }
        }

        private void OnFirstLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnFirstLoaded;
            if (_pendingResult.HasValue)
            {
                DialogResult = _pendingResult.Value;
                Close();
            }
        }
    }
}
