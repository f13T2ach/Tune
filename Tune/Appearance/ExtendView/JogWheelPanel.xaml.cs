using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Tune.ViewModel;

namespace Tune.Appearance.ExtendView
{
    /// <summary>
    /// Turns the inner wheel while music is playing. The angle is kept across tracks, and only a
    /// change of playlist puts it back to the top; the turns themselves are counted as it goes.
    /// </summary>
    public partial class JogWheelPanel : UserControl
    {
        /// <summary>A third of a turn per second, which is 33 1/3 rpm, the speed of a record.</summary>
        private const double DegreesPerSecond = 200;

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };

        private object? _playlist;
        private double _angle;
        private DateTime _last = DateTime.UtcNow;

        public JogWheelPanel()
        {
            InitializeComponent();

            _timer.Tick += OnTick;
            Loaded += (_, _) => _timer.Start();
            Unloaded += (_, _) => _timer.Stop();
        }

        /// <summary>Gets how many full turns the wheel has made since the playlist was chosen.</summary>
        public int Turns { get; private set; }

        private void OnTick(object? sender, EventArgs e)
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