using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Api.Models.Responses;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Domain.Enums;

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller that handles user management operations including profile management, 
    /// role assignments, and account status changes
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.User.Base)]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Initializes a new instance of the UserController with required dependencies
        /// </summary>
        /// <param name="userService">The user service for business logic operations</param>
        /// <param name="logger">The logger for recording controller activities</param>
        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves a user by ID
        /// </summary>
        /// <param name="request">The request containing the user ID to retrieve</param>
        /// <returns>The user details if found</returns>
        [HttpGet(ApiRoutes.User.Get)]
        [ProducesResponseType(typeof(UserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserAsync([FromQuery] GetUserApiRequest request)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {UserId}", request.UserId);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(CreateErrorResponse("Invalid request", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }
                
                var userRequest = new GetUserRequest { UserId = request.UserId };
                var result = await _userService.GetUserAsync(userRequest);
                
                if (result == null)
                {
                    return NotFound(CreateErrorResponse($"User with ID {request.UserId} not found"));
                }
                
                return Ok(UserApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Retrieves a user by ID from route parameter
        /// </summary>
        /// <param name="id">The ID of the user to retrieve</param>
        /// <returns>The user details if found</returns>
        [HttpGet(ApiRoutes.User.GetById)]
        [ProducesResponseType(typeof(UserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation("Retrieving user with ID: {UserId}", id);
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(CreateErrorResponse("User ID cannot be null or empty"));
                }
                
                var result = await _userService.GetUserByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound(CreateErrorResponse($"User with ID {id} not found"));
                }
                
                return Ok(UserApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while retrieving the user"));
            }
        }

        /// <summary>
        /// Retrieves a paginated list of users with optional filtering
        /// </summary>
        /// <param name="request">The request containing pagination, search, and filter parameters</param>
        /// <returns>A paginated list of users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(UserListApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUsersAsync([FromQuery] GetUsersApiRequest request)
        {
            try
            {
                _logger.LogInformation("Retrieving users with filters: Page={Page}, PageSize={PageSize}, SearchTerm={SearchTerm}, RoleFilter={RoleFilter}, ActiveOnly={ActiveOnly}", 
                    request.Page, request.PageSize, request.SearchTerm, request.RoleFilter, request.ActiveOnly);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(CreateErrorResponse("Invalid request", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }
                
                var usersRequest = new GetUsersRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    SearchTerm = request.SearchTerm,
                    RoleFilter = request.RoleFilter,
                    ActiveOnly = request.ActiveOnly
                };
                
                var result = await _userService.GetUsersAsync(usersRequest);
                
                return Ok(UserListApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users with filters: Page={Page}, PageSize={PageSize}", 
                    request.Page, request.PageSize);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while retrieving users"));
            }
        }

        /// <summary>
        /// Retrieves a list of simplified user summaries, optionally filtered by role
        /// </summary>
        /// <param name="roleFilter">Optional filter to only return users with a specific role</param>
        /// <returns>A list of user summaries</returns>
        [HttpGet(ApiRoutes.User.Summaries)]
        [ProducesResponseType(typeof(List<UserSummaryApiResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserSummariesAsync([FromQuery] UserRole? roleFilter = null)
        {
            try
            {
                _logger.LogInformation("Retrieving user summaries with role filter: {RoleFilter}", roleFilter);
                
                var result = await _userService.GetUserSummariesAsync(roleFilter);
                
                var response = result.Select(UserSummaryApiResponse.FromContractResponse).ToList();
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user summaries with role filter: {RoleFilter}", roleFilter);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while retrieving user summaries"));
            }
        }

        /// <summary>
        /// Updates a user's profile information
        /// </summary>
        /// <param name="id">The ID of the user whose profile is being updated</param>
        /// <param name="request">The request containing the updated profile information</param>
        /// <returns>The updated profile details</returns>
        [HttpPut(ApiRoutes.User.Profile)]
        [ProducesResponseType(typeof(UpdateUserProfileApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfileAsync(string id, [FromBody] UpdateUserProfileApiRequest request)
        {
            try
            {
                _logger.LogInformation("Updating profile for user with ID: {UserId}", id);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(CreateErrorResponse("Invalid request", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }
                
                if (string.IsNullOrEmpty(id))
                {
                    return BadRequest(CreateErrorResponse("User ID cannot be null or empty"));
                }
                
                // Ensure the user is authorized to update this profile (self or admin)
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Administrator");
                
                if (currentUserId != id && !isAdmin)
                {
                    return Forbid();
                }
                
                var profileRequest = request.ToContractRequest();
                var result = await _userService.UpdateUserProfileAsync(id, profileRequest);
                
                if (result == null)
                {
                    return NotFound(CreateErrorResponse($"User with ID {id} not found"));
                }
                
                return Ok(UpdateUserProfileApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user with ID: {UserId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while updating the user profile"));
            }
        }

        /// <summary>
        /// Updates a user's assigned roles
        /// </summary>
        /// <param name="request">The request containing the user ID and the roles to assign</param>
        /// <returns>The updated role assignments</returns>
        [HttpPut(ApiRoutes.User.Roles)]
        [ProducesResponseType(typeof(UpdateUserRolesApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateUserRolesAsync([FromBody] UpdateUserRolesApiRequest request)
        {
            try
            {
                _logger.LogInformation("Updating roles for user with ID: {UserId}", request.UserId);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(CreateErrorResponse("Invalid request", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }
                
                var rolesRequest = request.ToContractRequest();
                var result = await _userService.UpdateUserRolesAsync(rolesRequest);
                
                if (result == null)
                {
                    return NotFound(CreateErrorResponse($"User with ID {request.UserId} not found"));
                }
                
                return Ok(UpdateUserRolesApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user with ID: {UserId}", request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while updating user roles"));
            }
        }

        /// <summary>
        /// Activates or deactivates a user account
        /// </summary>
        /// <param name="request">The request containing the user ID and desired active status</param>
        /// <returns>The updated user status</returns>
        [HttpPut(ApiRoutes.User.Status)]
        [ProducesResponseType(typeof(ChangeUserStatusApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> ChangeUserStatusAsync([FromBody] ChangeUserStatusApiRequest request)
        {
            try
            {
                _logger.LogInformation("Changing status for user with ID: {UserId} to {Status}", 
                    request.UserId, request.IsActive ? "Active" : "Inactive");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(CreateErrorResponse("Invalid request", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()));
                }
                
                var statusRequest = request.ToContractRequest();
                var result = await _userService.ChangeUserStatusAsync(statusRequest);
                
                if (result == null)
                {
                    return NotFound(CreateErrorResponse($"User with ID {request.UserId} not found"));
                }
                
                return Ok(ChangeUserStatusApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing status for user with ID: {UserId}", request.UserId);
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while changing user status"));
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's information
        /// </summary>
        /// <returns>The current user's details</returns>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving current user information");
                
                // Get the current user ID from the claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(CreateErrorResponse("User ID claim not found"));
                }
                
                var result = await _userService.GetUserByIdAsync(userId);
                
                if (result == null)
                {
                    return Unauthorized(CreateErrorResponse("User not found"));
                }
                
                return Ok(UserApiResponse.FromContractResponse(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user information");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    CreateErrorResponse("An error occurred while retrieving current user information"));
            }
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errors">Optional list of specific error messages</param>
        /// <returns>A standardized error response object</returns>
        private object CreateErrorResponse(string message, List<string> errors = null)
        {
            return new
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}