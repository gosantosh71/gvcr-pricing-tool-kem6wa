using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Domain.Entities;

namespace VatFilingPricingTool.Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Integration entities with specialized query and update methods.
    /// Provides data access operations for external system integrations such as ERP systems and OCR services.
    /// </summary>
    public interface IIntegrationRepository : IRepository<Integration>
    {
        /// <summary>
        /// Retrieves all integrations for a specific user
        /// </summary>
        /// <param name="userId">The identifier of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of integrations</returns>
        Task<IEnumerable<Integration>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Retrieves all integrations for a specific system type
        /// </summary>
        /// <param name="systemType">The type of the external system</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of integrations</returns>
        Task<IEnumerable<Integration>> GetBySystemTypeAsync(string systemType);

        /// <summary>
        /// Retrieves all active integrations across the system
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of active integrations</returns>
        Task<IEnumerable<Integration>> GetActiveIntegrationsAsync();

        /// <summary>
        /// Retrieves a specific integration for a user and system type combination
        /// </summary>
        /// <param name="userId">The identifier of the user</param>
        /// <param name="systemType">The type of the external system</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the integration if found, null otherwise</returns>
        Task<Integration> GetByUserIdAndSystemTypeAsync(string userId, string systemType);

        /// <summary>
        /// Updates the last synchronization date for an integration to the current UTC time
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the update was successful</returns>
        Task<bool> UpdateLastSyncDateAsync(string integrationId);

        /// <summary>
        /// Increments the retry count for an integration and returns the updated count
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated retry count</returns>
        Task<int> IncrementRetryCountAsync(string integrationId);

        /// <summary>
        /// Activates an integration by setting its IsActive property to true
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the activation was successful</returns>
        Task<bool> ActivateIntegrationAsync(string integrationId);

        /// <summary>
        /// Deactivates an integration by setting its IsActive property to false
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deactivation was successful</returns>
        Task<bool> DeactivateIntegrationAsync(string integrationId);

        /// <summary>
        /// Updates or adds a specific setting for an integration in its AdditionalSettings dictionary
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <param name="key">The setting key</param>
        /// <param name="value">The setting value</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the update was successful</returns>
        Task<bool> UpdateSettingAsync(string integrationId, string key, string value);
    }
}