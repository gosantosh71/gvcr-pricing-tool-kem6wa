using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for detailed user information
    /// </summary>
    public class UserResponse
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
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The name of the company that the user belongs to
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Date and time when the user account was created
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date and time of the user's most recent login, or null if they have never logged in
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for UserResponse
        /// </summary>
        public UserResponse()
        {
            Roles = new List<UserRole>();
        }
    }

    /// <summary>
    /// Response model for paginated list of users
    /// </summary>
    public class UserListResponse
    {
        /// <summary>
        /// List of user responses in the current page
        /// </summary>
        public List<UserResponse> Users { get; set; }

        /// <summary>
        /// The current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of users across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates whether there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Indicates whether there is a next page available
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Default constructor for UserListResponse
        /// </summary>
        public UserListResponse()
        {
            Users = new List<UserResponse>();
        }

        /// <summary>
        /// Creates a UserListResponse from a PagedList of UserModel
        /// </summary>
        /// <param name="pagedList">The source PagedList of UserModel objects</param>
        /// <returns>A UserListResponse populated with data from the PagedList</returns>
        public static UserListResponse FromPagedList(PagedList<UserModel> pagedList)
        {
            var response = new UserListResponse
            {
                PageNumber = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
                TotalPages = pagedList.TotalPages,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage
            };

            foreach (var userModel in pagedList.Items)
            {
                response.Users.Add(new UserResponse
                {
                    UserId = userModel.UserId,
                    Email = userModel.Email,
                    FirstName = userModel.FirstName,
                    LastName = userModel.LastName,
                    CompanyName = userModel.CompanyName,
                    PhoneNumber = userModel.PhoneNumber,
                    Roles = userModel.Roles,
                    CreatedDate = userModel.CreatedDate,
                    LastLoginDate = userModel.LastLoginDate,
                    IsActive = userModel.IsActive
                });
            }

            return response;
        }
    }

    /// <summary>
    /// Response model for simplified user information used in dropdowns and lists
    /// </summary>
    public class UserSummaryResponse
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Display name for the user (typically FirstName + LastName)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Collection of roles assigned to the user
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Indicates whether the user account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Default constructor for UserSummaryResponse
        /// </summary>
        public UserSummaryResponse()
        {
            Roles = new List<UserRole>();
        }
    }

    /// <summary>
    /// Response model for user profile update operations
    /// </summary>
    public class UpdateUserProfileResponse
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
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// The name of the company that the user belongs to
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// Contact phone number for the user
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indicates whether the update operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The date and time when the profile was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Default constructor for UpdateUserProfileResponse
        /// </summary>
        public UpdateUserProfileResponse()
        {
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for user role update operations
    /// </summary>
    public class UpdateUserRolesResponse
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Updated collection of roles assigned to the user
        /// </summary>
        public List<UserRole> Roles { get; set; }

        /// <summary>
        /// Indicates whether the update operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The date and time when the roles were updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Default constructor for UpdateUserRolesResponse
        /// </summary>
        public UpdateUserRolesResponse()
        {
            Roles = new List<UserRole>();
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Response model for user status change operations
    /// </summary>
    public class ChangeUserStatusResponse
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The updated active status of the user
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the status change operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The date and time when the status was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Default constructor for ChangeUserStatusResponse
        /// </summary>
        public ChangeUserStatusResponse()
        {
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}