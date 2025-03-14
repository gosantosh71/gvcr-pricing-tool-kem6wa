using FluentAssertions; // FluentAssertions, Version=6.2.0
using Microsoft.AspNetCore.Mvc.Testing; // Microsoft.AspNetCore.Mvc.Testing, Version=6.0.0
using System; // System, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Net.Http.Json; // System.Net.Http.Json, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using System.Collections.Generic; // System.Collections.Generic, Version=6.0.0
using Xunit; // Xunit, Version=2.4.1
using VatFilingPricingTool.IntegrationTests.TestServer; // Base class for integration tests providing common setup and utility methods
using VatFilingPricingTool.Common.Models.ApiResponse; // Generic API response wrapper for handling test responses
using VatFilingPricingTool.Api.Models.Requests; // Request model for login authentication tests
using VatFilingPricingTool.Api.Models.Responses; // Response model for successful authentication tests
using VatFilingPricingTool.Domain.Enums; // User role enumeration for authentication testing
using VatFilingPricingTool.Common.Constants; // Constants for API route paths

namespace VatFilingPricingTool.IntegrationTests.Api
{
    /// <summary>
    /// Integration tests for the AuthController endpoints
    /// </summary>
    public class AuthControllerIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Initializes a new instance of the AuthControllerIntegrationTests class
        /// </summary>
        public AuthControllerIntegrationTests()
        {
            // LD1: Call base constructor to set up the test environment
        }

        /// <summary>
        /// Tests that the login endpoint returns a success response with valid credentials
        /// </summary>
        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResponse()
        {
            // LD1: Create a login request with valid credentials
            var loginRequest = CreateTestLoginRequest();

            // LD1: Send a POST request to the login endpoint
            var response = await PostAsync<LoginRequest, AuthSuccessResponse>(ApiRoutes.Auth.Login, loginRequest);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a valid token
            response.Data.Token.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains a valid refresh token
            response.Data.RefreshToken.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains user information
            response.Data.User.Should().NotBeNull();
        }

