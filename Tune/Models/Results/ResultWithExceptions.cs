namespace Tune.Models.Results
{
    /// <summary>
    /// Represents the result of an operation that may have encountered one or more exceptions.
    /// </summary>
    public class ResultWithExceptions
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded (i.e., no exceptions occurred).
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the list of exceptions that were caught, or an empty list if the operation succeeded.
        /// </summary>
        public IReadOnlyList<Exception> Exceptions { get; }

        /// <summary>
        /// Gets an optional combined error message. If not provided, all exception messages are concatenated.
        /// </summary>
        public string? ErrorMessage { get; }

        private ResultWithExceptions(bool success, IReadOnlyList<Exception>? exceptions, string? errorMessage)
        {
            Success = success;
            Exceptions = exceptions ?? Array.Empty<Exception>();
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result (no exceptions).
        /// </summary>
        public static ResultWithExceptions SuccessResult()
            => new ResultWithExceptions(true, null, null);

        /// <summary>
        /// Creates a failure result from a single exception.
        /// </summary>
        public static ResultWithExceptions FailureResult(Exception ex, string? customMessage = null)
            => FailureResult(new[] { ex }, customMessage);

        /// <summary>
        /// Creates a failure result from multiple exceptions.
        /// </summary>
        public static ResultWithExceptions FailureResult(IEnumerable<Exception> exceptions, string? customMessage = null)
        {
            var list = exceptions?.ToList() ?? new List<Exception>();
            if (list.Count == 0)
                throw new ArgumentException("At least one exception must be provided for a failure result.", nameof(exceptions));

            var msg = customMessage ?? string.Join("; ", list.Select(e => e.Message));
            return new ResultWithExceptions(false, list, msg);
        }

        /// <summary>
        /// Returns a new result that includes the current exceptions plus an additional exception.
        /// The Success flag will remain false.
        /// </summary>
        public ResultWithExceptions AddException(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            var newList = new List<Exception>(Exceptions) { ex };
            // Combine error messages: keep custom message if present, otherwise concatenate
            var newMsg = ErrorMessage != null
                ? ErrorMessage + "; " + ex.Message
                : string.Join("; ", newList.Select(e => e.Message));
            return new ResultWithExceptions(false, newList, newMsg);
        }
    }
}