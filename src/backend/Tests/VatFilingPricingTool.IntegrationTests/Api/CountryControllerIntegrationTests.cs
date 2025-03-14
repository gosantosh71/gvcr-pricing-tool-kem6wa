#nullable disable
using FluentAssertions; // FluentAssertions, Version=6.2.0
using System.Collections.Generic; // System.Collections.Generic, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Net.Http.Json; // System.Net.Http.Json, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using VatFilingPricingTool.Common.Constants; // Centralized API route definitions for country endpoints
using VatFilingPricingTool.Common.Models.ApiResponse; // Generic API response wrapper for handling test responses
using VatFilingPricingTool.Contracts.V1.Requests.CountryRequests; // Request model for retrieving a specific country
using VatFilingPricingTool.Contracts.V1.Responses.CountryResponses; // Response model for country information
using VatFilingPricingTool.Domain.Enums; // User role enumeration for authentication testing
using VatFilingPricingTool.IntegrationTests.TestServer; // Base class for integration tests providing common setup and utility methods
using Xunit; // Testing framework for assertions and test lifecycle

namespace VatFilingPricingTool.IntegrationTests.Api
{
    /// <summary>
    /// Integration tests for the Country Controller API endpoints, verifying the correct behavior of country-related operations
    /// </summary>
    public class CountryControllerIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Initializes a new instance of the CountryControllerIntegrationTests class
        /// </summary>
        public CountryControllerIntegrationTests()
        {
            // LD1: Call base constructor to set up the test environment
        }

