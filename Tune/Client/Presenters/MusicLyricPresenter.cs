using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Tune.Models.Lyric;
using Tune.Values.Static;

namespace Tune.Client.Presenters
{
    public partial class MusicLyricPresenter : ObservableObject
    {
        /// <summary>Most lines one timestamp can show: original, translation, romanisation.</summary>
        public const int MaxLines = 3;

        private int _lineLimit = MaxLines;
        private bool _secondaryIsThirdLine;
        private LyricBase? _lyricData;

        [ObservableProperty]
        private string _lyric = Strings.NO_CONTENT;

        /// <summary>
        /// Gets where the current lyric sits, written as "current/total", or the no-content
        /// placeholder when no lyric is loaded or the first line has not been reached.
        /// </summary>
        [ObservableProperty]
        private string _lyricPosition = NoPosition;

        /// <summary>The readout shown while nothing is playing: the placeholder on both sides.</summary>
        private static string NoPosition => $"{Strings.NO_CONTENT}/{Strings.NO_CONTENT}";

        /// <summary>
        /// Gets or sets a value indicating whether the secondary line is the third line of the
        /// timestamp, which is normally the reading. The translation is used when there is no
        /// third line, so asking for it never leaves the secondary line empty.
        /// </summary>
        public bool SecondaryIsThirdLine
        {
            get => _secondaryIsThirdLine;
            set
            {
                if (value == _secondaryIsThirdLine) return;

                _secondaryIsThirdLine = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the lines shown under the main one. They belong to the same timestamp, so they
        /// are normally the translation and, when the file has one, the romanisation.
        /// </summary>
        public ObservableCollection<string> ExtraLines { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets or sets how many lines of a timestamp are shown. Lowering it hides the lines at
        /// the end, so a limit of two leaves the romanisation out.
        /// </summary>
        public int LineLimit
        {
            get => _lineLimit;
            set
            {
                int wanted = Math.Clamp(value, 1, MaxLines);
                if (wanted == _lineLimit) return;

                _lineLimit = wanted;
                OnPropertyChanged();
            }
        }

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
        /// **NOTE**: The main lyric would hold last while the extra lines would not.
        ///           Some music have a (or more) part(s) of non-translated content,
        ///           we cant hold the last lyric line, which would make user confused.
        /// </summary>
        /// <param name="seconds"></param>
        public void UpdateLyricsBySecond(double seconds)
        {
            if (_lyricData == null)
            {
                Lyric = Strings.NO_CONTENT;
                LyricPosition = NoPosition;
                ExtraLines.Clear();
                return;
            }

            List<string> lines = _lyricData.GetLyricLinesAtTime(seconds, _lineLimit);

            Lyric = lines.Count > 0 ? lines[0] : Strings.NO_CONTENT;

            int position = _lyricData.GetLyricPositionAtTime(seconds);
            LyricPosition = position > 0
                ? $"{position}/{_lyricData.LyricCount}"
                : NoPosition;

            // The secondary line is one line only: the translation or the third line, never both.
            // Asking for the third line falls back to the translation when there is none.
            ExtraLines.Clear();

            if (_lineLimit < 2 || lines.Count < 2)
                return;

            ExtraLines.Add(_secondaryIsThirdLine && lines.Count > 2 ? lines[2] : lines[1]);
        }
    }
}
