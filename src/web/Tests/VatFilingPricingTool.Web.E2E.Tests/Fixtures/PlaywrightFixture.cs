using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Playwright; // v1.30.0
using VatFilingPricingTool.Web.E2E.Tests.Helpers;
using Xunit; // v2.4.2

namespace VatFilingPricingTool.Web.E2E.Tests.Fixtures
{
    /// <summary>
    /// Test fixture that manages Playwright browser instance for E2E testing
    /// </summary>
    public class PlaywrightFixture : IAsyncDisposable
    {
        /// <summary>
        /// Gets the Playwright instance.
        /// </summary>
        public IPlaywright Playwright { get; private set; }

        /// <summary>
        /// Gets the Browser instance.
        /// </summary>
        public IBrowser Browser { get; private set; }

        /// <summary>
        /// Gets the BrowserContext instance.
        /// </summary>
        public IBrowserContext Context { get; private set; }

        /// <summary>
        /// Gets the Page instance.
        /// </summary>
        public IPage Page { get; private set; }

        /// <summary>
        /// Gets the base URL for the application under test.
        /// </summary>
        public string BaseUrl { get; }

        /// <summary>
        /// Gets the path where screenshots are saved.
        /// </summary>
        public string ScreenshotPath { get; }

        /// <summary>
        /// Initializes a new instance of the PlaywrightFixture class and sets up the browser.
        /// </summary>
        public PlaywrightFixture()
        {
            BaseUrl = TestSettings.BaseUrl;
            ScreenshotPath = TestSettings.ScreenshotPath;
            
            // Initialize Playwright synchronously in the constructor
            InitializePlaywrightAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes Playwright and creates browser, context, and page instances.
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        private async Task InitializePlaywrightAsync()
        {
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            
            // Create browser based on configuration
            Browser = TestSettings.BrowserType.ToLowerInvariant() switch
            {
                "firefox" => await Playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions 
                { 
                    Headless = TestSettings.Headless 
                }),
                "webkit" => await Playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions 
                { 
                    Headless = TestSettings.Headless 
                }),
                _ => await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions 
                { 
                    Headless = TestSettings.Headless 
                }),
            };
            
            // Create browser context with viewport and locale
            Context = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = new ViewportSize
                {
                    Width = 1920,
                    Height = 1080
                },
                Locale = "en-US"
            });
            
            // Create new page
            Page = await Context.NewPageAsync();
            
            // Set default timeout from configuration
            Page.SetDefaultTimeout(TestSettings.DefaultTimeout);
            
            // Ensure screenshot directory exists
            if (!Directory.Exists(ScreenshotPath))
            {
                Directory.CreateDirectory(ScreenshotPath);
            }
        }

        /// <summary>
        /// Navigates to a specified path relative to the base URL.
        /// </summary>
        /// <param name="path">The path to navigate to, relative to the base URL.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task NavigateToAsync(string path)
        {
            string url = new Uri(new Uri(BaseUrl), path).ToString();
            await Page.GotoAsync(url);
            await WaitForLoadStateAsync();
        }

        /// <summary>
        /// Takes a screenshot of the current page state and saves it to the configured path.
        /// </summary>
        /// <param name="screenshotName">Name for the screenshot file.</param>
        /// <returns>Path to the saved screenshot.</returns>
        public async Task<string> TakeScreenshotAsync(string screenshotName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filename = $"{timestamp}_{screenshotName}.png";
            var filePath = Path.Combine(ScreenshotPath, filename);
            
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = filePath,
                FullPage = true
            });
            
            return filePath;
        }

        /// <summary>
        /// Waits for the page to reach a specific load state.
        /// </summary>
        /// <param name="state">The load state to wait for. Defaults to NetworkIdle.</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        public async Task WaitForLoadStateAsync(LoadState? state = null)
        {
            await Page.WaitForLoadStateAsync(state ?? LoadState.NetworkIdle);
        }

        /// <summary>
        /// Disposes of Playwright resources when the fixture is no longer needed.
        /// </summary>
        /// <returns>ValueTask representing the asynchronous dispose operation.</returns>
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (Page != null)
                {
                    await Page.CloseAsync();
                    Page = null;
                }
            }
            catch (Exception) { /* Suppress errors during cleanup */ }

            try
            {
                if (Context != null)
                {
                    await Context.CloseAsync();
                    Context = null;
                }
            }
            catch (Exception) { /* Suppress errors during cleanup */ }

            try
            {
                if (Browser != null)
                {
                    await Browser.CloseAsync();
                    Browser = null;
                }
            }
            catch (Exception) { /* Suppress errors during cleanup */ }

            try
            {
                if (Playwright != null)
                {
                    Playwright.Dispose();
                    Playwright = null;
                }
            }
            catch (Exception) { /* Suppress errors during cleanup */ }
        }
    }
}