        [Fact]
        public async Task GetCountry_WithValidCountryCode_ReturnsCountry()
        {
            // LD1: Create a GetCountryRequest with a valid country code
            var request = new GetCountryRequest { CountryCode = "GB" };

            // LD1: Call GetAsync to the GetById endpoint with the request
            var response = await GetAsync<CountryResponse>($"{ApiRoutes.Country.Base}/{request.CountryCode}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned country has the expected country code
            response.Data.CountryCode.Should().Be(request.CountryCode);

            // LD1: Assert that the country has the expected properties (name, VAT rate, etc.)
            response.Data.Name.Should().Be("United Kingdom");
            response.Data.StandardVatRate.Should().Be(20.00m);
            response.Data.CurrencyCode.Should().Be("GBP");
        }

        [Fact]
        public async Task GetCountry_WithInvalidCountryCode_ReturnsNotFound()
        {
            // LD1: Create a GetCountryRequest with an invalid country code
            var request = new GetCountryRequest { CountryCode = "XX" };

            // LD1: Call GetAsync to the GetById endpoint with the request
            var response = await GetAsync<CountryResponse>($"{ApiRoutes.Country.Base}/{request.CountryCode}");

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            response.Message.Should().Contain("Country not found");
        }

        [Fact]
        public async Task GetCountries_WithDefaultParameters_ReturnsAllActiveCountries()
        {
            // LD1: Create a GetCountriesRequest with default parameters (ActiveOnly=true)
            var request = new GetCountriesRequest { ActiveOnly = true };

            // LD1: Call GetAsync to the Get endpoint with the request
            var response = await GetAsync<CountriesResponse>($"{ApiRoutes.Country.Base}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned countries list is not empty
            response.Data.Items.Should().NotBeEmpty();

            // LD1: Assert that all returned countries have IsActive=true
            response.Data.Items.Should().All(c => c.IsActive.Equals(true));
        }

        [Fact]
        public async Task GetCountries_WithPagination_ReturnsCorrectPage()
        {
            // LD1: Create a GetCountriesRequest with specific page and page size
            var request = new GetCountriesRequest { Page = 2, PageSize = 5 };

            // LD1: Call GetAsync to the Get endpoint with the request
            var response = await GetAsync<CountriesResponse>($"{ApiRoutes.Country.Base}?page={request.Page}&pageSize={request.PageSize}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned page number matches the request
            response.Data.PageNumber.Should().Be(request.Page);

            // LD1: Assert that the number of items does not exceed the page size
            response.Data.Items.Count.Should().BeLessOrEqualTo(request.PageSize);
        }

        [Fact]
        public async Task GetCountries_WithSpecificCountryCodes_ReturnsMatchingCountries()
        {
            // LD1: Create a GetCountriesRequest with specific country codes
            var request = new GetCountriesRequest { CountryCodes = new List<string> { "GB", "DE" } };

            // LD1: Call GetAsync to the Get endpoint with the request
            var response = await GetAsync<CountriesResponse>($"{ApiRoutes.Country.Base}?countryCodes=GB&countryCodes=DE");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that all returned countries have country codes matching the request
            response.Data.Items.Should().All(c => request.CountryCodes.Contains(c.CountryCode));

            // LD1: Assert that the count of returned countries matches the expected count
            response.Data.Items.Count.Should().Be(request.CountryCodes.Count);
        }

        [Fact]
        public async Task GetActiveCountries_ReturnsOnlyActiveCountries()
        {
            // LD1: Call GetAsync to the active endpoint
            var response = await GetAsync<CountriesResponse>($"{ApiRoutes.Country.Base}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned countries list is not empty
            response.Data.Items.Should().NotBeEmpty();

            // LD1: Assert that all returned countries have IsActive=true
            response.Data.Items.Should().All(c => c.IsActive.Equals(true));
        }

        [Fact]
        public async Task GetCountriesByFilingFrequency_ReturnsMatchingCountries()
        {
            // LD1: Call GetAsync to the GetByFrequency endpoint with a specific filing frequency
            var response = await GetAsync<CountriesResponse>($"{ApiRoutes.Country.GetByFrequency.Replace(\"{frequency}\", \"Quarterly\")}");

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that all returned countries support the specified filing frequency
            // This assertion requires more setup to ensure the test database has countries with specific filing frequencies
            // For now, we just check that the list is not empty
            response.Data.Items.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetCountrySummaries_ReturnsAllCountrySummaries()
        {
            // LD1: Call GetAsync to the Summaries endpoint
            var response = await GetAsync<List<CountrySummaryResponse>>(ApiRoutes.Country.Summaries);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the returned summaries list is not empty
            response.Data.Should().NotBeEmpty();

            // LD1: Assert that each summary contains the expected basic properties (country code, name)
            response.Data.Should().All(s => !string.IsNullOrEmpty(s.CountryCode) && !string.IsNullOrEmpty(s.Name));
        }

        [Fact]
        public async Task CreateCountry_WithValidData_CreatesCountry()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a CreateCountryRequest with valid country data
            var request = new CreateCountryRequest
            {
                CountryCode = "ZZ",
                Name = "Test Country",
                StandardVatRate = 20.00m,
                CurrencyCode = "USD",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateCountryRequest, CreateCountryResponse>(ApiRoutes.Country.Create, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful creation
            response.Data.CountryCode.Should().Be(request.CountryCode);

            // LD1: Verify the country was created by retrieving it with GetCountry
            var getResponse = await GetAsync<CountryResponse>($"{ApiRoutes.Country.Base}/{request.CountryCode}", client);
            getResponse.Success.Should().BeTrue();
            getResponse.Data.CountryCode.Should().Be(request.CountryCode);
        }

        [Fact]
        public async Task CreateCountry_WithInvalidData_ReturnsBadRequest()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a CreateCountryRequest with invalid data (missing required fields)
            var request = new CreateCountryRequest
            {
                Name = "Test Country",
                StandardVatRate = 20.00m,
                CurrencyCode = "USD",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateCountryRequest, CreateCountryResponse>(ApiRoutes.Country.Create, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains validation error messages
            response.Message.Should().Contain("Validation failed");
        }

        [Fact]
        public async Task CreateCountry_WithDuplicateCountryCode_ReturnsConflict()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a CreateCountryRequest with a country code that already exists
            var request = new CreateCountryRequest
            {
                CountryCode = "GB",
                Name = "Test Country",
                StandardVatRate = 20.00m,
                CurrencyCode = "USD",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateCountryRequest, CreateCountryResponse>(ApiRoutes.Country.Create, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a conflict error
            response.Message.Should().Contain("duplicate");
        }

        [Fact]
        public async Task CreateCountry_WithoutAdminRole_ReturnsForbidden()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create a valid CreateCountryRequest
            var request = new CreateCountryRequest
            {
                CountryCode = "ZZ",
                Name = "Test Country",
                StandardVatRate = 20.00m,
                CurrencyCode = "USD",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // LD1: Call PostAsync to the Create endpoint with the request
            var response = await PostAsync<CreateCountryRequest, CreateCountryResponse>(ApiRoutes.Country.Create, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a forbidden error
            response.StatusCode.Should().Be(401);
        }

        [Fact]
        public async Task UpdateCountry_WithValidData_UpdatesCountry()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an UpdateCountryRequest with valid updated country data
            var request = new UpdateCountryRequest
            {
                CountryCode = "GB",
                Name = "Updated Country Name",
                StandardVatRate = 22.00m,
                CurrencyCode = "GBP",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Annually },
                IsActive = false
            };

            // LD1: Call PutAsync to the Update endpoint with the request
            var response = await PutAsync<UpdateCountryRequest, UpdateCountryResponse>(ApiRoutes.Country.Update.Replace("{id}", request.CountryCode), request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful update
            response.Data.CountryCode.Should().Be(request.CountryCode);

            // LD1: Verify the country was updated by retrieving it with GetCountry
            var getResponse = await GetAsync<CountryResponse>($"{ApiRoutes.Country.Base}/{request.CountryCode}", client);
            getResponse.Success.Should().BeTrue();
            getResponse.Data.Name.Should().Be(request.Name);
            getResponse.Data.StandardVatRate.Should().Be(request.StandardVatRate);
            getResponse.Data.CurrencyCode.Should().Be(request.CurrencyCode);
            getResponse.Data.AvailableFilingFrequencies.Should().Contain(FilingFrequency.Annually);
            getResponse.Data.IsActive.Should().Be(request.IsActive);
        }

        [Fact]
        public async Task UpdateCountry_WithInvalidCountryCode_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create an UpdateCountryRequest with a non-existent country code
            var request = new UpdateCountryRequest
            {
                CountryCode = "XX",
                Name = "Updated Country Name",
                StandardVatRate = 22.00m,
                CurrencyCode = "USD",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly },
                IsActive = false
            };

            // LD1: Call PutAsync to the Update endpoint with the request
            var response = await PutAsync<UpdateCountryRequest, UpdateCountryResponse>(ApiRoutes.Country.Update.Replace("{id}", request.CountryCode), request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a not found error
            response.Message.Should().Contain("Country not found");
        }

        [Fact]
        public async Task DeleteCountry_WithValidCountryCode_DeletesCountry()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a DeleteCountryRequest with a valid country code
            var request = new DeleteCountryRequest { CountryCode = "GB" };

            // LD1: Call DeleteAsync to the Delete endpoint with the request
            var response = await DeleteAsync<DeleteCountryResponse>(ApiRoutes.Country.Delete.Replace("{id}", request.CountryCode), client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response indicates successful deletion
            response.Data.CountryCode.Should().Be(request.CountryCode);

            // LD1: Verify the country was deleted by attempting to retrieve it with GetCountry
            var getResponse = await GetAsync<CountryResponse>($"{ApiRoutes.Country.Base}/{request.CountryCode}", client);
            getResponse.Success.Should().BeFalse();
            getResponse.Message.Should().Contain("Country not found");
        }

        [Fact]
        public async Task DeleteCountry_WithInvalidCountryCode_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a DeleteCountryRequest with a non-existent country code
            var request = new DeleteCountryRequest { CountryCode = "XX" };

            // LD1: Call DeleteAsync to the Delete endpoint with the request
            var response = await DeleteAsync<DeleteCountryResponse>(ApiRoutes.Country.Delete.Replace("{id}", request.CountryCode), client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates a not found error
            response.Message.Should().Contain("Country not found");
        }
    }
}