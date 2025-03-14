using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Security.Claims; // System.Security.Claims package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using Microsoft.AspNetCore.Http; // Microsoft.AspNetCore.Http package version 6.0.0
using Microsoft.AspNetCore.Mvc; // Microsoft.AspNetCore.Mvc package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using Moq; // Moq package version 4.18.2
using VatFilingPricingTool.Api.Controllers; // Import PricingController class
using VatFilingPricingTool.Api.Models.Requests; // Import CalculationRequest, GetCalculationRequest, SaveCalculationRequest, GetCalculationHistoryRequest, CompareCalculationsRequest, DeleteCalculationRequest classes
using VatFilingPricingTool.Api.Models.Responses; // Import CalculationResponse, CalculationHistoryResponse, SaveCalculationResponse, CompareCalculationsResponse, DeleteCalculationResponse classes
using VatFilingPricingTool.Common.Models; // Import Result and PagedList classes
using VatFilingPricingTool.Domain.Enums; // Import ServiceType and FilingFrequency enums
using VatFilingPricingTool.Service.Interfaces; // Import IPricingService interface
using VatFilingPricingTool.UnitTests.Helpers; // Import MockData and TestHelpers classes
using Xunit; // Xunit package version 2.4.1
using FluentAssertions; // FluentAssertions package version 6.7.0

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Contains unit tests for the PricingController class
    /// </summary>
    public class PricingControllerTests
    {
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly Mock<ILogger<PricingController>> _mockLogger;
        private readonly PricingController _controller;

        /// <summary>
        /// Initializes a new instance of the PricingControllerTests class with mocked dependencies
        /// </summary>
        public PricingControllerTests()
        {
            _mockPricingService = new Mock<IPricingService>();
            _mockLogger = new Mock<ILogger<PricingController>>();
            _controller = new PricingController(_mockPricingService.Object, _mockLogger.Object);
        }

        /// <summary>
        /// Tests that CalculatePricingAsync returns a successful result with valid input
        /// </summary>
        [Fact]
        public async Task CalculatePricingAsync_WithValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid CalculationRequest with ServiceType.StandardFiling, 500 transactions, Quarterly frequency, and countries GB and DE
            var request = CreateValidCalculationRequest();

            // Arrange: Create a mock calculation response with total cost and country breakdowns
            var calculationResponse = new CalculationResponse
            {
                CalculationId = TestHelpers.GetRandomCalculationId(),
                TotalCost = 2750m,
                CurrencyCode = "EUR",
                CountryBreakdowns = new List<CountryBreakdownResponse>
                {
                    new CountryBreakdownResponse { CountryCode = "GB", TotalCost = 1500m },
                    new CountryBreakdownResponse { CountryCode = "DE", TotalCost = 1250m }
                }
            };

            // Arrange: Setup _mockPricingService to return a successful Result with the mock calculation response
            _mockPricingService.Setup(x => x.CalculatePricingAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CalculateRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Success(calculationResponse));

            // Act: Call _controller.CalculatePricingAsync with the request
            var result = await _controller.CalculatePricingAsync(request);

            // Assert: Verify the result is an OkObjectResult
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is a CalculationResponse with expected properties
            Assert.IsType<CalculationResponse>(okResult.Value);
            var response = okResult.Value as CalculationResponse;
            Assert.Equal(calculationResponse.CalculationId, response.CalculationId);
            Assert.Equal(calculationResponse.TotalCost, response.TotalCost);
            Assert.Equal(calculationResponse.CurrencyCode, response.CurrencyCode);
            Assert.Equal(calculationResponse.CountryBreakdowns.Count, response.CountryBreakdowns.Count);

            // Assert: Verify _mockPricingService.CalculatePricingAsync was called once with the correct parameters
            _mockPricingService.Verify(x => x.CalculatePricingAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CalculateRequest>()), Times.Once);
        }

        /// <summary>
        /// Tests that CalculatePricingAsync returns a bad request result with invalid input
        /// </summary>
        [Fact]
        public async Task CalculatePricingAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid CalculationRequest with missing required fields
            var request = new CalculationRequest();
            _controller.ModelState.AddModelError("ServiceType", "Service type is required");

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.CalculatePricingAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CalculateRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Failure("Invalid request", "PRICING-002"));

            // Act: Call _controller.CalculatePricingAsync with the invalid request
            var result = await _controller.CalculatePricingAsync(request);

            // Assert: Verify the result is a BadRequestObjectResult
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result contains the expected error message
            Assert.IsType<ProblemDetails>(badRequestResult.Value);
            var problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.Equal("Request validation failed", problemDetails.Title);
        }

        /// <summary>
        /// Tests that GetCalculationAsync returns a successful result with a valid calculation ID
        /// </summary>
        [Fact]
        public async Task GetCalculationAsync_WithValidId_ReturnsOkResult()
        {
            // Arrange: Create a GetCalculationRequest with a valid calculation ID
            var calculationId = TestHelpers.GetRandomCalculationId();
            var request = new GetCalculationRequest { CalculationId = calculationId };

            // Arrange: Create a mock calculation response
            var calculationResponse = new CalculationResponse
            {
                CalculationId = calculationId,
                TotalCost = 2750m,
                CurrencyCode = "EUR",
                CountryBreakdowns = new List<CountryBreakdownResponse>
                {
                    new CountryBreakdownResponse { CountryCode = "GB", TotalCost = 1500m },
                    new CountryBreakdownResponse { CountryCode = "DE", TotalCost = 1250m }
                }
            };

            // Arrange: Setup _mockPricingService to return a successful Result with the mock calculation
            _mockPricingService.Setup(x => x.GetCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Success(calculationResponse));

            // Act: Call _controller.GetCalculationAsync with the request
            var result = await _controller.GetCalculationAsync(request);

            // Assert: Verify the result is an OkObjectResult
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is a CalculationResponse with expected properties
            Assert.IsType<CalculationResponse>(okResult.Value);
            var response = okResult.Value as CalculationResponse;
            Assert.Equal(calculationResponse.CalculationId, response.CalculationId);
            Assert.Equal(calculationResponse.TotalCost, response.TotalCost);
            Assert.Equal(calculationResponse.CurrencyCode, response.CurrencyCode);
            Assert.Equal(calculationResponse.CountryBreakdowns.Count, response.CountryBreakdowns.Count);

            // Assert: Verify _mockPricingService.GetCalculationAsync was called once with the correct parameters
            _mockPricingService.Verify(x => x.GetCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationRequest>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetCalculationAsync returns a not found result with an invalid calculation ID
        /// </summary>
        [Fact]
        public async Task GetCalculationAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange: Create a GetCalculationRequest with an invalid calculation ID
            var calculationId = TestHelpers.GetRandomCalculationId();
            var request = new GetCalculationRequest { CalculationId = calculationId };

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.GetCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Failure("Calculation not found", "PRICING-009"));

            // Act: Call _controller.GetCalculationAsync with the request
            var result = await _controller.GetCalculationAsync(request);

            // Assert: Verify the result is a NotFoundResult
            Assert.IsType<NotFoundResult>(result);

            // Assert: Verify _mockPricingService.GetCalculationAsync was called once with the correct parameters
            _mockPricingService.Verify(x => x.GetCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationRequest>()), Times.Once);
        }

        /// <summary>
        /// Tests that SaveCalculationAsync returns a created result with valid input
        /// </summary>
        [Fact]
        public async Task SaveCalculationAsync_WithValidRequest_ReturnsCreatedResult()
        {
            // Arrange: Create a valid SaveCalculationRequest with service type, transaction volume, and country breakdowns
            var request = CreateValidSaveCalculationRequest();

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Create a mock save calculation response with calculation ID and date
            var saveCalculationResponse = new SaveCalculationResponse
            {
                CalculationId = TestHelpers.GetRandomCalculationId(),
                CalculationDate = DateTime.UtcNow
            };

            // Arrange: Setup _mockPricingService to return a successful Result with the mock response
            _mockPricingService.Setup(x => x.SaveCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.SaveCalculationRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result<SaveCalculationResponse>.Success(saveCalculationResponse));

            // Act: Call _controller.SaveCalculationAsync with the request
            var result = await _controller.SaveCalculationAsync(request);

            // Assert: Verify the result is a CreatedResult
            Assert.IsType<CreatedResult>(result);
            var createdResult = result as CreatedResult;

            // Assert: Verify the result value is a SaveCalculationResponse with expected properties
            Assert.IsType<SaveCalculationResponse>(createdResult.Value);
            var response = createdResult.Value as SaveCalculationResponse;
            Assert.Equal(saveCalculationResponse.CalculationId, response.CalculationId);
            Assert.Equal(saveCalculationResponse.CalculationDate, response.CalculationDate);

            // Assert: Verify _mockPricingService.SaveCalculationAsync was called once with the correct parameters and user ID
            _mockPricingService.Verify(x => x.SaveCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.SaveCalculationRequest>(), userId), Times.Once);
        }

        /// <summary>
        /// Tests that SaveCalculationAsync returns a bad request result with invalid input
        /// </summary>
        [Fact]
        public async Task SaveCalculationAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid SaveCalculationRequest with missing required fields
            var request = new SaveCalculationRequest();
            _controller.ModelState.AddModelError("ServiceType", "Service type is required");

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.SaveCalculationAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.SaveCalculationRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result<SaveCalculationResponse>.Failure("Invalid request", "PRICING-002"));

            // Act: Call _controller.SaveCalculationAsync with the invalid request
            var result = await _controller.SaveCalculationAsync(request);

            // Assert: Verify the result is a BadRequestObjectResult
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result contains the expected error message
            Assert.IsType<ProblemDetails>(badRequestResult.Value);
            var problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.Equal("Request validation failed", problemDetails.Title);
        }

        /// <summary>
        /// Tests that GetCalculationHistoryAsync returns a successful result with valid input
        /// </summary>
        [Fact]
        public async Task GetCalculationHistoryAsync_WithValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid GetCalculationHistoryRequest with date range, page, and page size
            var request = new GetCalculationHistoryRequest
            {
                StartDate = DateTime.Now.AddDays(-30),
                EndDate = DateTime.Now,
                Page = 1,
                PageSize = 10
            };

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Create a mock paged list of calculation summaries
            var calculationSummaries = new List<VatFilingPricingTool.Contracts.V1.Models.CalculationModel>
            {
                new VatFilingPricingTool.Contracts.V1.Models.CalculationModel { CalculationId = TestHelpers.GetRandomCalculationId(), TotalCost = 1000, CurrencyCode = "EUR" },
                new VatFilingPricingTool.Contracts.V1.Models.CalculationModel { CalculationId = TestHelpers.GetRandomCalculationId(), TotalCost = 2000, CurrencyCode = "USD" }
            };
            var pagedList = PagedList<VatFilingPricingTool.Contracts.V1.Models.CalculationModel>.Create(calculationSummaries, 2, 1, 10);

            // Arrange: Setup _mockPricingService to return a successful Result with the mock paged list
            _mockPricingService.Setup(x => x.GetCalculationHistoryAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationHistoryRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result<CalculationHistoryResponse>.Success(CalculationHistoryResponse.FromPagedList(pagedList)));

            // Act: Call _controller.GetCalculationHistoryAsync with the request
            var result = await _controller.GetCalculationHistoryAsync(request);

            // Assert: Verify the result is an OkObjectResult
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is a CalculationHistoryResponse with expected properties
            Assert.IsType<CalculationHistoryResponse>(okResult.Value);
            var response = okResult.Value as CalculationHistoryResponse;
            Assert.Equal(pagedList.PageNumber, response.PageNumber);
            Assert.Equal(pagedList.PageSize, response.PageSize);
            Assert.Equal(pagedList.TotalCount, response.TotalCount);

            // Assert: Verify _mockPricingService.GetCalculationHistoryAsync was called once with the correct parameters and user ID
            _mockPricingService.Verify(x => x.GetCalculationHistoryAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationHistoryRequest>(), userId), Times.Once);
        }

        /// <summary>
        /// Tests that GetCalculationHistoryAsync returns a bad request result with invalid input
        /// </summary>
        [Fact]
        public async Task GetCalculationHistoryAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid GetCalculationHistoryRequest with invalid parameters
            var request = new GetCalculationHistoryRequest { Page = -1, PageSize = 0 };
            _controller.ModelState.AddModelError("Page", "Page must be greater than 0");
            _controller.ModelState.AddModelError("PageSize", "Page size must be between 1 and 100");

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.GetCalculationHistoryAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.GetCalculationHistoryRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result<CalculationHistoryResponse>.Failure("Invalid request", "PRICING-002"));

            // Act: Call _controller.GetCalculationHistoryAsync with the invalid request
            var result = await _controller.GetCalculationHistoryAsync(request);

            // Assert: Verify the result is a BadRequestObjectResult
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result contains the expected error message
            Assert.IsType<ProblemDetails>(badRequestResult.Value);
            var problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.Equal("Request validation failed", problemDetails.Title);
        }

        /// <summary>
        /// Tests that CompareCalculationsAsync returns a successful result with valid input
        /// </summary>
        [Fact]
        public async Task CompareCalculationsAsync_WithValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid CompareCalculationsRequest with multiple calculation scenarios
            var request = CreateValidCompareCalculationsRequest();

            // Arrange: Create a mock comparison response with scenarios and cost comparisons
            var compareCalculationsResponse = new CompareCalculationsResponse
            {
                Scenarios = new List<CalculationScenarioResponse>
                {
                    new CalculationScenarioResponse { ScenarioId = "1", TotalCost = 1000 },
                    new CalculationScenarioResponse { ScenarioId = "2", TotalCost = 1200 }
                },
                TotalCostComparison = new Dictionary<string, decimal> { { "1", 1000 }, { "2", 1200 } },
                CountryCostComparison = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { "GB", new Dictionary<string, decimal> { { "1", 500 }, { "2", 600 } } },
                    { "DE", new Dictionary<string, decimal> { { "1", 500 }, { "2", 600 } } }
                }
            };

            // Arrange: Setup _mockPricingService to return a successful Result with the mock comparison
            _mockPricingService.Setup(x => x.CompareCalculationsAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CompareCalculationsRequest>()))
                .ReturnsAsync(Result<CalculationComparisonResponse>.Success(compareCalculationsResponse));

            // Act: Call _controller.CompareCalculationsAsync with the request
            var result = await _controller.CompareCalculationsAsync(request);

            // Assert: Verify the result is an OkObjectResult
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is a CompareCalculationsResponse with expected properties
            Assert.IsType<CompareCalculationsResponse>(okResult.Value);
            var response = okResult.Value as CompareCalculationsResponse;
            Assert.Equal(compareCalculationsResponse.Scenarios.Count, response.Scenarios.Count);
            Assert.Equal(compareCalculationsResponse.TotalCostComparison.Count, response.TotalCostComparison.Count);
            Assert.Equal(compareCalculationsResponse.CountryCostComparison.Count, response.CountryCostComparison.Count);

            // Assert: Verify _mockPricingService.CompareCalculationsAsync was called once with the correct parameters
            _mockPricingService.Verify(x => x.CompareCalculationsAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CompareCalculationsRequest>()), Times.Once);
        }

        /// <summary>
        /// Tests that CompareCalculationsAsync returns a bad request result with invalid input
        /// </summary>
        [Fact]
        public async Task CompareCalculationsAsync_WithInvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid CompareCalculationsRequest with less than two scenarios
            var request = new CompareCalculationsRequest { Scenarios = new List<CalculationScenario>() };

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.CompareCalculationsAsync(It.IsAny<VatFilingPricingTool.Contracts.V1.Requests.CompareCalculationsRequest>()))
                .ReturnsAsync(Result<CalculationComparisonResponse>.Failure("Invalid request", "PRICING-002"));

            // Act: Call _controller.CompareCalculationsAsync with the invalid request
            var result = await _controller.CompareCalculationsAsync(request);

            // Assert: Verify the result is a BadRequestObjectResult
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the result contains the expected error message
            Assert.IsType<ProblemDetails>(badRequestResult.Value);
            var problemDetails = badRequestResult.Value as ProblemDetails;
            Assert.Equal("Invalid request", problemDetails.Title);
        }

        /// <summary>
        /// Tests that DeleteCalculationAsync returns a successful result with a valid calculation ID
        /// </summary>
        [Fact]
        public async Task DeleteCalculationAsync_WithValidId_ReturnsOkResult()
        {
            // Arrange: Create a DeleteCalculationRequest with a valid calculation ID
            var calculationId = TestHelpers.GetRandomCalculationId();
            var request = new DeleteCalculationRequest { CalculationId = calculationId };

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Setup _mockPricingService to return a successful Result
            _mockPricingService.Setup(x => x.DeleteCalculationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success());

            // Act: Call _controller.DeleteCalculationAsync with the request
            var result = await _controller.DeleteCalculationAsync(request);

            // Assert: Verify the result is an OkObjectResult
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is a DeleteCalculationResponse with expected properties
            Assert.IsType<DeleteCalculationResponse>(okResult.Value);
            var response = okResult.Value as DeleteCalculationResponse;
            Assert.Equal(calculationId, response.CalculationId);
            Assert.True(response.Deleted);

            // Assert: Verify _mockPricingService.DeleteCalculationAsync was called once with the correct parameters and user ID
            _mockPricingService.Verify(x => x.DeleteCalculationAsync(calculationId, userId), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteCalculationAsync returns a not found result with an invalid calculation ID
        /// </summary>
        [Fact]
        public async Task DeleteCalculationAsync_WithInvalidId_ReturnsNotFound()
        {
            // Arrange: Create a DeleteCalculationRequest with an invalid calculation ID
            var calculationId = TestHelpers.GetRandomCalculationId();
            var request = new DeleteCalculationRequest { CalculationId = calculationId };

            // Arrange: Setup controller context with a user ID
            var userId = TestHelpers.GetRandomUserId();
            TestHelpers.SetupControllerContext(_controller, userId);

            // Arrange: Setup _mockPricingService to return a failure Result with an error message
            _mockPricingService.Setup(x => x.DeleteCalculationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure("Calculation not found", "PRICING-009"));

            // Act: Call _controller.DeleteCalculationAsync with the request
            var result = await _controller.DeleteCalculationAsync(request);

            // Assert: Verify the result is a NotFoundResult
            Assert.IsType<NotFoundResult>(result);

            // Assert: Verify _mockPricingService.DeleteCalculationAsync was called once with the correct parameters and user ID
            _mockPricingService.Verify(x => x.DeleteCalculationAsync(calculationId, userId), Times.Once);
        }

        /// <summary>
        /// Helper method to create a valid calculation request for testing
        /// </summary>
        /// <returns>A valid calculation request</returns>
        private CalculationRequest CreateValidCalculationRequest()
        {
            var request = new CalculationRequest();
            request.ServiceType = ServiceType.StandardFiling;
            request.TransactionVolume = 500;
            request.Frequency = FilingFrequency.Quarterly;
            request.CountryCodes = new List<string> { "GB", "DE" };
            return request;
        }

        /// <summary>
        /// Helper method to create a valid save calculation request for testing
        /// </summary>
        /// <returns>A valid save calculation request</returns>
        private SaveCalculationRequest CreateValidSaveCalculationRequest()
        {
            var request = new SaveCalculationRequest();
            request.ServiceType = ServiceType.StandardFiling;
            request.TransactionVolume = 500;
            request.Frequency = FilingFrequency.Quarterly;
            request.TotalCost = 2750m;
            request.CurrencyCode = "EUR";
            request.CountryBreakdowns = new List<CountryBreakdownRequest>
            {
                new CountryBreakdownRequest { CountryCode = "GB", CountryName = "United Kingdom", BaseCost = 1200m, AdditionalCost = 300m, TotalCost = 1500m },
                new CountryBreakdownRequest { CountryCode = "DE", CountryName = "Germany", BaseCost = 1000m, AdditionalCost = 250m, TotalCost = 1250m }
            };
            return request;
        }

        /// <summary>
        /// Helper method to create a valid compare calculations request for testing
        /// </summary>
        /// <returns>A valid compare calculations request</returns>
        private CompareCalculationsRequest CreateValidCompareCalculationsRequest()
        {
            var request = new CompareCalculationsRequest();
            request.Scenarios = new List<CalculationScenario>
            {
                new CalculationScenario
                {
                    ScenarioId = "1",
                    ScenarioName = "Standard Filing",
                    ServiceType = ServiceType.StandardFiling,
                    TransactionVolume = 500,
                    Frequency = FilingFrequency.Quarterly,
                    CountryCodes = new List<string> { "GB", "DE" }
                },
                new CalculationScenario
                {
                    ScenarioId = "2",
                    ScenarioName = "Complex Filing",
                    ServiceType = ServiceType.ComplexFiling,
                    TransactionVolume = 500,
                    Frequency = FilingFrequency.Quarterly,
                    CountryCodes = new List<string> { "GB", "DE" }
                }
            };
            return request;
        }
    }
}