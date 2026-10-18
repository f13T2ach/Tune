using System.Windows.Controls;

namespace Tune.Appearance.ExtendView
{
    /// <summary>
    /// Shows the jog wheel. The angle comes from the presenter, so the wheel keeps turning while this
    /// panel is off screen and simply shows the right angle when it comes back.
    /// </summary>
    public partial class JogWheelPanel : UserControl
    {
        public JogWheelPanel()
        {
            InitializeComponent();
        }
    }
}