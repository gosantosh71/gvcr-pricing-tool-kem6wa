using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Domain.Entities;
using VatFilingPricingTool.Infrastructure.Integration.ERP;
using VatFilingPricingTool.Infrastructure.Integration.OCR;

namespace VatFilingPricingTool.Service.Interfaces
{
    /// <summary>
    /// Service interface for managing and utilizing integrations with external systems such as ERP and OCR services
    /// </summary>
    public interface IIntegrationService
    {
        /// <summary>
        /// Retrieves all integrations configured in the system
        /// </summary>
        /// <returns>A collection of all integration configurations</returns>
        Task<IEnumerable<Integration>> GetIntegrationsAsync();

        /// <summary>
        /// Retrieves a specific integration by its ID
        /// </summary>
        /// <param name="integrationId">The ID of the integration to retrieve</param>
        /// <returns>The integration with the specified ID, or null if not found</returns>
        Task<Integration> GetIntegrationByIdAsync(string integrationId);

        /// <summary>
        /// Retrieves all integrations configured for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>A collection of integrations for the specified user</returns>
        Task<IEnumerable<Integration>> GetUserIntegrationsAsync(string userId);

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
        Task<Integration> CreateIntegrationAsync(
            string userId,
            string systemType,
            string connectionString,
            string apiKey,
            string apiEndpoint,
            Dictionary<string, string> additionalSettings);

        /// <summary>
        /// Updates an existing integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to update</param>
        /// <param name="connectionString">The new connection string (null to keep existing)</param>
        /// <param name="apiKey">The new API key (null to keep existing)</param>
        /// <param name="apiEndpoint">The new API endpoint (null to keep existing)</param>
        /// <param name="additionalSettings">New additional settings (null to keep existing)</param>
        /// <returns>The updated integration configuration</returns>
        Task<Integration> UpdateIntegrationAsync(
            string integrationId,
            string connectionString,
            string apiKey,
            string apiEndpoint,
            Dictionary<string, string> additionalSettings);

        /// <summary>
        /// Deletes an integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to delete</param>
        /// <returns>True if the integration was successfully deleted, false otherwise</returns>
        Task<bool> DeleteIntegrationAsync(string integrationId);

        /// <summary>
        /// Tests the connection to an external system using the specified integration configuration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to test</param>
        /// <returns>True if the connection test was successful, false otherwise</returns>
        Task<bool> TestConnectionAsync(string integrationId);

        /// <summary>
        /// Imports transaction data from an ERP or other external system using the specified integration
        /// </summary>
        /// <param name="integrationId">The ID of the integration to use</param>
        /// <param name="parameters">Parameters for the import operation</param>
        /// <returns>The result of the import operation including any imported data</returns>
        Task<ImportResult> ImportDataAsync(string integrationId, ImportParameters parameters);

        /// <summary>
        /// Processes a document using OCR services to extract structured data from invoices and VAT forms
        /// </summary>
        /// <param name="integrationId">The ID of the integration to use</param>
        /// <param name="documentUrl">The URL of the document to process</param>
        /// <param name="options">Options for the document processing operation</param>
        /// <returns>The result of the document processing operation including extracted data</returns>
        Task<DocumentProcessingResult> ProcessDocumentAsync(
            string integrationId,
            string documentUrl,
            DocumentProcessingOptions options);

        /// <summary>
        /// Retrieves a list of available external system types that can be integrated
        /// </summary>
        /// <returns>A collection of available system types (e.g., Dynamics365, AzureCognitiveServices)</returns>
        Task<IEnumerable<string>> GetAvailableSystemTypesAsync();
    }
}