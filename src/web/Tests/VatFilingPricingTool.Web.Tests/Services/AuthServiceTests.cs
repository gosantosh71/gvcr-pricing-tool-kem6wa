using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Services.Implementations;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Authentication;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Tests.Mock;
using VatFilingPricingTool.Web.Tests.Helpers;

namespace VatFilingPricingTool.Web.Tests.Services
{
    /// <summary>
    /// Contains unit tests for the AuthService class
    /// </summary>
    public class AuthServiceTests
    {
        private readonly Mock<ApiClient> mockApiClient;
        private readonly Mock<TokenAuthenticationStateProvider> mockAuthStateProvider;
        private readonly Mock<LocalStorageHelper> mockLocalStorage;
        private readonly Mock<ILogger<AuthService>> mockLogger;
        private readonly Mock<IOptions<AzureAdAuthOptions>> mockAzureAdOptions;
        private readonly AuthService authService;

        /// <summary>
        /// Initializes a new instance of the AuthServiceTests class with mocked dependencies
        /// </summary>
        public AuthServiceTests()
        {
            // Initialize mocks
            mockApiClient = new Mock<ApiClient>();
            mockAuthStateProvider = new Mock<TokenAuthenticationStateProvider>();
            mockLocalStorage = new Mock<LocalStorageHelper>();
            mockLogger = new Mock<ILogger<AuthService>>();
            mockAzureAdOptions = new Mock<IOptions<AzureAdAuthOptions>>();
            
            // Configure AzureAdOptions
            mockAzureAdOptions.Setup(x => x.Value).Returns(new AzureAdAuthOptions());
            
            // Initialize AuthService with mocked dependencies
            authService = new AuthService(
                mockApiClient.Object,
                mockAuthStateProvider.Object,
                mockLocalStorage.Object,
                mockLogger.Object,
                mockAzureAdOptions.Object);
        }

        /// <summary>
        /// Tests that LoginAsync returns a successful response when valid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthSuccessResponse()
        {
            // Arrange: Create a login request with test email and password
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "Password123!"
            };
            
            // Arrange: Create an expected auth success response with token, user, etc.
            var expectedResponse = new AuthSuccessResponse
            {
                Token = "test-token",
                RefreshToken = "test-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = TestData.CreateTestUser("user-123", "test@example.com")
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the login endpoint
            mockApiClient.Setup(x => x.PostAsync<LoginRequest, AuthSuccessResponse>(
                ApiEndpoints.Auth.Login, loginRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.LoginAsync with the login request
            var result = await authService.LoginAsync(loginRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedResponse.Token);
            result.RefreshToken.Should().Be(expectedResponse.RefreshToken);
            result.User.Email.Should().Be(expectedResponse.User.Email);
            
            // Assert: Verify mockAuthStateProvider.MarkUserAsAuthenticated was called with the response
            mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(expectedResponse), Times.Once);
            
            // Assert: Verify authService.IsUserAuthenticated is true
            authService.IsUserAuthenticated.Should().BeTrue();
        }

        /// <summary>
        /// Tests that LoginAsync throws an exception when invalid credentials are provided
        /// </summary>
        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsException()
        {
            // Arrange: Create a login request with test email and password
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "WrongPassword!"
            };
            
            // Arrange: Set up mockApiClient to throw an exception for the login endpoint
            mockApiClient.Setup(x => x.PostAsync<LoginRequest, AuthSuccessResponse>(
                ApiEndpoints.Auth.Login, loginRequest, false))
                .ThrowsAsync(new HttpRequestException("Invalid credentials"));
            
            // Act & Assert: Verify that calling authService.LoginAsync throws an exception
            await Assert.ThrowsAsync<HttpRequestException>(() => authService.LoginAsync(loginRequest));
            
            // Assert: Verify authService.IsUserAuthenticated remains false
            authService.IsUserAuthenticated.Should().BeFalse();
        }

