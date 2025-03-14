using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Authentication.WebAssembly.Msal; // v6.0.0
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Authentication
{
    /// <summary>
    /// Implements the IAuthService interface to provide authentication functionality for the VAT Filing Pricing Tool web application.
    /// </summary>
    public class AuthenticationService : IAuthService
    {
        private readonly ApiClient apiClient;
        private readonly TokenAuthenticationStateProvider authStateProvider;
        private readonly LocalStorageHelper localStorage;
        private readonly ILogger<AuthenticationService> logger;
        private readonly AzureAdAuthOptions azureAdOptions;
        private readonly IOptions<AzureAdAuthOptions> azureAdOptionsAccessor;

        /// <summary>
        /// Gets a value indicating whether the current user is authenticated.
        /// </summary>
        public bool IsUserAuthenticated { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AuthenticationService class with the required dependencies.
        /// </summary>
        /// <param name="apiClient">Client for making API requests.</param>
        /// <param name="authStateProvider">Provider for managing authentication state.</param>
        /// <param name="localStorage">Helper for accessing local storage.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        /// <param name="azureAdOptionsAccessor">Options for Azure AD authentication.</param>
        public AuthenticationService(
            ApiClient apiClient,
            TokenAuthenticationStateProvider authStateProvider,
            LocalStorageHelper localStorage,
            ILogger<AuthenticationService> logger,
            IOptions<AzureAdAuthOptions> azureAdOptionsAccessor)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.authStateProvider = authStateProvider ?? throw new ArgumentNullException(nameof(authStateProvider));
            this.localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.azureAdOptionsAccessor = azureAdOptionsAccessor ?? throw new ArgumentNullException(nameof(azureAdOptionsAccessor));
            this.azureAdOptions = azureAdOptionsAccessor.Value;
            
            // Initialize the IsUserAuthenticated property
            IsUserAuthenticated = false;
            
            // Initialize authentication state asynchronously
            _ = InitializeAuthenticationStateAsync();
        }

        /// <summary>
        /// Initializes the authentication state by checking for existing tokens.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task InitializeAuthenticationStateAsync()
        {
            string token = await localStorage.GetAuthTokenAsync();
            IsUserAuthenticated = !string.IsNullOrEmpty(token);
            
            logger.LogInformation("Authentication state initialized. User is authenticated: {IsAuthenticated}", IsUserAuthenticated);
        }

        /// <summary>
        /// Authenticates a user with email and password credentials.
        /// </summary>
        /// <param name="request">The login request containing email and password.</param>
        /// <returns>Authentication response containing token and user information.</returns>
        public async Task<AuthSuccessResponse> LoginAsync(LoginRequest request)
        {
            logger.LogInformation("Login attempt for user: {Email}", request.Email);
            
            try
            {
                var response = await apiClient.PostAsync<LoginRequest, AuthSuccessResponse>(
                    ApiEndpoints.Auth.Login, 
                    request, 
                    false); // No authentication required for login
                
                // Update authentication state
                await authStateProvider.MarkUserAsAuthenticated(response);
                IsUserAuthenticated = true;
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Login failed for user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="request">The registration request containing user details.</param>
        /// <returns>Registration response containing user ID and email.</returns>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            logger.LogInformation("Registration attempt for user: {Email}", request.Email);
            
            try
            {
                return await apiClient.PostAsync<RegisterRequest, RegisterResponse>(
                    ApiEndpoints.Auth.Register, 
                    request, 
                    false); // No authentication required for registration
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Registration failed for user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        /// <param name="request">The password reset request containing the user's email.</param>
        /// <returns>Password reset response.</returns>
        public async Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request)
        {
            logger.LogInformation("Password reset request for user: {Email}", request.Email);
            
            try
            {
                return await apiClient.PostAsync<PasswordResetRequest, PasswordResetResponse>(
                    ApiEndpoints.Auth.ForgotPassword, 
                    request, 
                    false); // No authentication required for password reset
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Password reset request failed for user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Changes a user's password using a reset token.
        /// </summary>
        /// <param name="request">The password change request containing the reset token and new password.</param>
        /// <returns>Password change response.</returns>
        public async Task<PasswordChangeResponse> ChangePasswordAsync(PasswordChangeRequest request)
        {
            logger.LogInformation("Password change request for user: {Email}", request.Email);
            
            try
            {
                return await apiClient.PostAsync<PasswordChangeRequest, PasswordChangeResponse>(
                    ApiEndpoints.Auth.ResetPassword, 
                    request, 
                    false); // No authentication required for password change with token
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Password change failed for user {Email}: {Message}", request.Email, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Refreshes an authentication token using a refresh token.
        /// </summary>
        /// <param name="request">The refresh token request.</param>
        /// <returns>Token refresh response containing new token information.</returns>
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            logger.LogInformation("Token refresh attempt");
            
            try
            {
                return await apiClient.PostAsync<RefreshTokenRequest, RefreshTokenResponse>(
                    ApiEndpoints.Auth.RefreshToken, 
                    request, 
                    false); // No authentication required for token refresh
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token refresh failed: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validates an authentication token.
        /// </summary>
        /// <param name="token">The token to validate.</param>
        /// <returns>True if the token is valid, otherwise false.</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            
            try
            {
                // Call the validate token endpoint
                var response = await apiClient.GetAsync(ApiEndpoints.Auth.ValidateToken);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Token validation failed: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Authenticates a user with Azure AD credentials.
        /// </summary>
        /// <param name="request">The Azure AD authentication request.</param>
        /// <returns>Azure AD authentication response.</returns>
        public async Task<AzureAdAuthResponse> AuthenticateWithAzureAdAsync(AzureAdAuthRequest request)
        {
            logger.LogInformation("Azure AD authentication attempt");
            
            try
            {
                var response = await apiClient.PostAsync<AzureAdAuthRequest, AzureAdAuthResponse>(
                    ApiEndpoints.Auth.AzureAd, 
                    request, 
                    false); // No authentication required for Azure AD authentication
                
                // Update authentication state
                var authResponse = new AuthSuccessResponse
                {
                    Token = response.Token,
                    RefreshToken = response.RefreshToken,
                    ExpiresAt = response.ExpiresAt,
                    User = response.User,
                    Success = true
                };
                
                await authStateProvider.MarkUserAsAuthenticated(authResponse);
                IsUserAuthenticated = true;
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Azure AD authentication failed: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's information.
        /// </summary>
        /// <returns>Current user information or null if not authenticated.</returns>
        public async Task<UserModel> GetCurrentUserAsync()
        {
            if (!IsUserAuthenticated)
            {
                return null;
            }
            
            try
            {
                // First try to get from local storage
                var user = await localStorage.GetUserDataAsync();
                if (user != null)
                {
                    return user;
                }
                
                // If not in local storage, get from API
                return await apiClient.GetAsync<UserModel>(ApiEndpoints.Auth.CurrentUser);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get current user: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Logs out the current user and clears authentication state.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LogoutAsync()
        {
            logger.LogInformation("User logout attempt");
            
            try
            {
                // Try to notify the server about logout
                await apiClient.PostAsync(ApiEndpoints.Auth.Logout);
            }
            catch (Exception ex)
            {
                // Log the error but continue with local logout
                logger.LogWarning(ex, "Server notification of logout failed: {Message}", ex.Message);
            }
            finally
            {
                // Update authentication state even if server logout fails
                await authStateProvider.MarkUserAsLoggedOut();
                IsUserAuthenticated = false;
            }
        }

        /// <summary>
        /// Generates the Azure AD login URL with appropriate parameters.
        /// </summary>
        /// <returns>The Azure AD login URL.</returns>
        public async Task<string> GetAzureAdLoginUrlAsync()
        {
            // Construct the Azure AD login URL
            string url = $"{azureAdOptions.Authority}/oauth2/v2.0/authorize";
            
            // Add required parameters
            url += $"?client_id={azureAdOptions.ClientId}";
            url += "&response_type=code";
            url += $"&redirect_uri={Uri.EscapeDataString(azureAdOptions.RedirectUri)}";
            url += $"&scope={Uri.EscapeDataString(string.Join(" ", azureAdOptions.DefaultScopes))}";
            url += "&response_mode=query";
            
            return await Task.FromResult(url);
        }

        /// <summary>
        /// Handles the callback from Azure AD authentication.
        /// </summary>
        /// <param name="code">The authorization code from Azure AD.</param>
        /// <param name="state">The state parameter from Azure AD.</param>
        /// <returns>Azure AD authentication response.</returns>
        public async Task<AzureAdAuthResponse> HandleAzureAdCallbackAsync(string code, string state)
        {
            logger.LogInformation("Handling Azure AD callback");
            
            // Create the Azure AD auth request
            var request = new AzureAdAuthRequest
            {
                IdToken = code
            };
            
            // Process the Azure AD authentication
            return await AuthenticateWithAzureAdAsync(request);
        }

        /// <summary>
        /// Attempts to refresh the authentication token if it is expired.
        /// </summary>
        /// <returns>True if the token was refreshed successfully, otherwise false.</returns>
        public async Task<bool> TryRefreshTokenAsync()
        {
            bool result = await authStateProvider.RefreshToken();
            IsUserAuthenticated = result;
            return result;
        }
    }
}