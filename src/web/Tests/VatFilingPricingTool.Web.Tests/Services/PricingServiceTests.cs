using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Implementations;
using VatFilingPricingTool.Web.Tests.Helpers;
using VatFilingPricingTool.Web.Tests.Mock;
using Xunit;

namespace VatFilingPricingTool.Web.Tests.Services
{
    public class PricingServiceTests
    {
        private readonly Mock<ILogger<PricingService>> mockLogger;
        private readonly Mock<LocalStorageHelper> mockLocalStorage;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClientFactory httpClientFactory;
        private readonly ApiClient apiClient;
        private readonly PricingService pricingService;

        public PricingServiceTests()
        {
            // Initialize mocks
            mockLogger = new Mock<ILogger<PricingService>>();
            mockLocalStorage = new Mock<LocalStorageHelper>();
            mockHttpHandler = new MockHttpMessageHandler();
            
            // Setup mock localStorage to return a test auth token
            mockLocalStorage
                .Setup(ls => ls.GetAuthTokenAsync())
                .ReturnsAsync("test-auth-token");
            
            // Create an HttpClient with the mock handler
            var httpClient = new HttpClient(mockHttpHandler);
            
            // Mock HttpClientFactory to return our configured HttpClient
            httpClientFactory = Mock.Of<HttpClientFactory>(f => 
                f.CreateClient() == httpClient && 
                f.CreateResilientAuthenticatedClient() == httpClient);
            
            // Initialize API client and service
            apiClient = new ApiClient(httpClientFactory, mockLocalStorage.Object, Mock.Of<ILogger<ApiClient>>());
            pricingService = new PricingService(apiClient, mockLogger.Object);
        }

        [Fact]
        public async Task CalculatePricingAsync_WithValidInput_ReturnsCalculationResult()
        {
            // Arrange
            var input = TestData.CreateTestCalculationInput();
            var expectedResult = TestData.CreateTestCalculationResult("calc-123", "user-123");
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Calculate)
                .RespondWithApiResponse(expectedResult);
            
            // Act
            var result = await pricingService.CalculatePricingAsync(input);
            
            // Assert
            result.Should().NotBeNull();
            result.CalculationId.Should().Be(expectedResult.CalculationId);
            result.TotalCost.Should().Be(expectedResult.TotalCost);
            result.CountryBreakdowns.Count.Should().Be(expectedResult.CountryBreakdowns.Count);
        }
        
