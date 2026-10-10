using Tune.Models.Results;
using Xunit;

namespace Tune.Tests.Models.Results
{
    /// <summary>Verifies the behavior of <see cref="ResultContentWithExceptions{T}"/>.</summary>
    public class ResultContentWithExceptionsTests
    {
        #region ==== Success ====

        /// <summary>Verifies that a success result exposes the content and no exception.</summary>
        [Fact]
        public void SuccessResult_WithContent_ReportsSuccessWithoutException()
        {
            ResultContentWithExceptions<string> result = ResultContentWithExceptions<string>.SuccessResult("payload");

            Assert.True(result.Success);
            Assert.Equal("payload", result.Content);
            Assert.Empty(result.Exceptions!);
            Assert.Null(result.ErrorMessage);
        }

        #endregion

        #region ==== Failure ====

        /// <summary>Verifies that a single exception is stored as a one-item list.</summary>
        [Fact]
        public void FailureResult_WithSingleException_StoresOneException()
        {
            var exception = new InvalidOperationException("boom");

            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(exception);

            Assert.False(result.Success);
            Assert.Same(exception, Assert.Single(result.Exceptions!));
        }

        /// <summary>Verifies that a failure can still carry partial content.</summary>
        [Fact]
        public void FailureResult_WithContent_KeepsThePartialContent()
        {
            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(new InvalidOperationException("boom"), 7);

            Assert.Equal(7, result.Content);
        }

        /// <summary>Verifies that a custom message overrides the joined exception messages.</summary>
        [Fact]
        public void FailureResult_WithCustomMessage_UsesCustomMessage()
        {
            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(new InvalidOperationException("boom"), 0, "custom");

            Assert.Equal("custom", result.ErrorMessage);
        }

        /// <summary>Verifies that a failure result requires at least one exception.</summary>
        [Fact]
        public void FailureResult_WithNoExceptions_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => ResultContentWithExceptions<int>.FailureResult(Array.Empty<Exception>()));
        }

        #endregion

        #region ==== Accumulation ====

        /// <summary>Verifies that adding an exception extends the list and keeps the result failed.</summary>
        [Fact]
        public void AddException_OnFailureResult_AppendsException()
        {
            var second = new InvalidOperationException("second");

            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(new InvalidOperationException("first")).AddException(second);

            Assert.False(result.Success);
            Assert.Equal(2, result.Exceptions!.Count);
            Assert.Same(second, result.Exceptions[1]);
        }

        /// <summary>Verifies that adding an exception to a success result is rejected.</summary>
        [Fact]
        public void AddException_OnSuccessResult_ThrowsInvalidOperationException()
        {
            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.SuccessResult(1);

            Assert.Throws<InvalidOperationException>(() => result.AddException(new InvalidOperationException("boom")));
        }

        /// <summary>Verifies that adding a null exception is rejected.</summary>
        [Fact]
        public void AddException_WithNull_ThrowsArgumentNullException()
        {
            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(new InvalidOperationException("first"));

            Assert.Throws<ArgumentNullException>(() => result.AddException(null!));
        }

        #endregion

        #region ==== Deconstruction ====

        /// <summary>Verifies that deconstruction exposes all four parts of the result.</summary>
        [Fact]
        public void Deconstruct_OnFailureResult_ExposesAllParts()
        {
            var exception = new InvalidOperationException("boom");
            ResultContentWithExceptions<int> result = ResultContentWithExceptions<int>.FailureResult(exception, 7);

            var (success, content, exceptions, errorMessage) = result;

            Assert.False(success);
            Assert.Equal(7, content);
            Assert.Same(exception, Assert.Single(exceptions!));
            Assert.Equal("boom", errorMessage);
        }

        #endregion
    }
}
