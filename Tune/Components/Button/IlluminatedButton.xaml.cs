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
    public partial class IlluminatedButton : UserControl
    {
        private static readonly Color DefaultFill = Color.FromArgb(38, 0xFF, 0xFF, 0xFF);
        private static readonly Color CheckedFill = Color.FromArgb(255, 0xFF, 0xFF, 0xFF);
        private readonly SolidColorBrush _fillBrush = new SolidColorBrush(Colors.Transparent);

        public IlluminatedButton()
        {
            InitializeComponent();
            BackgroundBorder.Background = new SolidColorBrush(DefaultFill);
            UpdateVisual();
        }

        #region IsChecked

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(IlluminatedButton),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsCheckedChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(IlluminatedButton),
                new PropertyMetadata("BUTTON", OnTextChanged));
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (IlluminatedButton)d;
            ctrl.UpdateVisual();
            ctrl.RaiseEvent(new RoutedEventArgs(
                (bool)e.NewValue ? CheckedEvent : UncheckedEvent, ctrl));
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as IlluminatedButton;
            if (control != null && control.ContentText != null)
            {
                control.ContentText.Text = e.NewValue?.ToString() ?? "";
            }
        }

        private void UpdateVisual()
        {
            if (BackgroundBorder == null|| ContentText == null) return;
            BackgroundBorder.Background = IsChecked ? new SolidColorBrush(CheckedFill) : new SolidColorBrush(DefaultFill);
            ContentText.Foreground = IsChecked ? new SolidColorBrush(Colors.Black): new SolidColorBrush(Colors.White);

        }

        #endregion

        #region Checked / Unchecked Routed Event

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

        #region Interact

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
