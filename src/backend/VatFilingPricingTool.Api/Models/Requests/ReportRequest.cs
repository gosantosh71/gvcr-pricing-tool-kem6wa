using System; // Version: 6.0.0 - Core .NET functionality
using System.ComponentModel.DataAnnotations; // Version: 6.0.0 - For validation attributes on model properties
using System.Text.Json.Serialization; // Version: 6.0.0 - For JSON serialization attributes
using VatFilingPricingTool.Domain.Enums; // For ReportFormat enum

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for generating a new report based on a calculation
    /// </summary>
    public class GenerateReportRequest
    {
        /// <summary>
        /// The unique identifier of the calculation to generate a report for
        /// </summary>
        [Required(ErrorMessage = "Calculation ID is required")]
        public string CalculationId { get; set; }

        /// <summary>
        /// The title of the report to be generated
        /// </summary>
        [Required(ErrorMessage = "Report title is required")]
        [StringLength(100, ErrorMessage = "Report title cannot exceed 100 characters")]
        public string ReportTitle { get; set; }

        /// <summary>
        /// The format of the report to be generated
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Whether to include country breakdown in the report
        /// </summary>
        public bool IncludeCountryBreakdown { get; set; }

        /// <summary>
        /// Whether to include service details in the report
        /// </summary>
        public bool IncludeServiceDetails { get; set; }

        /// <summary>
        /// Whether to include applied discounts in the report
        /// </summary>
        public bool IncludeAppliedDiscounts { get; set; }

        /// <summary>
        /// Whether to include historical comparison in the report
        /// </summary>
        public bool IncludeHistoricalComparison { get; set; }

        /// <summary>
        /// Whether to include tax rate details in the report
        /// </summary>
        public bool IncludeTaxRateDetails { get; set; }

        /// <summary>
        /// Options for report delivery after generation
        /// </summary>
        public ReportDeliveryOptions DeliveryOptions { get; set; }

        /// <summary>
        /// Default constructor for the GenerateReportRequest
        /// </summary>
        public GenerateReportRequest()
        {
            Format = ReportFormat.PDF;
            IncludeCountryBreakdown = true;
            IncludeServiceDetails = true;
            IncludeAppliedDiscounts = true;
            IncludeHistoricalComparison = false;
            IncludeTaxRateDetails = false;
            DeliveryOptions = new ReportDeliveryOptions();
        }
    }

    /// <summary>
    /// Options for report delivery after generation
    /// </summary>
    public class ReportDeliveryOptions
    {
        /// <summary>
        /// Whether to download the report immediately after generation
        /// </summary>
        public bool DownloadImmediately { get; set; }

        /// <summary>
        /// Whether to send the report via email
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// The email address to send the report to
        /// </summary>
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The subject of the email when sending the report
        /// </summary>
        [StringLength(100, ErrorMessage = "Email subject cannot exceed 100 characters")]
        public string EmailSubject { get; set; }

        /// <summary>
        /// The message body of the email when sending the report
        /// </summary>
        [StringLength(1000, ErrorMessage = "Email message cannot exceed 1000 characters")]
        public string EmailMessage { get; set; }

        /// <summary>
        /// Default constructor for the ReportDeliveryOptions
        /// </summary>
        public ReportDeliveryOptions()
        {
            DownloadImmediately = true;
            SendEmail = false;
            EmailAddress = string.Empty;
            EmailSubject = string.Empty;
            EmailMessage = string.Empty;
        }
    }

    /// <summary>
    /// Request model for retrieving a specific report by ID
    /// </summary>
    public class GetReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to retrieve
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// Default constructor for the GetReportRequest
        /// </summary>
        public GetReportRequest()
        {
            ReportId = string.Empty;
        }
    }

    /// <summary>
    /// Request model for retrieving a paginated list of reports with optional filtering
    /// </summary>
    public class GetReportHistoryRequest
    {
        /// <summary>
        /// Filter reports created on or after this date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Filter reports created on or before this date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Filter reports by report type
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Filter reports by format
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat? Format { get; set; }

        /// <summary>
        /// The page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1")]
        public int Page { get; set; }

        /// <summary>
        /// The number of reports per page
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; }

        /// <summary>
        /// Whether to include archived reports in the results
        /// </summary>
        public bool IncludeArchived { get; set; }

        /// <summary>
        /// Default constructor for the GetReportHistoryRequest
        /// </summary>
        public GetReportHistoryRequest()
        {
            Page = 1;
            PageSize = 10;
            IncludeArchived = false;
            ReportType = string.Empty;
        }
    }

    /// <summary>
    /// Request model for downloading a specific report, optionally converting to a different format
    /// </summary>
    public class DownloadReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to download
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// The format to convert the report to when downloading (optional)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat? Format { get; set; }

        /// <summary>
        /// Default constructor for the DownloadReportRequest
        /// </summary>
        public DownloadReportRequest()
        {
            ReportId = string.Empty;
        }
    }

    /// <summary>
    /// Request model for emailing a specific report to the specified email address
    /// </summary>
    public class EmailReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to email
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// The email address to send the report to
        /// </summary>
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The subject of the email when sending the report
        /// </summary>
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(100, ErrorMessage = "Subject cannot exceed 100 characters")]
        public string Subject { get; set; }

        /// <summary>
        /// The message body of the email when sending the report
        /// </summary>
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
        public string Message { get; set; }

        /// <summary>
        /// Default constructor for the EmailReportRequest
        /// </summary>
        public EmailReportRequest()
        {
            ReportId = string.Empty;
            EmailAddress = string.Empty;
            Subject = string.Empty;
            Message = string.Empty;
        }
    }

    /// <summary>
    /// Request model for archiving a specific report
    /// </summary>
    public class ArchiveReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to archive
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// Default constructor for the ArchiveReportRequest
        /// </summary>
        public ArchiveReportRequest()
        {
            ReportId = string.Empty;
        }
    }

    /// <summary>
    /// Request model for unarchiving a previously archived report
    /// </summary>
    public class UnarchiveReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to unarchive
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// Default constructor for the UnarchiveReportRequest
        /// </summary>
        public UnarchiveReportRequest()
        {
            ReportId = string.Empty;
        }
    }

    /// <summary>
    /// Request model for permanently deleting a report and its associated file
    /// </summary>
    public class DeleteReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to delete
        /// </summary>
        [Required(ErrorMessage = "Report ID is required")]
        public string ReportId { get; set; }

        /// <summary>
        /// Default constructor for the DeleteReportRequest
        /// </summary>
        public DeleteReportRequest()
        {
            ReportId = string.Empty;
        }
    }
}