using System.Collections.Generic; // Version 6.0.0
using System.Threading.Tasks; // Version 6.0.0
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Interface for user management services that provides methods for user retrieval, 
    /// profile management, role assignment, and account status operations.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by ID based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the user ID to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        Task<UserResponse> GetUserAsync(GetUserRequest request);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        Task<UserResponse> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        Task<UserResponse> GetUserByEmailAsync(string email);

        /// <summary>
        /// Retrieves a paginated list of users with optional filtering.
        /// </summary>
        /// <param name="request">The request containing pagination, search, and filter parameters</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated user list.</returns>
        Task<UserListResponse> GetUsersAsync(GetUsersRequest request);

        /// <summary>
        /// Retrieves a list of simplified user summaries, optionally filtered by role.
        /// Used primarily for dropdowns and user selection components.
        /// </summary>
        /// <param name="roleFilter">Optional filter to only return users with a specific role</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of user summaries.</returns>
        Task<List<UserSummaryResponse>> GetUserSummariesAsync(UserRole? roleFilter = null);

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose profile is being updated</param>
        /// <param name="request">The request containing the updated profile information</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the update operation result.</returns>
        Task<UpdateUserProfileResponse> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request);

        /// <summary>
        /// Updates a user's assigned roles.
        /// </summary>
        /// <param name="request">The request containing the user ID and the updated roles</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the update operation result.</returns>
        Task<UpdateUserRolesResponse> UpdateUserRolesAsync(UpdateUserRolesRequest request);

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="request">The request containing the user ID and the desired active status</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the status change operation result.</returns>
        Task<ChangeUserStatusResponse> ChangeUserStatusAsync(ChangeUserStatusRequest request);
    }
}