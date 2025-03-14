using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Domain.Enums;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Domain.Exceptions;
using VatFilingPricingTool.Common.Constants;

namespace VatFilingPricingTool.Service.Models
{
    /// <summary>
    /// Service layer model for VAT filing reports with metadata and content specifications
    /// </summary>
    public class ReportModel
    {
        /// <summary>
        /// Gets or sets the report ID
        /// </summary>
        public string ReportId { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID who owns this report
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the calculation ID this report is based on
        /// </summary>
        public string CalculationId { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the report
        /// </summary>
        public string ReportTitle { get; set; }
        
        /// <summary>
        /// Gets or sets the report type
        /// </summary>
        public string ReportType { get; set; }
        
        /// <summary>
        /// Gets or sets the format of the report
        /// </summary>
        public ReportFormat Format { get; set; }
        
        /// <summary>
        /// Gets or sets the URL where the report is stored
        /// </summary>
        public string StorageUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the date and time when the report was generated
        /// </summary>
        public DateTime GenerationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the file size of the report in bytes
        /// </summary>
        public long FileSize { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include country breakdown in the report
        /// </summary>
        public bool IncludeCountryBreakdown { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include service details in the report
        /// </summary>
        public bool IncludeServiceDetails { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include applied discounts in the report
        /// </summary>
        public bool IncludeAppliedDiscounts { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include historical comparison in the report
        /// </summary>
        public bool IncludeHistoricalComparison { get; set; }
        
        /// <summary>
        /// Gets or sets whether to include tax rate details in the report
        /// </summary>
        public bool IncludeTaxRateDetails { get; set; }
        
        /// <summary>
        /// Gets or sets whether the report is archived
        /// </summary>
        public bool IsArchived { get; set; }
        
        /// <summary>
        /// Gets or sets the calculation data used for the report
        /// </summary>
        public CalculationModel CalculationData { get; set; }
        
        /// <summary>
        /// Default constructor for the ReportModel
        /// </summary>
        public ReportModel()
        {
            GenerationDate = DateTime.UtcNow;
            Format = ReportFormat.PDF; // Default format
            IncludeCountryBreakdown = true; // Default to including country breakdown
            IncludeServiceDetails = true; // Default to including service details
            IncludeAppliedDiscounts = true; // Default to including applied discounts
            IncludeHistoricalComparison = false; // Default to not including historical comparison
            IncludeTaxRateDetails = false; // Default to not including tax rate details
            IsArchived = false;
        }
        
        /// <summary>
        /// Creates a ReportModel from a domain Report entity
        /// </summary>
        /// <param name="entity">The Report entity</param>
        /// <param name="calculationData">The calculation data for the report</param>
        /// <returns>A new ReportModel populated from the entity</returns>
        public static ReportModel FromEntity(Report entity, CalculationModel calculationData)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
                
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
                IsArchived = entity.IsArchived,
                CalculationData = calculationData
            };
            
            // Parse the report type to determine what sections to include
            if (!string.IsNullOrEmpty(entity.ReportType))
            {
                model.IncludeCountryBreakdown = entity.ReportType.Contains("Country", StringComparison.OrdinalIgnoreCase);
                model.IncludeServiceDetails = entity.ReportType.Contains("Service", StringComparison.OrdinalIgnoreCase);
                model.IncludeAppliedDiscounts = entity.ReportType.Contains("Discount", StringComparison.OrdinalIgnoreCase);
                model.IncludeHistoricalComparison = entity.ReportType.Contains("Historical", StringComparison.OrdinalIgnoreCase);
                model.IncludeTaxRateDetails = entity.ReportType.Contains("Tax", StringComparison.OrdinalIgnoreCase);
            }
            
            return model;
        }
        
        /// <summary>
        /// Converts the ReportModel to a domain Report entity
        /// </summary>
        /// <returns>A new Report entity populated from this model</returns>
        public Report ToEntity()
        {
            // Validate the model first
            Validate();
            
            // Create a new Report entity using the factory method
            var entity = Report.Create(
                UserId,
                CalculationId,
                ReportTitle,
                ReportType,
                Format
            );
            
            // If we have storage info, update it
            if (!string.IsNullOrEmpty(StorageUrl) && FileSize > 0)
            {
                entity.UpdateStorageInfo(StorageUrl, FileSize);
            }
            
            // Set archived status if needed
            if (IsArchived)
            {
                entity.Archive();
            }
            
            return entity;
        }
        
        /// <summary>
        /// Converts the service ReportModel to a contract ReportModel for API responses
        /// </summary>
        /// <returns>A contract model populated from this service model</returns>
        public Contracts.V1.Models.ReportModel ToContractModel()
        {
            var contractModel = new Contracts.V1.Models.ReportModel
            {
                ReportId = this.ReportId,
                UserId = this.UserId,
                CalculationId = this.CalculationId,
                ReportTitle = this.ReportTitle,
                ReportType = this.ReportType,
                Format = (int)this.Format,
                StorageUrl = this.StorageUrl,
                GenerationDate = this.GenerationDate,
                FileSize = this.FileSize,
                IncludeCountryBreakdown = this.IncludeCountryBreakdown,
                IncludeServiceDetails = this.IncludeServiceDetails,
                IncludeAppliedDiscounts = this.IncludeAppliedDiscounts,
                IncludeHistoricalComparison = this.IncludeHistoricalComparison,
                IncludeTaxRateDetails = this.IncludeTaxRateDetails,
                IsArchived = this.IsArchived
            };
            
            return contractModel;
        }
        
        /// <summary>
        /// Creates a service ReportModel from a contract ReportModel
        /// </summary>
        /// <param name="contractModel">The contract model to convert</param>
        /// <param name="calculationData">The calculation data for the report</param>
        /// <returns>A service model populated from the contract model</returns>
        public static ReportModel FromContractModel(Contracts.V1.Models.ReportModel contractModel, CalculationModel calculationData)
        {
            if (contractModel == null)
                throw new ArgumentNullException(nameof(contractModel));
                
            var serviceModel = new ReportModel
            {
                ReportId = contractModel.ReportId,
                UserId = contractModel.UserId,
                CalculationId = contractModel.CalculationId,
                ReportTitle = contractModel.ReportTitle,
                ReportType = contractModel.ReportType,
                Format = (ReportFormat)contractModel.Format,
                StorageUrl = contractModel.StorageUrl,
                GenerationDate = contractModel.GenerationDate,
                FileSize = contractModel.FileSize,
                IncludeCountryBreakdown = contractModel.IncludeCountryBreakdown,
                IncludeServiceDetails = contractModel.IncludeServiceDetails,
                IncludeAppliedDiscounts = contractModel.IncludeAppliedDiscounts,
                IncludeHistoricalComparison = contractModel.IncludeHistoricalComparison,
                IncludeTaxRateDetails = contractModel.IncludeTaxRateDetails,
                IsArchived = contractModel.IsArchived,
                CalculationData = calculationData
            };
            
            return serviceModel;
        }
        
        /// <summary>
        /// Gets the file extension based on the report format
        /// </summary>
        /// <returns>The file extension for the report format</returns>
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
        /// Gets the content type based on the report format
        /// </summary>
        /// <returns>The MIME content type for the report format</returns>
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
        /// Generates a filename for the report based on title and format
        /// </summary>
        /// <returns>A formatted filename with appropriate extension</returns>
        public string GetFileName()
        {
            string sanitizedTitle = SanitizeFileName(ReportTitle);
            string extension = GetFileExtension();
            return $"{sanitizedTitle}{extension}";
        }
        
        /// <summary>
        /// Validates the report model to ensure it contains all required data
        /// </summary>
        public void Validate()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(ReportTitle))
                errors.Add("Report title is required");
                
            if (string.IsNullOrEmpty(CalculationId))
                errors.Add("Calculation ID is required");
                
            if (CalculationData == null)
                errors.Add("Calculation data is required");
                
            if (errors.Count > 0)
                throw new ValidationException("Report model validation failed", errors);
        }
        
        /// <summary>
        /// Updates the storage information for the report after it has been generated and stored
        /// </summary>
        /// <param name="storageUrl">The URL where the report is stored</param>
        /// <param name="fileSize">The size of the report file in bytes</param>
        public void SetStorageDetails(string storageUrl, long fileSize)
        {
            if (string.IsNullOrEmpty(storageUrl))
                throw new ArgumentException("Storage URL cannot be null or empty", nameof(storageUrl));
                
            if (fileSize <= 0)
                throw new ArgumentException("File size must be greater than zero", nameof(fileSize));
                
            StorageUrl = storageUrl;
            FileSize = fileSize;
        }
        
        /// <summary>
        /// Removes invalid characters from a filename
        /// </summary>
        /// <param name="fileName">The filename to sanitize</param>
        /// <returns>The sanitized filename</returns>
        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Report";
                
            // Replace invalid filename characters with underscore
            string invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"[{0}]", invalidChars);
            
            string sanitized = Regex.Replace(fileName, invalidRegStr, "_");
            
            // Trim whitespace
            return sanitized.Trim();
        }
    }
}