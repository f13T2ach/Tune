using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using Tune.Appearance.SoundEffectTools;
using Tune.Helpers;
using Tune.ViewModel;

namespace Tune.Appearance
{
    public partial class SoundEffect : UserControl
    {
        private readonly IServiceProvider _isolatedProvider;
        private readonly string _name = "声效工具";
        private bool _isDisposed;   // 标志是否已释放

        public SoundEffect()
        {
            var services = new ServiceCollection();

            services.AddTransient<HelpPage>();
            services.AddTransient<EqualizerPage>();
            services.AddTransient<EffectPage>();
            services.AddTransient<AnalysisPage>();
            services.AddTransient<SongPage>();
            services.AddTransient<SoundEffectMain>();

            _isolatedProvider = services.BuildServiceProvider();
            InitializeComponent();

            TitleButton.Text = _name;
            if (App.Services != null)
            {
                DataContext = App.Services.GetRequiredService<SoundEffectViewModel>();
            }
            NavigateTo(typeof(SoundEffectMain));
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _isDisposed = true;
            if (_isolatedProvider is IDisposable disp)
                disp.Dispose();
        }

        private void OnToolBarButtonClick(object sender, RoutedEventArgs e)
        {
            // 如果已释放或控件未加载，忽略事件
            if (_isDisposed || !IsLoaded) return;

            if (sender is Components.Button btn && btn.Tag is string typeName)
            {
                var pageType = Type.GetType(typeName);
                if (pageType != null)
                {
                    NavigateTo(pageType);
                    TitleButton.Text = btn.Text;  // 同步标题
                }
            }
        }

        private void OnBackToMainClick(object sender, RoutedEventArgs e)
        {
            if (_isDisposed || !IsLoaded) return;

            NavigateTo(typeof(SoundEffectMain));
            TitleButton.Text = _name;
        }

        private void NavigateTo(Type pageType)
        {
            // 再次确保未被释放（以防内部调用未检查）
            if (_isDisposed || _isolatedProvider == null) return;

            try
            {
                if (_isolatedProvider.GetRequiredService(pageType) is UserControl page)
                {
                    if (PageContainer.Content is IDisposable oldPage)
                        oldPage.Dispose();

                    PageContainer.Content = page;
                }
            }
            catch (ObjectDisposedException ex)
            {
                LoggerHelper.LogError("SoundEffect Failed to navigate to page: " + ex.Message, ex, nameof(NavigateTo));
            }
        }
    }
}