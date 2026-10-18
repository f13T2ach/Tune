using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Engine.Effects;
using Tune.Models.Messages;

namespace Tune.ViewModel
{
    public partial class SoundEffectViewModel : ObservableObject
    {
        // ==== Shared ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicDisplayPresenter _musicDisplay;
        private readonly MusicLyricPresenter _musicLyric;
        private readonly IAudioEffectChain _effectChain;

        // ==== Public Properties ====
        public PlayerController PlayerController => _playerController;
        public ListController ListController => _listController;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;

        /// <summary>
        /// The sound effect processor shared with the main page.
        /// </summary>
        public IAudioEffectChain Effect => _effectChain;

        /// <summary>
        /// Gets the label of the effect switch.
        /// </summary>
        public string EffectToggleText => _effectChain.IsEnabled ? "ON" : "OFF";

        // ==== Player Commands ====
        [RelayCommand]
        private void Seek(double seconds) => _playerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => _playerController.BeginDrag();

        /// <summary>
        /// Turns the effect stages on or off. The level stage stays active either way.
        /// </summary>
        [RelayCommand]
        private void ToggleEffect() => _effectChain.IsEnabled = !_effectChain.IsEnabled;

        public SoundEffectViewModel(
            PlayerController playerController,
            ListController listController,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric,
            IAudioEffectChain effectChain)
        {
            _playerController = playerController;
            _listController = listController;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;
            _effectChain = effectChain;

            _effectChain.PropertyChanged += OnEffectChainPropertyChanged;

            //_playerController.PropertyChanged += OnPlayerControllerPropertyChanged;
        }

        [RelayCommand] public void NavigateToMain() => WeakReferenceMessenger.Default.Send(new NavigationMessage(PageKind.Main));

        private void OnEffectChainPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IAudioEffectChain.IsEnabled))
                OnPropertyChanged(nameof(EffectToggleText));
        }

        public void Dispose()
        {
            _effectChain.PropertyChanged -= OnEffectChainPropertyChanged;
            //_playerController.PropertyChanged -= OnPlayerControllerPropertyChanged;
        }
    }
}
