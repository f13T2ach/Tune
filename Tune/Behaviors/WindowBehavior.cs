using System;
using System.Windows;

namespace Tune.Behaviors
{
    public static class WindowBehavior
    {
        // ========== 附加属性：AspectRatio（宽高比） ==========
        public static readonly DependencyProperty AspectRatioProperty =
            DependencyProperty.RegisterAttached(
                "AspectRatio",
                typeof(double?),
                typeof(WindowBehavior),
                new PropertyMetadata(null, OnAspectRatioChanged));

        public static void SetAspectRatio(DependencyObject element, double? value)
            => element.SetValue(AspectRatioProperty, value);

        public static double? GetAspectRatio(DependencyObject element)
            => (double?)element.GetValue(AspectRatioProperty);

        // ========== 附加属性：MinWidth（最小宽度） ==========
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.RegisterAttached(
                "MinWidth",
                typeof(double),
                typeof(WindowBehavior),
                new PropertyMetadata(800.0));

        public static void SetMinWidth(DependencyObject element, double value)
            => element.SetValue(MinWidthProperty, value);

        public static double GetMinWidth(DependencyObject element)
            => (double)element.GetValue(MinWidthProperty);

        // ========== 附加属性：MinHeight（最小高度） ==========
        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.RegisterAttached(
                "MinHeight",
                typeof(double),
                typeof(WindowBehavior),
                new PropertyMetadata(400.0));

        public static void SetMinHeight(DependencyObject element, double value)
            => element.SetValue(MinHeightProperty, value);

        public static double GetMinHeight(DependencyObject element)
            => (double)element.GetValue(MinHeightProperty);

        // ========== 属性变更处理 ==========
        private static void OnAspectRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Window window) return;

            var aspectRatio = e.NewValue as double?;
            if (aspectRatio.HasValue)
            {
                // 订阅 SizeChanged 事件
                window.SizeChanged += OnWindowSizeChanged;
                // 设置最小尺寸（如果未显式设置，则使用默认值）
                if (!window.IsLoaded)
                    window.Loaded += (s, _) => ApplyMinSize(window);
                else
                    ApplyMinSize(window);
            }
            else
            {
                window.SizeChanged -= OnWindowSizeChanged;
            }
        }

        private static void ApplyMinSize(Window window)
        {
            window.MinWidth = GetMinWidth(window);
            window.MinHeight = GetMinHeight(window);
        }

        private static void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = (Window)sender;
            var aspectRatio = GetAspectRatio(window);
            if (!aspectRatio.HasValue) return;

            // 避免递归
            if (window.IsLoaded && window.WindowState == WindowState.Maximized)
                return;

            // 判断是宽度还是高度变化（避免锁死）
            double widthChange = Math.Abs(e.NewSize.Width - e.PreviousSize.Width);
            double heightChange = Math.Abs(e.NewSize.Height - e.PreviousSize.Height);

            if (widthChange > 0.1 && heightChange <= 0.1)
            {
                // 水平拖拽 → 按比例调整高度
                double desiredHeight = e.NewSize.Width / aspectRatio.Value;
                if (desiredHeight >= window.MinHeight && desiredHeight <= SystemParameters.VirtualScreenHeight)
                {
                    window.Height = desiredHeight;
                }
            }
            else if (heightChange > 0.1 && widthChange <= 0.1)
            {
                // 垂直拖拽 → 按比例调整宽度
                double desiredWidth = e.NewSize.Height * aspectRatio.Value;
                if (desiredWidth >= window.MinWidth && desiredWidth <= SystemParameters.VirtualScreenWidth)
                {
                    window.Width = desiredWidth;
                }
            }
        }
    }
}