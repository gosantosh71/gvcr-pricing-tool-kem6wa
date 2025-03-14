using Microsoft.Extensions.Logging; // version 6.0.0
using Azure.AI.FormRecognizer; // version 4.0.0
using Azure.AI.Vision.ImageAnalysis; // version 0.15.1-beta.1
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using VatFilingPricingTool.Infrastructure.Integration.OCR;
using VatFilingPricingTool.Infrastructure.Resilience;
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Infrastructure.Logging;

namespace VatFilingPricingTool.Infrastructure.Integration.OCR
{
    /// <summary>
    /// Client for Azure Cognitive Services that provides OCR capabilities for document processing
    /// </summary>
    public class AzureCognitiveServicesClient
    {
        private readonly OcrOptions _options;
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILoggingService _logger;
        private readonly DocumentAnalysisClient _formRecognizerClient;
        private readonly ImageAnalysisClient _computerVisionClient;

        /// <summary>
        /// Initializes a new instance of the AzureCognitiveServicesClient class
        /// </summary>
        /// <param name="options">The OCR configuration options</param>
        /// <param name="retryPolicy">The retry policy for handling transient failures</param>
        /// <param name="circuitBreakerPolicy">The circuit breaker policy for preventing cascading failures</param>
        /// <param name="logger">The logging service</param>
        public AzureCognitiveServicesClient(
            OcrOptions options,
            IRetryPolicy retryPolicy,
            ICircuitBreakerPolicy circuitBreakerPolicy,
            ILoggingService logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize Azure Form Recognizer client if enabled
            if (_options.EnableFormRecognizer)
            {
                if (string.IsNullOrEmpty(_options.ApiEndpoint) || string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogWarning("Form Recognizer is enabled but API endpoint or key is missing");
                }
                else
                {
                    var formRecognizerCredential = new Azure.AzureKeyCredential(_options.ApiKey);
                    _formRecognizerClient = new DocumentAnalysisClient(
                        new Uri(_options.ApiEndpoint),
                        formRecognizerCredential,
                        new DocumentAnalysisClientOptions
                        {
                            Retry =
                            {
                                MaxRetries = 0, // We'll handle retries ourselves through the retry policy
                                NetworkTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds)
                            }
                        });
                    _logger.LogInformation("Form Recognizer client initialized successfully");
                }
            }

