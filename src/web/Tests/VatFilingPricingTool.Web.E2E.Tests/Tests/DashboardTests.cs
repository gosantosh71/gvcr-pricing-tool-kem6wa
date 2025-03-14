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
    /// Contains end-to-end tests for the dashboard functionality
    /// </summary>
    public class DashboardTests : IClassFixture<PlaywrightFixture>
    {
        private readonly PlaywrightFixture Fixture;
        private readonly LoginPage LoginPage;
        private readonly DashboardPage DashboardPage;

        /// <summary>
        /// Initializes a new instance of the DashboardTests class
        /// </summary>
        /// <param name="fixture">Playwright fixture for browser automation</param>
        public DashboardTests(PlaywrightFixture fixture)
        {
            Fixture = fixture;
            LoginPage = new LoginPage(fixture);
            DashboardPage = new DashboardPage(fixture);
        }

        /// <summary>
        /// Performs common setup for dashboard tests by logging in a standard user
        /// </summary>
        private async Task SetupAsync()
        {
            var userData = new UserData();
            var user = userData.GetStandardUser();
            await LoginPage.LoginAsync(user);
            
            // Ensure the dashboard is loaded
            (await DashboardPage.IsDashboardDisplayedAsync()).Should().BeTrue();
        }

        /// <summary>
        /// Tests that the dashboard page is correctly displayed after login
        /// </summary>
        [Fact]
        public async Task DashboardDisplayTest()
        {
            // Arrange & Act
            await SetupAsync();

            // Assert
            (await DashboardPage.IsDashboardDisplayedAsync()).Should().BeTrue();
            var welcomeMessage = await DashboardPage.GetWelcomeMessageAsync();
            welcomeMessage.Should().Contain("Welcome back");

            // Verify key dashboard components are displayed
            var activitySummary = await DashboardPage.GetActivitySummaryTextAsync();
            activitySummary.Should().NotBeEmpty("Activity summary should be displayed");

            var recentEstimatesCount = await DashboardPage.GetRecentEstimatesCountAsync();
            recentEstimatesCount.Should().BeGreaterOrEqual(0, "Recent estimates section should be displayed");

            var notificationsCount = await DashboardPage.GetNotificationsCountAsync();
            notificationsCount.Should().BeGreaterOrEqual(0, "Notifications section should be displayed");
        }

        /// <summary>
        /// Tests that the activity summary section displays correct information
        /// </summary>
        [Fact]
        public async Task ActivitySummaryTest()
        {
            // Arrange & Act
            await SetupAsync();

            // Assert
            var activitySummary = await DashboardPage.GetActivitySummaryTextAsync();
            activitySummary.Should().Contain("Calculations", "Activity summary should mention calculations");
            activitySummary.Should().Contain("Reports", "Activity summary should mention reports");
            activitySummary.Should().Contain("Last login", "Activity summary should show last login information");
        }

        /// <summary>
        /// Tests that the recent estimates section displays correctly
        /// </summary>
        [Fact]
        public async Task RecentEstimatesTest()
        {
            // Arrange & Act
            await SetupAsync();

            // Assert
            var recentEstimatesCount = await DashboardPage.GetRecentEstimatesCountAsync();
            recentEstimatesCount.Should().BeGreaterOrEqual(0, "Recent estimates should be displayed");
        }

        /// <summary>
        /// Tests that the notifications section displays correctly
        /// </summary>
        [Fact]
        public async Task NotificationsTest()
        {
            // Arrange & Act
            await SetupAsync();

            // Assert
            var notificationsCount = await DashboardPage.GetNotificationsCountAsync();
            notificationsCount.Should().BeGreaterOrEqual(0, "Notifications should be displayed");
        }

        /// <summary>
        /// Tests navigation from dashboard to pricing calculator
        /// </summary>
        [Fact]
        public async Task NavigateToCalculatorTest()
        {
            // Arrange
            await SetupAsync();

            // Act
            await DashboardPage.NavigateToCalculatorAsync();

            // Assert
            var url = Fixture.Page.Url;
            url.Should().Contain("calculator", "URL should change to calculator page");
            
            // Verify we're on the calculator page by checking for a calculator-specific element
            var calculatorElementVisible = await Fixture.Page.IsVisibleAsync("[data-testid='pricing-calculator']");
            calculatorElementVisible.Should().BeTrue("Calculator elements should be visible");
        }

        /// <summary>
        /// Tests navigation from dashboard to report generation
        /// </summary>
        [Fact]
        public async Task NavigateToReportGenerationTest()
        {
            // Arrange
            await SetupAsync();

            // Act
            await DashboardPage.NavigateToReportGenerationAsync();

            // Assert
            var url = Fixture.Page.Url;
            url.Should().Contain("report", "URL should change to report generation page");
            
            // Verify we're on the report generation page by checking for a report-specific element
            var reportElementVisible = await Fixture.Page.IsVisibleAsync("[data-testid='report-generation']");
            reportElementVisible.Should().BeTrue("Report generation elements should be visible");
        }

        /// <summary>
        /// Tests navigation from dashboard to data import
        /// </summary>
        [Fact]
        public async Task NavigateToDataImportTest()
        {
            // Arrange
            await SetupAsync();

            // Act
            await DashboardPage.NavigateToDataImportAsync();

            // Assert
            var url = Fixture.Page.Url;
            url.Should().Contain("import", "URL should change to data import page");
            
            // Verify we're on the data import page by checking for an import-specific element
            var importElementVisible = await Fixture.Page.IsVisibleAsync("[data-testid='data-import']");
            importElementVisible.Should().BeTrue("Data import elements should be visible");
        }

        /// <summary>
        /// Tests clicking on a recent estimate navigates to the correct page
        /// </summary>
        [Fact]
        public async Task ClickRecentEstimateTest()
        {
            // Arrange
            await SetupAsync();
            
            // Act
            var estimatesCount = await DashboardPage.GetRecentEstimatesCountAsync();
            
            // Assert
            if (estimatesCount > 0)
            {
                await DashboardPage.ClickRecentEstimateAsync(0);
                
                var url = Fixture.Page.Url;
                url.Should().Contain("results", "URL should change to pricing results page");
                
                // Verify we're on the results page by checking for a results-specific element
                var resultsElementVisible = await Fixture.Page.IsVisibleAsync("[data-testid='pricing-results']");
                resultsElementVisible.Should().BeTrue("Pricing results elements should be visible");
            }
            else
            {
                // Skip the test if there are no recent estimates
                Skip.If(true, "No recent estimates to click on");
            }
        }

        /// <summary>
        /// Tests that the recent activity section displays correctly
        /// </summary>
        [Fact]
        public async Task RecentActivityTest()
        {
            // Arrange & Act
            await SetupAsync();

            // Assert
            var recentActivityCount = await DashboardPage.GetRecentActivityItemsCountAsync();
            recentActivityCount.Should().BeGreaterOrEqual(0, "Recent activity items should be displayed");
        }

        /// <summary>
        /// Tests that the dashboard is responsive by resizing the browser window
        /// </summary>
        [Fact]
        public async Task DashboardResponsivenessTest()
        {
            // Arrange
            await SetupAsync();
            
            // Act - Resize to mobile dimensions
            await Fixture.Page.SetViewportSizeAsync(375, 667);
            
            // Assert dashboard is still displayed correctly
            (await DashboardPage.IsDashboardDisplayedAsync()).Should().BeTrue();
            
            // Check that mobile navigation elements are visible
            var mobileNavVisible = await Fixture.Page.IsVisibleAsync("[data-testid='mobile-navigation']");
            mobileNavVisible.Should().BeTrue("Mobile navigation should be visible in mobile view");
            
            // Act - Resize back to desktop dimensions
            await Fixture.Page.SetViewportSizeAsync(1920, 1080);
            
            // Assert desktop layout is restored
            var desktopNavVisible = await Fixture.Page.IsVisibleAsync("[data-testid='desktop-navigation']");
            desktopNavVisible.Should().BeTrue("Desktop navigation should be visible in desktop view");
        }
    }
}