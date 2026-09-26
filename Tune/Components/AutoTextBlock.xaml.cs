using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tune.Components
{
    /// <summary>
    /// AutoTextBlock.xaml 的交互逻辑
    /// </summary>
    public partial class AutoTextBlock : UserControl
    {
        private enum DisplayMode { Normal, Half, Scroll }

        private bool _updating;
        private DisplayMode _currentMode = DisplayMode.Normal;

        public AutoTextBlock()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdateMode();
            SizeChanged += (s, e) => UpdateMode();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(AutoTextBlock),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(AutoTextBlock),
                new PropertyMetadata(TextAlignment.Left, OnTextChanged));

        public TextAlignment TextAlignment
        {
            get => (TextAlignment)GetValue(TextAlignmentProperty);
            set => SetValue(TextAlignmentProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((AutoTextBlock)d).UpdateMode();
        }

        // 当外部显式设置了字体相关属性时，把它转发给内部 TextBlock
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == FontSizeProperty ||
                e.Property == FontFamilyProperty ||
                e.Property == FontWeightProperty ||
                e.Property == FontStyleProperty ||
                e.Property == FontStretchProperty ||
                e.Property == ForegroundProperty)
            {
                ForwardFontProperty(e.Property, e.NewValue);
                UpdateMode();
            }
        }

        private void ForwardFontProperty(DependencyProperty prop, object value)
        {
            if (PART_Normal == null) return;

            // 如果 AutoTextBlock 上没有本地值 → 清掉内部 TextBlock 的本地值，
            // 让内部 TextBlock 继续走全局隐式样式
            if (ReadLocalValue(prop) == DependencyProperty.UnsetValue)
            {
                PART_Normal.ClearValue(prop);
                PART_Half.ClearValue(prop);
                PART_ScrollText.ClearValue(prop);
            }
            else
            {
                PART_Normal.SetValue(prop, value);
                PART_Half.SetValue(prop, value);
                PART_ScrollText.SetValue(prop, value);
            }
        }

        private void UpdateMode()
        {
            if (PART_Normal == null || _updating || !IsLoaded) return;

            var text = Text;
            if (string.IsNullOrEmpty(text)) return;

            double availW = ActualWidth;
            double availH = ActualHeight;
            if (availW <= 0 || availH <= 0) return;

            _updating = true;
            try
            {
                // 关键：从内部 TextBlock 上读取“当前生效值”，
                // 它可能是隐式样式里的，也可能是被转发过来的本地值
                double fs = PART_Normal.FontSize;
                var typeface = new Typeface(
                    PART_Normal.FontFamily,
                    PART_Normal.FontStyle,
                    PART_Normal.FontWeight,
                    PART_Normal.FontStretch);
                var fg = PART_Normal.Foreground;
                double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                // 模式 1：原字号、单行是否放得下
                var ft1 = new FormattedText(text, CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, typeface, fs, fg, dpi);
                if (ft1.Width <= availW + 0.5 && ft1.Height <= availH + 0.5)
                {
                    SetMode(DisplayMode.Normal, fs);
                    return;
                }

                // 模式 2：字号 / 2、换行后是否放得下
                var ft2 = new FormattedText(text, CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, typeface, fs / 2.0, fg, dpi);
                ft2.MaxTextWidth = Math.Max(1, availW);
                if (ft2.Height <= availH + 0.5)
                {
                    SetMode(DisplayMode.Half, fs);
                    return;
                }

                // 模式 3：滚动
                SetMode(DisplayMode.Scroll, fs);
            }
            finally
            {
                _updating = false;
            }
        }

        private void SetMode(DisplayMode mode, double baseFontSize)
        {
            if (_currentMode != mode)
            {
                _currentMode = mode;
                PART_Normal.Visibility = mode == DisplayMode.Normal ? Visibility.Visible : Visibility.Collapsed;
                PART_Half.Visibility = mode == DisplayMode.Half ? Visibility.Visible : Visibility.Collapsed;
                PART_Scroll.Visibility = mode == DisplayMode.Scroll ? Visibility.Visible : Visibility.Collapsed;
            }

            if (mode == DisplayMode.Half)
            {
                double half = baseFontSize / 2.0;
                if (Math.Abs(PART_Half.FontSize - half) > 0.01)
                {
                    // SetCurrentValue 不会破坏隐式样式（不会变成"本地值"）
                    PART_Half.SetCurrentValue(TextBlock.FontSizeProperty, half);
                }
            }
        }
    }
}
