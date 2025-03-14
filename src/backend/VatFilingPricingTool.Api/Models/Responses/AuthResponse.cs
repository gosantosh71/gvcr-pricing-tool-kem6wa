using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Contracts.V1.Models;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// API response model for successful authentication containing JWT token and user information
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

        /// <summary>
        /// Creates an API AuthSuccessResponse from a contract AuthSuccessResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API AuthSuccessResponse with mapped properties</returns>
        public static AuthSuccessResponse FromContractResponse(Contracts.V1.Responses.AuthSuccessResponse response)
        {
            if (response == null)
                return null;

            return new AuthSuccessResponse
            {
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User
            };
        }
    }

    /// <summary>
    /// API response model for failed authentication containing error details
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
    /// API response model for user registration containing registration status and user details
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

        /// <summary>
        /// Creates an API RegisterResponse from a contract RegisterResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API RegisterResponse with mapped properties</returns>
        public static RegisterResponse FromContractResponse(Contracts.V1.Responses.RegisterResponse response)
        {
            if (response == null)
                return null;

            return new RegisterResponse
            {
                Success = response.Success,
                UserId = response.UserId,
                Email = response.Email
            };
        }
    }

    /// <summary>
    /// API response model for password reset request containing status and email
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

        /// <summary>
        /// Creates an API PasswordResetResponse from a contract PasswordResetResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API PasswordResetResponse with mapped properties</returns>
        public static PasswordResetResponse FromContractResponse(Contracts.V1.Responses.PasswordResetResponse response)
        {
            if (response == null)
                return null;

            return new PasswordResetResponse
            {
                Success = response.Success,
                Email = response.Email
            };
        }
    }

    /// <summary>
    /// API response model for password change operation containing status and email
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

        /// <summary>
        /// Creates an API PasswordChangeResponse from a contract PasswordChangeResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API PasswordChangeResponse with mapped properties</returns>
        public static PasswordChangeResponse FromContractResponse(Contracts.V1.Responses.PasswordChangeResponse response)
        {
            if (response == null)
                return null;

            return new PasswordChangeResponse
            {
                Success = response.Success,
                Email = response.Email
            };
        }
    }

    /// <summary>
    /// API response model for token refresh operation containing new token information
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

        /// <summary>
        /// Creates an API RefreshTokenResponse from a contract RefreshTokenResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API RefreshTokenResponse with mapped properties</returns>
        public static RefreshTokenResponse FromContractResponse(Contracts.V1.Responses.RefreshTokenResponse response)
        {
            if (response == null)
                return null;

            return new RefreshTokenResponse
            {
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt
            };
        }
    }

    /// <summary>
    /// API response model for Azure AD authentication containing token information and user details
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

        /// <summary>
        /// Creates an API AzureAdAuthResponse from a contract AzureAdAuthResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API AzureAdAuthResponse with mapped properties</returns>
        public static AzureAdAuthResponse FromContractResponse(Contracts.V1.Responses.AzureAdAuthResponse response)
        {
            if (response == null)
                return null;

            return new AzureAdAuthResponse
            {
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                ExpiresAt = response.ExpiresAt,
                User = response.User,
                IsNewUser = response.IsNewUser
            };
        }
    }

    /// <summary>
    /// API response model for token validation operation containing validation status
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

        /// <summary>
        /// Creates an API TokenValidationResponse from a contract TokenValidationResponse
        /// </summary>
        /// <param name="response">Contract response to convert</param>
        /// <returns>An API TokenValidationResponse with mapped properties</returns>
        public static TokenValidationResponse FromContractResponse(Contracts.V1.Responses.TokenValidationResponse response)
        {
            if (response == null)
                return null;

            return new TokenValidationResponse
            {
                IsValid = response.IsValid,
                UserId = response.UserId,
                ExpiresAt = response.ExpiresAt
            };
        }
    }
}