        /// <summary>
        /// Tests that RegisterAsync returns a successful response when a valid registration request is provided
        /// </summary>
        [Fact]
        public async Task RegisterAsync_ValidRequest_ReturnsRegisterResponse()
        {
            // Arrange: Create a registration request with test email, password, and name
            var registerRequest = new RegisterRequest
            {
                Email = "new@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!",
                FirstName = "New",
                LastName = "User"
            };
            
            // Arrange: Create an expected registration response with user ID and email
            var expectedResponse = new RegisterResponse
            {
                Success = true,
                UserId = "new-user-123",
                Email = registerRequest.Email
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the register endpoint
            mockApiClient.Setup(x => x.PostAsync<RegisterRequest, RegisterResponse>(
                ApiEndpoints.Auth.Register, registerRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.RegisterAsync with the registration request
            var result = await authService.RegisterAsync(registerRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.UserId.Should().Be(expectedResponse.UserId);
            result.Email.Should().Be(expectedResponse.Email);
        }

        /// <summary>
        /// Tests that RequestPasswordResetAsync returns a successful response when a valid email is provided
        /// </summary>
        [Fact]
        public async Task RequestPasswordResetAsync_ValidEmail_ReturnsSuccessResponse()
        {
            // Arrange: Create a password reset request with test email
            var resetRequest = new PasswordResetRequest
            {
                Email = "test@example.com"
            };
            
            // Arrange: Create an expected password reset response with success message
            var expectedResponse = new PasswordResetResponse
            {
                Success = true,
                Email = resetRequest.Email
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the password reset endpoint
            mockApiClient.Setup(x => x.PostAsync<PasswordResetRequest, PasswordResetResponse>(
                ApiEndpoints.Auth.ForgotPassword, resetRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.RequestPasswordResetAsync with the password reset request
            var result = await authService.RequestPasswordResetAsync(resetRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Email.Should().Be(expectedResponse.Email);
        }

        /// <summary>
        /// Tests that ChangePasswordAsync returns a successful response when a valid password change request is provided
        /// </summary>
        [Fact]
        public async Task ChangePasswordAsync_ValidRequest_ReturnsSuccessResponse()
        {
            // Arrange: Create a password change request with token, email, and new password
            var changeRequest = new PasswordChangeRequest
            {
                Email = "test@example.com",
                ResetToken = "reset-token",
                NewPassword = "NewPassword123!",
                ConfirmPassword = "NewPassword123!"
            };
            
            // Arrange: Create an expected password change response with success message
            var expectedResponse = new PasswordChangeResponse
            {
                Success = true,
                Email = changeRequest.Email
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the password change endpoint
            mockApiClient.Setup(x => x.PostAsync<PasswordChangeRequest, PasswordChangeResponse>(
                ApiEndpoints.Auth.ResetPassword, changeRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.ChangePasswordAsync with the password change request
            var result = await authService.ChangePasswordAsync(changeRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Email.Should().Be(expectedResponse.Email);
        }

        /// <summary>
        /// Tests that RefreshTokenAsync returns a successful response when a valid refresh token is provided
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_ValidToken_ReturnsRefreshTokenResponse()
        {
            // Arrange: Create a refresh token request with test refresh token
            var refreshRequest = new RefreshTokenRequest
            {
                RefreshToken = "test-refresh-token"
            };
            
            // Arrange: Create an expected refresh token response with new token and expiration
            var expectedResponse = new RefreshTokenResponse
            {
                Token = "new-token",
                RefreshToken = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the refresh token endpoint
            mockApiClient.Setup(x => x.PostAsync<RefreshTokenRequest, RefreshTokenResponse>(
                ApiEndpoints.Auth.RefreshToken, refreshRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.RefreshTokenAsync with the refresh token request
            var result = await authService.RefreshTokenAsync(refreshRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedResponse.Token);
            result.RefreshToken.Should().Be(expectedResponse.RefreshToken);
            result.ExpiresAt.Should().BeCloseTo(expectedResponse.ExpiresAt, TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns true when a valid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_ValidToken_ReturnsTrue()
        {
            // Arrange: Create a test token string
            var token = "valid-token";
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            
            // Arrange: Set up mockApiClient to return a successful response for the validation endpoint
            mockApiClient.Setup(x => x.PostAsync(ApiEndpoints.Auth.ValidateToken, true))
                .ReturnsAsync(response);
            
            // Act: Call authService.ValidateTokenAsync with the test token
            var result = await authService.ValidateTokenAsync(token);
            
            // Assert: Verify the result is true
            result.Should().BeTrue();
        }

        /// <summary>
        /// Tests that ValidateTokenAsync returns false when an invalid token is provided
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange: Create a test token string
            var token = "invalid-token";
            
            // Arrange: Set up mockApiClient to throw an exception for the validation endpoint
            mockApiClient.Setup(x => x.PostAsync(ApiEndpoints.Auth.ValidateToken, true))
                .ThrowsAsync(new HttpRequestException("Invalid token"));
            
            // Act: Call authService.ValidateTokenAsync with the test token
            var result = await authService.ValidateTokenAsync(token);
            
            // Assert: Verify the result is false
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that AuthenticateWithAzureAdAsync returns a successful response when a valid Azure AD token is provided
        /// </summary>
        [Fact]
        public async Task AuthenticateWithAzureAdAsync_ValidToken_ReturnsAuthSuccessResponse()
        {
            // Arrange: Create an Azure AD auth request with ID token
            var azureAdRequest = new AzureAdAuthRequest
            {
                IdToken = "azure-ad-token"
            };
            
            // Arrange: Create an expected auth success response with token, user, etc.
            var expectedResponse = new AzureAdAuthResponse
            {
                Token = "test-token",
                RefreshToken = "test-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = TestData.CreateTestUser("user-123", "test@example.com")
            };
            
            // Arrange: Set up mockApiClient to return the expected response for the Azure AD auth endpoint
            mockApiClient.Setup(x => x.PostAsync<AzureAdAuthRequest, AzureAdAuthResponse>(
                ApiEndpoints.Auth.AzureAd, azureAdRequest, false))
                .ReturnsAsync(expectedResponse);
            
            // Act: Call authService.AuthenticateWithAzureAdAsync with the Azure AD auth request
            var result = await authService.AuthenticateWithAzureAdAsync(azureAdRequest);
            
            // Assert: Verify the result matches the expected response
            result.Should().NotBeNull();
            result.Token.Should().Be(expectedResponse.Token);
            result.RefreshToken.Should().Be(expectedResponse.RefreshToken);
            result.User.Email.Should().Be(expectedResponse.User.Email);
            
            // Assert: Verify mockAuthStateProvider.MarkUserAsAuthenticated was called with the response
            mockAuthStateProvider.Verify(x => x.MarkUserAsAuthenticated(It.IsAny<AuthSuccessResponse>()), Times.Once);
            
            // Assert: Verify authService.IsUserAuthenticated is true
            authService.IsUserAuthenticated.Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetCurrentUserAsync returns the user model when the user is authenticated
        /// </summary>
        [Fact]
        public async Task GetCurrentUserAsync_Authenticated_ReturnsUserModel()
        {
            // Arrange: Create a test user model
            var user = TestData.CreateTestUser("user-123", "test@example.com");
            
            // Arrange: Set up mockLocalStorage to return a valid auth token
            mockLocalStorage.Setup(x => x.GetAuthTokenAsync())
                .ReturnsAsync("valid-token");
            
            // Arrange: Set up mockLocalStorage to return the test user model
            mockLocalStorage.Setup(x => x.GetUserDataAsync())
                .ReturnsAsync(user);
            
            // Act: Call authService.GetCurrentUserAsync
            var result = await authService.GetCurrentUserAsync();
            
            // Assert: Verify the result matches the test user model
            result.Should().NotBeNull();
            result.UserId.Should().Be(user.UserId);
            result.Email.Should().Be(user.Email);
        }

        /// <summary>
        /// Tests that GetCurrentUserAsync returns null when the user is not authenticated
        /// </summary>
        [Fact]
        public async Task GetCurrentUserAsync_NotAuthenticated_ReturnsNull()
        {
            // Arrange: Set up mockLocalStorage to return null for auth token
            mockLocalStorage.Setup(x => x.GetAuthTokenAsync())
                .ReturnsAsync((string)null);
            
            // Act: Call authService.GetCurrentUserAsync
            var result = await authService.GetCurrentUserAsync();
            
            // Assert: Verify the result is null
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that LogoutAsync calls the auth state provider to mark the user as logged out and clears local storage
        /// </summary>
        [Fact]
        public async Task LogoutAsync_CallsAuthStateProviderAndClearsStorage()
        {
            // Arrange: Set up mockApiClient to return a successful response for the logout endpoint
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            mockApiClient.Setup(x => x.PostAsync(ApiEndpoints.Auth.Logout))
                .ReturnsAsync(response);
            
            // Act: Call authService.LogoutAsync
            await authService.LogoutAsync();
            
            // Assert: Verify mockAuthStateProvider.MarkUserAsLoggedOut was called
            mockAuthStateProvider.Verify(x => x.MarkUserAsLoggedOut(), Times.Once);
            
            // Assert: Verify mockLocalStorage.ClearAuthDataAsync was called
            mockLocalStorage.Verify(x => x.ClearAuthDataAsync(), Times.Once);
            
            // Assert: Verify authService.IsUserAuthenticated is false
            authService.IsUserAuthenticated.Should().BeFalse();
        }
    }
}