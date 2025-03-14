using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Playwright;
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;

namespace VatFilingPricingTool.Web.E2E.Tests.PageObjects
{
    /// <summary>
    /// Page object representing the Dashboard page in the VAT Filing Pricing Tool web application
    /// </summary>
    public class DashboardPage : BasePage
    {
        private const string PagePath = "/dashboard";
        private const string WelcomeMessageSelector = "text=Welcome back";
        private const string ActivitySummarySelector = "[data-testid='activity-summary']";
        private const string RecentEstimatesSelector = "[data-testid='recent-estimates']";
        private const string RecentEstimateItemSelector = "[data-testid='recent-estimate-item']";
        private const string NotificationsSelector = "[data-testid='notifications']";
        private const string NotificationItemSelector = "[data-testid='notification-item']";
        private const string NewCalculationButtonSelector = "[data-testid='new-calculation-button']";
        private const string GenerateReportButtonSelector = "[data-testid='generate-report-button']";
        private const string ImportDataButtonSelector = "[data-testid='import-data-button']";
        private const string ViewAllActivityButtonSelector = "[data-testid='view-all-activity-button']";
        private const string RecentActivitySelector = "[data-testid='recent-activity']";
        private const string RecentActivityItemSelector = "[data-testid='recent-activity-item']";

        /// <summary>
        /// Initializes a new instance of the DashboardPage class
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance</param>
        public DashboardPage(PlaywrightFixture fixture) : base(fixture, PagePath)
        {
        }

        /// <summary>
        /// Checks if the dashboard page is displayed
        /// </summary>
        /// <returns>True if the dashboard is displayed, otherwise false</returns>
        public async Task<bool> IsDashboardDisplayedAsync()
        {
            await WaitForElementToBeVisibleAsync(WelcomeMessageSelector);
            
            bool activitySummaryVisible = await IsElementVisibleAsync(ActivitySummarySelector);
            bool recentEstimatesVisible = await IsElementVisibleAsync(RecentEstimatesSelector);
            bool notificationsVisible = await IsElementVisibleAsync(NotificationsSelector);
            
            return activitySummaryVisible && recentEstimatesVisible && notificationsVisible;
        }

        /// <summary>
        /// Gets the welcome message text from the dashboard
        /// </summary>
        /// <returns>The welcome message text</returns>
        public async Task<string> GetWelcomeMessageAsync()
        {
            return await GetTextContentAsync(WelcomeMessageSelector);
        }

        /// <summary>
        /// Gets the activity summary text from the dashboard
        /// </summary>
        /// <returns>The activity summary text</returns>
        public async Task<string> GetActivitySummaryTextAsync()
        {
            return await GetTextContentAsync(ActivitySummarySelector);
        }

        /// <summary>
        /// Gets the count of recent estimates displayed on the dashboard
        /// </summary>
        /// <returns>The number of recent estimates</returns>
        public async Task<int> GetRecentEstimatesCountAsync()
        {
            var elements = await Page.QuerySelectorAllAsync(RecentEstimateItemSelector);
            return elements.Count;
        }

        /// <summary>
        /// Gets the count of notifications displayed on the dashboard
        /// </summary>
        /// <returns>The number of notifications</returns>
        public async Task<int> GetNotificationsCountAsync()
        {
            var elements = await Page.QuerySelectorAllAsync(NotificationItemSelector);
            return elements.Count;
        }

        /// <summary>
        /// Navigates to the pricing calculator page by clicking the New Calculation button
        /// </summary>
        /// <returns>Asynchronous task representing the navigation operation</returns>
        public async Task NavigateToCalculatorAsync()
        {
            await ClickAsync(NewCalculationButtonSelector);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Navigates to the report generation page by clicking the Generate Report button
        /// </summary>
        /// <returns>Asynchronous task representing the navigation operation</returns>
        public async Task NavigateToReportGenerationAsync()
        {
            await ClickAsync(GenerateReportButtonSelector);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Navigates to the data import page by clicking the Import Data button
        /// </summary>
        /// <returns>Asynchronous task representing the navigation operation</returns>
        public async Task NavigateToDataImportAsync()
        {
            await ClickAsync(ImportDataButtonSelector);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Navigates to the full activity history page by clicking the View All Activity button
        /// </summary>
        /// <returns>Asynchronous task representing the navigation operation</returns>
        public async Task ViewAllActivityAsync()
        {
            await ClickAsync(ViewAllActivityButtonSelector);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Clicks on a recent estimate item at the specified index
        /// </summary>
        /// <param name="index">The zero-based index of the recent estimate to click</param>
        /// <returns>Asynchronous task representing the click operation</returns>
        public async Task ClickRecentEstimateAsync(int index)
        {
            var elements = await Page.QuerySelectorAllAsync(RecentEstimateItemSelector);
            if (index < 0 || index >= elements.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), $"Index {index} is out of range. Only {elements.Count} estimates are available.");
            }

            await elements[index].ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Gets the count of recent activity items displayed on the dashboard
        /// </summary>
        /// <returns>The number of recent activity items</returns>
        public async Task<int> GetRecentActivityItemsCountAsync()
        {
            var elements = await Page.QuerySelectorAllAsync(RecentActivityItemSelector);
            return elements.Count;
        }

        /// <summary>
        /// Gets the text content of the recent activity section
        /// </summary>
        /// <returns>The text content of the recent activity section</returns>
        public async Task<string> GetRecentActivityTextAsync()
        {
            return await GetTextContentAsync(RecentActivitySelector);
        }
    }
}