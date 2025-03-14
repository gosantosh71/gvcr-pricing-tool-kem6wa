using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;
using VatFilingPricingTool.Web.E2E.Tests.PageObjects;
using VatFilingPricingTool.Web.E2E.Tests.Helpers;

namespace VatFilingPricingTool.Web.E2E.Tests.Tests
{
    /// <summary>
    /// Contains end-to-end tests for the report generation functionality
    /// </summary>
    [Collection("Playwright")]
    public class ReportGenerationTests
    {
        private readonly PlaywrightFixture fixture;
        private readonly LoginPage loginPage;
        private readonly DashboardPage dashboardPage;

        /// <summary>
        /// Initializes a new instance of the ReportGenerationTests class
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance</param>
        public ReportGenerationTests(PlaywrightFixture fixture)
        {
            this.fixture = fixture;
            loginPage = new LoginPage(fixture);
            dashboardPage = new DashboardPage(fixture);
        }

        /// <summary>
        /// Initializes the test by logging in and navigating to the report generation page
        /// </summary>
        /// <returns>Asynchronous task representing the initialization operation</returns>
        private async Task InitializeTestAsync()
        {
            var user = new UserData().GetStandardUser();
            await loginPage.LoginAsync(user);
            await dashboardPage.NavigateToReportGenerationAsync();
            
            // Verify we're on the report generation page
            await fixture.Page.WaitForSelectorAsync("[data-testid='report-title-input']");
        }

        /// <summary>
        /// Tests the generation of a PDF report with default options
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GeneratePdfReportTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Q2 2023 VAT Filing Estimate");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Default content options (Country Breakdown, Service Details, Applied Discounts) are already checked
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("pdf_report_generated");
        }

        /// <summary>
        /// Tests the generation of an Excel report with custom options
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateExcelReportTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Monthly VAT Analysis");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select Excel format
            await fixture.Page.CheckAsync("[data-testid='format-excel']");
            
