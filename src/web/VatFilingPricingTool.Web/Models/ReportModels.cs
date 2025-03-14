using System;  // System v6.0.0
using System.Collections.Generic;  // System.Collections.Generic v6.0.0
using System.ComponentModel.DataAnnotations;  // System.ComponentModel.DataAnnotations v6.0.0
using System.Text.Json.Serialization;  // System.Text.Json v6.0.0

namespace VatFilingPricingTool.Web.Models
{
    /// <summary>
    /// Represents a report generated from VAT filing cost calculations with metadata about the report content and storage
    /// </summary>
    public class ReportModel
    {
        /// <summary>
        /// The unique identifier for this report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// The identifier of the user who generated this report
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The identifier of the calculation that this report is based on
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// The title of the report
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// The type of report (e.g., cost summary, detailed breakdown)
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// The format of the report (0=PDF, 1=Excel, 2=CSV, 3=HTML)
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        /// The URL where the report file is stored
        /// </summary>
        public string StorageUrl { get; set; }

        /// <summary>
        /// The date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// The size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

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
        /// Indicates if this report has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Default constructor for the ReportModel
        /// </summary>
        public ReportModel()
        {
            GenerationDate = DateTime.UtcNow;
            Format = 0; // PDF by default
            IncludeCountryBreakdown = true;
            IncludeServiceDetails = true;
            IncludeAppliedDiscounts = true;
            IncludeHistoricalComparison = false;
            IncludeTaxRateDetails = false;
            IsArchived = false;
        }

        /// <summary>
        /// Gets the file extension based on the report format
        /// </summary>
        /// <returns>The file extension for the report format</returns>
        public string GetFileExtension()
        {
            switch (Format)
            {
                case 0: // PDF
                    return ".pdf";
                case 1: // Excel
                    return ".xlsx";
                case 2: // CSV
                    return ".csv";
                case 3: // HTML
                    return ".html";
                default:
                    return ".txt";
            }
        }

        /// <summary>
        /// Gets the content type based on the report format
        /// </summary>
        /// <returns>The MIME content type for the report format</returns>
        public string GetContentType()
        {
            switch (Format)
            {
                case 0: // PDF
                    return "application/pdf";
                case 1: // Excel
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case 2: // CSV
                    return "text/csv";
                case 3: // HTML
                    return "text/html";
                default:
                    return "text/plain";
            }
        }

        /// <summary>
        /// Gets a user-friendly name for the report format
        /// </summary>
        /// <returns>The name of the report format</returns>
        public string GetFormatName()
        {
            switch (Format)
            {
                case 0:
                    return "PDF";
                case 1:
                    return "Excel";
                case 2:
                    return "CSV";
                case 3:
                    return "HTML";
                default:
                    return "Unknown";
            }
        }
    }

    /// <summary>
    /// Model for requesting the generation of a new report with specific parameters and content options
    /// </summary>
    public class ReportRequestModel
    {
        /// <summary>
        /// The title of the report
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string ReportTitle { get; set; }

        /// <summary>
        /// The identifier of the calculation that this report is based on
        /// </summary>
        [Required]
        public string CalculationId { get; set; }

        /// <summary>
        /// The format of the report (0=PDF, 1=Excel, 2=CSV, 3=HTML)
        /// </summary>
        [Range(0, 3)]
        public int Format { get; set; }

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
        /// Report delivery options
        /// </summary>
        public ReportDeliveryOptions DeliveryOptions { get; set; }

        /// <summary>
        /// Default constructor for the ReportRequestModel
        /// </summary>
        public ReportRequestModel()
        {
            Format = 0; // PDF by default
            IncludeCountryBreakdown = true;
            IncludeServiceDetails = true;
            IncludeAppliedDiscounts = true;
            IncludeHistoricalComparison = false;
            IncludeTaxRateDetails = false;
            DeliveryOptions = new ReportDeliveryOptions();
        }
    }

    /// <summary>
    /// Specifies how a generated report should be delivered to the user
    /// </summary>
    public class ReportDeliveryOptions
    {
        /// <summary>
        /// Whether the report should be downloaded immediately
        /// </summary>
        public bool DownloadImmediately { get; set; }

        /// <summary>
        /// Whether the report should be sent via email
        /// </summary>
        public bool SendEmail { get; set; }

        /// <summary>
        /// The email address to send the report to if SendEmail is true
        /// </summary>
        [EmailAddress]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Whether the report should be stored for later access
        /// </summary>
        public bool StoreForLater { get; set; }

        /// <summary>
        /// Default constructor for the ReportDeliveryOptions
        /// </summary>
        public ReportDeliveryOptions()
        {
            DownloadImmediately = true;
            SendEmail = false;
            StoreForLater = true;
        }
    }

