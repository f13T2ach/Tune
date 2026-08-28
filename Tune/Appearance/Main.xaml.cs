using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using Tune.Local;
using Tune.ViewModel;

namespace Tune.Appearance
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : UserControl
    {
        public Main()
        {
            InitializeComponent();

            if (App.Services != null)
            {
                DataContext =
                                App.Services.GetRequiredService<MainViewModel>();
            }

            Loaded += Main_Loaded;
        }


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            // Init data on startup
            VolumeKnob.AnimateValueTo(TuneSettings.Default.PlayerVolume);
        }
    }
}
