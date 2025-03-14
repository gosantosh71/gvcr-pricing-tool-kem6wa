using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Services.Implementations;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Tests.Helpers;
using VatFilingPricingTool.Web.Tests.Mock;

namespace VatFilingPricingTool.Web.Tests.Services
{
    /// <summary>
    /// Contains unit tests for the CountryService class
    /// </summary>
    public class CountryServiceTests
    {
        private readonly Mock<ILogger<CountryService>> mockLogger;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;
        private readonly Mock<HttpClientFactory> mockHttpClientFactory;
        private readonly Mock<LocalStorageHelper> mockLocalStorage;
        private readonly ApiClient apiClient;
        private readonly ICountryService countryService;

        /// <summary>
        /// Initializes a new instance of the CountryServiceTests class with test dependencies
        /// </summary>
        public CountryServiceTests()
        {
            // Initialize mocks
            mockLogger = new Mock<ILogger<CountryService>>();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler) { BaseAddress = new Uri("https://api.example.com") };
            
            mockHttpClientFactory = new Mock<HttpClientFactory>();
            mockHttpClientFactory.Setup(x => x.CreateClient()).Returns(httpClient);
            mockHttpClientFactory.Setup(x => x.CreateAuthenticatedClient()).Returns(httpClient);
            mockHttpClientFactory.Setup(x => x.CreateResilientAuthenticatedClient()).Returns(httpClient);
            
            mockLocalStorage = new Mock<LocalStorageHelper>();
            
            // Create API client
            apiClient = new ApiClient(mockHttpClientFactory.Object, mockLocalStorage.Object, mockLogger.Object);
            
            // Create the service to test
            countryService = new CountryService(apiClient, mockLogger.Object);
        }

        /// <summary>
        /// Tests that GetCountryAsync returns the correct country when a valid country code is provided
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithValidCountryCode_ReturnsCountry()
        {
            // Arrange: Create a test country with CountryCode 'GB'
            var testCountry = new CountryModel 
            { 
                CountryCode = "GB", 
                Name = "United Kingdom", 
                StandardVatRate = 20.0m,
                CurrencyCode = "GBP",
                IsActive = true
            };
            
            // Arrange: Setup mock HTTP handler to return the test country for the country endpoint
            mockHttpHandler.When(ApiEndpoints.Country.GetById.Replace("{id}", "GB"))
                .RespondWithApiResponse(testCountry);
            
            // Act: Call countryService.GetCountryAsync("GB")
            var result = await countryService.GetCountryAsync("GB");
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result has the correct CountryCode 'GB'
            result.CountryCode.Should().Be("GB");
        }

        /// <summary>
        /// Tests that GetCountryAsync returns null when an invalid country code is provided
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithInvalidCountryCode_ReturnsNull()
        {
            // Arrange: Setup mock HTTP handler to return an error for the country endpoint
            mockHttpHandler.When(ApiEndpoints.Country.GetById.Replace("{id}", "XX"))
                .RespondWithApiError("Country not found");
            
            // Act: Call countryService.GetCountryAsync("XX")
            var result = await countryService.GetCountryAsync("XX");
            
            // Assert: Verify the result is null
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests that GetCountriesAsync returns all countries
        /// </summary>
        [Fact]
        public async Task GetCountriesAsync_ReturnsAllCountries()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.GetCountriesAsync(false)
            var result = await countryService.GetCountriesAsync(false);
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains the expected number of countries
            result.Count.Should().Be(testCountries.Count);
            // Assert: Verify the result contains countries with the expected country codes
            result.Select(c => c.CountryCode).Should().Contain(new[] { "GB", "DE", "FR", "ES", "IT" });
        }

        /// <summary>
        /// Tests that GetCountriesAsync with activeOnly=true returns only active countries
        /// </summary>
        [Fact]
        public async Task GetCountriesAsync_WithActiveOnly_ReturnsOnlyActiveCountries()
        {
            // Arrange: Create a list of test countries with some inactive countries
            var testCountries = TestData.CreateTestCountries();
            testCountries[3].IsActive = false; // Make Spain inactive
            testCountries[4].IsActive = false; // Make Italy inactive
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.GetCountriesAsync(true)
            var result = await countryService.GetCountriesAsync(true);
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains only active countries
            result.Count.Should().Be(3); // Only GB, DE, FR are active
            // Assert: Verify all countries in the result have IsActive=true
            result.All(c => c.IsActive).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetActiveCountriesAsync returns only active countries
        /// </summary>
        [Fact]
        public async Task GetActiveCountriesAsync_ReturnsOnlyActiveCountries()
        {
            // Arrange: Create a list of test countries with some inactive countries
            var testCountries = TestData.CreateTestCountries();
            testCountries[3].IsActive = false; // Make Spain inactive
            testCountries[4].IsActive = false; // Make Italy inactive
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.GetActiveCountriesAsync()
            var result = await countryService.GetActiveCountriesAsync();
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains only active countries
            result.Count.Should().Be(3); // Only GB, DE, FR are active
            // Assert: Verify all countries in the result have IsActive=true
            result.All(c => c.IsActive).Should().BeTrue();
        }

        /// <summary>
        /// Tests that GetCountriesByFilingFrequencyAsync returns countries that support a specific filing frequency
        /// </summary>
        [Fact]
        public async Task GetCountriesByFilingFrequencyAsync_ReturnsCountriesWithSpecificFrequency()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the filing frequency endpoint
            mockHttpHandler.When(ApiEndpoints.Country.GetByFrequency.Replace("{frequency}", "2"))
                .RespondWithApiResponse(testCountries.Take(3).ToList()); // First 3 countries support quarterly filing
            
            // Act: Call countryService.GetCountriesByFilingFrequencyAsync(2) // Quarterly
            var result = await countryService.GetCountriesByFilingFrequencyAsync(2); // Quarterly
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains the expected number of countries
            result.Count.Should().Be(3);
            // Assert: Verify the result contains countries with the expected country codes
            result.Select(c => c.CountryCode).Should().Contain(new[] { "GB", "DE", "FR" });
        }

