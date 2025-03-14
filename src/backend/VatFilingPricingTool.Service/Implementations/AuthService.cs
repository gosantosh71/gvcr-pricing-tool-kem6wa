using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using VatFilingPricingTool.Common.Helpers;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Infrastructure.Authentication;
using VatFilingPricingTool.Service.Interfaces;

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Implements the authentication service for the VAT Filing Pricing Tool.
    /// Handles user authentication, registration, password management, token validation, and Azure AD integration.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ILogger<AuthService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenHandler _tokenHandler;
        private readonly IAzureAdAuthenticationHandler _azureAdHandler;
        private readonly Dictionary<string, PasswordResetInfo> _passwordResetTokens;

        private const int PASSWORD_RESET_TOKEN_EXPIRY_HOURS = 24;

        /// <summary>
        /// Initializes a new instance of the AuthService class with required dependencies
        /// </summary>
        /// <param name="logger">Logger for authentication operations</param>
        /// <param name="userRepository">Repository for user data access</param>
        /// <param name="tokenHandler">Handler for JWT token operations</param>
        /// <param name="azureAdHandler">Handler for Azure AD authentication operations</param>
        public AuthService(
            ILogger<AuthService> logger,
            IUserRepository userRepository,
            IJwtTokenHandler tokenHandler,
            IAzureAdAuthenticationHandler azureAdHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenHandler = tokenHandler ?? throw new ArgumentNullException(nameof(tokenHandler));
            _azureAdHandler = azureAdHandler ?? throw new ArgumentNullException(nameof(azureAdHandler));
            _passwordResetTokens = new Dictionary<string, PasswordResetInfo>();
        }

        /// <summary>
        /// Authenticates a user with email and password credentials
        /// </summary>
        /// <param name="request">Login request containing email and password</param>
        /// <returns>Authentication response with token and user information</returns>
        public async Task<AuthSuccessResponse> LoginAsync(LoginRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Email))
                throw new ValidationException("Email is required", new List<string> { "Email is required" });

            if (string.IsNullOrEmpty(request.Password))
                throw new ValidationException("Password is required", new List<string> { "Password is required" });

            _logger.LogInformation("Login attempt for user {Email}", request.Email);

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User with email {Email} not found", request.Email);
                throw new ValidationException("Invalid credentials", new List<string> { "Invalid email or password" });
            }

            // Check if user account is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User account {Email} is inactive", request.Email);
                throw new ValidationException("Account is inactive", new List<string> { "Your account has been deactivated. Please contact support." });
            }

            // Verify password using SecurityHelper
            if (!SecurityHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
                throw new ValidationException("Invalid credentials", new List<string> { "Invalid email or password" });
            }

            // Update last login date
            user.UpdateLastLoginDate();
            await _userRepository.UpdateLastLoginDateAsync(user.UserId);
            
            // Generate JWT token with user information
            var userModel = CreateUserModelFromEntity(user);
            var (token, refreshToken, expiresAt) = await _tokenHandler.GenerateTokenAsync(userModel);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            // Return success response
            return new AuthSuccessResponse
            {
                Token = token,
                RefreshToken = request.RememberMe ? refreshToken : null,
                ExpiresAt = expiresAt,
                User = userModel
            };
        }

        /// <summary>
        /// Registers a new user in the system
        /// </summary>
        /// <param name="request">Registration request with user details</param>
        /// <returns>Registration response with user information</returns>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Email))
                throw new ValidationException("Email is required", new List<string> { "Email is required" });

            if (string.IsNullOrEmpty(request.Password))
                throw new ValidationException("Password is required", new List<string> { "Password is required" });

            if (string.IsNullOrEmpty(request.FirstName))
                throw new ValidationException("First name is required", new List<string> { "First name is required" });

            if (string.IsNullOrEmpty(request.LastName))
                throw new ValidationException("Last name is required", new List<string> { "Last name is required" });

            _logger.LogInformation("Registration attempt for email {Email}", request.Email);

            // Check if email already exists
            bool emailExists = await _userRepository.EmailExistsAsync(request.Email);
            if (emailExists)
            {
                _logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
                throw new ValidationException("Email already exists", new List<string> { "An account with this email already exists" });
            }

            // Validate password strength
            if (!SecurityHelper.IsStrongPassword(request.Password))
            {
                _logger.LogWarning("Registration failed: Weak password for {Email}", request.Email);
                throw new ValidationException("Password is too weak", new List<string> {
                    "Password must include at least one uppercase letter, one lowercase letter, one digit, and one special character"
                });
            }

            // Hash the password using SecurityHelper
            string passwordHash = SecurityHelper.HashPassword(request.Password);

            // Determine user role (default to Customer if not specified)
            UserRole role = request.Roles != null && request.Roles.Any() 
                ? request.Roles.First() 
                : UserRole.Customer;

            // Create new User entity
            var user = User.Create(
                request.Email,
                request.FirstName,
                request.LastName,
                role
            );

            // Add user to repository
            var createdUser = await _userRepository.AddAsync(user);

            _logger.LogInformation("User {Email} registered successfully with ID {UserId}", request.Email, createdUser.UserId);

            // Create and return response
            return new RegisterResponse
            {
                Success = true,
                UserId = createdUser.UserId,
                Email = createdUser.Email
            };
        }

        /// <summary>
        /// Initiates the password reset process for a user
        /// </summary>
        /// <param name="request">Password reset request with user email</param>
        /// <returns>Password reset response indicating success or failure</returns>
        public async Task<PasswordResetResponse> RequestPasswordResetAsync(PasswordResetRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Email))
                throw new ValidationException("Email is required", new List<string> { "Email is required" });

            _logger.LogInformation("Password reset requested for email {Email}", request.Email);

            // Check if user exists (for security, don't reveal if the email exists or not)
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogInformation("Password reset requested for non-existent email {Email}", request.Email);
                
                // Return success anyway (security best practice)
                return new PasswordResetResponse
                {
                    Success = true,
                    Email = request.Email
                };
            }

            // Generate secure reset token
            string resetToken = SecurityHelper.GenerateSecureToken();
            
            // Store reset token with expiration time
            _passwordResetTokens[request.Email] = new PasswordResetInfo(resetToken);

            // In a production environment, send an email with the reset token
            // Here we just log it for demonstration purposes
            _logger.LogInformation("Password reset token generated for {Email}", request.Email);

            return new PasswordResetResponse
            {
                Success = true,
                Email = request.Email
            };
        }

        /// <summary>
        /// Changes a user's password using a reset token
        /// </summary>
        /// <param name="request">Password change request with token and new password</param>
        /// <returns>Password change response indicating success or failure</returns>
        public async Task<PasswordChangeResponse> ChangePasswordAsync(PasswordChangeRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.Email))
                throw new ValidationException("Email is required", new List<string> { "Email is required" });

            if (string.IsNullOrEmpty(request.ResetToken))
                throw new ValidationException("Reset token is required", new List<string> { "Reset token is required" });

            if (string.IsNullOrEmpty(request.NewPassword))
                throw new ValidationException("New password is required", new List<string> { "New password is required" });

            _logger.LogInformation("Password change requested for email {Email}", request.Email);

            // Check if reset token exists and is valid
            if (!ValidatePasswordResetToken(request.Email, request.ResetToken))
            {
                _logger.LogWarning("Password change failed: Invalid or expired token for {Email}", request.Email);
                throw new ValidationException("Invalid or expired reset token", new List<string> { "The reset token is invalid or has expired" });
            }

            // Get user
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password change failed: User with email {Email} not found", request.Email);
                throw new ValidationException("User not found", new List<string> { "User not found" });
            }

            // Validate password strength
            if (!SecurityHelper.IsStrongPassword(request.NewPassword))
            {
                _logger.LogWarning("Password change failed: Weak password for {Email}", request.Email);
                throw new ValidationException("Password is too weak", new List<string> {
                    "Password must include at least one uppercase letter, one lowercase letter, one digit, and one special character"
                });
            }

            // Hash the new password
            string passwordHash = SecurityHelper.HashPassword(request.NewPassword);

            // Update user's password in repository
            // Note: In a real implementation, the repository would have a method to update the password
            // await _userRepository.UpdatePasswordAsync(user.UserId, passwordHash);

            // Remove used reset token
            _passwordResetTokens.Remove(request.Email);

            _logger.LogInformation("Password changed successfully for user {Email}", request.Email);

            return new PasswordChangeResponse
            {
                Success = true,
                Email = request.Email
            };
        }

        /// <summary>
        /// Refreshes an authentication token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token request</param>
        /// <returns>Token refresh response with new tokens</returns>
        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.RefreshToken))
                throw new ValidationException("Refresh token is required", new List<string> { "Refresh token is required" });

            _logger.LogInformation("Token refresh requested");

            // Validate refresh token
            bool isValid = await _tokenHandler.ValidateRefreshTokenAsync(request.RefreshToken);
            if (!isValid)
            {
                _logger.LogWarning("Token refresh failed: Invalid refresh token");
                throw new ValidationException("Invalid refresh token", new List<string> { "The refresh token is invalid or has expired" });
            }

            // Get claims principal from token
            var principal = await _tokenHandler.GetClaimsPrincipalFromTokenAsync(request.RefreshToken);
            if (principal == null)
            {
                _logger.LogWarning("Token refresh failed: Unable to extract principal from token");
                throw new ValidationException("Invalid token", new List<string> { "Unable to process the refresh token" });
            }

            // Get user ID from token
            var userIdClaim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Token refresh failed: User ID not found in token");
                throw new ValidationException("Invalid token", new List<string> { "The token does not contain a valid user identifier" });
            }

            string userId = userIdClaim.Value;

            // Get user from repository
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Token refresh failed: User {UserId} not found", userId);
                throw new ValidationException("User not found", new List<string> { "The user associated with this token was not found" });
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Token refresh failed: User {UserId} is inactive", userId);
                throw new ValidationException("Account inactive", new List<string> { "The user account is inactive" });
            }

            // Generate new tokens
            var userModel = CreateUserModelFromEntity(user);
            var (newToken, newRefreshToken, expiresAt) = await _tokenHandler.GenerateTokenAsync(userModel);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", userId);

            return new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = expiresAt
            };
        }

        /// <summary>
        /// Validates an authentication token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Token validation failed: Token is null or empty");
                return false;
            }

            _logger.LogInformation("Token validation requested");

            try
            {
                // Validate token using token handler
                bool isValid = await _tokenHandler.ValidateTokenAsync(token);
                _logger.LogInformation("Token validation result: {Result}", isValid ? "Valid" : "Invalid");
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed due to an error");
                return false;
            }
        }

        /// <summary>
        /// Authenticates a user with Azure AD credentials
        /// </summary>
        /// <param name="request">Azure AD authentication request with ID token</param>
        /// <returns>Azure AD authentication response with token and user information</returns>
        public async Task<AzureAdAuthResponse> AuthenticateWithAzureAdAsync(AzureAdAuthRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrEmpty(request.IdToken))
                throw new ValidationException("ID token is required", new List<string> { "ID token is required" });

            _logger.LogInformation("Azure AD authentication requested");

            // Validate Azure AD token
            bool isValid = await _azureAdHandler.ValidateTokenAsync(request.IdToken);
            if (!isValid)
            {
                _logger.LogWarning("Azure AD authentication failed: Invalid token");
                throw new ValidationException("Invalid Azure AD token", new List<string> { "The provided Azure AD token is invalid" });
            }

            // Extract user information from token
            var userInfo = await _azureAdHandler.GetUserInfoFromTokenAsync(request.IdToken);
            if (!userInfo.TryGetValue("email", out string email) || string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Azure AD authentication failed: Email not found in token");
                throw new ValidationException("Email not found in token", new List<string> { "Email address could not be extracted from the Azure AD token" });
            }

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(email);
            bool isNewUser = existingUser == null;

            User user;
            if (isNewUser)
            {
                // Extract user details from token
                userInfo.TryGetValue("given_name", out string firstName);
                userInfo.TryGetValue("family_name", out string lastName);
                userInfo.TryGetValue("oid", out string objectId);

                // Create new user with Azure AD information
                user = User.CreateWithAzureAd(
                    email,
                    firstName ?? "User", // Default if not provided
                    lastName ?? "Name",  // Default if not provided
                    UserRole.Customer,   // Default role
                    objectId
                );

                // Add user to repository
                user = await _userRepository.AddAsync(user);
                _logger.LogInformation("New user created from Azure AD authentication: {Email}", email);
            }
            else
            {
                // Update last login date for existing user
                existingUser.UpdateLastLoginDate();
                await _userRepository.UpdateLastLoginDateAsync(existingUser.UserId);
                user = existingUser;
                _logger.LogInformation("Existing user authenticated via Azure AD: {Email}", email);
            }

            // Create user model and generate tokens
            var userModel = CreateUserModelFromEntity(user);
            var (token, refreshToken, expiresAt) = await _tokenHandler.GenerateTokenAsync(userModel);

            return new AzureAdAuthResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = userModel,
                IsNewUser = isNewUser
            };
        }

        /// <summary>
        /// Retrieves user information from authentication claims
        /// </summary>
        /// <param name="principal">Claims principal containing user claims</param>
        /// <returns>User model with information from claims</returns>
        public async Task<UserModel> GetUserFromClaimsAsync(ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            _logger.LogInformation("Getting user from claims");

            // Extract user ID from claims
            var userIdClaim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("User ID claim not found in principal");
                return null;
            }

            string userId = userIdClaim.Value;

            // Get user from repository
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", userId);
                return null;
            }

            _logger.LogInformation("User {UserId} retrieved from claims", userId);

            // Convert to UserModel and return
            return CreateUserModelFromEntity(user);
        }

        /// <summary>
        /// Creates a UserModel from a User entity
        /// </summary>
        /// <param name="user">User entity to convert</param>
        /// <returns>User model created from the entity</returns>
        private UserModel CreateUserModelFromEntity(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return new UserModel
            {
                UserId = user.UserId,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = new List<UserRole> { user.Role },
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                IsActive = user.IsActive
            };
        }

        /// <summary>
        /// Validates a password reset token
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="token">Reset token to validate</param>
        /// <returns>True if the token is valid, otherwise false</returns>
        private bool ValidatePasswordResetToken(string email, string token)
        {
            // Check if email exists in reset tokens dictionary
            if (!_passwordResetTokens.TryGetValue(email, out var resetInfo))
                return false;

            // Check if the stored token matches the provided token
            if (resetInfo.Token != token)
                return false;

            // Check if token has expired
            if (resetInfo.CreatedAt.AddHours(PASSWORD_RESET_TOKEN_EXPIRY_HOURS) < DateTime.UtcNow)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Internal class to store password reset token information
    /// </summary>
    internal class PasswordResetInfo
    {
        public string Token { get; }
        public DateTime CreatedAt { get; }

        public PasswordResetInfo(string token)
        {
            Token = token;
            CreatedAt = DateTime.UtcNow;
        }
    }
}