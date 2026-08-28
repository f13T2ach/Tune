using Tune.Values.Static;

namespace Tune.Models.Music
{
    /// <summary>
    /// Music Metadata
    /// Nullable: true
    /// </summary>
    public class MusicMetadata
    {
        public string Title { get; set; } = Strings.NO_CONTENT;
        public string Artist { get; set; } = Strings.NO_CONTENT;
        public string Album { get; set; } = Strings.NO_CONTENT;
        public float SampleRateKHz { get; set; } = 0;         // kHz
        public int BitrateKbps { get; set; }          // kbps
        public int Channels { get; set; }
        public Exception? Exception { get; set; }
    }
}
