using Tune.Values.Paths;

namespace Tune.Client.Logger
{
    public static class LoggerInstance
    {
        private static readonly Logger _instance = new Logger(LocalPaths.LOCAL_LOG_N, 5, 100);
        public static Logger Instance => _instance;
    }

}
