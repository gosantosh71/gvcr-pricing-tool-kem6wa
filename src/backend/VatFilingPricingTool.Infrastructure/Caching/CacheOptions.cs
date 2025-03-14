using System; // System v6.0.0

namespace VatFilingPricingTool.Infrastructure.Caching
{
    /// <summary>
    /// Configuration options for Redis cache connection and behavior in the VAT Filing Pricing Tool.
    /// These settings control cache connections, performance parameters, and expiration policies.
    /// </summary>
    public class CacheOptions
    {
        /// <summary>
        /// Gets or sets the Redis connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Redis instance name, used for key prefixing.
        /// </summary>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether caching is enabled.
        /// When disabled, the application will bypass cache operations.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the default expiration time for cache entries in minutes.
        /// </summary>
        public int DefaultExpirationMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of times to retry connection to Redis when connection fails.
        /// </summary>
        public int ConnectionRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to abort connection if 
        /// the connection to Redis fails. When set to false, operations will 
        /// degrade gracefully when Redis is unavailable.
        /// </summary>
        public bool AbortOnConnectFail { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheOptions"/> class 
        /// with default values appropriate for the VAT Filing Pricing Tool.
        /// </summary>
        public CacheOptions()
        {
            // Set default values for cache behavior
            Enabled = true;
            DefaultExpirationMinutes = 30;
            ConnectionRetryCount = 3;
            ConnectionTimeoutSeconds = 5;
            AbortOnConnectFail = false;
            InstanceName = "VatFilingPricingTool";
        }
    }
}