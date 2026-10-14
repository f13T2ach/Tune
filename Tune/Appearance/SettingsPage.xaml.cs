using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using Tune.ViewModel;

namespace Tune.Appearance
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage()
        {
            InitializeComponent();

            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<SettingsPageViewModel>();
            }
        }
    }
}
