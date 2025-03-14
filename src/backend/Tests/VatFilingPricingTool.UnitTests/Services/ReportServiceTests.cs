#nullable enable
using System; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.IO; // Version: 6.0.0
using System.Linq; // Version: 6.0.0
using System.Threading.Tasks; // Version: 6.0.0
using FluentAssertions; // Version: 6.7.0
using Microsoft.Extensions.Logging; // Version: 6.0.0
using Moq; // Version: 4.18.2
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Infrastructure.Integration.Email;
using VatFilingPricingTool.Infrastructure.Storage;
using VatFilingPricingTool.Service.Implementations;
using VatFilingPricingTool.UnitTests.Helpers;
using Xunit; // Version: 2.4.2

namespace VatFilingPricingTool.UnitTests.Services
{
    /// <summary>
    /// Test class for the ReportService implementation
    /// </summary>
    public class ReportServiceTests
    {
        private readonly Mock<ILogger<ReportService>> _mockLogger;
        private readonly Mock<IReportRepository> _mockReportRepository;
        private readonly Mock<IPricingService> _mockPricingService;
        private readonly Mock<BlobStorageClient> _mockBlobStorageClient;
        private readonly Mock<IEmailSender> _mockEmailSender;
        private readonly IReportService _reportService;
        private readonly string _testUserId;

        /// <summary>
        /// Initializes a new instance of the ReportServiceTests class
        /// </summary>
        public ReportServiceTests()
        {
            _mockLogger = new Mock<ILogger<ReportService>>();
            _mockReportRepository = new Mock<IReportRepository>();
            _mockPricingService = new Mock<IPricingService>();
            _mockBlobStorageClient = new Mock<BlobStorageClient>();
            _mockEmailSender = new Mock<IEmailSender>();

            _reportService = new ReportService(
                _mockLogger.Object,
                _mockReportRepository.Object,
                _mockPricingService.Object,
                _mockBlobStorageClient.Object,
                _mockEmailSender.Object);

            _testUserId = "customer-id";
        }

        /// <summary>
        /// Tests that GenerateReportAsync returns a success result when given a valid request
        /// </summary>
        [Fact]
        public async Task GenerateReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new GenerateReportRequest
            {
                CalculationId = "calculation-id",
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF
            };

