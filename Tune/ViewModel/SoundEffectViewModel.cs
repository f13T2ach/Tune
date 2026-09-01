using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Models.Messages;

namespace Tune.ViewModel
{
    public partial class SoundEffectViewModel
    {
        // ==== Shared ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicDisplayPresenter _musicDisplay;
        private readonly MusicLyricPresenter _musicLyric;

        // ==== Public Properties ====
        public PlayerController PlayerController => _playerController;
        public ListController ListController => _listController;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;

        // ==== Player Commands ====
        [RelayCommand]
        private void Seek(double seconds) => _playerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => _playerController.BeginDrag();



        public SoundEffectViewModel(
            PlayerController playerController,
            ListController listController,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric)
        {
            _playerController = playerController;
            _listController = listController;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;

            //_playerController.PropertyChanged += OnPlayerControllerPropertyChanged;
        }

        [RelayCommand] public void NavigateToMain() => WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(Main)));

        public void Dispose()
        {
            //_playerController.PropertyChanged -= OnPlayerControllerPropertyChanged;
        }
    }
}
