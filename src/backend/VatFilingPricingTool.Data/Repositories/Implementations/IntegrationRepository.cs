using Microsoft.EntityFrameworkCore; // Microsoft.EntityFrameworkCore package version 6.0.0
using Microsoft.Extensions.Logging; // Microsoft.Extensions.Logging package version 6.0.0
using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0
using System.Linq; // System.Linq package version 6.0.0
using System.Threading.Tasks; // System.Threading.Tasks package version 6.0.0
using VatFilingPricingTool.Data.Context; // Internal import
using VatFilingPricingTool.Data.Extensions; // Internal import
using VatFilingPricingTool.Data.Repositories.Interfaces; // Internal import
using VatFilingPricingTool.Domain.Entities; // Internal import

namespace VatFilingPricingTool.Data.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for Integration entities with specialized query and update operations
    /// </summary>
    public class IntegrationRepository : Repository<Integration>, IIntegrationRepository
    {
        private readonly IVatFilingDbContext _context;
        private readonly ILogger<IntegrationRepository> _logger;

        /// <summary>
        /// Initializes a new instance of the IntegrationRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        /// <param name="logger">Optional logger instance</param>
        public IntegrationRepository(IVatFilingDbContext context, ILogger<IntegrationRepository> logger = null) : base(context, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all integrations for a specific user
        /// </summary>
        /// <param name="userId">The identifier of the user</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of integrations for the specified user.</returns>
        public async Task<IEnumerable<Integration>> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            _logger?.LogInformation("Retrieving integrations for user {UserId}", userId);

            var integrations = await FindAsync(i => i.UserId == userId);

            _logger?.LogInformation("Retrieved {IntegrationCount} integrations for user {UserId}", integrations.Count(), userId);

            return integrations;
        }

        /// <summary>
        /// Retrieves all integrations for a specific system type
        /// </summary>
        /// <param name="systemType">The type of the external system</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of integrations for the specified system type.</returns>
        public async Task<IEnumerable<Integration>> GetBySystemTypeAsync(string systemType)
        {
            if (string.IsNullOrEmpty(systemType))
            {
                throw new ArgumentException("System type cannot be null or empty", nameof(systemType));
            }

            _logger?.LogInformation("Retrieving integrations for system type {SystemType}", systemType);

            var integrations = await FindAsync(i => i.SystemType == systemType);

            _logger?.LogInformation("Retrieved {IntegrationCount} integrations for system type {SystemType}", integrations.Count(), systemType);

            return integrations;
        }

        /// <summary>
        /// Retrieves all active integrations across the system
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of all active integrations.</returns>
        public async Task<IEnumerable<Integration>> GetActiveIntegrationsAsync()
        {
            _logger?.LogInformation("Retrieving all active integrations");

            var integrations = await FindAsync(i => i.IsActive);

            _logger?.LogInformation("Retrieved {IntegrationCount} active integrations", integrations.Count());

            return integrations;
        }

        /// <summary>
        /// Retrieves a specific integration for a user and system type combination
        /// </summary>
        /// <param name="userId">The identifier of the user</param>
        /// <param name="systemType">The type of the external system</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the integration if found, or null.</returns>
        public async Task<Integration> GetByUserIdAndSystemTypeAsync(string userId, string systemType)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(systemType))
            {
                throw new ArgumentException("System type cannot be null or empty", nameof(systemType));
            }

            _logger?.LogInformation("Retrieving integration for user {UserId} and system type {SystemType}", userId, systemType);

            var integration = await _context.Integrations
                .FirstOrDefaultAsync(i => i.UserId == userId && i.SystemType == systemType);

            if (integration != null)
            {
                _logger?.LogInformation("Integration found for user {UserId} and system type {SystemType}", userId, systemType);
            }
            else
            {
                _logger?.LogInformation("Integration not found for user {UserId} and system type {SystemType}", userId, systemType);
            }

            return integration;
        }

        /// <summary>
        /// Updates the last synchronization date for an integration to the current UTC time
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the update was successful.</returns>
        public async Task<bool> UpdateLastSyncDateAsync(string integrationId)
        {
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            _logger?.LogInformation("Updating LastSyncDate for integration {IntegrationId}", integrationId);

            var integration = await GetByIdAsync(integrationId);
            if (integration == null)
            {
                _logger?.LogWarning("Integration with ID {IntegrationId} not found", integrationId);
                return false;
            }

            integration.UpdateLastSyncDate();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("LastSyncDate updated successfully for integration {IntegrationId}", integrationId);
            return true;
        }

        /// <summary>
        /// Increments the retry count for an integration and returns the updated count
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the updated retry count.</returns>
        public async Task<int> IncrementRetryCountAsync(string integrationId)
        {
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            _logger?.LogInformation("Incrementing RetryCount for integration {IntegrationId}", integrationId);

            var integration = await GetByIdAsync(integrationId);
            if (integration == null)
            {
                _logger?.LogWarning("Integration with ID {IntegrationId} not found", integrationId);
                return -1; // Indicate not found
            }

            int updatedRetryCount = integration.IncrementRetryCount();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("RetryCount incremented to {RetryCount} for integration {IntegrationId}", updatedRetryCount, integrationId);
            return updatedRetryCount;
        }

        /// <summary>
        /// Activates an integration by setting its IsActive property to true
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the activation was successful.</returns>
        public async Task<bool> ActivateIntegrationAsync(string integrationId)
        {
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            _logger?.LogInformation("Activating integration {IntegrationId}", integrationId);

            var integration = await GetByIdAsync(integrationId);
            if (integration == null)
            {
                _logger?.LogWarning("Integration with ID {IntegrationId} not found", integrationId);
                return false;
            }

            integration.Activate();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Integration {IntegrationId} activated successfully", integrationId);
            return true;
        }

        /// <summary>
        /// Deactivates an integration by setting its IsActive property to false
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deactivation was successful.</returns>
        public async Task<bool> DeactivateIntegrationAsync(string integrationId)
        {
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            _logger?.LogInformation("Deactivating integration {IntegrationId}", integrationId);

            var integration = await GetByIdAsync(integrationId);
            if (integration == null)
            {
                _logger?.LogWarning("Integration with ID {IntegrationId} not found", integrationId);
                return false;
            }

            integration.Deactivate();
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Integration {IntegrationId} deactivated successfully", integrationId);
            return true;
        }

        /// <summary>
        /// Updates or adds a specific setting for an integration in its AdditionalSettings dictionary
        /// </summary>
        /// <param name="integrationId">The identifier of the integration</param>
        /// <param name="key">The setting key</param>
        /// <param name="value">The setting value</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the setting update was successful.</returns>
        public async Task<bool> UpdateSettingAsync(string integrationId, string key, string value)
        {
            if (string.IsNullOrEmpty(integrationId))
            {
                throw new ArgumentException("Integration ID cannot be null or empty", nameof(integrationId));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Setting key cannot be null or empty", nameof(key));
            }

            _logger?.LogInformation("Updating setting {Key} for integration {IntegrationId}", key, integrationId);

            var integration = await GetByIdAsync(integrationId);
            if (integration == null)
            {
                _logger?.LogWarning("Integration with ID {IntegrationId} not found", integrationId);
                return false;
            }

            integration.AddSetting(key, value);
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Setting {Key} updated successfully for integration {IntegrationId}", key, integrationId);
            return true;
        }
    }
}