using System; // System version 6.0.0
using System.Collections.Generic; // System.Collections.Generic version 6.0.0
using System.ComponentModel.DataAnnotations; // System.ComponentModel.DataAnnotations version 6.0.0

namespace VatFilingPricingTool.Api.Models.Requests
{
    /// <summary>
    /// Request model for creating a new integration with an external system
    /// </summary>
    public class CreateIntegrationRequest
    {
        /// <summary>
        /// Type of external system (e.g., "Dynamics365", "CognitiveServices")
        /// </summary>
        [Required(ErrorMessage = "System type is required")]
        [StringLength(50, ErrorMessage = "System type cannot exceed 50 characters")]
        public string SystemType { get; set; }

        /// <summary>
        /// Connection string for the external system
        /// </summary>
        [StringLength(500, ErrorMessage = "Connection string cannot exceed 500 characters")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// API key for authentication with the external system
        /// </summary>
        [StringLength(100, ErrorMessage = "API key cannot exceed 100 characters")]
        public string ApiKey { get; set; }

        /// <summary>
        /// API endpoint URL for the external system
        /// </summary>
        [StringLength(255, ErrorMessage = "API endpoint cannot exceed 255 characters")]
        [Url(ErrorMessage = "API endpoint must be a valid URL")]
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// Additional configuration settings for the integration
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; }

        /// <summary>
        /// Default constructor for CreateIntegrationRequest
        /// </summary>
        public CreateIntegrationRequest()
        {
            AdditionalSettings = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Request model for updating an existing integration configuration
    /// </summary>
    public class UpdateIntegrationRequest
    {
        /// <summary>
        /// Unique identifier for the integration to update
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }

        /// <summary>
        /// Updated connection string for the external system
        /// </summary>
        [StringLength(500, ErrorMessage = "Connection string cannot exceed 500 characters")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Updated API key for authentication with the external system
        /// </summary>
        [StringLength(100, ErrorMessage = "API key cannot exceed 100 characters")]
        public string ApiKey { get; set; }

        /// <summary>
        /// Updated API endpoint URL for the external system
        /// </summary>
        [StringLength(255, ErrorMessage = "API endpoint cannot exceed 255 characters")]
        [Url(ErrorMessage = "API endpoint must be a valid URL")]
        public string ApiEndpoint { get; set; }

        /// <summary>
        /// Updated additional configuration settings for the integration
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; }

        /// <summary>
        /// Default constructor for UpdateIntegrationRequest
        /// </summary>
        public UpdateIntegrationRequest()
        {
            AdditionalSettings = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Request model for testing a connection to an external system
    /// </summary>
    public class TestConnectionRequest
    {
        /// <summary>
        /// Unique identifier for the integration to test
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }
    }

    /// <summary>
    /// Request model for importing data from an external system like ERP
    /// </summary>
    public class ImportDataRequest
    {
        /// <summary>
        /// Unique identifier for the integration to use for import
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }

        /// <summary>
        /// Start date for data to import
        /// </summary>
        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for data to import
        /// </summary>
        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Type of entity to import (e.g., "Invoices", "Transactions")
        /// </summary>
        [Required(ErrorMessage = "Entity type is required")]
        [StringLength(50, ErrorMessage = "Entity type cannot exceed 50 characters")]
        public string EntityType { get; set; }

        /// <summary>
        /// Optional maximum number of records to import
        /// </summary>
        public int? MaxRecords { get; set; }

        /// <summary>
        /// Additional parameters for the import operation
        /// </summary>
        public Dictionary<string, object> AdditionalParameters { get; set; }

        /// <summary>
        /// Default constructor for ImportDataRequest
        /// </summary>
        public ImportDataRequest()
        {
            AdditionalParameters = new Dictionary<string, object>();
            StartDate = DateTime.UtcNow.AddMonths(-1);
            EndDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Request model for processing a document using OCR services
    /// </summary>
    public class ProcessDocumentRequest
    {
        /// <summary>
        /// Unique identifier for the integration to use for document processing
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }

        /// <summary>
        /// URL to the document for processing
        /// </summary>
        [Required(ErrorMessage = "Document URL is required")]
        [Url(ErrorMessage = "Document URL must be a valid URL")]
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Type of document to process (e.g., "Invoice", "VATForm")
        /// </summary>
        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot exceed 50 characters")]
        public string DocumentType { get; set; }

        /// <summary>
        /// Document language code (e.g., "en", "de", "fr")
        /// </summary>
        [StringLength(10, ErrorMessage = "Language code cannot exceed 10 characters")]
        public string Language { get; set; }

        /// <summary>
        /// Specific fields to extract from the document
        /// </summary>
        public List<string> ExtractFields { get; set; }

        /// <summary>
        /// Minimum confidence threshold for extracted data (0.0 to 1.0)
        /// </summary>
        [Range(0, 1, ErrorMessage = "Minimum confidence must be between 0 and 1")]
        public double? MinimumConfidence { get; set; }

        /// <summary>
        /// Additional options for document processing
        /// </summary>
        public Dictionary<string, object> AdditionalOptions { get; set; }

        /// <summary>
        /// Default constructor for ProcessDocumentRequest
        /// </summary>
        public ProcessDocumentRequest()
        {
            ExtractFields = new List<string>();
            AdditionalOptions = new Dictionary<string, object>();
            Language = "en";
            MinimumConfidence = 0.7;
        }
    }

    /// <summary>
    /// Request model for retrieving a specific integration by ID
    /// </summary>
    public class GetIntegrationRequest
    {
        /// <summary>
        /// Unique identifier for the integration to retrieve
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }
    }

    /// <summary>
    /// Request model for retrieving all integrations for a specific user
    /// </summary>
    public class GetUserIntegrationsRequest
    {
        /// <summary>
        /// Unique identifier for the user whose integrations should be retrieved
        /// </summary>
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }
    }

    /// <summary>
    /// Request model for deleting an integration configuration
    /// </summary>
    public class DeleteIntegrationRequest
    {
        /// <summary>
        /// Unique identifier for the integration to delete
        /// </summary>
        [Required(ErrorMessage = "Integration ID is required")]
        public string IntegrationId { get; set; }
    }
}