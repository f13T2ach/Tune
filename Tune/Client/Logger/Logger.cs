using System.IO;
using Tune.Helpers;

namespace Tune.Client.Logger
{
    /// <summary>
    /// A simple file logger that keeps log files for a specified number of days (default 5)
    /// and a maximum number of files (default 50).
    /// Each program run creates a single log file named with the startup timestamp (to the second).
    /// Older files are automatically deleted on the first write operation of each day.
    /// </summary>
    public class Logger
    {
        private readonly string _logDirectory;
        public string LogDirectory => _logDirectory;

        private readonly int _maxDays;
        private readonly int _maxFiles;
        private readonly object _lock = new object();
        private DateTime _lastCleanupDate = DateTime.MinValue;

        // Stores the full file name (including path) used for this run
        private readonly string _logFilePath;
        public string LogFilePath => _logFilePath;

        /// <summary>
        /// Initializes a new logger instance.
        /// </summary>
        /// <param name="logDirectory">
        /// Directory where log files are stored. If null, defaults to "Logs" folder under the application base directory.
        /// </param>
        /// <param name="maxDays">
        /// Number of days to keep. Logs older than this will be deleted. Default is 5.
        /// </param>
        /// <param name="maxFiles">
        /// Maximum number of log files to keep. If exceeded, the oldest files are deleted. Default is 50.
        /// </param>
        public Logger(string logDirectory, int maxDays = 5, int maxFiles = 100)
        {
            _logDirectory = logDirectory;
            _maxDays = maxDays;
            _maxFiles = maxFiles;

            // Ensure the log directory exists
            if (!Directory.Exists(_logDirectory))
                Directory.CreateDirectory(_logDirectory);

            // Generate a filename with the startup timestamp (accurate to seconds)
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            _logFilePath = Path.Combine(_logDirectory, $"log_{timestamp}.txt");
        }

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        public void Info(string message, string source = "Unknown") => WriteLog("INFO", message, source);

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        public void Warning(string message, string source = "Unknown") => WriteLog("WARNING", message, source);

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        public void Error(string message, string source = "Unknown") => WriteLog("ERROR", message, source);

        /// <summary>
        /// Writes a debug message to the log.
        /// </summary>
        public void Debug(string message, string source = "Unknown") => WriteLog("DEBUG", message, source);

        /// <summary>
        /// Core logging method – synchronises writes and triggers cleanup when needed.
        /// </summary>
        private void WriteLog(string level, string message, string from)
        {
            lock (_lock)
            {
                // Perform cleanup only once per day to reduce I/O overhead
                CleanupOldLogs();

                // Build the log entry
                string logEntry = $"[{DateTime.Now:HH:mm:ss}] [{level}->{from}] {message}";

                // Append to the fixed file for this run
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
        }

        /// <summary>
        /// Deletes log files that are older than the configured retention period,
        /// and also ensures the total number of log files does not exceed the configured maximum.
        /// </summary>
        private void CleanupOldLogs()
        {
            // Skip cleanup if already done today
            if (DateTime.Today == _lastCleanupDate)
                return;

            DateTime cutoff = DateTime.Today.AddDays(-_maxDays);
            DateTime today = DateTime.Today;

            // Get all log files matching the naming pattern
            string[] logFiles = Directory.GetFiles(_logDirectory, "log_*.txt");

            // 1. Delete files based on date (older than cutoff or future date)
            foreach (string file in logFiles)
            {
                try
                {
                    DateTime lastWrite = File.GetLastWriteTime(file);
                    if (lastWrite.Date < cutoff || lastWrite.Date > today)
                        File.Delete(file);
                }
                catch (Exception ex)
                {
                    LoggerHelper.LogError($"Failed to clean up by date: '{file}'", ex, nameof(CleanupOldLogs));
                }
            }

            // 2. Enforce file count limit (keep only the most recent _maxFiles)
            // Re-fetch the remaining files after date-based deletion
            string[] remainingFiles = Directory.GetFiles(_logDirectory, "log_*.txt");
            if (remainingFiles.Length > _maxFiles)
            {
                // Sort by last write time (oldest first)
                var sortedFiles = remainingFiles
                    .Select(f => new { FilePath = f, LastWrite = File.GetLastWriteTime(f) })
                    .OrderBy(x => x.LastWrite)
                    .ToList();

                // Determine how many to delete
                int filesToDelete = sortedFiles.Count - _maxFiles;
                var filesToRemove = sortedFiles.Take(filesToDelete);

                foreach (var fileInfo in filesToRemove)
                {
                    try
                    {
                        File.Delete(fileInfo.FilePath);
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.LogError($"Failed to delete old file by count limit: '{fileInfo.FilePath}'", ex, nameof(CleanupOldLogs));
                    }
                }
            }

            // Update the last cleanup date to today
            _lastCleanupDate = DateTime.Today;
        }
    }
}