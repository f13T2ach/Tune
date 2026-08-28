using System;
using System.IO;
using Tune.Values.Paths;

namespace Tune.Utils.FileUtils
{
    public static class EnvironmentUtils
    {
        /// <summary>
        /// Gets the current working directory of the application.
        /// </summary>
        /// <returns>A string containing the path of the current directory.</returns>
        public static string GetEnvironmentDir()
        {
            return Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// Init Environment:
        ///     Create work dirs
        /// </summary>
        public static void InitEnvironment()
        {
            foreach (string path in LocalPaths.PATHS_TO_INIT)
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
        }
    }
}