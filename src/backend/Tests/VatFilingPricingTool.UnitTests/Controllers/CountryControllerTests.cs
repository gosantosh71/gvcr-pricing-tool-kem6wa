#nullable enable
using System; // System v6.0.0
using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Linq; // System.Linq v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc v6.0.0
using Moq; // Moq v4.18.2
using Xunit; // Xunit v2.4.2
using FluentAssertions; // FluentAssertions v6.7.0
using VatFilingPricingTool.Api.Controllers;
using VatFilingPricingTool.Service.Interfaces;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.UnitTests.Helpers;

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Contains unit tests for the CountryController class to verify its behavior for country-related operations
    /// </summary>
    public class CountryControllerTests
    {
        private readonly Mock<ICountryService> _mockCountryService;
        private readonly CountryController _controller;

        /// <summary>
        /// Initializes a new instance of the CountryControllerTests class with mocked dependencies
        /// </summary>
        public CountryControllerTests()
        {
            _mockCountryService = new Mock<ICountryService>();
            _controller = new CountryController(_mockCountryService.Object);
        }

        /// <summary>
        /// Tests that GetCountryAsync returns a successful response with country data when given a valid request
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithValidRequest_ReturnsOkWithCountry()
        {
            // Arrange
            var request = new GetCountryRequest { CountryCode = "GB" };
            var countryResponse = new CountryResponse { CountryCode = "GB", Name = "United Kingdom" };
            _mockCountryService.Setup(x => x.GetCountryAsync(It.IsAny<GetCountryRequest>()))
                .ReturnsAsync(Result<CountryResponse>.Success(countryResponse));

            // Act
            var result = await _controller.GetCountryAsync(request.CountryCode);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<CountryResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(countryResponse);
        }

        /// <summary>
        /// Tests that GetCountryAsync returns a bad request response when given a null request
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.GetCountryAsync(null);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var apiResponse = badRequestResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that GetCountryAsync returns a not found response when the country does not exist
        /// </summary>
        [Fact]
        public async Task GetCountryAsync_WhenCountryNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new GetCountryRequest { CountryCode = "XX" };
            _mockCountryService.Setup(x => x.GetCountryAsync(It.IsAny<GetCountryRequest>()))
                .ReturnsAsync(Result<CountryResponse>.Failure("Country not found", "COUNTRY-001"));

            // Act
            var result = await _controller.GetCountryAsync(request.CountryCode);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            var apiResponse = notFoundResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Country not found");
        }

        /// <summary>
        /// Tests that GetCountriesAsync returns a successful response with paginated countries when given a valid request
        /// </summary>
        [Fact]
        public async Task GetCountriesAsync_WithValidRequest_ReturnsOkWithCountries()
        {
            // Arrange
            var request = new GetCountriesRequest { Page = 1, PageSize = 10 };
            var countriesResponse = new CountriesResponse { Items = new List<CountryResponse> { new CountryResponse { CountryCode = "GB", Name = "United Kingdom" } }, PageNumber = 1, PageSize = 10, TotalCount = 1 };
            _mockCountryService.Setup(x => x.GetCountriesAsync(It.IsAny<GetCountriesRequest>()))
                .ReturnsAsync(Result<CountriesResponse>.Success(countriesResponse));

            // Act
            var result = await _controller.GetCountriesAsync(request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<CountriesResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(countriesResponse);
        }

        /// <summary>
        /// Tests that GetActiveCountriesAsync returns a successful response with active countries
        /// </summary>
        [Fact]
        public async Task GetActiveCountriesAsync_ReturnsOkWithActiveCountries()
        {
            // Arrange
            var activeCountries = new List<CountryResponse> { new CountryResponse { CountryCode = "GB", Name = "United Kingdom", IsActive = true } };
            _mockCountryService.Setup(x => x.GetActiveCountriesAsync())
                .ReturnsAsync(Result<List<CountryResponse>>.Success(activeCountries));

            // Act
            var result = await _controller.GetActiveCountriesAsync();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<List<CountryResponse>>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(activeCountries);
        }

        /// <summary>
        /// Tests that GetCountriesByFilingFrequencyAsync returns a successful response with countries that support the specified filing frequency
        /// </summary>
        [Fact]
        public async Task GetCountriesByFilingFrequencyAsync_WithValidFrequency_ReturnsOkWithCountries()
        {
            // Arrange
            var countries = new List<CountryResponse> { new CountryResponse { CountryCode = "GB", Name = "United Kingdom" } };
            _mockCountryService.Setup(x => x.GetCountriesByFilingFrequencyAsync(It.IsAny<FilingFrequency>()))
                .ReturnsAsync(Result<List<CountryResponse>>.Success(countries));

            // Act
            var result = await _controller.GetCountriesByFilingFrequencyAsync(FilingFrequency.Quarterly);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<List<CountryResponse>>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(countries);
        }

        /// <summary>
        /// Tests that GetCountrySummariesAsync returns a successful response with country summaries
        /// </summary>
        [Fact]
        public async Task GetCountrySummariesAsync_ReturnsOkWithSummaries()
        {
            // Arrange
            var summaries = new List<CountrySummaryResponse> { new CountrySummaryResponse { CountryCode = "GB", Name = "United Kingdom" } };
            _mockCountryService.Setup(x => x.GetCountrySummariesAsync())
                .ReturnsAsync(Result<List<CountrySummaryResponse>>.Success(summaries));

            // Act
            var result = await _controller.GetCountrySummariesAsync();

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<List<CountrySummaryResponse>>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(summaries);
        }

        /// <summary>
        /// Tests that CreateCountryAsync returns a created response with the new country when given a valid request
        /// </summary>
        [Fact]
        public async Task CreateCountryAsync_WithValidRequest_ReturnsCreatedWithCountry()
        {
            // Arrange
            var request = new CreateCountryRequest { CountryCode = "GB", Name = "United Kingdom", StandardVatRate = 20.0m, CurrencyCode = "GBP", AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Quarterly } };
            var createCountryResponse = new CreateCountryResponse { CountryCode = "GB", Success = true };
            _mockCountryService.Setup(x => x.CreateCountryAsync(It.IsAny<CreateCountryRequest>()))
                .ReturnsAsync(Result<CreateCountryResponse>.Success(createCountryResponse));

            // Act
            var result = await _controller.CreateCountryAsync(request);

            // Assert
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            createdAtActionResult.Should().NotBeNull();
            var apiResponse = createdAtActionResult.Value as ApiResponse<CreateCountryResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(createCountryResponse);
        }

        /// <summary>
        /// Tests that CreateCountryAsync returns a bad request response when given a null request
        /// </summary>
        [Fact]
        public async Task CreateCountryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.CreateCountryAsync(null);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var apiResponse = badRequestResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that CreateCountryAsync returns a conflict response when the country already exists
        /// </summary>
        [Fact]
        public async Task CreateCountryAsync_WhenCountryExists_ReturnsConflict()
        {
            // Arrange
            var request = new CreateCountryRequest { CountryCode = "GB", Name = "United Kingdom", StandardVatRate = 20.0m, CurrencyCode = "GBP", AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Quarterly } };
            _mockCountryService.Setup(x => x.CreateCountryAsync(It.IsAny<CreateCountryRequest>()))
                .ReturnsAsync(Result<CreateCountryResponse>.Failure("Country already exists", "COUNTRY-003"));

            // Act
            var result = await _controller.CreateCountryAsync(request);

            // Assert
            var conflictResult = result.Result as ConflictObjectResult;
            conflictResult.Should().NotBeNull();
            var apiResponse = conflictResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Country already exists");
        }

        /// <summary>
        /// Tests that UpdateCountryAsync returns a success response with the updated country when given a valid request
        /// </summary>
        [Fact]
        public async Task UpdateCountryAsync_WithValidRequest_ReturnsOkWithUpdatedCountry()
        {
            // Arrange
            var request = new UpdateCountryRequest { CountryCode = "GB", Name = "United Kingdom", StandardVatRate = 20.0m, CurrencyCode = "GBP", AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Quarterly }, IsActive = true };
            var updateCountryResponse = new UpdateCountryResponse { CountryCode = "GB", Success = true };
            _mockCountryService.Setup(x => x.UpdateCountryAsync(It.IsAny<UpdateCountryRequest>()))
                .ReturnsAsync(Result<UpdateCountryResponse>.Success(updateCountryResponse));

            // Act
            var result = await _controller.UpdateCountryAsync(request.CountryCode, request);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<UpdateCountryResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(updateCountryResponse);
        }

        /// <summary>
        /// Tests that UpdateCountryAsync returns a bad request response when given a null request
        /// </summary>
        [Fact]
        public async Task UpdateCountryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.UpdateCountryAsync("GB", null);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var apiResponse = badRequestResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that UpdateCountryAsync returns a not found response when the country does not exist
        /// </summary>
        [Fact]
        public async Task UpdateCountryAsync_WhenCountryNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new UpdateCountryRequest { CountryCode = "XX", Name = "NonExistent", StandardVatRate = 20.0m, CurrencyCode = "EUR", AvailableFilingFrequencies = new List<FilingFrequency> { FilingFrequency.Quarterly }, IsActive = true };
            _mockCountryService.Setup(x => x.UpdateCountryAsync(It.IsAny<UpdateCountryRequest>()))
                .ReturnsAsync(Result<UpdateCountryResponse>.Failure("Country not found", "COUNTRY-001"));

            // Act
            var result = await _controller.UpdateCountryAsync(request.CountryCode, request);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            var apiResponse = notFoundResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Country not found");
        }

        /// <summary>
        /// Tests that DeleteCountryAsync returns a success response with the deleted country when given a valid request
        /// </summary>
        [Fact]
        public async Task DeleteCountryAsync_WithValidRequest_ReturnsOkWithDeletedCountry()
        {
            // Arrange
            var request = new DeleteCountryRequest { CountryCode = "GB" };
            var deleteCountryResponse = new DeleteCountryResponse { CountryCode = "GB", Success = true };
            _mockCountryService.Setup(x => x.DeleteCountryAsync(It.IsAny<DeleteCountryRequest>()))
                .ReturnsAsync(Result<DeleteCountryResponse>.Success(deleteCountryResponse));

            // Act
            var result = await _controller.DeleteCountryAsync(request.CountryCode);

            // Assert
            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            var apiResponse = okResult.Value as ApiResponse<DeleteCountryResponse>;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(deleteCountryResponse);
        }

        /// <summary>
        /// Tests that DeleteCountryAsync returns a bad request response when given a null request
        /// </summary>
        [Fact]
        public async Task DeleteCountryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange

            // Act
            var result = await _controller.DeleteCountryAsync(null);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            var apiResponse = badRequestResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that DeleteCountryAsync returns a not found response when the country does not exist
        /// </summary>
        [Fact]
        public async Task DeleteCountryAsync_WhenCountryNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new DeleteCountryRequest { CountryCode = "XX" };
            _mockCountryService.Setup(x => x.DeleteCountryAsync(It.IsAny<DeleteCountryRequest>()))
                .ReturnsAsync(Result<DeleteCountryResponse>.Failure("Country not found", "COUNTRY-001"));

            // Act
            var result = await _controller.DeleteCountryAsync(request.CountryCode);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            var apiResponse = notFoundResult.Value as ApiResponse;
            apiResponse.Should().NotBeNull();
            apiResponse.Success.Should().BeFalse();
            apiResponse.Message.Should().Be("Country not found");
        }
    }
}