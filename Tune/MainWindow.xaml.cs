using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using Tune.Appearance;
using Tune.Assets.Localization;
using Tune.Models.Messages;
using WPFLocalizeExtension.Engine;

namespace Tune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isResizing;
        private readonly double _aspectRatio = 1613.0 / 829.0; // 设计宽高比
        public MainWindow()
        {
            InitializeComponent();

            // Register navigation message
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, OnNavigationMessage);

            this.MinWidth = 800;
            this.MinHeight = 400;

            this.SizeChanged += Window_SizeChanged;
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

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Avoid Loop
            if (_isResizing) return;
            _isResizing = true;

            // 如果窗口处于最大化或最小化状态，不强制比例（可选）
            if (this.WindowState == WindowState.Maximized)
            {
                _isResizing = false;
                return;
            }

            // 判断是宽度变化还是高度变化（根据变化量）
            double widthChange = Math.Abs(e.NewSize.Width - e.PreviousSize.Width);
            double heightChange = Math.Abs(e.NewSize.Height - e.PreviousSize.Height);

            if (widthChange > 0.1) // 用户水平拖拽
            {
                double desiredHeight = e.NewSize.Width / _aspectRatio;
                // 限制高度在合理范围内（可选）
                if (desiredHeight >= this.MinHeight && desiredHeight <= SystemParameters.VirtualScreenHeight)
                {
                    this.Height = desiredHeight;
                }
            }
            else if (heightChange > 0.1) // 用户垂直拖拽
            {
                double desiredWidth = e.NewSize.Height * _aspectRatio;
                if (desiredWidth >= this.MinWidth && desiredWidth <= SystemParameters.VirtualScreenWidth)
                {
                    this.Width = desiredWidth;
                }
            }

            _isResizing = false;
        }


    }
}