using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Components.Authorization;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the IAuthService interface that provides authentication functionality
    /// for the VAT Filing Pricing Tool web application
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApiClient apiClient;
        private readonly TokenAuthenticationStateProvider authStateProvider;
        private readonly LocalStorageHelper localStorage;
        private readonly ILogger<AuthService> logger;
        private readonly IOptions<AzureAdAuthOptions> azureAdOptions;

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated
        /// </summary>
        public bool IsUserAuthenticated { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AuthService class with required dependencies
        /// </summary>
        /// <param name="apiClient">Client for making HTTP requests to the backend API</param>
        /// <param name="authStateProvider">Provider for authentication state and token refresh</param>
        /// <param name="localStorage">Helper for accessing browser local storage</param>
        /// <param name="logger">Logger for diagnostic information</param>
        /// <param name="azureAdOptions">Configuration options for Azure AD authentication</param>
        public AuthService(
            ApiClient apiClient,
            TokenAuthenticationStateProvider authStateProvider,
            LocalStorageHelper localStorage,
            ILogger<AuthService> logger,
            IOptions<AzureAdAuthOptions> azureAdOptions)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
            this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.azureAdOptions = azureAdOptions ?? throw new ArgumentNullException(nameof(azureAdOptions));

            // Initialize IsUserAuthenticated to false
            IsUserAuthenticated = false;

            // Check if a token exists in local storage to set initial authentication state
            _ = CheckAuthenticationStateAsync();
        }

        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="request">The login request containing email and password</param>
        /// <returns>Authentication response containing token and user information</returns>
        public async Task<AuthSuccessResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                logger.LogInformation("Attempting to authenticate user: {Email}", request.Email);

                // Send login request to the API
                var response = await apiClient.PostAsync<LoginRequest, AuthSuccessResponse>(
                    ApiEndpoints.Auth.Login, request, false);

                if (response != null)
                {
                    // Update authentication state
                    await authStateProvider.MarkUserAsAuthenticated(response);
                    IsUserAuthenticated = true;
                    logger.LogInformation("User authenticated successfully: {Email}", request.Email);
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error authenticating user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">The registration request containing user details</param>
        /// <returns>Registration response containing user ID and email</returns>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                logger.LogInformation("Attempting to register new user: {Email}", request.Email);

                // Send registration request to the API
                var response = await apiClient.PostAsync<RegisterRequest, RegisterResponse>(
                    ApiEndpoints.Auth.Register, request, false);

                logger.LogInformation("User registered successfully: {Email}", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error registering user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Initiates the password reset process for a user
        /// </summary>
        /// <param name="request">The password reset request containing the user's email</param>
        /// <returns>Password reset response</returns>
        public async Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request)
        {
            try
            {
                logger.LogInformation("Requesting password reset for user: {Email}", request.Email);

                // Send password reset request to the API
                var response = await apiClient.PostAsync<PasswordResetRequest, PasswordResetResponse>(
                    ApiEndpoints.Auth.ForgotPassword, request, false);

                logger.LogInformation("Password reset requested successfully for: {Email}", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error requesting password reset for {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Changes a user's password using a reset token
        /// </summary>
        /// <param name="request">The password change request containing the reset token and new password</param>
        /// <returns>Password change response</returns>
        public async Task<PasswordChangeResponse> ChangePasswordAsync(PasswordChangeRequest request)
        {
            try
            {
                logger.LogInformation("Changing password for user: {Email}", request.Email);

                // Send password change request to the API
                var response = await apiClient.PostAsync<PasswordChangeRequest, PasswordChangeResponse>(
                    ApiEndpoints.Auth.ResetPassword, request, false);

                logger.LogInformation("Password changed successfully for: {Email}", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error changing password for {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Refreshes an authentication token using a refresh token
        /// </summary>
        /// <param name="request">The refresh token request</param>
        /// <returns>Token refresh response containing new token information</returns>
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                logger.LogInformation("Attempting to refresh authentication token");

                // Send refresh token request to the API
                var response = await apiClient.PostAsync<RefreshTokenRequest, RefreshTokenResponse>(
                    ApiEndpoints.Auth.RefreshToken, request, false);

                logger.LogInformation("Token refreshed successfully");
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validates an authentication token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                // Send token validation request to the API
                var response = await apiClient.PostAsync(ApiEndpoints.Auth.ValidateToken, true);

                // If the request is successful (no exception thrown), the token is valid
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Authenticates a user with Azure AD credentials
        /// </summary>
        /// <param name="request">The Azure AD authentication request</param>
        /// <returns>Azure AD authentication response</returns>
        public async Task<AzureAdAuthResponse> AuthenticateWithAzureAdAsync(AzureAdAuthRequest request)
        {
            try
            {
                logger.LogInformation("Attempting to authenticate user with Azure AD");

                // Send Azure AD authentication request to the API
                var response = await apiClient.PostAsync<AzureAdAuthRequest, AzureAdAuthResponse>(
                    ApiEndpoints.Auth.AzureAd, request, false);

                if (response != null)
                {
                    // Update authentication state using a synthetic AuthSuccessResponse
                    var authSuccess = new AuthSuccessResponse
                    {
                        Token = response.Token,
                        RefreshToken = response.RefreshToken,
                        ExpiresAt = response.ExpiresAt,
                        User = response.User,
                        Success = true
                    };

                    await authStateProvider.MarkUserAsAuthenticated(authSuccess);
                    IsUserAuthenticated = true;
                    logger.LogInformation("User authenticated successfully with Azure AD: {Email}", response.User?.Email);
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error authenticating user with Azure AD: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's information
        /// </summary>
        /// <returns>Current user information or null if not authenticated</returns>
        public async Task<UserModel> GetCurrentUserAsync()
        {
            // Check if there's a valid authentication token
            string token = await localStorage.GetAuthTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                logger.LogInformation("No authentication token found when retrieving current user");
                IsUserAuthenticated = false;
                return null;
            }

            try
            {
                // Try to get user data from local storage first
                var user = await localStorage.GetUserDataAsync();
                if (user != null)
                {
                    logger.LogDebug("Retrieved user data from local storage: {Email}", user.Email);
                    return user;
                }

                // If not in local storage, get from API
                logger.LogInformation("User data not found in local storage, retrieving from API");
                user = await apiClient.GetAsync<UserModel>(ApiEndpoints.Auth.CurrentUser);
                
                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving current user information: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Logs out the current user and clears authentication state
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task LogoutAsync()
        {
            logger.LogInformation("Logging out user");

            try
            {
                // Call logout endpoint on the API
                await apiClient.PostAsync(ApiEndpoints.Auth.Logout);
            }
            catch (Exception ex)
            {
                // Log the error but continue with client-side logout
                logger.LogWarning(ex, "Error calling logout API: {Message}", ex.Message);
            }

            // Clear authentication state regardless of API call success
            await authStateProvider.MarkUserAsLoggedOut();
            await localStorage.ClearAuthDataAsync();
            IsUserAuthenticated = false;

            logger.LogInformation("User logged out successfully");
        }

        /// <summary>
        /// Checks the current authentication state and refreshes the token if needed
        /// </summary>
        /// <returns>True if the user is authenticated, otherwise false</returns>
        private async Task<bool> CheckAuthenticationStateAsync()
        {
            try
            {
                // Check if there's a valid authentication token
                string token = await localStorage.GetAuthTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    IsUserAuthenticated = false;
                    return false;
                }

                // Attempt to refresh the token if needed
                bool isAuthenticated = await authStateProvider.RefreshToken();
                IsUserAuthenticated = isAuthenticated;

                return isAuthenticated;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking authentication state: {Message}", ex.Message);
                IsUserAuthenticated = false;
                return false;
            }
        }
    }
}