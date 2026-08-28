using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tune.Helpers.ValueConverters
{

    class BoolToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 将 bool 转换为 Visibility。
        /// </summary>
        /// <param name="value">源布尔值。</param>
        /// <param name="targetType">目标类型（必须为 Visibility）。</param>
        /// <param name="parameter">可选参数。若传入 "Invert"，则结果取反（true→Collapsed, false→Visible）。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>对应的 Visibility 值。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue))
                return Visibility.Visible; // 或 DependencyProperty.UnsetValue，视情况而定

            bool invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 将 Visibility 转换回 bool。
        /// </summary>
        /// <param name="value">源 Visibility 值。</param>
        /// <param name="targetType">目标类型（必须为 bool）。</param>
        /// <param name="parameter">可选参数（与 Convert 中的参数对应）。</param>
        /// <param name="culture">区域信息。</param>
        /// <returns>对应的布尔值（Visible → true，其他 → false）。</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Visibility visibility))
                return false;

            bool invert = parameter?.ToString()?.Equals("Invert", StringComparison.OrdinalIgnoreCase) == true;
            bool isVisible = (visibility == Visibility.Visible);
            return invert ? !isVisible : isVisible;
        }
    }
}