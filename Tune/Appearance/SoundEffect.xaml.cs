using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using Tune.Appearance.SoundEffectTools;
using Tune.Assets.Localization;
using Tune.Helpers;
using Tune.ViewModel;

namespace Tune.Appearance
{
    public partial class SoundEffect : UserControl
    {
        private IServiceProvider? _isolatedProvider;
        private readonly string _name = LangStrings.SoundEffectTools;
        private bool _isDisposed;

        // Remember te current page type to restore it when reloading the control
        // Remember te current page type to restore it when reloading the control
        private Type _currentPageType = typeof(SoundEffectMain);

        public SoundEffect()
        {
            InitializeComponent();

            TitleButton.Text = _name;
            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<SoundEffectViewModel>();
            }

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // rebuild the isolated service provider if it has been disposed
            if (_isolatedProvider == null || _isDisposed)
            {
                var services = new ServiceCollection();

                services.AddTransient<HelpPage>();
                services.AddTransient<EqualizerPage>();
                services.AddTransient<EffectPage>();
                services.AddTransient<AnalysisPage>();
                services.AddTransient<SongPage>();
                services.AddTransient<SoundEffectMain>();

                _isolatedProvider = services.BuildServiceProvider();
                _isDisposed = false;
            }

            // back to the last page if it was recorded, otherwise go to the main page
            NavigateTo(_currentPageType);

            // updae the title based on the current page type
            UpdateTitle();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isDisposed = true;
            if (_isolatedProvider is IDisposable disp)
                disp.Dispose();
            _isolatedProvider = null;
        }

        private void OnToolBarButtonClick(object sender, RoutedEventArgs e)
        {
            if (_isDisposed || !IsLoaded) return;

            if (sender is Components.Button btn && btn.Tag is string typeName)
            {
                var pageType = Type.GetType(typeName);
                if (pageType != null)
                {
                    _currentPageType = pageType;   // rec
                    NavigateTo(pageType);
                    TitleButton.Text = btn.Text;
                }
            }
        }

        private void OnBackToMainClick(object sender, RoutedEventArgs e)
        {
            if (_isDisposed || !IsLoaded) return;

            _currentPageType = typeof(SoundEffectMain);   // rec
            NavigateTo(_currentPageType);
            TitleButton.Text = _name;
        }

        private void NavigateTo(Type pageType)
        {
            if (_isDisposed || _isolatedProvider == null) return;

            try
            {
                if (_isolatedProvider.GetRequiredService(pageType) is UserControl page)
                {
                    // Tool pages are built by the isolated provider and own no view model,
                    // so they share this one to reach the same effect processor.
                    page.DataContext ??= DataContext;

                    if (PageContainer.Content is IDisposable oldPage)
                        oldPage.Dispose();

                    PageContainer.Content = page;
                }
            }
            catch (ObjectDisposedException ex)
            {
                LoggerHelper.LogError(
                    "SoundEffect Failed to navigate to page: " + ex.Message,
                    ex, nameof(NavigateTo));
            }
        }


        private void UpdateTitle()
        {
            if (_currentPageType == typeof(SoundEffectMain))
            {
                TitleButton.Text = _name;
                return;
            }

            foreach (var child in FindVisualChildren<Components.Button>(this))
            {
                if (child.Tag is string tag &&
                    Type.GetType(tag) == _currentPageType)
                {
                    TitleButton.Text = child.Text;
                    return;
                }
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null) yield break;
            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T t) yield return t;
                foreach (var d in FindVisualChildren<T>(child))
                    yield return d;
            }
        }
    }
}