using Microsoft.Extensions.Options; // version 6.0.0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Integration.OCR;
using VatFilingPricingTool.Infrastructure.Logging;
using VatFilingPricingTool.Infrastructure.Resilience;
using VatFilingPricingTool.Infrastructure.Storage;

namespace VatFilingPricingTool.Infrastructure.Integration.OCR
{
    /// <summary>
    /// Interface defining the contract for OCR processing operations
    /// </summary>
    public interface IOcrProcessor
    {
        /// <summary>
        /// Processes a document using OCR to extract structured data
        /// </summary>
        /// <param name="documentData">The document data as a byte array</param>
        /// <param name="fileName">The name of the document file</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <returns>The result containing extracted data from the document</returns>
        Task<Result<Dictionary<string, string>>> ProcessDocumentAsync(byte[] documentData, string fileName, string documentType);

        /// <summary>
        /// Processes a document from a URL using OCR to extract structured data
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <returns>The result containing extracted data from the document</returns>
        Task<Result<Dictionary<string, string>>> ProcessDocumentUrlAsync(string documentUrl, string documentType);

        /// <summary>
        /// Validates if a document type is supported for OCR processing
        /// </summary>
        /// <param name="fileName">The name of the document file</param>
        /// <returns>The result indicating if the document type is valid</returns>
        Task<Result<bool>> ValidateDocumentTypeAsync(string fileName);
    }

