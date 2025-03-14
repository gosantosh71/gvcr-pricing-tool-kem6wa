#nullable enable
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq;
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Infrastructure.Authentication;
using VatFilingPricingTool.Service.Implementations;
using Xunit; // Testing framework
using FluentAssertions; // For more readable assertions in tests
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the AuthService class
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtTokenHandler> _mockTokenHandler;
        private readonly Mock<IAzureAdAuthenticationHandler> _mockAzureAdHandler;
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the AuthServiceTests class
        /// </summary>
        public AuthServiceTests()
        {
            _mockLogger = new Mock<ILogger<AuthService>>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockTokenHandler = new Mock<IJwtTokenHandler>();
            _mockAzureAdHandler = new Mock<IAzureAdAuthenticationHandler>();

            _authService = new AuthService(
                _mockLogger.Object,
                _mockUserRepository.Object,
                _mockTokenHandler.Object,
                _mockAzureAdHandler.Object);
        }

        /// <summary>
        /// Tests that LoginAsync returns a successful response when valid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsAuthSuccessResponse()
        {
            // Arrange: Create a login request with valid email and password
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "P@$$wOrd" };

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginRequest.Email)).ReturnsAsync(mockUser);

            // Arrange: Set up mock token handler to return a valid token
            _mockTokenHandler.Setup(handler => handler.GenerateTokenAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(("token", "refreshToken", DateTime.UtcNow.AddMinutes(60)));

            // Act: Call _authService.LoginAsync with the login request
            var response = await _authService.LoginAsync(loginRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response contains a valid token
            Assert.NotNull(response.Token);

            // Assert: Verify the response contains the correct user information
            Assert.Equal(mockUser.Email, response.User.Email);

            // Assert: Verify the user repository was called to update the last login date
            _mockUserRepository.Verify(repo => repo.UpdateLastLoginDateAsync(It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that LoginAsync throws an exception when invalid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ThrowsException()
        {
            // Arrange: Create a login request with invalid email and password
            var loginRequest = new LoginRequest { Email = "invalid@example.com", Password = "wrongPassword" };

            // Arrange: Set up mock user repository to return null (user not found)
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginRequest.Email)).ReturnsAsync((User)null);

            // Act & Assert: Verify that calling _authService.LoginAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.LoginAsync(loginRequest));

            // Assert: Verify the exception message indicates invalid credentials
            Assert.Equal("Invalid credentials", exception.Message);
        }

        /// <summary>
        /// Tests that LoginAsync throws an exception when the user account is inactive
        /// </summary>
        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsException()
        {
            // Arrange: Create a login request with valid email and password
            var loginRequest = new LoginRequest { Email = "inactive@example.com", Password = "P@$$wOrd" };

            // Arrange: Set up mock user repository to return an inactive user
            var mockUser = User.Create("inactive@example.com", "Inactive", "User", UserRole.Customer);
            mockUser.Deactivate();
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginRequest.Email)).ReturnsAsync(mockUser);

            // Act & Assert: Verify that calling _authService.LoginAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.LoginAsync(loginRequest));

            // Assert: Verify the exception message indicates the account is inactive
            Assert.Equal("Account is inactive", exception.Message);
        }

        /// <summary>
        /// Tests that RegisterAsync returns a successful response when valid registration data is provided
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange: Create a registration request with valid data
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "P@$$wOrd123",
                ConfirmPassword = "P@$$wOrd123",
                FirstName = "New",
                LastName = "User"
            };

            // Arrange: Set up mock user repository to indicate email doesn't exist
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerRequest.Email)).ReturnsAsync(false);

            // Arrange: Set up mock user repository to successfully add the user
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Act: Call _authService.RegisterAsync with the registration request
            var response = await _authService.RegisterAsync(registerRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response indicates success
            Assert.True(response.Success);

            // Assert: Verify the response contains the correct user ID and email
            Assert.NotNull(response.UserId);
            Assert.Equal(registerRequest.Email, response.Email);

            // Assert: Verify the user repository was called to add the user
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Tests that RegisterAsync throws an exception when the email already exists
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ThrowsException()
        {
            // Arrange: Create a registration request with valid data
            var registerRequest = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "P@$$wOrd",
                ConfirmPassword = "P@$$wOrd",
                FirstName = "Existing",
                LastName = "User"
            };

            // Arrange: Set up mock user repository to indicate email already exists
            _mockUserRepository.Setup(repo => repo.EmailExistsAsync(registerRequest.Email)).ReturnsAsync(true);

            // Act & Assert: Verify that calling _authService.RegisterAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.RegisterAsync(registerRequest));

            // Assert: Verify the exception message indicates the email already exists
            Assert.Equal("Email already exists", exception.Message);
        }

        /// <summary>
        /// Tests that RequestPasswordResetAsync returns a successful response when a valid email is provided
        /// </summary>
        [Fact]
        public async Task RequestPasswordResetAsync_WithValidEmail_ReturnsSuccessResponse()
        {
            // Arrange: Create a password reset request with a valid email
            var resetRequest = new PasswordResetRequest { Email = "test@example.com" };

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(resetRequest.Email)).ReturnsAsync(mockUser);

            // Act: Call _authService.RequestPasswordResetAsync with the reset request
            var response = await _authService.RequestPasswordResetAsync(resetRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response indicates success
            Assert.True(response.Success);

            // Assert: Verify the response contains the correct email
            Assert.Equal(resetRequest.Email, response.Email);
        }

        /// <summary>
        /// Tests that RequestPasswordResetAsync returns a successful response even when the email doesn't exist (security best practice)
        /// </summary>
        [Fact]
        public async Task RequestPasswordResetAsync_WithNonExistentEmail_ReturnsSuccessResponse()
        {
            // Arrange: Create a password reset request with a non-existent email
            var resetRequest = new PasswordResetRequest { Email = "nonexistent@example.com" };

            // Arrange: Set up mock user repository to return null (user not found)
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(resetRequest.Email)).ReturnsAsync((User)null);

            // Act: Call _authService.RequestPasswordResetAsync with the reset request
            var response = await _authService.RequestPasswordResetAsync(resetRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response indicates success
            Assert.True(response.Success);

            // Assert: Verify the response contains the email (even though it doesn't exist)
            Assert.Equal(resetRequest.Email, response.Email);
        }

        /// <summary>
        /// Tests that ChangePasswordAsync returns a successful response when a valid token is provided
        /// </summary>
        [Fact]
        public async Task ChangePasswordAsync_WithValidToken_ReturnsSuccessResponse()
        {
            // Arrange: Create a password change request with valid data
            var changeRequest = new PasswordChangeRequest
            {
                Email = "test@example.com",
                ResetToken = "validToken",
                NewPassword = "NewP@$$wOrd",
                ConfirmPassword = "NewP@$$wOrd"
            };

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(changeRequest.Email)).ReturnsAsync(mockUser);

            // Arrange: First call RequestPasswordResetAsync to generate a valid token
            var resetRequest = new PasswordResetRequest { Email = "test@example.com" };
            await _authService.RequestPasswordResetAsync(resetRequest);

            // Act: Call _authService.ChangePasswordAsync with the change request
            var response = await _authService.ChangePasswordAsync(changeRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response indicates success
            Assert.True(response.Success);

            // Assert: Verify the response contains the correct email
            Assert.Equal(changeRequest.Email, response.Email);

            // Assert: Verify the user repository was called to update the user's password
            //_mockUserRepository.Verify(repo => repo.UpdatePasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that ChangePasswordAsync throws an exception when an invalid token is provided
        /// </summary>
        [Fact]
        public async Task ChangePasswordAsync_WithInvalidToken_ThrowsException()
        {
            // Arrange: Create a password change request with an invalid token
            var changeRequest = new PasswordChangeRequest
            {
                Email = "test@example.com",
                ResetToken = "invalidToken",
                NewPassword = "NewP@$$wOrd",
                ConfirmPassword = "NewP@$$wOrd"
            };

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(changeRequest.Email)).ReturnsAsync(mockUser);

            // Act & Assert: Verify that calling _authService.ChangePasswordAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.ChangePasswordAsync(changeRequest));

            // Assert: Verify the exception message indicates the token is invalid
            Assert.Equal("Invalid or expired reset token", exception.Message);
        }

        /// <summary>
        /// Tests that RefreshTokenAsync returns new tokens when a valid refresh token is provided
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange: Create a refresh token request with a valid refresh token
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "validRefreshToken" };

            // Arrange: Set up mock token handler to validate the refresh token and return a user ID
            _mockTokenHandler.Setup(handler => handler.ValidateRefreshTokenAsync(refreshTokenRequest.RefreshToken)).ReturnsAsync(true);
            _mockTokenHandler.Setup(handler => handler.GetClaimsPrincipalFromTokenAsync(refreshTokenRequest.RefreshToken))
                .ReturnsAsync(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "testUserId") })));

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync("testUserId")).ReturnsAsync(mockUser);

            // Arrange: Set up mock token handler to generate new tokens
            _mockTokenHandler.Setup(handler => handler.GenerateTokenAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(("newToken", "newRefreshToken", DateTime.UtcNow.AddMinutes(60)));

            // Act: Call _authService.RefreshTokenAsync with the refresh request
            var response = await _authService.RefreshTokenAsync(refreshTokenRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response contains new tokens
            Assert.NotNull(response.Token);
            Assert.NotNull(response.RefreshToken);

            // Assert: Verify the response contains the correct expiration time
            Assert.True(response.ExpiresAt > DateTime.UtcNow);
        }

        /// <summary>
        /// Tests that RefreshTokenAsync throws an exception when an invalid refresh token is provided
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
        {
            // Arrange: Create a refresh token request with an invalid refresh token
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalidRefreshToken" };

            // Arrange: Set up mock token handler to fail validation
            _mockTokenHandler.Setup(handler => handler.ValidateRefreshTokenAsync(refreshTokenRequest.RefreshToken)).ReturnsAsync(false);

            // Act & Assert: Verify that calling _authService.RefreshTokenAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.RefreshTokenAsync(refreshTokenRequest));

            // Assert: Verify the exception message indicates the token is invalid
            Assert.Equal("Invalid refresh token", exception.Message);
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns true when a valid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
        {
            // Arrange: Create a valid token string
            var validToken = "validTokenString";

            // Arrange: Set up mock token handler to validate the token successfully
            _mockTokenHandler.Setup(handler => handler.ValidateTokenAsync(validToken)).ReturnsAsync(true);

            // Act: Call _authService.ValidateTokenAsync with the token
            var result = await _authService.ValidateTokenAsync(validToken);

            // Assert: Verify the result is true
            Assert.True(result);
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns false when an invalid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
        {
            // Arrange: Create an invalid token string
            var invalidToken = "invalidTokenString";

            // Arrange: Set up mock token handler to fail validation
            _mockTokenHandler.Setup(handler => handler.ValidateTokenAsync(invalidToken)).ReturnsAsync(false);

            // Act: Call _authService.ValidateTokenAsync with the token
            var result = await _authService.ValidateTokenAsync(invalidToken);

            // Assert: Verify the result is false
            Assert.False(result);
        }

        /// <summary>
        /// Tests that AuthenticateWithAzureAdAsync returns a successful response when a valid Azure AD token is provided
        /// </summary>
        [Fact]
        public async Task AuthenticateWithAzureAdAsync_WithValidToken_ReturnsAuthResponse()
        {
            // Arrange: Create an Azure AD auth request with a valid token
            var azureAdAuthRequest = new AzureAdAuthRequest { IdToken = "validAzureAdToken" };

            // Arrange: Set up mock Azure AD handler to validate the token successfully
            _mockAzureAdHandler.Setup(handler => handler.ValidateTokenAsync(azureAdAuthRequest.IdToken)).ReturnsAsync(true);

            // Arrange: Set up mock Azure AD handler to return user information
            var userInfo = new Dictionary<string, string> { { "email", "test@example.com" } };
            _mockAzureAdHandler.Setup(handler => handler.GetUserInfoFromTokenAsync(azureAdAuthRequest.IdToken)).ReturnsAsync(userInfo);

            // Arrange: Set up mock user repository to indicate the user already exists
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("test@example.com")).ReturnsAsync(mockUser);

            // Arrange: Set up mock token handler to generate tokens
            _mockTokenHandler.Setup(handler => handler.GenerateTokenAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(("token", "refreshToken", DateTime.UtcNow.AddMinutes(60)));

            // Act: Call _authService.AuthenticateWithAzureAdAsync with the request
            var response = await _authService.AuthenticateWithAzureAdAsync(azureAdAuthRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response contains valid tokens
            Assert.NotNull(response.Token);
            Assert.NotNull(response.RefreshToken);

            // Assert: Verify the response contains the correct user information
            Assert.Equal("test@example.com", response.User.Email);

            // Assert: Verify the response indicates the user is not new (isNewUser = false)
            Assert.False(response.IsNewUser);
        }

        /// <summary>
        /// Tests that AuthenticateWithAzureAdAsync creates a new user and returns a successful response when a valid Azure AD token is provided for a new user
        /// </summary>
        [Fact]
        public async Task AuthenticateWithAzureAdAsync_WithNewUser_CreatesUserAndReturnsAuthResponse()
        {
            // Arrange: Create an Azure AD auth request with a valid token
            var azureAdAuthRequest = new AzureAdAuthRequest { IdToken = "validAzureAdToken" };

            // Arrange: Set up mock Azure AD handler to validate the token successfully
            _mockAzureAdHandler.Setup(handler => handler.ValidateTokenAsync(azureAdAuthRequest.IdToken)).ReturnsAsync(true);

            // Arrange: Set up mock Azure AD handler to return user information
            var userInfo = new Dictionary<string, string> { { "email", "newuser@example.com" } };
            _mockAzureAdHandler.Setup(handler => handler.GetUserInfoFromTokenAsync(azureAdAuthRequest.IdToken)).ReturnsAsync(userInfo);

            // Arrange: Set up mock user repository to indicate the user doesn't exist
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync("newuser@example.com")).ReturnsAsync((User)null);

            // Arrange: Set up mock user repository to successfully add the user
            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync((User user) => user);

            // Arrange: Set up mock token handler to generate tokens
            _mockTokenHandler.Setup(handler => handler.GenerateTokenAsync(It.IsAny<UserModel>()))
                .ReturnsAsync(("token", "refreshToken", DateTime.UtcNow.AddMinutes(60)));

            // Act: Call _authService.AuthenticateWithAzureAdAsync with the request
            var response = await _authService.AuthenticateWithAzureAdAsync(azureAdAuthRequest);

            // Assert: Verify the response is not null
            Assert.NotNull(response);

            // Assert: Verify the response contains valid tokens
            Assert.NotNull(response.Token);
            Assert.NotNull(response.RefreshToken);

            // Assert: Verify the response contains the correct user information
            Assert.Equal("newuser@example.com", response.User.Email);

            // Assert: Verify the response indicates the user is new (isNewUser = true)
            Assert.True(response.IsNewUser);

            // Assert: Verify the user repository was called to add the user
            _mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Once);
        }

        /// <summary>
        /// Tests that AuthenticateWithAzureAdAsync throws an exception when an invalid Azure AD token is provided
        /// </summary>
        [Fact]
        public async Task AuthenticateWithAzureAdAsync_WithInvalidToken_ThrowsException()
        {
            // Arrange: Create an Azure AD auth request with an invalid token
            var azureAdAuthRequest = new AzureAdAuthRequest { IdToken = "invalidAzureAdToken" };

            // Arrange: Set up mock Azure AD handler to fail validation
            _mockAzureAdHandler.Setup(handler => handler.ValidateTokenAsync(azureAdAuthRequest.IdToken)).ReturnsAsync(false);

            // Act & Assert: Verify that calling _authService.AuthenticateWithAzureAdAsync throws an exception
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _authService.AuthenticateWithAzureAdAsync(azureAdAuthRequest));

            // Assert: Verify the exception message indicates the token is invalid
            Assert.Equal("Invalid Azure AD token", exception.Message);
        }

        /// <summary>
        /// Tests that GetUserFromClaimsAsync returns a user model when valid claims are provided
        /// </summary>
        [Fact]
        public async Task GetUserFromClaimsAsync_WithValidClaims_ReturnsUserModel()
        {
            // Arrange: Create a ClaimsPrincipal with a valid sub claim containing a user ID
            var userId = "testUserId";
            var claims = new List<Claim> { new Claim("sub", userId) };
            var claimsIdentity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Arrange: Set up mock user repository to return a valid user
            var mockUser = User.Create("test@example.com", "Test", "User", UserRole.Customer);
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(mockUser);

            // Act: Call _authService.GetUserFromClaimsAsync with the claims principal
            var result = await _authService.GetUserFromClaimsAsync(claimsPrincipal);

            // Assert: Verify the result is not null
            Assert.NotNull(result);

            // Assert: Verify the result contains the correct user information
            Assert.Equal(mockUser.Email, result.Email);
        }

        /// <summary>
        /// Tests that GetUserFromClaimsAsync returns null when invalid claims are provided
        /// </summary>
        [Fact]
        public async Task GetUserFromClaimsAsync_WithInvalidClaims_ReturnsNull()
        {
            // Arrange: Create a ClaimsPrincipal without a sub claim
            var claims = new List<Claim>();
            var claimsIdentity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Act: Call _authService.GetUserFromClaimsAsync with the claims principal
            var result = await _authService.GetUserFromClaimsAsync(claimsPrincipal);

            // Assert: Verify the result is null
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetUserFromClaimsAsync returns null when the user ID in the claims doesn't exist
        /// </summary>
        [Fact]
        public async Task GetUserFromClaimsAsync_WithNonExistentUser_ReturnsNull()
        {
            // Arrange: Create a ClaimsPrincipal with a valid sub claim containing a non-existent user ID
            var userId = "nonExistentUserId";
            var claims = new List<Claim> { new Claim("sub", userId) };
            var claimsIdentity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Arrange: Set up mock user repository to return null (user not found)
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User)null);

            // Act: Call _authService.GetUserFromClaimsAsync with the claims principal
            var result = await _authService.GetUserFromClaimsAsync(claimsPrincipal);

            // Assert: Verify the result is null
            Assert.Null(result);
        }
    }
}