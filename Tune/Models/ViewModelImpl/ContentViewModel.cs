using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Models.Messages;


namespace Tune.Models.ViewModelImpl
{
    /// <summary>
    /// Base class for view models that are displayed in the main content area of the app.
    /// </summary>
    public abstract partial class ContentViewModel : ObservableObject
    {
        protected ContentViewModel(
            PlayerController playerController,
            MusicPlaybackController playback,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric)
        {
            PlayerController = playerController;
            Playback = playback;
            MusicDisplay = musicDisplay;
            MusicLyric = musicLyric;
        }

        // ==== State bound by the shared player control bar ====

        public PlayerController PlayerController { get; }
        public MusicDisplayPresenter MusicDisplay { get; }
        public MusicLyricPresenter MusicLyric { get; }
        protected MusicPlaybackController Playback { get; }

        // ==== Commands for UI Binding ====

        [RelayCommand] private void PlayNextMusic() => Playback.PlayNextMusic();
        [RelayCommand] private void PlayPreviousMusic() => Playback.PlayPreviousMusic();
        [RelayCommand] private void Seek(double seconds) => PlayerController.Seek(seconds);
        [RelayCommand] private void TimelineStartDragging() => PlayerController.BeginDrag();

        [RelayCommand]
        private void NavigateToMain() =>
            WeakReferenceMessenger.Default.Send(new NavigationMessage(PageKind.Main));
    }
}