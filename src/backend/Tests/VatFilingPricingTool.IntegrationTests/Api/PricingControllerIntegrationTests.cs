using FluentAssertions; // FluentAssertions, Version=6.2.0
using System.Collections.Generic; // System.Collections.Generic, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using VatFilingPricingTool.Common.Constants; // Constants for API route paths
using VatFilingPricingTool.Api.Models.Requests; // API request model for calculation
using VatFilingPricingTool.Api.Models.Responses; // API response model for calculation results
using VatFilingPricingTool.Domain.Enums; // Enum for VAT filing service types
using VatFilingPricingTool.IntegrationTests.TestServer; // Base class for integration tests

using Xunit; // Testing framework for assertions and test lifecycle

namespace VatFilingPricingTool.IntegrationTests.Api
{
    /// <summary>
    /// Integration tests for the PricingController API endpoints
    /// </summary>
    public class PricingControllerIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Initializes a new instance of the PricingControllerIntegrationTests class
        /// </summary>
        public PricingControllerIntegrationTests()
        {
            // LD1: Call base constructor to set up test environment
        }

        /// <summary>
        /// Tests that the calculate pricing endpoint returns a successful response with valid input
        /// </summary>
        [Fact]
        public async Task CalculatePricing_WithValidRequest_ReturnsSuccessfulResponse()
        {
            // LD1: Create a valid calculation request with service type, transaction volume, frequency, and countries
            var request = CreateValidCalculationRequest();

            // LD1: Send POST request to the calculate pricing endpoint
            var response = await PostAsync<CalculationRequest, CalculationResponse>(ApiRoutes.Pricing.Calculate, request);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data is not null
            response.Data.Should().NotBeNull();

            // LD1: Assert that the total cost is greater than zero
            response.Data.TotalCost.Should().BeGreaterThan(0);

            // LD1: Assert that the country breakdowns match the requested countries
            response.Data.CountryBreakdowns.Count.Should().Be(request.CountryCodes.Count);
        }

        /// <summary>
        /// Tests that the calculate pricing endpoint returns a bad request response with invalid input
        /// </summary>
        [Fact]
        public async Task CalculatePricing_WithInvalidRequest_ReturnsBadRequest()
        {
            // LD1: Create an invalid calculation request with missing required fields
            var request = new CalculationRequest
            {
                TransactionVolume = 500,
                Frequency = FilingFrequency.Monthly,
            };

            // LD1: Send POST request to the calculate pricing endpoint
            var response = await PostAsync<CalculationRequest, CalculationResponse>(ApiRoutes.Pricing.Calculate, request);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains validation errors
            response.Metadata.ContainsKey("ValidationErrors").Should().BeTrue();
        }

        /// <summary>
        /// Tests that the save calculation endpoint returns a successful response with valid input
        /// </summary>
        [Fact]
        public async Task SaveCalculation_WithValidRequest_ReturnsSuccessfulResponse()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create a valid save calculation request with all required fields
            var request = CreateValidSaveCalculationRequest();

            // LD1: Send POST request to the save calculation endpoint
            var response = await PostAsync<SaveCalculationRequest, SaveCalculationResponse>(ApiRoutes.Pricing.Save, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data is not null
            response.Data.Should().NotBeNull();

            // LD1: Assert that the calculation ID is not empty
            response.Data.CalculationId.Should().NotBeEmpty();
        }

        /// <summary>
        /// Tests that the get calculation endpoint returns the correct calculation for a valid ID
        /// </summary>
        [Fact]
        public async Task GetCalculation_WithValidId_ReturnsCalculation()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create and save a calculation to get a valid ID
            var saveRequest = CreateValidSaveCalculationRequest();
            var saveResponse = await PostAsync<SaveCalculationRequest, SaveCalculationResponse>(ApiRoutes.Pricing.Save, saveRequest, client);
            var calculationId = saveResponse.Data.CalculationId;

