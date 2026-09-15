using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tune.Models.Music;
using Tune.Values.Static;

namespace Tune.Components.Content
{
    public partial class MusicItem : UserControl
    {
        // ========= DependencyProperty ==========
        public static readonly DependencyProperty MusicProperty =
            DependencyProperty.Register(nameof(Music), typeof(Music), typeof(MusicItem),
                new PropertyMetadata(null, OnMusicChanged));

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(MusicItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.Register(nameof(ClickCommandParameter), typeof(object), typeof(MusicItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty PlayNextCommandProperty =
            DependencyProperty.Register(nameof(PlayNextCommand), typeof(ICommand), typeof(MusicItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty AddToPlaylistCommandProperty =
            DependencyProperty.Register(nameof(AddToPlaylistCommand), typeof(ICommand), typeof(MusicItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty MoreCommandProperty =
            DependencyProperty.Register(nameof(MoreCommand), typeof(ICommand), typeof(MusicItem),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register(nameof(DeleteCommand), typeof(ICommand), typeof(MusicItem),
                new PropertyMetadata(null));

        // ========= PublicProperty ==========
        public Music? Music
        {
            get => (Music?)GetValue(MusicProperty);
            set => SetValue(MusicProperty, value);
        }

        public ICommand? ClickCommand
        {
            get => (ICommand?)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        public object? ClickCommandParameter
        {
            get => GetValue(ClickCommandParameterProperty);
            set => SetValue(ClickCommandParameterProperty, value);
        }

        public ICommand? PlayNextCommand
        {
            get => (ICommand?)GetValue(PlayNextCommandProperty);
            set => SetValue(PlayNextCommandProperty, value);
        }

        public ICommand? AddToPlaylistCommand
        {
            get => (ICommand?)GetValue(AddToPlaylistCommandProperty);
            set => SetValue(AddToPlaylistCommandProperty, value);
        }

        public ICommand? MoreCommand
        {
            get => (ICommand?)GetValue(MoreCommandProperty);
            set => SetValue(MoreCommandProperty, value);
        }

        public ICommand? DeleteCommand
        {
            get => (ICommand?)GetValue(DeleteCommandProperty);
            set => SetValue(DeleteCommandProperty, value);
        }

        // ========= Background ==========
        private static readonly SolidColorBrush HoverBrush =
            CreateFrozenBrush(Color.FromArgb(64, 255, 255, 255));   // 25% 白

        private static readonly SolidColorBrush PressedBrush =
            CreateFrozenBrush(Color.FromArgb(102, 255, 255, 255));  // 40% 白

        private static SolidColorBrush CreateFrozenBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        // ========= Internal State ==========
        private bool _isHovering;
        private bool _isPressed;
        private bool _isOverButtons;

        // ========= Constructor ==========
        public MusicItem()
        {
            InitializeComponent();
            UpdateMusicInfo();
        }

        // ========= Music 变更 ==========
        private static void OnMusicChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is MusicItem item)
                item.UpdateMusicInfo();
        }

        private void UpdateMusicInfo()
        {
            if (Music != null && !Music.isEmpty)
            {
                MusicNameText.Text = Music.Metadata.Title;
                ArtistNameText.Text = Music.Metadata.Artist;
            }
            else
            {
                MusicNameText.Text = Strings.UNKNOWN;
                ArtistNameText.Text = Strings.EMPTY;
            }
        }

        /// <summary>
        /// Update the background according to hover / press / button-area state.
        /// When mouse is over the button panel, no row-level hover effect is shown.
        /// </summary>
        private void UpdateBackground()
        {
            if (_isOverButtons)
            {
                RootGrid.Background = Brushes.Transparent;
                return;
            }

            if (_isPressed)
                RootGrid.Background = PressedBrush;
            else if (_isHovering)
                RootGrid.Background = HoverBrush;
            else
                RootGrid.Background = Brushes.Transparent;
        }

        // ========= 鼠标事件 ==========
        private void RootGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            _isHovering = true;
            UpdateBackground();
        }

        private void RootGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            _isHovering = false;
            _isPressed = false;
            _isOverButtons = false;
            UpdateBackground();
        }

        private void RootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPressed = true;
            UpdateBackground();
            RootGrid.CaptureMouse();

            var parameter = ClickCommandParameter ?? Music;
            if (ClickCommand != null && ClickCommand.CanExecute(parameter))
            {
                ClickCommand.Execute(parameter);
            }

            // 阻止事件冒泡到 ListBoxItem，避免列表项被选中
            e.Handled = true;
        }

        private void RootGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPressed = false;
            UpdateBackground();

            if (RootGrid.IsMouseCaptured)
                RootGrid.ReleaseMouseCapture();

            e.Handled = true;
        }

        private void RootGrid_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _isPressed = false;
            UpdateBackground();
        }

        // ========= 按钮区域鼠标事件 ==========
        private void ButtonPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            _isOverButtons = true;
            UpdateBackground();
        }

        private void ButtonPanel_MouseLeave(object sender, MouseEventArgs e)
        {
            _isOverButtons = false;

            // Mouse left the button panel but is still inside the row → show row hover.
            if (IsMouseOver)
                _isHovering = true;

            UpdateBackground();
        }
    }
}