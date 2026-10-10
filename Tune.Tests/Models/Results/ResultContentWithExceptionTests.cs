using System.IO;

using Tune.Models.Results;
using Xunit;

namespace Tune.Tests.Models.Results
{
    /// <summary>Verifies the behavior of <see cref="ResultContentWithException{T}"/>.</summary>
    public class ResultContentWithExceptionTests
    {
        #region ==== Success ====

        /// <summary>Verifies that a success result exposes the content it was given.</summary>
        [Fact]
        public void SuccessResult_WithContent_ExposesTheSameContent()
        {
            ResultContentWithException<int> result = ResultContentWithException<int>.SuccessResult(42);

            Assert.True(result.Success);
            Assert.Equal(42, result.Content);
            Assert.Null(result.Exception);
            Assert.Null(result.ErrorMessage);
        }

        /// <summary>Verifies that a success result accepts a null reference as content.</summary>
        [Fact]
        public void SuccessResult_WithNullContent_ExposesNull()
        {
            ResultContentWithException<string> result = ResultContentWithException<string>.SuccessResult(null!);

            Assert.True(result.Success);
            Assert.Null(result.Content);
        }

        #endregion

        #region ==== Failure ====

        /// <summary>Verifies that a failure result keeps the exception that caused it.</summary>
        [Fact]
        public void FailureResult_WithException_KeepsTheSameException()
        {
            var exception = new IOException("broken");

            ResultContentWithException<int> result = ResultContentWithException<int>.FailureResult(exception);

            Assert.False(result.Success);
            Assert.Same(exception, result.Exception);
        }

        /// <summary>Verifies that a value type content falls back to its default on failure.</summary>
        [Fact]
        public void FailureResult_WithValueTypeContent_FallsBackToDefault()
        {
            ResultContentWithException<int> result = ResultContentWithException<int>.FailureResult(new IOException("broken"));

            Assert.Equal(0, result.Content);
        }

        /// <summary>Verifies that a reference type content falls back to null on failure.</summary>
        [Fact]
        public void FailureResult_WithReferenceTypeContent_FallsBackToNull()
        {
            ResultContentWithException<string> result = ResultContentWithException<string>.FailureResult(new IOException("broken"));

            Assert.Null(result.Content);
        }

        /// <summary>Verifies that a failure result falls back to the exception message.</summary>
        [Fact]
        public void FailureResult_WithoutCustomMessage_UsesExceptionMessage()
        {
            ResultContentWithException<int> result = ResultContentWithException<int>.FailureResult(new IOException("broken"));

            Assert.Equal("broken", result.ErrorMessage);
        }

        /// <summary>Verifies that a custom message overrides the exception message.</summary>
        [Fact]
        public void FailureResult_WithCustomMessage_UsesCustomMessage()
        {
            ResultContentWithException<int> result = ResultContentWithException<int>.FailureResult(new IOException("broken"), "custom");

            Assert.Equal("custom", result.ErrorMessage);
        }

        #endregion
    }
}
