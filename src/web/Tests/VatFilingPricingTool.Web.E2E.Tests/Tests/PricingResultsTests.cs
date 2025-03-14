using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.E2E.Tests.Fixtures;
using VatFilingPricingTool.Web.E2E.Tests.PageObjects;
using VatFilingPricingTool.Web.E2E.Tests.Helpers;

namespace VatFilingPricingTool.Web.E2E.Tests.Tests
{
    /// <summary>
    /// Contains end-to-end tests for the pricing results functionality
    /// </summary>
    public class PricingResultsTests : IAsyncDisposable
    {
        private readonly PlaywrightFixture Fixture;
        private readonly LoginPage LoginPage;
        private readonly CalculatorPage CalculatorPage;
        private readonly ResultsPage ResultsPage;

        /// <summary>
        /// Initializes a new instance of the PricingResultsTests class
        /// </summary>
        /// <param name="fixture">The Playwright fixture that manages the browser instance</param>
        public PricingResultsTests(PlaywrightFixture fixture)
        {
            Fixture = fixture;
            LoginPage = new LoginPage(fixture);
            CalculatorPage = new CalculatorPage(fixture);
            ResultsPage = new ResultsPage(fixture);
        }

        /// <summary>
        /// Performs cleanup after tests are complete
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            // PlaywrightFixture cleanup is handled by the test framework
            await ValueTask.CompletedTask;
        }

        /// <summary>
        /// Common setup for tests that require authentication and navigation to results page
        /// </summary>
        private async Task<ResultsPage> SetupTestAsync()
        {
            // Get test user
            var user = new UserData().GetStandardUser();
            
            // Login
            await LoginPage.LoginAsync(user);
            
            // Navigate to calculator
            await CalculatorPage.NavigateToCalculatorPageAsync();
            
            // Setup test data
            var countries = new List<string> { "United Kingdom", "Germany", "France" };
            
            // Fill calculator form and submit
            return await CalculatorPage.FillCalculatorFormAsync(
                countries, 
                "Complex", 
                500, 
                "Quarterly",
                new List<string> { "tax-consultancy" });
        }

        [Fact]
        public async Task Test_ResultsPage_DisplaysCorrectTotalCost()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Assert
            var totalCost = await ResultsPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty("because the total cost should be displayed");
            totalCost.Should().Contain("€", "because the total should include a currency symbol");
            totalCost.Should().Contain("per quarter", "because the filing frequency is quarterly");
            
