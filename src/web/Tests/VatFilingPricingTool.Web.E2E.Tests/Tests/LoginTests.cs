using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;
using VatFilingPricingTool.Web.E2E.Tests.PageObjects;
using VatFilingPricingTool.Web.E2E.Tests.Helpers;

namespace VatFilingPricingTool.Web.E2E.Tests.Tests
{
    /// <summary>
    /// Contains end-to-end tests for the authentication functionality
    /// </summary>
    public class LoginTests : IClassFixture<PlaywrightFixture>
    {
        private readonly PlaywrightFixture Fixture;
        private readonly LoginPage LoginPage;
        private readonly DashboardPage DashboardPage;
        
        /// <summary>
        /// Initializes a new instance of the LoginTests class
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance</param>
        public LoginTests(PlaywrightFixture fixture)
        {
            Fixture = fixture;
            LoginPage = new LoginPage(fixture);
            DashboardPage = new DashboardPage(fixture);
        }
        
        /// <summary>
        /// Tests that a user can successfully log in with valid credentials
        /// </summary>
        [Fact]
        public async Task SuccessfulLoginTest()
        {
            // Arrange
            var userData = new UserData();
            var user = userData.GetStandardUser();
            
            // Act
            await LoginPage.LoginAsync(user);
            
            // Assert
            var isDashboardDisplayed = await DashboardPage.IsDashboardDisplayedAsync();
            isDashboardDisplayed.Should().BeTrue("because a user with valid credentials should be redirected to the dashboard");
            
            var welcomeMessage = await DashboardPage.GetWelcomeMessageAsync();
            welcomeMessage.Should().Contain(user.GetFullName(), "because the welcome message should contain the user's name");
        }
        
        /// <summary>
        /// Tests that an admin user can successfully log in with valid credentials
        /// </summary>
        [Fact]
        public async Task AdminLoginTest()
        {
            // Arrange
            var userData = new UserData();
            var adminUser = userData.GetAdminUser();
            
            // Act
            await LoginPage.LoginAsync(adminUser);
            
            // Assert
            var isDashboardDisplayed = await DashboardPage.IsDashboardDisplayedAsync();
            isDashboardDisplayed.Should().BeTrue("because an admin user with valid credentials should be redirected to the dashboard");
            
            var welcomeMessage = await DashboardPage.GetWelcomeMessageAsync();
            welcomeMessage.Should().Contain(adminUser.GetFullName(), "because the welcome message should contain the admin's name");
        }
        
        /// <summary>
        /// Tests that login fails with invalid credentials and displays an error message
        /// </summary>
        [Fact]
        public async Task FailedLoginTest()
        {
            // Arrange
            var userData = new UserData();
            var invalidUser = userData.GetInvalidUser();
            
            // Act
            await LoginPage.LoginAsync(invalidUser);
            
            // Assert
            var isLoginPageDisplayed = await LoginPage.IsLoginPageDisplayedAsync();
            isLoginPageDisplayed.Should().BeTrue("because a user with invalid credentials should remain on the login page");
            
            var errorMessage = await LoginPage.GetErrorMessageAsync();
            errorMessage.Should().NotBeEmpty("because an error message should be displayed");
            errorMessage.Should().Contain("Invalid", "because the error message should indicate invalid credentials");
        }
        
        /// <summary>
        /// Tests the Azure AD login flow (Note: This test may be skipped or mocked as it involves external authentication)
        /// </summary>
        [Fact(Skip = "Requires Azure AD authentication which cannot be fully automated")]
        public async Task AzureAdLoginTest()
        {
            // Note: This test is skipped as it requires external Azure AD authentication
            // This is more of a placeholder for a test that would be run manually or with specific mocking
            await LoginPage.LoginWithAzureAdAsync();
            // In a real implementation, we might use a mock or special test account
        }
        
        /// <summary>
        /// Tests that login fails when credentials are empty
        /// </summary>
        [Fact]
        public async Task EmptyCredentialsTest()
        {
            // Arrange
            var emptyUser = new TestUser("", "", "Empty", "User", "None");
            
            // Act
            await LoginPage.LoginAsync(emptyUser);
            
            // Assert
            var isLoginPageDisplayed = await LoginPage.IsLoginPageDisplayedAsync();
            isLoginPageDisplayed.Should().BeTrue("because a user with empty credentials should remain on the login page");
            
            var errorMessage = await LoginPage.GetErrorMessageAsync();
            errorMessage.Should().NotBeEmpty("because an error message should be displayed");
            errorMessage.Should().Contain("required", "because the error message should indicate required fields");
        }
        
        /// <summary>
        /// Tests that all required elements are displayed on the login page
        /// </summary>
        [Fact]
        public async Task LoginPageElementsTest()
        {
            // Act
            await LoginPage.NavigateToAsync();
            
            // Assert
            var isLoginPageDisplayed = await LoginPage.IsLoginPageDisplayedAsync();
            isLoginPageDisplayed.Should().BeTrue("because all essential login elements should be visible");
            
            // Additional assertions for specific UI elements
            (await Fixture.Page.IsVisibleAsync("[data-testid='email-input']")).Should().BeTrue("because email input field should be visible");
            (await Fixture.Page.IsVisibleAsync("[data-testid='password-input']")).Should().BeTrue("because password input field should be visible");
            (await Fixture.Page.IsVisibleAsync("[data-testid='sign-in-button']")).Should().BeTrue("because sign in button should be visible");
            (await Fixture.Page.IsVisibleAsync("[data-testid='azure-ad-button']")).Should().BeTrue("because Azure AD button should be visible");
            (await Fixture.Page.IsVisibleAsync("[data-testid='forgot-password-link']")).Should().BeTrue("because forgot password link should be visible");
            (await Fixture.Page.IsVisibleAsync("[data-testid='register-link']")).Should().BeTrue("because register link should be visible");
        }
    }
}