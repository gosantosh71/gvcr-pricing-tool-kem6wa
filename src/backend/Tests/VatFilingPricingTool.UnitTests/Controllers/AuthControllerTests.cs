using Microsoft.AspNetCore.Http; // Microsoft.AspNetCore.Http package version 6.0.0
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Api.Controllers;
using VatFilingPricingTool.Api.Models.Requests;
using VatFilingPricingTool.Api.Models.Responses;
using VatFilingPricingTool.Contracts.V1.Models;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.UnitTests.Helpers;
using Xunit; // Xunit package version 2.4.2
using FluentAssertions; // FluentAssertions package version 6.7.0

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Test class for the AuthController, containing unit tests for all authentication-related endpoints
    /// </summary>
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly AuthController _controller;

        /// <summary>
        /// Initializes a new instance of the AuthControllerTests class with mocked dependencies
        /// </summary>
        public AuthControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests that LoginAsync returns OK with a token when valid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsOkWithToken()
        {
            // Arrange: Create a login request with valid email and password
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "P@$$wOrd" };

            // Arrange: Create a mock successful authentication response with token
            var authSuccessResponse = new AuthSuccessResponse
            {
                Token = "test_token",
                RefreshToken = "test_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserModel { UserId = TestHelpers.GetRandomUserId(), Email = "test@example.com", FirstName = "Test", LastName = "User" }
            };

            // Arrange: Setup _mockAuthService.LoginAsync to return the mock response
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<Contracts.V1.Requests.LoginRequest>()))
                .ReturnsAsync(authSuccessResponse);

            // Act: Call _controller.LoginAsync with the login request
            var result = await _controller.LoginAsync(loginRequest);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is AuthSuccessResponse with expected token
            okResult.Value.Should().BeOfType<AuthSuccessResponse>();
            var responseValue = okResult.Value as AuthSuccessResponse;
            responseValue.Token.Should().Be("test_token");

            // Assert: Verify _mockAuthService.LoginAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.LoginAsync(It.Is<Contracts.V1.Requests.LoginRequest>(r => r.Email == loginRequest.Email)), Times.Once);
        }

        /// <summary>
        /// Tests that LoginAsync returns BadRequest when invalid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_InvalidCredentials_ReturnsBadRequest()
        {
            // Arrange: Create a login request with invalid credentials
            var loginRequest = new LoginRequest { Email = "invalid@example.com", Password = "wrongPassword" };

            // Arrange: Setup _mockAuthService.LoginAsync to throw an exception
            _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<Contracts.V1.Requests.LoginRequest>()))
                .ThrowsAsync(new Exception("Invalid credentials"));

            // Act: Call _controller.LoginAsync with the login request
            var result = await _controller.LoginAsync(loginRequest);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result value is AuthFailureResponse with error message
            badRequestResult.Value.Should().BeOfType<AuthFailureResponse>();
            var responseValue = badRequestResult.Value as AuthFailureResponse;
            responseValue.Success.Should().BeFalse();
            responseValue.Message.Should().Be("Login failed");

            // Assert: Verify _mockAuthService.LoginAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.LoginAsync(It.Is<Contracts.V1.Requests.LoginRequest>(r => r.Email == loginRequest.Email)), Times.Once);
        }

        /// <summary>
        /// Tests that RegisterAsync returns Created with user ID when valid registration data is provided
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsCreatedWithUserId()
        {
            // Arrange: Create a registration request with valid user data
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "P@$$wOrd",
                ConfirmPassword = "P@$$wOrd",
                FirstName = "New",
                LastName = "User"
            };

            // Arrange: Create a mock successful registration response with user ID
            var registerResponse = new RegisterResponse { Success = true, UserId = TestHelpers.GetRandomUserId(), Email = "newuser@example.com" };

            // Arrange: Setup _mockAuthService.RegisterAsync to return the mock response
            _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<Contracts.V1.Requests.RegisterRequest>()))
                .ReturnsAsync(registerResponse);

            // Act: Call _controller.RegisterAsync with the registration request
            var result = await _controller.RegisterAsync(registerRequest);

            // Assert: Verify the result is CreatedResult
            result.Should().BeOfType<CreatedResult>();
            var createdResult = result as CreatedResult;

            // Assert: Verify the result value is RegisterResponse with expected user ID
            createdResult.Value.Should().BeOfType<RegisterResponse>();
            var responseValue = createdResult.Value as RegisterResponse;
            responseValue.UserId.Should().Be(registerResponse.UserId);

            // Assert: Verify _mockAuthService.RegisterAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.RegisterAsync(It.Is<Contracts.V1.Requests.RegisterRequest>(r => r.Email == registerRequest.Email)), Times.Once);
        }

        /// <summary>
        /// Tests that RegisterAsync returns BadRequest when password and confirm password don't match
        /// </summary>
        [Fact]
        public async Task RegisterAsync_PasswordMismatch_ReturnsBadRequest()
        {
            // Arrange: Create a registration request with mismatched passwords
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "P@$$wOrd",
                ConfirmPassword = "DifferentP@$$wOrd",
                FirstName = "New",
                LastName = "User"
            };

            // Act: Call _controller.RegisterAsync with the registration request
            var result = await _controller.RegisterAsync(registerRequest);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result value is AuthFailureResponse with password mismatch error
            badRequestResult.Value.Should().BeOfType<AuthFailureResponse>();
            var responseValue = badRequestResult.Value as AuthFailureResponse;
            responseValue.Success.Should().BeFalse();
            responseValue.Message.Should().Be("Passwords do not match");

            // Assert: Verify _mockAuthService.RegisterAsync was not called
            _mockAuthService.Verify(x => x.RegisterAsync(It.IsAny<Contracts.V1.Requests.RegisterRequest>()), Times.Never);
        }

        /// <summary>
        /// Tests that ForgotPasswordAsync returns OK with confirmation when a valid email is provided
        /// </summary>
        [Fact]
        public async Task ForgotPasswordAsync_ValidEmail_ReturnsOkWithConfirmation()
        {
            // Arrange: Create a password reset request with valid email
            var passwordResetRequest = new PasswordResetRequest { Email = "test@example.com" };

            // Arrange: Create a mock successful password reset response
            var passwordResetResponse = new PasswordResetResponse { Success = true, Email = "test@example.com" };

            // Arrange: Setup _mockAuthService.RequestPasswordResetAsync to return the mock response
            _mockAuthService.Setup(x => x.RequestPasswordResetAsync(It.IsAny<Contracts.V1.Requests.PasswordResetRequest>()))
                .ReturnsAsync(passwordResetResponse);

            // Act: Call _controller.ForgotPasswordAsync with the password reset request
            var result = await _controller.ForgotPasswordAsync(passwordResetRequest);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is PasswordResetResponse with expected email
            okResult.Value.Should().BeOfType<PasswordResetResponse>();
            var responseValue = okResult.Value as PasswordResetResponse;
            responseValue.Email.Should().Be("test@example.com");

            // Assert: Verify _mockAuthService.RequestPasswordResetAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.RequestPasswordResetAsync(It.Is<Contracts.V1.Requests.PasswordResetRequest>(r => r.Email == passwordResetRequest.Email)), Times.Once);
        }

        /// <summary>
        /// Tests that ResetPasswordAsync returns OK with confirmation when valid reset data is provided
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_ValidRequest_ReturnsOkWithConfirmation()
        {
            // Arrange: Create a password change request with valid token and matching passwords
            var passwordChangeRequest = new PasswordChangeRequest
            {
                Email = "test@example.com",
                ResetToken = "test_token",
                NewPassword = "NewP@$$wOrd",
                ConfirmPassword = "NewP@$$wOrd"
            };

            // Arrange: Create a mock successful password change response
            var passwordChangeResponse = new PasswordChangeResponse { Success = true, Email = "test@example.com" };

            // Arrange: Setup _mockAuthService.ChangePasswordAsync to return the mock response
            _mockAuthService.Setup(x => x.ChangePasswordAsync(It.IsAny<Contracts.V1.Requests.PasswordChangeRequest>()))
                .ReturnsAsync(passwordChangeResponse);

            // Act: Call _controller.ResetPasswordAsync with the password change request
            var result = await _controller.ResetPasswordAsync(passwordChangeRequest);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is PasswordChangeResponse with expected email
            okResult.Value.Should().BeOfType<PasswordChangeResponse>();
            var responseValue = okResult.Value as PasswordChangeResponse;
            responseValue.Email.Should().Be("test@example.com");

            // Assert: Verify _mockAuthService.ChangePasswordAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.ChangePasswordAsync(It.Is<Contracts.V1.Requests.PasswordChangeRequest>(r => r.Email == passwordChangeRequest.Email)), Times.Once);
        }

        /// <summary>
        /// Tests that ResetPasswordAsync returns BadRequest when password and confirm password don't match
        /// </summary>
        [Fact]
        public async Task ResetPasswordAsync_PasswordMismatch_ReturnsBadRequest()
        {
            // Arrange: Create a password change request with mismatched passwords
            var passwordChangeRequest = new PasswordChangeRequest
            {
                Email = "test@example.com",
                ResetToken = "test_token",
                NewPassword = "NewP@$$wOrd",
                ConfirmPassword = "DifferentP@$$wOrd"
            };

            // Act: Call _controller.ResetPasswordAsync with the password change request
            var result = await _controller.ResetPasswordAsync(passwordChangeRequest);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result value is AuthFailureResponse with password mismatch error
            badRequestResult.Value.Should().BeOfType<AuthFailureResponse>();
            var responseValue = badRequestResult.Value as AuthFailureResponse;
            responseValue.Success.Should().BeFalse();
            responseValue.Message.Should().Be("Passwords do not match");

            // Assert: Verify _mockAuthService.ChangePasswordAsync was not called
            _mockAuthService.Verify(x => x.ChangePasswordAsync(It.IsAny<Contracts.V1.Requests.PasswordChangeRequest>()), Times.Never);
        }

        /// <summary>
        /// Tests that RefreshTokenAsync returns OK with new token when a valid refresh token is provided
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsOkWithNewToken()
        {
            // Arrange: Create a refresh token request with valid token
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "valid_refresh_token" };

            // Arrange: Create a mock successful token refresh response with new token
            var refreshTokenResponse = new RefreshTokenResponse
            {
                Token = "new_test_token",
                RefreshToken = "new_test_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            // Arrange: Setup _mockAuthService.RefreshTokenAsync to return the mock response
            _mockAuthService.Setup(x => x.RefreshTokenAsync(It.IsAny<Contracts.V1.Requests.RefreshTokenRequest>()))
                .ReturnsAsync(refreshTokenResponse);

            // Act: Call _controller.RefreshTokenAsync with the refresh token request
            var result = await _controller.RefreshTokenAsync(refreshTokenRequest);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is RefreshTokenResponse with expected new token
            okResult.Value.Should().BeOfType<RefreshTokenResponse>();
            var responseValue = okResult.Value as RefreshTokenResponse;
            responseValue.Token.Should().Be("new_test_token");

            // Assert: Verify _mockAuthService.RefreshTokenAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.RefreshTokenAsync(It.Is<Contracts.V1.Requests.RefreshTokenRequest>(r => r.RefreshToken == refreshTokenRequest.RefreshToken)), Times.Once);
        }

        /// <summary>
        /// Tests that LogoutAsync returns OK for an authenticated user
        /// </summary>
        [Fact]
        public async Task LogoutAsync_AuthenticatedUser_ReturnsOk()
        {
            // Arrange: Setup controller context with authenticated user
            TestHelpers.SetupControllerContext(_controller, TestHelpers.GetRandomUserId());

            // Act: Call _controller.LogoutAsync()
            var result = await _controller.LogoutAsync();

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result contains a success message
            okResult.Value.Should().BeOfType<object>().Which.ToString().Contains("Logout successful").Should().BeTrue();
        }

        /// <summary>
        /// Tests that AzureAdLoginAsync returns OK with token when a valid Azure AD token is provided
        /// </summary>
        [Fact]
        public async Task AzureAdLoginAsync_ValidToken_ReturnsOkWithToken()
        {
            // Arrange: Create an Azure AD auth request with valid token
            var azureAdAuthRequest = new AzureAdAuthRequest { IdToken = "valid_azure_ad_token" };

            // Arrange: Create a mock successful Azure AD authentication response
            var azureAdAuthResponse = new AzureAdAuthResponse
            {
                Token = "test_token",
                RefreshToken = "test_refresh_token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserModel { UserId = TestHelpers.GetRandomUserId(), Email = "test@example.com", FirstName = "Test", LastName = "User" },
                IsNewUser = false
            };

            // Arrange: Setup _mockAuthService.AuthenticateWithAzureAdAsync to return the mock response
            _mockAuthService.Setup(x => x.AuthenticateWithAzureAdAsync(It.IsAny<AzureAdAuthRequest>()))
                .ReturnsAsync(azureAdAuthResponse);

            // Act: Call _controller.AzureAdLoginAsync with the Azure AD auth request
            var result = await _controller.AzureAdLoginAsync(azureAdAuthRequest);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is AzureAdAuthResponse with expected token
            okResult.Value.Should().BeOfType<AzureAdAuthResponse>();
            var responseValue = okResult.Value as AzureAdAuthResponse;
            responseValue.Token.Should().Be("test_token");

            // Assert: Verify _mockAuthService.AuthenticateWithAzureAdAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.AuthenticateWithAzureAdAsync(It.Is<AzureAdAuthRequest>(r => r.IdToken == azureAdAuthRequest.IdToken)), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns OK with validation result when a valid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_ValidToken_ReturnsOkWithValidation()
        {
            // Arrange: Create a valid token string
            var token = "valid_token";

            // Arrange: Setup _mockAuthService.ValidateTokenAsync to return true
            _mockAuthService.Setup(x => x.ValidateTokenAsync(token)).ReturnsAsync(true);

            // Act: Call _controller.ValidateTokenAsync with the token
            var result = await _controller.ValidateTokenAsync(token);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is TokenValidationResponse with IsValid=true
            okResult.Value.Should().BeOfType<TokenValidationResponse>();
            var responseValue = okResult.Value as TokenValidationResponse;
            responseValue.IsValid.Should().BeTrue();

            // Assert: Verify _mockAuthService.ValidateTokenAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.ValidateTokenAsync(token), Times.Once);
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns OK with invalid result when an invalid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_InvalidToken_ReturnsOkWithInvalidResult()
        {
            // Arrange: Create an invalid token string
            var token = "invalid_token";

            // Arrange: Setup _mockAuthService.ValidateTokenAsync to return false
            _mockAuthService.Setup(x => x.ValidateTokenAsync(token)).ReturnsAsync(false);

            // Act: Call _controller.ValidateTokenAsync with the token
            var result = await _controller.ValidateTokenAsync(token);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is TokenValidationResponse with IsValid=false
            okResult.Value.Should().BeOfType<TokenValidationResponse>();
            var responseValue = okResult.Value as TokenValidationResponse;
            responseValue.IsValid.Should().BeFalse();

            // Assert: Verify _mockAuthService.ValidateTokenAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.ValidateTokenAsync(token), Times.Once);
        }

        /// <summary>
        /// Tests that GetCurrentUserAsync returns OK with user information for an authenticated user
        /// </summary>
        [Fact]
        public async Task GetCurrentUserAsync_AuthenticatedUser_ReturnsOkWithUserInfo()
        {
            // Arrange: Create a mock user model
            var mockUser = new UserModel { UserId = TestHelpers.GetRandomUserId(), Email = "test@example.com", FirstName = "Test", LastName = "User" };

            // Arrange: Setup controller context with authenticated user
            TestHelpers.SetupControllerContext(_controller, mockUser.UserId);

            // Arrange: Setup _mockAuthService.GetUserFromClaimsAsync to return the mock user
            _mockAuthService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(mockUser);

            // Act: Call _controller.GetCurrentUserAsync()
            var result = await _controller.GetCurrentUserAsync();

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is UserModel with expected user data
            okResult.Value.Should().BeOfType<UserModel>();
            var responseValue = okResult.Value as UserModel;
            responseValue.UserId.Should().Be(mockUser.UserId);
            responseValue.Email.Should().Be(mockUser.Email);

            // Assert: Verify _mockAuthService.GetUserFromClaimsAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCurrentUserAsync returns Unauthorized when user is not found
        /// </summary>
        [Fact]
        public async Task GetCurrentUserAsync_UserNotFound_ReturnsUnauthorized()
        {
            // Arrange: Setup controller context with authenticated user
            TestHelpers.SetupControllerContext(_controller, TestHelpers.GetRandomUserId());

            // Arrange: Setup _mockAuthService.GetUserFromClaimsAsync to return null
            _mockAuthService.Setup(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync((UserModel)null);

            // Act: Call _controller.GetCurrentUserAsync()
            var result = await _controller.GetCurrentUserAsync();

            // Assert: Verify the result is UnauthorizedResult
            result.Should().BeOfType<UnauthorizedResult>();

            // Assert: Verify _mockAuthService.GetUserFromClaimsAsync was called once with correct parameters
            _mockAuthService.Verify(x => x.GetUserFromClaimsAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);
        }
    }
}