        /// <summary>
        /// Tests that GetCountrySummariesAsync returns country summaries
        /// </summary>
        [Fact]
        public async Task GetCountrySummariesAsync_ReturnsCountrySummaries()
        {
            // Arrange: Create a list of test country summaries
            var testSummaries = TestData.CreateTestCountrySummaries();
            
            // Arrange: Setup mock HTTP handler to return the test country summaries for the summaries endpoint
            mockHttpHandler.When($"{ApiEndpoints.Country.Get}?summaries=true")
                .RespondWithApiResponse(testSummaries);
            
            // Act: Call countryService.GetCountrySummariesAsync()
            var result = await countryService.GetCountrySummariesAsync();
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains the expected number of country summaries
            result.Count.Should().Be(testSummaries.Count);
            // Assert: Verify the result contains country summaries with the expected country codes
            result.Select(c => c.CountryCode).Should().Contain(new[] { "GB", "DE", "FR", "ES", "IT" });
        }

        /// <summary>
        /// Tests that InitializeCountrySelectionAsync returns an initialized country selection model
        /// </summary>
        [Fact]
        public async Task InitializeCountrySelectionAsync_ReturnsInitializedSelectionModel()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.InitializeCountrySelectionAsync()
            var result = await countryService.InitializeCountrySelectionAsync();
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result.AvailableCountries contains the expected number of country options
            result.AvailableCountries.Count.Should().Be(5);
            // Assert: Verify the result.SelectedCountryCodes is empty
            result.SelectedCountryCodes.Should().BeEmpty();
            // Assert: Verify the country options have the correct Value and Text properties
            result.AvailableCountries[0].Value.Should().Be("GB");
            result.AvailableCountries[0].Text.Should().Be("United Kingdom");
            // Assert: Verify all country options have IsSelected=false
            result.AvailableCountries.All(c => c.IsSelected == false).Should().BeTrue();
        }

        /// <summary>
        /// Tests that SearchCountriesAsync with an empty search term returns all active countries
        /// </summary>
        [Fact]
        public async Task SearchCountriesAsync_WithEmptySearchTerm_ReturnsAllActiveCountries()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.SearchCountriesAsync("")
            var result = await countryService.SearchCountriesAsync("");
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains the expected number of country options
            result.Count.Should().Be(5);
            // Assert: Verify the result contains country options with the expected country codes
            result.Select(c => c.Value).Should().Contain(new[] { "GB", "DE", "FR", "ES", "IT" });
        }

        /// <summary>
        /// Tests that SearchCountriesAsync with a search term returns filtered countries
        /// </summary>
        [Fact]
        public async Task SearchCountriesAsync_WithSearchTerm_ReturnsFilteredCountries()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Act: Call countryService.SearchCountriesAsync("United")
            var result = await countryService.SearchCountriesAsync("United");
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains only countries with names containing 'United'
            result.Count.Should().Be(1);
            // Assert: Verify the result contains the expected country option with Value='GB'
            result.Select(c => c.Value).Should().Contain("GB");
        }

        /// <summary>
        /// Tests that GetSelectedCountriesAsync returns the selected countries
        /// </summary>
        [Fact]
        public async Task GetSelectedCountriesAsync_WithValidCountryCodes_ReturnsSelectedCountries()
        {
            // Arrange: Create a list of test countries
            var testCountries = TestData.CreateTestCountries();
            
            // Arrange: Setup mock HTTP handler to return the test countries for the countries endpoint
            mockHttpHandler.When(ApiEndpoints.Country.Get)
                .RespondWithApiResponse(testCountries);
            
            // Arrange: Create a list of selected country codes ['GB', 'DE']
            var selectedCountryCodes = new List<string> { "GB", "DE" };
            
            // Act: Call countryService.GetSelectedCountriesAsync(selectedCountryCodes)
            var result = await countryService.GetSelectedCountriesAsync(selectedCountryCodes);
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result contains exactly 2 countries
            result.Count.Should().Be(2);
            // Assert: Verify the result contains countries with the expected country codes 'GB' and 'DE'
            result.Select(c => c.CountryCode).Should().Contain(new[] { "GB", "DE" });
        }

        /// <summary>
        /// Tests that GetSelectedCountriesAsync with empty country codes returns an empty list
        /// </summary>
        [Fact]
        public async Task GetSelectedCountriesAsync_WithEmptyCountryCodes_ReturnsEmptyList()
        {
            // Arrange: Create an empty list of country codes
            var emptyCountryCodes = new List<string>();
            
            // Act: Call countryService.GetSelectedCountriesAsync(emptyCountryCodes)
            var result = await countryService.GetSelectedCountriesAsync(emptyCountryCodes);
            
            // Assert: Verify the result is not null
            result.Should().NotBeNull();
            // Assert: Verify the result is an empty list
            result.Should().BeEmpty();
        }
    }
}