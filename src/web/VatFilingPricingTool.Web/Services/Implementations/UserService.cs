using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the IUserService interface that provides user management functionality 
    /// for the VAT Filing Pricing Tool web application.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApiClient apiClient;
        private readonly AuthenticationService authService;
        private readonly ILogger<UserService> logger;

        /// <summary>
        /// Initializes a new instance of the UserService class with required dependencies.
        /// </summary>
        /// <param name="apiClient">Client for making API requests.</param>
        /// <param name="authService">Service for authentication and user information.</param>
        /// <param name="logger">Logger for diagnostic information.</param>
        public UserService(
            ApiClient apiClient,
            AuthenticationService authService,
            ILogger<UserService> logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.authService = authService ?? throw new ArgumentNullException(nameof(authService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation, containing the current user profile or null if not authenticated.</returns>
        public async Task<UserProfileModel> GetCurrentUserProfileAsync()
        {
            try
            {
                logger.LogInformation("Retrieving current user profile");
                
                // Get the current user from the authentication service
                var currentUser = await authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    logger.LogWarning("No authenticated user found");
                    return null;
                }
                
                // Use the user ID to get the full profile
                return await GetUserProfileAsync(currentUser.UserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving current user profile: {Message}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Retrieves a user profile by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task that represents the asynchronous operation, containing the user profile or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when userId is null or empty.</exception>
        public async Task<UserProfileModel> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty");
            }
            
            try
            {
                logger.LogInformation("Retrieving user profile for user ID: {UserId}", userId);
                
                // Construct the API endpoint URL
                string endpoint = ApiEndpoints.User.GetById.Replace("{id}", userId);
                
                // Make the API request
                return await apiClient.GetAsync<UserProfileModel>(endpoint);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving user profile for user ID {UserId}: {Message}", userId, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="model">The updated user profile data.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null.</exception>
        public async Task<bool> UpdateUserProfileAsync(UserProfileUpdateModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "User profile update model cannot be null");
            }
            
            try
            {
                logger.LogInformation("Updating user profile for user ID: {UserId}", model.UserId);
                
                // Make the API request to update the profile
                var response = await apiClient.PutAsync<UserProfileUpdateModel, bool>(
                    ApiEndpoints.User.UpdateProfile, 
                    model);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user profile for user ID {UserId}: {Message}", model.UserId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users with optional filtering.
        /// </summary>
        /// <param name="filter">Filter criteria for the user list.</param>
        /// <returns>A task that represents the asynchronous operation, containing the paginated user list.</returns>
        public async Task<UserListModel> GetUsersAsync(UserFilterModel filter)
        {
            // Initialize filter if it's null
            filter = filter ?? new UserFilterModel();
            
            try
            {
                logger.LogInformation("Retrieving users with filter - Page: {Page}, PageSize: {PageSize}, SearchTerm: {SearchTerm}, RoleFilter: {RoleFilter}, ActiveOnly: {ActiveOnly}",
                    filter.Page, filter.PageSize, filter.SearchTerm, filter.RoleFilter, filter.ActiveOnly);
                
                // Construct query string
                string queryString = $"?page={filter.Page}&pageSize={filter.PageSize}";
                
                if (!string.IsNullOrEmpty(filter.SearchTerm))
                {
                    queryString += $"&searchTerm={Uri.EscapeDataString(filter.SearchTerm)}";
                }
                
                if (filter.RoleFilter.HasValue)
                {
                    queryString += $"&roleFilter={filter.RoleFilter.Value}";
                }
                
                if (filter.ActiveOnly.HasValue)
                {
                    queryString += $"&activeOnly={filter.ActiveOnly.Value}";
                }
                
                // Make the API request
                return await apiClient.GetAsync<UserListModel>(ApiEndpoints.Admin.Users + queryString);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving users: {Message}", ex.Message);
                return new UserListModel(); // Return empty list on error
            }
        }

        /// <summary>
        /// Updates a user's assigned roles.
        /// </summary>
        /// <param name="model">The roles update model containing the user ID and new role assignments.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null or userId is empty.</exception>
        public async Task<bool> UpdateUserRolesAsync(UserRolesUpdateModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "User roles update model cannot be null");
            }
            
            if (string.IsNullOrEmpty(model.UserId))
            {
                throw new ArgumentNullException(nameof(model.UserId), "User ID cannot be null or empty");
            }
            
            try
            {
                logger.LogInformation("Updating roles for user ID: {UserId}", model.UserId);
                
                // Make the API request to update the roles
                string endpoint = ApiEndpoints.Admin.Users + "/roles";
                var response = await apiClient.PutAsync<UserRolesUpdateModel, bool>(endpoint, model);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating roles for user ID {UserId}: {Message}", model.UserId, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="model">The status update model containing the user ID and desired active status.</param>
        /// <returns>A task that represents the asynchronous operation, containing true if the update was successful, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when model is null or userId is empty.</exception>
        public async Task<bool> UpdateUserStatusAsync(UserStatusUpdateModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model), "User status update model cannot be null");
            }
            
            if (string.IsNullOrEmpty(model.UserId))
            {
                throw new ArgumentNullException(nameof(model.UserId), "User ID cannot be null or empty");
            }
            
            try
            {
                logger.LogInformation("Updating status for user ID: {UserId}, Active: {IsActive}", model.UserId, model.IsActive);
                
                // Make the API request to update the status
                string endpoint = ApiEndpoints.Admin.Users + "/status";
                var response = await apiClient.PutAsync<UserStatusUpdateModel, bool>(endpoint, model);
                
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating status for user ID {UserId}: {Message}", model.UserId, ex.Message);
                return false;
            }
        }
    }
}