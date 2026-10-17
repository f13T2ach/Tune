using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Tune.Appearance;
using Tune.Assets.Localization;
using Tune.Client.Controllers;
using Tune.Client.Logger;
using Tune.Client.Presenters;
using Tune.Engine;
using Tune.Engine.Effects;
using Tune.Helpers;
using Tune.Helpers.Localization;
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
            // Has to run before the first dispatcher operation: the dispatcher restores the
            // culture it captured earlier, which would undo this call if it ran any later.
            LocalizationManager.ApplySavedLanguage();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
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

                // Sound effect processor: owns the volume and every effect parameter
                services.AddSingleton<IAudioEffectParameterStore, TuneSettingsParameterStore>();
                services.AddSingleton<IAudioEffectChain, AudioEffectChain>();
                services.AddSingleton<Tune.Persistence.EqualizerStorage.EqualizerPresetStore>();

                services.AddSingleton<PlayerController>();
                services.AddSingleton<ListController>();
                services.AddSingleton<MusicDisplayPresenter>();
                services.AddSingleton<MusicLyricPresenter>();
                services.AddSingleton<AudioLevelPresenter>();

                services.AddSingleton<MusicPlaybackController>();

                // Shared viewmodels & windows
                services.AddSingleton<Main>();
                services.AddSingleton<CurrentPlaylist>();
                services.AddSingleton<SoundEffect>();
                services.AddSingleton<PlaylistsPage>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<Tune.Appearance.SoundEffectTools.EqualizerPage>();

                services.AddSingleton<PlaylistsPageViewModel>();
                services.AddSingleton<SettingsPageViewModel>();
                services.AddSingleton<EqualizerPageViewModel>();
                services.AddSingleton<PlayerBarViewModel>();
                services.AddSingleton<MainViewModel>();
                services.AddTransient<CurrentPlaylistViewModel>();
                services.AddTransient<SoundEffectViewModel>();
                Services = services.BuildServiceProvider();

                // Bring back the equalizer scheme the player was left on, so the main page
                // shows it before any equalizer page has been opened. The kind is read from
                // the state and dispatched here, which keeps the two kinds alternatives and
                // leaves the graphic equalizer free to take over later.
                var presetStore = Services.GetRequiredService<Tune.Persistence.EqualizerStorage.EqualizerPresetStore>();
                var lastPreset = presetStore.Find(Tune.Helpers.UiState.UiStateStore.Get(EqualizerScheme.SchemeKey));

                if (lastPreset != null)
                {
                    Tune.Engine.Effects.EqualizerSchemeKind kind = EqualizerScheme.StoredKind();

                    // An older state file has no kind yet; the ten band one is the only
                    // equalizer that could have written it.
                    if (kind == Tune.Engine.Effects.EqualizerSchemeKind.None)
                        kind = Tune.Engine.Effects.EqualizerSchemeKind.TenBand;

                    if (kind == Tune.Engine.Effects.EqualizerSchemeKind.TenBand)
                    {
                        Services.GetRequiredService<IAudioEffectChain>().TenBand.ApplyGains(lastPreset.Gains);
                        EqualizerScheme.Current.Set(kind, lastPreset.Name);
                    }
                }

                // The lyric line limit is settled once at startup, the same way the language is.

                Services.GetRequiredService<MusicLyricPresenter>().LineLimit = Tune.Local.TuneSettings.Default.LyricLineLimit;

                // A secondary lyric of "third" shows the reading; anything else shows the translation.
                Services.GetRequiredService<MusicLyricPresenter>().SecondaryIsThirdLine =
                    string.Equals(Tune.Local.TuneSettings.Default.SecondaryLyric, "third", StringComparison.OrdinalIgnoreCase);

                // The meters read the level of the chain, which is the end of the audio path.
                Services.GetRequiredService<AudioLevelPresenter>()
                    .Attach(Services.GetRequiredService<IAudioEffectChain>());


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

                MessageBox.Show(string.Format(LangStrings.StartupFailed, LoggerInstance.Instance.LogFilePath), "Tune", MessageBoxButton.OK, MessageBoxImage.Error);
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