using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tune.Models.Playlist;

namespace Tune.Components.Content
{
    /// <summary>
    /// 歌单列表项。数据通过 Playlist 依赖属性绑定，
    /// 播放动作通过 PlayCommand 依赖属性对外暴露。
    /// </summary>
    public partial class PlaylistItem : UserControl
    {
        public PlaylistItem()
        {
            InitializeComponent();
        }

        // ============ Playlist 依赖属性 ============
        public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register(
                nameof(Playlist),
                typeof(PlaylistBase),
                typeof(PlaylistItem),
                new PropertyMetadata(null, OnPlaylistChanged));

        public PlaylistBase? Playlist
        {
            get => (PlaylistBase?)GetValue(PlaylistProperty);
            set => SetValue(PlaylistProperty, value);
        }

        private static void OnPlaylistChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PlaylistItem item)
                item.OnPlaylistChanged(e.OldValue as PlaylistBase, e.NewValue as PlaylistBase);
        }

        private void OnPlaylistChanged(PlaylistBase? oldValue, PlaylistBase? newValue)
        {
            // 若需要监听 MusicList 变化，可在此挂钩
        }

        // ============ PlayCommand 依赖属性 ============
        public static readonly DependencyProperty PlayCommandProperty =
            DependencyProperty.Register(
                nameof(PlayCommand),
                typeof(ICommand),
                typeof(PlaylistItem),
                new PropertyMetadata(null));

        public ICommand? PlayCommand
        {
            get => (ICommand?)GetValue(PlayCommandProperty);
            set => SetValue(PlayCommandProperty, value);
        }

        // ============ 备选：仅当 ImageButton 不支持 Command 时使用 ============
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (Playlist is null) return;
            if (PlayCommand is null) return;

            if (PlayCommand.CanExecute(Playlist))
                PlayCommand.Execute(Playlist);
        }
    }
}