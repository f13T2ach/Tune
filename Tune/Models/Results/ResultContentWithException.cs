using System;
using System.Collections.Generic;
using System.Text;

namespace Tune.Models.Results
{
    /// <summary>
    /// Represents the result of an operation that returns content or an exception.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    public class ResultContentWithException<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the content if the operation succeeded; otherwise, the default value of <typeparamref name="T"/>.
        /// Should only be accessed when <see cref="Success"/> is true.
        /// </summary>
        public T Content { get; }

        /// <summary>
        /// Gets the exception that was caught, or null if the operation succeeded.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets an optional error message; if null, the exception's message is used.
        /// </summary>
        public string? ErrorMessage { get; }

        private ResultContentWithException(bool success, T content, Exception? exception, string? errorMessage)
        {
            Success = success;
            Content = content;
            Exception = exception;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Creates a successful result with the provided content.
        /// </summary>
        /// <param name="content">The content to return.</param>
        /// <returns>A successful result instance.</returns>
        public static ResultContentWithException<T> SuccessResult(T content)
            => new ResultContentWithException<T>(true, content, null, null);

        /// <summary>
        /// Creates a failure result from an exception.
        /// </summary>
        /// <param name="ex">The caught exception.</param>
        /// <param name="customMessage">An optional custom error message.</param>
        /// <returns>A failure result instance.</returns>
        public static ResultContentWithException<T> FailureResult(Exception ex, string? customMessage = null)
            // Use default(T) to handle both reference and value types safely.
            => new ResultContentWithException<T>(false, default(T)!, ex, customMessage ?? ex?.Message);
    }
}