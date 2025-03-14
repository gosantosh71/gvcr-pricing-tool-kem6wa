using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Data.Context;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for User entities that provides data access operations specific to users
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Initializes a new instance of the UserRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public UserRepository(IVatFilingDbContext context, ILogger<UserRepository> logger = null)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Retrieves a user by their email address
        /// </summary>
        /// <param name="email">The email address of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user if found, or null.</returns>
        public async Task<User> GetByEmailAsync(string email)
        {
            _logger?.LogInformation("Retrieving user by email: {Email}", email);
            
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty");
            }
            
            // Normalize email to lowercase for case-insensitive comparison
            email = email.ToLowerInvariant();
            
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
            
            _logger?.LogInformation("User with email {Email} {Result}", email, user != null ? "found" : "not found");
            
            return user;
        }

        /// <summary>
        /// Retrieves a user by their email address with their roles included
        /// </summary>
        /// <param name="email">The email address of the user to retrieve</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user with roles if found, or null if not found</returns>
        public async Task<User> GetByEmailWithRolesAsync(string email)
        {
            _logger?.LogInformation("Retrieving user by email with roles: {Email}", email);
            
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty");
            }
            
            // Normalize email to lowercase for case-insensitive comparison
            email = email.ToLowerInvariant();
            
            var user = await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
            
            _logger?.LogInformation("User with email {Email} {Result}", email, user != null ? "found" : "not found");
            
            return user;
        }

        /// <summary>
        /// Retrieves all users with their roles included
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all users with their roles.</returns>
        public async Task<IEnumerable<User>> GetUsersWithRolesAsync()
        {
            _logger?.LogInformation("Retrieving all users with roles");
            
            var users = await _dbSet.ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} users with roles", users.Count);
            
            return users;
        }

        /// <summary>
        /// Retrieves all users that have a specific role
        /// </summary>
        /// <param name="role">The role to filter users by</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of users with the specified role</returns>
        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            _logger?.LogInformation("Retrieving users with role: {Role}", role);
            
            var users = await _dbSet.Where(u => u.Role == role).ToListAsync();
            
            _logger?.LogInformation("Retrieved {Count} users with role {Role}", users.Count, role);
            
            return users;
        }

        /// <summary>
        /// Checks if a user with the specified email exists
        /// </summary>
        /// <param name="email">The email address to check</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the email exists</returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            _logger?.LogInformation("Checking if email exists: {Email}", email);
            
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty");
            }
            
            // Normalize email to lowercase for case-insensitive comparison
            email = email.ToLowerInvariant();
            
            var exists = await _dbSet.AnyAsync(u => u.Email == email);
            
            _logger?.LogInformation("Email {Email} {Result}", email, exists ? "exists" : "does not exist");
            
            return exists;
        }

        /// <summary>
        /// Adds a role to a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="role">The role to add</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        public async Task<bool> AddRoleToUserAsync(string userId, UserRole role)
        {
            _logger?.LogInformation("Adding role {Role} to user {UserId}", role, userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for role assignment", userId);
                return false;
            }
            
            user.UpdateRole(role);
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("Role {Role} added to user {UserId}", role, userId);
            
            return true;
        }

        /// <summary>
        /// Removes a role from a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="role">The role to remove</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the operation was successful</returns>
        public async Task<bool> RemoveRoleFromUserAsync(string userId, UserRole role)
        {
            _logger?.LogInformation("Removing role {Role} from user {UserId}", role, userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for role removal", userId);
                return false;
            }
            
            if (!user.HasRole(role))
            {
                _logger?.LogWarning("User {UserId} does not have role {Role}", userId, role);
                return false;
            }
            
            // Set the role to Customer (default role)
            user.UpdateRole(UserRole.Customer);
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("Role {Role} removed from user {UserId}", role, userId);
            
            return true;
        }

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <param name="companyName">The user's company name</param>
        /// <param name="phoneNumber">The user's phone number</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated user</returns>
        public async Task<User> UpdateUserProfileAsync(string userId, string firstName, string lastName, string companyName, string phoneNumber)
        {
            _logger?.LogInformation("Updating profile for user {UserId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for profile update", userId);
                return null;
            }
            
            // Update the user's profile with the provided information
            user.UpdateProfile(firstName, lastName);
            
            // Note: Company name and phone number are not part of the current User entity
            // If these properties are added in the future, they should be updated here
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("Profile updated for user {UserId}", userId);
            
            return user;
        }

        /// <summary>
        /// Updates a user's last login date to the current time
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the update was successful</returns>
        public async Task<bool> UpdateLastLoginDateAsync(string userId)
        {
            _logger?.LogInformation("Updating last login date for user {UserId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for login date update", userId);
                return false;
            }
            
            user.UpdateLastLoginDate();
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("Last login date updated for user {UserId}", userId);
            
            return true;
        }

        /// <summary>
        /// Activates a user account
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the activation was successful</returns>
        public async Task<bool> ActivateUserAsync(string userId)
        {
            _logger?.LogInformation("Activating user account {UserId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for activation", userId);
                return false;
            }
            
            user.Activate();
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("User account {UserId} activated", userId);
            
            return true;
        }

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deactivation was successful</returns>
        public async Task<bool> DeactivateUserAsync(string userId)
        {
            _logger?.LogInformation("Deactivating user account {UserId}", userId);
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            var user = await GetByIdAsync(userId);
            
            if (user == null)
            {
                _logger?.LogWarning("User {UserId} not found for deactivation", userId);
                return false;
            }
            
            user.Deactivate();
            
            await UpdateAsync(user);
            
            _logger?.LogInformation("User account {UserId} deactivated", userId);
            
            return true;
        }
    }
}