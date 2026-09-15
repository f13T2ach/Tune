using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Client.Presenters;
using Tune.Helpers.FormatConverters;
using Tune.Local;
using Tune.Models.Music;
using Tune.Models.Results;

namespace Tune.Client.Controllers
{
    /// <summary>
    /// Global playback scheduler.
    /// Owns the workflow of switching to another music:
    /// load into engine, refresh display metadata and reset lyrics.
    /// Any ViewModel that needs to switch tracks should inject this controller
    /// instead of manipulating PlayerController / Presenters individually.
    /// </summary>
    public partial class MusicPlaybackController : ObservableObject
    {
        private readonly PlayerController _playerController;
        private readonly MusicDisplayPresenter _musicDisplay;
        private readonly MusicLyricPresenter _musicLyric;

        /// <summary>
        /// The music currently loaded into the player.
        /// Single source of truth shared across all pages.
        /// </summary>
        [ObservableProperty]
        private Music _currentMusic = new Music();

        public MusicPlaybackController(
            PlayerController playerController,
            MusicDisplayPresenter musicDisplay,
            MusicLyricPresenter musicLyric)
        {
            _playerController = playerController;
            _musicDisplay = musicDisplay;
            _musicLyric = musicLyric;

            // Progress-related events are subscribed here so that individual
            // page ViewModels do not have to duplicate this wiring.
            _playerController.ProgressUpdated += (_, seconds) =>
                _musicLyric.UpdateLyricsBySecond(seconds);
            _playerController.TotalSecondsChanged += (_, _) =>
                _musicDisplay.SetTotalSeconds(_playerController.TotalSeconds);
        }

        /// <summary>
        /// Switch to another music and load it into the player.
        /// 1. Instance is EMPTY:
        ///    Unload the player, reset current lyric data.
        /// 2. Instance is VALID:
        ///    Load file, load current lyric data, set volume.
        /// Afterwards, reset the playing progress.
        /// </summary>
        /// <param name="music"></param>
        public PlayerControllerLoadResult PlayMusic(Music music)
        {
            if (music == null)
                return new PlayerControllerLoadResult(false);

            PlayerControllerLoadResult result;
            if (!music.isEmpty)
            {
                double volume =
                    UIVolumeToDoubleVolume.Convert(
                        TuneSettings.Default.PlayerVolume);
                result = _playerController.Load(music.MusicPath, volume);
            }
            else
            {
                _playerController.Unload();
                result = new PlayerControllerLoadResult(true);
            }

            if (!result.IsSucceeded)
                return result;

            CurrentMusic = music;
            _musicLyric.UpdateLyric(music.Lyrics);   // It would handle lyric load failure
            _musicDisplay.Update(music);
            _musicLyric.UpdateLyricsBySecond(0);
            _musicDisplay.SetTotalSeconds(_playerController.TotalSeconds);
            return result;
        }

        /// <summary>
        /// Unload the current playing music (STOP MUSIC).
        /// Behaviour: load an empty Music();
        /// </summary>
        public void Unload() => PlayMusic(new Music());

        /// <summary>
        /// Play
        /// </summary>
        public void Play() => _playerController.Play();

        /// <summary>
        /// Pause
        /// </summary>
        public void Pause() => _playerController.Pause();

        /// <summary>
        /// Stop
        /// </summary>
        public void Stop()
        {
            _playerController.Stop();
            _musicLyric.UpdateLyricsBySecond(0);
        }
    }
}