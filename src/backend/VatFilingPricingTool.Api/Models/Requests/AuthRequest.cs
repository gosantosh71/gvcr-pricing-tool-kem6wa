using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// API request model for user authentication with email and password
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

        /// <summary>
        /// Converts the API login request to a contract login request
        /// </summary>
        /// <returns>A contract login request with mapped properties</returns>
        public Contracts.V1.Requests.LoginRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.LoginRequest
            {
                Email = this.Email,
                Password = this.Password,
                RememberMe = this.RememberMe
            };
        }
    }

    /// <summary>
    /// API request model for user registration with personal details and credentials
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

        /// <summary>
        /// Converts the API registration request to a contract registration request
        /// </summary>
        /// <returns>A contract registration request with mapped properties</returns>
        public Contracts.V1.Requests.RegisterRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.RegisterRequest
            {
                Email = this.Email,
                Password = this.Password,
                ConfirmPassword = this.ConfirmPassword,
                FirstName = this.FirstName,
                LastName = this.LastName,
                CompanyName = this.CompanyName,
                PhoneNumber = this.PhoneNumber,
                Roles = this.Roles
            };
        }
    }

    /// <summary>
    /// API request model for initiating a password reset process
    /// </summary>
    public class PasswordResetRequest
    {
        /// <summary>
        /// The email address for the account requiring password reset
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// Converts the API password reset request to a contract password reset request
        /// </summary>
        /// <returns>A contract password reset request with mapped properties</returns>
        public Contracts.V1.Requests.PasswordResetRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.PasswordResetRequest
            {
                Email = this.Email
            };
        }
    }

    /// <summary>
    /// API request model for changing a password using a reset token
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

        /// <summary>
        /// Converts the API password change request to a contract password change request
        /// </summary>
        /// <returns>A contract password change request with mapped properties</returns>
        public Contracts.V1.Requests.PasswordChangeRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.PasswordChangeRequest
            {
                Email = this.Email,
                ResetToken = this.ResetToken,
                NewPassword = this.NewPassword,
                ConfirmPassword = this.ConfirmPassword
            };
        }
    }

    /// <summary>
    /// API request model for refreshing an authentication token
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// The refresh token used to obtain a new access token
        /// </summary>
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Converts the API refresh token request to a contract refresh token request
        /// </summary>
        /// <returns>A contract refresh token request with mapped properties</returns>
        public Contracts.V1.Requests.RefreshTokenRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.RefreshTokenRequest
            {
                RefreshToken = this.RefreshToken
            };
        }
    }

    /// <summary>
    /// API request model for authenticating with Azure Active Directory
    /// </summary>
    public class AzureAdAuthRequest
    {
        /// <summary>
        /// The ID token received from Azure AD after successful authentication
        /// </summary>
        [Required(ErrorMessage = "ID token is required")]
        public string IdToken { get; set; }

        /// <summary>
        /// Converts the API Azure AD authentication request to a contract Azure AD authentication request
        /// </summary>
        /// <returns>A contract Azure AD authentication request with mapped properties</returns>
        public Contracts.V1.Requests.AzureAdAuthRequest ToContractRequest()
        {
            return new Contracts.V1.Requests.AzureAdAuthRequest
            {
                IdToken = this.IdToken
            };
        }
    }
}