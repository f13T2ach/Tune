using NAudio.Wave;
using Tune.Helpers;
using Tune.Models.Music;
using Tune.Values.Static;

namespace Tune.Utils.MusicUtils
{
    public static class MetadataGetter
    {
        /// <summary>
        /// Get Metadata from a music file.
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <returns>MusicMetadata object containing metadata and properties</returns>
        public static MusicMetadata GetMetadata(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return new MusicMetadata();

            try
            {
                using var file = TagLib.File.Create(filePath);

                var tag = file.Tag;
                var props = file.Properties;
                int bitrateKbps = 0;
                // fallback to NAudio if TagLib doesn't provide bitrate
                if (props.AudioBitrate == 0)
                {
                    int naudioBitrate = GetBitrateWithNAudio(filePath);
                    if (naudioBitrate > 0)
                        bitrateKbps = naudioBitrate;
                }
                else
                {
                    bitrateKbps = props.AudioBitrate;
                }

                return new MusicMetadata
                {
                    Title = !string.IsNullOrEmpty(tag.Title)
                        ? tag.Title
                        : System.IO.Path.GetFileNameWithoutExtension(filePath),

                    Artist = !string.IsNullOrEmpty(tag.FirstPerformer)
                        ? tag.FirstPerformer
                        : Strings.NO_CONTENT,

                    Album = !string.IsNullOrEmpty(tag.Album)
                        ? tag.Album
                        : Strings.NO_CONTENT,

                    SampleRateKHz = props.AudioSampleRate / 1000f,

                    BitrateKbps = bitrateKbps,

                    Channels = props.AudioChannels,

                    Exception = null
                };
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to get file metadata: '{filePath}'", ex, nameof(GetMetadata));
                {
                    // Return default metadata with error information
                    return new MusicMetadata
                    {
                        Title = System.IO.Path.GetFileNameWithoutExtension(filePath),
                        Artist = "ERR",
                        Album = "ERR",
                        SampleRateKHz = 0,
                        BitrateKbps = 0,
                        Channels = 0,
                        Exception = ex
                    };
                }
            }
        }

        private static int GetBitrateWithNAudio(string filePath)
        {
            try
            {
                using var reader = new AudioFileReader(filePath);
                var format = reader.WaveFormat;
                // Average bytes per second * 8 gives bitrate (bps)
                int bitrateBps = (int)(format.AverageBytesPerSecond * 8);
                // Convert to kbps
                return bitrateBps / 1000;
            }
            catch (Exception ex)
            {
                LoggerHelper.LogError($"Failed to get Bitrate '{filePath}'", ex, nameof(GetBitrateWithNAudio));
                return 0;
            }
        }
    }
}