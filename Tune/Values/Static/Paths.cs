using Tune.Values.Enums;

namespace Tune.Values.Static
{
    public static class Paths
    {
        public static string LISTPLAY_ICON = "/Tune;component/Assets/Components/Loop/";

        public static Dictionary<EListPlayStatus, string> LISTPLAY_ICON_PATH_DICT = new Dictionary<EListPlayStatus, string>{
            { EListPlayStatus.NoLoop, LISTPLAY_ICON + "NoLoop.png" },
            { EListPlayStatus.SingleLoop, LISTPLAY_ICON + "Single.png" },
            { EListPlayStatus.ListLoop, LISTPLAY_ICON + "List.png" },
            { EListPlayStatus.RandomListPlay, LISTPLAY_ICON + "Random.png" },
        };
    }
}
