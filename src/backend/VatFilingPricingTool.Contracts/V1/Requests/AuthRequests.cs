using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Domain.Enums; // For UserRole enum

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for user authentication with email and password
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The user's email address used for authentication
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// The user's password for authentication
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        /// <summary>
        /// Indicates whether the user wants to stay logged in
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
        /// The email address for the new user account
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// The password for the new user account
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string Password { get; set; }

        /// <summary>
        /// Confirmation of the password to ensure correct entry
        /// </summary>
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// The user's first name
        /// </summary>
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name
        /// </summary>
        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// The name of the user's company
        /// </summary>
        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// The user's phone number
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The roles assigned to the user
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
        /// The email address for the account requiring password reset
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
        /// The email address of the account
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// The reset token received via email for password reset verification
        /// </summary>
        [Required(ErrorMessage = "Reset token is required")]
        public string ResetToken { get; set; }

        /// <summary>
        /// The new password for the account
        /// </summary>
        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", 
            ErrorMessage = "Password must include at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string NewPassword { get; set; }

        /// <summary>
        /// Confirmation of the new password to ensure correct entry
        /// </summary>
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>
    /// Request model for refreshing an authentication token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token used to obtain a new access token
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
        /// The ID token received from Azure AD after successful authentication
        /// </summary>
        [Required(ErrorMessage = "ID token is required")]
        public string IdToken { get; set; }
    }
}