            // LD1: Create a get calculation request with the saved ID
            var getRequest = new GetCalculationRequest { CalculationId = calculationId };

            // LD1: Send GET request to the get calculation endpoint
            var response = await GetAsync<CalculationResponse>($"{ApiRoutes.Pricing.Base}/{calculationId}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data is not null
            response.Data.Should().NotBeNull();

            // LD1: Assert that the returned calculation ID matches the requested ID
            response.Data.CalculationId.Should().Be(calculationId);
        }

        /// <summary>
        /// Tests that the get calculation endpoint returns not found for an invalid ID
        /// </summary>
        [Fact]
        public async Task GetCalculation_WithInvalidId_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create a get calculation request with an invalid ID
            var invalidId = "invalid-id";
            var getRequest = new GetCalculationRequest { CalculationId = invalidId };

            // LD1: Send GET request to the get calculation endpoint
            var response = await GetAsync<CalculationResponse>($"{ApiRoutes.Pricing.Base}/{invalidId}", client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response indicates not found
            response.StatusCode.Should().Be(400);
        }

        /// <summary>
        /// Tests that the get calculation history endpoint returns the calculation history
        /// </summary>
        [Fact]
        public async Task GetCalculationHistory_ReturnsHistory()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create and save multiple calculations to populate history
            await PostAsync<SaveCalculationRequest, SaveCalculationResponse>(ApiRoutes.Pricing.Save, CreateValidSaveCalculationRequest(), client);
            await PostAsync<SaveCalculationRequest, SaveCalculationResponse>(ApiRoutes.Pricing.Save, CreateValidSaveCalculationRequest(), client);

            // LD1: Create a get calculation history request with pagination parameters
            var getHistoryRequest = new GetCalculationHistoryRequest { Page = 1, PageSize = 10 };

            // LD1: Send GET request to the calculation history endpoint
            var response = await GetAsync<CalculationHistoryResponse>($"{ApiRoutes.Pricing.History}?page={getHistoryRequest.Page}&pageSize={getHistoryRequest.PageSize}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data is not null
            response.Data.Should().NotBeNull();

            // LD1: Assert that the history items are returned
            response.Data.Items.Should().NotBeEmpty();

            // LD1: Assert that the total count is correct
            response.Data.TotalCount.Should().BeGreaterThanOrEqualTo(2);
        }

        /// <summary>
        /// Tests that the compare calculations endpoint returns a valid comparison for multiple scenarios
        /// </summary>
        [Fact]
        public async Task CompareCalculations_WithValidScenarios_ReturnsComparison()
        {
            // LD1: Create a compare calculations request with multiple scenarios
            var request = CreateValidCompareCalculationsRequest();

            // LD1: Send POST request to the compare calculations endpoint
            var response = await PostAsync<CompareCalculationsRequest, CompareCalculationsResponse>(ApiRoutes.Pricing.Compare, request);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data is not null
            response.Data.Should().NotBeNull();

            // LD1: Assert that the scenarios are included in the response
            response.Data.Scenarios.Should().NotBeEmpty();

            // LD1: Assert that the total cost comparison is provided
            response.Data.TotalCostComparison.Should().NotBeEmpty();
        }

        /// <summary>
        /// Tests that the delete calculation endpoint successfully deletes a calculation
        /// </summary>
        [Fact]
        public async Task DeleteCalculation_WithValidId_ReturnsSuccess()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create and save a calculation to get a valid ID
            var saveRequest = CreateValidSaveCalculationRequest();
            var saveResponse = await PostAsync<SaveCalculationRequest, SaveCalculationResponse>(ApiRoutes.Pricing.Save, saveRequest, client);
            var calculationId = saveResponse.Data.CalculationId;

            // LD1: Create a delete calculation request with the saved ID
            var deleteRequest = new DeleteCalculationRequest { CalculationId = calculationId };

