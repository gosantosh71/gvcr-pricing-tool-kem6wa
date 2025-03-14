using System.Collections.Generic; // System.Collections.Generic v6.0.0
using System.Threading.Tasks; // System.Threading.Tasks v6.0.0
using VatFilingPricingTool.Web.Models;

namespace VatFilingPricingTool.Web.Services.Interfaces
{
    /// <summary>
    /// Client-side service interface for report generation, retrieval, and management operations
    /// in the VAT Filing Pricing Tool. Provides functionality for generating reports based on
    /// VAT filing calculations, exporting reports in various formats, and managing report lifecycle.
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates a new report based on calculation data with specified format and content options.
        /// </summary>
        /// <param name="request">The report generation request containing parameters and options</param>
        /// <returns>A task containing the generated report model with metadata and download URL</returns>
        Task<ReportModel> GenerateReportAsync(ReportRequestModel request);

        /// <summary>
        /// Retrieves a specific report by ID with detailed information and download URL.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to retrieve</param>
        /// <returns>A task containing the requested report model</returns>
        Task<ReportModel> GetReportAsync(string reportId);

        /// <summary>
        /// Retrieves a paginated list of reports with optional filtering by date, type, and format.
        /// </summary>
        /// <param name="filter">The filter criteria for reports</param>
        /// <returns>A task containing a paginated list of report summaries</returns>
        Task<ReportListModel> GetReportsAsync(ReportFilterModel filter);

        /// <summary>
        /// Downloads a specific report, optionally converting to a different format than originally generated.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to download</param>
        /// <param name="format">The desired format for the report (0=PDF, 1=Excel, 2=CSV, 3=HTML)</param>
        /// <returns>A task containing the download URL for the report</returns>
        Task<string> DownloadReportAsync(string reportId, int format);

        /// <summary>
        /// Emails a specific report to the specified email address with optional subject and message.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to email</param>
        /// <param name="emailAddress">The recipient email address</param>
        /// <param name="subject">The subject line for the email</param>
        /// <param name="message">Additional message to include in the email body</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        Task<bool> EmailReportAsync(string reportId, string emailAddress, string subject, string message);

        /// <summary>
        /// Archives a specific report to hide it from regular report listings while preserving the data.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to archive</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        Task<bool> ArchiveReportAsync(string reportId);

        /// <summary>
        /// Unarchives a previously archived report to make it visible in regular report listings again.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to unarchive</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        Task<bool> UnarchiveReportAsync(string reportId);

        /// <summary>
        /// Permanently deletes a report and its associated file from storage.
        /// </summary>
        /// <param name="reportId">The unique identifier of the report to delete</param>
        /// <returns>A task containing a boolean indicating success or failure</returns>
        Task<bool> DeleteReportAsync(string reportId);

        /// <summary>
        /// Retrieves the available report format options for display in the UI.
        /// </summary>
        /// <returns>A task containing a list of report format options</returns>
        Task<List<ReportFormatOption>> GetReportFormatsAsync();
    }
}