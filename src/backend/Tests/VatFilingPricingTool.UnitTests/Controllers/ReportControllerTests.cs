#region

using System; // Version: 6.0.0 - Core .NET functionality
using System.Collections.Generic; // Version: 6.0.0 - For collection types like List
using System.Linq; // Version: 6.0.0 - For LINQ queries
using System.Security.Claims; // Version: 6.0.0 - For working with claims-based identity
using System.Threading.Tasks; // Version: 6.0.0 - For Task-based asynchronous operations
using Microsoft.AspNetCore.Http; // Version: 6.0.0 - For HTTP status codes and context
using Microsoft.AspNetCore.Mvc; // Version: 6.0.0 - For controller testing and action result types
using Microsoft.Extensions.Logging; // Version: 6.0.0 - For logging interfaces
using Moq; // Version: 4.18.2 - For creating and configuring mock objects
using VatFilingPricingTool.Api.Controllers; // The controller class being tested
using VatFilingPricingTool.Common.Models; // Result pattern for operation outcomes
using VatFilingPricingTool.Contracts.V1.Requests; // Request model for generating a report
using VatFilingPricingTool.Contracts.V1.Responses; // Response model for report generation
using VatFilingPricingTool.Domain.Enums; // Enum for report formats
using VatFilingPricingTool.Service.Interfaces; // Interface for the report service that will be mocked in tests
using VatFilingPricingTool.UnitTests.Helpers; // Provides helper methods for testing
using Xunit; // Testing framework
using FluentAssertions; // For more readable assertions

#endregion

namespace VatFilingPricingTool.UnitTests.Controllers
{
    /// <summary>
    /// Contains unit tests for the ReportController class
    /// </summary>
    public class ReportControllerTests
    {
        private readonly Mock<IReportService> _mockReportService;
        private readonly Mock<ILogger<ReportController>> _mockLogger;
        private readonly ReportController _controller;

        /// <summary>
        /// Initializes a new instance of the ReportControllerTests class with mocked dependencies
        /// </summary>
        public ReportControllerTests()
        {
            // Arrange
            _mockReportService = new Mock<IReportService>();
            _mockLogger = new Mock<ILogger<ReportController>>();
            _controller = new ReportController(_mockReportService.Object, _mockLogger.Object);

            // Set up controller context with a test user ID
            TestHelpers.SetupControllerContext(_controller, "test-user-id");
        }

        /// <summary>
        /// Tests that GenerateReportAsync returns OK result with valid request
        /// </summary>
        [Fact]
        public async Task GenerateReportAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid GenerateReportRequest with test data
            var request = new GenerateReportRequest
            {
                CalculationId = "test-calculation-id",
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true
            };

            // Arrange: Set up _mockReportService to return a successful result with a GenerateReportResponse
            var response = new GenerateReportResponse { ReportId = "test-report-id", DownloadUrl = "http://example.com/report.pdf" };
            _mockReportService.Setup(s => s.GenerateReportAsync(request, "test-user-id"))
                .ReturnsAsync(Result<GenerateReportResponse>.Success(response));

            // Act: Call _controller.GenerateReportAsync with the request
            var result = await _controller.GenerateReportAsync(request);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is GenerateReportResponse with expected values
            okResult.Value.Should().BeOfType<GenerateReportResponse>();
            var reportResponse = okResult.Value as GenerateReportResponse;
            reportResponse.ReportId.Should().Be("test-report-id");
            reportResponse.DownloadUrl.Should().Be("http://example.com/report.pdf");

            // Assert: Verify _mockReportService.GenerateReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.GenerateReportAsync(request, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateReportAsync returns BadRequest with invalid request
        /// </summary>
        [Fact]
        public async Task GenerateReportAsync_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid GenerateReportRequest (null or missing required fields)
            var request = new GenerateReportRequest { ReportTitle = null };

            // Arrange: Add model errors to controller ModelState
            _controller.ModelState.AddModelError("ReportTitle", "ReportTitle is required");

