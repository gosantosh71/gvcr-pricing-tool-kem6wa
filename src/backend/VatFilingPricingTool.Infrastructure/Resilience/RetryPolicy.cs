using Microsoft.Extensions.Logging; // version 6.0.0
using System;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Extensions;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Infrastructure.Resilience
{
    /// <summary>
    /// Interface defining the contract for retry policies
    /// </summary>
    public interface IRetryPolicy
    {
        /// <summary>
        /// Gets the number of retry attempts
        /// </summary>
        int RetryCount { get; }

        /// <summary>
        /// Gets the delay between retry attempts
        /// </summary>
        TimeSpan RetryDelay { get; }

        /// <summary>
        /// Executes an operation with retry logic
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operation">The operation to execute with retry logic</param>
        /// <param name="operationName">The name of the operation for logging purposes</param>
        /// <returns>The result of the operation after retries</returns>
        Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> operation, string operationName);

        /// <summary>
        /// Executes an operation with retry logic that doesn't return a value
        /// </summary>
        /// <param name="operation">The operation to execute with retry logic</param>
        /// <param name="operationName">The name of the operation for logging purposes</param>
        /// <returns>The result of the operation after retries</returns>
        Task<Result> ExecuteAsync(Func<Task<Result>> operation, string operationName);
    }

    /// <summary>
    /// Implementation of the retry pattern for resilient service calls
    /// </summary>
    public class RetryPolicy : IRetryPolicy
    {
        private readonly int _retryCount;
        private readonly TimeSpan _retryDelay;
        private readonly bool _useExponentialBackoff;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with default settings
        /// </summary>
        /// <param name="logger">The logging service for retry events</param>
        public RetryPolicy(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = PolicyConfiguration.DefaultRetryCount;
            _retryDelay = PolicyConfiguration.DefaultRetryDelay;
            _useExponentialBackoff = true;
        }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class with custom settings
        /// </summary>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="retryDelay">The delay between retry attempts</param>
        /// <param name="useExponentialBackoff">Whether to use exponential backoff for retry delay</param>
        /// <param name="logger">The logging service for retry events</param>
        public RetryPolicy(int retryCount, TimeSpan retryDelay, bool useExponentialBackoff, ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = retryCount > 0 ? retryCount : throw new ArgumentOutOfRangeException(nameof(retryCount), "Retry count must be greater than zero");
            _retryDelay = retryDelay > TimeSpan.Zero ? retryDelay : throw new ArgumentOutOfRangeException(nameof(retryDelay), "Retry delay must be greater than zero");
            _useExponentialBackoff = useExponentialBackoff;
        }

        /// <inheritdoc />
        public int RetryCount => _retryCount;

        /// <inheritdoc />
        public TimeSpan RetryDelay => _retryDelay;

        /// <inheritdoc />
        public async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> operation, string operationName)
        {
            int attempt = 0;
            Exception lastException = null;

            do
            {
                try
                {
                    // If this is a retry attempt (not the first attempt), apply delay
                    if (attempt > 0)
                    {
                        TimeSpan delay = CalculateDelay(attempt);
                        _logger.LogWarning($"Retry {attempt} of {_retryCount} for operation '{operationName}', waiting {delay.TotalMilliseconds}ms before next attempt");
                        await Task.Delay(delay);
                    }

                    var result = await operation();
                    
                    if (result.IsSuccess)
                    {
                        if (attempt > 0)
                        {
                            _logger.LogInformation($"Operation '{operationName}' succeeded after {attempt + 1} attempts");
                        }
                        return result;
                    }
                    
                    // For non-exception failures, return the failure immediately (don't retry business logic failures)
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // If this is not a transient exception, don't retry
                    if (!ExceptionExtensions.IsTransient(ex))
                    {
                        _logger.LogError($"Non-transient exception occurred during operation '{operationName}'", ex);
                        return ex.ToResult<T>();
                    }

                    _logger.LogWarning($"Transient exception occurred during attempt {attempt + 1} of operation '{operationName}': {ex.Message}");
                    
                    // Continue to next iteration to retry unless we've exhausted all retries
                    if (attempt >= _retryCount)
                    {
                        break;
                    }
                }

                attempt++;
            } while (attempt <= _retryCount);

            // If we get here, all retries were exhausted
            _logger.LogError($"Operation '{operationName}' failed after {attempt + 1} attempts", lastException);
            return GetRetryFailureResult<T>(operationName, attempt + 1, lastException);
        }

        /// <inheritdoc />
        public async Task<Result> ExecuteAsync(Func<Task<Result>> operation, string operationName)
        {
            int attempt = 0;
            Exception lastException = null;

            do
            {
                try
                {
                    // If this is a retry attempt (not the first attempt), apply delay
                    if (attempt > 0)
                    {
                        TimeSpan delay = CalculateDelay(attempt);
                        _logger.LogWarning($"Retry {attempt} of {_retryCount} for operation '{operationName}', waiting {delay.TotalMilliseconds}ms before next attempt");
                        await Task.Delay(delay);
                    }

                    var result = await operation();
                    
                    if (result.IsSuccess)
                    {
                        if (attempt > 0)
                        {
                            _logger.LogInformation($"Operation '{operationName}' succeeded after {attempt + 1} attempts");
                        }
                        return result;
                    }
                    
                    // For non-exception failures, return the failure immediately (don't retry business logic failures)
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // If this is not a transient exception, don't retry
                    if (!ExceptionExtensions.IsTransient(ex))
                    {
                        _logger.LogError($"Non-transient exception occurred during operation '{operationName}'", ex);
                        return ex.ToResult();
                    }

                    _logger.LogWarning($"Transient exception occurred during attempt {attempt + 1} of operation '{operationName}': {ex.Message}");
                    
                    // Continue to next iteration to retry unless we've exhausted all retries
                    if (attempt >= _retryCount)
                    {
                        break;
                    }
                }

                attempt++;
            } while (attempt <= _retryCount);

            // If we get here, all retries were exhausted
            _logger.LogError($"Operation '{operationName}' failed after {attempt + 1} attempts", lastException);
            return GetRetryFailureResult(operationName, attempt + 1, lastException);
        }

        /// <summary>
        /// Calculates the delay for the next retry attempt, applying exponential backoff if enabled
        /// </summary>
        /// <param name="attempt">The current retry attempt (1-based)</param>
        /// <returns>The calculated delay</returns>
        private TimeSpan CalculateDelay(int attempt)
        {
            if (_useExponentialBackoff)
            {
                // Calculate exponential backoff: baseDelay * 2^(attempt-1)
                // For example, with baseDelay = 2s:
                // attempt 1: 2s * 2^0 = 2s
                // attempt 2: 2s * 2^1 = 4s
                // attempt 3: 2s * 2^2 = 8s
                double exponentialBackoff = Math.Pow(2, attempt - 1);
                TimeSpan calculatedDelay = TimeSpan.FromMilliseconds(_retryDelay.TotalMilliseconds * exponentialBackoff);
                
                // Add a small random jitter to prevent all retried operations from hitting the service at exactly the same time
                // This helps prevent the "thundering herd" problem
                Random jitter = new Random();
                int jitterMilliseconds = jitter.Next(0, 100);
                return calculatedDelay.Add(TimeSpan.FromMilliseconds(jitterMilliseconds));
            }
            
            return _retryDelay;
        }

        /// <summary>
        /// Creates a standardized failure result for when all retries are exhausted
        /// </summary>
        /// <param name="operationName">The name of the operation that failed</param>
        /// <param name="attempts">The number of attempts that were made</param>
        /// <param name="lastException">The last exception that occurred</param>
        /// <returns>A failure result with retry information</returns>
        private Result GetRetryFailureResult(string operationName, int attempts, Exception lastException)
        {
            string errorMessage = PolicyConfiguration.GetRetryErrorMessage(operationName, attempts, lastException);
            string errorCode = PolicyConfiguration.GetRetryErrorCode();
            return Result.Failure(errorMessage, errorCode);
        }

        /// <summary>
        /// Creates a standardized typed failure result for when all retries are exhausted
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operationName">The name of the operation that failed</param>
        /// <param name="attempts">The number of attempts that were made</param>
        /// <param name="lastException">The last exception that occurred</param>
        /// <returns>A typed failure result with retry information</returns>
        private Result<T> GetRetryFailureResult<T>(string operationName, int attempts, Exception lastException)
        {
            string errorMessage = PolicyConfiguration.GetRetryErrorMessage(operationName, attempts, lastException);
            string errorCode = PolicyConfiguration.GetRetryErrorCode();
            return Result<T>.Failure(errorMessage, errorCode);
        }
    }
}