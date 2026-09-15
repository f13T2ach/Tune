using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Helpers.ValueConverters;
using Tune.Models.Music;
using Tune.Values.Static;

namespace Tune.Client.Presenters
{
    public partial class MusicDisplayPresenter : ObservableObject
    {
        [ObservableProperty]
        private string _title = Strings.NO_CONTENT;

        [ObservableProperty]
        private string _artist = Strings.NO_CONTENT;

        [ObservableProperty]
        private string _album = Strings.NO_CONTENT;

        [ObservableProperty]
        private string _rateText = Strings.RATE_SAMPLE_DIVED_BPS;   // Sample Rate / Bitrate

        [ObservableProperty]
        private string _duration = Strings.DURATION_MIN_SEC_ZERO;   // Duration For Display

        [ObservableProperty]
        private double _totalSeconds = 0;

        [ObservableProperty]
        private bool _isTranslated = false;

        [ObservableProperty]
        private bool _isSingle = false;

        public Music? CurrentMusic;

        /// <summary>
        /// Update Metadata of the Music Instance for dislpay
        /// Setting the argument null **CANNOT** reset this instance.
        /// To reset, call Update(new Music())
        /// </summary>
        public void Update(Music music)
        {
            CurrentMusic = music;

            if (music == null)
                return;

            Title = music.Metadata.Title ?? Strings.NO_CONTENT;
            Artist = music.Metadata.Artist ?? Strings.NO_CONTENT;
            Album = music.Metadata.Album ?? Strings.NO_CONTENT;
            RateText = $"{music.Metadata.SampleRateKHz}/{music.Metadata.BitrateKbps}";


            IsSingle = !string.IsNullOrWhiteSpace(music.Metadata.Title)
                       && !string.IsNullOrWhiteSpace(music.Metadata.Album)
                       && string.Equals(
                           music.Metadata.Title.Trim(),
                           music.Metadata.Album.Trim(),
                           StringComparison.OrdinalIgnoreCase);


            IsTranslated = music.isLyricsAvailable
                           && music.Lyrics != null
                           && music.Lyrics.isTranslated;

        }

        /// <summary>
        /// Update Total seconds and format it
        /// </summary>
        public void SetTotalSeconds(double seconds)
        {
            TotalSeconds = seconds;
            Duration = SecondsToDurationConverter.StaticConvert(seconds);
        }
    }
}
