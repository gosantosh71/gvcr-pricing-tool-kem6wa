using FluentAssertions; // FluentAssertions, Version=6.2.0
using Microsoft.AspNetCore.Mvc.Testing; // Microsoft.AspNetCore.Mvc.Testing, Version=6.0.0
using System; // System, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using Xunit; // Xunit, Version=2.4.1
using VatFilingPricingTool.IntegrationTests.TestServer; // Factory for creating the test server and HTTP clients
using VatFilingPricingTool.Contracts.V1.Requests.ReportRequests; // Request model for generating a report
using VatFilingPricingTool.Contracts.V1.Responses.ReportResponses; // Response model for report generation
using VatFilingPricingTool.Domain.Enums; // Enum defining supported report formats
using VatFilingPricingTool.Contracts.V1.Models; // Model for calculation data used in report generation tests

namespace VatFilingPricingTool.IntegrationTests.Services
{
    /// <summary>
    /// Integration tests for the report generation functionality
    /// </summary>
    public class ReportGenerationTests : IntegrationTestBase
    {
        private readonly string _calculationId;
        private readonly string _apiEndpoint;

        /// <summary>
        /// Initializes a new instance of the ReportGenerationTests class
        /// </summary>
        public ReportGenerationTests()
        {
            // Call base constructor to set up test environment
            // LD1: Set _apiEndpoint to '/api/v1/reports'
            _apiEndpoint = "/api/v1/reports";
            // LD1: Set _calculationId to a predefined test calculation ID
            _calculationId = "c8f7e8d6-9a5b-4c3d-8e7f-1a2b3c4d5e6f";
        }

        /// <summary>
        /// Sets up test data required for report generation tests
        /// </summary>
        private async Task SetupTestData()
        {
            // Create a test calculation if it doesn't exist
            // Store the calculation ID for use in tests
        }

        /// <summary>
        /// Tests that report generation succeeds with valid data
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithValidData_ShouldSucceed()
        {
            // Arrange: Create a GenerateReportRequest with valid data
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            // Arrange: Set up an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send POST request to generate report
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, request, client);

            // Assert: Response is successful
            response.Success.Should().BeTrue();

            // Assert: Response data contains valid ReportId
            response.Data.ReportId.Should().NotBeNullOrEmpty();

            // Assert: Response data contains DownloadUrl
            response.Data.DownloadUrl.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that report generation succeeds with different formats
        /// </summary>
        [Theory]
        [InlineData(ReportFormat.PDF)]
        [InlineData(ReportFormat.Excel)]
        [InlineData(ReportFormat.CSV)]
        [InlineData(ReportFormat.HTML)]
        public async Task GenerateReport_WithDifferentFormats_ShouldSucceed(ReportFormat format)
        {
            // Arrange: Create a GenerateReportRequest with specified format
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = format,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            // Arrange: Set up an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send POST request to generate report
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, request, client);

            // Assert: Response is successful
            response.Success.Should().BeTrue();

            // Assert: Response data contains valid ReportId
            response.Data.ReportId.Should().NotBeNullOrEmpty();

            // Assert: Response data Format matches requested format
            response.Data.Format.Should().Be(format);
        }

        /// <summary>
        /// Tests that report generation succeeds with different content options
        /// </summary>
        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public async Task GenerateReport_WithDifferentOptions_ShouldSucceed(bool includeCountryBreakdown, bool includeServiceDetails, bool includeAppliedDiscounts)
        {
            // Arrange: Create a GenerateReportRequest with specified options
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = includeCountryBreakdown,
                IncludeServiceDetails = includeServiceDetails,
                IncludeAppliedDiscounts = includeAppliedDiscounts
            };

