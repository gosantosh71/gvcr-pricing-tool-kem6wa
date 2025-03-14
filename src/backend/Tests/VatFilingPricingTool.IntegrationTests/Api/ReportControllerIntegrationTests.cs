using FluentAssertions; // FluentAssertions, Version=6.2.0
using Microsoft.AspNetCore.Mvc.Testing; // Microsoft.AspNetCore.Mvc.Testing, Version=6.0.0
using System; // System, Version=6.0.0
using System.Net.Http; // System.Net.Http, Version=6.0.0
using System.Net.Http.Json; // System.Net.Http.Json, Version=6.0.0
using System.Text.Json; // System.Text.Json, Version=6.0.0
using System.Threading.Tasks; // System.Threading.Tasks, Version=6.0.0
using VatFilingPricingTool.Common.Models.ApiResponse; // Generic API response wrapper for handling test responses
using VatFilingPricingTool.Domain.Enums; // User role enumeration for authentication testing
using VatFilingPricingTool.IntegrationTests.TestServer; // Base class for integration tests
using VatFilingPricingTool.Common.Constants; // Constants for API route paths
using VatFilingPricingTool.Contracts.V1.Requests.ReportRequests; // Request model for generating a report
using VatFilingPricingTool.Contracts.V1.Responses.ReportResponses; // Response model for report generation

namespace VatFilingPricingTool.IntegrationTests.Api
{
    /// <summary>
    /// Integration tests for the Report Controller API endpoints
    /// </summary>
    [Collection("Sequential")]
    public class ReportControllerIntegrationTests : IntegrationTestBase
    {
        private string _calculationId = "b4494832-554a-4458-a63f-593599091c90";
        private string _reportId;

        /// <summary>
        /// Initializes a new instance of the ReportControllerIntegrationTests class
        /// </summary>
        public ReportControllerIntegrationTests()
        {
            // LD1: Initialize _calculationId with a test calculation ID
            // LD1: Initialize _reportId as null (will be set during test execution)
        }

        /// <summary>
        /// Tests that a valid report generation request returns a successful response
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithValidRequest_ReturnsSuccessResponse()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a valid GenerateReportRequest with test calculation ID
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Test Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                DeliveryOptions = new ReportDeliveryOptions
                {
                    DownloadImmediately = true,
                    SendEmail = false
                }
            };

            // LD1: Send POST request to the generate report endpoint
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(ApiRoutes.Report.Generate, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a valid report ID
            response.Data.Should().NotBeNull();
            response.Data.ReportId.Should().NotBeNullOrEmpty();

            // LD1: Store the report ID for use in subsequent tests
            _reportId = response.Data.ReportId;
        }

        /// <summary>
        /// Tests that retrieving a report with a valid ID returns the report details
        /// </summary>
        [Fact]
        public async Task GetReport_WithValidId_ReturnsReport()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send GET request to the get report endpoint with the stored report ID
            var response = await GetAsync<GetReportResponse>($"{ApiRoutes.Report.GetById.Replace(\"{{id}}\", _reportId)}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains the expected report details
            response.Data.Should().NotBeNull();
            response.Data.ReportId.Should().Be(_reportId);

            // LD1: Assert that the report title matches the expected value
            response.Data.ReportTitle.Should().Be("Test Report");
        }

        /// <summary>
        /// Tests that retrieving report history returns a list of reports
        /// </summary>
        [Fact]
        public async Task GetReportHistory_ReturnsReportList()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a GetReportHistoryRequest with pagination parameters
            var request = new GetReportHistoryRequest { Page = 1, PageSize = 10 };

            // LD1: Send GET request to the report history endpoint
            var response = await GetAsync<GetReportHistoryResponse>($"{ApiRoutes.Report.GetAll}?page={request.Page}&pageSize={request.PageSize}", client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a list of reports
            response.Data.Should().NotBeNull();
            response.Data.Reports.Should().NotBeNull();

            // LD1: Assert that the total count is greater than or equal to 0
            response.Data.TotalCount.Should().BeGreaterOrEqualTo(0);
        }

        /// <summary>
        /// Tests that downloading a report with a valid ID returns the file content
        /// </summary>
        [Fact]
        public async Task DownloadReport_WithValidId_ReturnsFileContent()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send GET request to the download report endpoint with the stored report ID
            var response = await client.GetAsync($"{ApiRoutes.Report.Download.Replace("{{id}}", _reportId)}");

            // LD1: Assert that the response is successful
            response.EnsureSuccessStatusCode();

            // LD1: Assert that the response contains file content
            var content = await response.Content.ReadAsByteArrayAsync();
            content.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// Tests that emailing a report with valid parameters returns a successful response
        /// </summary>
        [Fact]
        public async Task EmailReport_WithValidRequest_ReturnsSuccessResponse()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a valid EmailReportRequest with test report ID and email details
            var request = new EmailReportRequest
            {
                ReportId = _reportId,
                EmailAddress = "test@example.com",
                Subject = "Test Report",
                Message = "Please find attached the test report."
            };

