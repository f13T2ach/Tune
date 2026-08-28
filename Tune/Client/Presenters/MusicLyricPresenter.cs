using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Models.Lyric;
using Tune.Values.Static;

namespace Tune.Client.Presenters
{
    public partial class MusicLyricPresenter : ObservableObject
    {
        [ObservableProperty]
        private string _lyric = Strings.NO_CONTENT;
        [ObservableProperty]
        private string _translatedLyric = Strings.NO_CONTENT;
        private LyricBase? _lyricData;

        /// <summary>
        /// Init Lyric Presenter
        /// Reset this instance by setting agrument null
        /// </summary>
        /// <param name="lyricData"></param>
        public void UpdateLyric(LyricBase? lyricData)
        {
            _lyricData = lyricData;
        }

        /// <summary>
        /// Update Current Lyrics
        /// 1. NULL
        ///    Show NO_CONTENT
        /// 2. VALID & NULL
        ///    Show part
        /// **NOTE**: The main lyric would hold last while the translate lyric would not.
        ///           Some music have a (or more) part(s) of non-translated content, 
        ///           we cant hold the last lyric line, which would make user confused.
        /// </summary>
        /// <param name="seconds"></param>
        public void UpdateLyricsBySecond(double seconds)
        {
            if (_lyricData == null)
            {
                Lyric = Strings.NO_CONTENT;
                TranslatedLyric = Strings.NO_CONTENT;
                return;
            }

            Lyric = _lyricData.GetLyricAtTime(seconds, _lyricData.Lyrics);

            if (_lyricData.isTranslated && _lyricData.TranslatedLyrics != null)
            {
                TranslatedLyric = _lyricData.GetLyricAtTime(seconds, _lyricData.TranslatedLyrics, false);
            }
            else
            {
                TranslatedLyric = Strings.EMPTY;
            }
        }
    }
}
