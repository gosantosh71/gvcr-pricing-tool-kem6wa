using System; // Version: 6.0.0 - Core .NET functionality
using System.ComponentModel.DataAnnotations; // Version: 6.0.0 - For validation attributes on model properties
using System.Text.Json.Serialization; // Version: 6.0.0 - For JSON serialization attributes
using VatFilingPricingTool.Domain.Enums; // For report format enumerations

namespace VatFilingPricingTool.Contracts.V1.Requests
{
    /// <summary>
    /// Request model for generating a new report based on a calculation result.
    /// </summary>
    public class GenerateReportRequest
    {
        /// <summary>
        /// The unique identifier of the calculation to base the report on
        /// </summary>
        [Required(ErrorMessage = "CalculationId is required")]
        [StringLength(50, ErrorMessage = "CalculationId cannot exceed 50 characters")]
        public string CalculationId { get; set; }

        /// <summary>
        /// The title of the report
        /// </summary>
        [Required(ErrorMessage = "ReportTitle is required")]
        [StringLength(100, ErrorMessage = "ReportTitle cannot exceed 100 characters")]
        public string ReportTitle { get; set; }

        /// <summary>
        /// The format of the report (PDF, Excel, CSV, HTML)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Whether to include a country-by-country cost breakdown in the report
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
        /// Whether to include historical comparison with previous calculations
        /// </summary>
        public bool IncludeHistoricalComparison { get; set; }

        /// <summary>
        /// Whether to include detailed tax rate information for each country
        /// </summary>
        public bool IncludeTaxRateDetails { get; set; }

        /// <summary>
        /// Options for how the report should be delivered after generation
        /// </summary>
        public ReportDeliveryOptions DeliveryOptions { get; set; }

        /// <summary>
        /// Default constructor with sensible defaults for report generation
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
        /// Whether the report should be available for immediate download
        /// </summary>
        public bool DownloadImmediately { get; set; }

        /// <summary>
        /// Whether the report should be sent via email
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// The email address to send the report to (required if SendEmail is true)
        /// </summary>
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The subject line for the email (required if SendEmail is true)
        /// </summary>
        [StringLength(200, ErrorMessage = "Email subject cannot exceed 200 characters")]
        public string EmailSubject { get; set; }

        /// <summary>
        /// The message body for the email
        /// </summary>
        [StringLength(2000, ErrorMessage = "Email message cannot exceed 2000 characters")]
        public string EmailMessage { get; set; }

        /// <summary>
        /// Default constructor with sensible defaults for report delivery
        /// </summary>
        public ReportDeliveryOptions()
        {
            DownloadImmediately = true;
            SendEmail = false;
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
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving a paginated list of reports with optional filtering
    /// </summary>
    public class GetReportHistoryRequest
    {
        /// <summary>
        /// The start date for filtering reports (optional)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The end date for filtering reports (optional)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The type of report to filter by (optional)
        /// </summary>
        [StringLength(50, ErrorMessage = "ReportType cannot exceed 50 characters")]
        public string ReportType { get; set; }

        /// <summary>
        /// The format of reports to filter by (optional)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat? Format { get; set; }

        /// <summary>
        /// The page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
        public int Page { get; set; }

        /// <summary>
        /// The number of reports per page
        /// </summary>
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize { get; set; }

        /// <summary>
        /// Whether to include archived reports in the results
        /// </summary>
        public bool IncludeArchived { get; set; }

        /// <summary>
        /// Default constructor with sensible defaults for pagination
        /// </summary>
        public GetReportHistoryRequest()
        {
            Page = 1;
            PageSize = 10;
            IncludeArchived = false;
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
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }

        /// <summary>
        /// The format to convert the report to (optional - if not specified, the original format will be used)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat? Format { get; set; }
    }

    /// <summary>
    /// Request model for emailing a specific report to the specified email address
    /// </summary>
    public class EmailReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to email
        /// </summary>
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }

        /// <summary>
        /// The email address to send the report to
        /// </summary>
        [Required(ErrorMessage = "EmailAddress is required")]
        [EmailAddress(ErrorMessage = "Please provide a valid email address")]
        [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The subject line for the email
        /// </summary>
        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters")]
        public string Subject { get; set; }

        /// <summary>
        /// The message body for the email
        /// </summary>
        [StringLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
        public string Message { get; set; }
    }

    /// <summary>
    /// Request model for archiving a specific report
    /// </summary>
    public class ArchiveReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to archive
        /// </summary>
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }
    }

    /// <summary>
    /// Request model for unarchiving a previously archived report
    /// </summary>
    public class UnarchiveReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to unarchive
        /// </summary>
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }
    }

    /// <summary>
    /// Request model for permanently deleting a report and its associated file
    /// </summary>
    public class DeleteReportRequest
    {
        /// <summary>
        /// The unique identifier of the report to delete
        /// </summary>
        [Required(ErrorMessage = "ReportId is required")]
        [StringLength(50, ErrorMessage = "ReportId cannot exceed 50 characters")]
        public string ReportId { get; set; }
    }
}