using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tune.Components
{
    public partial class SwitchMusic : UserControl
    {
        // 1. 注册 LastCommand 依赖属性
        public static readonly DependencyProperty LastCommandProperty =
            DependencyProperty.Register(
                nameof(LastCommand),
                typeof(ICommand),
                typeof(SwitchMusic),
                new PropertyMetadata(null));

        // 2. 注册 NextCommand 依赖属性
        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register(
                nameof(NextCommand),
                typeof(ICommand),
                typeof(SwitchMusic),
                new PropertyMetadata(null));

        // 3. CLR 包装器 (供 XAML 绑定使用)
        public ICommand LastCommand
        {
            get => (ICommand)GetValue(LastCommandProperty);
            set => SetValue(LastCommandProperty, value);
        }

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        public SwitchMusic()
        {
            InitializeComponent();
        }
    }
}