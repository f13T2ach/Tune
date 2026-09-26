using System.Windows;
using System.Windows.Media;

namespace Tune.Models.NewDialog
{
    public class DialogHost : Window
    {
        public DialogHost(UIElement content, double width, double height)
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Content = content;
            Width = width;
            Height = height;
        }
    }
}
