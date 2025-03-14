using System; // Version: 6.0.0 - Core .NET functionality
using System.ComponentModel.DataAnnotations; // Version: 6.0.0 - For validation attributes on model properties
using System.Text.Json.Serialization; // Version: 6.0.0 - For JSON serialization attributes
using VatFilingPricingTool.Domain.Enums; // Defines the format of the report (PDF, Excel, CSV, HTML)

namespace VatFilingPricingTool.Contracts.V1.Models
{
    /// <summary>
    /// Represents a report generated from VAT filing cost calculations with metadata about the report content and storage
    /// </summary>
    public class ReportModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report.
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who generated the report.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the calculation that the report is based on.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the title of the report.
        /// </summary>
        [StringLength(200, ErrorMessage = "Report title cannot exceed 200 characters")]
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets the type or category of the report.
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets or sets the format of the report (PDF, Excel, CSV, HTML).
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReportFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the URL where the report is stored in blob storage.
        /// </summary>
        public string StorageUrl { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the report was generated.
        /// </summary>
        public DateTime GenerationDate { get; set; }

        /// <summary>
        /// Gets or sets the size of the report file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include country breakdown in the report.
        /// </summary>
        public bool IncludeCountryBreakdown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include service details in the report.
        /// </summary>
        public bool IncludeServiceDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include applied discounts in the report.
        /// </summary>
        public bool IncludeAppliedDiscounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include historical comparison in the report.
        /// </summary>
        public bool IncludeHistoricalComparison { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include tax rate details in the report.
        /// </summary>
        public bool IncludeTaxRateDetails { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this report has been archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportModel"/> class.
        /// </summary>
        public ReportModel()
        {
            GenerationDate = DateTime.UtcNow;
            Format = ReportFormat.PDF;
            IncludeCountryBreakdown = true;
            IncludeServiceDetails = true;
            IncludeAppliedDiscounts = true;
            IncludeHistoricalComparison = false;
            IncludeTaxRateDetails = false;
            IsArchived = false;
        }

        /// <summary>
        /// Creates a ReportModel from a domain Report entity.
        /// </summary>
        /// <param name="entity">The domain Report entity.</param>
        /// <returns>A new ReportModel populated from the entity.</returns>
        /// <exception cref="ArgumentNullException">Thrown if entity is null.</exception>
        public static ReportModel FromEntity(Report entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var model = new ReportModel
            {
                ReportId = entity.ReportId,
                UserId = entity.UserId,
                CalculationId = entity.CalculationId,
                ReportTitle = entity.ReportTitle,
                ReportType = entity.ReportType,
                Format = entity.Format,
                StorageUrl = entity.StorageUrl,
                GenerationDate = entity.GenerationDate,
                FileSize = entity.FileSize,
                IsArchived = entity.IsArchived
            };

            // Parse report type to determine included sections
            if (entity.ReportType.Contains("CountryBreakdown"))
                model.IncludeCountryBreakdown = true;
            
            if (entity.ReportType.Contains("ServiceDetails"))
                model.IncludeServiceDetails = true;
            
            if (entity.ReportType.Contains("AppliedDiscounts"))
                model.IncludeAppliedDiscounts = true;
            
            if (entity.ReportType.Contains("HistoricalComparison"))
                model.IncludeHistoricalComparison = true;
            
            if (entity.ReportType.Contains("TaxRateDetails"))
                model.IncludeTaxRateDetails = true;

            return model;
        }

        /// <summary>
        /// Gets the file extension based on the report format.
        /// </summary>
        /// <returns>The file extension for the report format.</returns>
        public string GetFileExtension()
        {
            return Format switch
            {
                ReportFormat.PDF => ".pdf",
                ReportFormat.Excel => ".xlsx",
                ReportFormat.CSV => ".csv",
                ReportFormat.HTML => ".html",
                _ => ".txt"
            };
        }

        /// <summary>
        /// Gets the content type based on the report format.
        /// </summary>
        /// <returns>The MIME content type for the report format.</returns>
        public string GetContentType()
        {
            return Format switch
            {
                ReportFormat.PDF => "application/pdf",
                ReportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ReportFormat.CSV => "text/csv",
                ReportFormat.HTML => "text/html",
                _ => "text/plain"
            };
        }

        /// <summary>
        /// Generates a filename for the report based on title and format.
        /// </summary>
        /// <returns>A formatted filename with appropriate extension.</returns>
        public string GetFileName()
        {
            // Create a sanitized version of the report title by removing invalid filename characters
            var sanitizedTitle = string.Join("_", ReportTitle.Split(System.IO.Path.GetInvalidFileNameChars()));
            
            // Get the appropriate file extension
            var extension = GetFileExtension();
            
            // Combine the sanitized title with the extension
            return $"{sanitizedTitle}{extension}";
        }
    }
}