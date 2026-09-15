using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Windows;
using Tune.Appearance;
using Tune.Client.Controllers;
using Tune.Client.Logger;
using Tune.Client.Presenters;
using Tune.Engine;
using Tune.Helpers;
using Tune.Persistence.PlaylistStorage;
using Tune.Services;
using Tune.Services.FileDialog;
using Tune.Services.Interfaces;
using Tune.Utils.FileUtils;
using Tune.ViewModel;

namespace Tune
{
    public partial class App : Application
    {
        public static ServiceProvider? Services { get; private set; }

        public App()
        {

        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");
            base.OnStartup(e);

            try
            {
                // Work environment init
                EnvironmentUtils.InitEnvironment();

                // DI build up
                var services = new ServiceCollection();
                services.AddSingleton<IFileDialogService, FileDialogService>();
                services.AddSingleton<IAudioPlayerEngine, AudioPlayerEngine>();
                services.AddSingleton<ITimeService, TimeService>();
                services.AddSingleton<IAudioDeviceService, AudioDeviceService>();

                services.AddSingleton<PlayerController>();          // 注意：PlayerController 依赖 IAudioPlayerEngine，DI 会自动注入
                services.AddSingleton<ListController>();
                services.AddSingleton<MusicDisplayPresenter>();
                services.AddSingleton<MusicLyricPresenter>();

                services.AddSingleton<MusicPlaybackController>();

                // Shared viewmodels & windows
                services.AddSingleton<Main>();
                services.AddSingleton<CurrentPlaylist>();
                services.AddSingleton<SoundEffect>();

                services.AddSingleton<MainViewModel>();
                services.AddTransient<CurrentPlaylistViewModel>();
                services.AddTransient<SoundEffectViewModel>();
                Services = services.BuildServiceProvider();

                // Register loadable services
                var provider = new PlaylistProvider();
                var playlistService = new PlaylistService(provider);
                LoadableServicesManager.Register(playlistService);

                // load every background services
                await LoadableServicesManager.LoadAllAsync();

                //throw new Exception("test");
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Tune encountered an error while starting up, log file has been already saved at {LoggerInstance.Instance.LogFilePath}", "Tune", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggerHelper.LogError("Failed at starting up.", ex, nameof(OnStartup));
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Services?.GetRequiredService<IAudioPlayerEngine>()?.Unload();
            // LoadableServicesManager.
            base.OnExit(e);
        }
    }
}