using System.Windows.Controls;

namespace Tune.Models.NewDialog
{
    public abstract class DialogContentBase : UserControl
    {
        /// true => Confirm，false => cancelled
        public event EventHandler<bool>? CloseRequested;

        protected void Confirm() => CloseRequested?.Invoke(this, true);
        protected void Cancel() => CloseRequested?.Invoke(this, false);

        protected virtual void OnShown() { }
        internal void RaiseShown() => OnShown();
    }
}
