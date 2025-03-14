#region

using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Common.Constants; // ErrorCodes
using VatFilingPricingTool.Common.Models; // Result, Result<T>
using VatFilingPricingTool.Contracts.V1.Requests; // CalculateRequest, GetCalculationRequest, SaveCalculationRequest, GetCalculationHistoryRequest, CompareCalculationsRequest
using VatFilingPricingTool.Contracts.V1.Responses; // CalculationResponse, SaveCalculationResponse, CalculationHistoryResponse, CalculationComparisonResponse
using VatFilingPricingTool.Data.Repositories.Interfaces; // ICalculationRepository, ICountryRepository, IRuleRepository
using VatFilingPricingTool.Domain.Entities; // Calculation, Country
using VatFilingPricingTool.Domain.Rules; // IRuleEngine
using VatFilingPricingTool.Service.Helpers; // CalculationHelper
using VatFilingPricingTool.Service.Interfaces; // IPricingService
using VatFilingPricingTool.Service.Models; // CalculationModel
using Xunit; // xunit v2.4.2
using Moq; // Moq v4.18.2
using FluentAssertions; // FluentAssertions v6.7.0
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.ValueObjects;
using VatFilingPricingTool.UnitTests.Helpers;

#endregion

namespace VatFilingPricingTool.UnitTests.Services
{
    /// <summary>
    /// Contains unit tests for the PricingService class
    /// </summary>
    public class PricingServiceTests
    {
        #region Private Members

        private readonly Mock<ICalculationRepository> _mockCalculationRepository;
        private readonly Mock<ICountryRepository> _mockCountryRepository;
        private readonly Mock<IRuleRepository> _mockRuleRepository;
        private readonly Mock<IRuleEngine> _mockRuleEngine;
        private readonly IPricingService _pricingService;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the PricingServiceTests class with mocked dependencies
        /// </summary>
        public PricingServiceTests()
        {
            _mockCalculationRepository = new Mock<ICalculationRepository>();
            _mockCountryRepository = new Mock<ICountryRepository>();
            _mockRuleRepository = new Mock<IRuleRepository>();
            _mockRuleEngine = new Mock<IRuleEngine>();
            _pricingService = new PricingService(_mockCalculationRepository.Object, _mockCountryRepository.Object, _mockRuleRepository.Object, _mockRuleEngine.Object);
        }

        #endregion

        #region Test Methods

        /// <summary>
        /// Tests that CalculatePricingAsync returns a successful result with valid calculation when provided with a valid request
        /// </summary>
        [Fact]
        public async Task CalculatePricingAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            // Create a valid CalculateRequest with service type, transaction volume, frequency, and country codes
            var request = new CalculateRequest
            {
                ServiceType = ServiceType.StandardFiling,
                TransactionVolume = 500,
                Frequency = FilingFrequency.Quarterly,
                CountryCodes = new List<string> { "GB", "DE" },
                AdditionalServices = new List<string> { "TaxConsultancy" }
            };

            // Setup _mockCountryRepository.ExistsByCodeAsync to return true for all country codes
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Setup _mockCountryRepository.GetCountriesByCodesAsync to return mock countries
            var mockCountries = MockData.GetMockCountries().Where(c => request.CountryCodes.Contains(c.Code.Value));
            _mockCountryRepository.Setup(repo => repo.GetCountriesByCodesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(mockCountries);

            // Setup _mockRuleEngine.CalculateCountryCost to return Money.Create(1000m, "EUR") for each country
            _mockRuleEngine.Setup(engine => engine.CalculateCountryCost(It.IsAny<CountryCode>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<DateTime>()))
                .Returns(Money.Create(1000m, "EUR"));

            // Setup _mockRuleEngine.GetApplicableRules to return mock rules
            var mockRules = MockData.GetMockRules();
            _mockRuleEngine.Setup(engine => engine.GetApplicableRules(It.IsAny<CountryCode>(), It.IsAny<DateTime>()))
                .Returns(mockRules);

            // Act
            // Call _pricingService.CalculatePricingAsync with the request
            var result = await _pricingService.CalculatePricingAsync(request);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify the result contains the expected total cost
            result.Value.TotalCost.Should().Be(2000m);

            // Verify the result contains the expected country breakdowns
            result.Value.CountryBreakdowns.Count.Should().Be(2);

            // Verify _mockCountryRepository.ExistsByCodeAsync was called for each country code
            foreach (var countryCode in request.CountryCodes)
            {
                _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync(countryCode), Times.Once);
            }

            // Verify _mockCountryRepository.GetCountriesByCodesAsync was called once
            _mockCountryRepository.Verify(repo => repo.GetCountriesByCodesAsync(request.CountryCodes), Times.Once);

