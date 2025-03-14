using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Helpers;

namespace VatFilingPricingTool.Web.Handlers
{
    /// <summary>
    /// A delegating handler that automatically adds authentication tokens to outgoing HTTP requests
    /// </summary>
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly LocalStorageHelper localStorage;
        private readonly TokenAuthenticationStateProvider authStateProvider;
        private readonly ILogger<AuthorizationMessageHandler> logger;
        private readonly string[] authorizedUrls;

        /// <summary>
        /// Initializes a new instance of the AuthorizationMessageHandler class with the required dependencies
        /// </summary>
        /// <param name="localStorage">Helper for accessing local storage</param>
        /// <param name="authStateProvider">Provider for authentication state and token refresh</param>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="authorizedUrls">Array of URL prefixes that require authorization</param>
        public AuthorizationMessageHandler(
            LocalStorageHelper localStorage,
            TokenAuthenticationStateProvider authStateProvider,
            ILogger<AuthorizationMessageHandler> logger,
            string[] authorizedUrls)
        {
            this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            this.authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.authorizedUrls = authorizedUrls ?? throw new ArgumentNullException(nameof(authorizedUrls));
        }

        /// <summary>
        /// Intercepts HTTP requests and adds authentication tokens to the Authorization header if required
        /// </summary>
        /// <param name="request">The HTTP request message to send</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
        /// <returns>The HTTP response message from the server</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Check if the request URL matches any of the authorized URLs
            if (request.RequestUri != null && IsAuthorizedEndpoint(request.RequestUri))
            {
                logger.LogDebug("Request to authorized endpoint: {Url}", request.RequestUri);

                // If authorized, attempt to get the authentication token from local storage
                string token = await localStorage.GetAuthTokenAsync();

                // If token is null or empty, try to refresh the token using authStateProvider.RefreshToken()
                if (string.IsNullOrEmpty(token))
                {
                    logger.LogDebug("Token not found, attempting to refresh");
                    
                    bool refreshSuccessful = await authStateProvider.RefreshToken();
                    if (refreshSuccessful)
                    {
                        // If token refresh is successful, get the new token from local storage
                        token = await localStorage.GetAuthTokenAsync();
                        logger.LogDebug("Token refreshed successfully");
                    }
                }

                // If a valid token is available, add it to the Authorization header as a Bearer token
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    
                    // Log the addition of the authorization header
                    logger.LogDebug("Added Authorization header to request");
                }
                else
                {
                    logger.LogWarning("No valid authentication token available for authorized endpoint");
                }
            }

            // Call the base SendAsync method to proceed with the request
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // If the response status code is 401 (Unauthorized), attempt to refresh the token
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && 
                request.RequestUri != null && 
                IsAuthorizedEndpoint(request.RequestUri))
            {
                logger.LogDebug("Received 401 Unauthorized response, attempting to refresh token");

                // If token refresh is successful, retry the request with the new token
                bool refreshSuccessful = await authStateProvider.RefreshToken();
                if (refreshSuccessful)
                {
                    // Get the new token
                    string newToken = await localStorage.GetAuthTokenAsync();
                    
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        // Create a new request as the original request can't be reused
                        var newRequest = new HttpRequestMessage
                        {
                            Method = request.Method,
                            RequestUri = request.RequestUri,
                            Content = request.Content,
                        };

                        // Copy headers (except Authorization which we'll set specifically)
                        foreach (var header in request.Headers)
                        {
                            if (header.Key != "Authorization")
                            {
                                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }
                        }

                        // Add the new token
                        newRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                        
                        // Send the new request
                        logger.LogDebug("Retrying request with new token");
                        response = await base.SendAsync(newRequest, cancellationToken);
                    }
                }
            }

            // Return the HTTP response
            return response;
        }

        /// <summary>
        /// Determines if the request URL requires authorization
        /// </summary>
        /// <param name="requestUri">The URI of the request</param>
        /// <returns>True if the URL requires authorization, otherwise false</returns>
        private bool IsAuthorizedEndpoint(Uri requestUri)
        {
            // If requestUri is null, return false
            if (requestUri == null)
            {
                return false;
            }

            string url = requestUri.ToString().ToLowerInvariant();
            
            // For each authorized URL in authorizedUrls
            foreach (string authorizedUrl in authorizedUrls)
            {
                // Check if the request URL starts with the authorized URL
                if (url.StartsWith(authorizedUrl.ToLowerInvariant()))
                {
                    // If a match is found, return true
                    return true;
                }
            }

            // If no match is found, return false
            return false;
        }
    }
}