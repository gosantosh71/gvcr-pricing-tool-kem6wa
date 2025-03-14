using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Polly; // Polly v7.2.3
using Polly.Extensions.Http; // Polly.Extensions.Http v3.0.0
using Polly.Timeout;
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Handlers;
using VatFilingPricingTool.Web.Helpers;

namespace VatFilingPricingTool.Web.Clients
{
    /// <summary>
    /// Factory class for creating and configuring HttpClient instances with appropriate handlers and settings
    /// for the VAT Filing Pricing Tool web application. This class centralizes HTTP client creation with
    /// proper authentication, resilience patterns, and configuration.
    /// </summary>
    public class HttpClientFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<HttpClientFactory> logger;

        /// <summary>
        /// Initializes a new instance of the HttpClientFactory class with the required dependencies.
        /// </summary>
        /// <param name="serviceProvider">Service provider for resolving dependencies.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public HttpClientFactory(
            IServiceProvider serviceProvider,
            ILogger<HttpClientFactory> logger)
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a standard HttpClient instance without authentication.
        /// </summary>
        /// <returns>A configured HttpClient instance.</returns>
        public HttpClient CreateClient()
        {
            var client = new HttpClient();
            ConfigureDefaultHeaders(client);
            client.Timeout = TimeSpan.FromSeconds(30);
            
            return client;
        }

        /// <summary>
        /// Creates an HttpClient instance with authentication token handling.
        /// </summary>
        /// <returns>A configured HttpClient instance with authentication.</returns>
        public HttpClient CreateAuthenticatedClient()
        {
            // Resolve the AuthorizationMessageHandler from the service provider
            var authHandler = serviceProvider.GetRequiredService<AuthorizationMessageHandler>();
            
            var client = new HttpClient(authHandler);
            ConfigureDefaultHeaders(client);
            client.Timeout = TimeSpan.FromSeconds(30);
            
            return client;
        }

        /// <summary>
        /// Creates an HttpClient instance with resilience policies for handling transient failures.
        /// </summary>
        /// <returns>A configured HttpClient instance with resilience policies.</returns>
        public HttpClient CreateResilientClient()
        {
            // Create a custom handler with policies
            var resilienceHandler = new ResilienceHttpMessageHandler(
                new HttpClientHandler(),
                CreatePolicyWrap(),
                logger);
            
            var client = new HttpClient(resilienceHandler);
            ConfigureDefaultHeaders(client);
            
            return client;
        }

        /// <summary>
        /// Creates an HttpClient instance with both authentication and resilience policies.
        /// </summary>
        /// <returns>A configured HttpClient instance with authentication and resilience policies.</returns>
        public HttpClient CreateResilientAuthenticatedClient()
        {
            // Resolve the AuthorizationMessageHandler from the service provider
            var authHandler = serviceProvider.GetRequiredService<AuthorizationMessageHandler>();
            
            // Create a custom handler with policies
            var resilienceHandler = new ResilienceHttpMessageHandler(
                authHandler,
                CreatePolicyWrap(),
                logger);
            
            var client = new HttpClient(resilienceHandler);
            ConfigureDefaultHeaders(client);
            
            return client;
        }

        /// <summary>
        /// Configures default headers for an HttpClient instance.
        /// </summary>
        /// <param name="client">The HttpClient to configure.</param>
        private void ConfigureDefaultHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "VAT Filing Pricing Tool Web Client");
        }

        /// <summary>
        /// Creates a combined policy wrap with retry, circuit breaker, and timeout policies.
        /// </summary>
        /// <returns>A combined policy</returns>
        private IAsyncPolicy<HttpResponseMessage> CreatePolicyWrap()
        {
            return Policy.WrapAsync(
                CreateRetryPolicy(),
                CreateCircuitBreakerPolicy(),
                CreateTimeoutPolicy());
        }

        /// <summary>
        /// Creates a retry policy for handling transient HTTP failures.
        /// </summary>
        /// <returns>A configured retry policy.</returns>
        private IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    3, // Number of retries
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + // Exponential backoff
                                    TimeSpan.FromMilliseconds(new Random().Next(0, 100)), // Jitter
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogWarning("Retry {RetryAttempt} after {Timespan} due to {Reason}",
                            retryAttempt,
                            timespan,
                            outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                    });
        }

        /// <summary>
        /// Creates a circuit breaker policy to prevent repeated calls to failing services.
        /// </summary>
        /// <returns>A configured circuit breaker policy.</returns>
        private IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    5, // Number of consecutive failures before breaking circuit
                    TimeSpan.FromSeconds(30), // Duration circuit stays open before trying again
                    onBreak: (outcome, timespan) =>
                    {
                        logger.LogWarning("Circuit breaker opened for {Timespan} due to {Reason}",
                            timespan,
                            outcome.Exception?.Message ?? outcome.Result?.ReasonPhrase);
                    },
                    onReset: () =>
                    {
                        logger.LogInformation("Circuit breaker reset");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation("Circuit breaker half-open");
                    });
        }

        /// <summary>
        /// Creates a timeout policy to prevent long-running requests.
        /// </summary>
        /// <returns>A configured timeout policy.</returns>
        private IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(
                10, // Seconds
                TimeoutStrategy.Pessimistic,
                onTimeoutAsync: (context, timespan, task) =>
                {
                    logger.LogWarning("Request timed out after {Timespan}", timespan);
                    return Task.CompletedTask;
                });
        }
    }

    /// <summary>
    /// A delegating handler that applies resilience policies to HTTP requests.
    /// </summary>
    internal class ResilienceHttpMessageHandler : DelegatingHandler
    {
        private readonly IAsyncPolicy<HttpResponseMessage> policy;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the ResilienceHttpMessageHandler class.
        /// </summary>
        /// <param name="innerHandler">The inner handler to delegate to.</param>
        /// <param name="policy">The policy to apply to HTTP requests.</param>
        /// <param name="logger">The logger for diagnostic information.</param>
        public ResilienceHttpMessageHandler(
            HttpMessageHandler innerHandler,
            IAsyncPolicy<HttpResponseMessage> policy,
            ILogger logger)
            : base(innerHandler)
        {
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sends an HTTP request using the resilience policies.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            System.Threading.CancellationToken cancellationToken)
        {
            return policy.ExecuteAsync(() => base.SendAsync(request, cancellationToken));
        }
    }
}