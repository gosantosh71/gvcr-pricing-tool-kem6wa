using Bunit; // bunit version 1.12.6
using Moq; // Moq version 4.18.2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VatFilingPricingTool.Web.Components;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Tests.Helpers;
using Xunit; // xunit version 2.4.2
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging version 6.0.0

namespace VatFilingPricingTool.Web.Tests.Components
{
    /// <summary>
    /// Test class for the CountrySelector component
    /// </summary>
    public class CountrySelectorTests : IDisposable
    {
        private TestContext context;
        private Mock<ICountryService> mockCountryService;

        /// <summary>
        /// Default constructor for the CountrySelectorTests class
        /// </summary>
        public CountrySelectorTests()
        {
            // Initialize properties to null, they will be set up in the Setup method
            context = null;
            mockCountryService = null;
        }

        /// <summary>
        /// Sets up the test context and mocks for each test
        /// </summary>
        private void Setup()
        {
            // Create test context
            context = RenderComponent.CreateTestContext();
            
            // Create mock country service
            mockCountryService = RenderComponent.CreateMockService<ICountryService>(context);
            
            // Set up mock to return test data
            var countrySelectionModel = new CountrySelectionModel
            {
                AvailableCountries = TestData.CreateTestCountrySummaries().Select(c => new CountryOption
                {
                    Value = c.CountryCode,
                    Text = c.Name,
                    FlagCode = c.CountryCode.ToLower(),
                    IsSelected = false
                }).ToList(),
                SelectedCountryCodes = new List<string>()
            };

            mockCountryService.Setup(s => s.InitializeCountrySelectionAsync())
                .ReturnsAsync(countrySelectionModel);
                
            mockCountryService.Setup(s => s.SearchCountriesAsync(It.IsAny<string>()))
                .ReturnsAsync((string searchTerm) => 
                {
                    return countrySelectionModel.AvailableCountries
                        .Where(c => c.Text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                });
        }

        /// <summary>
        /// Cleans up resources after each test
        /// </summary>
        public void Dispose()
        {
            context?.Dispose();
            context = null;
            mockCountryService = null;
        }

        /// <summary>
        /// Tests that the CountrySelector component renders without errors
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldRender_WithoutErrors()
        {
            // Arrange
            Setup();

            // Act
            var cut = context.RenderComponent<CountrySelector>();

            // Assert
            Assert.NotNull(cut);
            Assert.NotNull(cut.Find("button.dropdown-toggle"));
            Assert.Contains("Select Countries", cut.Find("button.dropdown-toggle").TextContent);
        }

        /// <summary>
        /// Tests that the CountrySelector displays countries when the dropdown is opened
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldDisplayCountries_WhenDropdownOpened()
        {
            // Arrange
            Setup();

            // Act
            var cut = context.RenderComponent<CountrySelector>();
            cut.Find("button.dropdown-toggle").Click();

            // Assert
            Assert.Contains("show", cut.Find(".dropdown").ClassList);
            
            // Check that all test countries are displayed
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            Assert.Equal(5, countryItems.Count); // 5 test countries
            
            // Check specific countries
            Assert.Contains("United Kingdom", cut.Markup);
            Assert.Contains("Germany", cut.Markup);
            Assert.Contains("France", cut.Markup);
        }

        /// <summary>
        /// Tests that clicking a country toggles its selection state
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldToggleCountrySelection_WhenCountryClicked()
        {
            // Arrange
            Setup();

            // Act
            var cut = context.RenderComponent<CountrySelector>();
            cut.Find("button.dropdown-toggle").Click();
            
            // Select the first country (United Kingdom)
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            countryItems[0].Click();
            
            // Assert that country is selected
            Assert.Contains("United Kingdom", cut.Find("button.dropdown-toggle").TextContent);
            
            // Deselect the country
            cut.Find("button.dropdown-toggle").Click();
            countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            countryItems[0].Click();
            
            // Assert that country is deselected
            Assert.Contains("Select Countries", cut.Find("button.dropdown-toggle").TextContent);
        }

        /// <summary>
        /// Tests that entering a search term filters the displayed countries
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldFilterCountries_WhenSearchTermEntered()
        {
            // Arrange
            Setup();
            
            // Set up mock to return only UK for "United" search
            mockCountryService.Setup(s => s.SearchCountriesAsync("United"))
                .ReturnsAsync(new List<CountryOption> 
                { 
                    new CountryOption 
                    { 
                        Value = "GB", 
                        Text = "United Kingdom", 
                        FlagCode = "gb" 
                    } 
                });

            // Act
            var cut = context.RenderComponent<CountrySelector>();
            cut.Find("button.dropdown-toggle").Click();
            
            // Enter search term
            var searchInput = cut.Find("input.form-control");
            searchInput.Change("United");
            
            // Assert
            mockCountryService.Verify(s => s.SearchCountriesAsync("United"), Times.Once);
            
            // Only matching countries should be displayed
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            Assert.Single(countryItems);
            Assert.Contains("United Kingdom", countryItems[0].TextContent);
        }

        /// <summary>
        /// Tests that the OnSelectionChanged event is triggered when selection changes
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldTriggerSelectionChangedEvent_WhenSelectionChanges()
        {
            // Arrange
            Setup();
            var selectionChangedFired = false;
            var selectedCountries = new List<string>();
            
            // Act
            var cut = context.RenderComponent<CountrySelector>(parameters => parameters
                .Add(p => p.OnSelectionChanged, (List<string> countries) => 
                {
                    selectionChangedFired = true;
                    selectedCountries = countries;
                })
            );
            
            cut.Find("button.dropdown-toggle").Click();
            
            // Select the first country (United Kingdom)
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            countryItems[0].Click();
            
            // Assert
            Assert.True(selectionChangedFired);
            Assert.Single(selectedCountries);
            Assert.Equal("GB", selectedCountries[0]);
        }

        /// <summary>
        /// Tests that the component respects the disabled state
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldRespectDisabledState_WhenDisabled()
        {
            // Arrange
            Setup();
            
            // Act
            var cut = context.RenderComponent<CountrySelector>(parameters => parameters
                .Add(p => p.Disabled, true)
            );
            
            // Assert
            Assert.Contains("disabled", cut.Find("button.dropdown-toggle").Attributes.ToDictionary().Keys);
            
            // Try to click the dropdown button (shouldn't open the dropdown)
            cut.Find("button.dropdown-toggle").Click();
            
            // Verify dropdown didn't open
            Assert.DoesNotContain("show", cut.Find(".dropdown").ClassList);
        }

        /// <summary>
        /// Tests that the component displays pre-selected countries when provided
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldDisplaySelectedCountries_WhenProvidedInitially()
        {
            // Arrange
            Setup();
            var preSelectedCountries = new List<string> { "GB", "DE" };
            
            // Act
            var cut = context.RenderComponent<CountrySelector>(parameters => parameters
                .Add(p => p.SelectedCountries, preSelectedCountries)
            );
            
            // Assert
            Assert.Contains("2 Countries Selected", cut.Find("button.dropdown-toggle").TextContent);
            
            // Open dropdown to check selected countries
            cut.Find("button.dropdown-toggle").Click();
            
            // Verify countries are marked as selected
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            
            // The countries GB and DE should have the 'active' class
            var ukCountry = countryItems.FirstOrDefault(c => c.TextContent.Contains("United Kingdom"));
            var deCountry = countryItems.FirstOrDefault(c => c.TextContent.Contains("Germany"));
            
            Assert.NotNull(ukCountry);
            Assert.NotNull(deCountry);
            Assert.Contains("active", ukCountry.ClassList);
            Assert.Contains("active", deCountry.ClassList);
        }

        /// <summary>
        /// Tests that clicking the clear button clears all selected countries
        /// </summary>
        [Fact]
        public void CountrySelector_ShouldClearSelection_WhenClearButtonClicked()
        {
            // Arrange
            Setup();
            
            // Act
            var cut = context.RenderComponent<CountrySelector>();
            cut.Find("button.dropdown-toggle").Click();
            
            // Select multiple countries
            var countryItems = cut.FindAll(".dropdown-item")
                .Where(i => !i.HasAttribute("disabled"))
                .ToList();
            countryItems[0].Click(); // GB
            countryItems[1].Click(); // DE
            
            // Assert that countries are selected
            Assert.Contains("2 Countries Selected", cut.Find("button.dropdown-toggle").TextContent);
            
            // Open dropdown again to access the clear button
            cut.Find("button.dropdown-toggle").Click();
            
            // Click the "Clear All" button 
            var clearButton = cut.FindAll("button").FirstOrDefault(b => b.TextContent.Contains("Clear All"));
            Assert.NotNull(clearButton);
            clearButton.Click();
            
            // Assert that no countries are selected
            Assert.Contains("Select Countries", cut.Find("button.dropdown-toggle").TextContent);
        }
    }
}