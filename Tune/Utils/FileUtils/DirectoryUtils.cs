using System.IO;

namespace Tune.Utils.FileUtils
{
    /// <summary>
    /// Provides utility methods for directory operations.
    /// </summary>
    public static class DirectoryUtils
    {
        /// <summary>
        /// Retrieves the full paths of all files located directly within the specified directory.
        /// Subdirectories are not searched.
        /// THIS MIGHT THROW EXECPTIONS!!!
        /// </summary>
        /// <param name="directoryPath">The path of the directory to search for files.</param>
        /// <returns>An array of full file paths. Returns an empty array if the directory contains no files.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="directoryPath"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="directoryPath"/> is empty or contains invalid characters.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown when the specified directory does not exist.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission.</exception>
        /// <exception cref="PathTooLongException">Thrown when the specified path exceeds the system-defined maximum length.</exception>
        public static string[] GetFilesInDirectory(string directoryPath)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));

            // Use Directory.GetFiles with TopDirectoryOnly to exclude subfolders.
            return Directory.GetFiles(directoryPath, "*", SearchOption.TopDirectoryOnly);
        }
    }
}