using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Tune.Components
{
    public partial class Button : UserControl
    {
        // 依赖属性
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(Button),
                new PropertyMetadata("Button", OnTextChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(Button),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(Button),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsDisabledProperty =
            DependencyProperty.Register("IsDisabled", typeof(bool), typeof(Button),
                new PropertyMetadata(false, OnIsDisabledChanged));

        // 颜色定义
        private readonly Brush DefaultBg = new SolidColorBrush(Colors.Black);
        private readonly Brush DefaultBorder = new SolidColorBrush(Colors.White);
        private readonly Brush HoverBg = new SolidColorBrush(Color.FromRgb(74, 74, 74));
        private readonly Brush HoverBorder = new SolidColorBrush(Color.FromRgb(224, 224, 224));
        private readonly Brush PressBg = new SolidColorBrush(Color.FromRgb(26, 26, 26));
        private readonly Brush PressBorder = new SolidColorBrush(Color.FromRgb(170, 170, 170));
        private readonly Brush DisabledBorder = new SolidColorBrush(Color.FromRgb(90, 90, 90));

        // 状态标志
        private bool _isPressed = false;

        // 字体改变
        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;
            if (button.ButtonText == null) return;

            // 检查是否有本地显式设置的值
            if (button.ReadLocalValue(FontSizeProperty) != DependencyProperty.UnsetValue)
            {
                // 显式设置了，应用该值
                button.ButtonText.SetCurrentValue(TextBlock.FontSizeProperty, (double)e.NewValue);
            }
            else
            {
                // 未显式设置，清除 TextBlock 的本地值，让它继承全局样式
                button.ButtonText.ClearValue(TextBlock.FontSizeProperty);
            }
        }

        public Button()
        {
            InitializeComponent();
            // 初始状态
            SetDefaultState();

            Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //do nothing
        }

        static Button()
        {
            // 重写 FontSize 属性的元数据，添加变更回调
            FontSizeProperty.OverrideMetadata(
                typeof(Button),
                new FrameworkPropertyMetadata(
                    12.0,  // 默认值（不重要，因为我们会判断本地值）
                    FrameworkPropertyMetadataOptions.Inherits,
                    OnFontSizeChanged));
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        /// <summary>
        /// When true the button looks unavailable and ignores both the mouse and its command.
        /// </summary>
        public bool IsDisabled
        {
            get { return (bool)GetValue(IsDisabledProperty); }
            set { SetValue(IsDisabledProperty, value); }
        }

        private static void OnIsDisabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as Button)?.ApplyDisabledState();
        }

        /// <summary>Applies the look that matches the current availability.</summary>
        private void ApplyDisabledState()
        {
            if (IsDisabled)
            {
                _isPressed = false;
                Mouse.Capture(null);
                MainBorder.Background = DefaultBg;
                MainBorder.BorderBrush = DisabledBorder;
                MainBorder.RenderTransform = null;
                ButtonText.Opacity = 0.4;
            }
            else
            {
                ButtonText.Opacity = 1;
                SetDefaultState();
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as Button;
            if (control != null && control.ButtonText != null)
            {
                control.ButtonText.Text = e.NewValue?.ToString() ?? "";
            }
        }

        // 鼠标进入
        private void MainBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsDisabled) return;

            if (!_isPressed)
            {
                SetHoverState();
            }
        }

        // 鼠标离开
        private void MainBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsDisabled) return;

            if (!_isPressed)
            {
                SetDefaultState();
            }
            // 如果按下状态还在，但鼠标离开了，说明用户拖拽出去了，释放捕获并重置
            else
            {
                _isPressed = false;
                SetDefaultState();
                Mouse.Capture(null);
            }
        }

        // 鼠标左键按下
        private void MainBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsDisabled)
            {
                e.Handled = true;
                return;
            }

            _isPressed = true;
            SetPressState();
            Mouse.Capture(MainBorder); // 捕获鼠标确保能收到 MouseUp
            e.Handled = true; // 防止事件继续传递
        }

        // 鼠标左键释放
        private void MainBorder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDisabled)
            {
                e.Handled = true;
                return;
            }

            if (_isPressed)
            {
                _isPressed = false;
                Mouse.Capture(null);
                if (IsMouseOver)
                    SetHoverState();
                else
                    SetDefaultState();

                if (IsMouseOver)
                {
                    ExecuteClick();
                }
                e.Handled = true;
            }
        }

        // 设置默认状态
        private void SetDefaultState()
        {
            MainBorder.Background = DefaultBg;
            MainBorder.BorderBrush = DefaultBorder;
            MainBorder.RenderTransform = null;
        }

        // 设置悬停状态
        private void SetHoverState()
        {
            MainBorder.Background = HoverBg;
            MainBorder.BorderBrush = HoverBorder;
            MainBorder.RenderTransform = null;
        }

        // 设置按下状态
        private void SetPressState()
        {
            MainBorder.Background = PressBg;
            MainBorder.BorderBrush = PressBorder;
            MainBorder.RenderTransform = new ScaleTransform(0.97, 0.97);
            MainBorder.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        // 执行点击
        private void ExecuteClick()
        {
            if (IsDisabled) return;

            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }

            // 检查 Click 是否被订阅
            if (Click == null)
            {
                return;
            }
            else
            {
                Click?.Invoke(this, new RoutedEventArgs());
            }
        }

        // 点击事件
        public event RoutedEventHandler Click;
    }
}
