using System; // version 6.0.0
using System.Collections.Generic; // version 6.0.0
using System.Linq; // version 6.0.0
using VatFilingPricingTool.Common.Models.ApiResponse;
using VatFilingPricingTool.Domain.Entities.Integration;
using VatFilingPricingTool.Infrastructure.Integration.ERP.DynamicsConnector;
using VatFilingPricingTool.Infrastructure.Integration.OCR.AzureCognitiveServicesClient;

namespace VatFilingPricingTool.Api.Models.Responses
{
    /// <summary>
    /// Response model for a single integration configuration
    /// </summary>
    public class ApiIntegrationResponse
    {
        /// <summary>
        /// The unique identifier for the integration
        /// </summary>
        public string IntegrationId { get; set; }
        
        /// <summary>
        /// The identifier of the user who owns this integration
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// The type of external system (e.g., "Dynamics365", "CognitiveServices")
        /// </summary>
        public string SystemType { get; set; }
        
        /// <summary>
        /// The connection string used to connect to the external system
        /// </summary>
        public string ConnectionString { get; set; }
        
        /// <summary>
        /// The date and time of the last successful synchronization
        /// </summary>
        public DateTime LastSyncDate { get; set; }
        
        /// <summary>
        /// Indicates whether this integration is currently active
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// The API key used for authentication with the external system
        /// </summary>
        public string ApiKey { get; set; }
        
        /// <summary>
        /// The API endpoint URL for the external system
        /// </summary>
        public string ApiEndpoint { get; set; }
        
        /// <summary>
        /// Additional configuration settings for this integration
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; set; }

        /// <summary>
        /// Default constructor for ApiIntegrationResponse
        /// </summary>
        public ApiIntegrationResponse()
        {
            AdditionalSettings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates an ApiIntegrationResponse from an Integration entity
        /// </summary>
        /// <param name="integration">The Integration entity to convert</param>
        /// <returns>An ApiIntegrationResponse populated with the integration data</returns>
        public static ApiIntegrationResponse FromEntity(Integration integration)
        {
            if (integration == null)
                throw new ArgumentNullException(nameof(integration));

            return new ApiIntegrationResponse
            {
                IntegrationId = integration.IntegrationId,
                UserId = integration.UserId,
                SystemType = integration.SystemType,
                ConnectionString = MaskSensitiveData(integration.ConnectionString),
                LastSyncDate = integration.LastSyncDate,
                IsActive = integration.IsActive,
                ApiKey = MaskSensitiveData(integration.ApiKey),
                ApiEndpoint = integration.ApiEndpoint,
                AdditionalSettings = integration.AdditionalSettings ?? new Dictionary<string, string>()
            };
        }

        /// <summary>
        /// Masks sensitive data by showing only the first and last characters
        /// </summary>
        /// <param name="sensitiveData">The sensitive data to mask</param>
        /// <returns>Masked string or null if input is null</returns>
        private static string MaskSensitiveData(string sensitiveData)
        {
            if (string.IsNullOrEmpty(sensitiveData))
                return sensitiveData;

            if (sensitiveData.Length <= 8)
                return "********";

            // Show first and last 4 characters
            return $"{sensitiveData.Substring(0, 4)}********{sensitiveData.Substring(sensitiveData.Length - 4)}";
        }

        /// <summary>
        /// Converts the ApiIntegrationResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiIntegrationResponse> ToApiResponse()
        {
            return ApiResponse<ApiIntegrationResponse>.CreateSuccess(this);
        }
    }

    /// <summary>
    /// Response model for a list of integration configurations
    /// </summary>
    public class ApiIntegrationListResponse
    {
        /// <summary>
        /// List of integration configurations
        /// </summary>
        public List<ApiIntegrationResponse> Integrations { get; set; }
        
        /// <summary>
        /// Total count of integrations
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Default constructor for ApiIntegrationListResponse
        /// </summary>
        public ApiIntegrationListResponse()
        {
            Integrations = new List<ApiIntegrationResponse>();
            TotalCount = 0;
        }

