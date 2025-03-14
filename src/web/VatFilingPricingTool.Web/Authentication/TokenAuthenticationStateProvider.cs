using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Clients;

namespace VatFilingPricingTool.Web.Authentication
{
    /// <summary>
    /// Concrete implementation of the AuthenticationStateProvider that manages token-based authentication state 
    /// for the VAT Filing Pricing Tool web application.
    /// </summary>
    public class TokenAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger<TokenAuthenticationStateProvider> logger;

        /// <summary>
        /// Initializes a new instance of the TokenAuthenticationStateProvider class with the required dependencies.
        /// </summary>
        /// <param name="localStorage">Helper for accessing local storage.</param>
        /// <param name="httpClient">Client for making HTTP requests.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public TokenAuthenticationStateProvider(
            LocalStorageHelper localStorage,
            IHttpClient httpClient,
            ILogger<TokenAuthenticationStateProvider> logger) : base(localStorage)
        {
            this.httpClient = httpClient;
            this.logger = logger;
            
            // Initialize authentication state by loading token and user data
            _ = InitializeAuthenticationStateAsync();
        }

        /// <summary>
        /// Updates the authentication state to reflect a successful user authentication.
        /// </summary>
        /// <param name="authResponse">The authentication response containing token and user information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task MarkUserAsAuthenticated(AuthSuccessResponse authResponse)
        {
            logger.LogInformation("User authenticated successfully: {Email}", authResponse.User.Email);
            
            // Save auth data to local storage
            await localStorage.SaveAuthDataAsync(authResponse);
            
            // Update current state
            CurrentUser = authResponse.User;
            AuthToken = authResponse.Token;
            TokenExpiration = authResponse.ExpiresAt;
            IsTokenExpired = TokenExpiration <= DateTime.UtcNow;
            
            // Notify UI of authentication state change
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Updates the authentication state to reflect a user logout.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task MarkUserAsLoggedOut()
        {
            logger.LogInformation("User logged out: {Email}", CurrentUser?.Email);
            
            // Clear auth data from local storage
            await localStorage.ClearAuthDataAsync();
            
            // Reset current state
            CurrentUser = null;
            AuthToken = null;
            TokenExpiration = null;
            IsTokenExpired = true;
            
            // Notify UI of authentication state change
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Attempts to refresh the authentication token if it is expired.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing true if the token was refreshed successfully, otherwise false.</returns>
        public async Task<bool> RefreshToken()
        {
            // Check if token is expired
            if (!TokenExpiration.HasValue || TokenExpiration.Value > DateTime.UtcNow)
            {
                // Token is still valid, no need to refresh
                return true;
            }

            logger.LogInformation("Attempting to refresh token");
            
            // Get refresh token
            string refreshToken = await localStorage.GetRefreshTokenAsync();
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                logger.LogWarning("Refresh token not found");
                return false;
            }
            
            try
            {
                // Create refresh token request
                var refreshRequest = new RefreshTokenRequest
                {
                    RefreshToken = refreshToken
                };
                
                // Send refresh token request to the API
                var response = await httpClient.PostAsJsonAsync<RefreshTokenResponse>(
                    AuthEndpoints.RefreshToken, refreshRequest);
                
                if (response != null)
                {
                    // Update authentication state with new token information
                    await UpdateAuthenticationState(response);
                    logger.LogInformation("Token refreshed successfully");
                    return true;
                }
                
                logger.LogWarning("Failed to refresh token - no response from server");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Updates the authentication state with new token information.
        /// </summary>
        /// <param name="response">The refresh token response containing new token information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task UpdateAuthenticationState(RefreshTokenResponse response)
        {
            // Update current state
            AuthToken = response.Token;
            TokenExpiration = response.ExpiresAt;
            IsTokenExpired = TokenExpiration <= DateTime.UtcNow;
            
            // Create an AuthSuccessResponse to use with SaveAuthDataAsync
            var authData = new AuthSuccessResponse
            {
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt,
                User = CurrentUser // Keep the current user data
            };
            
            // Save the updated token information to local storage
            await localStorage.SaveAuthDataAsync(authData);
            
            // Notify UI of authentication state change
            NotifyAuthenticationStateChanged();
        }
    }
}