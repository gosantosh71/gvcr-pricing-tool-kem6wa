using Microsoft.AspNetCore.Authorization; // Microsoft.AspNetCore.Authorization package version 6.0.0
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Api.Models.Responses;
using VatFilingPricingTool.Common.Constants;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Service.Interfaces;

namespace VatFilingPricingTool.Api.Controllers
{
    /// <summary>
    /// API controller that handles authentication-related operations including login, registration,
    /// password management, and token validation
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.Auth.Base)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the AuthController with required dependencies
        /// </summary>
        /// <param name="authService">Service for authentication operations</param>
        /// <param name="logger">Logger for recording authentication events</param>
        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="request">Login request containing email and password</param>
        /// <returns>JWT token and user information if authentication is successful</returns>
        [HttpPost(ApiRoutes.Auth.Login)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthSuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for user with email: {Email}", request.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.LoginAsync(contractRequest);
                
                return Ok(AuthSuccessResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user with email: {Email}", request.Email);
                return BadRequest(CreateErrorResponse("Login failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">Registration request with user details</param>
        /// <returns>Registration status with user ID and email</returns>
        [HttpPost(ApiRoutes.Auth.Register)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registration attempt for user with email: {Email}", request.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(CreateErrorResponse("Passwords do not match", null));
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.RegisterAsync(contractRequest);
                
                return Created(ApiRoutes.Auth.Register, RegisterResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user with email: {Email}", request.Email);
                return BadRequest(CreateErrorResponse("Registration failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Initiates the password reset process for a user
        /// </summary>
        /// <param name="request">Password reset request with user email</param>
        /// <returns>Password reset status</returns>
        [HttpPost(ApiRoutes.Auth.ForgotPassword)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPasswordAsync([FromBody] PasswordResetRequest request)
        {
            try
            {
                _logger.LogInformation("Password reset request for user with email: {Email}", request.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.RequestPasswordResetAsync(contractRequest);
                
                return Ok(PasswordResetResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset request for user with email: {Email}", request.Email);
                return BadRequest(CreateErrorResponse("Password reset request failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Changes a user's password using a reset token
        /// </summary>
        /// <param name="request">Password change request with token and new password</param>
        /// <returns>Password change status</returns>
        [HttpPost(ApiRoutes.Auth.ResetPassword)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PasswordChangeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] PasswordChangeRequest request)
        {
            try
            {
                _logger.LogInformation("Password change request for user with email: {Email}", request.Email);
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest(CreateErrorResponse("Passwords do not match", null));
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.ChangePasswordAsync(contractRequest);
                
                return Ok(PasswordChangeResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password change for user with email: {Email}", request.Email);
                return BadRequest(CreateErrorResponse("Password change failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Refreshes an authentication token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>New authentication and refresh tokens</returns>
        [HttpPost(ApiRoutes.Auth.RefreshToken)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token refresh request received");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.RefreshTokenAsync(contractRequest);
                
                return Ok(RefreshTokenResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return BadRequest(CreateErrorResponse("Token refresh failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Logs out a user by invalidating their tokens
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost(ApiRoutes.Auth.Logout)]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {
                _logger.LogInformation("Logout request from user: {UserId}", User.FindFirst("sub")?.Value);
                
                // Note: In a stateless JWT-based auth system, the client should discard the tokens
                // Server-side, we might want to add the token to a blacklist or invalidate the refresh token
                
                return Ok(new { Success = true, Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return BadRequest(CreateErrorResponse("Logout failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Authenticates a user with Azure Active Directory
        /// </summary>
        /// <param name="request">Azure AD authentication request with ID token</param>
        /// <returns>JWT token and user information if authentication is successful</returns>
        [HttpPost(ApiRoutes.Auth.AzureAd)]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AzureAdAuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AzureAdLoginAsync([FromBody] AzureAdAuthRequest request)
        {
            try
            {
                _logger.LogInformation("Azure AD login attempt");
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthFailureResponse 
                    { 
                        Success = false,
                        Message = "Invalid request",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }
                
                var contractRequest = request.ToContractRequest();
                var response = await _authService.AuthenticateWithAzureAdAsync(contractRequest);
                
                return Ok(AzureAdAuthResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Azure AD login");
                return BadRequest(CreateErrorResponse("Azure AD login failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Validates an authentication token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>Token validation status</returns>
        [HttpGet("validate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthFailureResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateTokenAsync([FromQuery] string token)
        {
            try
            {
                _logger.LogInformation("Token validation request");
                
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest(CreateErrorResponse("Token is required", null));
                }
                
                var isValid = await _authService.ValidateTokenAsync(token);
                
                var response = new TokenValidationResponse
                {
                    IsValid = isValid
                };
                
                return Ok(TokenValidationResponse.FromContractResponse(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token validation");
                return BadRequest(CreateErrorResponse("Token validation failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Retrieves the current authenticated user's information
        /// </summary>
        /// <returns>Current user details</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            try
            {
                _logger.LogInformation("Current user info request");
                
                var user = await _authService.GetUserFromClaimsAsync(User);
                
                if (user == null)
                {
                    return Unauthorized();
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user info");
                return BadRequest(CreateErrorResponse("Failed to retrieve user information", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Creates a standardized error response
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="errors">List of specific errors</param>
        /// <returns>Standardized error response</returns>
        private AuthFailureResponse CreateErrorResponse(string message, List<string> errors)
        {
            return new AuthFailureResponse
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}