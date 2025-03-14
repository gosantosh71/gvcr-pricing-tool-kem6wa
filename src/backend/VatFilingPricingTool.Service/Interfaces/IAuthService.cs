using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Defines the contract for the authentication service in the VAT Filing Pricing Tool.
    /// This service is responsible for user authentication, registration, password management,
    /// token validation, and Azure AD integration.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="request">Login request containing email and password</param>
        /// <returns>Authentication response with token and user information</returns>
        Task<AuthSuccessResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">Registration request with user details</param>
        /// <returns>Registration response indicating success or failure</returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Initiates the password reset process for a user
        /// </summary>
        /// <param name="request">Password reset request with user email</param>
        /// <returns>Password reset response indicating success or failure</returns>
        Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request);

        /// <summary>
        /// Changes a user's password using a reset token
        /// </summary>
        /// <param name="request">Password change request with token and new password</param>
        /// <returns>Password change response indicating success or failure</returns>
        Task<PasswordChangeResponse> ChangePasswordAsync(PasswordChangeRequest request);

        /// <summary>
        /// Refreshes an authentication token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>Token refresh response with new tokens</returns>
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
        /// <param name="request">Azure AD authentication request with ID token</param>
        /// <returns>Azure AD authentication response with token and user information</returns>
        Task<AzureAdAuthResponse> AuthenticateWithAzureAdAsync(AzureAdAuthRequest request);

        /// <summary>
        /// Retrieves user information from authentication claims
        /// </summary>
        /// <param name="principal">Claims principal containing user claims</param>
        /// <returns>User model with information from claims</returns>
        Task<UserModel> GetUserFromClaimsAsync(ClaimsPrincipal principal);
    }
}