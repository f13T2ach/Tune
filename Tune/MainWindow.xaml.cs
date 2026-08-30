using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using Tune.Appearance;
using Tune.Models.Messages;
using WPFLocalizeExtension.Engine;

namespace Tune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LocalizeDictionary.Instance.Culture = new CultureInfo("zh-CN");
            //this.WindowState = WindowState.Maximized;
            // Register navigation message
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, OnNavigationMessage);

            // Set default page
            NavigateTo(typeof(Main));
        }

        private void OnNavigationMessage(object recipient, NavigationMessage message)
        {
            NavigateTo(message.PageType);
        }

        private void NavigateTo(Type pageType)
        {
            // Get page instance from DI docker
            if (App.Services?.GetRequiredService(pageType) is UIElement page)
            {
                PageContainer.Content = page;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cancel the registration for avoiding the memory leaking
            WeakReferenceMessenger.Default.Unregister<NavigationMessage>(this);
            base.OnClosed(e);
        }


    }
}