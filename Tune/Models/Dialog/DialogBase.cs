using System.Windows.Controls;

namespace Tune.Models.Dialog
{
    /// <summary>
    /// The abstract layer of Dialog, which provides the universal properties of the IDialogContent
    /// A simple Dialog:
    ///     1. Confirm
    ///     2. Cancel
    /// Properties:
    ///     1. Width & Height
    ///     2. Arguments for Init (For Multi Arguments, use `MultiArguments<...>`
    /// The `SelectPlaylist.xaml.cs` is a stantard example.
    /// </summary>
    public abstract class DialogBase<TArg, TResult> : UserControl, IDialogContent<TArg?, TResult?>
    {
        // ==== Realization ====

        public event EventHandler<EventArgs>? ConfirmRequested;
        public event EventHandler<EventArgs>? CancelRequested;

        double IDialogContent<TArg?, TResult?>.Width
        {
            get => this.Width;
            set => this.Width = value;
        }

        double IDialogContent<TArg?, TResult?>.Height
        {
            get => this.Height;
            set => this.Height = value;
        }

        public TArg? Argument { get; set; }             // As this is an Argument, we just need to read it

        public virtual TResult? Result { get; set; }


        // ==== Abstract Layer Event Process ====

        protected virtual void RaiseConfirm()
        {
            ConfirmRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void RaiseCancel()
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}