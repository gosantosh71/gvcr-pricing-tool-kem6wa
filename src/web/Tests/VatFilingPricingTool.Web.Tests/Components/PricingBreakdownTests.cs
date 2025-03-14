using Bunit; // bunit version 1.12.6
using Xunit; // xunit version 2.4.2
using System;
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Linq; // System.Linq v6.0.0
using VatFilingPricingTool.Web.Tests.Helpers;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Components;

namespace VatFilingPricingTool.Web.Tests.Components
{
    /// <summary>
    /// Test class for the PricingBreakdown component
    /// </summary>
    public class PricingBreakdownTests : IDisposable
    {
        private TestContext context;
        private CalculationResultModel testCalculation;

        /// <summary>
        /// Default constructor for the PricingBreakdownTests class
        /// </summary>
        public PricingBreakdownTests()
        {
            // Initialize properties to null
            context = null;
            testCalculation = null;
        }

        /// <summary>
        /// Sets up the test context for each test
        /// </summary>
        private void Setup()
        {
            context = RenderComponent.CreateTestContext();
            testCalculation = TestData.CreateTestCalculationResult("calc-123", "user-123");
        }

        /// <summary>
        /// Cleans up resources after each test
        /// </summary>
        public void Dispose()
        {
            context?.Dispose();
            context = null;
            testCalculation = null;
        }

