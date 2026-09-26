using System.Windows;
using System.Windows.Controls;

namespace Tune.Components.TabFrame
{
    /// <summary>
    /// Header = ?, Content = ?
    /// </summary>
    public class TabFrameItem : HeaderedContentControl
    {
        static TabFrameItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TabFrameItem),
                new FrameworkPropertyMetadata(typeof(TabFrameItem)));
        }

        private static readonly DependencyPropertyKey IsSelectedPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsSelected), typeof(bool), typeof(TabFrameItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSelectedProperty =
            IsSelectedPropertyKey.DependencyProperty;

        /// <summary>Only for TabFrame internal update. Use {Binding IsSelected}。</summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            internal set => SetValue(IsSelectedPropertyKey, value);
        }
    }
}