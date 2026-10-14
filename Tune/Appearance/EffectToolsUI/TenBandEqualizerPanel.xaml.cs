using System.Windows.Controls;

namespace Tune.Appearance.EffectToolsUI
{
    /// <summary>
    /// The ten band equalizer itself: the preset area on the left and the chart with the
    /// ten faders on the right. Like the chain and the parameter panel next to it, it
    /// takes its data context from the page that hosts it.
    /// </summary>
    public partial class TenBandEqualizerPanel : UserControl
    {
        public TenBandEqualizerPanel()
        {
            InitializeComponent();
        }
    }
}
