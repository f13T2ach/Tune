using Tune.Persistence.PlaylistStorage;
using Tune.Services;
using Tune.Values.Enums;

namespace Tune.Helpers
{
    public class LoadableServicesHelper
    {
        public static PlaylistService? GetPlaylistService()
        {
            return LoadableServicesManager.GetLoadableInstance(ELoadableServicesTags.PlaylistServiceTag) as PlaylistService;
        }
    }
}
