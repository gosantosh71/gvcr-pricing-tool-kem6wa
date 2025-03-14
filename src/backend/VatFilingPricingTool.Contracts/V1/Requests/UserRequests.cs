using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for retrieving a specific user by ID
    /// </summary>
    public class GetUserRequest
    {
        /// <summary>
        /// The unique identifier of the user to retrieve
        /// </summary>
        [Required]
        public string UserId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a paginated list of users with optional filtering
    /// </summary>
    public class GetUsersRequest
    {
        /// <summary>
        /// The page number to retrieve (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        /// <summary>
        /// The number of users to retrieve per page
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Optional search term to filter users by name or email
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Optional filter to only return users with a specific role
        /// </summary>
        public UserRole? RoleFilter { get; set; }

        /// <summary>
        /// Optional filter to only return active users when set to true
        /// </summary>
        public bool? ActiveOnly { get; set; }
    }

    /// <summary>
    /// Request model for updating a user's profile information
    /// </summary>
    public class UpdateUserProfileRequest
    {
        /// <summary>
        /// The user's first name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        /// <summary>
        /// The user's last name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        /// <summary>
        /// The name of the company the user belongs to
        /// </summary>
        [StringLength(200)]
        public string CompanyName { get; set; }

        /// <summary>
        /// The user's phone number
        /// </summary>
        [Phone]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// The user's email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    /// <summary>
    /// Request model for updating a user's assigned roles
    /// </summary>
    public class UpdateUserRolesRequest
    {
        /// <summary>
        /// The unique identifier of the user whose roles are being updated
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// The list of roles to assign to the user
        /// </summary>
        [Required]
        public List<UserRole> Roles { get; set; } = new List<UserRole>();
    }

    /// <summary>
    /// Request model for activating or deactivating a user account
    /// </summary>
    public class ChangeUserStatusRequest
    {
        /// <summary>
        /// The unique identifier of the user whose status is being changed
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Whether the user account should be active
        /// </summary>
        public bool IsActive { get; set; }
    }
}