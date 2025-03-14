using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Service.Models;

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Implementation of the IUserService interface that provides user management functionality for the VAT Filing Pricing Tool.
    /// Handles user retrieval, profile management, role assignment, and account status operations.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        /// <summary>
        /// Initializes a new instance of the UserService class with dependency injection.
        /// </summary>
        /// <param name="userRepository">The user repository for data access.</param>
        /// <exception cref="ArgumentNullException">Thrown when userRepository is null.</exception>
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Retrieves a user by ID based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the user ID to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the user ID is empty or invalid.</exception>
        public async Task<UserResponse> GetUserAsync(GetUserRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.UserId))
                throw new ValidationException("User ID is required");

            return await GetUserByIdAsync(request.UserId);
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when userId is null or empty.</exception>
        public async Task<UserResponse> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            var userModel = UserModel.FromDomain(user);
            return MapToUserResponse(userModel);
        }

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">The email address of the user to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the user details.</returns>
        /// <exception cref="ArgumentNullException">Thrown when email is null or empty.</exception>
        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email));

            var user = await _userRepository.GetByEmailWithRolesAsync(email);
            if (user == null)
                return null;

            var userModel = UserModel.FromDomain(user);
            return MapToUserResponse(userModel);
        }

        /// <summary>
        /// Retrieves a paginated list of users with optional filtering.
        /// </summary>
        /// <param name="request">The request containing pagination, search, and filter parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated user list.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        public async Task<UserListResponse> GetUsersAsync(GetUsersRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var users = await _userRepository.GetUsersWithRolesAsync();
            var filteredUsers = users.AsQueryable();

            // Apply filtering
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filteredUsers = filteredUsers.Where(u =>
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            if (request.RoleFilter.HasValue)
            {
                filteredUsers = filteredUsers.Where(u => u.Role == request.RoleFilter.Value);
            }

            if (request.ActiveOnly == true)
            {
                filteredUsers = filteredUsers.Where(u => u.IsActive);
            }

            // Map to UserModel and create paged list
            var userModels = filteredUsers
                .Select(u => UserModel.FromDomain(u))
                .ToList();

            var pagedList = PagedList<UserModel>.Create(
                userModels,
                request.Page,
                request.PageSize);

            // Return response
            return UserListResponse.FromPagedList(pagedList);
        }

        /// <summary>
        /// Retrieves a list of simplified user summaries, optionally filtered by role.
        /// Used primarily for dropdowns and user selection components.
        /// </summary>
        /// <param name="roleFilter">Optional filter to only return users with a specific role.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of user summaries.</returns>
        public async Task<List<UserSummaryResponse>> GetUserSummariesAsync(UserRole? roleFilter = null)
        {
            IEnumerable<User> users;

            if (roleFilter.HasValue)
            {
                users = await _userRepository.GetUsersByRoleAsync(roleFilter.Value);
            }
            else
            {
                users = await _userRepository.GetUsersWithRolesAsync();
            }

            var userSummaries = users
                .Select(u => UserModel.FromDomain(u))
                .Select(model => new UserSummaryResponse
                {
                    UserId = model.UserId,
                    DisplayName = model.GetFullName(),
                    Email = model.Email,
                    Roles = model.Roles,
                    IsActive = model.IsActive
                })
                .ToList();

            return userSummaries;
        }

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="userId">The unique identifier of the user whose profile is being updated.</param>
        /// <param name="request">The request containing the updated profile information.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the update operation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when userId or request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the user is not found.</exception>
        public async Task<UpdateUserProfileResponse> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ValidationException($"User with ID {userId} not found");

            // Update user profile
            user.UpdateProfile(request.FirstName, request.LastName);
            
            // Save changes to repository
            var updatedUser = await _userRepository.UpdateUserProfileAsync(
                userId, 
                request.FirstName, 
                request.LastName, 
                request.CompanyName, 
                request.PhoneNumber);

            // Create response
            var response = new UpdateUserProfileResponse
            {
                UserId = updatedUser.UserId,
                Email = updatedUser.Email,
                FirstName = updatedUser.FirstName,
                LastName = updatedUser.LastName,
                CompanyName = updatedUser.CompanyName,
                PhoneNumber = updatedUser.PhoneNumber,
                Success = true,
                UpdatedAt = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Updates a user's assigned roles.
        /// </summary>
        /// <param name="request">The request containing the user ID and the updated roles.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the update operation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the user ID is empty, no roles are specified, or the user is not found.</exception>
        public async Task<UpdateUserRolesResponse> UpdateUserRolesAsync(UpdateUserRolesRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.UserId))
                throw new ValidationException("User ID is required");

            if (request.Roles == null || !request.Roles.Any())
                throw new ValidationException("At least one role must be specified");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ValidationException($"User with ID {request.UserId} not found");

            // Get current roles
            var userModel = UserModel.FromDomain(user);
            var currentRoles = userModel.Roles.ToList();
            
            // Determine roles to remove and add
            var rolesToRemove = currentRoles.Except(request.Roles).ToList();
            var rolesToAdd = request.Roles.Except(currentRoles).ToList();

            // Update roles in repository
            foreach (var role in rolesToRemove)
            {
                await _userRepository.RemoveRoleFromUserAsync(request.UserId, role);
            }

            foreach (var role in rolesToAdd)
            {
                await _userRepository.AddRoleToUserAsync(request.UserId, role);
            }

            // Create response
            var response = new UpdateUserRolesResponse
            {
                UserId = request.UserId,
                Roles = request.Roles,
                Success = true,
                UpdatedAt = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Activates or deactivates a user account.
        /// </summary>
        /// <param name="request">The request containing the user ID and the desired active status.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the status change operation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the user ID is empty or the user is not found.</exception>
        public async Task<ChangeUserStatusResponse> ChangeUserStatusAsync(ChangeUserStatusRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.UserId))
                throw new ValidationException("User ID is required");

            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ValidationException($"User with ID {request.UserId} not found");

            bool success;
            if (request.IsActive)
            {
                user.Activate();
                success = await _userRepository.ActivateUserAsync(request.UserId);
            }
            else
            {
                user.Deactivate();
                success = await _userRepository.DeactivateUserAsync(request.UserId);
            }

            // Create response
            var response = new ChangeUserStatusResponse
            {
                UserId = request.UserId,
                IsActive = request.IsActive,
                Success = success,
                UpdatedAt = DateTime.UtcNow
            };

            return response;
        }

        /// <summary>
        /// Maps a UserModel to a UserResponse.
        /// </summary>
        /// <param name="model">The user model to map.</param>
        /// <returns>A UserResponse object populated with data from the UserModel.</returns>
        private UserResponse MapToUserResponse(UserModel model)
        {
            return new UserResponse
            {
                UserId = model.UserId,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CompanyName = model.CompanyName,
                PhoneNumber = model.PhoneNumber,
                Roles = model.Roles,
                CreatedDate = model.CreatedDate,
                LastLoginDate = model.LastLoginDate,
                IsActive = model.IsActive
            };
        }
    }
}