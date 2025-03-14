using Microsoft.Extensions.Logging; // version 6.0.0
using Polly; // version 7.2.3
using Polly.CircuitBreaker;
using Polly.Retry;
using System;
using System.Net.Http; // version 6.0.0
using System.Net.Http.Json; // version 6.0.0
using System.Text.Json; // version 6.0.0
using System.Threading.Tasks; // version 6.0.0
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Common.Extensions;
using VatFilingPricingTool.Common.Models;

namespace VatFilingPricingTool.Infrastructure.Clients
{
    /// <summary>
    /// A reusable HTTP client wrapper with built-in resilience patterns for making API requests to external services.
    /// Provides standardized error handling, retries, circuit breaking, and timeout configuration.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private IAsyncPolicy _retryPolicy;
        private IAsyncPolicy _circuitBreakerPolicy;
        private TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the ApiClient class with the specified HttpClient and logger.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance to use for API requests.</param>
        /// <param name="logger">The logger instance for logging API operations.</param>
        /// <exception cref="ArgumentNullException">Thrown when httpClient or logger is null.</exception>
        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            
            _timeout = TimeSpan.FromSeconds(30);
            _retryPolicy = null;
            _circuitBreakerPolicy = null;
        }

        /// <summary>
        /// Configures the API client with a retry policy for handling transient failures.
        /// </summary>
        /// <param name="retryPolicy">The retry policy to apply.</param>
        /// <returns>The configured ApiClient instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when retryPolicy is null.</exception>
        public ApiClient WithRetryPolicy(IAsyncPolicy retryPolicy)
        {
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            return this;
        }

        /// <summary>
        /// Configures the API client with a circuit breaker policy to prevent cascading failures.
        /// </summary>
        /// <param name="circuitBreakerPolicy">The circuit breaker policy to apply.</param>
        /// <returns>The configured ApiClient instance for method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when circuitBreakerPolicy is null.</exception>
        public ApiClient WithCircuitBreaker(IAsyncPolicy circuitBreakerPolicy)
        {
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
            return this;
        }

        /// <summary>
        /// Configures the API client with a custom timeout.
        /// </summary>
        /// <param name="timeout">The timeout to apply to API requests.</param>
        /// <returns>The configured ApiClient instance for method chaining.</returns>
        /// <exception cref="ArgumentException">Thrown when timeout is less than or equal to zero.</exception>
        public ApiClient WithTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException("Timeout must be greater than zero", nameof(timeout));
            }
            
            _timeout = timeout;
            return this;
        }

        /// <summary>
        /// Sends a GET request to the specified URI and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>A Result containing the deserialized response or error details.</returns>
        public async Task<Result<T>> GetAsync<T>(string requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result<T>.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending GET request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync<T>(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.GetAsync(requestUri, cts.Token);
                    return await ProcessResponseAsync<T>(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during GET request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult<T>();
            }
        }

        /// <summary>
        /// Sends a POST request with the specified content to the URI and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The content to send in the request body.</param>
        /// <returns>A Result containing the deserialized response or error details.</returns>
        public async Task<Result<T>> PostAsync<T>(string requestUri, object content)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result<T>.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync<T>(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.PostAsJsonAsync(requestUri, content, _jsonOptions, cts.Token);
                    return await ProcessResponseAsync<T>(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during POST request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult<T>();
            }
        }

        /// <summary>
        /// Sends a POST request with the specified content to the URI without expecting a typed response.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The content to send in the request body.</param>
        /// <returns>A Result indicating success or failure.</returns>
        public async Task<Result> PostAsync(string requestUri, object content)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending POST request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.PostAsJsonAsync(requestUri, content, _jsonOptions, cts.Token);
                    return await ProcessResponseAsync(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during POST request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult();
            }
        }

        /// <summary>
        /// Sends a PUT request with the specified content to the URI and returns the deserialized response.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The content to send in the request body.</param>
        /// <returns>A Result containing the deserialized response or error details.</returns>
        public async Task<Result<T>> PutAsync<T>(string requestUri, object content)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result<T>.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending PUT request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync<T>(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.PutAsJsonAsync(requestUri, content, _jsonOptions, cts.Token);
                    return await ProcessResponseAsync<T>(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PUT request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult<T>();
            }
        }

        /// <summary>
        /// Sends a PUT request with the specified content to the URI without expecting a typed response.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <param name="content">The content to send in the request body.</param>
        /// <returns>A Result indicating success or failure.</returns>
        public async Task<Result> PutAsync(string requestUri, object content)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending PUT request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.PutAsJsonAsync(requestUri, content, _jsonOptions, cts.Token);
                    return await ProcessResponseAsync(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during PUT request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult();
            }
        }

        /// <summary>
        /// Sends a DELETE request to the specified URI.
        /// </summary>
        /// <param name="requestUri">The URI to send the request to.</param>
        /// <returns>A Result indicating success or failure.</returns>
        public async Task<Result> DeleteAsync(string requestUri)
        {
            if (string.IsNullOrEmpty(requestUri))
            {
                return Result.Failure("Request URI cannot be null or empty", ErrorCodes.Integration.BadRequest);
            }

            try
            {
                _logger.LogInformation("Sending DELETE request to {RequestUri}", requestUri);
                
                return await ExecuteWithResilienceAsync(async () =>
                {
                    using var cts = new System.Threading.CancellationTokenSource(_timeout);
                    var response = await _httpClient.DeleteAsync(requestUri, cts.Token);
                    return await ProcessResponseAsync(response);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during DELETE request to {RequestUri}: {ErrorMessage}", requestUri, ex.Message);
                return ex.ToResult();
            }
        }

        /// <summary>
        /// Executes an API request with configured resilience policies.
        /// </summary>
        /// <typeparam name="T">The type of the result value.</typeparam>
        /// <param name="requestFunc">The function that performs the actual request.</param>
        /// <returns>The result of the request execution with resilience policies applied.</returns>
        private async Task<Result<T>> ExecuteWithResilienceAsync<T>(Func<Task<Result<T>>> requestFunc)
        {
            // Create a combined policy if both retry and circuit breaker are configured
            if (_circuitBreakerPolicy != null && _retryPolicy != null)
            {
                return await Policy.WrapAsync(_circuitBreakerPolicy, _retryPolicy).ExecuteAsync(requestFunc);
            }
            
            // Apply individual policies if only one is configured
            if (_circuitBreakerPolicy != null)
            {
                return await _circuitBreakerPolicy.ExecuteAsync(requestFunc);
            }
            
            if (_retryPolicy != null)
            {
                return await _retryPolicy.ExecuteAsync(requestFunc);
            }
            
            // Execute directly if no policies are configured
            return await requestFunc();
        }

        /// <summary>
        /// Executes an API request with configured resilience policies without expecting a typed response.
        /// </summary>
        /// <param name="requestFunc">The function that performs the actual request.</param>
        /// <returns>The result of the request execution with resilience policies applied.</returns>
        private async Task<Result> ExecuteWithResilienceAsync(Func<Task<Result>> requestFunc)
        {
            // Create a combined policy if both retry and circuit breaker are configured
            if (_circuitBreakerPolicy != null && _retryPolicy != null)
            {
                return await Policy.WrapAsync(_circuitBreakerPolicy, _retryPolicy).ExecuteAsync(requestFunc);
            }
            
            // Apply individual policies if only one is configured
            if (_circuitBreakerPolicy != null)
            {
                return await _circuitBreakerPolicy.ExecuteAsync(requestFunc);
            }
            
            if (_retryPolicy != null)
            {
                return await _retryPolicy.ExecuteAsync(requestFunc);
            }
            
            // Execute directly if no policies are configured
            return await requestFunc();
        }

        /// <summary>
        /// Processes an HTTP response and converts it to a Result&lt;T&gt;.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content to.</typeparam>
        /// <param name="response">The HTTP response to process.</param>
        /// <returns>A Result containing the deserialized response or error details.</returns>
        private async Task<Result<T>> ProcessResponseAsync<T>(HttpResponseMessage response)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var content = await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
                        return Result<T>.Success(content);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize response from {RequestUri} to type {ResponseType}",
                            response.RequestMessage?.RequestUri, typeof(T).Name);
                        return Result<T>.Failure(
                            $"Failed to deserialize response: {ex.Message}",
                            ErrorCodes.Integration.DataImportFailed);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Received non-success status code {StatusCode} from {RequestUri}: {ErrorContent}",
                        (int)response.StatusCode, response.RequestMessage?.RequestUri, errorContent);
                    
                    var errorCode = GetErrorCodeFromStatusCode(response.StatusCode);
                    return Result<T>.Failure(
                        $"API request failed with status code {(int)response.StatusCode}: {errorContent}",
                        errorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing response from {RequestUri}", response.RequestMessage?.RequestUri);
                return ex.ToResult<T>();
            }
        }

        /// <summary>
        /// Processes an HTTP response without expecting a typed response.
        /// </summary>
        /// <param name="response">The HTTP response to process.</param>
        /// <returns>A Result indicating success or failure.</returns>
        private async Task<Result> ProcessResponseAsync(HttpResponseMessage response)
        {
            try
            {
                if (response.IsSuccessStatusCode)
                {
                    return Result.Success();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Received non-success status code {StatusCode} from {RequestUri}: {ErrorContent}",
                        (int)response.StatusCode, response.RequestMessage?.RequestUri, errorContent);
                    
                    var errorCode = GetErrorCodeFromStatusCode(response.StatusCode);
                    return Result.Failure(
                        $"API request failed with status code {(int)response.StatusCode}: {errorContent}",
                        errorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing response from {RequestUri}", response.RequestMessage?.RequestUri);
                return ex.ToResult();
            }
        }

        /// <summary>
        /// Maps an HTTP status code to an appropriate error code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to map.</param>
        /// <returns>The mapped error code.</returns>
        private string GetErrorCodeFromStatusCode(System.Net.HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case System.Net.HttpStatusCode.BadRequest:
                    return ErrorCodes.Integration.BadRequest;
                
                case System.Net.HttpStatusCode.Unauthorized:
                case System.Net.HttpStatusCode.Forbidden:
                    return ErrorCodes.Integration.AuthenticationFailed;
                
                case System.Net.HttpStatusCode.NotFound:
                    return ErrorCodes.Integration.NotFound;
                
                case System.Net.HttpStatusCode.RequestTimeout:
                    return ErrorCodes.Integration.Timeout;
                
                case System.Net.HttpStatusCode.TooManyRequests: // 429
                    return ErrorCodes.Integration.ConnectionFailed;
                
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.BadGateway:
                case System.Net.HttpStatusCode.ServiceUnavailable:
                    return ErrorCodes.Integration.ServiceUnavailable;
                
                case System.Net.HttpStatusCode.GatewayTimeout:
                    return ErrorCodes.Integration.Timeout;
                
                default:
                    return ErrorCodes.Integration.ConnectionFailed;
            }
        }
    }
}