            // LD1: Send POST request to the email report endpoint
            var response = await PostAsync<EmailReportRequest, EmailReportResponse>(ApiRoutes.Report.Email.Replace("{{id}}", _reportId), request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the email was sent successfully
            response.Data.Should().NotBeNull();
            response.Data.EmailSent.Should().BeTrue();
        }

        /// <summary>
        /// Tests that archiving a report with a valid ID returns a successful response
        /// </summary>
        [Fact]
        public async Task ArchiveReport_WithValidId_ReturnsSuccessResponse()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send PUT request to the archive report endpoint with the stored report ID
            var response = await client.PutAsync($"{ApiRoutes.Report.Archive.Replace("{{id}}", _reportId)}", null);

            // LD1: Assert that the response is successful
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Tests that unarchiving a report with a valid ID returns a successful response
        /// </summary>
        [Fact]
        public async Task UnarchiveReport_WithValidId_ReturnsSuccessResponse()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send PUT request to the unarchive report endpoint with the stored report ID
            var response = await client.PutAsync($"{ApiRoutes.Report.Unarchive.Replace("{{id}}", _reportId)}", null);

            // LD1: Assert that the response is successful
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Tests that deleting a report with a valid ID returns a successful response
        /// </summary>
        [Fact]
        public async Task DeleteReport_WithValidId_ReturnsSuccessResponse()
        {
            // LD1: Skip the test if no report ID is available from previous test
            if (string.IsNullOrEmpty(_reportId))
            {
                Skip = "Skipping test because GenerateReport test was not executed or failed.";
                return;
            }

            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send DELETE request to the delete report endpoint with the stored report ID
            var response = await client.DeleteAsync($"{ApiRoutes.Report.Delete.Replace("{{id}}", _reportId)}");

            // LD1: Assert that the response is successful
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Tests that attempting to retrieve a report with an invalid ID returns a not found response
        /// </summary>
        [Fact]
        public async Task GetReport_WithInvalidId_ReturnsNotFound()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Send GET request to the get report endpoint with an invalid report ID
            var response = await client.GetAsync($"{ApiRoutes.Report.GetById.Replace("{{id}}", Guid.NewGuid().ToString())}");

            // LD1: Assert that the response is not successful
            response.IsSuccessStatusCode.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Report not found");
        }

        /// <summary>
        /// Tests that attempting to generate a report with an invalid calculation ID returns a bad request response
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithInvalidCalculationId_ReturnsBadRequest()
        {
            // LD1: Create an authenticated client with Administrator role
            var client = CreateAuthenticatedClient(UserRole.Administrator);

            // LD1: Create a GenerateReportRequest with an invalid calculation ID
            var request = new GenerateReportRequest
            {
                CalculationId = Guid.NewGuid().ToString(),
                ReportTitle = "Invalid Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                DeliveryOptions = new ReportDeliveryOptions
                {
                    DownloadImmediately = true,
                    SendEmail = false
                }
            };

            // LD1: Send POST request to the generate report endpoint
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(ApiRoutes.Report.Generate, request, client);

            // LD1: Assert that the response is not successful
            response.Success.Should().BeFalse();

            // LD1: Assert that the response contains an appropriate error message
            response.Message.Should().Contain("Calculation not found");
        }

        /// <summary>
        /// Tests that a customer role can generate a report successfully
        /// </summary>
        [Fact]
        public async Task GenerateReport_WithCustomerRole_ReturnsSuccessResponse()
        {
            // LD1: Create an authenticated client with Customer role
            var client = CreateAuthenticatedClient(UserRole.Customer);

            // LD1: Create a valid GenerateReportRequest with test calculation ID
            var request = new GenerateReportRequest
            {
                CalculationId = _calculationId,
                ReportTitle = "Customer Report",
                Format = ReportFormat.PDF,
                IncludeCountryBreakdown = true,
                IncludeServiceDetails = true,
                IncludeAppliedDiscounts = true,
                DeliveryOptions = new ReportDeliveryOptions
                {
                    DownloadImmediately = true,
                    SendEmail = false
                }
            };

            // LD1: Send POST request to the generate report endpoint
            var response = await PostAsync<GenerateReportRequest, GenerateReportResponse>(ApiRoutes.Report.Generate, request, client);

            // LD1: Assert that the response is successful
            response.Success.Should().BeTrue();

            // LD1: Assert that the response data contains a valid report ID
            response.Data.Should().NotBeNull();
            response.Data.ReportId.Should().NotBeNullOrEmpty();
        }
    }
}