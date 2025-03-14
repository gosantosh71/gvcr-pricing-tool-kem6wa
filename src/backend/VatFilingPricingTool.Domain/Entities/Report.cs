using System;
using System.Collections.Generic;
using System.IO;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a report generated from VAT filing cost calculations with metadata about the report content and storage.
    /// The actual report content is stored in Azure Blob Storage while this entity holds metadata and reference information.
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Gets or sets the unique identifier for the report.
        /// </summary>
        public string ReportId { get; set; }

        /// <summary>
        /// Gets or sets the user ID who owns this report.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the calculation ID this report is based on.
        /// </summary>
        public string CalculationId { get; set; }

        /// <summary>
        /// Gets or sets the title of the report.
        /// </summary>
        public string ReportTitle { get; set; }

        /// <summary>
        /// Gets or sets the report type (e.g., "Cost Summary", "Detailed Breakdown", "Comparison Report").
        /// </summary>
        public string ReportType { get; set; }

        /// <summary>
        /// Gets or sets the format of the report (PDF, Excel, CSV, HTML).
        /// </summary>
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
        /// Gets or sets the file size of the report in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the report is archived.
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Gets or sets the user associated with this report.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the calculation associated with this report.
        /// </summary>
        public Calculation Calculation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Report"/> class.
        /// </summary>
        public Report()
        {
            GenerationDate = DateTime.UtcNow;
            Format = ReportFormat.PDF; // Default format
            IsArchived = false;
        }

        /// <summary>
        /// Creates a new report instance with the specified parameters.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the report.</param>
        /// <param name="calculationId">The calculation ID that this report is based on.</param>
        /// <param name="reportTitle">The title of the report.</param>
        /// <param name="reportType">The type of report.</param>
        /// <param name="format">The format of the report.</param>
        /// <returns>A new Report instance.</returns>
        public static Report Create(string userId, string calculationId, string reportTitle, string reportType, ReportFormat format)
        {
            // Validate parameters
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(userId))
                errors.Add("User ID cannot be null or empty.");
                
            if (string.IsNullOrEmpty(calculationId))
                errors.Add("Calculation ID cannot be null or empty.");
                
            if (string.IsNullOrEmpty(reportTitle))
                errors.Add("Report title cannot be null or empty.");
                
            if (string.IsNullOrEmpty(reportType))
                errors.Add("Report type cannot be null or empty.");
                
            if (errors.Count > 0)
                throw new ValidationException("Invalid report parameters", errors);
                
            // Create a new report instance
            var report = new Report
            {
                ReportId = Guid.NewGuid().ToString(),
                UserId = userId,
                CalculationId = calculationId,
                ReportTitle = reportTitle,
                ReportType = reportType,
                Format = format,
                GenerationDate = DateTime.UtcNow,
                IsArchived = false
            };
            
            return report;
        }

        /// <summary>
        /// Updates the storage information for the report after it has been generated and stored.
        /// </summary>
        /// <param name="storageUrl">The URL where the report is stored.</param>
        /// <param name="fileSize">The size of the report file in bytes.</param>
        public void UpdateStorageInfo(string storageUrl, long fileSize)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(storageUrl))
                errors.Add("Storage URL cannot be null or empty.");
                
            if (fileSize <= 0)
                errors.Add("File size must be greater than zero.");
                
            if (errors.Count > 0)
                throw new ValidationException("Invalid storage information", errors);
                
            StorageUrl = storageUrl;
            FileSize = fileSize;
        }

        /// <summary>
        /// Archives the report.
        /// </summary>
        public void Archive()
        {
            IsArchived = true;
        }

        /// <summary>
        /// Unarchives the report.
        /// </summary>
        public void Unarchive()
        {
            IsArchived = false;
        }

        /// <summary>
        /// Updates the report title.
        /// </summary>
        /// <param name="newTitle">The new title for the report.</param>
        public void UpdateTitle(string newTitle)
        {
            if (string.IsNullOrEmpty(newTitle))
                throw new ValidationException("Report title cannot be null or empty.");
                
            ReportTitle = newTitle;
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
            // Sanitize the report title to make it safe for use in a filename
            string safeTitle = ReportTitle;
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                safeTitle = safeTitle.Replace(c, '_');
            }
            
            // Replace spaces with underscores and remove any other potentially problematic characters
            safeTitle = safeTitle.Replace(' ', '_').Replace('.', '_').Replace(',', '_');
            
            return $"{safeTitle}{GetFileExtension()}";
        }

        /// <summary>
        /// Validates the report data.
        /// </summary>
        private void Validate()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(UserId))
                errors.Add("User ID cannot be null or empty.");
                
            if (string.IsNullOrEmpty(CalculationId))
                errors.Add("Calculation ID cannot be null or empty.");
                
            if (string.IsNullOrEmpty(ReportTitle))
                errors.Add("Report title cannot be null or empty.");
                
            if (string.IsNullOrEmpty(ReportType))
                errors.Add("Report type cannot be null or empty.");
                
            if (errors.Count > 0)
                throw new ValidationException("Report validation failed", errors);
        }
    }
}