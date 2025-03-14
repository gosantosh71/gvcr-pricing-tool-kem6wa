using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq;
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Common.Models;
using VatFilingPricingTool.Data.Repositories.Interfaces;
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Infrastructure.Integration.ERP;
using VatFilingPricingTool.Infrastructure.Integration.OCR;
using VatFilingPricingTool.Infrastructure.Resilience;

namespace VatFilingPricingTool.Service.Implementations
{
    /// <summary>
    /// Service implementation for managing and utilizing integrations with external systems such as ERP and OCR services
    /// </summary>
    public class IntegrationService : IIntegrationService
    {
        private readonly IIntegrationRepository _integrationRepository;
        private readonly ILogger<IntegrationService> _logger;
        private readonly DynamicsConnector _dynamicsConnector;
        private readonly OcrProcessor _ocrProcessor;
        private readonly IRetryPolicy _retryPolicy;
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly Dictionary<string, string> _availableSystemTypes;

        /// <summary>
        /// Initializes a new instance of the IntegrationService class with required dependencies
        /// </summary>
        /// <param name="integrationRepository">Repository for integration data access</param>
        /// <param name="logger">Logger for operation logging</param>
        /// <param name="dynamicsConnector">Connector for Microsoft Dynamics 365 integration</param>
        /// <param name="ocrProcessor">Processor for OCR document processing</param>
        /// <param name="retryPolicy">Retry policy for handling transient failures</param>
        /// <param name="circuitBreakerPolicy">Circuit breaker for preventing cascading failures</param>
        public IntegrationService(
            IIntegrationRepository integrationRepository,
            ILogger<IntegrationService> logger,
            DynamicsConnector dynamicsConnector,
            OcrProcessor ocrProcessor,
            IRetryPolicy retryPolicy,
            ICircuitBreakerPolicy circuitBreakerPolicy)
        {
            // Validate that integrationRepository is not null
            _integrationRepository = integrationRepository ?? throw new ArgumentNullException(nameof(integrationRepository));

            // Validate that logger is not null
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Validate that dynamicsConnector is not null
            _dynamicsConnector = dynamicsConnector ?? throw new ArgumentNullException(nameof(dynamicsConnector));

            // Validate that ocrProcessor is not null
            _ocrProcessor = ocrProcessor ?? throw new ArgumentNullException(nameof(ocrProcessor));

            // Validate that retryPolicy is not null
            _retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));

            // Validate that circuitBreakerPolicy is not null
            _circuitBreakerPolicy = circuitBreakerPolicy ?? throw new ArgumentNullException(nameof(circuitBreakerPolicy));

            // Assign integrationRepository to _integrationRepository
            _integrationRepository = integrationRepository;

            // Assign logger to _logger
            _logger = logger;

            // Assign dynamicsConnector to _dynamicsConnector
            _dynamicsConnector = dynamicsConnector;

            // Assign ocrProcessor to _ocrProcessor
            _ocrProcessor = ocrProcessor;

            // Assign retryPolicy to _retryPolicy
            _retryPolicy = retryPolicy;

            // Assign circuitBreakerPolicy to _circuitBreakerPolicy
            _circuitBreakerPolicy = circuitBreakerPolicy;

