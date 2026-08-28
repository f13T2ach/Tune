using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tune.Components.Frame
{
    /// <summary>
    /// WindowFrame.xaml 的交互逻辑
    /// </summary>
    public partial class WindowFrame : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(WindowFrame), new PropertyMetadata("窗口"));

        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(WindowFrame), new PropertyMetadata(Brushes.White));

        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(WindowFrame), new PropertyMetadata(new Thickness(2.5)));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public new Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public new Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public WindowFrame()
        {
            InitializeComponent();
            //NameScope.SetNameScope(this, null);
        }
    }
}
