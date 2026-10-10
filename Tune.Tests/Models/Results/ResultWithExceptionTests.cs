using Tune.Models.Results;
using Xunit;

namespace Tune.Tests.Models.Results
{
    /// <summary>Verifies the behavior of <see cref="ResultWithException"/>.</summary>
    public class ResultWithExceptionTests
    {
        #region ==== Success ====

        /// <summary>Verifies that a success result reports success and carries no exception.</summary>
        [Fact]
        public void SuccessResult_WithNoArguments_ReportsSuccessWithoutException()
        {
            ResultWithException result = ResultWithException.SuccessResult();

            Assert.True(result.Success);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorMessage);
        }

        #endregion

        #region ==== Failure ====

        /// <summary>Verifies that a failure result keeps the exception that caused it.</summary>
        [Fact]
        public void FailureResult_WithException_KeepsTheSameException()
        {
            var exception = new InvalidOperationException("boom");

            ResultWithException result = ResultWithException.FailureResult(exception);

            Assert.False(result.Success);
            Assert.Same(exception, result.Exception);
        }

        /// <summary>Verifies that a failure result falls back to the exception message.</summary>
        [Fact]
        public void FailureResult_WithoutCustomMessage_UsesExceptionMessage()
        {
            ResultWithException result = ResultWithException.FailureResult(new InvalidOperationException("boom"));

            Assert.Equal("boom", result.ErrorMessage);
        }

        /// <summary>Verifies that a custom message overrides the exception message.</summary>
        [Fact]
        public void FailureResult_WithCustomMessage_UsesCustomMessage()
        {
            ResultWithException result = ResultWithException.FailureResult(new InvalidOperationException("boom"), "custom");

            Assert.Equal("custom", result.ErrorMessage);
        }

        /// <summary>Verifies that an empty custom message is kept instead of the exception message.</summary>
        [Fact]
        public void FailureResult_WithEmptyCustomMessage_KeepsEmptyMessage()
        {
            ResultWithException result = ResultWithException.FailureResult(new InvalidOperationException("boom"), string.Empty);

            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        #endregion
    }
}