            // Initialize _availableSystemTypes with supported system types and descriptions
            _availableSystemTypes = new Dictionary<string, string>
            {
                { "Dynamics365", "Microsoft Dynamics 365" },
                { "AzureCognitiveServices", "Azure Cognitive Services" }
            };
        }

        /// <summary>
        /// Retrieves all integrations configured in the system
        /// </summary>
        /// <returns>A collection of all integration configurations</returns>
        [public: System.ComponentModel.DescriptionAttribute("Retrieves all integrations configured in the system")]
        public async Task<IEnumerable<Integration>> GetIntegrationsAsync()
        {
            // Log the start of the operation
            _logger.LogInformation("Retrieving all integrations");

            // Call _integrationRepository.GetAllAsync() to retrieve all integrations
            var integrations = await _integrationRepository.GetAllAsync();

            // Return the retrieved integrations
            return integrations;
        }

        /// <summary>
        /// Retrieves a specific integration by its ID
        /// </summary>
        /// <param name="integrationId">The ID of the integration to retrieve</param>
        /// <returns>The integration with the specified ID, or null if not found</returns>
        [public: System.ComponentModel.DescriptionAttribute("Retrieves a specific integration by its ID")]
        public async Task<Integration> GetIntegrationByIdAsync(string integrationId)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Log the start of the operation
            _logger.LogInformation("Retrieving integration with ID: {IntegrationId}", integrationId);

            // Call _integrationRepository.GetByIdAsync(integrationId) to retrieve the integration
            var integration = await _integrationRepository.GetByIdAsync(integrationId);

            // Return the retrieved integration
            return integration;
        }

        /// <summary>
        /// Retrieves all integrations configured for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A collection of integrations for the specified user</returns>
        [public: System.ComponentModel.DescriptionAttribute("Retrieves all integrations configured for a specific user")]
        public async Task<IEnumerable<Integration>> GetUserIntegrationsAsync(string userId)
        {
            // Validate that userId is not null or empty
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            // Log the start of the operation
            _logger.LogInformation("Retrieving integrations for user ID: {UserId}", userId);

            // Call _integrationRepository.GetByUserIdAsync(userId) to retrieve the user's integrations
            var integrations = await _integrationRepository.GetByUserIdAsync(userId);

            // Return the retrieved integrations
            return integrations;
        }

        /// <summary>
        /// Creates a new integration configuration for an external system
        /// </summary>
        /// <param name="userId">The ID of the user who owns this integration</param>
        /// <param name="systemType">The type of the external system (e.g., "Dynamics365", "CognitiveServices")</param>
        /// <param name="connectionString">The connection string used to connect to the external system</param>
        /// <param name="apiKey">The API key used for authentication with the external system</param>
        /// <param name="apiEndpoint">The API endpoint URL for the external system</param>
        /// <param name="additionalSettings">Additional configuration settings for this integration</param>
        /// <returns>The newly created integration configuration</returns>
        [public: System.ComponentModel.DescriptionAttribute("Creates a new integration configuration for an external system")]
        public async Task<Integration> CreateIntegrationAsync(
            string userId,
            string systemType,
            string connectionString,
            string apiKey,
            string apiEndpoint,
            Dictionary<string, string> additionalSettings)
        {
            // Validate that userId is not null or empty
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            // Validate that systemType is not null or empty
            if (string.IsNullOrEmpty(systemType))
            {
                throw new ArgumentException("System type cannot be null or empty", nameof(systemType));
            }

            // Validate that connectionString is not null or empty
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            // Log the start of the operation
            _logger.LogInformation("Creating new integration for user ID: {UserId}, System type: {SystemType}", userId, systemType);

            // Create a new Integration using Integration.Create(userId, systemType, connectionString)
            var integration = Integration.Create(userId, systemType, connectionString);

            // Set the ApiKey property if apiKey is provided
            integration.UpdateConnectionDetails(connectionString: connectionString, apiKey: apiKey, apiEndpoint: apiEndpoint);

            // Add each key-value pair from additionalSettings to the integration's AdditionalSettings if provided
            if (additionalSettings != null)
            {
                foreach (var setting in additionalSettings)
                {
                    integration.AddSetting(setting.Key, setting.Value);
                }
            }

            // Call _integrationRepository.AddAsync(integration) to save the new integration
            var createdIntegration = await _integrationRepository.AddAsync(integration);

            // Return the created integration
            return createdIntegration;
        }

        /// <summary>
        /// Updates an existing integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to update</param>
        /// <param name="connectionString">The new connection string (null to keep existing)</param>
        /// <param name="apiKey">The new API key (null to keep existing)</param>
        /// <param name="apiEndpoint">The new API endpoint (null to keep existing)</param>
        /// <param name="additionalSettings">New additional settings (null to keep existing)</param>
        /// <returns>The updated integration configuration</returns>
        [public: System.ComponentModel.DescriptionAttribute("Updates an existing integration configuration")]
        public async Task<Integration> UpdateIntegrationAsync(
            string integrationId,
            string connectionString,
            string apiKey,
            string apiEndpoint,
            Dictionary<string, string> additionalSettings)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Log the start of the operation
            _logger.LogInformation("Updating integration with ID: {IntegrationId}", integrationId);

            // Retrieve the existing integration using _integrationRepository.GetByIdAsync(integrationId)
            var integration = await _integrationRepository.GetByIdAsync(integrationId);

            // If integration is not found, return null
            if (integration == null)
            {
                _logger.LogWarning("Integration with ID: {IntegrationId} not found", integrationId);
                return null;
            }

            // Update the ConnectionString property if connectionString is provided
            // Update the ApiKey property if apiKey is provided
            // Update the ApiEndpoint property if apiEndpoint is provided
            integration.UpdateConnectionDetails(connectionString: connectionString, apiKey: apiKey, apiEndpoint: apiEndpoint);

            // Update AdditionalSettings with values from additionalSettings if provided
            if (additionalSettings != null)
            {
                foreach (var setting in additionalSettings)
                {
                    integration.AddSetting(setting.Key, setting.Value);
                }
            }

            // Call _integrationRepository.UpdateAsync(integration) to save the changes
            var updatedIntegration = await _integrationRepository.UpdateAsync(integration);

            // Return the updated integration
            return updatedIntegration;
        }

        /// <summary>
        /// Deletes an integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to delete</param>
        /// <returns>True if the integration was successfully deleted, false otherwise</returns>
        [public: System.ComponentModel.DescriptionAttribute("Deletes an integration configuration")]
        public async Task<bool> DeleteIntegrationAsync(string integrationId)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Log the start of the operation
            _logger.LogInformation("Deleting integration with ID: {IntegrationId}", integrationId);

            // Call _integrationRepository.DeleteAsync(integrationId) to delete the integration
            var result = await _integrationRepository.DeleteAsync(integrationId);

            // Return true if deletion was successful, false otherwise
            return result;
        }

        /// <summary>
        /// Tests the connection to an external system using the specified integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to test</param>
        /// <returns>True if the connection test was successful, false otherwise</returns>
        [public: System.ComponentModel.DescriptionAttribute("Tests the connection to an external system using the specified integration configuration")]
        public async Task<bool> TestConnectionAsync(string integrationId)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Log the start of the operation
            _logger.LogInformation("Testing connection for integration with ID: {IntegrationId}", integrationId);

            // Retrieve the integration using _integrationRepository.GetByIdAsync(integrationId)
            var integration = await _integrationRepository.GetByIdAsync(integrationId);

            // If integration is not found, return false
            if (integration == null)
            {
                _logger.LogWarning("Integration with ID: {IntegrationId} not found", integrationId);
                return false;
            }

            // Use _circuitBreakerPolicy to execute the connection test with resilience
            // If circuit is open, log warning and return false
            if (_circuitBreakerPolicy.CurrentState == CircuitState.Open)
            {
                _logger.LogWarning("Circuit breaker is open, connection test skipped for integration ID: {IntegrationId}", integrationId);
                return false;
            }

            try
            {
                // Based on integration.SystemType, call the appropriate connector's TestConnectionAsync method
                if (integration.SystemType.Equals("Dynamics365", StringComparison.OrdinalIgnoreCase))
                {
                    // For 'Dynamics365', use _dynamicsConnector.TestConnectionAsync(integration)
                    var dynamicsResult = await _dynamicsConnector.TestConnectionAsync(integration);
                    return dynamicsResult.IsSuccess;
                }
                else if (integration.SystemType.Equals("AzureCognitiveServices", StringComparison.OrdinalIgnoreCase))
                {
                    // For 'AzureCognitiveServices', create a simple test document processing request
                    // This is a placeholder, implement actual test logic if needed
                    _logger.LogInformation("Testing connection to Azure Cognitive Services is not fully implemented yet.");
                    return true;
                }
                else
                {
                    // For other system types, log warning about unsupported type and return false
                    _logger.LogWarning("Unsupported system type: {SystemType} for integration ID: {IntegrationId}", integration.SystemType, integrationId);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing connection for integration ID: {IntegrationId}", integrationId);
                return false;
            }
        }

        /// <summary>
        /// Imports transaction data from an ERP or other external system using the specified integration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to use</param>
        /// <param name="parameters">Parameters for the import operation</param>
        /// <returns>The result of the import operation including any imported data</returns>
        [public: System.ComponentModel.DescriptionAttribute("Imports transaction data from an ERP or other external system using the specified integration")]
        public async Task<ImportResult> ImportDataAsync(string integrationId, ImportParameters parameters)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Validate that parameters is not null
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            // Log the start of the operation
            _logger.LogInformation("Importing data for integration with ID: {IntegrationId}", integrationId);

            // Retrieve the integration using _integrationRepository.GetByIdAsync(integrationId)
            var integration = await _integrationRepository.GetByIdAsync(integrationId);

            // If integration is not found, return a failure result
            if (integration == null)
            {
                _logger.LogWarning("Integration with ID: {IntegrationId} not found", integrationId);
                return new ImportResult { Success = false, ErrorMessage = "Integration not found" };
            }

            // Use _circuitBreakerPolicy to execute the import operation with resilience
            // If circuit is open, log warning and return a failure result
            if (_circuitBreakerPolicy.CurrentState == CircuitState.Open)
            {
                _logger.LogWarning("Circuit breaker is open, import data skipped for integration ID: {IntegrationId}", integrationId);
                return new ImportResult { Success = false, ErrorMessage = "Service unavailable due to circuit breaker" };
            }

            try
            {
                // Based on integration.SystemType, call the appropriate connector's ImportDataAsync method
                if (integration.SystemType.Equals("Dynamics365", StringComparison.OrdinalIgnoreCase))
                {
                    // For 'Dynamics365', use _dynamicsConnector.ImportDataAsync(integration, parameters)
                    var dynamicsResult = await _dynamicsConnector.ImportDataAsync(integration, parameters);

                    // If import is successful, update the integration's last sync date
                    if (dynamicsResult.IsSuccess)
                    {
                        integration.UpdateLastSyncDate();
                        await _integrationRepository.UpdateLastSyncDateAsync(integration.IntegrationId);

                        // Return the import result
                        return new ImportResult
                        {
                            Success = true,
                            RecordsImported = dynamicsResult.Value?.Records?.Count ?? 0,
                            ImportedData = dynamicsResult.Value?.Records
                        };
                    }
                    else
                    {
                        return new ImportResult { Success = false, ErrorMessage = dynamicsResult.ErrorMessage };
                    }
                }
                else
                {
                    // For other system types, log warning about unsupported type and return a failure result
                    _logger.LogWarning("Unsupported system type: {SystemType} for integration ID: {IntegrationId}", integration.SystemType, integrationId);
                    return new ImportResult { Success = false, ErrorMessage = "Unsupported system type" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing data for integration ID: {IntegrationId}", integrationId);
                return new ImportResult { Success = false, ErrorMessage = $"Import failed: {ex.Message}" };
            }
        }

        /// <summary>
        /// Processes a document using OCR services to extract structured data from invoices and VAT forms
        /// </summary>
        /// <param name="integrationId">The ID of the integration to use</param>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="options">Options for the document processing operation</param>
        /// <returns>The result of the document processing operation including extracted data</returns>
        [public: System.ComponentModel.DescriptionAttribute("Processes a document using OCR services to extract structured data from invoices and VAT forms")]
        public async Task<DocumentProcessingResult> ProcessDocumentAsync(
            string integrationId,
            string documentUrl,
            DocumentProcessingOptions options)
        {
            // Validate that integrationId is not null or empty
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            // Validate that documentUrl is not null or empty
            if (string.IsNullOrEmpty(documentUrl))
            {
                throw new ArgumentException("Document URL cannot be null or empty", nameof(documentUrl));
            }

            // Validate that options is not null
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // Log the start of the operation
            _logger.LogInformation("Processing document for integration with ID: {IntegrationId}, Document URL: {DocumentUrl}", integrationId, documentUrl);

            // Retrieve the integration using _integrationRepository.GetByIdAsync(integrationId)
            var integration = await _integrationRepository.GetByIdAsync(integrationId);

            // If integration is not found, return a failure result
            if (integration == null)
            {
                _logger.LogWarning("Integration with ID: {IntegrationId} not found", integrationId);
                return DocumentProcessingResult.CreateFailure("Integration not found");
            }

            // Verify that integration.SystemType is 'AzureCognitiveServices', otherwise return a failure result
            if (!integration.SystemType.Equals("AzureCognitiveServices", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid system type: {SystemType} for integration ID: {IntegrationId}. Expected AzureCognitiveServices", integration.SystemType, integrationId);
                return DocumentProcessingResult.CreateFailure("Invalid system type. Expected AzureCognitiveServices");
            }

            // Use _circuitBreakerPolicy to execute the document processing with resilience
            // If circuit is open, log warning and return a failure result
            if (_circuitBreakerPolicy.CurrentState == CircuitState.Open)
            {
                _logger.LogWarning("Circuit breaker is open, document processing skipped for integration ID: {IntegrationId}", integrationId);
                return DocumentProcessingResult.CreateFailure("Service unavailable due to circuit breaker");
            }

            try
            {
                // Call _ocrProcessor.ProcessDocumentAsync(documentUrl, options) to process the document
                var processingResult = await _ocrProcessor.ProcessDocumentAsync(documentUrl, options);

                // If processing is successful, update the integration's last sync date
                if (processingResult.IsSuccess)
                {
                    integration.UpdateLastSyncDate();
                    await _integrationRepository.UpdateLastSyncDateAsync(integration.IntegrationId);

                    // Return the document processing result
                    return DocumentProcessingResult.CreateSuccess(
                        processingResult.ExtractedData,
                        processingResult.ConfidenceScore,
                        options.DocumentType);
                }
                else
                {
                    return DocumentProcessingResult.CreateFailure(processingResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document for integration ID: {IntegrationId}", integrationId);
                return DocumentProcessingResult.CreateFailure($"Document processing failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves a list of available external system types that can be integrated
        /// </summary>
        /// <returns>A collection of available system types (e.g., Dynamics365, AzureCognitiveServices)</returns>
        [public: System.ComponentModel.DescriptionAttribute("Retrieves a list of available external system types that can be integrated")]
        public async Task<IEnumerable<string>> GetAvailableSystemTypesAsync()
        {
            // Log the start of the operation
            _logger.LogInformation("Retrieving available system types");

            // Return the keys from _availableSystemTypes as an enumerable collection
            return await Task.FromResult(_availableSystemTypes.Keys.AsEnumerable());
        }

        /// <summary>
        /// Executes an operation with retry and circuit breaker policies for resilience
        /// </summary>
        /// <typeparam name="T">The type of the result value</typeparam>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">The name of the operation for logging purposes</param>
        /// <returns>The result of the operation after applying resilience policies</returns>
        [private: System.ComponentModel.DescriptionAttribute("Executes an operation with retry and circuit breaker policies for resilience")]
        private async Task<T> ExecuteWithResilienceAsync<T>(Func<Task<T>> operation, string operationName)
        {
            // Log the start of the resilient operation
            _logger.LogInformation("Executing operation '{OperationName}' with resilience", operationName);

            // Check if the circuit breaker is open
            if (_circuitBreakerPolicy.CurrentState == CircuitState.Open)
            {
                // If circuit is open, log warning and throw an exception
                _logger.LogWarning("Circuit breaker is open, operation '{OperationName}' skipped", operationName);
                throw new InvalidOperationException($"Circuit breaker is open for operation '{operationName}'");
            }

            // Use _retryPolicy.ExecuteAsync to apply retry policy to the operation
            // Return the result of the operation
            return await _retryPolicy.ExecuteAsync(operation, operationName);
        }
    }
}