using DevBase.Format;
using DevBase.Format.Formats.LrcFormat;
using DevBase.Format.Structure;
using DevBase.Generics;
using System.Diagnostics;
using System.IO;
using Tune.Helpers;

namespace Tune.Models.Lyric
{
    /// <summary>
    /// Lrc lyric class, inherits from LyricBase and represents a lyric in LRC format.
    /// Nullable: true
    /// </summary>
    public class LrcLyric : LyricBase
    {
        public FileParser<LrcParser, AList<TimeStampedLyric>> parser = new FileParser<LrcParser, AList<TimeStampedLyric>>();
        public override AList<TimeStampedLyric> Lyrics { get; set; }
        public override bool isTranslated { get; set; }
        public override AList<TimeStampedLyric> TranslatedLyrics { get; set; } = new AList<TimeStampedLyric>();
        public override List<TimeSpan> TranslatedTimestamps { get; set; } = new List<TimeSpan>();

#pragma warning disable CS8618  // I dont care
        public LrcLyric() { }
#pragma warning restore CS8618

        public LrcLyric(string lyricPath)
        {
            // Parse from file
            string content = File.ReadAllText(lyricPath);
            content = StringUtils.RemoveEmptyLines(content);        //  The DevBase.Format is quite a piece of shit, it will throw an exception if there are empty lines in the lyric file, so we need to remove them first.
            content = StringUtils.FillEmptyLyricText(content);      //  If the lyric file has empty lyric text, the DevBase.Format will also throw an exception.
                                                                    //  So fuck the DevBase.Format, if I fill the empty lyric text with a space, it will not throw an exception, it would fucking replace my " " to a "♪" in the lyric text, 
                                                                    //  I must use a special character (U+200E) to fill the empty lyric text.
            Lyrics = parser.ParseFromString(content);

            //Lyrics.ForEach(line => Debug.WriteLine($"[{line.StartTime.ToString(@"mm\:ss\.ff")}] {line.Text}"));
            GetIsTranslatedAndFlag(Lyrics);

            // The grouping runs whether or not the file is translated, because a plain lyric
            // is simply a list of groups that hold one line each.
            DepartTranslatedLyrics(Lyrics);

            if (isTranslated)
            {
                Debug.WriteLine($"[{nameof(LrcLyric)}] Current LrcLyric is translated.");
                //TranslatedLyrics.ForEach(line => Debug.WriteLine($"[{line.StartTime.ToString(@"mm\:ss\.ff")}] {line.Text}"));
            }
        }
    }
}