            // Initialize Azure Computer Vision client if enabled
            if (_options.EnableComputerVision)
            {
                if (string.IsNullOrEmpty(_options.ApiEndpoint) || string.IsNullOrEmpty(_options.ApiKey))
                {
                    _logger.LogWarning("Computer Vision is enabled but API endpoint or key is missing");
                }
                else
                {
                    _computerVisionClient = new ImageAnalysisClient(
                        new Uri(_options.ApiEndpoint),
                        new Azure.AzureKeyCredential(_options.ApiKey),
                        new ImageAnalysisClientOptions
                        {
                            Retry =
                            {
                                MaxRetries = 0, // We'll handle retries ourselves through the retry policy
                                NetworkTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds)
                            }
                        });
                    _logger.LogInformation("Computer Vision client initialized successfully");
                }
            }
        }

        /// <summary>
        /// Processes a document using OCR to extract structured data
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="options">The document processing options</param>
        /// <returns>The result of the document processing operation including extracted data</returns>
        public async Task<DocumentProcessingResult> ProcessDocument(string documentUrl, DocumentProcessingOptions options)
        {
            if (string.IsNullOrEmpty(documentUrl))
            {
                throw new ArgumentNullException(nameof(documentUrl), "Document URL cannot be null or empty");
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Document processing options cannot be null");
            }

            _logger.LogInformation($"Processing document: {documentUrl}, Type: {options.DocumentType ?? "Unknown"}");

            // Determine the appropriate processing method based on document type and available services
            bool useFormRecognizer = _options.EnableFormRecognizer && _formRecognizerClient != null &&
                                     (options.PreferFormRecognizer || !_options.EnableComputerVision || _computerVisionClient == null);

            string documentType = options.DocumentType ?? "Generic";

            try
            {
                // Use circuit breaker and retry policy to execute the operation
                Result<Dictionary<string, string>> result;

                if (useFormRecognizer)
                {
                    result = await _circuitBreakerPolicy.ExecuteAsync(
                        () => _retryPolicy.ExecuteAsync(
                            () => ProcessWithFormRecognizerAsync(documentUrl, documentType),
                            "FormRecognizerProcessing"),
                        "FormRecognizerProcessing");
                }
                else if (_options.EnableComputerVision && _computerVisionClient != null)
                {
                    result = await _circuitBreakerPolicy.ExecuteAsync(
                        () => _retryPolicy.ExecuteAsync(
                            () => ProcessWithComputerVisionAsync(documentUrl),
                            "ComputerVisionProcessing"),
                        "ComputerVisionProcessing");
                }
                else
                {
                    _logger.LogError("No OCR service is available. Both Form Recognizer and Computer Vision are disabled or not properly initialized.");
                    return DocumentProcessingResult.CreateFailure("No OCR service is available");
                }

                if (result.IsSuccess)
                {
                    // Apply minimum confidence override if specified
                    double confidenceScore = options.MinimumConfidenceOverride ?? _options.MinimumConfidenceScore;
                    
                    return DocumentProcessingResult.CreateSuccess(
                        result.Value,
                        confidenceScore,
                        documentType);
                }
                else
                {
                    return DocumentProcessingResult.CreateFailure(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document: {ex.Message}", ex);
                return DocumentProcessingResult.CreateFailure($"Document processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a document using Azure Form Recognizer
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="documentType">The type of document being processed</param>
        /// <returns>The extracted data from the document</returns>
        private async Task<Result<Dictionary<string, string>>> ProcessWithFormRecognizerAsync(string documentUrl, string documentType)
        {
            try
            {
                if (_formRecognizerClient == null)
                {
                    return Result<Dictionary<string, string>>.Failure("Form Recognizer client is not initialized");
                }

                // Determine the appropriate model ID based on document type
                string modelId = GetModelIdForDocumentType(documentType);

                _logger.LogInformation($"Processing document with Form Recognizer using model: {modelId}");

                // Analyze the document using the appropriate model
                var operation = await _formRecognizerClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, modelId, new Uri(documentUrl));
                var result = operation.Value;

                // Extract fields from the analysis result
                var extractedData = ExtractFieldsFromAnalysisResult(result);

                _logger.LogInformation($"Successfully processed document with Form Recognizer. Extracted {extractedData.Count} fields.");
                
                return Result<Dictionary<string, string>>.Success(extractedData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document with Form Recognizer: {ex.Message}", ex);
                return Result<Dictionary<string, string>>.Failure($"Form Recognizer processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a document using Azure Computer Vision OCR
        /// </summary>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <returns>The extracted text from the document</returns>
        private async Task<Result<Dictionary<string, string>>> ProcessWithComputerVisionAsync(string documentUrl)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    return Result<Dictionary<string, string>>.Failure("Computer Vision client is not initialized");
                }

                _logger.LogInformation("Processing document with Computer Vision OCR");

                // Configure the image analysis options
                var analysisOptions = new ImageAnalysisOptions()
                {
                    Features = ImageAnalysisFeature.Text | ImageAnalysisFeature.Caption,
                    Language = "en",
                    ModelVersion = "latest"
                };

                // Analyze the image
                var result = await _computerVisionClient.AnalyzeAsync(new Uri(documentUrl), analysisOptions);

                // Extract text from the analysis result
                var extractedData = ExtractTextFromImageAnalysis(result);

                _logger.LogInformation($"Successfully processed document with Computer Vision. Extracted {extractedData.Count} text elements.");
                
                return Result<Dictionary<string, string>>.Success(extractedData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing document with Computer Vision: {ex.Message}", ex);
                return Result<Dictionary<string, string>>.Failure($"Computer Vision processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Extracts field values from a Form Recognizer analysis result
        /// </summary>
        /// <param name="result">The analysis result</param>
        /// <returns>A dictionary of extracted fields and their values</returns>
        private Dictionary<string, string> ExtractFieldsFromAnalysisResult(AnalyzeResult result)
        {
            var extractedFields = new Dictionary<string, string>();

            try
            {
                // Process each document in the result
                foreach (var document in result.Documents)
                {
                    // Process each field in the document
                    foreach (var field in document.Fields)
                    {
                        string key = field.Key;
                        string value = field.Value.Content;
                        double? confidence = field.Value.Confidence;

                        // Apply confidence threshold
                        if (!confidence.HasValue || confidence.Value >= _options.MinimumConfidenceScore)
                        {
                            // Apply field mapping if available
                            if (_options.FieldMappings.TryGetValue(document.DocType, out var mappings) && 
                                mappings.TryGetValue(key, out var mappedKey))
                            {
                                key = mappedKey;
                            }

                            extractedFields[key] = value;
                        }
                    }
                }

                // If no documents were found or no fields were extracted
                if (extractedFields.Count == 0 && result.Pages.Count > 0)
                {
                    // Try to extract some basic information from the text content
                    string allText = string.Join(" ", result.Pages.SelectMany(p => p.Lines).Select(l => l.Content));
                    extractedFields["RawText"] = allText;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting fields from analysis result: {ex.Message}", ex);
            }

            return extractedFields;
        }

        /// <summary>
        /// Extracts text content from a Computer Vision analysis result
        /// </summary>
        /// <param name="result">The image analysis result</param>
        /// <returns>A dictionary of extracted text content</returns>
        private Dictionary<string, string> ExtractTextFromImageAnalysis(ImageAnalysisResult result)
        {
            var extractedText = new Dictionary<string, string>();

            try
            {
                // Extract raw text
                if (result.Text != null)
                {
                    string rawText = string.Join(" ", result.Text.Lines.Select(l => l.Content));
                    extractedText["RawText"] = rawText;

                    // Try to identify key-value pairs in the text
                    var potentialKeyValuePairs = new Dictionary<string, string>();
                    
                    // Simple heuristic: look for lines with a colon
                    foreach (var line in result.Text.Lines)
                    {
                        string content = line.Content;
                        int colonIndex = content.IndexOf(':');
                        
                        if (colonIndex > 0 && colonIndex < content.Length - 1)
                        {
                            string key = content.Substring(0, colonIndex).Trim();
                            string value = content.Substring(colonIndex + 1).Trim();
                            
                            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            {
                                potentialKeyValuePairs[key] = value;
                            }
                        }
                    }

                    // Add potential key-value pairs if found
                    foreach (var kvp in potentialKeyValuePairs)
                    {
                        extractedText[kvp.Key] = kvp.Value;
                    }
                }

                // Add caption if available
                if (result.Caption != null && !string.IsNullOrEmpty(result.Caption.Content))
                {
                    extractedText["Caption"] = result.Caption.Content;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting text from image analysis result: {ex.Message}", ex);
            }

            return extractedText;
        }

        /// <summary>
        /// Gets the appropriate Form Recognizer model ID for a document type
        /// </summary>
        /// <param name="documentType">The type of document</param>
        /// <returns>The model ID to use for the document type</returns>
        private string GetModelIdForDocumentType(string documentType)
        {
            // Check if a custom model ID is configured for this document type
            if (_options.ModelIds != null && _options.ModelIds.TryGetValue(documentType, out string modelId))
            {
                return modelId;
            }

            // Use appropriate built-in model based on document type
            switch (documentType.ToLowerInvariant())
            {
                case "invoice":
                    return "prebuilt-invoice";
                case "receipt":
                    return "prebuilt-receipt";
                case "vatreturn":
                    return "prebuilt-tax.us.form-w2"; // Using tax form as a close match for VAT return
                default:
                    return "prebuilt-document"; // Default to general document model
            }
        }
    }

    /// <summary>
    /// Represents the result of a document processing operation
    /// </summary>
    public class DocumentProcessingResult
    {
        /// <summary>
        /// Gets a value indicating whether the document processing operation was successful
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
        /// Gets the timestamp when the document was processed
        /// </summary>
        public DateTime ProcessingTime { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DocumentProcessingResult class
        /// </summary>
        /// <param name="isSuccess">Whether the operation was successful</param>
        /// <param name="extractedData">The extracted data</param>
        /// <param name="confidenceScore">The confidence score</param>
        /// <param name="documentType">The document type</param>
        /// <param name="errorMessage">The error message if the operation failed</param>
        public DocumentProcessingResult(
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
            ProcessingTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Creates a successful document processing result
        /// </summary>
        /// <param name="extractedData">The extracted data</param>
        /// <param name="confidenceScore">The confidence score</param>
        /// <param name="documentType">The document type</param>
        /// <returns>A successful document processing result</returns>
        public static DocumentProcessingResult CreateSuccess(
            Dictionary<string, string> extractedData,
            double confidenceScore,
            string documentType)
        {
            return new DocumentProcessingResult(
                true,
                extractedData,
                confidenceScore,
                documentType,
                null);
        }

        /// <summary>
        /// Creates a failed document processing result
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A failed document processing result</returns>
        public static DocumentProcessingResult CreateFailure(string errorMessage)
        {
            return new DocumentProcessingResult(
                false,
                null,
                0.0,
                null,
                errorMessage);
        }
    }

    /// <summary>
    /// Options for document processing operations
    /// </summary>
    public class DocumentProcessingOptions
    {
        /// <summary>
        /// Gets or sets the type of document to process
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prefer Form Recognizer over Computer Vision
        /// </summary>
        public bool PreferFormRecognizer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to extract tables and forms
        /// </summary>
        public bool ExtractTablesAndForms { get; set; }

        /// <summary>
        /// Gets or sets a minimum confidence score override
        /// </summary>
        public double? MinimumConfidenceOverride { get; set; }

        /// <summary>
        /// Gets or sets custom parameters for the document processing operation
        /// </summary>
        public Dictionary<string, string> CustomParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the DocumentProcessingOptions class
        /// </summary>
        public DocumentProcessingOptions()
        {
            PreferFormRecognizer = true;
            ExtractTablesAndForms = true;
            MinimumConfidenceOverride = null;
            CustomParameters = new Dictionary<string, string>();
        }
    }
}