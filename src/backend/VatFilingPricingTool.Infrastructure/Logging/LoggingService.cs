using Microsoft.ApplicationInsights; // v2.20.0
using Microsoft.ApplicationInsights.DataContracts; // v2.20.0
using Microsoft.Extensions.Logging; // v6.0.0
using Microsoft.Extensions.Options; // v6.0.0
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VatFilingPricingTool.Common.Extensions;

namespace VatFilingPricingTool.Infrastructure.Logging
{
    /// <summary>
    /// Interface defining the contract for logging services in the VAT Filing Pricing Tool
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Logs an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking the request across services</param>
        void LogInformation(string message, object data = null, string correlationId = null);

        /// <summary>
        /// Logs a warning message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking the request across services</param>
        void LogWarning(string message, object data = null, string correlationId = null);

        /// <summary>
        /// Logs an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception that caused the error</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking the request across services</param>
        void LogError(string message, Exception exception = null, object data = null, string correlationId = null);

        /// <summary>
        /// Logs a critical error message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception that caused the error</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking the request across services</param>
        void LogCritical(string message, Exception exception = null, object data = null, string correlationId = null);

        /// <summary>
        /// Logs an audit event for security and compliance tracking
        /// </summary>
        /// <param name="action">The action being performed</param>
        /// <param name="userId">The ID of the user performing the action</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking the request across services</param>
        void LogAudit(string action, string userId, object data = null, string correlationId = null);
    }

    /// <summary>
    /// Implementation of the ILoggingService interface providing centralized logging functionality
    /// with support for multiple destinations and structured logging
    /// </summary>
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly LoggingOptions _options;
        private readonly TelemetryClient _telemetryClient;
        private readonly HttpClient _httpClient;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the LoggingService class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="options">Logging configuration options</param>
        public LoggingService(ILogger<LoggingService> logger, IOptions<LoggingOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

            // Initialize HttpClient for Azure Log Analytics if enabled
            if (_options.EnableAzureLogAnalytics)
            {
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("Log-Type", "VatFilingPricingToolLogs");
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            }

            // Initialize Application Insights telemetry client if enabled
            if (_options.EnableApplicationInsights && !string.IsNullOrEmpty(_options.ApplicationInsightsConnectionString))
            {
                _telemetryClient = new TelemetryClient
                {
                    InstrumentationKey = _options.ApplicationInsightsConnectionString
                };
            }

            // Create log directory if file logging is enabled
            if (_options.EnableFileLogging && !string.IsNullOrEmpty(_options.FileLogPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(_options.FileLogPath, "dummy.txt")));
            }
        }

        /// <inheritdoc />
        public void LogInformation(string message, object data = null, string correlationId = null)
        {
            var logEntry = CreateLogEntry(message, LogLevel.Information, null, data, correlationId);
            WriteLogEntry(logEntry, LogLevel.Information);
        }

        /// <inheritdoc />
        public void LogWarning(string message, object data = null, string correlationId = null)
        {
            var logEntry = CreateLogEntry(message, LogLevel.Warning, null, data, correlationId);
            WriteLogEntry(logEntry, LogLevel.Warning);
        }

        /// <inheritdoc />
        public void LogError(string message, Exception exception = null, object data = null, string correlationId = null)
        {
            var logEntry = CreateLogEntry(message, LogLevel.Error, exception, data, correlationId);
            WriteLogEntry(logEntry, LogLevel.Error);
        }

        /// <inheritdoc />
        public void LogCritical(string message, Exception exception = null, object data = null, string correlationId = null)
        {
            var logEntry = CreateLogEntry(message, LogLevel.Critical, exception, data, correlationId);
            WriteLogEntry(logEntry, LogLevel.Critical);
        }

        /// <inheritdoc />
        public void LogAudit(string action, string userId, object data = null, string correlationId = null)
        {
            var logEntry = CreateLogEntry(action, LogLevel.Information, null, data, correlationId, true);
            logEntry["userId"] = userId;
            logEntry["auditType"] = "SecurityAudit";
            logEntry["timestamp"] = DateTime.UtcNow.ToString("o");
            
            WriteLogEntry(logEntry, LogLevel.Information);
        }

        /// <summary>
        /// Creates a structured log entry with consistent format
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">The log level</param>
        /// <param name="exception">The exception (if any)</param>
        /// <param name="data">Additional contextual data</param>
        /// <param name="correlationId">Correlation ID for tracking</param>
        /// <param name="isAudit">Whether this is an audit log entry</param>
        /// <returns>A dictionary representing the structured log entry</returns>
        private Dictionary<string, object> CreateLogEntry(
            string message, 
            LogLevel level, 
            Exception exception = null, 
            object data = null, 
            string correlationId = null,
            bool isAudit = false)
        {
            var logEntry = new Dictionary<string, object>
            {
                ["timestamp"] = DateTime.UtcNow.ToString("o"),
                ["level"] = level.ToString(),
                ["message"] = message
            };

            // Add correlation ID if provided
            if (!string.IsNullOrEmpty(correlationId))
            {
                logEntry["correlationId"] = correlationId;
            }

            // Add exception details if provided
            if (exception != null)
            {
                logEntry["exception"] = new Dictionary<string, object>
                {
                    ["type"] = exception.GetType().Name,
                    ["message"] = exception.GetDetailedMessage(),
                    ["stackTrace"] = exception.StackTrace,
                    ["errorCode"] = exception.GetErrorCode()
                };
            }

            // Add contextual data if provided
            if (data != null)
            {
                logEntry["data"] = data;
            }

            // Add audit-specific fields
            if (isAudit)
            {
                logEntry["isAudit"] = true;
                logEntry["action"] = message;
            }

            // Apply sensitive data masking if enabled
            if (_options.EnableSensitiveDataMasking)
            {
                return MaskSensitiveData(logEntry);
            }

            return logEntry;
        }

        /// <summary>
        /// Writes a log entry to all configured destinations
        /// </summary>
        /// <param name="logEntry">The structured log entry</param>
        /// <param name="level">The log level</param>
        private void WriteLogEntry(Dictionary<string, object> logEntry, LogLevel level)
        {
            // Check if log level meets minimum level requirement
            if (level < _options.LogLevel)
            {
                return;
            }

            // Write to console if enabled
            if (_options.EnableConsoleLogging)
            {
                WriteToConsole(logEntry, level);
            }

            // Write to file if enabled
            if (_options.EnableFileLogging && !string.IsNullOrEmpty(_options.FileLogPath))
            {
                WriteToFile(logEntry);
            }

            // Write to Application Insights if enabled
            if (_options.EnableApplicationInsights && _telemetryClient != null)
            {
                WriteToApplicationInsights(logEntry, level);
            }

            // Write to Azure Log Analytics if enabled
            if (_options.EnableAzureLogAnalytics && 
                !string.IsNullOrEmpty(_options.LogAnalyticsWorkspaceId) && 
                !string.IsNullOrEmpty(_options.LogAnalyticsSharedKey) && 
                _httpClient != null)
            {
                WriteToLogAnalytics(logEntry);
            }
        }

        /// <summary>
        /// Writes a log entry to the console with appropriate color coding
        /// </summary>
        /// <param name="logEntry">The structured log entry</param>
        /// <param name="level">The log level</param>
        private void WriteToConsole(Dictionary<string, object> logEntry, LogLevel level)
        {
            ConsoleColor originalColor = Console.ForegroundColor;

            // Set console color based on log level
            switch (level)
            {
                case LogLevel.Trace:
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Information:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                default:
                    break;
            }

            try
            {
                // Format the log entry as JSON and write to console
                string json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = false });
                Console.WriteLine(json);
            }
            finally
            {
                // Reset console color
                Console.ForegroundColor = originalColor;
            }
        }

        /// <summary>
        /// Writes a log entry to a file with date-based naming
        /// </summary>
        /// <param name="logEntry">The structured log entry</param>
        private void WriteToFile(Dictionary<string, object> logEntry)
        {
            try
            {
                // Create file path with date-based naming
                string fileName = $"log-{DateTime.UtcNow:yyyy-MM-dd}.json";
                string filePath = Path.Combine(_options.FileLogPath, fileName);

                // Format the log entry as JSON
                string json = JsonSerializer.Serialize(logEntry, new JsonSerializerOptions { WriteIndented = false });

                // Append log entry to file with thread-safe locking
                lock (_lockObject)
                {
                    File.AppendAllText(filePath, json + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Log to fallback logger if file writing fails
                _logger.LogError(ex, "Failed to write log entry to file");
            }
        }

        /// <summary>
        /// Writes a log entry to Application Insights
        /// </summary>
        /// <param name="logEntry">The structured log entry</param>
        /// <param name="level">The log level</param>
        private void WriteToApplicationInsights(Dictionary<string, object> logEntry, LogLevel level)
        {
            try
            {
                // Create telemetry properties from log entry
                var properties = new Dictionary<string, string>();
                
                foreach (var kvp in logEntry)
                {
                    if (kvp.Value != null)
                    {
                        if (kvp.Value is string str)
                        {
                            properties[kvp.Key] = str;
                        }
                        else
                        {
                            properties[kvp.Key] = JsonSerializer.Serialize(kvp.Value);
                        }
                    }
                }

                // Get correlation ID if it exists
                string correlationId = logEntry.ContainsKey("correlationId") ? 
                    logEntry["correlationId"]?.ToString() : null;

                // Create appropriate telemetry based on log level
                switch (level)
                {
                    case LogLevel.Critical:
                    case LogLevel.Error:
                        var exceptionTelemetry = new ExceptionTelemetry
                        {
                            Message = logEntry["message"]?.ToString(),
                            SeverityLevel = level == LogLevel.Critical ? 
                                Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Critical : 
                                Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error
                        };

                        foreach (var prop in properties)
                        {
                            exceptionTelemetry.Properties.Add(prop.Key, prop.Value);
                        }

                        if (!string.IsNullOrEmpty(correlationId))
                        {
                            exceptionTelemetry.Context.Operation.Id = correlationId;
                        }

                        _telemetryClient.TrackException(exceptionTelemetry);
                        break;

                    case LogLevel.Warning:
                        var warningTelemetry = new TraceTelemetry(
                            logEntry["message"]?.ToString(), 
                            Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning);

                        foreach (var prop in properties)
                        {
                            warningTelemetry.Properties.Add(prop.Key, prop.Value);
                        }

                        if (!string.IsNullOrEmpty(correlationId))
                        {
                            warningTelemetry.Context.Operation.Id = correlationId;
                        }

                        _telemetryClient.TrackTrace(warningTelemetry);
                        break;

                    default:
                        // Check if this is an audit event
                        if (logEntry.ContainsKey("isAudit") && logEntry["isAudit"] is bool isAudit && isAudit)
                        {
                            var eventTelemetry = new EventTelemetry(logEntry["action"]?.ToString());
                            
                            foreach (var prop in properties)
                            {
                                eventTelemetry.Properties.Add(prop.Key, prop.Value);
                            }

                            if (!string.IsNullOrEmpty(correlationId))
                            {
                                eventTelemetry.Context.Operation.Id = correlationId;
                            }

                            _telemetryClient.TrackEvent(eventTelemetry);
                        }
                        else
                        {
                            var traceTelemetry = new TraceTelemetry(
                                logEntry["message"]?.ToString(), 
                                Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);

                            foreach (var prop in properties)
                            {
                                traceTelemetry.Properties.Add(prop.Key, prop.Value);
                            }

                            if (!string.IsNullOrEmpty(correlationId))
                            {
                                traceTelemetry.Context.Operation.Id = correlationId;
                            }

                            _telemetryClient.TrackTrace(traceTelemetry);
                        }
                        break;
                }

                // Flush telemetry to ensure immediate sending
                _telemetryClient.Flush();
            }
            catch (Exception ex)
            {
                // Log to fallback logger if Application Insights logging fails
                _logger.LogError(ex, "Failed to write log entry to Application Insights");
            }
        }

        /// <summary>
        /// Writes a log entry to Azure Log Analytics workspace
        /// </summary>
        /// <param name="logEntry">The structured log entry</param>
        private void WriteToLogAnalytics(Dictionary<string, object> logEntry)
        {
            try
            {
                // Format the log entry as JSON
                string json = JsonSerializer.Serialize(new[] { logEntry });
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Create request to Log Analytics Data Collector API
                string date = DateTime.UtcNow.ToString("r");
                string signature = GenerateLogAnalyticsSignature(
                    _options.LogAnalyticsWorkspaceId,
                    _options.LogAnalyticsSharedKey,
                    date,
                    jsonBytes.Length);

                string url = $"https://{_options.LogAnalyticsWorkspaceId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01";
                
                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Add("Authorization", signature);
                    request.Headers.Add("Log-Type", "VatFilingPricingToolLogs");
                    request.Headers.Add("x-ms-date", date);
                    request.Headers.Add("time-generated-field", "timestamp");
                    request.Content = new ByteArrayContent(jsonBytes);
                    request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                    // Send the request
                    var response = _httpClient.SendAsync(request).Result;
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        // Log to fallback logger if Log Analytics request fails
                        _logger.LogWarning($"Failed to write log entry to Azure Log Analytics. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log to fallback logger if Log Analytics logging fails
                _logger.LogError(ex, "Failed to write log entry to Azure Log Analytics");
            }
        }

        /// <summary>
        /// Masks sensitive data in log entries to protect privacy
        /// </summary>
        /// <param name="logEntry">The log entry to mask</param>
        /// <returns>The log entry with sensitive data masked</returns>
        private Dictionary<string, object> MaskSensitiveData(Dictionary<string, object> logEntry)
        {
            if (!_options.EnableSensitiveDataMasking)
            {
                return logEntry;
            }

            var maskedLogEntry = new Dictionary<string, object>();

            foreach (var kvp in logEntry)
            {
                if (kvp.Value is string stringValue)
                {
                    // Mask email addresses
                    if (IsEmailField(kvp.Key))
                    {
                        maskedLogEntry[kvp.Key] = MaskEmail(stringValue);
                    }
                    // Mask passwords and secrets
                    else if (IsPasswordField(kvp.Key))
                    {
                        maskedLogEntry[kvp.Key] = "********";
                    }
                    // Mask potential PII in other fields
                    else
                    {
                        maskedLogEntry[kvp.Key] = MaskPII(stringValue);
                    }
                }
                else if (kvp.Value is Dictionary<string, object> dict)
                {
                    // Recursively mask nested dictionaries
                    maskedLogEntry[kvp.Key] = MaskSensitiveData(dict);
                }
                else
                {
                    // Copy non-string values unchanged
                    maskedLogEntry[kvp.Key] = kvp.Value;
                }
            }

            return maskedLogEntry;
        }

        /// <summary>
        /// Checks if a field name indicates it contains an email address
        /// </summary>
        private bool IsEmailField(string fieldName)
        {
            return fieldName.Contains("email", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Contains("mail", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if a field name indicates it contains a password or secret
        /// </summary>
        private bool IsPasswordField(string fieldName)
        {
            return fieldName.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Contains("key", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                   fieldName.Contains("credential", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Masks an email address to protect privacy
        /// </summary>
        private string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            {
                return email;
            }

            var parts = email.Split('@');
            if (parts.Length != 2)
            {
                return email;
            }

            string username = parts[0];
            string domain = parts[1];

            if (username.Length <= 2)
            {
                return $"{username}@{domain}";
            }

            return $"{username[0]}****{username[username.Length - 1]}@{domain}";
        }

        /// <summary>
        /// Masks potential personally identifiable information in a string
        /// </summary>
        private string MaskPII(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            // Mask credit card numbers
            text = Regex.Replace(text, 
                @"\b(?:\d[ -]*?){13,16}\b", 
                match => "****-****-****-" + match.Value.Replace(" ", "").Replace("-", "").Substring(12), 
                RegexOptions.IgnoreCase);

            // Mask social security numbers (US)
            text = Regex.Replace(text, 
                @"\b\d{3}[-_ ]?\d{2}[-_ ]?\d{4}\b", 
                "***-**-****", 
                RegexOptions.IgnoreCase);

            // Mask UK National Insurance numbers
            text = Regex.Replace(text, 
                @"\b[A-Za-z]{2}[ ]?\d{2}[ ]?\d{2}[ ]?\d{2}[ ]?[A-Za-z]\b", 
                "** ** ** ** *", 
                RegexOptions.IgnoreCase);

            // Mask phone numbers
            text = Regex.Replace(text, 
                @"\b(?:\+\d{1,3}[- ]?)?\(?\d{3}\)?[- ]?\d{3}[- ]?\d{4}\b", 
                "***-***-****", 
                RegexOptions.IgnoreCase);

            return text;
        }

        /// <summary>
        /// Generates the authorization signature for Azure Log Analytics API
        /// </summary>
        /// <param name="workspaceId">The Log Analytics workspace ID</param>
        /// <param name="sharedKey">The Log Analytics shared key</param>
        /// <param name="date">The request date</param>
        /// <param name="contentLength">The content length</param>
        /// <returns>The authorization signature</returns>
        private string GenerateLogAnalyticsSignature(string workspaceId, string sharedKey, string date, int contentLength)
        {
            string stringToSign = $"POST\n{contentLength}\napplication/json\nx-ms-date:{date}\n/api/logs";
            
            byte[] signatureBytes = Convert.FromBase64String(sharedKey);
            byte[] stringToSignBytes = Encoding.UTF8.GetBytes(stringToSign);
            
            using (var hmac = new HMACSHA256(signatureBytes))
            {
                byte[] hashBytes = hmac.ComputeHash(stringToSignBytes);
                string signature = Convert.ToBase64String(hashBytes);
                return $"SharedKey {workspaceId}:{signature}";
            }
        }
    }
}