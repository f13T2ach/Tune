using System;
using System.IO;
using System.Text;
using Tune.Models.Results;

namespace Tune.Utils.FileUtils
{
    /// <summary>
    /// Provides utility methods for file input/output operations.
    /// </summary>
    internal static class FileIOUtils
    {

        /// <summary>
        /// Writes the specified content to a file, creating a new file or overwriting an existing one.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="content">The content to write.</param>
        /// <param name="encoding">The encoding to use (UTF-8 if not specified).</param>
        /// <returns>A <see cref="ResultWithException"/> indicating success or failure.</returns>
        public static ResultWithException WriteToFile(string filePath, string content, Encoding? encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));
                if (content == null)
                    throw new ArgumentNullException(nameof(content));

                encoding ??= Encoding.UTF8;
                File.WriteAllText(filePath, content, encoding);
                return ResultWithException.SuccessResult();
            }
            catch (Exception ex)
            {
                return ResultWithException.FailureResult(ex);
            }
        }

        /// <summary>
        /// Modifies an existing file by replacing its entire content with the new content.
        /// </summary>
        /// <param name="filePath">The full path to the file.</param>
        /// <param name="newContent">The new content to replace the existing content.</param>
        /// <param name="encoding">The encoding to use (UTF-8 if not specified).</param>
        /// <returns>A <see cref="ResultWithException"/> indicating success or failure.</returns>
        public static ResultWithException ModifyFile(string filePath, string newContent, Encoding? encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));
                if (newContent == null)
                    throw new ArgumentNullException(nameof(newContent));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}", filePath);

                encoding ??= Encoding.UTF8;
                File.WriteAllText(filePath, newContent, encoding);
                return ResultWithException.SuccessResult();
            }
            catch (Exception ex)
            {
                return ResultWithException.FailureResult(ex);
            }
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The full path to the file to delete.</param>
        /// <returns>A <see cref="ResultWithException"/> indicating success or failure.</returns>
        public static ResultWithException DeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}", filePath);

                File.Delete(filePath);
                return ResultWithException.SuccessResult();
            }
            catch (Exception ex)
            {
                return ResultWithException.FailureResult(ex);
            }
        }

        /// <summary>
        /// ReadFile
        /// </summary>
        /// <param name="filePath">The full path to the file to read</param>
        /// <param name="encoding">The encoding to use (UTF-8 if not specified).</param>
        /// <returns>A <see cref="ResultContentWithException<string>"/> indicating succrss or failure and the content of file.</returns>
        public static ResultContentWithException<string> ReadFile(string filePath, Encoding? encoding = null)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentNullException(nameof(filePath));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not found: {filePath}", filePath);

                encoding ??= Encoding.UTF8;
                string content = File.ReadAllText(filePath, encoding);
                return ResultContentWithException<string>.SuccessResult(content);
            }
            catch (Exception ex)
            {
                return ResultContentWithException<string>.FailureResult(ex);
            }
        }
    }
}