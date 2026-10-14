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

            string text = Text;
            double availW = ActualWidth;

            _updating = true;
            try
            {
                double fs = PART_Normal.FontSize;

                if (string.IsNullOrEmpty(text) || availW <= 0)
                {
                    SetMode(DisplayMode.Normal, fs);
                    return;
                }

                // 关键：从内部 TextBlock 上读取“当前生效值”，
                // 它可能是隐式样式里的，也可能是被转发过来的本地值
                var typeface = new Typeface(
                    PART_Normal.FontFamily,
                    PART_Normal.FontStyle,
                    PART_Normal.FontWeight,
                    PART_Normal.FontStretch);
                var fg = PART_Normal.Foreground;
                double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

                // 只比较宽度。比较高度会形成自反馈：换行和滚动条都会改变本控件
                // 的高度，而行高又由内容决定，于是模式不停来回切换。
                // 迟滞 1px 让已经生效的模式在边界上保持稳定。
                const double slack = 1.0;

                var full = new FormattedText(text, CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, typeface, fs, fg, dpi);
                double fullSlack = _currentMode == DisplayMode.Normal ? slack : 0.0;
                if (full.Width <= availW + fullSlack)
                {
                    SetMode(DisplayMode.Normal, fs);
                    return;
                }

                var half = new FormattedText(text, CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, typeface, fs / 2.0, fg, dpi);
                double halfSlack = _currentMode == DisplayMode.Half ? slack : 0.0;
                if (half.Width <= availW + halfSlack)
                {
                    SetMode(DisplayMode.Half, fs);
                    return;
                }

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
