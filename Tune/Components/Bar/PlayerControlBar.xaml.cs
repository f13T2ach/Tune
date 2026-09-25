using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using Tune.ViewModel;

namespace Tune.Components.Bar
{
    public partial class PlayerControlBar : UserControl
    {
        /// <summary>
        /// Aggregated view model (typically PlayerBarViewModel).
        /// Assigned to DataContext so bindings inside the component resolve
        /// with the same paths used by the hosting page.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(object),
                typeof(PlayerControlBar),
                new PropertyMetadata(null, OnViewModelChanged));

        public object? ViewModel
        {
            get => GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public PlayerControlBar()
        {
            InitializeComponent();
            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<PlayerBarViewModel>();
            }
        }

        private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PlayerControlBar)d).DataContext = e.NewValue;
        }
    }
}