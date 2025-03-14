using System.Threading.Tasks;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Contracts.V1.Requests;
using VatFilingPricingTool.Contracts.V1.Responses;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Service interface for report generation, retrieval, and management operations
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Generates a new report based on calculation data with specified format and content options
        /// </summary>
        /// <param name="request">The report generation request containing calculation ID, title, format, and content options</param>
        /// <param name="userId">ID of the user generating the report</param>
        /// <returns>A result containing the generated report details and download URL</returns>
        Task<Result<GenerateReportResponse>> GenerateReportAsync(GenerateReportRequest request, string userId);
        
        /// <summary>
        /// Retrieves a specific report by ID with detailed information and download URL
        /// </summary>
        /// <param name="request">The report retrieval request containing the report ID</param>
        /// <param name="userId">ID of the user requesting the report</param>
        /// <returns>A result containing the report details</returns>
        Task<Result<GetReportResponse>> GetReportAsync(GetReportRequest request, string userId);
        
        /// <summary>
        /// Retrieves a paginated list of reports for a user with optional filtering by date, type, and format
        /// </summary>
        /// <param name="request">The report history request containing pagination, filtering and sorting parameters</param>
        /// <param name="userId">ID of the user requesting report history</param>
        /// <returns>A result containing the paginated list of reports</returns>
        Task<Result<GetReportHistoryResponse>> GetReportHistoryAsync(GetReportHistoryRequest request, string userId);
        
        /// <summary>
        /// Downloads a specific report, optionally converting to a different format than originally generated
        /// </summary>
        /// <param name="request">The download request containing report ID and optional format conversion</param>
        /// <param name="userId">ID of the user downloading the report</param>
        /// <returns>A result containing the report download details</returns>
        Task<Result<DownloadReportResponse>> DownloadReportAsync(DownloadReportRequest request, string userId);
        
        /// <summary>
        /// Emails a specific report to the specified email address with optional subject and message
        /// </summary>
        /// <param name="request">The email request containing report ID, recipient, subject and message</param>
        /// <param name="userId">ID of the user requesting the email</param>
        /// <returns>A result indicating whether the email was sent successfully</returns>
        Task<Result<EmailReportResponse>> EmailReportAsync(EmailReportRequest request, string userId);
        
        /// <summary>
        /// Archives a specific report to hide it from regular report listings while preserving the data
        /// </summary>
        /// <param name="request">The archive request containing report ID</param>
        /// <param name="userId">ID of the user archiving the report</param>
        /// <returns>A result indicating whether the report was archived successfully</returns>
        Task<Result<ArchiveReportResponse>> ArchiveReportAsync(ArchiveReportRequest request, string userId);
        
        /// <summary>
        /// Unarchives a previously archived report to make it visible in regular report listings again
        /// </summary>
        /// <param name="reportId">ID of the report to unarchive</param>
        /// <param name="userId">ID of the user unarchiving the report</param>
        /// <returns>A result indicating whether the report was unarchived successfully</returns>
        Task<Result<ArchiveReportResponse>> UnarchiveReportAsync(string reportId, string userId);
        
        /// <summary>
        /// Permanently deletes a report and its associated file from storage
        /// </summary>
        /// <param name="reportId">ID of the report to delete</param>
        /// <param name="userId">ID of the user deleting the report</param>
        /// <returns>A result indicating whether the report was deleted successfully</returns>
        Task<Result> DeleteReportAsync(string reportId, string userId);
    }
}