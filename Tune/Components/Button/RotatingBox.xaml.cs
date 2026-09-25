using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    /// <summary>
    /// A square icon that rotates 45° on hover, fades out its inner square and
    /// main diagonal, and dims to 75% opacity while pressed.
    /// </summary>
    public partial class RotatingBox : UserControl
    {
        private bool _isPressed;

        public RotatingBox()
        {
            InitializeComponent();
            Loaded += (_, _) => UpdateStates();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateStates();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            // Reset press state when the cursor leaves the control.
            if (!IsMouseCaptured) _isPressed = false;
            UpdateStates();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            _isPressed = true;
            CaptureMouse();
            UpdateStates();
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            _isPressed = false;
            if (IsMouseCaptured) ReleaseMouseCapture();
            UpdateStates();
        }

        /// <summary>
        /// Drive both state groups. They are independent, so hover and press
        /// can be active at the same time.
        /// </summary>
        private void UpdateStates()
        {
            VisualStateManager.GoToElementState(
                LayoutRoot, IsMouseOver ? "Hovered" : "NotHovered", true);

            VisualStateManager.GoToElementState(
                LayoutRoot, _isPressed ? "Pressed" : "NotPressed", true);
        }
    }
}