using System; // Version: 6.0.0
using System.Collections.Generic; // Version: 6.0.0
using System.Text.Json.Serialization; // Version: 6.0.0
using VatFilingPricingTool.Common.Models; // Provides standardized API response structure and pagination
using VatFilingPricingTool.Contracts.V1.Models; // Provides the data model for report information
using VatFilingPricingTool.Domain.Enums; // Defines the available formats for report generation and export

namespace VatFilingPricingTool.Contracts.V1.Responses
{
    /// <summary>
    /// Response model for report generation operations
    /// </summary>
    public class GenerateReportResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the generated report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the descriptive title of the report
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets the format of the generated report (PDF, Excel, CSV, HTML)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the URL where the generated report can be downloaded
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report is ready for download
        /// </summary>
        public bool IsReady { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenerateReportResponse"/> class
        /// </summary>
        public GenerateReportResponse()
        {
            GenerationDate = DateTime.UtcNow;
            IsReady = false;
        }

        /// <summary>
        /// Creates a GenerateReportResponse from a ReportModel
        /// </summary>
        /// <param name="model">The ReportModel to create the response from</param>
        /// <returns>A new GenerateReportResponse populated from the model</returns>
        /// <exception cref="ArgumentNullException">Thrown if model is null</exception>
        public static GenerateReportResponse FromReportModel(ReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new GenerateReportResponse
            {
                ReportId = model.ReportId,
                ReportTitle = model.ReportTitle,
                Format = model.Format,
                DownloadUrl = model.StorageUrl,
                GenerationDate = model.GenerationDate,
                FileSize = model.FileSize,
                IsReady = !string.IsNullOrEmpty(model.StorageUrl)
            };
        }
    }

    /// <summary>
    /// Response model for retrieving a specific report
    /// </summary>
    public class GetReportResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the descriptive title of the report
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the calculation this report is based on
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the format of the report (PDF, Excel, CSV, HTML)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the URL where the report can be downloaded
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report includes country breakdown
        /// </summary>
        public bool IncludeCountryBreakdown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report includes service details
        /// </summary>
        public bool IncludeServiceDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report includes applied discounts
        /// </summary>
        public bool IncludeAppliedDiscounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report includes historical comparison
        /// </summary>
        public bool IncludeHistoricalComparison { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report includes tax rate details
        /// </summary>
        public bool IncludeTaxRateDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this report has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Creates a GetReportResponse from a ReportModel
        /// </summary>
        /// <param name="model">The ReportModel to create the response from</param>
        /// <returns>A new GetReportResponse populated from the model</returns>
        /// <exception cref="ArgumentNullException">Thrown if model is null</exception>
        public static GetReportResponse FromReportModel(ReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new GetReportResponse
            {
                ReportId = model.ReportId,
                ReportTitle = model.ReportTitle,
                CalculationId = model.CalculationId,
                Format = model.Format,
                DownloadUrl = model.StorageUrl,
                GenerationDate = model.GenerationDate,
                FileSize = model.FileSize,
                IncludeCountryBreakdown = model.IncludeCountryBreakdown,
                IncludeServiceDetails = model.IncludeServiceDetails,
                IncludeAppliedDiscounts = model.IncludeAppliedDiscounts,
                IncludeHistoricalComparison = model.IncludeHistoricalComparison,
                IncludeTaxRateDetails = model.IncludeTaxRateDetails,
                IsArchived = model.IsArchived
            };
        }
    }

    /// <summary>
    /// Simplified report model for list display
    /// </summary>
    public class ReportListItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the descriptive title of the report
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets the format of the report (PDF, Excel, CSV, HTML)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this report has been archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Creates a ReportListItem from a ReportModel
        /// </summary>
        /// <param name="model">The ReportModel to create the list item from</param>
        /// <returns>A new ReportListItem populated from the model</returns>
        /// <exception cref="ArgumentNullException">Thrown if model is null</exception>
        public static ReportListItem FromReportModel(ReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new ReportListItem
            {
                ReportId = model.ReportId,
                ReportTitle = model.ReportTitle,
                Format = model.Format,
                GenerationDate = model.GenerationDate,
                FileSize = model.FileSize,
                IsArchived = model.IsArchived
            };
        }
    }

    /// <summary>
    /// Response model for retrieving a paginated list of reports
    /// </summary>
    public class GetReportHistoryResponse
    {
        /// <summary>
        /// Gets or sets the list of reports for the current page
        /// </summary>
        public List<ReportListItem> Reports { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of reports across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GetReportHistoryResponse"/> class
        /// </summary>
        public GetReportHistoryResponse()
        {
            Reports = new List<ReportListItem>();
        }

        /// <summary>
        /// Creates a GetReportHistoryResponse from a PagedList of ReportModels
        /// </summary>
        /// <param name="pagedList">The paged list of ReportModels</param>
        /// <returns>A new GetReportHistoryResponse populated from the paged list</returns>
        /// <exception cref="ArgumentNullException">Thrown if pagedList is null</exception>
        public static GetReportHistoryResponse FromPagedList(PagedList<ReportModel> pagedList)
        {
            if (pagedList == null)
                throw new ArgumentNullException(nameof(pagedList));

            var response = new GetReportHistoryResponse
            {
                PageNumber = pagedList.PageNumber,
                PageSize = pagedList.PageSize,
                TotalCount = pagedList.TotalCount,
                TotalPages = pagedList.TotalPages,
                HasPreviousPage = pagedList.HasPreviousPage,
                HasNextPage = pagedList.HasNextPage,
                Reports = pagedList.Items.ConvertAll(model => ReportListItem.FromReportModel(model))
            };

            return response;
        }
    }

    /// <summary>
    /// Response model for downloading a report
    /// </summary>
    public class DownloadReportResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the name of the report file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the MIME content type of the report file
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the URL where the report can be downloaded
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the format of the report (PDF, Excel, CSV, HTML)
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Creates a DownloadReportResponse from a ReportModel
        /// </summary>
        /// <param name="model">The ReportModel to create the response from</param>
        /// <returns>A new DownloadReportResponse populated from the model</returns>
        /// <exception cref="ArgumentNullException">Thrown if model is null</exception>
        public static DownloadReportResponse FromReportModel(ReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return new DownloadReportResponse
            {
                ReportId = model.ReportId,
                FileName = model.GetFileName(),
                ContentType = model.GetContentType(),
                DownloadUrl = model.StorageUrl,
                FileSize = model.FileSize,
                Format = model.Format
            };
        }
    }

    /// <summary>
    /// Response model for emailing a report
    /// </summary>
    public class EmailReportResponse
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the email address where the report was sent
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email was sent successfully
        /// </summary>
        public bool EmailSent { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the email was sent
        /// </summary>
        public DateTime? SentTime { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailReportResponse"/> class
        /// </summary>
        public EmailReportResponse()
        {
            EmailSent = false;
        }
    }
}