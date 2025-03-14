using System;
using System.Collections.Generic;
using System.Linq;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Common.Models
{
    /// <summary>
    /// Represents the outcome of an operation without a return value, indicating success or failure with optional error details.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Gets the error code if the operation failed.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Gets the list of validation errors if applicable.
        /// </summary>
        public List<string> ValidationErrors { get; protected set; }

        /// <summary>
        /// Protected constructor to enforce factory method usage.
        /// </summary>
        protected Result()
        {
            IsSuccess = false;
            ErrorMessage = null;
            ErrorCode = null;
            ValidationErrors = null;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        /// <returns>A successful result.</returns>
        public static Result Success()
        {
            var result = new Result
            {
                IsSuccess = true
            };
            return result;
        }

        /// <summary>
        /// Creates a failure result with an error message and error code.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <param name="errorCode">The error code identifying the type of failure.</param>
        /// <returns>A failure result with the specified error details.</returns>
        public static Result Failure(string errorMessage, string errorCode = null)
        {
            var result = new Result
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode ?? ErrorCodes.General.UnknownError
            };
            return result;
        }

        /// <summary>
        /// Creates a validation failure result with a list of validation errors.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <returns>A validation failure result with the specified validation errors.</returns>
        public static Result ValidationFailure(List<string> validationErrors)
        {
            var result = new Result
            {
                IsSuccess = false,
                ErrorMessage = "Validation failed",
                ErrorCode = ErrorCodes.General.ValidationError,
                ValidationErrors = validationErrors
            };
            return result;
        }

        /// <summary>
        /// Checks if the result has validation errors.
        /// </summary>
        /// <returns>True if the result has validation errors, false otherwise.</returns>
        public bool HasValidationErrors()
        {
            return ValidationErrors != null && ValidationErrors.Any();
        }
    }

    /// <summary>
    /// Represents the outcome of an operation with a return value of type T, indicating success or failure with optional error details and a value on success.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// Gets a value indicating whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; protected set; }

        /// <summary>
        /// Gets the error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; protected set; }

        /// <summary>
        /// Gets the error code if the operation failed.
        /// </summary>
        public string ErrorCode { get; protected set; }

        /// <summary>
        /// Gets the list of validation errors if applicable.
        /// </summary>
        public List<string> ValidationErrors { get; protected set; }

        /// <summary>
        /// Gets the result value if the operation was successful.
        /// </summary>
        public T Value { get; protected set; }

        /// <summary>
        /// Protected constructor to enforce factory method usage.
        /// </summary>
        protected Result()
        {
            IsSuccess = false;
            ErrorMessage = null;
            ErrorCode = null;
            ValidationErrors = null;
            Value = default(T);
        }

        /// <summary>
        /// Creates a successful result with a value.
        /// </summary>
        /// <param name="value">The result value.</param>
        /// <returns>A successful result with the specified value.</returns>
        public static Result<T> Success(T value)
        {
            var result = new Result<T>
            {
                IsSuccess = true,
                Value = value
            };
            return result;
        }

        /// <summary>
        /// Creates a failure result with an error message and error code.
        /// </summary>
        /// <param name="errorMessage">The error message describing the failure.</param>
        /// <param name="errorCode">The error code identifying the type of failure.</param>
        /// <returns>A failure result with the specified error details.</returns>
        public static Result<T> Failure(string errorMessage, string errorCode = null)
        {
            var result = new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode ?? ErrorCodes.General.UnknownError,
                Value = default(T)
            };
            return result;
        }

        /// <summary>
        /// Creates a validation failure result with a list of validation errors.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <returns>A validation failure result with the specified validation errors.</returns>
        public static Result<T> ValidationFailure(List<string> validationErrors)
        {
            var result = new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = "Validation failed",
                ErrorCode = ErrorCodes.General.ValidationError,
                ValidationErrors = validationErrors,
                Value = default(T)
            };
            return result;
        }

        /// <summary>
        /// Checks if the result has validation errors.
        /// </summary>
        /// <returns>True if the result has validation errors, false otherwise.</returns>
        public bool HasValidationErrors()
        {
            return ValidationErrors != null && ValidationErrors.Any();
        }

        /// <summary>
        /// Creates a Result&lt;T&gt; from a Result, either preserving the error or wrapping the success in a typed result.
        /// </summary>
        /// <param name="result">The result to convert from.</param>
        /// <param name="value">The value to use if the result is successful.</param>
        /// <returns>A Result&lt;T&gt; based on the input Result.</returns>
        public static Result<T> From(Result result, T value)
        {
            if (result.IsSuccess)
            {
                return Success(value);
            }

            var typedResult = new Result<T>
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode,
                Value = default(T)
            };

            if (result.HasValidationErrors())
            {
                typedResult.ValidationErrors = result.ValidationErrors;
            }

            return typedResult;
        }
    }

    /// <summary>
    /// Extension methods for working with Result and Result&lt;T&gt; objects.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Converts a value to a successful Result&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to convert.</param>
        /// <returns>A successful Result&lt;T&gt; containing the value.</returns>
        public static Result<T> ToResult<T>(this T value)
        {
            return Result<T>.Success(value);
        }

        /// <summary>
        /// Executes an action if the result is successful.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="action">The action to execute if the result is successful.</param>
        /// <returns>The original result.</returns>
        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }
            return result;
        }

        /// <summary>
        /// Executes an action with the result value if the result is successful.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="action">The action to execute with the result value if the result is successful.</param>
        /// <returns>The original result.</returns>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
            {
                action(result.Value);
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <param name="action">The action to execute with the error message and error code if the result is a failure.</param>
        /// <returns>The original result.</returns>
        public static Result OnFailure(this Result result, Action<string, string> action)
        {
            if (!result.IsSuccess)
            {
                action(result.ErrorMessage, result.ErrorCode);
            }
            return result;
        }

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="result">The result to check.</param>
        /// <param name="action">The action to execute with the error message and error code if the result is a failure.</param>
        /// <returns>The original result.</returns>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<string, string> action)
        {
            if (!result.IsSuccess)
            {
                action(result.ErrorMessage, result.ErrorCode);
            }
            return result;
        }

        /// <summary>
        /// Maps a Result&lt;T&gt; to a Result&lt;U&gt; using a mapping function.
        /// </summary>
        /// <typeparam name="T">The type of the source result value.</typeparam>
        /// <typeparam name="U">The type of the target result value.</typeparam>
        /// <param name="result">The result to map.</param>
        /// <param name="mapper">The mapping function to apply to the result value.</param>
        /// <returns>A new Result&lt;U&gt; with the mapped value or the same error.</returns>
        public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
        {
            if (result.IsSuccess)
            {
                return Result<U>.Success(mapper(result.Value));
            }

            var typedResult = new Result<U>
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            };

            if (result.HasValidationErrors())
            {
                typedResult.ValidationErrors = result.ValidationErrors;
            }

            return typedResult;
        }

        /// <summary>
        /// Binds a Result&lt;T&gt; to a function that returns a Result&lt;U&gt;.
        /// </summary>
        /// <typeparam name="T">The type of the source result value.</typeparam>
        /// <typeparam name="U">The type of the target result value.</typeparam>
        /// <param name="result">The result to bind.</param>
        /// <param name="binder">The binding function to apply to the result value.</param>
        /// <returns>The result of the binder function or the same error.</returns>
        public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> binder)
        {
            if (result.IsSuccess)
            {
                return binder(result.Value);
            }

            var typedResult = new Result<U>
            {
                IsSuccess = false,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            };

            if (result.HasValidationErrors())
            {
                typedResult.ValidationErrors = result.ValidationErrors;
            }

            return typedResult;
        }
    }
}