using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Tune.Appearance;
using Tune.Helpers.Windows;
using Tune.Models.Messages;

namespace Tune
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowBlurHelper.ApplyBlur(this, useAcrylic: true);
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, OnNavigationMessage);
            NavigateTo(typeof(Main));
        }

        private void OnNavigationMessage(object recipient, NavigationMessage message)
        {
            NavigateTo(Resolve(message.Kind));
        }

        /// <summary>
        /// The only place that knows which type draws which page, so the view models stay free of
        /// any view type.
        /// </summary>
        private static Type Resolve(PageKind kind) => kind switch
        {
            PageKind.Playlists => typeof(PlaylistsPage),
            PageKind.CurrentPlaylist => typeof(CurrentPlaylist),
            PageKind.Settings => typeof(SettingsPage),
            PageKind.SoundEffect => typeof(SoundEffect),
            _ => typeof(Main)
        };

        private void NavigateTo(Type pageType)
        {
            if (App.Services?.GetRequiredService(pageType) is UIElement page)
            {
                PageContainer.Content = page;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            WeakReferenceMessenger.Default.Unregister<NavigationMessage>(this);
            base.OnClosed(e);
        }
    }
}