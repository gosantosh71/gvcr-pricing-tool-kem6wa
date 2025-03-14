using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging v6.0.0
using VatFilingPricingTool.Web.Clients;
using VatFilingPricingTool.Web.Models;
using VatFilingPricingTool.Web.Services.Interfaces;

namespace VatFilingPricingTool.Web.Services.Implementations
{
    /// <summary>
    /// Implementation of the IReportService interface that provides client-side functionality for generating,
    /// retrieving, and managing reports in the VAT Filing Pricing Tool. This service communicates with
    /// the backend API to perform report operations and handles the transformation of data between the
    /// client and server.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly ApiClient apiClient;
        private readonly ILogger<ReportService> logger;

        /// <summary>
        /// Initializes a new instance of the ReportService class with required dependencies.
        /// </summary>
        /// <param name="apiClient">Client for making HTTP requests to the backend API</param>
        /// <param name="logger">Logger for diagnostic information</param>
        public ReportService(ApiClient apiClient, ILogger<ReportService> logger)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a new report based on calculation data with specified format and content options.
        /// </summary>
        /// <param name="request">The report generation request containing parameters and options</param>
        /// <returns>A task containing the generated report model with metadata and download URL</returns>
        public async Task<ReportModel> GenerateReportAsync(ReportRequestModel request)
        {
            logger.LogInformation("Generating report with title: {Title}, format: {Format}", request.ReportTitle, request.Format);
            
            if (string.IsNullOrEmpty(request.ReportTitle))
            {
                throw new ArgumentException("Report title cannot be empty", nameof(request));
            }
            
            if (string.IsNullOrEmpty(request.CalculationId))
            {
                throw new ArgumentException("Calculation ID cannot be empty", nameof(request));
            }
            
            try
            {
                var endpoint = ApiEndpoints.Report.Generate;
                var result = await apiClient.PostAsync<ReportRequestModel, ReportModel>(endpoint, request);
                
                logger.LogInformation("Report generated successfully with ID: {ReportId}", result.ReportId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generating report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a specific report by ID with detailed information and download URL.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to retrieve</param>
        /// <returns>A task containing the requested report model</returns>
        public async Task<ReportModel> GetReportAsync(string reportId)
        {
            logger.LogInformation("Getting report with ID: {ReportId}", reportId);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            try
            {
                var endpoint = ApiEndpoints.Report.GetById.Replace("{id}", reportId);
                var result = await apiClient.GetAsync<ReportModel>(endpoint);
                
                logger.LogInformation("Retrieved report with title: {Title}", result.ReportTitle);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a paginated list of reports with optional filtering by date, type, and format.
        /// </summary>
        /// <param name="filter">The filter criteria for reports</param>
        /// <returns>A task containing a paginated list of report summaries</returns>
        public async Task<ReportListModel> GetReportsAsync(ReportFilterModel filter)
        {
            logger.LogInformation("Getting reports with filter parameters");
            
            try
            {
                // Initialize filter if null
                filter ??= new ReportFilterModel();
                
                var endpoint = $"{ApiEndpoints.Report.GetAll}?{filter.ToQueryString()}";
                var result = await apiClient.GetAsync<ReportListModel>(endpoint);
                
                logger.LogInformation("Retrieved {Count} reports", result.Items.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving reports: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Downloads a specific report, optionally converting to a different format than originally generated.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to download</param>
        /// <param name="format">The desired format for the report (0=PDF, 1=Excel, 2=CSV, 3=HTML)</param>
        /// <returns>A task containing the download URL for the report</returns>
        public async Task<string> DownloadReportAsync(string reportId, int format)
        {
            logger.LogInformation("Downloading report with ID: {ReportId}, format: {Format}", reportId, format);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            try
            {
                var endpoint = $"{ApiEndpoints.Report.Download.Replace("{id}", reportId)}?format={format}";
                var downloadUrl = await apiClient.GetAsync<string>(endpoint);
                
                logger.LogInformation("Report download URL generated: {DownloadUrl}", downloadUrl);
                return downloadUrl;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error downloading report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Emails a specific report to the specified email address with optional subject and message.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to email</param>
        /// <param name="emailAddress">The recipient email address</param>
        /// <param name="subject">The subject line for the email</param>
        /// <param name="message">Additional message to include in the email body</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        public async Task<bool> EmailReportAsync(string reportId, string emailAddress, string subject, string message)
        {
            logger.LogInformation("Emailing report with ID: {ReportId} to: {EmailAddress}", reportId, emailAddress);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentException("Email address cannot be empty", nameof(emailAddress));
            }
            
            try
            {
                var request = new EmailReportRequestModel
                {
                    ReportId = reportId,
                    EmailAddress = emailAddress,
                    Subject = subject ?? $"VAT Filing Report {reportId}",
                    Message = message
                };
                
                var endpoint = ApiEndpoints.Report.Email.Replace("{id}", reportId);
                var result = await apiClient.PostAsync<EmailReportRequestModel, bool>(endpoint, request);
                
                logger.LogInformation("Report email sent successfully: {Success}", result);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error emailing report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Archives a specific report to hide it from regular report listings while preserving the data.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to archive</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        public async Task<bool> ArchiveReportAsync(string reportId)
        {
            logger.LogInformation("Archiving report with ID: {ReportId}", reportId);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            try
            {
                var endpoint = $"{ApiEndpoints.Report.Base}/{reportId}/archive";
                var result = await apiClient.PutAsync<object, bool>(endpoint, null);
                
                logger.LogInformation("Report archived successfully: {Success}", result);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error archiving report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Unarchives a previously archived report to make it visible in regular report listings again.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to unarchive</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        public async Task<bool> UnarchiveReportAsync(string reportId)
        {
            logger.LogInformation("Unarchiving report with ID: {ReportId}", reportId);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            try
            {
                var endpoint = $"{ApiEndpoints.Report.Base}/{reportId}/unarchive";
                var result = await apiClient.PutAsync<object, bool>(endpoint, null);
                
                logger.LogInformation("Report unarchived successfully: {Success}", result);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error unarchiving report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Permanently deletes a report and its associated file from storage.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to delete</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        public async Task<bool> DeleteReportAsync(string reportId)
        {
            logger.LogInformation("Deleting report with ID: {ReportId}", reportId);
            
            if (string.IsNullOrEmpty(reportId))
            {
                throw new ArgumentException("Report ID cannot be empty", nameof(reportId));
            }
            
            try
            {
                var endpoint = ApiEndpoints.Report.GetById.Replace("{id}", reportId);
                var result = await apiClient.DeleteAsync<bool>(endpoint);
                
                logger.LogInformation("Report deleted successfully: {Success}", result);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves the available report format options for display in the UI.
        /// </summary>
        /// <returns>A task containing a list of report format options</returns>
        public async Task<List<ReportFormatOption>> GetReportFormatsAsync()
        {
            logger.LogInformation("Getting available report formats");
            
            try
            {
                var endpoint = $"{ApiEndpoints.Report.Base}/formats";
                var formats = await apiClient.GetAsync<List<ReportFormatOption>>(endpoint);
                
                logger.LogInformation("Retrieved {Count} report formats", formats.Count);
                return formats;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error retrieving report formats, using defaults: {Message}", ex.Message);
                
                // Return default formats if API call fails
                return new List<ReportFormatOption>
                {
                    new ReportFormatOption(0, "PDF", "file-pdf"),
                    new ReportFormatOption(1, "Excel", "file-excel"),
                    new ReportFormatOption(2, "CSV", "file-csv"),
                    new ReportFormatOption(3, "HTML", "file-code")
                };
            }
        }
    }
}