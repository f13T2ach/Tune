using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tune.Client.Controllers;
using Tune.Client.Presenters;

namespace Tune.ViewModel
{
    /// <summary>
    /// Aggregated view model for the reusable player control bar.
    /// Exposes PlayerController, MusicDisplay and MusicLyric with the same
    /// binding paths used before, so the component stays a drop-in replacement.
    /// </summary>
    public partial class PlayerBarViewModel : ObservableObject
    {
        public PlayerController PlayerController { get; }
        public MusicDisplayPresenter MusicDisplay { get; }
        public MusicLyricPresenter MusicLyric { get; }

        public PlayerBarViewModel(
            PlayerController playerController,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric)
        {
            PlayerController = playerController;
            MusicDisplay = musicDisplay;
            MusicLyric = musicLyric;
        }

        [RelayCommand]
        private void Seek(double seconds) => PlayerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => PlayerController.BeginDrag();
    }
}