        /// <summary>
        /// Creates an ApiIntegrationListResponse from a collection of Integration entities
        /// </summary>
        /// <param name="integrations">The collection of Integration entities</param>
        /// <returns>An ApiIntegrationListResponse populated with integration data</returns>
        public static ApiIntegrationListResponse FromEntityList(IEnumerable<Integration> integrations)
        {
            if (integrations == null)
                throw new ArgumentNullException(nameof(integrations));

            var response = new ApiIntegrationListResponse
            {
                Integrations = integrations.Select(ApiIntegrationResponse.FromEntity).ToList(),
                TotalCount = integrations.Count()
            };

            return response;
        }

        /// <summary>
        /// Converts the ApiIntegrationListResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiIntegrationListResponse> ToApiResponse()
        {
            return ApiResponse<ApiIntegrationListResponse>.CreateSuccess(this);
        }
    }

    /// <summary>
    /// Response model for connection test results
    /// </summary>
    public class ApiConnectionTestResponse
    {
        /// <summary>
        /// Indicates whether the connection test was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Message describing the connection test result
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The integration ID that was tested
        /// </summary>
        public string IntegrationId { get; set; }
        
        /// <summary>
        /// The system type that was tested
        /// </summary>
        public string SystemType { get; set; }
        
        /// <summary>
        /// The time when the test was performed
        /// </summary>
        public DateTime TestTime { get; set; }

