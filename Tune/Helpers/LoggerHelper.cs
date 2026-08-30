using System;
using Tune.Client.Logger;

namespace Tune.Helpers
{
    public static class LoggerHelper
    {
        /// <summary>
        /// Helper method to log errors consistently with full exception details (including stack trace and inner exceptions).
        /// </summary>
        /// <param name="message">A descriptive message.</param>
        /// <param name="exception">The exception (may be null).</param>
        /// <param name="methodName">The calling method name (optional).</param>
        public static void LogError(string message, Exception? exception, string? methodName = "")
        {
            string logMessage;

            if (exception == null)
            {
                logMessage = message;
            }
            else
            {
                // Include full exception details using ToString() which provides stack trace and inner exceptions
                logMessage = $"{message}{Environment.NewLine}Exception Details:{Environment.NewLine}{exception}";
            }

            if (!string.IsNullOrEmpty(methodName))
                LoggerInstance.Instance.Error(logMessage, methodName);
            else
                LoggerInstance.Instance.Error(logMessage);
        }

        /// <summary>
        /// Helper method to log errors consistently with full exception details (including stack trace and inner exceptions).
        /// </summary>
        /// <param name="exception">The exception (may be null).</param>
        /// <param name="methodName">The calling method name (optional).</param>
        public static void LogError(Exception? exception, string? methodName = null)
        {
            string logMessage;

            logMessage = $"{Environment.NewLine}Exception Details:{Environment.NewLine}{exception}";

            if (!string.IsNullOrEmpty(methodName))
                LoggerInstance.Instance.Error(logMessage, methodName);
            else
                LoggerInstance.Instance.Error(logMessage);
        }
    }
}