using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tune.Components
{
    /// <summary>
    /// CheckBox.xaml 的交互逻辑
    /// </summary>
    public partial class CheckBox : UserControl
    {
        // 选中时的填充色：rgba(255,255,255,0.39) -> #63FFFFFF
        private static readonly Color CheckedFill = Color.FromArgb(0x63, 0xFF, 0xFF, 0xFF);

        private readonly SolidColorBrush _fillBrush = new SolidColorBrush(Colors.Transparent);

        public CheckBox()
        {
            InitializeComponent();
            Ring.Fill = _fillBrush;
            UpdateVisual();
        }

        #region IsChecked

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(CheckBox),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsCheckedChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CheckBox)d;
            ctrl.UpdateVisual();
            ctrl.RaiseEvent(new RoutedEventArgs(
                (bool)e.NewValue ? CheckedEvent : UncheckedEvent, ctrl));
        }

        private void UpdateVisual()
        {
            if (Ring == null) return;
            _fillBrush.Color = IsChecked ? CheckedFill : Colors.Transparent;
        }

        #endregion

        #region Checked / Unchecked 路由事件

        public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent(
            "Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CheckBox));

        public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent(
            "Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CheckBox));

        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }

        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        #endregion

        #region 交互

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!IsEnabled) return;

            IsChecked = !IsChecked;
            Focus();
            e.Handled = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsEnabled && (e.Key == Key.Space || e.Key == Key.Enter))
            {
                IsChecked = !IsChecked;
                e.Handled = true;
                return;
            }
            base.OnKeyDown(e);
        }

        #endregion
    }
}