            // Verify that the total cost contains a numeric value
            totalCost.Should().MatchRegex(@"€[\d,]+(\.\d+)?", "because the total should contain a numeric value");
        }

        [Fact]
        public async Task Test_ResultsPage_DisplaysCountryBreakdown()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Assert
            var countryBreakdown = await ResultsPage.GetCountryBreakdownAsync();
            
            countryBreakdown.Should().NotBeEmpty("because the country breakdown should be displayed");
            countryBreakdown.Should().ContainKey("United Kingdom", "because UK was selected");
            countryBreakdown.Should().ContainKey("Germany", "because Germany was selected");
            countryBreakdown.Should().ContainKey("France", "because France was selected");
            
            // Verify that each country has a cost
            foreach (var cost in countryBreakdown.Values)
            {
                cost.Should().MatchRegex(@"€[\d,]+(\.\d+)?", "because each country should have a cost value");
            }
        }

        [Fact]
        public async Task Test_ResultsPage_DisplaysServiceDetails()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Assert
            var serviceDetails = await ResultsPage.GetServiceDetailsAsync();
            
            serviceDetails.Should().NotBeEmpty("because the service details should be displayed");
            serviceDetails.Should().ContainKey("Service Type", "because service type should be displayed");
            serviceDetails.Should().ContainKey("Transaction Volume", "because transaction volume should be displayed");
            serviceDetails.Should().ContainKey("Filing Frequency", "because filing frequency should be displayed");
            
            serviceDetails["Service Type"].Should().Contain("Complex Filing", "because complex filing was selected");
            serviceDetails["Transaction Volume"].Should().Contain("500", "because 500 was entered");
            serviceDetails["Filing Frequency"].Should().Contain("Quarterly", "because quarterly was selected");
        }

        [Fact]
        public async Task Test_ResultsPage_DisplaysDiscounts()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Assert
            var discounts = await ResultsPage.GetDiscountsAsync();
            
            // There should be at least one discount applied (either volume or multi-country)
            discounts.Should().NotBeEmpty("because at least one discount should be applied");
            
            // Verify that each discount has a value
            foreach (var discount in discounts.Values)
            {
                discount.Should().MatchRegex(@"-€[\d,]+(\.\d+)?", "because each discount should have a monetary value");
            }
        }

        [Fact]
        public async Task Test_ResultsPage_ViewDetailedBreakdown()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Click to view detailed breakdown
            await ResultsPage.ClickViewDetailedBreakdownAsync();
            
            // Assert - Take a screenshot for visual verification
            // In a real test, you would verify that additional details are shown
            await Fixture.TakeScreenshotAsync("DetailedBreakdown");
            
            // Additional assertions would depend on the implementation details
            // For example, checking for additional elements that become visible
        }

        [Fact]
        public async Task Test_ResultsPage_EditInputsNavigatesToCalculator()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Click edit inputs
            await ResultsPage.ClickEditInputsAsync();
            
            // Assert
            (await CalculatorPage.IsCalculatorPageDisplayedAsync())
                .Should().BeTrue("because clicking Edit Inputs should navigate back to the calculator");
            
            // In a complete test, you would also verify that the previous selections are preserved
        }

        [Fact]
        public async Task Test_ResultsPage_GenerateReport()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Generate a report
            await ResultsPage.GenerateReportAsync(
                "Q2 2023 VAT Filing Estimate", 
                "PDF", 
                includeCountryBreakdown: true,
                includeServiceDetails: true,
                includeAppliedDiscounts: true);
            
            // Assert
            var successMessage = await ResultsPage.GetSuccessMessageAsync();
            successMessage.Should().NotBeNullOrEmpty("because a success message should be displayed");
            successMessage.Should().Contain("report", "because the message should confirm report generation");
        }

        [Fact]
        public async Task Test_ResultsPage_SaveEstimate()
        {
            // Arrange & Act
            await SetupTestAsync();
            
            // Save the estimate
            await ResultsPage.SaveEstimateAsync("Q2 2023 VAT Estimate", "Estimate for quarterly VAT filing");
            
            // Assert
            var successMessage = await ResultsPage.GetSuccessMessageAsync();
            successMessage.Should().NotBeNullOrEmpty("because a success message should be displayed");
            successMessage.Should().Contain("saved", "because the message should confirm the estimate was saved");
        }

        [Fact]
        public async Task Test_ResultsPage_NavigateToExistingResult()
        {
            // Arrange
            var user = new UserData().GetStandardUser();
            await LoginPage.LoginAsync(user);
            
            // Act - Navigate to a known result ID
            // Note: In a real test, you would need a valid calculation ID
            string calculationId = "test-calculation-id";
            await ResultsPage.NavigateToResultsPageAsync(calculationId);
            
            // Assert
            (await ResultsPage.IsResultsPageDisplayedAsync())
                .Should().BeTrue("because navigating to an existing result should display the results page");
            
            var totalCost = await ResultsPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty("because the total cost should be displayed");
        }

        [Fact]
        public async Task Test_ResultsPage_MultiCountryCalculation()
        {
            // Arrange
            var user = new UserData().GetStandardUser();
            await LoginPage.LoginAsync(user);
            await CalculatorPage.NavigateToCalculatorPageAsync();
            
            // Act - Select 5 countries for multi-country testing
            var countries = new List<string> { "United Kingdom", "Germany", "France", "Italy", "Spain" };
            await CalculatorPage.FillCalculatorFormAsync(
                countries, 
                "Complex", 
                500, 
                "Quarterly",
                new List<string> { "tax-consultancy" });
            
            // Assert
            var countryBreakdown = await ResultsPage.GetCountryBreakdownAsync();
            
            countryBreakdown.Should().NotBeEmpty("because the country breakdown should be displayed");
            countryBreakdown.Keys.Should().Contain("United Kingdom", "because UK was selected");
            countryBreakdown.Keys.Should().Contain("Germany", "because Germany was selected");
            countryBreakdown.Keys.Should().Contain("France", "because France was selected");
            countryBreakdown.Keys.Should().Contain("Italy", "because Italy was selected");
            countryBreakdown.Keys.Should().Contain("Spain", "because Spain was selected");
            
            // Verify multi-country discount is applied
            var discounts = await ResultsPage.GetDiscountsAsync();
            discounts.Keys.Should().Contain("Multi-country Discount", "because a multi-country discount should be applied for 5 countries");
        }
    }
}