using System;

using CommunityToolkit.Mvvm.ComponentModel;

using Tune.Client.Controllers;
using Tune.Client.Tasks;

namespace Tune.Client.Presenters
{
    /// <summary>
    /// Keeps track of how far the jog wheel has turned. It runs for as long as the application does,
    /// so the angle keeps advancing while the panel is not on screen; only the drawing pauses when
    /// nobody is looking at it.
    /// </summary>
    public partial class WheelPresenter : ObservableObject, IFrameTask
    {
        /// <summary>One turn per minute, about 6 degrees a second.</summary>
        private const double DegreesPerSecond = 6;

        private readonly PlayerController _player;
        private readonly ListController _list;

        private object? _playlist;
        private double _total;

        public WheelPresenter(TaskRegister register, PlayerController player, ListController list)
        {
            _player = player;
            _list = list;

            register.Attach(this);
        }

        /// <summary>Gets the angle to draw, from 0 to 360.</summary>
        [ObservableProperty]
        private double _angle;

        /// <summary>Gets how many full turns the wheel has made since the playlist was chosen.</summary>
        [ObservableProperty]
        private int _turns;

        /// <inheritdoc />
        public void Tick(TimeSpan elapsed)
        {
            // A different playlist means a fresh record, so the wheel goes back to the top.
            object? playlist = _list.CurrentPlaylist;

            if (!ReferenceEquals(playlist, _playlist))
            {
                _playlist = playlist;
                _total = 0;
                Turns = 0;
                Angle = 0;

                return;
            }

            if (!_player.IsPlaying)
                return;

            _total += DegreesPerSecond * elapsed.TotalSeconds;

            // Keep only what is on screen; the full turns are counted separately.
            Turns = (int)(_total / 360);
            Angle = _total % 360;
        }
    }
}