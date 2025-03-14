using Microsoft.Extensions.Logging; // version 6.0.0
using System;
using System.Threading;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Extensions;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Infrastructure.Resilience
{
    /// <summary>
    /// Enumeration representing the possible states of a circuit breaker
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// Circuit is closed and operations are allowed to proceed normally
        /// </summary>
        Closed,
        
        /// <summary>
        /// Circuit is open and operations are not allowed to proceed
        /// </summary>
        Open,
        
        /// <summary>
        /// Circuit is in a testing state, allowing a limited number of operations to test if the system has recovered
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// Interface defining the contract for circuit breaker policies
    /// </summary>
    public interface ICircuitBreakerPolicy
    {
        /// <summary>
        /// Gets the number of failures required to trip the circuit breaker
        /// </summary>
        int FailureThreshold { get; }
        
        /// <summary>
        /// Gets the duration the circuit stays open before transitioning to half-open
        /// </summary>
        TimeSpan ResetTimeout { get; }
        
        /// <summary>
        /// Gets the current state of the circuit breaker
        /// </summary>
        CircuitState CurrentState { get; }
        
        /// <summary>
        /// Executes an operation with circuit breaker protection
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging and error reporting</param>
        /// <returns>The result of the operation if circuit is closed, or a failure result if circuit is open</returns>
        Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> operation, string operationName);
        
        /// <summary>
        /// Executes an operation with circuit breaker protection that doesn't return a value
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging and error reporting</param>
        /// <returns>The result of the operation if circuit is closed, or a failure result if circuit is open</returns>
        Task<Result> ExecuteAsync(Func<Task<Result>> operation, string operationName);
        
        /// <summary>
        /// Manually resets the circuit breaker to closed state
        /// </summary>
        void Reset();
        
        /// <summary>
        /// Manually trips the circuit breaker to open state
        /// </summary>
        void Trip();
    }

    /// <summary>
    /// Implementation of the circuit breaker pattern for resilient service calls
    /// </summary>
    public class CircuitBreakerPolicy : ICircuitBreakerPolicy
    {
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private CircuitState _state;
        private int _failureCount;
        private DateTime? _lastFailureTime;
        private DateTime? _openTime;
        private readonly object _stateChangeLock = new object();
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the CircuitBreakerPolicy class with default settings
        /// </summary>
        /// <param name="logger">The logging service</param>
        public CircuitBreakerPolicy(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _failureThreshold = PolicyConfiguration.DefaultCircuitBreakerThreshold;
            _resetTimeout = PolicyConfiguration.DefaultCircuitBreakerDuration;
            _state = CircuitState.Closed;
            _failureCount = 0;
            _lastFailureTime = null;
            _openTime = null;
        }

        /// <summary>
        /// Initializes a new instance of the CircuitBreakerPolicy class with custom settings
        /// </summary>
        /// <param name="failureThreshold">The number of failures required to trip the circuit breaker</param>
        /// <param name="resetTimeout">The duration the circuit stays open before transitioning to half-open</param>
        /// <param name="logger">The logging service</param>
        public CircuitBreakerPolicy(int failureThreshold, TimeSpan resetTimeout, ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _failureThreshold = failureThreshold > 0 ? failureThreshold : throw new ArgumentOutOfRangeException(nameof(failureThreshold), "Failure threshold must be greater than zero");
            _resetTimeout = resetTimeout > TimeSpan.Zero ? resetTimeout : throw new ArgumentOutOfRangeException(nameof(resetTimeout), "Reset timeout must be greater than zero");
            _state = CircuitState.Closed;
            _failureCount = 0;
            _lastFailureTime = null;
            _openTime = null;
        }

        /// <summary>
        /// Gets the number of failures required to trip the circuit breaker
        /// </summary>
        public int FailureThreshold => _failureThreshold;

        /// <summary>
        /// Gets the duration the circuit stays open before transitioning to half-open
        /// </summary>
        public TimeSpan ResetTimeout => _resetTimeout;

        /// <summary>
        /// Gets the current state of the circuit breaker
        /// </summary>
        public CircuitState CurrentState => _state;

        /// <summary>
        /// Executes an operation with circuit breaker protection
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging and error reporting</param>
        /// <returns>The result of the operation if circuit is closed, or a failure result if circuit is open</returns>
        public async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> operation, string operationName)
        {
            if (string.IsNullOrEmpty(operationName))
            {
                operationName = "UnnamedOperation";
            }

            // Check if circuit is open
            if (IsCircuitOpen())
            {
                // Try to transition to half-open if reset timeout has elapsed
                if (!TryTransitionHalfOpen())
                {
                    _logger.LogWarning($"Circuit breaker is open for operation '{operationName}'. Request is blocked.");
                    return GetCircuitBreakerFailureResult<T>(operationName);
                }
            }

            try
            {
                // Execute the operation
                var result = await operation();

                // If operation was successful, handle success
                if (result.IsSuccess)
                {
                    OnSuccessfulOperation();
                }
                else
                {
                    // If operation returned failure, increment failure count
                    OnFailedOperation(operationName, new Exception(result.ErrorMessage));
                }

                return result;
            }
            catch (Exception ex)
            {
                // If operation threw an exception, increment failure count
                OnFailedOperation(operationName, ex);
                return ex.ToResult<T>();
            }
        }

        /// <summary>
        /// Executes an operation with circuit breaker protection that doesn't return a value
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">Name of the operation for logging and error reporting</param>
        /// <returns>The result of the operation if circuit is closed, or a failure result if circuit is open</returns>
        public async Task<Result> ExecuteAsync(Func<Task<Result>> operation, string operationName)
        {
            if (string.IsNullOrEmpty(operationName))
            {
                operationName = "UnnamedOperation";
            }

            // Check if circuit is open
            if (IsCircuitOpen())
            {
                // Try to transition to half-open if reset timeout has elapsed
                if (!TryTransitionHalfOpen())
                {
                    _logger.LogWarning($"Circuit breaker is open for operation '{operationName}'. Request is blocked.");
                    return GetCircuitBreakerFailureResult(operationName);
                }
            }

            try
            {
                // Execute the operation
                var result = await operation();

                // If operation was successful, handle success
                if (result.IsSuccess)
                {
                    OnSuccessfulOperation();
                }
                else
                {
                    // If operation returned failure, increment failure count
                    OnFailedOperation(operationName, new Exception(result.ErrorMessage));
                }

                return result;
            }
            catch (Exception ex)
            {
                // If operation threw an exception, increment failure count
                OnFailedOperation(operationName, ex);
                return ex.ToResult();
            }
        }

        /// <summary>
        /// Manually resets the circuit breaker to closed state
        /// </summary>
        public void Reset()
        {
            lock (_stateChangeLock)
            {
                _failureCount = 0;
                _state = CircuitState.Closed;
                _lastFailureTime = null;
                _openTime = null;
                _logger.LogInformation("Circuit breaker manually reset to Closed state.");
            }
        }

        /// <summary>
        /// Manually trips the circuit breaker to open state
        /// </summary>
        public void Trip()
        {
            lock (_stateChangeLock)
            {
                _state = CircuitState.Open;
                _openTime = DateTime.UtcNow;
                _logger.LogWarning("Circuit breaker manually tripped to Open state.");
            }
        }

        /// <summary>
        /// Handles successful operation by resetting failure count and potentially closing the circuit
        /// </summary>
        private void OnSuccessfulOperation()
        {
            lock (_stateChangeLock)
            {
                _failureCount = 0;

                // If state is half-open and operation succeeded, transition to closed
                if (_state == CircuitState.HalfOpen)
                {
                    _state = CircuitState.Closed;
                    _logger.LogInformation("Circuit breaker state changed from HalfOpen to Closed after successful operation.");
                }
            }
        }

        /// <summary>
        /// Handles failed operation by incrementing failure count and potentially opening the circuit
        /// </summary>
        /// <param name="operationName">Name of the operation that failed</param>
        /// <param name="exception">The exception that caused the failure</param>
        private void OnFailedOperation(string operationName, Exception exception)
        {
            lock (_stateChangeLock)
            {
                _failureCount++;
                _lastFailureTime = DateTime.UtcNow;

                _logger.LogError($"Operation '{operationName}' failed. Failure count: {_failureCount}/{_failureThreshold}. Error: {exception.Message}", exception);

                // If failure count exceeds threshold, trip the circuit
                if (_failureCount >= _failureThreshold)
                {
                    _state = CircuitState.Open;
                    _openTime = DateTime.UtcNow;
                    _logger.LogWarning($"Circuit breaker tripped to Open state after {_failureCount} failures. Last error: {exception.Message}");
                }
            }
        }

        /// <summary>
        /// Attempts to transition the circuit from open to half-open state if the reset timeout has elapsed
        /// </summary>
        /// <returns>True if transition to half-open occurred, false otherwise</returns>
        private bool TryTransitionHalfOpen()
        {
            lock (_stateChangeLock)
            {
                // Only attempt transition if state is Open and reset timeout has elapsed
                if (_state == CircuitState.Open && _openTime.HasValue && DateTime.UtcNow - _openTime.Value >= _resetTimeout)
                {
                    _state = CircuitState.HalfOpen;
                    _logger.LogInformation($"Circuit breaker state changed from Open to HalfOpen after {_resetTimeout.TotalSeconds} seconds.");
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Checks if the circuit is currently open, preventing operations
        /// </summary>
        /// <returns>True if the circuit is open, false otherwise</returns>
        private bool IsCircuitOpen()
        {
            return _state == CircuitState.Open;
        }

        /// <summary>
        /// Creates a standardized failure result for when the circuit is open
        /// </summary>
        /// <param name="operationName">The name of the operation that was blocked</param>
        /// <returns>A failure result with circuit breaker information</returns>
        private Result GetCircuitBreakerFailureResult(string operationName)
        {
            string errorMessage = PolicyConfiguration.GetCircuitBreakerErrorMessage(operationName);
            string errorCode = PolicyConfiguration.GetCircuitBreakerErrorCode();
            return Result.Failure(errorMessage, errorCode);
        }

        /// <summary>
        /// Creates a standardized typed failure result for when the circuit is open
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operationName">The name of the operation that was blocked</param>
        /// <returns>A typed failure result with circuit breaker information</returns>
        private Result<T> GetCircuitBreakerFailureResult<T>(string operationName)
        {
            string errorMessage = PolicyConfiguration.GetCircuitBreakerErrorMessage(operationName);
            string errorCode = PolicyConfiguration.GetCircuitBreakerErrorCode();
            return Result<T>.Failure(errorMessage, errorCode);
        }
    }
}