using System; // Version 6.0.0 - Core .NET functionality

namespace VatFilingPricingTool.Infrastructure.Integration.Email
{
    /// <summary>
    /// Configuration options for email integration in the VAT Filing Pricing Tool.
    /// Provides settings for both SMTP and Azure Communication Services email delivery.
    /// </summary>
    public class EmailOptions
    {
        /// <summary>
        /// Gets or sets the SMTP server hostname or IP address.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server port. Default is 587 (TLS).
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SSL is enabled for SMTP connections.
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// Gets or sets the sender email address used in the From field.
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Gets or sets the sender display name used in the From field.
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets the username for SMTP authentication.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for SMTP authentication.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use Azure Communication Services instead of SMTP.
        /// When true, Azure Communication Services will be used for sending emails.
        /// </summary>
        public bool UseAzureCommunicationServices { get; set; }

        /// <summary>
        /// Gets or sets the Azure Communication Services connection string.
        /// Required when UseAzureCommunicationServices is set to true.
        /// </summary>
        public string AzureCommunicationServicesConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed email sends.
        /// Implements automatic retries for transient failures with exponential backoff.
        /// </summary>
        public int MaxRetryCount { get; set; }

        /// <summary>
        /// Gets or sets the folder path where email templates are stored.
        /// Templates are used for standardized email communications.
        /// </summary>
        public string TemplatesFolder { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds for SMTP operations.
        /// </summary>
        public int ConnectionTimeoutSeconds { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether email verification is enabled.
        /// When true, verification emails will be sent for new user registrations.
        /// </summary>
        public bool EnableEmailVerification { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether HTML emails are enabled.
        /// When true, emails can contain HTML formatting; otherwise, plain text is used.
        /// </summary>
        public bool EnableHtmlEmails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailOptions"/> class with default values.
        /// </summary>
        public EmailOptions()
        {
            // Set default SMTP settings
            SmtpPort = 587; // Default SMTP port with TLS
            EnableSsl = true;
            
            // Set default retry policy
            MaxRetryCount = 3;
            
            // Set default connection settings
            ConnectionTimeoutSeconds = 30;
            
            // Set default provider settings
            UseAzureCommunicationServices = false;
            
            // Set default email behavior
            EnableEmailVerification = true;
            EnableHtmlEmails = true;
            
            // Set default template location
            TemplatesFolder = "EmailTemplates";
        }
    }
}