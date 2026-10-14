using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using Tune.ViewModel;

namespace Tune.Appearance.SoundEffectTools
{
    /// <summary>
    /// The ten band equalizer page. Its view model comes from the container, so the
    /// page always edits the equalizer the audio chain is playing through.
    /// </summary>
    public partial class EqualizerPage : UserControl
    {
        public EqualizerPage()
        {
            InitializeComponent();

            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<EqualizerPageViewModel>();
            }
        }
    }
}
