using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Tune.ViewModel;

namespace Tune.Appearance
{
    public partial class CurrentPlaylist : UserControl
    {
        public CurrentPlaylist()
        {
            InitializeComponent();

            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<CurrentPlaylistViewModel>();
            }
        }
    }
}