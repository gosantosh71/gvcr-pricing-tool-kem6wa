using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Domain.Enums; // For UserRole enum

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Request model for user authentication with email and password
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Email address of the user trying to log in
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Password for authentication
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the authentication should persist beyond the current session
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// Default constructor for LoginRequest
        /// </summary>
        public LoginRequest()
        {
            RememberMe = false;
        }
    }

    /// <summary>
    /// Request model for user registration with personal details and credentials
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Email address for the new user account
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Password for the new user account
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the password to ensure correct entry
        /// </summary>
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user
        /// </summary>
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// Company name associated with the user
        /// </summary>
        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Roles assigned to the user
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Default constructor for RegisterRequest
        /// </summary>
        public RegisterRequest()
        {
            Roles = new List<UserRole>();
        }
    }

    /// <summary>
    /// Request model for initiating a password reset process
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// Email address of the account for which the password reset is requested
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }

    /// <summary>
    /// Request model for changing a password using a reset token
    /// </summary>
    public class PasswordChangeRequest
    {
        /// <summary>
        /// Email address of the account for which the password is being changed
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Token received for password reset verification
        /// </summary>
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; }

        /// <summary>
        /// New password for the account
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmation of the new password to ensure correct entry
        /// </summary>
        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Request model for refreshing an authentication token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Refresh token used to obtain a new access token
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Request model for authenticating with Azure Active Directory
    /// </summary>
    public class AzureAdAuthRequest
    {
        /// <summary>
        /// ID token received from Azure AD authentication
        /// </summary>
        [Required(ErrorMessage = "ID token is required")]
        public string IdToken { get; set; }
    }

    /// <summary>
    /// Base response model for authentication operations
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// Indicates whether the authentication operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Message providing additional information about the authentication result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Collection of error messages if the authentication operation failed
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Default constructor for AuthResponse
        /// </summary>
        public AuthResponse()
        {
            Success = true;
            Errors = new List<string>();
        }
    }

    /// <summary>
    /// Response model for successful authentication containing JWT token and user information
    /// </summary>
    public class AuthSuccessResponse : AuthResponse
    {
        /// <summary>
        /// JWT access token for authenticated API access
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Refresh token for obtaining a new access token without re-authentication
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration time of the access token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information for the authenticated user
        /// </summary>
        public UserModel User { get; set; }
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
        /// Unique identifier for the newly registered user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the registered user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for RegisterResponse
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
        /// Email address for which the password reset was requested
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for PasswordResetResponse
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
        /// Email address for which the password was changed
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Default constructor for PasswordChangeResponse
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
        /// New JWT access token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// New refresh token for future token refreshes
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration time of the new access token
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Response model for Azure AD authentication containing token information and user details
    /// </summary>
    public class AzureAdAuthResponse
    {
        /// <summary>
        /// JWT access token for authenticated API access
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Refresh token for obtaining a new access token without re-authentication
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Expiration time of the access token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// User information for the authenticated user
        /// </summary>
        public UserModel User { get; set; }

        /// <summary>
        /// Indicates whether this is the first time the user has authenticated with Azure AD
        /// </summary>
        public bool IsNewUser { get; set; }

        /// <summary>
        /// Default constructor for AzureAdAuthResponse
        /// </summary>
        public AzureAdAuthResponse()
        {
            IsNewUser = false;
        }
    }

    /// <summary>
    /// Model representing a user in the VAT Filing Pricing Tool system with authentication details, 
    /// profile information, and role-based permissions
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the user, used as the username for authentication
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user that determine their access permissions
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Date and time when the user account was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's most recent login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Name of the company the user is associated with
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Default constructor for the UserModel
        /// </summary>
        public UserModel()
        {
            Roles = new List<UserRole>();
            CreatedDate = DateTime.UtcNow;
            IsActive = true;
        }

        /// <summary>
        /// Returns the full name of the user by combining first and last name
        /// </summary>
        /// <returns>The full name of the user</returns>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        /// <param name="role">The role to check for</param>
        /// <returns>True if the user has the specified role, otherwise false</returns>
        public bool HasRole(UserRole role)
        {
            return Roles.Contains(role);
        }

        /// <summary>
        /// Checks if the user has administrator privileges
        /// </summary>
        /// <returns>True if the user has the Administrator role, otherwise false</returns>
        public bool IsAdmin()
        {
            return Roles.Contains(UserRole.Administrator);
        }
    }
}