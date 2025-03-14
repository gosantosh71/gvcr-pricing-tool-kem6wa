using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Interface for User repository operations that extends the generic IRepository with user-specific functionality.
    /// Provides methods for user authentication, role management, and profile operations.
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, or null if not found</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by their email address with their roles included
        /// </summary>
        /// <param name="email">The email address of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user with roles if found, or null if not found</returns>
        Task<User> GetByEmailWithRolesAsync(string email);

        /// <summary>
        /// Retrieves all users with their roles included
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of users with their roles</returns>
        Task<IEnumerable<User>> GetUsersWithRolesAsync();

        /// <summary>
        /// Retrieves all users that have a specific role
        /// </summary>
        /// <param name="role">The role to filter users by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of users with the specified role</returns>
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);

        /// <summary>
        /// Checks if a user with the specified email exists
        /// </summary>
        /// <param name="email">The email address to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the email exists</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Adds a role to a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="role">The role to add</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        Task<bool> AddRoleToUserAsync(string userId, UserRole role);

        /// <summary>
        /// Removes a role from a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="role">The role to remove</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        Task<bool> RemoveRoleFromUserAsync(string userId, UserRole role);

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <param name="companyName">The user's company name</param>
        /// <param name="phoneNumber">The user's phone number</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated user</returns>
        Task<User> UpdateUserProfileAsync(string userId, string firstName, string lastName, string companyName, string phoneNumber);

        /// <summary>
        /// Updates a user's last login date to the current time
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        Task<bool> UpdateLastLoginDateAsync(string userId);

        /// <summary>
        /// Activates a user account
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        Task<bool> ActivateUserAsync(string userId);

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        Task<bool> DeactivateUserAsync(string userId);
    }
}