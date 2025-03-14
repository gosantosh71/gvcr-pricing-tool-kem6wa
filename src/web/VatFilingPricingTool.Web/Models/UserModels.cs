using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations package version 6.0.0
using VatFilingPricingTool.Domain.Enums; // Importing UserRole enum

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Model representing a user profile in the VAT Filing Pricing Tool system with personal and authentication details
    /// </summary>
    public class UserProfileModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the user, used for authentication and communication
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user that determine their permissions
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Date and time when the user account was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's most recent login, null if the user has never logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Name of the company the user is associated with
        /// </summary>
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Phone number of the user
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Default constructor for the UserProfileModel
        /// </summary>
        public UserProfileModel()
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
            return FirstName + " " + LastName;
        }

        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        /// <param name="role">The role to check</param>
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

    /// <summary>
    /// Model for updating a user's profile information
    /// </summary>
    public class UserProfileUpdateModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        /// First name of the user
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the user
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        /// <summary>
        /// Name of the company the user is associated with
        /// </summary>
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Phone number of the user
        /// </summary>
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Model representing a user item in a list view with essential information
    /// </summary>
    public class UserListItemModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Full name of the user (first name + last name)
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Date and time when the user account was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's most recent login
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Default constructor for the UserListItemModel
        /// </summary>
        public UserListItemModel()
        {
            Roles = new List<UserRole>();
        }
    }

    /// <summary>
    /// Model representing a paginated list of users
    /// </summary>
    public class UserListModel
    {
        /// <summary>
        /// List of user items to display
        /// </summary>
        public List<UserListItemModel> Users { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of users matching the filter criteria
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total number of pages available
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Default constructor for the UserListModel
        /// </summary>
        public UserListModel()
        {
            Users = new List<UserListItemModel>();
            PageNumber = 1;
            PageSize = 10;
            TotalCount = 0;
            TotalPages = 0;
        }
    }

    /// <summary>
    /// Model for updating a user's roles
    /// </summary>
    public class UserRolesUpdateModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        /// Collection of roles to assign to the user
        /// </summary>
        [Required(ErrorMessage = "At least one role must be assigned")]
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Default constructor for the UserRolesUpdateModel
        /// </summary>
        public UserRolesUpdateModel()
        {
            Roles = new List<UserRole>();
        }
    }

    /// <summary>
    /// Model for updating a user's active status
    /// </summary>
    public class UserStatusUpdateModel
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        /// <summary>
        /// Indicates whether the user account should be active
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Model for filtering user lists with search and pagination parameters
    /// </summary>
    public class UserFilterModel
    {
        /// <summary>
        /// Search term to filter users by name or email
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Filter users by a specific role
        /// </summary>
        public UserRole? RoleFilter { get; set; }

        /// <summary>
        /// Filter to show only active users when true
        /// </summary>
        public bool? ActiveOnly { get; set; }

        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Default constructor for the UserFilterModel
        /// </summary>
        public UserFilterModel()
        {
            Page = 1;
            PageSize = 10;
            ActiveOnly = null;
            RoleFilter = null;
            SearchTerm = null;
        }
    }
}