            // Select all content options
            await fixture.Page.CheckAsync("[data-testid='content-country-breakdown']");
            await fixture.Page.CheckAsync("[data-testid='content-service-details']");
            await fixture.Page.CheckAsync("[data-testid='content-applied-discounts']");
            await fixture.Page.CheckAsync("[data-testid='content-historical-comparison']");
            await fixture.Page.CheckAsync("[data-testid='content-tax-rate-details']");
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("excel_report_generated");
        }

        /// <summary>
        /// Tests the generation of a report with email delivery option
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateReportWithEmailDeliveryTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Quarterly VAT Summary");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Default content options are already checked
            
            // Select 'Email to' delivery method and verify email field appears
            await fixture.Page.CheckAsync("[data-testid='delivery-email']");
            await fixture.Page.WaitForSelectorAsync("[data-testid='email-input']");
            
            // Verify the email field is pre-filled with the user's email
            var emailValue = await fixture.Page.InputValueAsync("[data-testid='email-input']");
            emailValue.Should().NotBeEmpty();
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success message about email delivery
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report has been sent to your email");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("email_report_delivery");
        }

        /// <summary>
        /// Tests that validation errors are displayed for missing report title
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task ValidationErrorsForMissingReportTitleTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Leave report title empty
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Default content options are already checked
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Get validation errors
            await fixture.Page.WaitForSelectorAsync("[data-testid='validation-error']");
            var errorMessage = await fixture.Page.TextContentAsync("[data-testid='validation-error']");
            
            // Verify that validation errors include a message about required report title
            errorMessage.Should().Contain("Report title is required");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("missing_title_validation");
        }

        /// <summary>
        /// Tests that validation errors are displayed for missing calculation selection
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task ValidationErrorsForMissingCalculationSelectionTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Test Report");
            
            // Do not select any calculation
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Default content options are already checked
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Get validation errors
            await fixture.Page.WaitForSelectorAsync("[data-testid='validation-error']");
            var errorMessage = await fixture.Page.TextContentAsync("[data-testid='validation-error']");
            
            // Verify that validation errors include a message about required calculation selection
            errorMessage.Should().Contain("Please select a calculation");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("missing_calculation_validation");
        }

        /// <summary>
        /// Tests the generation of a report with historical comparison option
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateReportWithHistoricalComparisonTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Historical VAT Comparison");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Select content options including 'Historical Comparison'
            await fixture.Page.CheckAsync("[data-testid='content-country-breakdown']");
            await fixture.Page.CheckAsync("[data-testid='content-service-details']");
            await fixture.Page.CheckAsync("[data-testid='content-applied-discounts']");
            await fixture.Page.CheckAsync("[data-testid='content-historical-comparison']");
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("historical_comparison_report");
        }

        /// <summary>
        /// Tests the generation of a report with tax rate details option
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateReportWithTaxRateDetailsTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "VAT Rate Analysis");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Select content options including 'Tax Rate Details'
            await fixture.Page.CheckAsync("[data-testid='content-country-breakdown']");
            await fixture.Page.CheckAsync("[data-testid='content-service-details']");
            await fixture.Page.CheckAsync("[data-testid='content-applied-discounts']");
            await fixture.Page.CheckAsync("[data-testid='content-tax-rate-details']");
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("tax_rate_details_report");
        }

        /// <summary>
        /// Tests the generation of a report as an accountant user
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateReportAsAccountantUserTest()
        {
            // Get an accountant test user
            var accountantUser = new UserData().GetAccountantUser();
            
            // Login using the accountant user credentials
            await loginPage.LoginAsync(accountantUser);
            
            // Navigate to report generation page
            await dashboardPage.NavigateToReportGenerationAsync();
            
            // Verify report generation page is displayed
            await fixture.Page.WaitForSelectorAsync("[data-testid='report-title-input']");
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Accountant VAT Report");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select PDF format
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Default content options are already checked
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Report generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("accountant_report_generated");
        }

        /// <summary>
        /// Tests the generation of both PDF and Excel report formats
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task GenerateBothPdfAndExcelReportTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Set report title
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Dual Format Report");
            
            // Select calculation
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            
            // Select 'Both' format option
            await fixture.Page.CheckAsync("[data-testid='format-both']");
            
            // Default content options are already checked
            
            // Select 'Download Immediately' delivery method
            await fixture.Page.CheckAsync("[data-testid='delivery-download']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Verify success
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            var successMessage = await fixture.Page.TextContentAsync("[data-testid='success-message']");
            successMessage.Should().Contain("Reports generated successfully");
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("both_formats_reports");
        }

        /// <summary>
        /// Tests navigation back to dashboard after report generation
        /// </summary>
        /// <returns>Asynchronous task representing the test operation</returns>
        [Fact]
        public async Task ReturnToDashboardAfterReportGenerationTest()
        {
            // Initialize the test environment
            await InitializeTestAsync();
            
            // Fill report form with valid data
            await fixture.Page.FillAsync("[data-testid='report-title-input']", "Test Report");
            await fixture.Page.SelectOptionAsync("[data-testid='calculation-dropdown']", "Current Calculation");
            await fixture.Page.CheckAsync("[data-testid='format-pdf']");
            
            // Generate report
            await fixture.Page.ClickAsync("[data-testid='generate-report-button']");
            
            // Wait for success message
            await fixture.Page.WaitForSelectorAsync("[data-testid='success-message']");
            
            // Return to dashboard
            await fixture.Page.ClickAsync("[data-testid='return-to-dashboard-button']");
            
            // Verify dashboard is displayed
            var isDashboardDisplayed = await dashboardPage.IsDashboardDisplayedAsync();
            isDashboardDisplayed.Should().BeTrue();
            
            // Take screenshot for test reporting
            await fixture.TakeScreenshotAsync("return_to_dashboard");
        }
    }
}