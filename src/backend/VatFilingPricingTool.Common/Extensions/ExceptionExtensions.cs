using System;
using System.Collections.Generic;
using System.Data.SqlClient; // version 4.8.3
using System.Linq;
using System.Net.Http; // version 6.0.0
using System.Net.Sockets; // version 6.0.0
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Exceptions;

namespace VatFilingPricingTool.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for Exception objects to standardize error handling
    /// throughout the VAT Filing Pricing Tool application.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Converts an Exception to a Result object with appropriate error details.
        /// </summary>
        /// <param name="exception">The exception to convert.</param>
        /// <returns>A Result object representing the exception.</returns>
        public static Result ToResult(this Exception exception)
        {
            if (exception == null)
            {
                return Result.Failure("An unknown error occurred", ErrorCodes.General.UnknownError);
            }

            if (exception is ValidationException validationException)
            {
                return Result.ValidationFailure(validationException.ValidationErrors);
            }

            if (exception is DomainException domainException)
            {
                return Result.Failure(domainException.Message, domainException.ErrorCode);
            }

            return Result.Failure(exception.Message, exception.GetErrorCode());
        }

        /// <summary>
        /// Converts an Exception to a Result&lt;T&gt; object with appropriate error details.
        /// </summary>
        /// <typeparam name="T">The type of the Result value.</typeparam>
        /// <param name="exception">The exception to convert.</param>
        /// <returns>A Result&lt;T&gt; object representing the exception.</returns>
        public static Result<T> ToResult<T>(this Exception exception)
        {
            if (exception == null)
            {
                return Result<T>.Failure("An unknown error occurred", ErrorCodes.General.UnknownError);
            }

            if (exception is ValidationException validationException)
            {
                return Result<T>.ValidationFailure(validationException.ValidationErrors);
            }

            if (exception is DomainException domainException)
            {
                return Result<T>.Failure(domainException.Message, domainException.ErrorCode);
            }

            return Result<T>.Failure(exception.Message, exception.GetErrorCode());
        }

        /// <summary>
        /// Extracts the error code from an exception, with special handling for different exception types.
        /// </summary>
        /// <param name="exception">The exception to extract the error code from.</param>
        /// <returns>The error code associated with the exception.</returns>
        public static string GetErrorCode(this Exception exception)
        {
            if (exception == null)
            {
                return ErrorCodes.General.UnknownError;
            }

            if (exception is DomainException domainException)
            {
                return domainException.ErrorCode;
            }

            if (exception is ValidationException)
            {
                return ErrorCodes.General.ValidationError;
            }

            // Check for SQL exceptions
            if (exception is SqlException sqlException)
            {
                switch (sqlException.Number)
                {
                    case 2627: // Unique constraint error
                    case 547:  // Constraint check violation
                    case 2601: // Duplicated key
                        return ErrorCodes.Data.DuplicateKey;
                    case 547:  // Foreign key violation
                        return ErrorCodes.Data.ForeignKeyViolation;
                    case 4060: // Cannot open database
                    case 18456: // Login failed
                        return ErrorCodes.Data.DatabaseConnectionFailed;
                    case 1205: // Deadlock victim
                        return ErrorCodes.Data.ConcurrencyConflict;
                    default:
                        return ErrorCodes.Data.QueryExecutionFailed;
                }
            }

            // Check for HTTP exceptions
            if (exception is HttpRequestException)
            {
                return ErrorCodes.Integration.ConnectionFailed;
            }

            // Check for other common exception types
            if (exception is TimeoutException)
            {
                return ErrorCodes.General.Timeout;
            }

            if (exception is UnauthorizedAccessException)
            {
                return ErrorCodes.General.Forbidden;
            }

            if (exception is ArgumentException || exception is FormatException)
            {
                return ErrorCodes.General.BadRequest;
            }

            if (exception is KeyNotFoundException || exception is IndexOutOfRangeException)
            {
                return ErrorCodes.General.NotFound;
            }

            return ErrorCodes.General.UnknownError;
        }

        /// <summary>
        /// Generates a detailed error message from an exception, including inner exception details.
        /// </summary>
        /// <param name="exception">The exception to generate a detailed message from.</param>
        /// <returns>A detailed error message.</returns>
        public static string GetDetailedMessage(this Exception exception)
        {
            if (exception == null)
            {
                return "An unknown error occurred.";
            }

            string message = exception.Message;

            // Recursively add inner exception messages
            Exception innerException = exception.InnerException;
            int depth = 0;
            const int maxDepth = 10; // Prevent infinite recursion in rare cases
            
            while (innerException != null && depth < maxDepth)
            {
                message += $" -> {innerException.Message}";
                innerException = innerException.InnerException;
                depth++;
            }

            return message;
        }

        /// <summary>
        /// Determines if an exception represents a transient error that can be retried.
        /// </summary>
        /// <param name="exception">The exception to check.</param>
        /// <returns>True if the exception is transient, false otherwise.</returns>
        public static bool IsTransient(this Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            // Check for SQL transient errors
            if (exception is SqlException sqlException)
            {
                // SQL error codes that are typically transient
                int[] transientSqlErrorCodes = { 
                    4060,  // Cannot open database
                    40197, // The service has encountered an error processing your request
                    40501, // The service is currently busy
                    40613, // Database is not currently available
                    49918, // Not enough resources to process request
                    1205,  // Deadlock victim
                    -2,    // Timeout
                    11001, // No such host is known
                    10928, // Resource ID: %d. The %s limit for the database is %d and has been reached.
                    10929, // Resource ID: %d. The %s minimum guarantee is %d, maximum limit is %d and the current usage for the database is %d. However, the server is currently too busy to support requests greater than %d for this database.
                    10053, // A transport-level error has occurred when receiving results from the server
                    10054, // A transport-level error has occurred when sending the request to the server
                    10060, // A network-related or instance-specific error occurred while establishing a connection to SQL Server
                    233    // The client was unable to establish a connection because of an error during connection initialization
                };
                return transientSqlErrorCodes.Contains(sqlException.Number);
            }

            // Check for HTTP transient errors
            if (exception is HttpRequestException httpException)
            {
                // Look for specific HTTP status codes in the message
                return httpException.Message.Contains("503") || // Service Unavailable
                       httpException.Message.Contains("504") || // Gateway Timeout
                       httpException.Message.Contains("429") || // Too Many Requests
                       httpException.Message.Contains("500") || // Internal Server Error
                       httpException.Message.Contains("502");   // Bad Gateway
            }

            // Check for network transient errors
            if (exception is SocketException socketException)
            {
                // Socket error codes that are typically transient
                int[] transientSocketErrorCodes = { 
                    10053, // Connection aborted
                    10054, // Connection reset
                    10060, // Connection timed out
                    10065, // No route to host
                    11001, // Host not found
                    11002, // Non-authoritative host not found
                    11003, // Non-recoverable error
                    11004  // Valid name, no data record of requested type
                };
                return transientSocketErrorCodes.Contains(socketException.ErrorCode);
            }

            // Check for other common transient exceptions
            return exception is TimeoutException ||
                   (exception is System.IO.IOException && !(exception is System.IO.FileNotFoundException)) ||
                   exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("connection reset", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("server unavailable", StringComparison.OrdinalIgnoreCase) ||
                   exception.Message.Contains("temporarily unavailable", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts validation errors from an exception if it's a ValidationException.
        /// </summary>
        /// <param name="exception">The exception to extract validation errors from.</param>
        /// <returns>Collection of validation error messages.</returns>
        public static IEnumerable<string> GetValidationErrors(this Exception exception)
        {
            if (exception == null)
            {
                return Enumerable.Empty<string>();
            }

            if (exception is ValidationException validationException)
            {
                return validationException.ValidationErrors ?? new List<string> { "Validation failed" };
            }

            // For non-validation exceptions, return a collection with a single error message
            return new[] { exception.Message };
        }
    }
}