            _mockPricingService.Setup(x => x.GetCalculationAsync(It.IsAny<GetCalculationRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Success(new CalculationResponse()));

            _mockBlobStorageClient.Setup(x => x.UploadBlobAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<string>.Success("https://storage.example.com/report.pdf"));

            _mockBlobStorageClient.Setup(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .Returns(Result<string>.Success("https://storage.example.com/report.pdf?sas"));

            _mockReportRepository.Setup(x => x.AddAsync(It.IsAny<Report>()))
                .ReturnsAsync(new Report());

            // Act
            var result = await _reportService.GenerateReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ReportId.Should().NotBeNullOrEmpty();
            result.Value.DownloadUrl.Should().NotBeNullOrEmpty();

            _mockPricingService.Verify(x => x.GetCalculationAsync(It.Is<GetCalculationRequest>(r => r.CalculationId == "calculation-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.UploadBlobAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockReportRepository.Verify(x => x.AddAsync(It.IsAny<Report>()), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
        }

        /// <summary>
        /// Tests that GenerateReportAsync returns a failure result when the calculation ID is invalid
        /// </summary>
        [Fact]
        public async Task GenerateReportAsync_WithInvalidCalculationId_ReturnsFailureResult()
        {
            // Arrange
            var request = new GenerateReportRequest
            {
                CalculationId = "invalid-calculation-id",
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF
            };

            _mockPricingService.Setup(x => x.GetCalculationAsync(It.IsAny<GetCalculationRequest>()))
                .ReturnsAsync(Result<CalculationResponse>.Failure("Calculation not found", "PRICING-004"));

            // Act
            var result = await _reportService.GenerateReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Calculation not found: invalid-calculation-id");
            result.ErrorCode.Should().Be("PRICING-004");

            _mockPricingService.Verify(x => x.GetCalculationAsync(It.Is<GetCalculationRequest>(r => r.CalculationId == "invalid-calculation-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.UploadBlobAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockReportRepository.Verify(x => x.AddAsync(It.IsAny<Report>()), Times.Never);
        }

        /// <summary>
        /// Tests that GetReportAsync returns a success result when given a valid request
        /// </summary>
        [Fact]
        public async Task GetReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new GetReportRequest { ReportId = "report-id" };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockBlobStorageClient.Setup(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .Returns(Result<string>.Success("https://storage.example.com/report.pdf?sas"));

            // Act
            var result = await _reportService.GetReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ReportId.Should().Be("report-id");
            result.Value.DownloadUrl.Should().NotBeNullOrEmpty();

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetReportAsync returns a failure result when the report ID is invalid
        /// </summary>
        [Fact]
        public async Task GetReportAsync_WithInvalidReportId_ReturnsFailureResult()
        {
            // Arrange
            var request = new GetReportRequest { ReportId = "invalid-report-id" };

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync((Report)null);

            // Act
            var result = await _reportService.GetReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Report not found with ReportId: invalid-report-id");
            result.ErrorCode.Should().Be("REPORT-001");

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "invalid-report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        /// <summary>
        /// Tests that GetReportAsync returns a failure result when the user is not authorized to access the report
        /// </summary>
        [Fact]
        public async Task GetReportAsync_WithUnauthorizedUser_ReturnsFailureResult()
        {
            // Arrange
            var request = new GetReportRequest { ReportId = "report-id" };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = "different-user-id";

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            // Act
            var result = await _reportService.GetReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Unauthorized access");
            result.ErrorCode.Should().Be("GENERAL-005");

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
        }

        /// <summary>
        /// Tests that GetReportHistoryAsync returns a success result with paginated reports
        /// </summary>
        [Fact]
        public async Task GetReportHistoryAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new GetReportHistoryRequest { Page = 1, PageSize = 10 };
            var mockReports = MockData.GetMockReports();

            _mockReportRepository.Setup(x => x.GetPagedByUserIdAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(PagedList<Report>.Create(mockReports, 1, 10));

            _mockBlobStorageClient.Setup(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()))
                .Returns(Result<string>.Success("https://storage.example.com/report.pdf?sas"));

            // Act
            var result = await _reportService.GetReportHistoryAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Reports.Count.Should().Be(3);
            result.Value.PageNumber.Should().Be(1);
            result.Value.PageSize.Should().Be(10);

            _mockReportRepository.Verify(x => x.GetPagedByUserIdAsync(It.Is<string>(id => id == _testUserId), It.Is<int>(p => p == 1), It.Is<int>(s => s == 10)), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GenerateSasUrl(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Exactly(3));
        }

        /// <summary>
        /// Tests that DownloadReportAsync returns a success result with the report content
        /// </summary>
        [Fact]
        public async Task DownloadReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new DownloadReportRequest { ReportId = "report-id", Format = ReportFormat.PDF };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockBlobStorageClient.Setup(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<byte[]>.Success(new byte[] { 0x01, 0x02, 0x03 }));

            // Act
            var result = await _reportService.DownloadReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ReportId.Should().Be("report-id");
            result.Value.FileContent.Should().BeEquivalentTo(new byte[] { 0x01, 0x02, 0x03 });
            result.Value.ContentType.Should().Be("application/pdf");

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that DownloadReportAsync converts the report format when requested format differs from original
        /// </summary>
        [Fact]
        public async Task DownloadReportAsync_WithDifferentFormat_ConvertsProperly()
        {
            // Arrange
            var request = new DownloadReportRequest { ReportId = "report-id", Format = ReportFormat.Excel };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;
            mockReport.Format = ReportFormat.PDF;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockBlobStorageClient.Setup(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<byte[]>.Success(new byte[] { 0x01, 0x02, 0x03 }));

            // Act
            var result = await _reportService.DownloadReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.ReportId.Should().Be("report-id");
            result.Value.FileContent.Should().BeEquivalentTo(new byte[] { 0x01, 0x02, 0x03 });
            result.Value.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        /// <summary>
        /// Tests that EmailReportAsync returns a success result when the email is sent successfully
        /// </summary>
        [Fact]
        public async Task EmailReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new EmailReportRequest { ReportId = "report-id", EmailAddress = "test@example.com", Subject = "Test Subject", Message = "Test Message" };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockBlobStorageClient.Setup(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result<byte[]>.Success(new byte[] { 0x01, 0x02, 0x03 }));

            _mockEmailSender.Setup(x => x.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _reportService.EmailReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.EmailSent.Should().BeTrue();

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockBlobStorageClient.Verify(x => x.GetBlobAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _mockEmailSender.Verify(x => x.SendTemplatedEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
        }

        /// <summary>
        /// Tests that ArchiveReportAsync returns a success result when the report is archived successfully
        /// </summary>
        [Fact]
        public async Task ArchiveReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            var request = new ArchiveReportRequest { ReportId = "report-id" };
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockReportRepository.Setup(x => x.ArchiveReportAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _reportService.ArchiveReportAsync(request, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsArchived.Should().BeTrue();

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == "report-id")), Times.Once);
            _mockReportRepository.Verify(x => x.ArchiveReportAsync(It.Is<string>(id => id == "report-id")), Times.Once);
        }

        /// <summary>
        /// Tests that UnarchiveReportAsync returns a success result when the report is unarchived successfully
        /// </summary>
        [Fact]
        public async Task UnarchiveReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            string reportId = "report-id";
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockReportRepository.Setup(x => x.UnarchiveReportAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _reportService.UnarchiveReportAsync(reportId, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.IsArchived.Should().BeFalse();

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == reportId)), Times.Once);
            _mockReportRepository.Verify(x => x.UnarchiveReportAsync(It.Is<string>(id => id == reportId)), Times.Once);
        }

        /// <summary>
        /// Tests that DeleteReportAsync returns a success result when the report is deleted successfully
        /// </summary>
        [Fact]
        public async Task DeleteReportAsync_WithValidRequest_ReturnsSuccessResult()
        {
            // Arrange
            string reportId = "report-id";
            var mockReport = MockData.GetMockReports().First();
            mockReport.UserId = _testUserId;
            mockReport.StorageUrl = "https://example.com/report.pdf";

            _mockReportRepository.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<string>()))
                .ReturnsAsync(mockReport);

            _mockBlobStorageClient.Setup(x => x.DeleteBlobAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success());

            _mockReportRepository.Setup(x => x.DeleteAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _reportService.DeleteReportAsync(reportId, _testUserId);

            // Assert
            result.IsSuccess.Should().BeTrue();

            _mockReportRepository.Verify(x => x.GetByIdWithDetailsAsync(It.Is<string>(id => id == reportId)), Times.Once);
            _mockBlobStorageClient.Verify(x => x.DeleteBlobAsync(It.Is<string>(blobName => blobName == "report.pdf"), It.IsAny<string>()), Times.Once);
            _mockReportRepository.Verify(x => x.DeleteAsync(It.Is<string>(id => id == reportId)), Times.Once);
        }
    }
}
#nullable restore