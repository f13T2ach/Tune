using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tune.Client.Controllers;
using Tune.Client.Presenters;
using Tune.Models.Music;

namespace Tune.ViewModel
{
    /// <summary>
    /// 当前播放控制视图模型（对应 CurrentPlaylist 用户控件）
    /// 与 MainViewModel 共享 PlayerController、ListController 及 Presenter 单例
    /// </summary>
    public partial class CurrentPlaylistViewModel : ObservableObject
    {
        // ==== 共享控制器与呈现器（通过 DI 注入单例） ====
        private readonly PlayerController _playerController;
        private readonly ListController _listController;
        private readonly MusicDisplayPresenter _musicDisplay;
        private readonly MusicLyricPresenter _musicLyric;

        // ==== 公开属性供 XAML 绑定 ====
        public PlayerController PlayerController => _playerController;
        public ListController ListController => _listController;
        public MusicDisplayPresenter MusicDisplay => _musicDisplay;
        public MusicLyricPresenter MusicLyric => _musicLyric;

        // ==== 命令（委托给 PlayerController） ====
        [RelayCommand]
        private void Seek(double seconds) => _playerController.Seek(seconds);

        [RelayCommand]
        private void TimelineStartDragging() => _playerController.BeginDrag();

        // 注：SwitchMusic 组件内部可能使用自身的命令，此处无需额外暴露

        public CurrentPlaylistViewModel(
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

        

        public void Dispose()
        {
            //_playerController.PropertyChanged -= OnPlayerControllerPropertyChanged;
        }
    }
}