using System;
using System.Collections.Generic;

namespace VatFilingPricingTool.Infrastructure.Integration.ERP
{
    /// <summary>
    /// Base configuration options for ERP system integration in the VAT Filing Pricing Tool.
    /// Provides settings for authentication, connection parameters, and resilience policies.
    /// </summary>
    public class ErpOptions
    {
        /// <summary>
        /// Gets or sets the client ID for OAuth authentication with the ERP system.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret for OAuth authentication with the ERP system.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID for Azure AD authentication.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the authority URL for authentication (e.g., Azure AD endpoint).
        /// </summary>
        public string AuthorityUrl { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed operations.
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the delay between retry attempts in milliseconds.
        /// Will be used with exponential backoff based on retry count.
        /// </summary>
        public int RetryDelayMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process ERP operations in the background.
        /// </summary>
        public bool UseBackgroundProcessing { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErpOptions"/> class with default values.
        /// </summary>
        public ErpOptions()
        {
            ConnectionTimeoutSeconds = 30;
            MaxRetryCount = 5;
            RetryDelayMilliseconds = 1000;
            UseBackgroundProcessing = false;
            AuthorityUrl = "https://login.microsoftonline.com/";
        }
    }

    /// <summary>
    /// Configuration options specific to Microsoft Dynamics 365 integration.
    /// Extends the base ERP options with Dynamics-specific settings.
    /// </summary>
    public class DynamicsOptions
    {
        /// <summary>
        /// Gets or sets the Dynamics 365 organization URL.
        /// </summary>
        public string OrganizationUrl { get; set; }

        /// <summary>
        /// Gets or sets the Dynamics 365 API version to use.
        /// </summary>
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping logical entity names to physical entity names in Dynamics 365.
        /// </summary>
        public Dictionary<string, string> Entities { get; set; }

        /// <summary>
        /// Gets or sets the default entity type to use when not specified.
        /// </summary>
        public string DefaultEntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use batch requests for multiple operations.
        /// </summary>
        public bool UseBatchRequests { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of operations in a single batch request.
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicsOptions"/> class with default values.
        /// </summary>
        public DynamicsOptions()
        {
            ApiVersion = "v9.2";
            UseBatchRequests = true;
            BatchSize = 50;
            DefaultEntityType = "invoice";
            Entities = new Dictionary<string, string>();
        }
    }
}