    /// <summary>
    /// Provides a summary view of a report for listing in the UI
    /// </summary>
    public class ReportSummaryModel
    {
        /// <summary>
        /// The unique identifier for this report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// The title of the report
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// The format of the report (0=PDF, 1=Excel, 2=CSV, 3=HTML)
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        /// The name of the report format
        /// </summary>
        public string FormatName { get; set; }

        /// <summary>
        /// The date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// The size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Indicates if this report has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Formats the file size in a human-readable format (KB, MB, etc.)
        /// </summary>
        /// <returns>Formatted file size with appropriate unit</returns>
        public string GetFormattedSize()
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (FileSize < KB)
                return $"{FileSize} bytes";
            else if (FileSize < MB)
                return $"{FileSize / KB:N1} KB";
            else if (FileSize < GB)
                return $"{FileSize / MB:N1} MB";
            else
                return $"{FileSize / GB:N1} GB";
        }
    }

    /// <summary>
    /// Represents a paginated list of report summaries for display in the UI
    /// </summary>
    public class ReportListModel
    {
        /// <summary>
        /// The list of report summaries for the current page
        /// </summary>
        public List<ReportSummaryModel> Items { get; set; }

        /// <summary>
        /// The current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The total number of items across all pages
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Default constructor for the ReportListModel
        /// </summary>
        public ReportListModel()
        {
            Items = new List<ReportSummaryModel>();
            PageNumber = 1;
            TotalPages = 1;
            TotalItems = 0;
        }

        /// <summary>
        /// Determines if there is a previous page of results
        /// </summary>
        /// <returns>True if there is a previous page, otherwise false</returns>
        public bool HasPreviousPage()
        {
            return PageNumber > 1;
        }

        /// <summary>
        /// Determines if there is a next page of results
        /// </summary>
        /// <returns>True if there is a next page, otherwise false</returns>
        public bool HasNextPage()
        {
            return PageNumber < TotalPages;
        }
    }

    /// <summary>
    /// Provides filtering criteria for retrieving reports
    /// </summary>
    public class ReportFilterModel
    {
        /// <summary>
        /// The start date for filtering reports by generation date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// The end date for filtering reports by generation date
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The format to filter reports by (0=PDF, 1=Excel, 2=CSV, 3=HTML)
        /// </summary>
        public int? Format { get; set; }

        /// <summary>
        /// Whether to include archived reports
        /// </summary>
        public bool? IsArchived { get; set; }

        /// <summary>
        /// The page number for pagination (1-based)
        /// </summary>
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items per page
        /// </summary>
        [Range(1, 100)]
        public int PageSize { get; set; }

        /// <summary>
        /// The field to sort by
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Whether to sort in descending order
        /// </summary>
        public bool SortDescending { get; set; }

        /// <summary>
        /// Default constructor for the ReportFilterModel
        /// </summary>
        public ReportFilterModel()
        {
            PageNumber = 1;
            PageSize = 10;
            SortBy = "GenerationDate";
            SortDescending = true;
        }

        /// <summary>
        /// Converts the filter to a query string for API requests
        /// </summary>
        /// <returns>Query string representation of the filter</returns>
        public string ToQueryString()
        {
            var parameters = new List<string>();

            if (StartDate.HasValue)
                parameters.Add($"startDate={StartDate.Value:yyyy-MM-dd}");
                
            if (EndDate.HasValue)
                parameters.Add($"endDate={EndDate.Value:yyyy-MM-dd}");
                
            if (Format.HasValue)
                parameters.Add($"format={Format.Value}");
                
            if (IsArchived.HasValue)
                parameters.Add($"isArchived={IsArchived.Value}");
                
            parameters.Add($"pageNumber={PageNumber}");
            parameters.Add($"pageSize={PageSize}");
            parameters.Add($"sortBy={SortBy}");
            parameters.Add($"sortDescending={SortDescending}");

            return string.Join("&", parameters);
        }
    }

    /// <summary>
    /// Represents a selectable report format option for the UI
    /// </summary>
    public class ReportFormatOption
    {
        /// <summary>
        /// The value of the format option
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The display text for the format option
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The icon to display for the format option
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Constructor for the ReportFormatOption
        /// </summary>
        /// <param name="value">The value of the format</param>
        /// <param name="text">The display text for the format</param>
        /// <param name="icon">The icon for the format</param>
        public ReportFormatOption(int value, string text, string icon)
        {
            Value = value;
            Text = text;
            Icon = icon;
        }
    }

    /// <summary>
    /// Model for requesting to email a report to a specific address
    /// </summary>
    public class EmailReportRequestModel
    {
        /// <summary>
        /// The unique identifier of the report to email
        /// </summary>
        [Required]
        public string ReportId { get; set; }

        /// <summary>
        /// The email address to send the report to
        /// </summary>
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        /// <summary>
        /// The subject line for the email
        /// </summary>
        [StringLength(200)]
        public string Subject { get; set; }

        /// <summary>
        /// Additional message to include in the email body
        /// </summary>
        [StringLength(1000)]
        public string Message { get; set; }
    }
}