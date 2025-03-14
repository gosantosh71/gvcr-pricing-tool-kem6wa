using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Implementations;
using VatFilingPricingTool.Web.Services.Interfaces;
using VatFilingPricingTool.Web.Helpers;
using VatFilingPricingTool.Web.Tests.Helpers;
using VatFilingPricingTool.Web.Tests.Mock;

namespace VatFilingPricingTool.Web.Tests.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<ILogger<ReportService>> mockLogger;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;
        private readonly Mock<HttpClientFactory> mockHttpClientFactory;
        private readonly Mock<LocalStorageHelper> mockLocalStorageHelper;
        private readonly ApiClient apiClient;
        private readonly IReportService reportService;

        public ReportServiceTests()
        {
            // Set up mocks
            mockLogger = new Mock<ILogger<ReportService>>();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler) { BaseAddress = new Uri("https://api.example.com") };
            
            mockHttpClientFactory = new Mock<HttpClientFactory>(null, null);
            mockHttpClientFactory.Setup(factory => factory.CreateClient()).Returns(httpClient);
            mockHttpClientFactory.Setup(factory => factory.CreateAuthenticatedClient()).Returns(httpClient);
            mockHttpClientFactory.Setup(factory => factory.CreateResilientAuthenticatedClient()).Returns(httpClient);
            
            mockLocalStorageHelper = new Mock<LocalStorageHelper>(null);
            mockLocalStorageHelper.Setup(h => h.GetAuthTokenAsync()).ReturnsAsync("test-token");
            
            apiClient = new ApiClient(mockHttpClientFactory.Object, mockLocalStorageHelper.Object, mockLogger.Object);
            reportService = new ReportService(apiClient, mockLogger.Object);
        }

        [Fact]
        public async Task GenerateReportAsync_ValidRequest_ReturnsReport()
        {
            // Arrange
            var request = TestData.CreateTestReportRequest("calc-123");
            var expectedReport = TestData.CreateTestReport("report-123", "calc-123", "user-123");
            
            mockHttpHandler.When(ApiEndpoints.Report.Generate)
                .RespondWithApiResponse(expectedReport);
            
            // Act
            var result = await reportService.GenerateReportAsync(request);
            
            // Assert
            result.Should().NotBeNull();
            result.ReportId.Should().Be(expectedReport.ReportId);
            result.ReportTitle.Should().Be(expectedReport.ReportTitle);
            result.Format.Should().Be(expectedReport.Format);
        }

        [Fact]
        public async Task GenerateReportAsync_ApiError_ThrowsException()
        {
            // Arrange
            var request = TestData.CreateTestReportRequest("calc-123");
            var errorMessage = "Failed to generate report";
            
            mockHttpHandler.When(ApiEndpoints.Report.Generate)
                .RespondWithApiError(errorMessage);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => reportService.GenerateReportAsync(request));
            
            exception.Message.Should().Contain(errorMessage);
        }

        [Fact]
        public async Task GetReportAsync_ValidId_ReturnsReport()
        {
            // Arrange
            var reportId = "report-123";
            var expectedReport = TestData.CreateTestReport(reportId, "calc-123", "user-123");
            var endpoint = ApiEndpoints.Report.GetById.Replace("{id}", reportId);
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(expectedReport);
            
            // Act
            var result = await reportService.GetReportAsync(reportId);
            
            // Assert
            result.Should().NotBeNull();
            result.ReportId.Should().Be(expectedReport.ReportId);
            result.ReportTitle.Should().Be(expectedReport.ReportTitle);
            result.Format.Should().Be(expectedReport.Format);
        }

        [Fact]
        public async Task GetReportAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var reportId = "invalid-id";
            var errorMessage = "Report not found";
            var endpoint = ApiEndpoints.Report.GetById.Replace("{id}", reportId);
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiError(errorMessage);
            
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => reportService.GetReportAsync(reportId));
            
            exception.Message.Should().Contain(errorMessage);
        }

        [Fact]
        public async Task GetReportsAsync_ValidFilter_ReturnsReportList()
        {
            // Arrange
            var filter = new ReportFilterModel
            {
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow,
                Format = 0 // PDF
            };
            
            var expectedReportList = new ReportListModel
            {
                Items = new List<ReportSummaryModel>
                {
                    new ReportSummaryModel { ReportId = "report-1", ReportTitle = "Report 1" },
                    new ReportSummaryModel { ReportId = "report-2", ReportTitle = "Report 2" }
                },
                PageNumber = 1,
                TotalPages = 1,
                TotalItems = 2
            };
            
            var endpoint = $"{ApiEndpoints.Report.GetAll}?{filter.ToQueryString()}";
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(expectedReportList);
            
            // Act
            var result = await reportService.GetReportsAsync(filter);
            
            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull().And.HaveCount(expectedReportList.Items.Count);
            result.PageNumber.Should().Be(expectedReportList.PageNumber);
            result.TotalPages.Should().Be(expectedReportList.TotalPages);
        }

        [Fact]
        public async Task DownloadReportAsync_ValidId_ReturnsDownloadUrl()
        {
            // Arrange
            var reportId = "report-123";
            var expectedUrl = "https://storage.example.com/reports/report-123.pdf";
            var endpoint = $"{ApiEndpoints.Report.Download.Replace("{id}", reportId)}?format=0";
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(expectedUrl);
            
            // Act
            var result = await reportService.DownloadReportAsync(reportId, 0);
            
            // Assert
            result.Should().NotBeNull().And.Be(expectedUrl);
        }

        [Fact]
        public async Task EmailReportAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var reportId = "report-123";
            var email = "test@example.com";
            var subject = "Test Subject";
            var message = "Test Message";
            var endpoint = ApiEndpoints.Report.Email.Replace("{id}", reportId);
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await reportService.EmailReportAsync(reportId, email, subject, message);
            
            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ArchiveReportAsync_ValidId_ReturnsSuccess()
        {
            // Arrange
            var reportId = "report-123";
            var endpoint = $"{ApiEndpoints.Report.Base}/{reportId}/archive";
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await reportService.ArchiveReportAsync(reportId);
            
            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UnarchiveReportAsync_ValidId_ReturnsSuccess()
        {
            // Arrange
            var reportId = "report-123";
            var endpoint = $"{ApiEndpoints.Report.Base}/{reportId}/unarchive";
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await reportService.UnarchiveReportAsync(reportId);
            
            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteReportAsync_ValidId_ReturnsSuccess()
        {
            // Arrange
            var reportId = "report-123";
            var endpoint = ApiEndpoints.Report.GetById.Replace("{id}", reportId);
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(true);
            
            // Act
            var result = await reportService.DeleteReportAsync(reportId);
            
            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GetReportFormatsAsync_ReturnsFormatOptions()
        {
            // Arrange
            var endpoint = $"{ApiEndpoints.Report.Base}/formats";
            var expectedFormats = new List<ReportFormatOption>
            {
                new ReportFormatOption(0, "PDF", "file-pdf"),
                new ReportFormatOption(1, "Excel", "file-excel"),
                new ReportFormatOption(2, "CSV", "file-csv"),
                new ReportFormatOption(3, "HTML", "file-code")
            };
            
            mockHttpHandler.When(endpoint)
                .RespondWithApiResponse(expectedFormats);
            
            // Act
            var result = await reportService.GetReportFormatsAsync();
            
            // Assert
            result.Should().NotBeNull().And.HaveCount(expectedFormats.Count);
            result.Should().Contain(f => f.Text == "PDF" && f.Value == 0);
            result.Should().Contain(f => f.Text == "Excel" && f.Value == 1);
            result.Should().Contain(f => f.Text == "CSV" && f.Value == 2);
            result.Should().Contain(f => f.Text == "HTML" && f.Value == 3);
        }
    }
}