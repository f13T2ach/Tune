using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Tune.Models.Results
{
    /// <summary>
    /// Represents the result of an operation that returns content and may have encountered one or more exceptions.
    /// If Success is true, Content is valid and Exceptions is empty.
    /// If Success is false, Exceptions contains at least one exception; Content may be default or a partial result.
    /// </summary>
    /// <typeparam name="T">The type of the content.</typeparam>
    public class ResultContentWithExceptions<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation succeeded (no exceptions).
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the content if available; otherwise, the default value of <typeparamref name="T"/>.
        /// </summary>
        public T? Content { get; }

        /// <summary>
        /// Gets the list of exceptions that were caught, or an empty list if the operation succeeded.
        /// </summary>
        public IReadOnlyList<Exception>? Exceptions { get; }

        /// <summary>
        /// Gets an optional combined error message. If not provided, all exception messages are concatenated.
        /// </summary>
        public string? ErrorMessage { get; }

        private ResultContentWithExceptions(bool success, T? content, IReadOnlyList<Exception>? exceptions, string? errorMessage)
        {
            Success = success;
            Content = content;
            Exceptions = exceptions ?? Array.Empty<Exception>();
            ErrorMessage = errorMessage;
        }

        private ResultContentWithExceptions()
        {
            Success = false;
            Content = default(T);
            Exceptions = null;
            ErrorMessage = null;
        }

        /// <summary>
        /// Creates a successful result with the provided content.
        /// </summary>
        /// <param name="content">The content to return.</param>
        /// <returns>A successful result instance with no exceptions.</returns>
        public static ResultContentWithExceptions<T> SuccessResult(T content)
            => new ResultContentWithExceptions<T>(true, content, null, null);

        /// <summary>
        /// Creates a failure result from a single exception.
        /// </summary>
        /// <param name="ex">The caught exception.</param>
        /// <param name="content">Optional content to include (e.g., partial result).</param>
        /// <param name="customMessage">An optional custom error message.</param>
        /// <returns>A failure result instance.</returns>
        public static ResultContentWithExceptions<T> FailureResult(Exception ex, T content = default!, string? customMessage = null)
            => FailureResult(new[] { ex }, content, customMessage);

        /// <summary>
        /// Creates a failure result from multiple exceptions.
        /// </summary>
        /// <param name="exceptions">The caught exceptions.</param>
        /// <param name="content">Optional content to include (e.g., partial result).</param>
        /// <param name="customMessage">An optional custom error message.</param>
        /// <returns>A failure result instance.</returns>
        public static ResultContentWithExceptions<T> FailureResult(IEnumerable<Exception> exceptions, T content = default!, string? customMessage = null)
        {
            var list = exceptions?.ToList() ?? new List<Exception>();
            if (list.Count == 0)
                throw new ArgumentException("At least one exception must be provided for a failure result.", nameof(exceptions));

            var msg = customMessage ?? string.Join("; ", list.Select(e => e.Message));
            return new ResultContentWithExceptions<T>(false, content, list, msg);
        }

        /// <summary>
        /// Returns a new result that includes the current exceptions plus an additional exception.
        /// The Success flag will remain false.
        /// </summary>
        public ResultContentWithExceptions<T> AddException(Exception ex)
        {
            if (ex == null) throw new ArgumentNullException(nameof(ex));
            if (Success)
                throw new InvalidOperationException("Cannot add an exception to a successful result.");
            var newList = new List<Exception>();
            if (Exceptions == null)
            { newList = new List<Exception>() { ex }; }
            else
            { newList = new List<Exception>(Exceptions) { ex }; }

            var newMsg = ErrorMessage != null
                ? ErrorMessage + "; " + ex.Message
                : string.Join("; ", newList.Select(e => e.Message));
            return new ResultContentWithExceptions<T>(false, Content, newList, newMsg);
        }

        /// <summary>
        /// Deconstructs the result into its components for pattern matching.
        /// </summary>
        public void Deconstruct(out bool success, out T? content, out IReadOnlyList<Exception>? exceptions, out string? errorMessage)
        {
            success = Success;
            content = Content;
            exceptions = Exceptions;
            errorMessage = ErrorMessage;
        }
    }
}