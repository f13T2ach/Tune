using System.Windows;
using System.Windows.Controls;

namespace Tune.Appearance.ExtendView
{
    /// <summary>Shows the custom slider with its value and its editing state.</summary>
    public partial class SliderSample : UserControl
    {
        public SliderSample()
        {
            InitializeComponent();

            UpdateHint(false);
        }

        /// <summary>Follows the slider through its event, which is the routed way to do it.</summary>
        private void OnEditingChanged(object sender, RoutedEventArgs e) =>
            UpdateHint(DemoSlider.IsEditing);

        /// <summary>Also reports focus, because the slider takes it when it is picked up.</summary>
        private void OnSliderFocus(object sender, RoutedEventArgs e) =>
            PART_EditingHint.Text = DemoSlider.IsEditing ? "editing (focused)" : "focused";

        /// <summary>Puts the hint back when the slider loses focus.</summary>
        private void OnSliderLostFocus(object sender, RoutedEventArgs e) => UpdateHint(DemoSlider.IsEditing);

        private void UpdateHint(bool editing) =>
            PART_EditingHint.Text = editing ? "editing" : $"idle, value {DemoSlider.Value:0}";
    }
}
