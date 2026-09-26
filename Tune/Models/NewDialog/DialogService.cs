namespace Tune.Models.NewDialog
{
    public static class DialogService
    {
        public static bool ShowDialog(this DialogContentBase content)
        {
            var host = new DialogHost(content, content.Width, content.Height);

            bool? pendingResult = null;

            // CloseRequest: Shown => Close;
            //               Not Shown => Schedule;
            content.CloseRequested += (_, ok) =>
            {
                if (host.IsLoaded)
                {
                    host.DialogResult = ok;
                    host.Close();
                }
                else
                {
                    pendingResult = ok;
                }
            };

            // Init workflow after shown
            host.Loaded += (_, __) =>
            {
                content.RaiseShown();
                if (pendingResult is { } r)
                {
                    host.DialogResult = r;
                    host.Close();
                }
            };

            host.ShowDialog();
            return host.DialogResult == true;
        }
    }
}
