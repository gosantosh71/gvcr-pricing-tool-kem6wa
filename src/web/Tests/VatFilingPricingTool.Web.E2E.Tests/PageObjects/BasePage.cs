using Microsoft.Playwright;
using System.Threading.Tasks;
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;

namespace VatFilingPricingTool.Web.E2E.Tests.PageObjects
{
    /// <summary>
    /// Abstract base class for all page objects in the E2E test framework.
    /// Provides common functionality for page interactions following the Page Object Model pattern.
    /// </summary>
    public abstract class BasePage
    {
        /// <summary>
        /// The Playwright fixture that manages the browser instance.
        /// </summary>
        protected readonly PlaywrightFixture Fixture;

        /// <summary>
        /// The Playwright page instance used for browser interactions.
        /// </summary>
        protected readonly IPage Page;

        /// <summary>
        /// The relative path to the page from the base URL.
        /// </summary>
        protected readonly string PagePath;

        /// <summary>
        /// The default timeout for waiting operations in milliseconds.
        /// </summary>
        protected const int DefaultTimeout = 30000; // 30 seconds

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePage"/> class.
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance.</param>
        /// <param name="pagePath">The relative path to the page from the base URL.</param>
        protected BasePage(PlaywrightFixture fixture, string pagePath)
        {
            Fixture = fixture;
            Page = fixture.Page;
            PagePath = pagePath;
        }

        /// <summary>
        /// Navigates to the page represented by this page object.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public virtual async Task NavigateToAsync()
        {
            await Fixture.NavigateToAsync(PagePath);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Clicks on an element identified by the specified selector.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected async Task ClickAsync(string selector)
        {
            await WaitForElementToBeVisibleAsync(selector);
            await Page.ClickAsync(selector);
        }

        /// <summary>
        /// Fills a form field identified by the specified selector with the provided value.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <param name="value">The value to fill in the form field.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected async Task FillAsync(string selector, string value)
        {
            await WaitForElementToBeVisibleAsync(selector);
            await Page.FillAsync(selector, value);
        }

        /// <summary>
        /// Gets the text content of an element identified by the specified selector.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <returns>The text content of the element.</returns>
        protected async Task<string> GetTextContentAsync(string selector)
        {
            await WaitForElementToBeVisibleAsync(selector);
            return await Page.TextContentAsync(selector) ?? string.Empty;
        }

        /// <summary>
        /// Waits for an element identified by the specified selector to be visible.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <param name="timeout">The maximum time to wait in milliseconds. Uses DefaultTimeout if not specified.</param>
        /// <returns>The element handle once visible.</returns>
        protected async Task<IElementHandle> WaitForElementToBeVisibleAsync(string selector, int? timeout = null)
        {
            var options = new WaitForSelectorOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeout ?? DefaultTimeout
            };

            return await Page.WaitForSelectorAsync(selector, options);
        }

        /// <summary>
        /// Checks if an element identified by the specified selector is visible.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <returns>True if the element is visible, otherwise false.</returns>
        protected async Task<bool> IsElementVisibleAsync(string selector)
        {
            var element = await Page.QuerySelectorAsync(selector);
            if (element == null)
            {
                return false;
            }

            return await element.IsVisibleAsync();
        }

        /// <summary>
        /// Selects an option in a dropdown element identified by the specified selector.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <param name="value">The value of the option to select.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected async Task SelectOptionAsync(string selector, string value)
        {
            await WaitForElementToBeVisibleAsync(selector);
            await Page.SelectOptionAsync(selector, value);
        }

        /// <summary>
        /// Checks or unchecks a checkbox element identified by the specified selector.
        /// </summary>
        /// <param name="selector">The selector to identify the element.</param>
        /// <param name="check">True to check the element, false to uncheck it.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected async Task CheckAsync(string selector, bool check = true)
        {
            await WaitForElementToBeVisibleAsync(selector);
            if (check)
            {
                await Page.CheckAsync(selector);
            }
            else
            {
                await Page.UncheckAsync(selector);
            }
        }

        /// <summary>
        /// Waits for navigation to complete.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected async Task WaitForNavigationAsync()
        {
            var options = new WaitForNavigationOptions
            {
                Timeout = DefaultTimeout
            };

            await Page.WaitForNavigationAsync(options);
        }
    }
}