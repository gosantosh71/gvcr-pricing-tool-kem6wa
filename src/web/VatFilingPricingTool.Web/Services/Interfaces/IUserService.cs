using System.Threading.Tasks;
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Interface that defines the contract for user management services in the VAT Filing Pricing Tool web application.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the current user profile or null if not authenticated.</returns>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<UserProfileModel> GetCurrentUserProfileAsync();

        /// <summary>
        /// Retrieves a user profile by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing the user profile or null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when userId is null or empty.</exception>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<UserProfileModel> GetUserProfileAsync(string userId);

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="model">The updated user profile data.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when model is null.</exception>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model);

        /// <summary>
        /// Retrieves a paginated list of users with optional filtering.
        /// </summary>
        /// <param name="filter">Filter criteria for the user list.</param>
        /// <returns>A task that represents the asynchronous operation, containing the paginated user list.</returns>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<UserListModel> GetUsersAsync(UserFilterModel filter);

        /// <summary>
        /// Updates a user's assigned roles.
        /// </summary>
        /// <param name="model">The roles update model containing the user ID and new role assignments.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when model is null or userId is empty.</exception>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<bool> UpdateUserRolesAsync(UserRolesUpdateModel model);

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="model">The status update model containing the user ID and desired active status.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when model is null or userId is empty.</exception>
        /// <exception cref="System.Exception">Thrown when an error occurs during the API request.</exception>
        Task<bool> UpdateUserStatusAsync(UserStatusUpdateModel model);
    }
}