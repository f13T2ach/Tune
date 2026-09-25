using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using Tune.ViewModel;

namespace Tune.Appearance
{
    public partial class PlaylistsPage : UserControl
    {
        public PlaylistsPage()
        {
            InitializeComponent();

            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<PlaylistsPageViewModel>();
            }
        }
    }
}