    /// <summary>
    /// Implementation of the OCR processor that handles document processing using Azure Cognitive Services
    /// </summary>
    public class OcrProcessor : IOcrProcessor
    {
        private readonly OcrOptions _options;
        private readonly AzureCognitiveServicesClient _cognitiveServicesClient;
        private readonly BlobStorageClient _blobStorageClient;
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the OcrProcessor class
        /// </summary>
        /// <param name="options">Configuration options for OCR processing</param>
        /// <param name="cognitiveServicesClient">Client for Azure Cognitive Services</param>
        /// <param name="blobStorageClient">Client for Azure Blob Storage</param>
        /// <param name="retryPolicy">Retry policy for handling transient failures</param>
        /// <param name="circuitBreakerPolicy">Circuit breaker policy for preventing cascading failures</param>
        /// <param name="logger">Logging service</param>
        public OcrProcessor(
            IOptions<OcrOptions> options,
            AzureCognitiveServicesClient cognitiveServicesClient,
            BlobStorageClient blobStorageClient,
            IRetryPolicy retryPolicy,
            ICircuitBreakerPolicy circuitBreakerPolicy,
            ILoggingService logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _cognitiveServicesClient = cognitiveServicesClient ?? throw new ArgumentNullException(nameof(cognitiveServicesClient));
            _blobStorageClient = blobStorageClient ?? throw new ArgumentNullException(nameof(blobStorageClient));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes a document using OCR to extract structured data
        /// </summary>
        /// <param name="documentData">The document data as a byte array</param>
        /// <param name="fileName">The name of the document file</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <returns>The result containing extracted data from the document</returns>
        public async Task<Result<Dictionary<string, string>>> ProcessDocumentAsync(byte[] documentData, string fileName, string documentType)
        {
            if (documentData == null || documentData.Length == 0)
            {
                return Result<Dictionary<string, string>>.Failure("Document data cannot be null or empty");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return Result<Dictionary<string, string>>.Failure("File name cannot be null or empty");
            }

            // Validate document file type
            var validationResult = await ValidateDocumentTypeAsync(fileName);
            if (!validationResult.IsSuccess)
            {
                return Result<Dictionary<string, string>>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);
            }

            _logger.LogInformation($"Processing document: {fileName}, Type: {documentType ?? "Unknown"}");

            try
            {
                // Upload document to blob storage to get a URL for processing
                var blobName = $"{Guid.NewGuid()}-{fileName}";
                var uploadResult = await _blobStorageClient.UploadBlobAsync(
                    documentData, 
                    blobName, 
                    "documents");

                if (!uploadResult.IsSuccess)
                {
                    return Result<Dictionary<string, string>>.Failure(
                        $"Failed to upload document for processing: {uploadResult.ErrorMessage}", 
                        uploadResult.ErrorCode);
                }

                // Generate a SAS URL for the uploaded document
                var sasResult = _blobStorageClient.GenerateSasUrl(
                    blobName, 
                    "documents");

                if (!sasResult.IsSuccess)
                {
                    return Result<Dictionary<string, string>>.Failure(
                        $"Failed to generate access URL for document: {sasResult.ErrorMessage}", 
                        sasResult.ErrorCode);
                }

                // Process the document using the URL
                return await ProcessDocumentUrlAsync(
                    sasResult.Value, 
                    documentType ?? GetDocumentTypeFromFileName(fileName));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document: {ex.Message}", ex);
                return Result<Dictionary<string, string>>.Failure($"Document processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a document from a URL using OCR to extract structured data
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <returns>The result containing extracted data from the document</returns>
        public async Task<Result<Dictionary<string, string>>> ProcessDocumentUrlAsync(string documentUrl, string documentType)
        {
            if (string.IsNullOrEmpty(documentUrl))
            {
                return Result<Dictionary<string, string>>.Failure("Document URL cannot be null or empty");
            }

            _logger.LogInformation($"Processing document from URL: {documentUrl}, Type: {documentType ?? "Unknown"}");

            try
            {
                // Create document processing options
                var options = new DocumentProcessingOptions
                {
                    DocumentType = documentType,
                    PreferFormRecognizer = true, // Prefer Form Recognizer for structured documents
                    ExtractTablesAndForms = true // Enable detailed extraction
                };

                // Use circuit breaker and retry policies for resilience
                var result = await _circuitBreakerPolicy.ExecuteAsync(
                    () => _retryPolicy.ExecuteAsync(
                        async () => 
                        {
                            // Process document using Cognitive Services
                            var processingResult = await _cognitiveServicesClient.ProcessDocument(documentUrl, options);
                            
                            if (processingResult.IsSuccess)
                            {
                                // Apply minimum confidence threshold
                                if (processingResult.ConfidenceScore >= _options.MinimumConfidenceScore)
                                {
                                    return Result<Dictionary<string, string>>.Success(processingResult.ExtractedData);
                                }
                                else
                                {
                                    return Result<Dictionary<string, string>>.Failure(
                                        $"Confidence score {processingResult.ConfidenceScore} is below minimum threshold {_options.MinimumConfidenceScore}");
                                }
                            }
                            else
                            {
                                return Result<Dictionary<string, string>>.Failure(processingResult.ErrorMessage);
                            }
                        }, 
                        "OCR_Document_Processing"),
                    "OCR_Document_Processing");

                if (result.IsSuccess)
                {
                    _logger.LogInformation($"Successfully processed document from URL: {documentUrl}");
                    return result;
                }
                else
                {
                    _logger.LogError($"Failed to process document from URL: {documentUrl}. Error: {result.ErrorMessage}");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document from URL: {ex.Message}", ex);
                return Result<Dictionary<string, string>>.Failure($"Document processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates if a document type is supported for OCR processing
        /// </summary>
        /// <param name="fileName">The name of the document file</param>
        /// <returns>The result indicating if the document type is valid</returns>
        public async Task<Result<bool>> ValidateDocumentTypeAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return Result<bool>.Failure("File name cannot be null or empty");
            }

            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            
            // Check if the file extension is supported
            if (!_options.SupportedFileTypes.Contains(extension))
            {
                _logger.LogWarning($"Unsupported file type: {extension}. Supported types: {string.Join(", ", _options.SupportedFileTypes)}");
                return Result<bool>.Failure($"Unsupported file type: {extension}. Supported types: {string.Join(", ", _options.SupportedFileTypes)}");
            }

            // Use BlobStorageClient to validate file type (additional security check)
            var validationResult = await _blobStorageClient.ValidateFileTypeAsync(fileName);
            if (!validationResult.IsSuccess)
            {
                return Result<bool>.Failure(validationResult.ErrorMessage, validationResult.ErrorCode);
            }

            return Result<bool>.Success(true);
        }

        /// <summary>
        /// Processes a document asynchronously in the background
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <param name="callback">Callback function to call with the processing result</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task ProcessDocumentInBackgroundAsync(
            string documentUrl, 
            string documentType, 
            Action<Result<Dictionary<string, string>>> callback)
        {
            _logger.LogInformation($"Starting background processing of document: {documentUrl}, Type: {documentType ?? "Unknown"}");
            
            try
            {
                var result = await ProcessDocumentUrlAsync(documentUrl, documentType);
                callback(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in background document processing: {ex.Message}", ex);
                callback(Result<Dictionary<string, string>>.Failure($"Background document processing failed: {ex.Message}"));
            }
            
            _logger.LogInformation($"Completed background processing of document: {documentUrl}");
        }

        /// <summary>
        /// Attempts to determine the document type from the file name
        /// </summary>
        /// <param name="fileName">The file name</param>
        /// <returns>The detected document type or a default type</returns>
        private string GetDocumentTypeFromFileName(string fileName)
        {
            string lowerFileName = fileName.ToLowerInvariant();
            
            if (lowerFileName.Contains("invoice") || lowerFileName.Contains("bill"))
            {
                return "Invoice";
            }
            else if (lowerFileName.Contains("receipt"))
            {
                return "Receipt";
            }
            else if (lowerFileName.Contains("vat") || lowerFileName.Contains("tax") || lowerFileName.Contains("return"))
            {
                return "VATReturn";
            }
            
            // Default to generic document type
            return "Document";
        }
    }

    /// <summary>
    /// Represents the result of an OCR processing operation
    /// </summary>
    public class OcrProcessingResult
    {
        /// <summary>
        /// Gets a value indicating whether the OCR processing operation was successful
        /// </summary>
        public bool IsSuccess { get; private set; }
        
        /// <summary>
        /// Gets the error message if the operation failed
        /// </summary>
        public string ErrorMessage { get; private set; }
        
        /// <summary>
        /// Gets the extracted data from the document
        /// </summary>
        public Dictionary<string, string> ExtractedData { get; private set; }
        
        /// <summary>
        /// Gets the confidence score of the extracted data
        /// </summary>
        public double ConfidenceScore { get; private set; }
        
        /// <summary>
        /// Gets the type of document that was processed
        /// </summary>
        public string DocumentType { get; private set; }
        
        /// <summary>
        /// Gets a unique identifier for this processing operation
        /// </summary>
        public string ProcessingId { get; private set; }

        /// <summary>
        /// Initializes a new instance of the OcrProcessingResult class
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="extractedData">The extracted data</param>
        /// <param name="confidenceScore">The confidence score</param>
        /// <param name="documentType">The document type</param>
        /// <param name="errorMessage">The error message if the operation failed</param>
        public OcrProcessingResult(
            bool isSuccess,
            Dictionary<string, string> extractedData,
            double confidenceScore,
            string documentType,
            string errorMessage)
        {
            IsSuccess = isSuccess;
            ExtractedData = extractedData ?? new Dictionary<string, string>();
            ConfidenceScore = confidenceScore;
            DocumentType = documentType;
            ErrorMessage = errorMessage ?? string.Empty;
            ProcessingId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Creates an OcrProcessingResult from a DocumentProcessingResult
        /// </summary>
        /// <param name="result">The DocumentProcessingResult</param>
        /// <returns>A new OcrProcessingResult based on the DocumentProcessingResult</returns>
        public static OcrProcessingResult FromDocumentProcessingResult(DocumentProcessingResult result)
        {
            return new OcrProcessingResult(
                result.IsSuccess,
                result.ExtractedData,
                result.ConfidenceScore,
                result.DocumentType,
                result.ErrorMessage);
        }
    }
}