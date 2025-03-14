using Microsoft.Extensions.Options; // v6.0.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Azure.Communication.Email; // v1.0.0

namespace VatFilingPricingTool.Infrastructure.Integration.Email
{
    /// <summary>
    /// Interface defining the contract for email sending services
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email asynchronously
        /// </summary>
        /// <param name="to">The recipient email address</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>Result indicating success or failure of the email sending operation</returns>
        Task<Result> SendEmailAsync(string to, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Sends an email with multiple recipients asynchronously
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>Result indicating success or failure of the email sending operation</returns>
        Task<Result> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = false);

        /// <summary>
        /// Sends an email with attachments asynchronously
        /// </summary>
        /// <param name="to">The recipient email address</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="attachments">The list of email attachments</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>Result indicating success or failure of the email sending operation</returns>
        Task<Result> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments, bool isHtml = false);

        /// <summary>
        /// Sends an email using a template with placeholders
        /// </summary>
        /// <param name="to">The recipient email address</param>
        /// <param name="subject">The email subject</param>
        /// <param name="templateName">The name of the template to use</param>
        /// <param name="placeholders">Dictionary of placeholder names and values to replace in the template</param>
        /// <returns>Result indicating success or failure of the email sending operation</returns>
        Task<Result> SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> placeholders);
    }

    /// <summary>
    /// Represents an email attachment with content and metadata
    /// </summary>
    public class EmailAttachment
    {
        /// <summary>
        /// Gets or sets the file name of the attachment
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content of the attachment
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets the content type of the attachment
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Initializes a new instance of the EmailAttachment class
        /// </summary>
        /// <param name="fileName">The file name of the attachment</param>
        /// <param name="content">The content of the attachment</param>
        /// <param name="contentType">The content type of the attachment</param>
        public EmailAttachment(string fileName, byte[] content, string contentType)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = contentType ?? "application/octet-stream";
        }
    }

    /// <summary>
    /// Implementation of the IEmailSender interface that provides email sending capabilities
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _options;
        private readonly IRetryPolicy _retryPolicy;
        private readonly ILoggingService _logger;

        /// <summary>
        /// Initializes a new instance of the EmailSender class
        /// </summary>
        /// <param name="options">The email configuration options</param>
        /// <param name="logger">The logging service</param>
        public EmailSender(IOptions<EmailOptions> options, ILoggingService logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryPolicy = new RetryPolicy(_options.MaxRetryCount, TimeSpan.FromSeconds(2), true, _logger);
        }

        /// <inheritdoc />
        public async Task<Result> SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Recipient email address cannot be null or empty", nameof(to));
            }

            _logger.LogInformation($"Sending email to {to} with subject: {subject}");
            
            // Create a list with single recipient and call the overloaded method
            return await SendEmailAsync(new List<string> { to }, subject, body, isHtml);
        }

        /// <inheritdoc />
        public async Task<Result> SendEmailAsync(List<string> to, string subject, string body, bool isHtml = false)
        {
            try
            {
                ValidateEmailParameters(to, subject, body);
                _logger.LogInformation($"Sending email to {to.Count} recipients with subject: {subject}");

                // Use retry policy to handle transient failures
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        if (_options.UseAzureCommunicationServices)
                        {
                            await SendViaAzureCommunicationServicesAsync(to, subject, body, isHtml);
                        }
                        else
                        {
                            await SendViaSmtpAsync(to, subject, body, isHtml);
                        }

                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to send email", ex);
                        throw; // Rethrow for retry policy to handle
                    }
                }, "SendEmail");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email after retries: {ex.Message}", ex);
                return Result.Failure($"Failed to send email: {ex.Message}", ex.GetErrorCode());
            }
        }

        /// <inheritdoc />
        public async Task<Result> SendEmailWithAttachmentsAsync(string to, string subject, string body, List<EmailAttachment> attachments, bool isHtml = false)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Recipient email address cannot be null or empty", nameof(to));
            }

            if (attachments == null || attachments.Count == 0)
            {
                throw new ArgumentException("Attachments list cannot be null or empty", nameof(attachments));
            }

            _logger.LogInformation($"Sending email with {attachments.Count} attachments to {to} with subject: {subject}");
            
            try
            {
                // Use retry policy to handle transient failures
                return await _retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        if (_options.UseAzureCommunicationServices)
                        {
                            await SendViaAzureCommunicationServicesWithAttachmentsAsync(new List<string> { to }, subject, body, attachments, isHtml);
                        }
                        else
                        {
                            await SendViaSmtpWithAttachmentsAsync(new List<string> { to }, subject, body, attachments, isHtml);
                        }

                        return Result.Success();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to send email with attachments", ex);
                        throw; // Rethrow for retry policy to handle
                    }
                }, "SendEmailWithAttachments");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email with attachments after retries: {ex.Message}", ex);
                return Result.Failure($"Failed to send email with attachments: {ex.Message}", ex.GetErrorCode());
            }
        }

        /// <inheritdoc />
        public async Task<Result> SendTemplatedEmailAsync(string to, string subject, string templateName, Dictionary<string, string> placeholders)
        {
            if (string.IsNullOrWhiteSpace(to))
            {
                throw new ArgumentException("Recipient email address cannot be null or empty", nameof(to));
            }

            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Template name cannot be null or empty", nameof(templateName));
            }

            _logger.LogInformation($"Sending templated email using template '{templateName}' to {to} with subject: {subject}");

            try
            {
                // Load template content
                string templateContent = await LoadTemplateAsync(templateName);
                
                // Apply placeholders
                if (placeholders != null)
                {
                    templateContent = ApplyPlaceholders(templateContent, placeholders);
                }
                
                // Send email with processed template
                return await SendEmailAsync(to, subject, templateContent, _options.EnableHtmlEmails);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send templated email: {ex.Message}", ex);
                return Result.Failure($"Failed to send templated email: {ex.Message}", ex.GetErrorCode());
            }
        }

        /// <summary>
        /// Sends an email via SMTP
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SendViaSmtpAsync(List<string> to, string subject, string body, bool isHtml)
        {
            using (var smtpClient = new SmtpClient(_options.SmtpServer, _options.SmtpPort))
            {
                smtpClient.EnableSsl = _options.EnableSsl;
                smtpClient.Timeout = _options.ConnectionTimeoutSeconds * 1000;
                
                if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(_options.Username, _options.Password);
                }

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_options.SenderEmail, _options.SenderName);
                    
                    foreach (var recipient in to)
                    {
                        mailMessage.To.Add(new MailAddress(recipient));
                    }
                    
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = isHtml;
                    
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
        }

        /// <summary>
        /// Sends an email with attachments via SMTP
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="attachments">The list of email attachments</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SendViaSmtpWithAttachmentsAsync(List<string> to, string subject, string body, List<EmailAttachment> attachments, bool isHtml)
        {
            using (var smtpClient = new SmtpClient(_options.SmtpServer, _options.SmtpPort))
            {
                smtpClient.EnableSsl = _options.EnableSsl;
                smtpClient.Timeout = _options.ConnectionTimeoutSeconds * 1000;
                
                if (!string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password))
                {
                    smtpClient.Credentials = new System.Net.NetworkCredential(_options.Username, _options.Password);
                }

                using (var mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(_options.SenderEmail, _options.SenderName);
                    
                    foreach (var recipient in to)
                    {
                        mailMessage.To.Add(new MailAddress(recipient));
                    }
                    
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = isHtml;
                    
                    // Add all attachments to the mail message
                    foreach (var attachment in attachments)
                    {
                        var memoryStream = new MemoryStream(attachment.Content);
                        var mailAttachment = new Attachment(memoryStream, attachment.FileName, attachment.ContentType);
                        mailMessage.Attachments.Add(mailAttachment);
                    }
                    
                    await smtpClient.SendMailAsync(mailMessage);
                    
                    // Dispose attachments
                    foreach (var attachment in mailMessage.Attachments)
                    {
                        attachment.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Sends an email via Azure Communication Services
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SendViaAzureCommunicationServicesAsync(List<string> to, string subject, string body, bool isHtml)
        {
            // Create email client using the connection string
            var emailClient = new EmailClient(_options.AzureCommunicationServicesConnectionString);
            
            // Create email content with subject and body
            var emailContent = new EmailContent(subject);
            if (isHtml)
            {
                emailContent.Html = body;
            }
            else
            {
                emailContent.PlainText = body;
            }
            
            // Create recipients
            var emailRecipients = new EmailRecipients(to);
            
            // Create email message
            var emailMessage = new EmailMessage(
                senderAddress: _options.SenderEmail,
                content: emailContent,
                recipients: emailRecipients);
            
            // Send the email
            await emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            
            _logger.LogInformation("Email sent successfully via Azure Communication Services");
        }

        /// <summary>
        /// Sends an email with attachments via Azure Communication Services
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <param name="attachments">The list of email attachments</param>
        /// <param name="isHtml">Whether the body is HTML content</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task SendViaAzureCommunicationServicesWithAttachmentsAsync(List<string> to, string subject, string body, List<EmailAttachment> attachments, bool isHtml)
        {
            // Create email client using the connection string
            var emailClient = new EmailClient(_options.AzureCommunicationServicesConnectionString);
            
            // Create email content with subject and body
            var emailContent = new EmailContent(subject);
            if (isHtml)
            {
                emailContent.Html = body;
            }
            else
            {
                emailContent.PlainText = body;
            }
            
            // Create recipients
            var emailRecipients = new EmailRecipients(to);
            
            // Create Azure email attachments from our EmailAttachment objects
            var azureAttachments = new List<Azure.Communication.Email.EmailAttachment>();
            foreach (var attachment in attachments)
            {
                azureAttachments.Add(new Azure.Communication.Email.EmailAttachment(
                    attachment.FileName,
                    attachment.ContentType,
                    new BinaryData(attachment.Content)));
            }
            
            // Create email message with attachments
            var emailMessage = new EmailMessage(
                senderAddress: _options.SenderEmail,
                content: emailContent,
                recipients: emailRecipients);
            
            // Add attachments to the message
            foreach (var attachment in azureAttachments)
            {
                emailMessage.Attachments.Add(attachment);
            }
            
            // Send the email
            await emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
            
            _logger.LogInformation($"Email with {attachments.Count} attachments sent successfully via Azure Communication Services");
        }

        /// <summary>
        /// Loads an email template from the templates folder
        /// </summary>
        /// <param name="templateName">The name of the template to load</param>
        /// <returns>The content of the template</returns>
        private async Task<string> LoadTemplateAsync(string templateName)
        {
            string templatePath = Path.Combine(_options.TemplatesFolder, $"{templateName}.html");
            
            if (File.Exists(templatePath))
            {
                // Load template from file
                return await File.ReadAllTextAsync(templatePath);
            }
            
            // Check if this is a default template name from the EmailTemplates class
            var defaultTemplateNames = EmailTemplates.GetDefaultTemplateNames();
            if (defaultTemplateNames.Contains(templateName, StringComparer.OrdinalIgnoreCase))
            {
                // Return a basic template with placeholder
                return $"<html><body><p>This is a placeholder for the {templateName} template.</p></body></html>";
            }
            
            // Template not found
            throw new FileNotFoundException($"Email template '{templateName}' not found and no default template is available.", templatePath);
        }

        /// <summary>
        /// Applies placeholder values to a template
        /// </summary>
        /// <param name="template">The template content</param>
        /// <param name="placeholders">Dictionary of placeholder names and values</param>
        /// <returns>The template with placeholders replaced with values</returns>
        private string ApplyPlaceholders(string template, Dictionary<string, string> placeholders)
        {
            string result = template;
            
            foreach (var placeholder in placeholders)
            {
                // Replace all occurrences of {{PlaceholderName}} with the value
                result = result.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            }
            
            return result;
        }

        /// <summary>
        /// Validates email sending parameters
        /// </summary>
        /// <param name="to">The list of recipient email addresses</param>
        /// <param name="subject">The email subject</param>
        /// <param name="body">The email body</param>
        /// <exception cref="ArgumentException">Thrown when parameters are invalid</exception>
        private void ValidateEmailParameters(List<string> to, string subject, string body)
        {
            if (to == null || to.Count == 0)
            {
                throw new ArgumentException("Recipients list cannot be null or empty", nameof(to));
            }

            // Basic email format validation for each recipient
            foreach (var email in to)
            {
                if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                {
                    throw new ArgumentException($"Invalid email format: {email}", nameof(to));
                }
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentException("Subject cannot be null or empty", nameof(subject));
            }

            if (body == null)
            {
                throw new ArgumentException("Body cannot be null", nameof(body));
            }
        }
    }
}