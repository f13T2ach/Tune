using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Tune.ViewModel;

namespace Tune.Appearance.ExtendView
{
    /// <summary>
    /// Turns the inner wheel while music is playing. The angle is kept across tracks, and only a
    /// change of playlist puts it back to the top; the turns themselves are counted as it goes.
    /// </summary>
    /// <remarks>
    /// The updates are driven by <see cref="CompositionTarget.Rendering"/> rather than a timer: it
    /// runs on the UI thread once per frame, so the wheel moves with the display instead of racing
    /// a separate timer that can fire in bursts and make the motion stutter.
    /// </remarks>
    public partial class JogWheelPanel : UserControl
    {
        /// <summary>One turn per minute, about 6 degrees a second.</summary>
        private const double DegreesPerSecond = 6;

        private object? _playlist;
        private double _angle;
        private DateTime _last = DateTime.UtcNow;
        private bool _rendering;

        public JogWheelPanel()
        {
            InitializeComponent();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>Gets how many full turns the wheel has made since the playlist was chosen.</summary>
        public int Turns { get; private set; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_rendering)
                return;

            _last = DateTime.UtcNow;
            CompositionTarget.Rendering += OnRendering;
            _rendering = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (!_rendering)
                return;

            CompositionTarget.Rendering -= OnRendering;
            _rendering = false;
        }

        private void OnRendering(object? sender, EventArgs e)
        {
            DateTime now = DateTime.UtcNow;
            double seconds = (now - _last).TotalSeconds;
            _last = now;

            if (DataContext is not MainViewModel viewModel)
                return;

            // A different playlist means a fresh record, so the wheel goes back to the top.
            object? playlist = viewModel.ListController.CurrentPlaylist;

            if (!ReferenceEquals(playlist, _playlist))
            {
                _playlist = playlist;
                _angle = 0;
                Turns = 0;
                WheelRotation.Angle = 0;

                return;
            }

            if (!viewModel.PlayerControl.IsPlaying)
                return;

            _angle += DegreesPerSecond * seconds;

            // Keep only what is on screen; the full turns are counted separately.
            Turns = (int)(_angle / 360);
            WheelRotation.Angle = _angle % 360;
        }
    }
}