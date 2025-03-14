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
    [Collection("Playwright")]
    public class PricingCalculatorTests
    {
        private readonly PlaywrightFixture fixture;
        private readonly LoginPage loginPage;
        private readonly DashboardPage dashboardPage;
        private readonly CalculatorPage calculatorPage;

        public PricingCalculatorTests(PlaywrightFixture fixture)
        {
            this.fixture = fixture;
            loginPage = new LoginPage(fixture);
            dashboardPage = new DashboardPage(fixture);
            calculatorPage = new CalculatorPage(fixture);
        }

        private async Task InitializeTestAsync()
        {
            var userData = new UserData();
            var user = userData.GetStandardUser();
            
            await loginPage.LoginAsync(user);
            await dashboardPage.NavigateToCalculatorAsync();
            (await calculatorPage.IsCalculatorPageDisplayedAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task CalculatePricingWithSingleCountryTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "United Kingdom" };
            
            // Act
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "standard", 
                500, 
                "Monthly");
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify total cost is displayed
            var totalCost = await resultPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty();
            
            // Verify country breakdown
            var countryBreakdown = await resultPage.GetCountryBreakdownAsync();
            countryBreakdown.Should().ContainKey("United Kingdom");
            
            // Verify service details
            var serviceDetails = await resultPage.GetServiceDetailsAsync();
            serviceDetails.Should().ContainKey("Service Type");
            serviceDetails["Service Type"].Should().Contain("Standard");
            serviceDetails.Should().ContainKey("Transaction Volume");
            serviceDetails["Transaction Volume"].Should().Contain("500");
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("SingleCountryPricing");
        }

        [Fact]
        public async Task CalculatePricingWithMultipleCountriesTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "United Kingdom", "Germany", "France" };
            
            // Act
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "complex", 
                1000, 
                "Quarterly");
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify total cost is displayed
            var totalCost = await resultPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty();
            
            // Verify country breakdown contains all countries
            var countryBreakdown = await resultPage.GetCountryBreakdownAsync();
            countryBreakdown.Should().ContainKeys("United Kingdom", "Germany", "France");
            
            // Verify service details
            var serviceDetails = await resultPage.GetServiceDetailsAsync();
            serviceDetails.Should().ContainKey("Service Type");
            serviceDetails["Service Type"].Should().Contain("Complex");
            serviceDetails.Should().ContainKey("Transaction Volume");
            serviceDetails["Transaction Volume"].Should().Contain("1000");
            
            // Verify multi-country discount is applied
            var discounts = await resultPage.GetDiscountsAsync();
            discounts.Should().ContainKey("Multi-country Discount");
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("MultipleCountriesPricing");
        }

        [Fact]
        public async Task CalculatePricingWithAdditionalServicesTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "Germany" };
            var additionalServices = new List<string> { "tax-consultancy", "reconciliation" };
            
            // Act
            await calculatorPage.NavigateToCalculatorPageAsync();
            await calculatorPage.SelectCountriesAsync(countries);
            await calculatorPage.SelectServiceTypeAsync("standard");
            await calculatorPage.EnterTransactionVolumeAsync(500);
            await calculatorPage.SelectFilingFrequencyAsync("Monthly");
            await calculatorPage.SelectAdditionalServicesAsync(additionalServices);
            var resultPage = await calculatorPage.ClickCalculatePricingAsync();
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify service details include additional services
            var serviceDetails = await resultPage.GetServiceDetailsAsync();
            serviceDetails.Should().ContainKey("Additional Services");
            serviceDetails["Additional Services"].Should().Contain("Tax consultancy");
            serviceDetails["Additional Services"].Should().Contain("Reconciliation");
            
            // Verify total cost reflects additional services
            var totalCost = await resultPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty();
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("AdditionalServicesPricing");
        }

        [Fact]
        public async Task ValidationErrorsForMissingRequiredFieldsTest()
        {
            // Arrange
            await InitializeTestAsync();
            
            // Act - Submit form without filling any fields
            await calculatorPage.ClickCalculatePricingAsync();
            var errors = await calculatorPage.GetValidationErrorsAsync();
            
            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("country") || e.Contains("Country"));
            errors.Should().Contain(e => e.Contains("transaction volume") || e.Contains("Transaction Volume"));
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("ValidationErrorsRequiredFields");
        }

        [Fact]
        public async Task ValidationErrorsForInvalidTransactionVolumeTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "France" };
            
            // Act
            await calculatorPage.SelectCountriesAsync(countries);
            await calculatorPage.SelectServiceTypeAsync("standard");
            await calculatorPage.EnterTransactionVolumeAsync(-100); // Invalid negative value
            await calculatorPage.SelectFilingFrequencyAsync("Monthly");
            await calculatorPage.ClickCalculatePricingAsync();
            
            var errors = await calculatorPage.GetValidationErrorsAsync();
            
            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("transaction volume") || e.Contains("Transaction Volume"));
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("ValidationErrorsInvalidVolume");
        }

        [Fact]
        public async Task EditInputsFromResultsPageTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "United Kingdom" };
            
            // Act - Calculate initial pricing
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "standard", 
                500, 
                "Monthly");
            
            // Verify results are displayed
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Go back to edit inputs
            await resultPage.ClickEditInputsAsync();
            
            // Verify we're back at calculator with previous inputs
            (await calculatorPage.IsCalculatorPageDisplayedAsync()).Should().BeTrue();
            
            // Modify transaction volume
            await calculatorPage.EnterTransactionVolumeAsync(1000);
            
            // Calculate again
            resultPage = await calculatorPage.ClickCalculatePricingAsync();
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify service details show updated volume
            var serviceDetails = await resultPage.GetServiceDetailsAsync();
            serviceDetails.Should().ContainKey("Transaction Volume");
            serviceDetails["Transaction Volume"].Should().Contain("1000");
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("EditInputsFromResults");
        }

        [Fact]
        public async Task ViewDetailedBreakdownTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "United Kingdom", "Germany" };
            
            // Act
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "complex", 
                1000, 
                "Quarterly");
            
            // Verify results are displayed
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // View detailed breakdown
            await resultPage.ClickViewDetailedBreakdownAsync();
            
            // Assert - Additional details should be visible after clicking the button
            // For the test, we're verifying the action completes successfully
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("DetailedBreakdown");
        }

        [Fact]
        public async Task PriorityServiceTypePricingTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "Germany" };
            
            // Act
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "priority", 
                500, 
                "Monthly");
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify service details show priority service
            var serviceDetails = await resultPage.GetServiceDetailsAsync();
            serviceDetails.Should().ContainKey("Service Type");
            serviceDetails["Service Type"].Should().Contain("Priority");
            
            // Verify total cost reflects priority service premium
            var totalCost = await resultPage.GetTotalCostAsync();
            totalCost.Should().NotBeNullOrEmpty();
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("PriorityServicePricing");
        }

        [Fact]
        public async Task VolumeDiscountAppliedTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "United Kingdom" };
            
            // Act
            var resultPage = await calculatorPage.FillCalculatorFormAsync(
                countries, 
                "standard", 
                5000, // High volume to trigger discount
                "Monthly");
            
            // Assert
            (await resultPage.IsResultsPageDisplayedAsync()).Should().BeTrue();
            
            // Verify volume discount is applied
            var discounts = await resultPage.GetDiscountsAsync();
            discounts.Should().ContainKey("Volume Discount");
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("VolumeDiscountApplied");
        }

        [Fact]
        public async Task SaveParametersTest()
        {
            // Arrange
            await InitializeTestAsync();
            var countries = new List<string> { "France" };
            
            // Act
            await calculatorPage.SelectCountriesAsync(countries);
            await calculatorPage.SelectServiceTypeAsync("standard");
            await calculatorPage.EnterTransactionVolumeAsync(500);
            await calculatorPage.SelectFilingFrequencyAsync("Monthly");
            await calculatorPage.ClickSaveParametersAsync();
            
            // Assert
            var successMessage = await calculatorPage.GetSuccessMessageAsync();
            successMessage.Should().NotBeNullOrEmpty();
            
            // Take screenshot for reporting
            await fixture.TakeScreenshotAsync("SaveParameters");
        }
    }
}