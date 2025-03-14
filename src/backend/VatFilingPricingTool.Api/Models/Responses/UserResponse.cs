using System; // Version 6.0.0
using System.Collections.Generic; // Version 6.0.0
using System.ComponentModel.DataAnnotations; // Version 6.0.0
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// API response model for detailed user information
    /// </summary>
    public class UserApiResponse
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
        /// Default constructor for UserApiResponse
        /// </summary>
        public UserApiResponse()
        {
            Roles = new List<UserRole>();
        }

        /// <summary>
        /// Creates an API UserApiResponse from a contract UserResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API UserApiResponse with mapped properties</returns>
        public static UserApiResponse FromContractResponse(Contracts.V1.Responses.UserResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new UserApiResponse
            {
                UserId = response.UserId,
                Email = response.Email,
                FirstName = response.FirstName,
                LastName = response.LastName,
                CompanyName = response.CompanyName,
                PhoneNumber = response.PhoneNumber,
                Roles = response.Roles,
                CreatedDate = response.CreatedDate,
                LastLoginDate = response.LastLoginDate,
                IsActive = response.IsActive
            };
        }
    }

    /// <summary>
    /// API response model for paginated list of users
    /// </summary>
    public class UserListApiResponse
    {
        /// <summary>
        /// List of user responses in the current page
        /// </summary>
        public List<UserApiResponse> Users { get; set; }

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
        /// Default constructor for UserListApiResponse
        /// </summary>
        public UserListApiResponse()
        {
            Users = new List<UserApiResponse>();
        }

        /// <summary>
        /// Creates an API UserListApiResponse from a contract UserListResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API UserListApiResponse with mapped properties</returns>
        public static UserListApiResponse FromContractResponse(Contracts.V1.Responses.UserListResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            var apiResponse = new UserListApiResponse
            {
                PageNumber = response.PageNumber,
                PageSize = response.PageSize,
                TotalCount = response.TotalCount,
                TotalPages = response.TotalPages,
                HasPreviousPage = response.HasPreviousPage,
                HasNextPage = response.HasNextPage
            };

            foreach (var user in response.Users)
            {
                apiResponse.Users.Add(UserApiResponse.FromContractResponse(user));
            }

            return apiResponse;
        }
    }

    /// <summary>
    /// API response model for simplified user information used in dropdowns and lists
    /// </summary>
    public class UserSummaryApiResponse
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
        /// Default constructor for UserSummaryApiResponse
        /// </summary>
        public UserSummaryApiResponse()
        {
            Roles = new List<UserRole>();
        }

        /// <summary>
        /// Creates an API UserSummaryApiResponse from a contract UserSummaryResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API UserSummaryApiResponse with mapped properties</returns>
        public static UserSummaryApiResponse FromContractResponse(Contracts.V1.Responses.UserSummaryResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new UserSummaryApiResponse
            {
                UserId = response.UserId,
                DisplayName = response.DisplayName,
                Email = response.Email,
                Roles = response.Roles,
                IsActive = response.IsActive
            };
        }
    }

    /// <summary>
    /// API response model for user profile update operations
    /// </summary>
    public class UpdateUserProfileApiResponse
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
        /// Default constructor for UpdateUserProfileApiResponse
        /// </summary>
        public UpdateUserProfileApiResponse()
        {
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an API UpdateUserProfileApiResponse from a contract UpdateUserProfileResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API UpdateUserProfileApiResponse with mapped properties</returns>
        public static UpdateUserProfileApiResponse FromContractResponse(Contracts.V1.Responses.UpdateUserProfileResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new UpdateUserProfileApiResponse
            {
                UserId = response.UserId,
                Email = response.Email,
                FirstName = response.FirstName,
                LastName = response.LastName,
                CompanyName = response.CompanyName,
                PhoneNumber = response.PhoneNumber,
                Success = response.Success,
                UpdatedAt = response.UpdatedAt
            };
        }
    }

    /// <summary>
    /// API response model for user role update operations
    /// </summary>
    public class UpdateUserRolesApiResponse
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
        /// Default constructor for UpdateUserRolesApiResponse
        /// </summary>
        public UpdateUserRolesApiResponse()
        {
            Roles = new List<UserRole>();
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an API UpdateUserRolesApiResponse from a contract UpdateUserRolesResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API UpdateUserRolesApiResponse with mapped properties</returns>
        public static UpdateUserRolesApiResponse FromContractResponse(Contracts.V1.Responses.UpdateUserRolesResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new UpdateUserRolesApiResponse
            {
                UserId = response.UserId,
                Roles = response.Roles,
                Success = response.Success,
                UpdatedAt = response.UpdatedAt
            };
        }
    }

    /// <summary>
    /// API response model for user status change operations
    /// </summary>
    public class ChangeUserStatusApiResponse
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
        /// Default constructor for ChangeUserStatusApiResponse
        /// </summary>
        public ChangeUserStatusApiResponse()
        {
            Success = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an API ChangeUserStatusApiResponse from a contract ChangeUserStatusResponse
        /// </summary>
        /// <param name="response">The contract response to map from</param>
        /// <returns>An API ChangeUserStatusApiResponse with mapped properties</returns>
        public static ChangeUserStatusApiResponse FromContractResponse(Contracts.V1.Responses.ChangeUserStatusResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            return new ChangeUserStatusApiResponse
            {
                UserId = response.UserId,
                IsActive = response.IsActive,
                Success = response.Success,
                UpdatedAt = response.UpdatedAt
            };
        }
    }
}