        [Fact]
        public async Task CalculatePricingAsync_WithInvalidInput_ThrowsException()
        {
            // Arrange
            var input = TestData.CreateTestCalculationInput();
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Calculate)
                .RespondWithApiError("Invalid calculation parameters", HttpStatusCode.BadRequest);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                pricingService.CalculatePricingAsync(input))
                .ConfigureAwait(false);
        }
        
        [Fact]
        public async Task GetCalculationAsync_WithValidId_ReturnsCalculation()
        {
            // Arrange
            var calculationId = "calc-123";
            var expectedResult = TestData.CreateTestCalculationResult(calculationId, "user-123");
            
            var endpoint = ApiEndpoints.Pricing.GetById.Replace("{id}", calculationId);
            mockHttpHandler
                .When(endpoint)
                .RespondWithApiResponse(expectedResult);
            
            // Act
            var result = await pricingService.GetCalculationAsync(calculationId);
            
            // Assert
            result.Should().NotBeNull();
            result.CalculationId.Should().Be(calculationId);
            result.TotalCost.Should().Be(expectedResult.TotalCost);
        }
        
        [Fact]
        public async Task GetCalculationAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var calculationId = "invalid-id";
            
            var endpoint = ApiEndpoints.Pricing.GetById.Replace("{id}", calculationId);
            mockHttpHandler
                .When(endpoint)
                .RespondWithApiError("Calculation not found", HttpStatusCode.NotFound);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                pricingService.GetCalculationAsync(calculationId))
                .ConfigureAwait(false);
        }
        
        [Fact]
        public async Task SaveCalculationAsync_WithValidModel_ReturnsTrue()
        {
            // Arrange
            var saveModel = new SaveCalculationModel
            {
                CalculationId = "calc-123",
                Name = "Test Calculation"
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Save)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await pricingService.SaveCalculationAsync(saveModel);
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public async Task SaveCalculationAsync_WithInvalidModel_ThrowsException()
        {
            // Arrange
            var saveModel = new SaveCalculationModel
            {
                CalculationId = "invalid-id",
                Name = "Test Calculation"
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Save)
                .RespondWithApiError("Failed to save calculation", HttpStatusCode.BadRequest);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                pricingService.SaveCalculationAsync(saveModel))
                .ConfigureAwait(false);
        }
        
        [Fact]
        public async Task GetCalculationHistoryAsync_ReturnsHistory()
        {
            // Arrange
            var filter = new CalculationFilterModel
            {
                PageNumber = 1,
                PageSize = 10
            };
            
            var historyModel = new CalculationHistoryModel
            {
                Items = new List<CalculationResultModel>
                {
                    TestData.CreateTestCalculationResult("calc-1", "user-123"),
                    TestData.CreateTestCalculationResult("calc-2", "user-123")
                },
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 1
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.History)
                .RespondWithApiResponse(historyModel);
            
            // Act
            var result = await pricingService.GetCalculationHistoryAsync(filter);
            
            // Assert
            result.Should().NotBeNull();
            result.Items.Count.Should().Be(2);
            result.TotalCount.Should().Be(2);
        }
        
        [Fact]
        public async Task DeleteCalculationAsync_WithValidId_ReturnsTrue()
        {
            // Arrange
            var calculationId = "calc-123";
            
            var endpoint = ApiEndpoints.Pricing.Delete.Replace("{id}", calculationId);
            mockHttpHandler
                .When(endpoint)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await pricingService.DeleteCalculationAsync(calculationId);
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public async Task DeleteCalculationAsync_WithInvalidId_ThrowsException()
        {
            // Arrange
            var calculationId = "invalid-id";
            
            var endpoint = ApiEndpoints.Pricing.Delete.Replace("{id}", calculationId);
            mockHttpHandler
                .When(endpoint)
                .RespondWithApiError("Calculation not found", HttpStatusCode.NotFound);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                pricingService.DeleteCalculationAsync(calculationId))
                .ConfigureAwait(false);
        }
        
        [Fact]
        public async Task GetServiceTypeOptionsAsync_ReturnsOptions()
        {
            // Arrange
            var options = new List<ServiceTypeOption>
            {
                new ServiceTypeOption(0, "Standard Filing", "Basic VAT filing service"),
                new ServiceTypeOption(1, "Complex Filing", "Comprehensive VAT filing with additional checks"),
                new ServiceTypeOption(2, "Priority Service", "Expedited VAT filing service")
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.ServiceTypes)
                .RespondWithApiResponse(options);
            
            // Act
            var result = await pricingService.GetServiceTypeOptionsAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].Value.Should().Be(0);
            result[0].Text.Should().Be("Standard Filing");
            result[1].Value.Should().Be(1);
            result[1].Text.Should().Be("Complex Filing");
        }
        
        [Fact]
        public async Task GetFilingFrequencyOptionsAsync_ReturnsOptions()
        {
            // Arrange
            var options = new List<FilingFrequencyOption>
            {
                new FilingFrequencyOption(1, "Monthly", "File VAT returns every month"),
                new FilingFrequencyOption(2, "Quarterly", "File VAT returns every 3 months"),
                new FilingFrequencyOption(3, "Annually", "File VAT returns once a year")
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.FilingFrequencies)
                .RespondWithApiResponse(options);
            
            // Act
            var result = await pricingService.GetFilingFrequencyOptionsAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].Value.Should().Be(1);
            result[0].Text.Should().Be("Monthly");
            result[1].Value.Should().Be(2);
            result[1].Text.Should().Be("Quarterly");
        }
        
        [Fact]
        public async Task GetAdditionalServiceOptionsAsync_ReturnsOptions()
        {
            // Arrange
            var options = new List<AdditionalServiceOption>
            {
                new AdditionalServiceOption("tax-consultancy", "Tax Consultancy", "Expert tax advice", 300.00m, "EUR"),
                new AdditionalServiceOption("historical-data", "Historical Data Processing", "Process prior period data", 200.00m, "EUR"),
                new AdditionalServiceOption("reconciliation", "Reconciliation Services", "Account reconciliation", 250.00m, "EUR")
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.AdditionalServices)
                .RespondWithApiResponse(options);
            
            // Act
            var result = await pricingService.GetAdditionalServiceOptionsAsync();
            
            // Assert
            result.Should().NotBeNull();
            result.Count.Should().Be(3);
            result[0].Value.Should().Be("tax-consultancy");
            result[0].Text.Should().Be("Tax Consultancy");
            result[1].Value.Should().Be("historical-data");
            result[1].Text.Should().Be("Historical Data Processing");
        }
        
        [Fact]
        public async Task CompareCalculationsAsync_WithValidInputs_ReturnsComparisonModel()
        {
            // Arrange
            var inputs = new List<CalculationInputModel>
            {
                TestData.CreateTestCalculationInput(),
                TestData.CreateTestCalculationInput()
            };
            
            // Modify second input for comparison
            inputs[1].ServiceType = 0; // Standard Filing
            inputs[1].TransactionVolume = 300;
            
            var results = new List<CalculationResultModel>
            {
                TestData.CreateTestCalculationResult("calc-1", "user-123"),
                TestData.CreateTestCalculationResult("calc-2", "user-123")
            };
            
            // Modify second result for comparison
            results[1].ServiceType = 0;
            results[1].ServiceTypeName = "Standard Filing";
            results[1].TransactionVolume = 300;
            results[1].TotalCost = 3000.00m; // Lower cost
            
            var comparisonModel = new CalculationComparisonModel
            {
                Calculations = results,
                LowestCostCalculation = results[1]
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Compare)
                .RespondWithApiResponse(comparisonModel);
            
            // Act
            var result = await pricingService.CompareCalculationsAsync(inputs);
            
            // Assert
            result.Should().NotBeNull();
            result.Calculations.Count.Should().Be(2);
            result.LowestCostCalculation.Should().NotBeNull();
            result.LowestCostCalculation.TotalCost.Should().Be(3000.00m);
        }
        
        [Fact]
        public async Task CompareCalculationsAsync_WithInvalidInputs_ThrowsException()
        {
            // Arrange
            var inputs = new List<CalculationInputModel>
            {
                TestData.CreateTestCalculationInput(),
                TestData.CreateTestCalculationInput()
            };
            
            mockHttpHandler
                .When(ApiEndpoints.Pricing.Compare)
                .RespondWithApiError("Invalid comparison parameters", HttpStatusCode.BadRequest);
            
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                pricingService.CompareCalculationsAsync(inputs))
                .ConfigureAwait(false);
        }
    }
}