            // Arrange: Set up an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send POST request to generate report
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, request, client);

            // Assert: Response is successful
            response.Success.Should().BeTrue();

            // Assert: Response data contains valid ReportId
            response.Data.ReportId.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that report generation fails with invalid calculation ID
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithInvalidCalculationId_ShouldFail()
        {
            // Arrange: Create a GenerateReportRequest with invalid calculation ID
            var request = new GenerateReportRequest
            {
                CalculationId = "invalid-calculation-id",
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            // Arrange: Set up an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send POST request to generate report
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, request, client);

            // Assert: Response is not successful
            response.Success.Should().BeFalse();

            // Assert: Response contains appropriate error message
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that report generation fails without authentication
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithoutAuthentication_ShouldFail()
        {
            // Arrange: Create a GenerateReportRequest with valid data
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            // Arrange: Use unauthenticated client
            // Act: Send POST request to generate report
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, request);

            // Assert: Response status code is 401 Unauthorized
            response.StatusCode.Should().Be(401);
        }

        /// <summary>
        /// Tests that report download succeeds with valid report ID
        /// </summary>
        [Fact]
        public async Task DownloadReport_WithValidId_ShouldSucceed()
        {
            // Arrange: Generate a report to get a valid report ID
            var generateRequest = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            var generateClient = CreateAuthenticatedClient(UserRole.Customer);
            var generateResponse = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, generateRequest, generateClient);

            // Arrange: Create a DownloadReportRequest with the report ID
            var downloadRequest = new DownloadReportRequest
            {
                ReportId = generateResponse.Data.ReportId,
                Format = ReportFormat.PDF
            };

            // Arrange: Set up an authenticated client with Customer role
            var downloadClient = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send GET request to download report
            var downloadResponse = await GetAsync<DownloadReportResponse>($"{_apiEndpoint}/download?reportId={downloadRequest.ReportId}&format={downloadRequest.Format}", downloadClient);

            // Assert: Response is successful
            downloadResponse.Success.Should().BeTrue();

            // Assert: Response data contains matching ReportId
            downloadResponse.Data.ReportId.Should().Be(generateResponse.Data.ReportId);

            // Assert: Response data contains non-empty FileName
            downloadResponse.Data.FileName.Should().NotBeNullOrEmpty();

            // Assert: Response data contains appropriate ContentType
            downloadResponse.Data.ContentType.Should().Be("application/pdf");

            // Assert: Response data contains FileSize greater than zero
            downloadResponse.Data.FileSize.Should().BeGreaterThan(0);
        }

        /// <summary>
        /// Tests that report download succeeds with format conversion
        /// </summary>
        [Theory]
        [InlineData(ReportFormat.PDF, ReportFormat.Excel)]
        [InlineData(ReportFormat.Excel, ReportFormat.PDF)]
        [InlineData(ReportFormat.PDF, ReportFormat.CSV)]
        public async Task DownloadReport_WithDifferentFormat_ShouldSucceed(ReportFormat originalFormat, ReportFormat downloadFormat)
        {
            // Arrange: Generate a report in originalFormat
            var generateRequest = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = originalFormat,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            var generateClient = CreateAuthenticatedClient(UserRole.Customer);
            var generateResponse = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, generateRequest, generateClient);

            // Arrange: Create a DownloadReportRequest with the report ID and downloadFormat
            var downloadRequest = new DownloadReportRequest
            {
                ReportId = generateResponse.Data.ReportId,
                Format = downloadFormat
            };

            // Arrange: Set up an authenticated client with Customer role
            var downloadClient = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send GET request to download report
            var downloadResponse = await GetAsync<DownloadReportResponse>($"{_apiEndpoint}/download?reportId={downloadRequest.ReportId}&format={downloadRequest.Format}", downloadClient);

            // Assert: Response is successful
            downloadResponse.Success.Should().BeTrue();

            // Assert: Response data Format matches downloadFormat
            downloadResponse.Data.Format.Should().Be(downloadFormat);
        }

        /// <summary>
        /// Tests that report download fails with invalid report ID
        /// </summary>
        [Fact]
        public async Task DownloadReport_WithInvalidId_ShouldFail()
        {
            // Arrange: Create a DownloadReportRequest with invalid report ID
            var downloadRequest = new DownloadReportRequest
            {
                ReportId = "invalid-report-id",
                Format = ReportFormat.PDF
            };

            // Arrange: Set up an authenticated client with Customer role
            var downloadClient = CreateAuthenticatedClient(UserRole.Customer);

            // Act: Send GET request to download report
            var response = await GetAsync<DownloadReportResponse>($"{_apiEndpoint}/download?reportId={downloadRequest.ReportId}&format={downloadRequest.Format}", downloadClient);

            // Assert: Response is not successful
            response.Success.Should().BeFalse();

            // Assert: Response contains appropriate error message
            response.Message.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that report download fails when attempted by a different user
        /// </summary>
        [Fact]
        public async Task DownloadReport_WithDifferentUser_ShouldFail()
        {
            // Arrange: Generate a report as Customer role
            var generateRequest = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true
            };

            var generateClient = CreateAuthenticatedClient(UserRole.Customer);
            var generateResponse = await PostAsync<GenerateReportRequest, GenerateReportResponse>(_apiEndpoint, generateRequest, generateClient);

            // Arrange: Create a DownloadReportRequest with the report ID
            var downloadRequest = new DownloadReportRequest
            {
                ReportId = generateResponse.Data.ReportId,
                Format = ReportFormat.PDF
            };

            // Arrange: Set up an authenticated client with a different user (Accountant role)
            var downloadClient = CreateAuthenticatedClient(UserRole.Accountant);

            // Act: Send GET request to download report
            var response = await GetAsync<DownloadReportResponse>($"{_apiEndpoint}/download?reportId={downloadRequest.ReportId}&format={downloadRequest.Format}", downloadClient);

            // Assert: Response is not successful
            response.Success.Should().BeFalse();

            // Assert: Response contains appropriate error message about authorization
            response.Message.Should().NotBeNullOrEmpty();
        }
    }
}