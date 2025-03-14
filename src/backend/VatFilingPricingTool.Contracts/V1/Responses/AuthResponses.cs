using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using VatFilingPricingTool.Contracts.V1.Models;

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for successful authentication containing JWT token and user information
    /// </summary>
    public class AuthSuccessResponse
    {
        /// <summary>
        /// JWT token for API authentication
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Refresh token for obtaining a new JWT token when the current one expires
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration date and time of the JWT token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information associated with the authentication
        /// </summary>
        public UserModel User { get; set; }
    }

    /// <summary>
    /// Response model for failed authentication containing error details
    /// </summary>
    public class AuthFailureResponse
    {
        /// <summary>
        /// Indicates whether the authentication was successful (always false for failure response)
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// General error message describing the authentication failure
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// List of specific error messages providing details about the authentication failure
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Default constructor for AuthFailureResponse
        /// Initializes Success to false and creates an empty Errors list
        /// </summary>
        public AuthFailureResponse()
        {
            Success = false;
            Errors = new List<string>();
        }
    }

    /// <summary>
    /// Response model for user registration containing registration status and user details
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Indicates whether the registration was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Unique identifier of the registered user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the registered user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for RegisterResponse
        /// Initializes Success to true
        /// </summary>
        public RegisterResponse()
        {
            Success = true;
        }
    }

    /// <summary>
    /// Response model for password reset request containing status and email
    /// </summary>
    public class PasswordResetResponse
    {
        /// <summary>
        /// Indicates whether the password reset request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Email address associated with the password reset request
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for PasswordResetResponse
        /// Initializes Success to true
        /// </summary>
        public PasswordResetResponse()
        {
            Success = true;
        }
    }

    /// <summary>
    /// Response model for password change operation containing status and email
    /// </summary>
    public class PasswordChangeResponse
    {
        /// <summary>
        /// Indicates whether the password change was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Email address associated with the password change
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for PasswordChangeResponse
        /// Initializes Success to true
        /// </summary>
        public PasswordChangeResponse()
        {
            Success = true;
        }
    }

    /// <summary>
    /// Response model for token refresh operation containing new token information
    /// </summary>
    public class RefreshTokenResponse
    {
        /// <summary>
        /// New JWT token issued after refresh
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// New refresh token for subsequent refresh operations
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration date and time of the new JWT token
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Response model for Azure AD authentication containing token information and user details
    /// </summary>
    public class AzureAdAuthResponse
    {
        /// <summary>
        /// JWT token for API authentication
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Refresh token for obtaining a new JWT token when the current one expires
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration date and time of the JWT token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information associated with the authentication
        /// </summary>
        public UserModel User { get; set; }

        /// <summary>
        /// Indicates whether this is the first time the user has authenticated via Azure AD
        /// </summary>
        public bool IsNewUser { get; set; }

        /// <summary>
        /// Default constructor for AzureAdAuthResponse
        /// Initializes IsNewUser to false
        /// </summary>
        public AzureAdAuthResponse()
        {
            IsNewUser = false;
        }
    }

    /// <summary>
    /// Response model for token validation operation containing validation status
    /// </summary>
    public class TokenValidationResponse
    {
        /// <summary>
        /// Indicates whether the token is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// User ID associated with the token if valid
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Expiration date and time of the token if valid
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Default constructor for TokenValidationResponse
        /// Initializes IsValid to false
        /// </summary>
        public TokenValidationResponse()
        {
            IsValid = false;
        }
    }
}