            // Act: Call _controller.GenerateReportAsync with the invalid request
            var result = await _controller.GenerateReportAsync(request);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.GenerateReportAsync was not called
            _mockReportService.Verify(s => s.GenerateReportAsync(It.IsAny<GenerateReportRequest>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that GenerateReportAsync returns appropriate error response when service fails
        /// </summary>
        [Fact]
        public async Task GenerateReportAsync_ServiceFailure_ReturnsAppropriateErrorResponse()
        {
            // Arrange: Create a valid GenerateReportRequest with test data
            var request = new GenerateReportRequest
            {
                CalculationId = "test-calculation-id",
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true
            };

            // Arrange: Set up _mockReportService to return a failure result with error message
            _mockReportService.Setup(s => s.GenerateReportAsync(request, "test-user-id"))
                .ReturnsAsync(Result<GenerateReportResponse>.Failure("Report generation failed", "REPORT-002"));

            // Act: Call _controller.GenerateReportAsync with the request
            var result = await _controller.GenerateReportAsync(request);

            // Assert: Verify the result is appropriate error result (BadRequest or NotFound)
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;

            // Assert: Verify the error message in the response matches the expected error
            dynamic value = badRequestResult.Value;
            Assert.Equal("Report generation failed", value.message);

            // Assert: Verify _mockReportService.GenerateReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.GenerateReportAsync(request, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GetReportAsync returns OK result with valid report ID
        /// </summary>
        [Fact]
        public async Task GetReportAsync_ValidId_ReturnsOkResult()
        {
            // Arrange: Create a valid report ID
            string reportId = "test-report-id";

            // Arrange: Set up _mockReportService to return a successful result with a GetReportResponse
            var response = new GetReportResponse { ReportId = reportId, DownloadUrl = "http://example.com/report.pdf" };
            _mockReportService.Setup(s => s.GetReportAsync(It.Is<GetReportRequest>(r => r.ReportId == reportId), "test-user-id"))
                .ReturnsAsync(Result<GetReportResponse>.Success(response));

            // Act: Call _controller.GetReportAsync with the report ID
            var result = await _controller.GetReportAsync(reportId);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is GetReportResponse with expected values
            okResult.Value.Should().BeOfType<GetReportResponse>();
            var reportResponse = okResult.Value as GetReportResponse;
            reportResponse.ReportId.Should().Be(reportId);
            reportResponse.DownloadUrl.Should().Be("http://example.com/report.pdf");

            // Assert: Verify _mockReportService.GetReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.GetReportAsync(It.Is<GetReportRequest>(r => r.ReportId == reportId), "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GetReportAsync returns BadRequest with invalid report ID
        /// </summary>
        [Fact]
        public async Task GetReportAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange: Create an invalid report ID (null or empty)
            string reportId = null;

            // Act: Call _controller.GetReportAsync with the invalid ID
            var result = await _controller.GetReportAsync(reportId);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.GetReportAsync was not called
            _mockReportService.Verify(s => s.GetReportAsync(It.IsAny<GetReportRequest>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that GetReportAsync returns NotFound when report doesn't exist
        /// </summary>
        [Fact]
        public async Task GetReportAsync_ReportNotFound_ReturnsNotFound()
        {
            // Arrange: Create a valid but non-existent report ID
            string reportId = "non-existent-report-id";

            // Arrange: Set up _mockReportService to return a failure result with 'not found' error
            _mockReportService.Setup(s => s.GetReportAsync(It.Is<GetReportRequest>(r => r.ReportId == reportId), "test-user-id"))
                .ReturnsAsync(Result<GetReportResponse>.Failure("Report not found", "REPORT-001"));

            // Act: Call _controller.GetReportAsync with the report ID
            var result = await _controller.GetReportAsync(reportId);

            // Assert: Verify the result is NotFoundObjectResult
            result.Should().BeOfType<NotFoundObjectResult>();

            // Assert: Verify _mockReportService.GetReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.GetReportAsync(It.Is<GetReportRequest>(r => r.ReportId == reportId), "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GetReportHistoryAsync returns OK result with valid request
        /// </summary>
        [Fact]
        public async Task GetReportHistoryAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid GetReportHistoryRequest with test data
            var request = new GetReportHistoryRequest { Page = 1, PageSize = 10 };

            // Arrange: Set up _mockReportService to return a successful result with a GetReportHistoryResponse
            var response = new GetReportHistoryResponse { Reports = new List<ReportListItem>(), PageNumber = 1, TotalCount = 0 };
            _mockReportService.Setup(s => s.GetReportHistoryAsync(request, "test-user-id"))
                .ReturnsAsync(Result<GetReportHistoryResponse>.Success(response));

            // Act: Call _controller.GetReportHistoryAsync with the request
            var result = await _controller.GetReportHistoryAsync(request);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is GetReportHistoryResponse with expected values
            okResult.Value.Should().BeOfType<GetReportHistoryResponse>();
            var historyResponse = okResult.Value as GetReportHistoryResponse;
            historyResponse.PageNumber.Should().Be(1);
            historyResponse.TotalCount.Should().Be(0);

            // Assert: Verify _mockReportService.GetReportHistoryAsync was called once with correct parameters
            _mockReportService.Verify(s => s.GetReportHistoryAsync(request, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that GetReportHistoryAsync returns BadRequest with invalid request
        /// </summary>
        [Fact]
        public async Task GetReportHistoryAsync_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid GetReportHistoryRequest (invalid page/pageSize)
            var request = new GetReportHistoryRequest { Page = 0, PageSize = 0 };

            // Arrange: Add model errors to controller ModelState
            _controller.ModelState.AddModelError("Page", "Page must be at least 1");
            _controller.ModelState.AddModelError("PageSize", "PageSize must be between 1 and 100");

            // Act: Call _controller.GetReportHistoryAsync with the invalid request
            var result = await _controller.GetReportHistoryAsync(request);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.GetReportHistoryAsync was not called
            _mockReportService.Verify(s => s.GetReportHistoryAsync(It.IsAny<GetReportHistoryRequest>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that DownloadReportAsync returns FileResult with valid report ID
        /// </summary>
        [Fact]
        public async Task DownloadReportAsync_ValidId_ReturnsFileResult()
        {
            // Arrange: Create a valid report ID
            string reportId = "test-report-id";

            // Arrange: Set up _mockReportService to return a successful result with a DownloadReportResponse containing file data
            var response = new DownloadReportResponse
            {
                ReportId = reportId,
                FileName = "test_report.pdf",
                ContentType = "application/pdf",
                FileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 } // Minimal PDF header
            };
            _mockReportService.Setup(s => s.DownloadReportAsync(It.Is<DownloadReportRequest>(r => r.ReportId == reportId), "test-user-id"))
                .ReturnsAsync(Result<DownloadReportResponse>.Success(response));

            // Act: Call _controller.DownloadReportAsync with the report ID
            var result = await _controller.DownloadReportAsync(reportId);

            // Assert: Verify the result is FileContentResult
            result.Should().BeOfType<FileContentResult>();
            var fileResult = result as FileContentResult;

            // Assert: Verify the file content, content type, and file name match expected values
            fileResult.FileContents.Should().BeEquivalentTo(new byte[] { 0x25, 0x50, 0x44, 0x46 });
            fileResult.ContentType.Should().Be("application/pdf");
            fileResult.FileDownloadName.Should().Be("test_report.pdf");

            // Assert: Verify _mockReportService.DownloadReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.DownloadReportAsync(It.Is<DownloadReportRequest>(r => r.ReportId == reportId), "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that DownloadReportAsync returns BadRequest with invalid report ID
        /// </summary>
        [Fact]
        public async Task DownloadReportAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange: Create an invalid report ID (null or empty)
            string reportId = null;

            // Act: Call _controller.DownloadReportAsync with the invalid ID
            var result = await _controller.DownloadReportAsync(reportId);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.DownloadReportAsync was not called
            _mockReportService.Verify(s => s.DownloadReportAsync(It.IsAny<DownloadReportRequest>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that DownloadReportAsync returns NotFound when report doesn't exist
        /// </summary>
        [Fact]
        public async Task DownloadReportAsync_ReportNotFound_ReturnsNotFound()
        {
            // Arrange: Create a valid but non-existent report ID
            string reportId = "non-existent-report-id";

            // Arrange: Set up _mockReportService to return a failure result with 'not found' error
            _mockReportService.Setup(s => s.DownloadReportAsync(It.Is<DownloadReportRequest>(r => r.ReportId == reportId), "test-user-id"))
                .ReturnsAsync(Result<DownloadReportResponse>.Failure("Report not found", "REPORT-001"));

            // Act: Call _controller.DownloadReportAsync with the report ID
            var result = await _controller.DownloadReportAsync(reportId);

            // Assert: Verify the result is NotFoundObjectResult
            result.Should().BeOfType<NotFoundObjectResult>();

            // Assert: Verify _mockReportService.DownloadReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.DownloadReportAsync(It.Is<DownloadReportRequest>(r => r.ReportId == reportId), "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that EmailReportAsync returns OK result with valid request
        /// </summary>
        [Fact]
        public async Task EmailReportAsync_ValidRequest_ReturnsOkResult()
        {
            // Arrange: Create a valid EmailReportRequest with test data
            var request = new EmailReportRequest
            {
                ReportId = "test-report-id",
                EmailAddress = "test@example.com",
                Subject = "Test Email",
                Message = "This is a test email."
            };

            // Arrange: Set up _mockReportService to return a successful result with an EmailReportResponse
            var response = new EmailReportResponse { ReportId = "test-report-id", EmailAddress = "test@example.com", EmailSent = true };
            _mockReportService.Setup(s => s.EmailReportAsync(request, "test-user-id"))
                .ReturnsAsync(Result<EmailReportResponse>.Success(response));

            // Act: Call _controller.EmailReportAsync with the request
            var result = await _controller.EmailReportAsync(request);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is EmailReportResponse with expected values
            okResult.Value.Should().BeOfType<EmailReportResponse>();
            var emailResponse = okResult.Value as EmailReportResponse;
            emailResponse.ReportId.Should().Be("test-report-id");
            emailResponse.EmailAddress.Should().Be("test@example.com");
            emailResponse.EmailSent.Should().BeTrue();

            // Assert: Verify _mockReportService.EmailReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.EmailReportAsync(request, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that EmailReportAsync returns BadRequest with invalid request
        /// </summary>
        [Fact]
        public async Task EmailReportAsync_InvalidRequest_ReturnsBadRequest()
        {
            // Arrange: Create an invalid EmailReportRequest (missing required fields)
            var request = new EmailReportRequest { ReportId = null };

            // Arrange: Add model errors to controller ModelState
            _controller.ModelState.AddModelError("ReportId", "ReportId is required");

            // Act: Call _controller.EmailReportAsync with the invalid request
            var result = await _controller.EmailReportAsync(request);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.EmailReportAsync was not called
            _mockReportService.Verify(s => s.EmailReportAsync(It.IsAny<EmailReportRequest>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that ArchiveReportAsync returns OK result with valid report ID
        /// </summary>
        [Fact]
        public async Task ArchiveReportAsync_ValidId_ReturnsOkResult()
        {
            // Arrange: Create a valid report ID
            string reportId = "test-report-id";

            // Arrange: Set up _mockReportService to return a successful result with an ArchiveReportResponse
            var response = new ArchiveReportResponse { ReportId = reportId, IsArchived = true };
            _mockReportService.Setup(s => s.ArchiveReportAsync(It.Is<ArchiveReportRequest>(r => r.ReportId == reportId), "test-user-id"))
                .ReturnsAsync(Result<ArchiveReportResponse>.Success(response));

            // Act: Call _controller.ArchiveReportAsync with the report ID
            var result = await _controller.ArchiveReportAsync(reportId);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is ArchiveReportResponse with IsArchived=true
            okResult.Value.Should().BeOfType<ArchiveReportResponse>();
            var archiveResponse = okResult.Value as ArchiveReportResponse;
            archiveResponse.IsArchived.Should().BeTrue();

            // Assert: Verify _mockReportService.ArchiveReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.ArchiveReportAsync(It.Is<ArchiveReportRequest>(r => r.ReportId == reportId), "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that UnarchiveReportAsync returns OK result with valid report ID
        /// </summary>
        [Fact]
        public async Task UnarchiveReportAsync_ValidId_ReturnsOkResult()
        {
            // Arrange: Create a valid report ID
            string reportId = "test-report-id";

            // Arrange: Set up _mockReportService to return a successful result with an ArchiveReportResponse
            var response = new ArchiveReportResponse { ReportId = reportId, IsArchived = false };
            _mockReportService.Setup(s => s.UnarchiveReportAsync(reportId, "test-user-id"))
                .ReturnsAsync(Result<ArchiveReportResponse>.Success(response));

            // Act: Call _controller.UnarchiveReportAsync with the report ID
            var result = await _controller.UnarchiveReportAsync(reportId);

            // Assert: Verify the result is OkObjectResult
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;

            // Assert: Verify the result value is ArchiveReportResponse with IsArchived=false
            okResult.Value.Should().BeOfType<ArchiveReportResponse>();
            var archiveResponse = okResult.Value as ArchiveReportResponse;
            archiveResponse.IsArchived.Should().BeFalse();

            // Assert: Verify _mockReportService.UnarchiveReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.UnarchiveReportAsync(reportId, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteReportAsync returns NoContent with valid report ID
        /// </summary>
        [Fact]
        public async Task DeleteReportAsync_ValidId_ReturnsNoContent()
        {
            // Arrange: Create a valid report ID
            string reportId = "test-report-id";

            // Arrange: Set up _mockReportService to return a successful result
            _mockReportService.Setup(s => s.DeleteReportAsync(reportId, "test-user-id"))
                .ReturnsAsync(Result.Success());

            // Act: Call _controller.DeleteReportAsync with the report ID
            var result = await _controller.DeleteReportAsync(reportId);

            // Assert: Verify the result is NoContentResult
            result.Should().BeOfType<NoContentResult>();

            // Assert: Verify _mockReportService.DeleteReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.DeleteReportAsync(reportId, "test-user-id"), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteReportAsync returns BadRequest with invalid report ID
        /// </summary>
        [Fact]
        public async Task DeleteReportAsync_InvalidId_ReturnsBadRequest()
        {
            // Arrange: Create an invalid report ID (null or empty)
            string reportId = null;

            // Act: Call _controller.DeleteReportAsync with the invalid ID
            var result = await _controller.DeleteReportAsync(reportId);

            // Assert: Verify the result is BadRequestObjectResult
            result.Should().BeOfType<BadRequestObjectResult>();

            // Assert: Verify _mockReportService.DeleteReportAsync was not called
            _mockReportService.Verify(s => s.DeleteReportAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that DeleteReportAsync returns NotFound when report doesn't exist
        /// </summary>
        [Fact]
        public async Task DeleteReportAsync_ReportNotFound_ReturnsNotFound()
        {
            // Arrange: Create a valid but non-existent report ID
            string reportId = "non-existent-report-id";

            // Arrange: Set up _mockReportService to return a failure result with 'not found' error
            _mockReportService.Setup(s => s.DeleteReportAsync(reportId, "test-user-id"))
                .ReturnsAsync(Result.Failure("Report not found", "REPORT-001"));

            // Act: Call _controller.DeleteReportAsync with the report ID
            var result = await _controller.DeleteReportAsync(reportId);

            // Assert: Verify the result is NotFoundObjectResult
            result.Should().BeOfType<NotFoundObjectResult>();

            // Assert: Verify _mockReportService.DeleteReportAsync was called once with correct parameters
            _mockReportService.Verify(s => s.DeleteReportAsync(reportId, "test-user-id"), Times.Once);
        }
    }
}