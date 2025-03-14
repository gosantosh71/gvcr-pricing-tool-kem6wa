using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for retrieving a specific user by ID.
    /// </summary>
    public class GetUserApiRequest
    {
        /// <summary>
        /// The unique identifier of the user to retrieve.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a paginated list of users with optional filtering.
    /// </summary>
    public class GetUsersApiRequest
    {
        /// <summary>
        /// The page number to retrieve (1-based).
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of users to retrieve per page.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Optional search term to filter users by name or email.
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Optional filter to return only users with a specific role.
        /// </summary>
        public UserRole? RoleFilter { get; set; }

        /// <summary>
        /// Optional filter to return only active users when set to true.
        /// </summary>
        public bool? ActiveOnly { get; set; }
    }

    /// <summary>
    /// Request model for updating a user's profile information.
    /// </summary>
    public class UpdateUserProfileApiRequest
    {
        /// <summary>
        /// The user's first name.
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// The name of the company the user belongs to.
        /// </summary>
        [StringLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// The user's phone number.
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(50, ErrorMessage = "Phone number cannot exceed 50 characters")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email { get; set; }

        /// <summary>
        /// Converts the API request model to a contract request model.
        /// </summary>
        /// <returns>A contract request model for the service layer.</returns>
        public UpdateUserProfileRequest ToContractRequest()
        {
            return new UpdateUserProfileRequest
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                CompanyName = this.CompanyName,
                PhoneNumber = this.PhoneNumber,
                Email = this.Email
            };
        }
    }

    /// <summary>
    /// Request model for updating a user's assigned roles.
    /// </summary>
    public class UpdateUserRolesApiRequest
    {
        /// <summary>
        /// The unique identifier of the user whose roles are being updated.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        /// The collection of roles to assign to the user.
        /// </summary>
        [Required(ErrorMessage = "At least one role must be specified")]
        public List<UserRole> Roles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Converts the API request model to a contract request model.
        /// </summary>
        /// <returns>A contract request model for the service layer.</returns>
        public UpdateUserRolesRequest ToContractRequest()
        {
            return new UpdateUserRolesRequest
            {
                UserId = this.UserId,
                Roles = new List<UserRole>(this.Roles)
            };
        }
    }

    /// <summary>
    /// Request model for activating or deactivating a user account.
    /// </summary>
    public class ChangeUserStatusApiRequest
    {
        /// <summary>
        /// The unique identifier of the user whose status is being changed.
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        /// Indicates whether the user account should be active (true) or inactive (false).
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Converts the API request model to a contract request model.
        /// </summary>
        /// <returns>A contract request model for the service layer.</returns>
        public ChangeUserStatusRequest ToContractRequest()
        {
            return new ChangeUserStatusRequest
            {
                UserId = this.UserId,
                IsActive = this.IsActive
            };
        }
    }

    // Note: The following contract request classes are referenced but are defined in the service layer
    // and should be imported in the actual implementation:
    // - UpdateUserProfileRequest
    // - UpdateUserRolesRequest
    // - ChangeUserStatusRequest
}