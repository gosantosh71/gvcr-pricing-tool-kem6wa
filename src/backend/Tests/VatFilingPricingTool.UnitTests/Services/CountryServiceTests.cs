#region

using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using Moq; // Moq v4.18.2
using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Linq; // System.Linq v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Common.Constants; // Error codes for validation failures
using VatFilingPricingTool.Common.Models; // Result wrapper for service responses
using VatFilingPricingTool.Contracts.V1.Requests; // Request model for retrieving, creating, updating, and deleting a country
using VatFilingPricingTool.Contracts.V1.Responses; // Response model for country data
using VatFilingPricingTool.Domain.Entities; // Domain entity for test data
using VatFilingPricingTool.Domain.Enums; // Enum for filing frequency in test data
using VatFilingPricingTool.Service.Implementations; // Implementation being tested
using VatFilingPricingTool.Service.Interfaces; // Interface being tested
using VatFilingPricingTool.UnitTests.Helpers; // Provides mock country data for testing
using Xunit; // Testing framework
using FluentAssertions; // Fluent assertions for test validation
using VatFilingPricingTool.Data.Repositories.Interfaces; // Repository interface to be mocked

#endregion

namespace VatFilingPricingTool.UnitTests.Services
{
    /// <summary>
    ///     Test class for the CountryService implementation
    /// </summary>
    public class CountryServiceTests
    {
        #region Properties

        private readonly Mock<ICountryRepository> _mockCountryRepository;
        private readonly Mock<ILogger<CountryService>> _mockLogger;
        private readonly ICountryService _countryService;
        private readonly List<Country> _mockCountries;

        #endregion

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the CountryServiceTests class with mocked dependencies
        /// </summary>
        public CountryServiceTests()
        {
            _mockCountryRepository = new Mock<ICountryRepository>();
            _mockLogger = new Mock<ILogger<CountryService>>();
            _mockCountries = MockData.GetMockCountries();
            _countryService = new CountryService(_mockCountryRepository.Object, _mockLogger.Object);
        }

        #endregion

        #region Test Methods

        /// <summary>
        ///     Tests that GetCountryAsync returns a country when a valid country code is provided
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithValidCode_ReturnsCountry()
        {
            // Arrange
            var expectedCountry = _mockCountries.FirstOrDefault(c => c.Code.Value == "GB");
            _mockCountryRepository.Setup(repo => repo.GetByCodeAsync("GB")).ReturnsAsync(expectedCountry);

            // Act
            var request = new GetCountryRequest { CountryCode = "GB" };
            var result = await _countryService.GetCountryAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("GB", result.Value.CountryCode);
            Assert.Equal("United Kingdom", result.Value.Name);
            _mockCountryRepository.Verify(repo => repo.GetByCodeAsync("GB"), Times.Once);
        }

        /// <summary>
        ///     Tests that GetCountryAsync returns a not found error when an invalid country code is provided
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithInvalidCode_ReturnsNotFound()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.GetByCodeAsync("XX")).ReturnsAsync((Country)null);

