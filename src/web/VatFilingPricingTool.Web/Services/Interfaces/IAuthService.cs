using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Web.Models; // Import from ../../Models/AuthModels.cs

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Interface that defines the contract for authentication services in the VAT Filing Pricing Tool web application
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Gets a value indicating whether the current user is authenticated
        /// </summary>
        bool IsUserAuthenticated { get; }

        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="request">The login request containing email and password</param>
        /// <returns>Authentication response containing token and user information</returns>
        Task<AuthSuccessResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">The registration request containing user details</param>
        /// <returns>Registration response containing user ID and email</returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Initiates the password reset process for a user
        /// </summary>
        /// <param name="request">The password reset request containing the user's email</param>
        /// <returns>Password reset response</returns>
        Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request);

        /// <summary>
        /// Changes a user's password using a reset token
        /// </summary>
        /// <param name="request">The password change request containing the reset token and new password</param>
        /// <returns>Password change response</returns>
        Task<PasswordChangeResponse> ChangePasswordAsync(PasswordChangeRequest request);

        /// <summary>
        /// Refreshes an authentication token using a refresh token
        /// </summary>
        /// <param name="request">The refresh token request</param>
        /// <returns>Token refresh response containing new token information</returns>
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);

        /// <summary>
        /// Validates an authentication token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        Task<bool> ValidateTokenAsync(string token);

        /// <summary>
        /// Authenticates a user with Azure AD credentials
        /// </summary>
        /// <param name="request">The Azure AD authentication request</param>
        /// <returns>Azure AD authentication response</returns>
        Task<AzureAdAuthResponse> AuthenticateWithAzureAdAsync(AzureAdAuthRequest request);

        /// <summary>
        /// Retrieves the current authenticated user's information
        /// </summary>
        /// <returns>Current user information or null if not authenticated</returns>
        Task<UserModel> GetCurrentUserAsync();

        /// <summary>
        /// Logs out the current user and clears authentication state
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task LogoutAsync();
    }
}