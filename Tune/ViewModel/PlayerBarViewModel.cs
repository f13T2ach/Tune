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
        public MusicPlaybackController PlaybackController { get; }

        public PlayerBarViewModel(
            PlayerController playerController,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric,
            MusicPlaybackController playback)
        {
            PlayerController = playerController;
            MusicDisplay = musicDisplay;
            MusicLyric = musicLyric;
            PlaybackController = playback;
        }

        [RelayCommand]
        private void Seek(double seconds) => PlayerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => PlayerController.BeginDrag();

        [RelayCommand] private void PlayNextMusic() => PlaybackController.PlayNextMusic();
        [RelayCommand] private void PlayPreviousMusic() => PlaybackController.PlayPreviousMusic();
    }
}