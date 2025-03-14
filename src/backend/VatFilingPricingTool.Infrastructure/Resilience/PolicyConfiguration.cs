using Microsoft.Extensions.Logging; // version 6.0.0
using System;
using System.Net.Http; // version 6.0.0
using System.Data.SqlClient; // version 4.8.3
using VatFilingPricingTool.Common.Extensions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Infrastructure.Resilience
{
    /// <summary>
    /// Static class providing configuration settings and utility methods for resilience policies 
    /// including retry and circuit breaker patterns.
    /// </summary>
    public static class PolicyConfiguration
    {
        /// <summary>
        /// Default number of retry attempts for transient failures
        /// </summary>
        public static int DefaultRetryCount { get; private set; }

        /// <summary>
        /// Default delay between retry attempts
        /// </summary>
        public static TimeSpan DefaultRetryDelay { get; private set; }

        /// <summary>
        /// Default threshold of failures before circuit breaker opens
        /// </summary>
        public static int DefaultCircuitBreakerThreshold { get; private set; }

        /// <summary>
        /// Default duration the circuit breaker stays open before moving to half-open state
        /// </summary>
        public static TimeSpan DefaultCircuitBreakerDuration { get; private set; }

        /// <summary>
        /// Default timeout for operations
        /// </summary>
        public static TimeSpan DefaultTimeout { get; private set; }

        /// <summary>
        /// Static constructor to initialize default values
        /// </summary>
        static PolicyConfiguration()
        {
            DefaultRetryCount = 3;
            DefaultRetryDelay = TimeSpan.FromSeconds(2);
            DefaultCircuitBreakerThreshold = 5;
            DefaultCircuitBreakerDuration = TimeSpan.FromMinutes(1);
            DefaultTimeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Determines if an exception represents a transient error that can be retried
        /// </summary>
        /// <param name="exception">The exception to check</param>
        /// <returns>True if the exception is transient, false otherwise</returns>
        public static bool IsTransientException(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }

            return ExceptionExtensions.IsTransient(exception);
        }

        /// <summary>
        /// Generates a standardized error message for retry failures
        /// </summary>
        /// <param name="operation">The name of the operation that failed</param>
        /// <param name="attempts">The number of retry attempts that were made</param>
        /// <param name="lastException">The last exception that occurred</param>
        /// <returns>A formatted error message describing the retry failure</returns>
        public static string GetRetryErrorMessage(string operation, int attempts, Exception lastException)
        {
            return $"Operation '{operation}' failed after {attempts} attempts with error: {lastException?.Message}";
        }

        /// <summary>
        /// Generates a standardized error message for circuit breaker open state
        /// </summary>
        /// <param name="operation">The name of the operation that failed</param>
        /// <returns>A formatted error message describing the circuit breaker state</returns>
        public static string GetCircuitBreakerErrorMessage(string operation)
        {
            return $"Circuit breaker is open for operation '{operation}'. Too many recent failures have occurred.";
        }

        /// <summary>
        /// Generates a standardized error message for operation timeouts
        /// </summary>
        /// <param name="operation">The name of the operation that timed out</param>
        /// <param name="timeout">The timeout duration</param>
        /// <returns>A formatted error message describing the timeout</returns>
        public static string GetTimeoutErrorMessage(string operation, TimeSpan timeout)
        {
            return $"Operation '{operation}' timed out after {timeout.TotalSeconds} seconds.";
        }

        /// <summary>
        /// Gets the appropriate error code for retry failures
        /// </summary>
        /// <returns>The error code for retry failures</returns>
        public static string GetRetryErrorCode()
        {
            return ErrorCodes.Integration.ConnectionFailed;
        }

        /// <summary>
        /// Gets the appropriate error code for circuit breaker failures
        /// </summary>
        /// <returns>The error code for circuit breaker failures</returns>
        public static string GetCircuitBreakerErrorCode()
        {
            return ErrorCodes.Integration.ServiceUnavailable;
        }

        /// <summary>
        /// Gets the appropriate error code for timeout failures
        /// </summary>
        /// <returns>The error code for timeout failures</returns>
        public static string GetTimeoutErrorCode()
        {
            return ErrorCodes.Integration.Timeout;
        }
    }
}