        /// <summary>
        /// Tests that the login endpoint returns a failure response with invalid credentials
        /// </summary>
        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ReturnsFailureResponse()
        {
            // LD1: Create a login request with invalid credentials
            var loginRequest = CreateTestLoginRequest(email: "invalid@example.com", password: "InvalidPassword");

            // LD1: Send a POST request to the login endpoint
            var response = await PostAsync<LoginRequest, AuthSuccessResponse>(ApiRoutes.Auth.Login, loginRequest);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that the register endpoint returns a success response with valid registration data
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsSuccessResponse()
        {
            // LD1: Create a registration request with valid data
            var registerRequest = CreateTestRegisterRequest();

            // LD1: Send a POST request to the register endpoint
            var response = await PostAsync<RegisterRequest, RegisterResponse>(ApiRoutes.Auth.Register, registerRequest);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a user ID
            response.Data.UserId.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains the registered email
            response.Data.Email.Should().Be(registerRequest.Email);
        }

        /// <summary>
        /// Tests that the register endpoint returns a failure response when registering with an existing email
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithExistingEmail_ReturnsFailureResponse()
        {
            // LD1: Create and register a user with a specific email
            var initialRegisterRequest = CreateTestRegisterRequest(email: "duplicate@example.com");
            await PostAsync<RegisterRequest, RegisterResponse>(ApiRoutes.Auth.Register, initialRegisterRequest);

            // LD1: Create a second registration request with the same email
            var duplicateRegisterRequest = CreateTestRegisterRequest(email: "duplicate@example.com");

            // LD1: Send a POST request to the register endpoint
            var response = await PostAsync<RegisterRequest, RegisterResponse>(ApiRoutes.Auth.Register, duplicateRegisterRequest);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message about duplicate email
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that the register endpoint returns a failure response when password and confirm password don't match
        /// </summary>
        [Fact]
        public async Task RegisterAsync_WithPasswordMismatch_ReturnsFailureResponse()
        {
            // LD1: Create a registration request with mismatched password and confirm password
            var registerRequest = CreateTestRegisterRequest();
            registerRequest.Password = "Password123!";
            registerRequest.ConfirmPassword = "DifferentPassword!";

            // LD1: Send a POST request to the register endpoint
            var response = await PostAsync<RegisterRequest, RegisterResponse>(ApiRoutes.Auth.Register, registerRequest);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message about password mismatch
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that the refresh token endpoint returns new tokens with a valid refresh token
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // LD1: Login with valid credentials to get initial tokens
            var loginRequest = CreateTestLoginRequest();
            var loginResponse = await PostAsync<LoginRequest, AuthSuccessResponse>(ApiRoutes.Auth.Login, loginRequest);

            // LD1: Create a refresh token request with the obtained refresh token
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = loginResponse.Data.RefreshToken };

            // LD1: Send a POST request to the refresh token endpoint
            var response = await PostAsync<RefreshTokenRequest, RefreshTokenResponse>(ApiRoutes.Auth.RefreshToken, refreshTokenRequest);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a new token
            response.Data.Token.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains a new refresh token
            response.Data.RefreshToken.Should().NotBeNullOrEmpty();

            // LD1: Assert that the new token is different from the original token
            response.Data.Token.Should().NotBe(loginResponse.Data.Token);
        }

        /// <summary>
        /// Tests that the refresh token endpoint returns a failure response with an invalid refresh token
        /// </summary>
        [Fact]
        public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFailureResponse()
        {
            // LD1: Create a refresh token request with an invalid refresh token
            var refreshTokenRequest = new RefreshTokenRequest { RefreshToken = "invalid_refresh_token" };

            // LD1: Send a POST request to the refresh token endpoint
            var response = await PostAsync<RefreshTokenRequest, RefreshTokenResponse>(ApiRoutes.Auth.RefreshToken, refreshTokenRequest);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that the validate token endpoint returns a valid result for a valid token
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsValidResult()
        {
            // LD1: Login with valid credentials to get a token
            var loginRequest = CreateTestLoginRequest();
            var loginResponse = await PostAsync<LoginRequest, AuthSuccessResponse>(ApiRoutes.Auth.Login, loginRequest);

            // LD1: Send a GET request to the validate token endpoint with the token
            var response = await GetAsync<TokenValidationResponse>($"{ApiRoutes.Auth.Base}/validate-token?token={loginResponse.Data.Token}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data indicates the token is valid
            response.Data.IsValid.Should().BeTrue();
        }

        /// <summary>
        /// Tests that the validate token endpoint returns an invalid result for an invalid token
        /// </summary>
        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsInvalidResult()
        {
            // LD1: Send a GET request to the validate token endpoint with an invalid token
            var response = await GetAsync<TokenValidationResponse>($"{ApiRoutes.Auth.Base}/validate-token?token=invalid_token");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data indicates the token is not valid
            response.Data.IsValid.Should().BeFalse();
        }

        /// <summary>
        /// Tests that the logout endpoint returns success for an authenticated user
        /// </summary>
        [Fact]
        public async Task LogoutAsync_WithAuthenticatedUser_ReturnsSuccess()
        {
            // LD1: Create an authenticated client with a specific user role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Send a POST request to the logout endpoint
            var response = await PostAsync<object, object>(ApiRoutes.Auth.Logout, null, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();
        }

        /// <summary>
        /// Tests that the current user endpoint returns user information for an authenticated user
        /// </summary>
        [Fact]
        public async Task GetCurrentUserAsync_WithAuthenticatedUser_ReturnsUserInfo()
        {
            // LD1: Create an authenticated client with a specific user role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Send a GET request to the current user endpoint
            var response = await GetAsync<AuthSuccessResponse>(ApiRoutes.User.Profile, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains user information with the correct role
            response.Data.User.Should().NotBeNull();
            response.Data.User.Roles.Should().Contain(UserRole.Customer);
        }

        /// <summary>
        /// Tests that the Azure AD login endpoint returns a success response with a valid token
        /// </summary>
        [Fact]
        public async Task AzureAdLoginAsync_WithValidToken_ReturnsSuccessResponse()
        {
            // LD1: Create an Azure AD auth request with a mock valid token
            var azureAdAuthRequest = new AzureAdAuthRequest { IdToken = "mock_azure_ad_token" };

            // LD1: Send a POST request to the Azure AD login endpoint
            var response = await PostAsync<AzureAdAuthRequest, AuthSuccessResponse>(ApiRoutes.Auth.AzureAd, azureAdAuthRequest);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a valid token
            response.Data.Token.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains a valid refresh token
            response.Data.RefreshToken.Should().NotBeNullOrEmpty();

            // LD1: Assert that the response data contains user information
            response.Data.User.Should().NotBeNull();
        }

        /// <summary>
        /// Creates a test login request with specified or default credentials
        /// </summary>
        /// <param name="email">The email address for the login request (default: "test@example.com")</param>
        /// <param name="password">The password for the login request (default: "Password123!")</param>
        /// <returns>A login request with the specified credentials</returns>
        private LoginRequest CreateTestLoginRequest(string email = "test@example.com", string password = "Password123!")
        {
            // LD1: Create a new LoginRequest instance
            var loginRequest = new LoginRequest();

            // LD1: Set Email to the provided email parameter
            loginRequest.Email = email;

            // LD1: Set Password to the provided password parameter
            loginRequest.Password = password;

            // LD1: Set RememberMe to true
            loginRequest.RememberMe = true;

            // LD1: Return the login request
            return loginRequest;
        }

        /// <summary>
        /// Creates a test registration request with specified or default data
        /// </summary>
        /// <param name="email">The email address for the registration request (default: "newuser@example.com")</param>
        /// <param name="password">The password for the registration request (default: "Password123!")</param>
        /// <returns>A registration request with the specified data</returns>
        private RegisterRequest CreateTestRegisterRequest(string email = "newuser@example.com", string password = "Password123!")
        {
            // LD1: Create a new RegisterRequest instance
            var registerRequest = new RegisterRequest();

            // LD1: Set Email to the provided email parameter
            registerRequest.Email = email;

            // LD1: Set Password to the provided password parameter
            registerRequest.Password = password;

            // LD1: Set ConfirmPassword to the provided password parameter
            registerRequest.ConfirmPassword = password;

            // LD1: Set FirstName to "Test"
            registerRequest.FirstName = "Test";

            // LD1: Set LastName to "User"
            registerRequest.LastName = "User";

            // LD1: Add UserRole.Customer to Roles
            registerRequest.Roles.Add(UserRole.Customer);

            // LD1: Return the registration request
            return registerRequest;
        }
    }
}