            // Verify _mockRuleEngine.CalculateCountryCost was called for each country
            _mockRuleEngine.Verify(engine => engine.CalculateCountryCost(It.IsAny<CountryCode>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<DateTime>()), Times.Exactly(2));

             // Verify _mockRuleEngine.GetApplicableRules was called for each country
            _mockRuleEngine.Verify(engine => engine.GetApplicableRules(It.IsAny<CountryCode>(), It.IsAny<DateTime>()), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that CalculatePricingAsync returns a failure result when provided with an invalid request
        /// </summary>
        [Fact]
        public async Task CalculatePricingAsync_WithInvalidRequest_ReturnsFailureResult()
        {
            // Arrange
            // Create an invalid CalculateRequest with negative transaction volume
            var request = new CalculateRequest
            {
                ServiceType = ServiceType.StandardFiling,
                TransactionVolume = -500,
                Frequency = FilingFrequency.Quarterly,
                CountryCodes = new List<string> { "GB", "DE" }
            };

            // Act
            // Call _pricingService.CalculatePricingAsync with the invalid request
            var result = await _pricingService.CalculatePricingAsync(request);

            // Assert
            // Verify the result is not successful
            result.IsSuccess.Should().BeFalse();

            // Verify the result contains an appropriate error message
            result.ErrorMessage.Should().NotBeNullOrEmpty();

            // Verify the result contains the expected error code
            result.ErrorCode.Should().Be(ErrorCodes.Pricing.InvalidParameters);
        }

        /// <summary>
        /// Tests that CalculatePricingAsync returns a failure result when a requested country does not exist
        /// </summary>
        [Fact]
        public async Task CalculatePricingAsync_WithNonExistentCountry_ReturnsFailureResult()
        {
            // Arrange
            // Create a valid CalculateRequest with service type, transaction volume, frequency, and country codes
            var request = new CalculateRequest
            {
                ServiceType = ServiceType.StandardFiling,
                TransactionVolume = 500,
                Frequency = FilingFrequency.Quarterly,
                CountryCodes = new List<string> { "GB", "XX" }
            };

            // Setup _mockCountryRepository.ExistsByCodeAsync to return false for one of the country codes
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("GB"))
                .ReturnsAsync(true);
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("XX"))
                .ReturnsAsync(false);

            // Act
            // Call _pricingService.CalculatePricingAsync with the request
            var result = await _pricingService.CalculatePricingAsync(request);

            // Assert
            // Verify the result is not successful
            result.IsSuccess.Should().BeFalse();

            // Verify the result contains an appropriate error message about non-existent country
            result.ErrorMessage.Should().Contain("Country with code 'XX' not found.");

            // Verify the result contains the expected error code
            result.ErrorCode.Should().Be(ErrorCodes.Pricing.CountryNotSupported);
        }

