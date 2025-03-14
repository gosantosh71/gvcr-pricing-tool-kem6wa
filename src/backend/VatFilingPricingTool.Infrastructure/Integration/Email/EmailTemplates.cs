using Microsoft.Extensions.Options; // v6.0.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace VatFilingPricingTool.Infrastructure.Integration.Email
{
    /// <summary>
    /// Manages email templates and placeholders for the VAT Filing Pricing Tool.
    /// Provides a centralized repository of email templates used for various system notifications,
    /// report delivery, and user communications.
    /// </summary>
    public class EmailTemplates
    {
        private readonly EmailOptions _options;
        private readonly ILoggingService _logger;
        private readonly Dictionary<string, string> _defaultTemplates;

        /// <summary>
        /// Initializes a new instance of the EmailTemplates class.
        /// </summary>
        /// <param name="options">The email configuration options.</param>
        /// <param name="logger">The logging service.</param>
        public EmailTemplates(IOptions<EmailOptions> options, ILoggingService logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _defaultTemplates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            InitializeDefaultTemplates();
            _logger.LogInformation("Email templates initialized", new { TemplateCount = _defaultTemplates.Count });
        }

        /// <summary>
        /// Gets a list of all available default template names.
        /// </summary>
        /// <returns>Collection of template names.</returns>
        public IEnumerable<string> GetDefaultTemplateNames()
        {
            return _defaultTemplates.Keys;
        }

        /// <summary>
        /// Gets the content of a default template by name.
        /// </summary>
        /// <param name="templateName">The name of the template to retrieve.</param>
        /// <returns>The template content.</returns>
        /// <exception cref="ArgumentException">Thrown if the template name is not found.</exception>
        public string GetDefaultTemplate(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name cannot be null or empty.", nameof(templateName));
            }

            if (_defaultTemplates.TryGetValue(templateName, out string templateContent))
            {
                return templateContent;
            }

            throw new ArgumentException($"Template '{templateName}' was not found.", nameof(templateName));
        }

        /// <summary>
        /// Gets the default placeholders for a specific template type.
        /// </summary>
        /// <param name="templateName">The name of the template.</param>
        /// <returns>Dictionary of placeholder names and default values.</returns>
        public Dictionary<string, string> GetDefaultPlaceholders(string templateName)
        {
            var placeholders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Common placeholders
                { "AppName", "VAT Filing Pricing Tool" },
                { "CurrentDate", DateTime.UtcNow.ToString("yyyy-MM-dd") },
                { "SupportEmail", "support@vatfilingpricing.com" },
                { "CompanyName", "VAT Filing Services Ltd." },
                { "Year", DateTime.UtcNow.Year.ToString() }
            };

            // Template-specific placeholders
            switch (templateName.ToLowerInvariant())
            {
                case "welcomeemail":
                    placeholders.Add("UserName", "[User Name]");
                    placeholders.Add("LoginUrl", "[Login URL]");
                    break;
                case "passwordreset":
                    placeholders.Add("UserName", "[User Name]");
                    placeholders.Add("ResetLink", "[Reset Link]");
                    placeholders.Add("ExpirationHours", "24");
                    break;
                case "reportready":
                    placeholders.Add("UserName", "[User Name]");
                    placeholders.Add("ReportName", "[Report Name]");
                    placeholders.Add("ReportUrl", "[Report URL]");
                    placeholders.Add("GenerationDate", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case "accountverification":
                    placeholders.Add("UserName", "[User Name]");
                    placeholders.Add("VerificationLink", "[Verification Link]");
                    placeholders.Add("ExpirationHours", "48");
                    break;
                case "pricingestimate":
                    placeholders.Add("UserName", "[User Name]");
                    placeholders.Add("EstimateName", "[Estimate Name]");
                    placeholders.Add("TotalAmount", "[Total Amount]");
                    placeholders.Add("Currency", "EUR");
                    placeholders.Add("EstimateUrl", "[Estimate URL]");
                    placeholders.Add("Countries", "[Countries]");
                    break;
                case "systemnotification":
                    placeholders.Add("NotificationType", "[Notification Type]");
                    placeholders.Add("NotificationMessage", "[Notification Message]");
                    placeholders.Add("ActionRequired", "No");
                    break;
                default:
                    _logger.LogInformation($"No specific placeholders for template '{templateName}', using common placeholders only");
                    break;
            }

            return placeholders;
        }

        /// <summary>
        /// Loads a template from a file in the templates folder.
        /// </summary>
        /// <param name="templateName">The name of the template file to load.</param>
        /// <returns>The template content.</returns>
        public async Task<string> LoadTemplateFromFile(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name cannot be null or empty.", nameof(templateName));
            }

            string templatePath = GetTemplatePath(templateName);
            
            if (File.Exists(templatePath))
            {
                try
                {
                    string templateContent = await File.ReadAllTextAsync(templatePath);
                    _logger.LogInformation($"Template loaded from file: {templatePath}");
                    return templateContent;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error loading template from file: {templatePath}", ex);
                    
                    // Fall back to default template if available
                    if (_defaultTemplates.TryGetValue(templateName, out string defaultTemplate))
                    {
                        _logger.LogInformation($"Using default template for '{templateName}' due to file loading error");
                        return defaultTemplate;
                    }
                    
                    throw; // Rethrow if no default template is available
                }
            }
            else
            {
                _logger.LogInformation($"Template file not found: {templatePath}, using default template if available");
                
                // Return default template if available
                if (_defaultTemplates.TryGetValue(templateName, out string defaultTemplate))
                {
                    return defaultTemplate;
                }
                
                throw new FileNotFoundException($"Template file not found and no default template is available for '{templateName}'", templatePath);
            }
        }

        /// <summary>
        /// Applies placeholder values to a template.
        /// </summary>
        /// <param name="template">The template content with placeholders.</param>
        /// <param name="placeholders">Dictionary of placeholder names and values.</param>
        /// <returns>The processed template with placeholders replaced.</returns>
        public string ApplyPlaceholders(string template, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                return template;
            }

            if (placeholders == null || placeholders.Count == 0)
            {
                return template;
            }

            string result = template;
            
            foreach (var placeholder in placeholders)
            {
                // Replace {{PlaceholderName}} with its value
                result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        /// <summary>
        /// Initializes the default templates dictionary with standard email templates.
        /// </summary>
        private void InitializeDefaultTemplates()
        {
            // Welcome Email
            _defaultTemplates["WelcomeEmail"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Welcome to {{AppName}}</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>Welcome to {{AppName}}</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello {{UserName}},</p>
        <p>Thank you for registering with {{AppName}}. We're excited to have you on board!</p>
        <p>With our platform, you can:</p>
        <ul>
            <li>Calculate VAT filing costs across multiple jurisdictions</li>
            <li>Generate detailed pricing reports</li>
            <li>Track historical pricing estimates</li>
        </ul>
        <p>To get started, please click the button below to access your account:</p>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{{LoginUrl}}"" style=""background-color: #0078D4; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">Login to Your Account</a>
        </div>
        <p>If you have any questions or need assistance, please contact our support team at {{SupportEmail}}.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
        <p>This email was sent to you because you registered for an account with {{AppName}}.</p>
    </footer>
</body>
</html>";

            // Password Reset
            _defaultTemplates["PasswordReset"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Password Reset Request</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>Password Reset Request</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello {{UserName}},</p>
        <p>We received a request to reset your password for your {{AppName}} account.</p>
        <p>To reset your password, please click the button below:</p>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{{ResetLink}}"" style=""background-color: #0078D4; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">Reset Password</a>
        </div>
        <p>This link will expire in {{ExpirationHours}} hours.</p>
        <p>If you did not request a password reset, please ignore this email or contact our support team at {{SupportEmail}} if you have concerns.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
    </footer>
</body>
</html>";

            // Report Ready
            _defaultTemplates["ReportReady"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Your Report is Ready</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>Your Report is Ready</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello {{UserName}},</p>
        <p>Your report <strong>{{ReportName}}</strong> has been generated and is now ready for viewing.</p>
        <p>Report details:</p>
        <ul>
            <li><strong>Report Name:</strong> {{ReportName}}</li>
            <li><strong>Generation Date:</strong> {{GenerationDate}}</li>
        </ul>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{{ReportUrl}}"" style=""background-color: #0078D4; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">View Report</a>
        </div>
        <p>If you have any questions about this report, please contact our support team at {{SupportEmail}}.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
    </footer>
</body>
</html>";

            // Account Verification
            _defaultTemplates["AccountVerification"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Verify Your Account</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>Verify Your Account</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello {{UserName}},</p>
        <p>Thank you for creating an account with {{AppName}}. To complete your registration, please verify your email address by clicking the button below:</p>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{{VerificationLink}}"" style=""background-color: #0078D4; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">Verify Email Address</a>
        </div>
        <p>This link will expire in {{ExpirationHours}} hours.</p>
        <p>If you did not create an account with us, please ignore this email or contact our support team at {{SupportEmail}}.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
    </footer>
</body>
</html>";

            // Pricing Estimate
            _defaultTemplates["PricingEstimate"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Your VAT Filing Pricing Estimate</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>Your VAT Filing Pricing Estimate</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello {{UserName}},</p>
        <p>Your VAT filing pricing estimate <strong>{{EstimateName}}</strong> has been created.</p>
        <div style=""background-color: #f9f9f9; border: 1px solid #ddd; padding: 15px; margin: 20px 0; border-radius: 4px;"">
            <h2 style=""margin-top: 0; color: #0078D4;"">Estimate Summary</h2>
            <p><strong>Total Amount:</strong> {{Currency}} {{TotalAmount}}</p>
            <p><strong>Countries:</strong> {{Countries}}</p>
            <p><strong>Date:</strong> {{CurrentDate}}</p>
        </div>
        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{{EstimateUrl}}"" style=""background-color: #0078D4; color: white; padding: 12px 20px; text-decoration: none; border-radius: 4px; font-weight: bold;"">View Detailed Estimate</a>
        </div>
        <p>If you have any questions about this estimate, please contact our support team at {{SupportEmail}}.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
    </footer>
</body>
</html>";

            // System Notification
            _defaultTemplates["SystemNotification"] = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>{{NotificationType}} - {{AppName}}</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto;"">
    <header style=""background-color: #0078D4; padding: 20px; color: white; text-align: center;"">
        <h1>{{NotificationType}}</h1>
    </header>
    <main style=""padding: 20px;"">
        <p>Hello,</p>
        <p>{{NotificationMessage}}</p>
        <p><strong>Action Required:</strong> {{ActionRequired}}</p>
        <p>If you have any questions, please contact our support team at {{SupportEmail}}.</p>
    </main>
    <footer style=""background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #666;"">
        <p>&copy; {{Year}} {{CompanyName}}. All rights reserved.</p>
        <p>This is an automated system notification from {{AppName}}.</p>
    </footer>
</body>
</html>";
        }
        
        /// <summary>
        /// Gets the full path to a template file.
        /// </summary>
        /// <param name="templateName">The name of the template file.</param>
        /// <returns>The full path to the template file.</returns>
        private string GetTemplatePath(string templateName)
        {
            string templateFileName = templateName;
            
            // Ensure the template name has the .html extension
            if (!templateFileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                templateFileName += ".html";
            }
            
            return Path.Combine(_options.TemplatesFolder, templateFileName);
        }
    }
}