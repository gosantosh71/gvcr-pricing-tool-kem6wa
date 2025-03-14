using System;
using System.Collections.Generic;

namespace VatFilingPricingTool.Infrastructure.Storage
{
    /// <summary>
    /// Configuration options for Azure Blob Storage connection and behavior.
    /// Controls storage connections, container names, and file handling settings
    /// for storing and retrieving reports, templates, and documents.
    /// </summary>
    public class StorageOptions
    {
        /// <summary>
        /// Gets or sets the Azure Storage connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the name of the blob container used for storing reports.
        /// </summary>
        public string ReportsContainerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the blob container used for storing templates.
        /// </summary>
        public string TemplatesContainerName { get; set; }

        /// <summary>
        /// Gets or sets the name of the blob container used for storing documents.
        /// </summary>
        public string DocumentsContainerName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically create
        /// containers if they don't exist.
        /// </summary>
        public bool CreateContainersIfNotExist { get; set; }

        /// <summary>
        /// Gets or sets the SAS token expiration time in hours.
        /// Used for generating limited-time access URLs to blobs.
        /// </summary>
        public int SasTokenExpirationHours { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed file extensions for upload.
        /// Used for security validation before storing files.
        /// </summary>
        public List<string> AllowedFileExtensions { get; set; }

        /// <summary>
        /// Initializes a new instance of the StorageOptions class with default values.
        /// </summary>
        public StorageOptions()
        {
            // Default container names
            ReportsContainerName = "reports";
            TemplatesContainerName = "templates";
            DocumentsContainerName = "documents";
            
            // Default to creating containers automatically
            CreateContainersIfNotExist = true;
            
            // Default SAS token expiration (24 hours)
            SasTokenExpirationHours = 24;
            
            // Initialize with common document types
            AllowedFileExtensions = new List<string>
            {
                ".pdf",
                ".xlsx",
                ".docx",
                ".csv",
                ".jpg",
                ".png"
            };
        }
    }
}