        /// <summary>
        /// Tests that the PricingBreakdown component renders without errors
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldRender_WithoutErrors()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDetailedBreakdown, false)
                .Add(p => p.ShowServiceDetails, false)
                .Add(p => p.ShowDiscounts, false));
            
            // Assert
            Assert.NotNull(component);
            var totalCostElement = component.Find(".total-cost");
            Assert.Contains(testCalculation.FormattedTotalCost, totalCostElement.TextContent);
            Assert.Contains("Cost Breakdown by Country", component.Markup);
            Assert.Contains("Service Details", component.Markup);
        }

        /// <summary>
        /// Tests that the total cost is displayed with correct formatting
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldDisplayTotalCost_WithCorrectFormatting()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation));
            
            // Assert
            var totalCostElement = component.Find(".total-cost");
            Assert.Equal(testCalculation.FormattedTotalCost, totalCostElement.TextContent);
            Assert.Contains(testCalculation.CurrencyCode, component.Markup);
        }

        /// <summary>
        /// Tests that clicking the detailed breakdown button toggles the visibility of country details
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldToggleDetailedBreakdown_WhenButtonClicked()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDetailedBreakdown, false));
            
            // Assert - Initially not visible
            Assert.False(component.Instance.ShowDetailedBreakdown);
            Assert.DoesNotContain("table-responsive", component.Markup);
            
            // Act - Toggle breakdown
            component.Find(".country-breakdown .section-header").Click();
            
            // Assert - Now visible
            Assert.True(component.Instance.ShowDetailedBreakdown);
            Assert.Contains("table-responsive", component.Markup);
            
            // Act - Toggle again
            component.Find(".country-breakdown .section-header").Click();
            
            // Assert - Hidden again
            Assert.False(component.Instance.ShowDetailedBreakdown);
            Assert.DoesNotContain("table-responsive", component.Markup);
        }

        /// <summary>
        /// Tests that clicking the service details button toggles the visibility of service information
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldToggleServiceDetails_WhenButtonClicked()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowServiceDetails, false));
            
            // Assert - Initially not visible
            Assert.False(component.Instance.ShowServiceDetails);
            
            // Act - Toggle service details
            component.Find(".service-details .section-header").Click();
            
            // Assert - Now visible
            Assert.True(component.Instance.ShowServiceDetails);
            Assert.Contains("Service Type:", component.Markup);
            
            // Act - Toggle again
            component.Find(".service-details .section-header").Click();
            
            // Assert - Hidden again
            Assert.False(component.Instance.ShowServiceDetails);
        }

        /// <summary>
        /// Tests that clicking the discounts button toggles the visibility of discount information
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldToggleDiscounts_WhenButtonClicked()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDiscounts, false));
            
            // Assert - Initially not visible
            Assert.False(component.Instance.ShowDiscounts);
            
            // Act - Toggle discounts
            component.Find(".discounts .section-header").Click();
            
            // Assert - Now visible
            Assert.True(component.Instance.ShowDiscounts);
            Assert.Contains("Discount Type", component.Markup);
            
            // Act - Toggle again
            component.Find(".discounts .section-header").Click();
            
            // Assert - Hidden again
            Assert.False(component.Instance.ShowDiscounts);
        }

        /// <summary>
        /// Tests that country breakdowns are displayed with correct data
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldDisplayCountryBreakdowns_WithCorrectData()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDetailedBreakdown, true));
            
            // Assert
            var tableRows = component.FindAll("tbody tr");
            
            // Verify the number of countries matches
            Assert.Equal(testCalculation.CountryBreakdowns.Count, tableRows.Count);
            
            // Verify each country's data
            for (int i = 0; i < testCalculation.CountryBreakdowns.Count; i++)
            {
                var country = testCalculation.CountryBreakdowns[i];
                var row = tableRows[i];
                
                // Verify country name is present
                Assert.Contains(country.CountryName, row.TextContent);
                
                // Verify base cost is present
                Assert.Contains(country.FormattedBaseCost, row.TextContent);
                
                // Verify total cost is present
                Assert.Contains(country.FormattedTotalCost, row.TextContent);
            }
            
            // Verify the total cost in the footer matches the calculation's total cost
            var footerRow = component.Find("tfoot tr");
            Assert.Contains(testCalculation.FormattedTotalCost, footerRow.TextContent);
        }

        /// <summary>
        /// Tests that service details are displayed with correct data
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldDisplayServiceDetails_WithCorrectData()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowServiceDetails, true));
            
            // Assert
            var serviceContent = component.Find(".service-details .section-content");
            
            // Verify service type is present
            Assert.Contains(testCalculation.ServiceTypeName, serviceContent.TextContent);
            
            // Verify transaction volume is present
            Assert.Contains(testCalculation.TransactionVolume.ToString(), serviceContent.TextContent);
            
            // Verify filing frequency is present
            Assert.Contains(testCalculation.FilingFrequencyName, serviceContent.TextContent);
            
            // Verify additional services are present
            foreach (var service in testCalculation.AdditionalServices)
            {
                Assert.Contains(service, serviceContent.TextContent);
            }
        }

        /// <summary>
        /// Tests that discounts are displayed with correct data
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldDisplayDiscounts_WithCorrectData()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDiscounts, true));
            
            // Assert
            var discountRows = component.FindAll(".discounts tbody tr");
            
            // Verify number of discounts matches
            Assert.Equal(testCalculation.Discounts.Count, discountRows.Count);
            
            // Verify each discount
            int index = 0;
            foreach (var discount in testCalculation.Discounts)
            {
                var row = discountRows[index++];
                
                // Verify discount name is present (in some formatted form)
                Assert.Contains(discount.Key.Replace("_", " "), row.TextContent, StringComparison.OrdinalIgnoreCase);
                
                // Verify discount amount is present (checking for currency symbol and value)
                Assert.Contains(testCalculation.CurrencyCode, row.TextContent);
                Assert.Contains(discount.Value.ToString("0.##"), row.TextContent, StringComparison.OrdinalIgnoreCase);
            }
            
            // Verify total discounts
            var footerRow = component.Find(".discounts tfoot tr");
            Assert.Contains(testCalculation.FormattedTotalDiscounts, footerRow.TextContent);
        }

        /// <summary>
        /// Tests that the OnViewDetailedBreakdown event is triggered when the detailed breakdown is toggled
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldTriggerViewDetailedBreakdownEvent_WhenToggled()
        {
            // Arrange
            Setup();
            bool eventTriggered = false;
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDetailedBreakdown, false)
                .Add(p => p.OnViewDetailedBreakdown, () => eventTriggered = true));
            
            // Initially not triggered
            Assert.False(eventTriggered);
            
            // Toggle breakdown
            component.Find(".country-breakdown .section-header").Click();
            
            // Assert - Event should be triggered
            Assert.True(eventTriggered);
        }

        /// <summary>
        /// Tests that the component respects initial visibility settings for sections
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldRespectInitialVisibilitySettings()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, testCalculation)
                .Add(p => p.ShowDetailedBreakdown, true)
                .Add(p => p.ShowServiceDetails, true)
                .Add(p => p.ShowDiscounts, true));
            
            // Assert - All sections should be visible initially
            Assert.True(component.Instance.ShowDetailedBreakdown);
            Assert.Contains("table-responsive", component.Markup);
            
            Assert.True(component.Instance.ShowServiceDetails);
            Assert.Contains("Service Type:", component.Markup);
            
            Assert.True(component.Instance.ShowDiscounts);
            Assert.Contains("Applied Discounts", component.Markup);
        }

        /// <summary>
        /// Tests that the component handles a null calculation gracefully
        /// </summary>
        [Fact]
        public void PricingBreakdown_ShouldHandleNullCalculation_Gracefully()
        {
            // Arrange
            Setup();
            
            // Act
            var component = context.RenderComponent<PricingBreakdown>(parameters => parameters
                .Add(p => p.Calculation, null)
                .Add(p => p.ShowDetailedBreakdown, true)
                .Add(p => p.ShowServiceDetails, true)
                .Add(p => p.ShowDiscounts, true));
            
            // Assert - Should render without errors and show message
            Assert.Contains("No calculation results available", component.Markup);
            Assert.DoesNotContain("Total Estimated Cost", component.Markup);
        }
    }
}