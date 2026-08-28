using System.Windows;
using System.Windows.Controls;

namespace Tune.Components.InputBox
{
    /// <summary>
    /// SimpleInputBox.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleInputBox : UserControl
    {
        // 依赖属性：实际输入文本
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SimpleInputBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // 依赖属性：提示文本
        public static readonly DependencyProperty HintProperty =
            DependencyProperty.Register("Hint", typeof(string), typeof(SimpleInputBox),
                new PropertyMetadata(string.Empty));

        public string Hint
        {
            get => (string)GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public SimpleInputBox()
        {
            InitializeComponent();
            UpdateHintVisibility(); // 初始化提示显示状态
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            // 获得焦点时隐藏提示（即使没文字）
            PART_Hint.Visibility = Visibility.Collapsed;
        }

        private void OnLostFocus(object sender, RoutedEventArgs e)
        {
            // 失去焦点时根据文字是否为空决定提示显示
            UpdateHintVisibility();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // 文本变化时，若当前未聚焦则更新提示；若聚焦则始终隐藏
            if (!PART_TextBox.IsFocused)
                UpdateHintVisibility();
        }

        private void UpdateHintVisibility()
        {
            // 提示显示条件：未聚焦 且 文本为空
            if (!PART_TextBox.IsFocused && string.IsNullOrEmpty(PART_TextBox.Text))
                PART_Hint.Visibility = Visibility.Visible;
            else
                PART_Hint.Visibility = Visibility.Collapsed;
        }
    }
}
