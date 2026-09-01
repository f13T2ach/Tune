namespace Tune.Values.Paths
{
    /// <summary>
    /// If the suffixes is "NL", means it doesn't have "\\" at start.
    /// If is "NR", means it doesn't have "\\" at end.
    /// If is "N", means it doesn't have "\\" at both start & end.
    /// </summary>
    public static class LocalPaths
    {

        public static string LOCAL_PLAYLIST_N = "playlists";

        public static string LOCAL_LOG_N = "logs";

        public static List<string> PATHS_TO_INIT = new List<string>
        {
            LOCAL_PLAYLIST_N, LOCAL_LOG_N
        };
    }
}