        /// <summary>
        /// Default constructor for ApiConnectionTestResponse
        /// </summary>
        public ApiConnectionTestResponse()
        {
            TestTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful connection test response
        /// </summary>
        /// <param name="integrationId">The integration ID that was tested</param>
        /// <param name="systemType">The system type that was tested</param>
        /// <returns>A successful connection test response</returns>
        public static ApiConnectionTestResponse CreateSuccess(string integrationId, string systemType)
        {
            return new ApiConnectionTestResponse
            {
                IsSuccess = true,
                Message = "Connection test successful",
                IntegrationId = integrationId,
                SystemType = systemType,
                TestTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Creates a failed connection test response
        /// </summary>
        /// <param name="integrationId">The integration ID that was tested</param>
        /// <param name="systemType">The system type that was tested</param>
        /// <param name="errorMessage">The error message describing why the connection failed</param>
        /// <returns>A failed connection test response</returns>
        public static ApiConnectionTestResponse CreateFailure(string integrationId, string systemType, string errorMessage)
        {
            return new ApiConnectionTestResponse
            {
                IsSuccess = false,
                Message = errorMessage,
                IntegrationId = integrationId,
                SystemType = systemType,
                TestTime = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Converts the ApiConnectionTestResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiConnectionTestResponse> ToApiResponse()
        {
            return ApiResponse<ApiConnectionTestResponse>.CreateSuccess(this);
        }
    }

    /// <summary>
    /// Response model for data import operation results
    /// </summary>
    public class ApiImportDataResponse
    {
        /// <summary>
        /// Indicates whether the import operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Error message if the import operation failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Number of records imported
        /// </summary>
        public int RecordCount { get; set; }
        
        /// <summary>
        /// Type of entity that was imported (e.g., "Invoice", "Transaction")
        /// </summary>
        public string EntityType { get; set; }
        
        /// <summary>
        /// Date and time when the import was performed
        /// </summary>
        public DateTime ImportDate { get; set; }
        
        /// <summary>
        /// Sample of the imported records for preview purposes
        /// </summary>
        public List<Dictionary<string, object>> SampleRecords { get; set; }

        /// <summary>
        /// Default constructor for ApiImportDataResponse
        /// </summary>
        public ApiImportDataResponse()
        {
            SampleRecords = new List<Dictionary<string, object>>();
            ImportDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an ApiImportDataResponse from an ImportResult
        /// </summary>
        /// <param name="result">The ImportResult to convert</param>
        /// <returns>An ApiImportDataResponse populated with the import result data</returns>
        public static ApiImportDataResponse FromImportResult(ImportResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var response = new ApiImportDataResponse
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                RecordCount = result.TotalCount,
                EntityType = result.EntityType,
                ImportDate = result.ImportDate
            };

            // Add up to 5 sample records for preview
            if (result.Records != null && result.Records.Any())
            {
                response.SampleRecords = result.Records
                    .Take(5)
                    .Select(r => new Dictionary<string, object>(r.AdditionalData))
                    .ToList();
            }

            return response;
        }

        /// <summary>
        /// Converts the ApiImportDataResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiImportDataResponse> ToApiResponse()
        {
            return ApiResponse<ApiImportDataResponse>.CreateSuccess(this);
        }
    }

    /// <summary>
    /// Response model for document processing operation results
    /// </summary>
    public class ApiDocumentProcessingResponse
    {
        /// <summary>
        /// Indicates whether the document processing operation was successful
        /// </summary>
        public bool IsSuccess { get; set; }
        
        /// <summary>
        /// Error message if the document processing operation failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Data extracted from the document
        /// </summary>
        public Dictionary<string, string> ExtractedData { get; set; }
        
        /// <summary>
        /// Confidence score of the extraction (0.0 to 1.0)
        /// </summary>
        public double ConfidenceScore { get; set; }
        
        /// <summary>
        /// Type of document that was processed
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Date and time when the document was processed
        /// </summary>
        public DateTime ProcessingTime { get; set; }

        /// <summary>
        /// Default constructor for ApiDocumentProcessingResponse
        /// </summary>
        public ApiDocumentProcessingResponse()
        {
            ExtractedData = new Dictionary<string, string>();
            ProcessingTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates an ApiDocumentProcessingResponse from a DocumentProcessingResult
        /// </summary>
        /// <param name="result">The DocumentProcessingResult to convert</param>
        /// <returns>An ApiDocumentProcessingResponse populated with the document processing result data</returns>
        public static ApiDocumentProcessingResponse FromProcessingResult(DocumentProcessingResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            return new ApiDocumentProcessingResponse
            {
                IsSuccess = result.IsSuccess,
                ErrorMessage = result.ErrorMessage,
                ExtractedData = result.ExtractedData ?? new Dictionary<string, string>(),
                ConfidenceScore = result.ConfidenceScore,
                DocumentType = result.DocumentType,
                ProcessingTime = result.ProcessingTime
            };
        }

        /// <summary>
        /// Converts the ApiDocumentProcessingResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiDocumentProcessingResponse> ToApiResponse()
        {
            return ApiResponse<ApiDocumentProcessingResponse>.CreateSuccess(this);
        }
    }

    /// <summary>
    /// Response model for available system types that can be integrated
    /// </summary>
    public class ApiSystemTypesResponse
    {
        /// <summary>
        /// List of available system types for integration
        /// </summary>
        public List<string> SystemTypes { get; set; }

        /// <summary>
        /// Default constructor for ApiSystemTypesResponse
        /// </summary>
        public ApiSystemTypesResponse()
        {
            SystemTypes = new List<string>();
        }

        /// <summary>
        /// Creates an ApiSystemTypesResponse from a collection of system types
        /// </summary>
        /// <param name="systemTypes">The collection of system types</param>
        /// <returns>An ApiSystemTypesResponse populated with system types</returns>
        public static ApiSystemTypesResponse FromSystemTypes(IEnumerable<string> systemTypes)
        {
            if (systemTypes == null)
                throw new ArgumentNullException(nameof(systemTypes));

            return new ApiSystemTypesResponse
            {
                SystemTypes = systemTypes.ToList()
            };
        }

        /// <summary>
        /// Converts the ApiSystemTypesResponse to a generic ApiResponse
        /// </summary>
        /// <returns>The wrapped API response</returns>
        public ApiResponse<ApiSystemTypesResponse> ToApiResponse()
        {
            return ApiResponse<ApiSystemTypesResponse>.CreateSuccess(this);
        }
    }
}