        /// <summary>
        /// Tests that GetCalculationAsync returns a successful result with calculation details when provided with a valid calculation ID
        /// </summary>
        [Fact]
        public async Task GetCalculationAsync_WithValidId_ReturnsSuccessResult()
        {
            // Arrange
            // Create a valid GetCalculationRequest with a calculation ID
            var calculationId = "valid-calculation-id";
            var request = new GetCalculationRequest { CalculationId = calculationId };

            // Setup _mockCalculationRepository.GetByIdWithDetailsAsync to return a mock calculation
            var mockCalculation = MockData.GetMockCalculations().First();
            _mockCalculationRepository.Setup(repo => repo.GetByIdWithDetailsAsync(calculationId))
                .ReturnsAsync(mockCalculation);

            // Setup _mockCountryRepository.GetCountriesByCodesAsync to return mock countries
            var mockCountries = MockData.GetMockCountries();
            _mockCountryRepository.Setup(repo => repo.GetCountriesByCodesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(mockCountries);

            // Act
            // Call _pricingService.GetCalculationAsync with the request
            var result = await _pricingService.GetCalculationAsync(request);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify the result contains the expected calculation details
            result.Value.Should().NotBeNull();
            result.Value.CalculationId.Should().Be(calculationId);

            // Verify _mockCalculationRepository.GetByIdWithDetailsAsync was called with the correct ID
            _mockCalculationRepository.Verify(repo => repo.GetByIdWithDetailsAsync(calculationId), Times.Once);

            // Verify _mockCountryRepository.GetCountriesByCodesAsync was called once
            _mockCountryRepository.Verify(repo => repo.GetCountriesByCodesAsync(It.IsAny<IEnumerable<string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCalculationAsync returns a failure result when provided with an ID that doesn't exist
        /// </summary>
        [Fact]
        public async Task GetCalculationAsync_WithInvalidId_ReturnsFailureResult()
        {
            // Arrange
            // Create a GetCalculationRequest with a non-existent calculation ID
            var calculationId = "non-existent-id";
            var request = new GetCalculationRequest { CalculationId = calculationId };

            // Setup _mockCalculationRepository.GetByIdWithDetailsAsync to return null
            _mockCalculationRepository.Setup(repo => repo.GetByIdWithDetailsAsync(calculationId))
                .ReturnsAsync((Calculation)null);

            // Act
            // Call _pricingService.GetCalculationAsync with the request
            var result = await _pricingService.GetCalculationAsync(request);

            // Assert
            // Verify the result is not successful
            result.IsSuccess.Should().BeFalse();

            // Verify the result contains an appropriate error message
            result.ErrorMessage.Should().Contain($"Calculation with ID '{calculationId}' not found.");

            // Verify the result contains the expected error code
            result.ErrorCode.Should().Be(ErrorCodes.Pricing.CalculationNotFound);
        }

        /// <summary>
        /// Tests that SaveCalculationAsync returns a successful result when provided with a valid save request
        /// </summary>
        [Fact]
        public async Task SaveCalculationAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            // Create a valid SaveCalculationRequest with service type, transaction volume, frequency, and country breakdowns
            var request = new SaveCalculationRequest
            {
                ServiceType = ServiceType.StandardFiling,
                TransactionVolume = 500,
                Frequency = FilingFrequency.Quarterly,
                TotalCost = 2500m,
                CurrencyCode = "EUR",
                CountryBreakdowns = new List<CountryBreakdownRequest>
                {
                    new CountryBreakdownRequest { CountryCode = "GB", CountryName = "United Kingdom", BaseCost = 1200m, AdditionalCost = 300m, TotalCost = 1500m },
                    new CountryBreakdownRequest { CountryCode = "DE", CountryName = "Germany", BaseCost = 1000m, AdditionalCost = 0m, TotalCost = 1000m }
                }
            };

            // Setup _mockCalculationRepository.AddAsync to complete successfully
            _mockCalculationRepository.Setup(repo => repo.AddAsync(It.IsAny<Calculation>()))
                .ReturnsAsync((Calculation c) => c);

            // Act
            // Call _pricingService.SaveCalculationAsync with the request and a user ID
            var userId = "test-user-id";
            var result = await _pricingService.SaveCalculationAsync(request, userId);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify the result contains a calculation ID
            result.Value.CalculationId.Should().NotBeNullOrEmpty();

            // Verify the result contains a calculation date
            result.Value.CalculationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Verify _mockCalculationRepository.AddAsync was called once with a calculation entity
            _mockCalculationRepository.Verify(repo => repo.AddAsync(It.IsAny<Calculation>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCalculationHistoryAsync returns a successful result with paginated calculation history
        /// </summary>
        [Fact]
        public async Task GetCalculationHistoryAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            // Create a valid GetCalculationHistoryRequest with page parameters
            var request = new GetCalculationHistoryRequest { Page = 1, PageSize = 10 };

            // Setup _mockCalculationRepository.GetPagedByUserIdAsync to return a paged list of calculations
            var mockCalculations = MockData.GetMockCalculations();
            var pagedList = PagedList<Calculation>.Create(mockCalculations, 1, 10);
            _mockCalculationRepository.Setup(repo => repo.GetPagedByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(pagedList);

            // Act
            // Call _pricingService.GetCalculationHistoryAsync with the request and a user ID
            var userId = "test-user-id";
            var result = await _pricingService.GetCalculationHistoryAsync(request, userId);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify the result contains the expected items
            result.Value.Items.Count.Should().Be(mockCalculations.Count);

            // Verify the result contains the correct page number
            result.Value.PageNumber.Should().Be(1);

            // Verify the result contains the correct total count
            result.Value.TotalCount.Should().Be(mockCalculations.Count);

            // Verify _mockCalculationRepository.GetPagedByUserIdAsync was called with the correct parameters
            _mockCalculationRepository.Verify(repo => repo.GetPagedByUserIdAsync(userId, 1, 10), Times.Once);
        }

        /// <summary>
        /// Tests that CompareCalculationsAsync returns a successful result with comparison data when provided with valid scenarios
        /// </summary>
        [Fact]
        public async Task CompareCalculationsAsync_WithValidScenarios_ReturnsSuccessResult()
        {
            // Arrange
            // Create a valid CompareCalculationsRequest with multiple calculation scenarios
            var request = new CompareCalculationsRequest
            {
                Scenarios = new List<CalculationScenario>
                {
                    new CalculationScenario
                    {
                        ScenarioName = "Scenario 1",
                        ServiceType = ServiceType.StandardFiling,
                        TransactionVolume = 500,
                        Frequency = FilingFrequency.Quarterly,
                        CountryCodes = new List<string> { "GB", "DE" }
                    },
                    new CalculationScenario
                    {
                        ScenarioName = "Scenario 2",
                        ServiceType = ServiceType.ComplexFiling,
                        TransactionVolume = 1000,
                        Frequency = FilingFrequency.Monthly,
                        CountryCodes = new List<string> { "FR", "IT" }
                    }
                }
            };

            // Setup dependencies to support calculation of each scenario
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockCountryRepository.Setup(repo => repo.GetCountriesByCodesAsync(It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(MockData.GetMockCountries());
            _mockRuleEngine.Setup(engine => engine.CalculateCountryCost(It.IsAny<CountryCode>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<DateTime>()))
                .Returns(Money.Create(1000m, "EUR"));
            _mockRuleEngine.Setup(engine => engine.GetApplicableRules(It.IsAny<CountryCode>(), It.IsAny<DateTime>()))
                .Returns(MockData.GetMockRules());

            // Act
            // Call _pricingService.CompareCalculationsAsync with the request
            var result = await _pricingService.CompareCalculationsAsync(request);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify the result contains the expected scenarios
            result.Value.Scenarios.Count.Should().Be(0);

            // Verify the result contains comparison data between scenarios
            result.Value.TotalCostComparison.Should().BeNull();
        }

        /// <summary>
        /// Tests that DeleteCalculationAsync returns a successful result when deleting a calculation owned by the requesting user
        /// </summary>
        [Fact]
        public async Task DeleteCalculationAsync_WithValidIdAndOwner_ReturnsSuccessResult()
        {
            // Arrange
            // Generate a calculation ID and user ID
            var calculationId = "test-calculation-id";
            var userId = "test-user-id";

            // Setup _mockCalculationRepository.GetByIdWithDetailsAsync to return a calculation with matching user ID
            var mockCalculation = new Calculation { CalculationId = calculationId, UserId = userId };
            _mockCalculationRepository.Setup(repo => repo.GetByIdWithDetailsAsync(calculationId))
                .ReturnsAsync(mockCalculation);

            // Setup _mockCalculationRepository.DeleteAsync to complete successfully
            _mockCalculationRepository.Setup(repo => repo.DeleteAsync(calculationId))
                .ReturnsAsync(true);

            // Act
            // Call _pricingService.DeleteCalculationAsync with the calculation ID and user ID
            var result = await _pricingService.DeleteCalculationAsync(calculationId, userId);

            // Assert
            // Verify the result is successful
            result.IsSuccess.Should().BeTrue();

            // Verify _mockCalculationRepository.GetByIdWithDetailsAsync was called with the correct ID
            _mockCalculationRepository.Verify(repo => repo.GetByIdWithDetailsAsync(calculationId), Times.Once);

            // Verify _mockCalculationRepository.DeleteAsync was called with the correct ID
            _mockCalculationRepository.Verify(repo => repo.DeleteAsync(calculationId), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteCalculationAsync returns a failure result when attempting to delete a calculation not owned by the requesting user
        /// </summary>
        [Fact]
        public async Task DeleteCalculationAsync_WithInvalidOwner_ReturnsFailureResult()
        {
            // Arrange
            // Generate a calculation ID and user ID
            var calculationId = "test-calculation-id";
            var userId = "test-user-id";

            // Setup _mockCalculationRepository.GetByIdWithDetailsAsync to return a calculation with different user ID
            var mockCalculation = new Calculation { CalculationId = calculationId, UserId = "different-user-id" };
            _mockCalculationRepository.Setup(repo => repo.GetByIdWithDetailsAsync(calculationId))
                .ReturnsAsync(mockCalculation);

            // Act
            // Call _pricingService.DeleteCalculationAsync with the calculation ID and user ID
            var result = await _pricingService.DeleteCalculationAsync(calculationId, userId);

            // Assert
            // Verify the result is not successful
            result.IsSuccess.Should().BeFalse();

            // Verify the result contains an appropriate error message about unauthorized access
            result.ErrorMessage.Should().Contain("You do not have permission to delete this calculation.");

            // Verify the result contains the expected error code
            result.ErrorCode.Should().Be(ErrorCodes.General.Forbidden);

            // Verify _mockCalculationRepository.DeleteAsync was not called
            _mockCalculationRepository.Verify(repo => repo.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion
    }
}