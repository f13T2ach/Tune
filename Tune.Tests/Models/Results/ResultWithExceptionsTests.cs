using Tune.Models.Results;
using Xunit;

namespace Tune.Tests.Models.Results
{
    /// <summary>Verifies the behavior of <see cref="ResultWithExceptions"/>.</summary>
    public class ResultWithExceptionsTests
    {
        #region ==== Success ====

        /// <summary>Verifies that a success result carries no exception.</summary>
        [Fact]
        public void SuccessResult_WithNoArguments_ReportsSuccessWithEmptyExceptions()
        {
            ResultWithExceptions result = ResultWithExceptions.SuccessResult();

            Assert.True(result.Success);
            Assert.Empty(result.Exceptions);
            Assert.Null(result.ErrorMessage);
        }

        #endregion

        #region ==== Failure ====

        /// <summary>Verifies that a single exception is stored as a one-item list.</summary>
        [Fact]
        public void FailureResult_WithSingleException_StoresOneException()
        {
            var exception = new InvalidOperationException("boom");

            ResultWithExceptions result = ResultWithExceptions.FailureResult(exception);

            Assert.False(result.Success);
            Assert.Same(exception, Assert.Single(result.Exceptions));
        }

        /// <summary>Verifies that several exceptions are stored in the order they were given.</summary>
        [Fact]
        public void FailureResult_WithSeveralExceptions_KeepsTheirOrder()
        {
            var first = new InvalidOperationException("first");
            var second = new InvalidOperationException("second");

            ResultWithExceptions result = ResultWithExceptions.FailureResult(new Exception[] { first, second });

            Assert.Equal(2, result.Exceptions.Count);
            Assert.Same(first, result.Exceptions[0]);
            Assert.Same(second, result.Exceptions[1]);
        }

        /// <summary>Verifies that exception messages are joined when no custom message is given.</summary>
        [Fact]
        public void FailureResult_WithoutCustomMessage_JoinsExceptionMessages()
        {
            ResultWithExceptions result = ResultWithExceptions.FailureResult(new Exception[] { new("first"), new("second") });

            Assert.Equal("first; second", result.ErrorMessage);
        }

        /// <summary>Verifies that a custom message overrides the joined exception messages.</summary>
        [Fact]
        public void FailureResult_WithCustomMessage_UsesCustomMessage()
        {
            ResultWithExceptions result = ResultWithExceptions.FailureResult(new Exception("first"), "custom");

            Assert.Equal("custom", result.ErrorMessage);
        }

        #endregion

        #region ==== Accumulation ====

        /// <summary>Verifies that adding an exception extends the list and keeps the result failed.</summary>
        [Fact]
        public void AddException_OnFailureResult_AppendsException()
        {
            var second = new InvalidOperationException("second");

            ResultWithExceptions result = ResultWithExceptions.FailureResult(new InvalidOperationException("first")).AddException(second);

            Assert.False(result.Success);
            Assert.Equal(2, result.Exceptions.Count);
            Assert.Same(second, result.Exceptions[1]);
        }

        /// <summary>Verifies that adding an exception appends its message to the existing message.</summary>
        [Fact]
        public void AddException_WithExistingMessage_AppendsExceptionMessage()
        {
            ResultWithExceptions result = ResultWithExceptions.FailureResult(new Exception("first")).AddException(new Exception("second"));

            Assert.Equal("first; second", result.ErrorMessage);
        }

        /// <summary>Verifies that adding a null exception is rejected.</summary>
        [Fact]
        public void AddException_WithNull_ThrowsArgumentNullException()
        {
            ResultWithExceptions result = ResultWithExceptions.FailureResult(new Exception("first"));

            Assert.Throws<ArgumentNullException>(() => result.AddException(null!));
        }

        #endregion

        #region ==== Edge Cases ====

        /// <summary>Verifies that a failure result requires at least one exception.</summary>
        [Fact]
        public void FailureResult_WithNoExceptions_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ResultWithExceptions.FailureResult(Array.Empty<Exception>()));
        }

        #endregion
    }
}
