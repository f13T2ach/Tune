namespace Tune.Models.Results
{
    /// <summary>
    /// Represents the result of a file I/O operation.
    /// </summary>
    public class ResultWithException
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the exception that was caught, or null if the operation succeeded.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets an optional error message; if null, the exception's message is used.
        /// </summary>
        public string? ErrorMessage { get; }

        private ResultWithException(bool success, Exception? exception, string? errorMessage)
        {
            Success = success;
            Exception = exception;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        public static ResultWithException SuccessResult() => new ResultWithException(true, null, null);

        /// <summary>
        /// Creates a failure result from an exception.
        /// </summary>
        public static ResultWithException FailureResult(Exception ex, string? customMessage = null)
            => new ResultWithException(false, ex, customMessage ?? ex?.Message);
    }
}