            // LD1: Send DELETE request to the delete calculation endpoint
            var response = await DeleteAsync<DeleteCalculationResponse>($"{ApiRoutes.Pricing.Base}/{calculationId}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data indicates successful deletion
            response.Data.Deleted.Should().BeTrue();

            // LD1: Verify the calculation is no longer retrievable
            var getResponse = await GetAsync<CalculationResponse>($"{ApiRoutes.Pricing.Base}/{calculationId}", client);
            getResponse.Success.Should().BeFalse();
        }

        /// <summary>
        /// Helper method to create a valid calculation request for testing
        /// </summary>
        /// <returns>A valid calculation request</returns>
        private CalculationRequest CreateValidCalculationRequest()
        {
            // LD1: Create a new CalculationRequest
            var request = new CalculationRequest();

            // LD1: Set ServiceType to StandardFiling
            request.ServiceType = ServiceType.StandardFiling;

            // LD1: Set TransactionVolume to 500
            request.TransactionVolume = 500;

            // LD1: Set Frequency to Monthly
            request.Frequency = FilingFrequency.Monthly;

            // LD1: Add country codes for UK, Germany, and France
            request.CountryCodes = new List<string> { "GB", "DE", "FR" };

            // LD1: Return the configured request
            return request;
        }

        /// <summary>
        /// Helper method to create a valid save calculation request for testing
        /// </summary>
        /// <returns>A valid save calculation request</returns>
        private SaveCalculationRequest CreateValidSaveCalculationRequest()
        {
            // LD1: Create a new SaveCalculationRequest
            var request = new SaveCalculationRequest();

            // LD1: Set ServiceType to StandardFiling
            request.ServiceType = ServiceType.StandardFiling;

            // LD1: Set TransactionVolume to 500
            request.TransactionVolume = 500;

            // LD1: Set Frequency to Monthly
            request.Frequency = FilingFrequency.Monthly;

            // LD1: Set TotalCost to 1000
            request.TotalCost = 1000;

            // LD1: Set CurrencyCode to EUR
            request.CurrencyCode = "EUR";

            // LD1: Add country breakdowns for UK, Germany, and France
            request.CountryBreakdowns = new List<CountryBreakdownRequest>
            {
                new CountryBreakdownRequest { CountryCode = "GB", CountryName = "United Kingdom", BaseCost = 300, AdditionalCost = 100, TotalCost = 400 },
                new CountryBreakdownRequest { CountryCode = "DE", CountryName = "Germany", BaseCost = 250, AdditionalCost = 50, TotalCost = 300 },
                new CountryBreakdownRequest { CountryCode = "FR", CountryName = "France", BaseCost = 250, AdditionalCost = 50, TotalCost = 300 }
            };

            // LD1: Set AdditionalServices
            request.AdditionalServices = new List<string> { "Tax Consultancy" };

            // LD1: Return the configured request
            return request;
        }

        /// <summary>
        /// Helper method to create a valid compare calculations request for testing
        /// </summary>
        /// <returns>A valid compare calculations request</returns>
        private CompareCalculationsRequest CreateValidCompareCalculationsRequest()
        {
            // LD1: Create a new CompareCalculationsRequest
            var request = new CompareCalculationsRequest();

            // LD1: Add a scenario for standard filing with monthly frequency
            request.Scenarios = new List<CalculationScenario>
            {
                new CalculationScenario
                {
                    ScenarioName = "Standard Filing Monthly",
                    ServiceType = ServiceType.StandardFiling,
                    TransactionVolume = 500,
                    Frequency = FilingFrequency.Monthly,
                    CountryCodes = new List<string> { "GB", "DE", "FR" }
                },
                new CalculationScenario
                {
                    ScenarioName = "Complex Filing Quarterly",
                    ServiceType = ServiceType.ComplexFiling,
                    TransactionVolume = 500,
                    Frequency = FilingFrequency.Quarterly,
                    CountryCodes = new List<string> { "GB", "DE", "FR" }
                }
            };

            // LD1: Return the configured request
            return request;
        }
    }
}