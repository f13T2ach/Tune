namespace Tune.Models.Dialog
{
    public interface IDialogContent<TArg, TResult> : IInitializableDialog
    {
        /// <summary>对话框参数</summary>
        TArg Argument { get; set; }

        /// <summary>对话框结果（由用户操作后设置）</summary>
        TResult Result { get; set; }

        /// <summary>对话框宽</summary>
        double Width { get; set; }

        /// <summary>对话框高</summary>
        double Height { get; set; }

        /// <summary>当用户确认时触发，用于关闭窗口并返回结果</summary>
        event EventHandler<EventArgs> ConfirmRequested;

        /// <summary>当用户取消时触发，用于关闭窗口并返回默认结果</summary>
        event EventHandler<EventArgs> CancelRequested;
    }
}
