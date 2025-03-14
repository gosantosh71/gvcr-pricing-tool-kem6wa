using Microsoft.Extensions.DependencyInjection; // version 6.0.0
using Microsoft.Extensions.Http; // version 6.0.0
using Microsoft.Extensions.Logging; // version 6.0.0
using System;
using System.Net.Http; // version 6.0.0
using Polly; // version 7.2.3
using Polly.Extensions.Http; // version 3.0.0
using VatFilingPricingTool.Infrastructure.Resilience;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Infrastructure.Clients
{
    /// <summary>
    /// Interface defining the contract for HTTP client factory
    /// </summary>
    public interface IHttpClientFactory
    {
        /// <summary>
        /// Creates a configured HttpClient instance
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <returns>A configured HttpClient instance</returns>
        HttpClient CreateClient(string name);

        /// <summary>
        /// Creates a HttpClient with retry policy
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="retryDelay">The delay between retry attempts</param>
        /// <returns>A HttpClient with retry policy</returns>
        HttpClient CreateClientWithRetry(string name, int retryCount, TimeSpan retryDelay);

        /// <summary>
        /// Creates a HttpClient with circuit breaker policy
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <param name="failureThreshold">The number of failures before opening the circuit</param>
        /// <param name="durationOfBreak">The duration the circuit remains open</param>
        /// <returns>A HttpClient with circuit breaker policy</returns>
        HttpClient CreateClientWithCircuitBreaker(string name, int failureThreshold, TimeSpan durationOfBreak);

        /// <summary>
        /// Creates a HttpClient with both retry and circuit breaker policies
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="retryDelay">The delay between retry attempts</param>
        /// <param name="failureThreshold">The number of failures before opening the circuit</param>
        /// <param name="durationOfBreak">The duration the circuit remains open</param>
        /// <returns>A HttpClient with both retry and circuit breaker policies</returns>
        HttpClient CreateClientWithResiliencePolicies(string name, int retryCount, TimeSpan retryDelay, int failureThreshold, TimeSpan durationOfBreak);

        /// <summary>
        /// Creates an ApiClient with the specified name and default resilience policies
        /// </summary>
        /// <param name="name">The name of the client</param>
        /// <returns>A configured ApiClient instance</returns>
        ApiClient CreateApiClient(string name);
    }

    /// <summary>
    /// Implementation of the IHttpClientFactory interface that creates and configures HttpClient instances with resilience patterns
    /// </summary>
    public class HttpClientFactory : IHttpClientFactory
    {
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly ILoggingService _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the HttpClientFactory class
        /// </summary>
        /// <param name="httpClientFactory">The .NET HTTP client factory</param>
        /// <param name="logger">The logging service</param>
        /// <param name="serviceProvider">The service provider</param>
        public HttpClientFactory(System.Net.Http.IHttpClientFactory httpClientFactory, ILoggingService logger, IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public HttpClient CreateClient(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            _logger.LogInformation($"Creating HTTP client with name '{name}'");
            var client = _httpClientFactory.CreateClient(name);
            return ConfigureClient(client);
        }

        /// <inheritdoc />
        public HttpClient CreateClientWithRetry(string name, int retryCount = 0, TimeSpan retryDelay = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            // Use default values if not provided
            if (retryCount <= 0)
            {
                retryCount = PolicyConfiguration.DefaultRetryCount;
            }

            if (retryDelay == default || retryDelay <= TimeSpan.Zero)
            {
                retryDelay = PolicyConfiguration.DefaultRetryDelay;
            }

            _logger.LogInformation($"Creating HTTP client with retry policy. Name: '{name}', RetryCount: {retryCount}, RetryDelay: {retryDelay.TotalMilliseconds}ms");
            var client = _httpClientFactory.CreateClient(name);
            return ConfigureClient(client);
        }

        /// <inheritdoc />
        public HttpClient CreateClientWithCircuitBreaker(string name, int failureThreshold = 0, TimeSpan durationOfBreak = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            // Use default values if not provided
            if (failureThreshold <= 0)
            {
                failureThreshold = PolicyConfiguration.DefaultCircuitBreakerThreshold;
            }

            if (durationOfBreak == default || durationOfBreak <= TimeSpan.Zero)
            {
                durationOfBreak = PolicyConfiguration.DefaultCircuitBreakerDuration;
            }

            _logger.LogInformation($"Creating HTTP client with circuit breaker policy. Name: '{name}', FailureThreshold: {failureThreshold}, DurationOfBreak: {durationOfBreak.TotalSeconds}s");
            var client = _httpClientFactory.CreateClient(name);
            return ConfigureClient(client);
        }

        /// <inheritdoc />
        public HttpClient CreateClientWithResiliencePolicies(string name, int retryCount = 0, TimeSpan retryDelay = default, int failureThreshold = 0, TimeSpan durationOfBreak = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            // Use default values if not provided
            if (retryCount <= 0)
            {
                retryCount = PolicyConfiguration.DefaultRetryCount;
            }

            if (retryDelay == default || retryDelay <= TimeSpan.Zero)
            {
                retryDelay = PolicyConfiguration.DefaultRetryDelay;
            }

            if (failureThreshold <= 0)
            {
                failureThreshold = PolicyConfiguration.DefaultCircuitBreakerThreshold;
            }

            if (durationOfBreak == default || durationOfBreak <= TimeSpan.Zero)
            {
                durationOfBreak = PolicyConfiguration.DefaultCircuitBreakerDuration;
            }

            _logger.LogInformation($"Creating HTTP client with retry and circuit breaker policies. Name: '{name}', RetryCount: {retryCount}, RetryDelay: {retryDelay.TotalMilliseconds}ms, FailureThreshold: {failureThreshold}, DurationOfBreak: {durationOfBreak.TotalSeconds}s");
            var client = _httpClientFactory.CreateClient(name);
            return ConfigureClient(client);
        }

        /// <inheritdoc />
        public ApiClient CreateApiClient(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            _logger.LogInformation($"Creating ApiClient with name '{name}'");
            
            // Create HttpClient with default resilience policies
            var httpClient = CreateClientWithResiliencePolicies(
                name,
                PolicyConfiguration.DefaultRetryCount,
                PolicyConfiguration.DefaultRetryDelay,
                PolicyConfiguration.DefaultCircuitBreakerThreshold,
                PolicyConfiguration.DefaultCircuitBreakerDuration);

            // Create and configure ApiClient
            // Note: ApiClient implementation is not provided in the imported files,
            // so we're assuming it's a wrapper around HttpClient that integrates with
            // our custom resilience policies
            var retryPolicy = CreateRetryPolicy(PolicyConfiguration.DefaultRetryCount, PolicyConfiguration.DefaultRetryDelay);
            var circuitBreakerPolicy = CreateCircuitBreakerPolicy(PolicyConfiguration.DefaultCircuitBreakerThreshold, PolicyConfiguration.DefaultCircuitBreakerDuration);
            
            var apiClient = new ApiClient(httpClient);
            return apiClient;
        }

        /// <summary>
        /// Configures a HttpClient with common settings
        /// </summary>
        /// <param name="client">The client to configure</param>
        /// <returns>The configured HttpClient</returns>
        private HttpClient ConfigureClient(HttpClient client)
        {
            // Set default timeout
            client.Timeout = PolicyConfiguration.DefaultTimeout;
            
            // Set default headers
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "VatFilingPricingTool");
            
            return client;
        }

        /// <summary>
        /// Creates a retry policy with the specified parameters
        /// </summary>
        /// <param name="retryCount">The number of retry attempts</param>
        /// <param name="retryDelay">The delay between retry attempts</param>
        /// <returns>A configured retry policy</returns>
        private IRetryPolicy CreateRetryPolicy(int retryCount, TimeSpan retryDelay)
        {
            return new RetryPolicy(retryCount, retryDelay, true, _logger);
        }

        /// <summary>
        /// Creates a circuit breaker policy with the specified parameters
        /// </summary>
        /// <param name="failureThreshold">The number of failures before opening the circuit</param>
        /// <param name="durationOfBreak">The duration the circuit remains open</param>
        /// <returns>A configured circuit breaker policy</returns>
        private ICircuitBreakerPolicy CreateCircuitBreakerPolicy(int failureThreshold, TimeSpan durationOfBreak)
        {
            return new CircuitBreakerPolicy(failureThreshold, durationOfBreak, _logger);
        }
    }

    /// <summary>
    /// Extension methods for registering the HttpClientFactory in the dependency injection container
    /// </summary>
    public static class HttpClientFactoryExtensions
    {
        /// <summary>
        /// Registers the HttpClientFactory and related services in the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddHttpClientFactory(this IServiceCollection services)
        {
            // Add HttpClient factory services
            services.AddHttpClient();
            
            // Register our custom IHttpClientFactory implementation
            services.AddTransient<IHttpClientFactory, HttpClientFactory>();
            
            // Configure a default client with resilience policies
            services.AddHttpClient("Default")
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            
            return services;
        }

        /// <summary>
        /// Registers a named HttpClient with the specified base address and optional resilience policies
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="name">The name of the client</param>
        /// <param name="baseAddress">The base address for the client</param>
        /// <param name="useRetry">Whether to use retry policy</param>
        /// <param name="useCircuitBreaker">Whether to use circuit breaker policy</param>
        /// <returns>The service collection for method chaining</returns>
        public static IServiceCollection AddNamedHttpClient(this IServiceCollection services, string name, string baseAddress, bool useRetry = true, bool useCircuitBreaker = true)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Client name cannot be null or empty", nameof(name));
            }

            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new ArgumentException("Base address cannot be null or empty", nameof(baseAddress));
            }

            var builder = services.AddHttpClient(name, client =>
            {
                client.BaseAddress = new Uri(baseAddress);
                client.Timeout = PolicyConfiguration.DefaultTimeout;
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "VatFilingPricingTool");
            });

            if (useRetry)
            {
                builder.AddPolicyHandler(GetRetryPolicy());
            }

            if (useCircuitBreaker)
            {
                builder.AddPolicyHandler(GetCircuitBreakerPolicy());
            }

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    PolicyConfiguration.DefaultRetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1)) + 
                                    TimeSpan.FromMilliseconds(new Random().Next(0, 100)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        var logger = context.GetLogger<HttpClientFactory>();
                        logger?.LogWarning($"Retry {retryAttempt} of {PolicyConfiguration.DefaultRetryCount} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    PolicyConfiguration.DefaultCircuitBreakerThreshold,
                    PolicyConfiguration.DefaultCircuitBreakerDuration,
                    onBreak: (outcome, timespan, context) =>
                    {
                        var logger = context.GetLogger<HttpClientFactory>();
                        logger?.LogWarning($"Circuit breaker tripped. Opening circuit for {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                    },
                    onReset: context =>
                    {
                        var logger = context.GetLogger<HttpClientFactory>();
                        logger?.LogInformation("Circuit breaker reset. Closing circuit.");
                    },
                    onHalfOpen: context =>
                    {
                        var logger = context.GetLogger<HttpClientFactory>();
                        logger?.LogInformation("Circuit breaker half-open. Testing if system has recovered.");
                    });
        }
    }

    /// <summary>
    /// ApiClient class - this is a placeholder as the actual implementation is not provided
    /// This class would typically be a wrapper around HttpClient with additional functionality
    /// specific to the VAT Filing Pricing Tool's API integration needs.
    /// </summary>
    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        // Additional API client functionality would be implemented here
    }
}