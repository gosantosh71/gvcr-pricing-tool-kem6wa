using System; // System package version 6.0.0
using System.Collections.Generic; // System.Collections.Generic package version 6.0.0

namespace VatFilingPricingTool.Domain.Entities
{
    /// <summary>
    /// Represents a connection to an external system such as ERP (Microsoft Dynamics 365) 
    /// or OCR services (Azure Cognitive Services) in the VAT Filing Pricing Tool.
    /// This entity stores connection details, authentication credentials, and configuration
    /// settings needed to interact with external systems for data import and document processing.
    /// </summary>
    public class Integration
    {
        /// <summary>
        /// Gets or sets the unique identifier for this integration.
        /// </summary>
        public string IntegrationId { get; private set; }

        /// <summary>
        /// Gets or sets the identifier of the user who owns this integration.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets or sets the type of the external system (e.g., "Dynamics365", "CognitiveServices").
        /// </summary>
        public string SystemType { get; private set; }

        /// <summary>
        /// Gets or sets the connection string used to connect to the external system.
        /// This may contain sensitive information and should be encrypted when stored.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets or sets the date and time of the last successful synchronization with the external system.
        /// </summary>
        public DateTime LastSyncDate { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this integration is currently active.
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the number of connection retry attempts made since the last successful connection.
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// Gets or sets the API key used for authentication with the external system.
        /// </summary>
        public string ApiKey { get; private set; }

        /// <summary>
        /// Gets or sets the API endpoint URL for the external system.
        /// </summary>
        public string ApiEndpoint { get; private set; }

        /// <summary>
        /// Gets or sets additional configuration settings for this integration.
        /// </summary>
        public Dictionary<string, string> AdditionalSettings { get; private set; }

        /// <summary>
        /// Gets or sets the user who owns this integration.
        /// </summary>
        public User User { get; private set; }

        /// <summary>
        /// Default constructor for the Integration entity.
        /// </summary>
        protected Integration()
        {
            AdditionalSettings = new Dictionary<string, string>();
            LastSyncDate = DateTime.UtcNow;
            IsActive = false;
            RetryCount = 0;
        }

        /// <summary>
        /// Factory method to create a new Integration instance.
        /// </summary>
        /// <param name="userId">The identifier of the user who owns this integration.</param>
        /// <param name="systemType">The type of the external system.</param>
        /// <param name="connectionString">The connection string used to connect to the external system.</param>
        /// <returns>A new Integration instance.</returns>
        public static Integration Create(string userId, string systemType, string connectionString)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(systemType))
            {
                throw new ArgumentException("System type cannot be null or empty", nameof(systemType));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            var integration = new Integration
            {
                IntegrationId = Guid.NewGuid().ToString(),
                UserId = userId,
                SystemType = systemType,
                ConnectionString = connectionString,
                LastSyncDate = DateTime.UtcNow,
                IsActive = false,
                RetryCount = 0,
                AdditionalSettings = new Dictionary<string, string>()
            };

            return integration;
        }

        /// <summary>
        /// Updates the last synchronization date to the current UTC time.
        /// </summary>
        public void UpdateLastSyncDate()
        {
            LastSyncDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Increments the retry count and returns the updated value.
        /// </summary>
        /// <returns>The updated retry count.</returns>
        public int IncrementRetryCount()
        {
            RetryCount++;
            return RetryCount;
        }

        /// <summary>
        /// Resets the retry count to zero.
        /// </summary>
        public void ResetRetryCount()
        {
            RetryCount = 0;
        }

        /// <summary>
        /// Activates the integration.
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the integration.
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Adds or updates a setting in the AdditionalSettings dictionary.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        public void AddSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Setting key cannot be null or empty", nameof(key));
            }

            if (AdditionalSettings.ContainsKey(key))
            {
                AdditionalSettings[key] = value;
            }
            else
            {
                AdditionalSettings.Add(key, value);
            }
        }

        /// <summary>
        /// Removes a setting from the AdditionalSettings dictionary.
        /// </summary>
        /// <param name="key">The setting key to remove.</param>
        /// <returns>True if the setting was removed, false if it didn't exist.</returns>
        public bool RemoveSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Setting key cannot be null or empty", nameof(key));
            }

            return AdditionalSettings.Remove(key);
        }

        /// <summary>
        /// Gets a setting value from the AdditionalSettings dictionary.
        /// </summary>
        /// <param name="key">The setting key.</param>
        /// <param name="defaultValue">The default value to return if the key is not found.</param>
        /// <returns>The setting value or defaultValue if not found.</returns>
        public string GetSetting(string key, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Setting key cannot be null or empty", nameof(key));
            }

            return AdditionalSettings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Updates the connection details for the integration.
        /// </summary>
        /// <param name="connectionString">The new connection string (if null or empty, the existing value is not changed).</param>
        /// <param name="apiKey">The new API key (if null, the existing value is not changed).</param>
        /// <param name="apiEndpoint">The new API endpoint (if null, the existing value is not changed).</param>
        public void UpdateConnectionDetails(string connectionString = null, string apiKey = null, string apiEndpoint = null)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                ConnectionString = connectionString;
            }

            if (apiKey != null)
            {
                ApiKey = apiKey;
            }

            if (apiEndpoint != null)
            {
                ApiEndpoint = apiEndpoint;
            }
        }
    }
}