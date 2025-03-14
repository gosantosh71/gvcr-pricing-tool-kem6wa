using Microsoft.Playwright; // v1.30.0
using System.Threading.Tasks; // v6.0.0
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;
using VatFilingPricingTool.Web.E2E.Tests.Helpers;

namespace VatFilingPricingTool.Web.E2E.Tests.PageObjects
{
    /// <summary>
    /// Page object representing the login page of the VAT Filing Pricing Tool web application
    /// </summary>
    public class LoginPage : BasePage
    {
        private const string EmailInputSelector = "[data-testid='email-input']";
        private const string PasswordInputSelector = "[data-testid='password-input']";
        private const string SignInButtonSelector = "[data-testid='sign-in-button']";
        private const string AzureAdButtonSelector = "[data-testid='azure-ad-button']";
        private const string ForgotPasswordLinkSelector = "[data-testid='forgot-password-link']";
        private const string RegisterLinkSelector = "[data-testid='register-link']";
        private const string ErrorMessageSelector = "[data-testid='error-message']";

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginPage"/> class.
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance.</param>
        public LoginPage(PlaywrightFixture fixture) : base(fixture, "/")
        {
            // All initialization is handled in the base constructor
        }

        /// <summary>
        /// Performs login with the provided user credentials
        /// </summary>
        /// <param name="user">The test user with credentials to use for login</param>
        /// <returns>A task representing the asynchronous login operation</returns>
        public async Task LoginAsync(TestUser user)
        {
            await NavigateToAsync();
            await WaitForElementToBeVisibleAsync(EmailInputSelector);
            await FillAsync(EmailInputSelector, user.Email);
            await FillAsync(PasswordInputSelector, user.Password);
            await ClickAsync(SignInButtonSelector);
            await Page.WaitForNavigationAsync();
        }

        /// <summary>
        /// Performs login using Azure AD authentication
        /// </summary>
        /// <returns>A task representing the asynchronous login operation</returns>
        public async Task LoginWithAzureAdAsync()
        {
            await NavigateToAsync();
            await WaitForElementToBeVisibleAsync(AzureAdButtonSelector);
            await ClickAsync(AzureAdButtonSelector);
            await Page.WaitForNavigationAsync();
            // Note: This method doesn't handle the Azure AD login form as it's external to the application
        }

        /// <summary>
        /// Checks if the login page is currently displayed
        /// </summary>
        /// <returns>True if the login page is displayed, otherwise false</returns>
        public async Task<bool> IsLoginPageDisplayedAsync()
        {
            var isEmailInputVisible = await IsElementVisibleAsync(EmailInputSelector);
            var isPasswordInputVisible = await IsElementVisibleAsync(PasswordInputSelector);
            var isSignInButtonVisible = await IsElementVisibleAsync(SignInButtonSelector);

            return isEmailInputVisible && isPasswordInputVisible && isSignInButtonVisible;
        }

        /// <summary>
        /// Gets the error message displayed on the login page
        /// </summary>
        /// <returns>The error message text or empty string if no error is displayed</returns>
        public async Task<string> GetErrorMessageAsync()
        {
            if (await IsElementVisibleAsync(ErrorMessageSelector))
            {
                return await GetTextContentAsync(ErrorMessageSelector);
            }

            return string.Empty;
        }

        /// <summary>
        /// Clicks the forgot password link to navigate to the password reset page
        /// </summary>
        /// <returns>A task representing the asynchronous navigation operation</returns>
        public async Task ForgotPasswordAsync()
        {
            await WaitForElementToBeVisibleAsync(ForgotPasswordLinkSelector);
            await ClickAsync(ForgotPasswordLinkSelector);
            await Page.WaitForNavigationAsync();
        }

        /// <summary>
        /// Clicks the register link to navigate to the registration page
        /// </summary>
        /// <returns>A task representing the asynchronous navigation operation</returns>
        public async Task RegisterNewUserAsync()
        {
            await WaitForElementToBeVisibleAsync(RegisterLinkSelector);
            await ClickAsync(RegisterLinkSelector);
            await Page.WaitForNavigationAsync();
        }
    }
}