            // Act
            var request = new GetCountryRequest { CountryCode = "XX" };
            var result = await _countryService.GetCountryAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.Country.CountryNotFound, result.ErrorCode);
            _mockCountryRepository.Verify(repo => repo.GetByCodeAsync("XX"), Times.Once);
        }

        /// <summary>
        ///     Tests that GetCountryAsync returns a bad request error when the request is null
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithNullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _countryService.GetCountryAsync(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.General.BadRequest, result.ErrorCode);
            _mockCountryRepository.Verify(repo => repo.GetByCodeAsync(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Tests that GetCountriesAsync returns a paged list of countries when a valid request is provided
        /// </summary>
        [Fact]
        public async Task GetCountriesAsync_WithValidRequest_ReturnsPagedCountries()
        {
            // Arrange
            _mockCountryRepository
                .Setup(repo => repo.GetPagedCountriesAsync(1, 10, true))
                .ReturnsAsync((_mockCountries, 5));

            // Act
            var request = new GetCountriesRequest { Page = 1, PageSize = 10, ActiveOnly = true };
            var result = await _countryService.GetCountriesAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(5, result.Value.Items.Count);
            Assert.Equal(1, result.Value.PageNumber);
            Assert.Equal(10, result.Value.PageSize);
            Assert.Equal(5, result.Value.TotalCount);
            Assert.Equal(1, result.Value.TotalPages);
            _mockCountryRepository.Verify(repo => repo.GetPagedCountriesAsync(1, 10, true), Times.Once);
        }

        /// <summary>
        ///     Tests that GetActiveCountriesAsync returns all active countries
        /// </summary>
        [Fact]
        public async Task GetActiveCountriesAsync_ReturnsActiveCountries()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.GetActiveCountriesAsync()).ReturnsAsync(_mockCountries);

            // Act
            var result = await _countryService.GetActiveCountriesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(_mockCountries.Count, result.Value.Count);
            _mockCountryRepository.Verify(repo => repo.GetActiveCountriesAsync(), Times.Once);
        }

        /// <summary>
        ///     Tests that GetCountriesByFilingFrequencyAsync returns countries that support a specific filing frequency
        /// </summary>
        [Fact]
        public async Task GetCountriesByFilingFrequencyAsync_ReturnsFilteredCountries()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.GetCountriesByFilingFrequencyAsync(FilingFrequency.Quarterly))
                .ReturnsAsync(_mockCountries.Where(c => c.AvailableFilingFrequencies.Contains(FilingFrequency.Quarterly)));

            // Act
            var result = await _countryService.GetCountriesByFilingFrequencyAsync(FilingFrequency.Quarterly);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value.All(c => c.AvailableFilingFrequencies.Contains(FilingFrequency.Quarterly)));
            _mockCountryRepository.Verify(repo => repo.GetCountriesByFilingFrequencyAsync(FilingFrequency.Quarterly), Times.Once);
        }

        /// <summary>
        ///     Tests that GetCountrySummariesAsync returns simplified country information
        /// </summary>
        [Fact]
        public async Task GetCountrySummariesAsync_ReturnsCountrySummaries()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.GetActiveCountriesAsync()).ReturnsAsync(_mockCountries);

            // Act
            var result = await _countryService.GetCountrySummariesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(_mockCountries.Count, result.Value.Count);
            Assert.True(result.Value.All(c => !string.IsNullOrEmpty(c.CountryCode) && !string.IsNullOrEmpty(c.Name) && c.StandardVatRate > 0));
            _mockCountryRepository.Verify(repo => repo.GetActiveCountriesAsync(), Times.Once);
        }

        /// <summary>
        ///     Tests that CreateCountryAsync creates a new country when a valid request is provided
        /// </summary>
        [Fact]
        public async Task CreateCountryAsync_WithValidRequest_CreatesCountry()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("NL")).ReturnsAsync(false);
            _mockCountryRepository.Setup(repo => repo.CreateAsync(It.IsAny<Country>()))
                .ReturnsAsync((Country country) => country);

            var request = new CreateCountryRequest
            {
                CountryCode = "NL",
                Name = "Netherlands",
                StandardVatRate = 21.0m,
                CurrencyCode = "EUR",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // Act
            var result = await _countryService.CreateCountryAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("NL", result.Value.CountryCode);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("NL"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.CreateAsync(It.Is<Country>(c => c.Code.Value == "NL")), Times.Once);
        }

        /// <summary>
        ///     Tests that CreateCountryAsync returns a duplicate error when the country code already exists
        /// </summary>
        [Fact]
        public async Task CreateCountryAsync_WithExistingCode_ReturnsDuplicateError()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("GB")).ReturnsAsync(true);

            var request = new CreateCountryRequest
            {
                CountryCode = "GB",
                Name = "United Kingdom",
                StandardVatRate = 20.0m,
                CurrencyCode = "GBP",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly }
            };

            // Act
            var result = await _countryService.CreateCountryAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.Country.DuplicateCountryCode, result.ErrorCode);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("GB"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.CreateAsync(It.IsAny<Country>()), Times.Never);
        }

        /// <summary>
        ///     Tests that UpdateCountryAsync updates an existing country when a valid request is provided
        /// </summary>
        [Fact]
        public async Task UpdateCountryAsync_WithValidRequest_UpdatesCountry()
        {
            // Arrange
            var existingCountry = _mockCountries.FirstOrDefault(c => c.Code.Value == "GB");
            _mockCountryRepository.Setup(repo => repo.GetByCodeAsync("GB")).ReturnsAsync(existingCountry);
            _mockCountryRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Country>()))
                .ReturnsAsync((Country country) => country);

            var request = new UpdateCountryRequest
            {
                CountryCode = "GB",
                Name = "Great Britain",
                StandardVatRate = 21.0m,
                CurrencyCode = "GBP",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Quarterly },
                IsActive = false
            };

            // Act
            var result = await _countryService.UpdateCountryAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Great Britain", result.Value.Name);
            Assert.Equal(21.0m, result.Value.StandardVatRate);
            Assert.False(result.Value.IsActive);
            _mockCountryRepository.Verify(repo => repo.GetByCodeAsync("GB"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.UpdateAsync(It.Is<Country>(c => c.Name == "Great Britain")), Times.Once);
        }

        /// <summary>
        ///     Tests that UpdateCountryAsync returns a not found error when the country code doesn't exist
        /// </summary>
        [Fact]
        public async Task UpdateCountryAsync_WithNonExistingCode_ReturnsNotFound()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.GetByCodeAsync("XX")).ReturnsAsync((Country)null);

            var request = new UpdateCountryRequest
            {
                CountryCode = "XX",
                Name = "NonExistingCountry",
                StandardVatRate = 20.0m,
                CurrencyCode = "EUR",
                AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Monthly },
                IsActive = true
            };

            // Act
            var result = await _countryService.UpdateCountryAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.Country.CountryNotFound, result.ErrorCode);
            _mockCountryRepository.Verify(repo => repo.GetByCodeAsync("XX"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Country>()), Times.Never);
        }

        /// <summary>
        ///     Tests that DeleteCountryAsync deletes an existing country when a valid request is provided
        /// </summary>
        [Fact]
        public async Task DeleteCountryAsync_WithValidRequest_DeletesCountry()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("GB")).ReturnsAsync(true);
            _mockCountryRepository.Setup(repo => repo.DeleteByCodeAsync("GB")).ReturnsAsync(true);

            var request = new DeleteCountryRequest { CountryCode = "GB" };

            // Act
            var result = await _countryService.DeleteCountryAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("GB"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.DeleteByCodeAsync("GB"), Times.Once);
        }

        /// <summary>
        ///     Tests that DeleteCountryAsync returns a not found error when the country code doesn't exist
        /// </summary>
        [Fact]
        public async Task DeleteCountryAsync_WithNonExistingCode_ReturnsNotFound()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("XX")).ReturnsAsync(false);

            var request = new DeleteCountryRequest { CountryCode = "XX" };

            // Act
            var result = await _countryService.DeleteCountryAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorCodes.Country.CountryNotFound, result.ErrorCode);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("XX"), Times.Once);
            _mockCountryRepository.Verify(repo => repo.DeleteByCodeAsync(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        ///     Tests that CountryExistsAsync returns true when the country code exists
        /// </summary>
        [Fact]
        public async Task CountryExistsAsync_WithExistingCode_ReturnsTrue()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("GB")).ReturnsAsync(true);

            // Act
            var result = await _countryService.CountryExistsAsync("GB");

            // Assert
            Assert.True(result);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("GB"), Times.Once);
        }

        /// <summary>
        ///     Tests that CountryExistsAsync returns false when the country code doesn't exist
        /// </summary>
        [Fact]
        public async Task CountryExistsAsync_WithNonExistingCode_ReturnsFalse()
        {
            // Arrange
            _mockCountryRepository.Setup(repo => repo.ExistsByCodeAsync("XX")).ReturnsAsync(false);

            // Act
            var result = await _countryService.CountryExistsAsync("XX");

            // Assert
            Assert.False(result);
            _mockCountryRepository.Verify(repo => repo.ExistsByCodeAsync("XX"), Times.Once);
        }

        #endregion
    }
}