using Microsoft.Extensions.Logging; // v6.0.0

namespace VatFilingPricingTool.Infrastructure.Logging
{
    /// <summary>
    /// Configuration options for logging in the VAT Filing Pricing Tool.
    /// Provides configuration for structured logging, correlation ID tracking,
    /// and various log destination options.
    /// </summary>
    public class LoggingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether console logging is enabled.
        /// </summary>
        public bool EnableConsoleLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether file logging is enabled.
        /// </summary>
        public bool EnableFileLogging { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Application Insights logging is enabled.
        /// </summary>
        public bool EnableApplicationInsights { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Azure Log Analytics logging is enabled.
        /// </summary>
        public bool EnableAzureLogAnalytics { get; set; }

        /// <summary>
        /// Gets or sets the minimum log level to capture.
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether sensitive data masking is enabled.
        /// When enabled, personally identifiable information (PII) and financial data 
        /// will be automatically redacted from log entries.
        /// </summary>
        public bool EnableSensitiveDataMasking { get; set; }

        /// <summary>
        /// Gets or sets the name of the HTTP header containing the correlation ID.
        /// This ID is used for tracking requests across multiple services.
        /// </summary>
        public string CorrelationIdHeaderName { get; set; }

        /// <summary>
        /// Gets or sets the file path for file logging.
        /// Only used when EnableFileLogging is set to true.
        /// </summary>
        public string FileLogPath { get; set; }

        /// <summary>
        /// Gets or sets the number of days to retain logs.
        /// </summary>
        public int RetentionDays { get; set; }

        /// <summary>
        /// Gets or sets the Application Insights connection string.
        /// Required when EnableApplicationInsights is set to true.
        /// </summary>
        public string ApplicationInsightsConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the Azure Log Analytics workspace ID.
        /// Required when EnableAzureLogAnalytics is set to true.
        /// </summary>
        public string LogAnalyticsWorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the Azure Log Analytics shared key.
        /// Required when EnableAzureLogAnalytics is set to true.
        /// </summary>
        public string LogAnalyticsSharedKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingOptions"/> class with default values.
        /// </summary>
        public LoggingOptions()
        {
            // Set default values for logging options
            EnableConsoleLogging = true;
            EnableFileLogging = false;
            EnableApplicationInsights = true;
            EnableAzureLogAnalytics = false;
            LogLevel = LogLevel.Information;
            EnableSensitiveDataMasking = true;
            CorrelationIdHeaderName = "X-Correlation-ID";
            RetentionDays = 30;
        }
    }
}