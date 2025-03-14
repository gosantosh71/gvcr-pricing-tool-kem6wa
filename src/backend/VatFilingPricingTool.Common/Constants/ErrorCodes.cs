namespace VatFilingPricingTool.Common.Constants
{
    /// <summary>
    /// Defines standardized error codes used throughout the VAT Filing Pricing Tool application
    /// to ensure consistent error handling and reporting across all components.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// General error codes applicable across the application
        /// </summary>
        public static class General
        {
            /// <summary>Internal server error</summary>
            public const string ServerError = "GENERAL-001";
            
            /// <summary>Unknown or unspecified error</summary>
            public const string UnknownError = "GENERAL-002";
            
            /// <summary>Validation error</summary>
            public const string ValidationError = "GENERAL-003";
            
            /// <summary>Resource not found</summary>
            public const string NotFound = "GENERAL-004";
            
            /// <summary>User is not authenticated</summary>
            public const string Unauthorized = "GENERAL-005";
            
            /// <summary>User is authenticated but not authorized for this action</summary>
            public const string Forbidden = "GENERAL-006";
            
            /// <summary>Bad request - invalid parameters or payload</summary>
            public const string BadRequest = "GENERAL-007";
            
            /// <summary>Resource conflict</summary>
            public const string Conflict = "GENERAL-008";
            
            /// <summary>Service is currently unavailable</summary>
            public const string ServiceUnavailable = "GENERAL-009";
            
            /// <summary>Operation timed out</summary>
            public const string Timeout = "GENERAL-010";
        }

        /// <summary>
        /// Error codes related to authentication and authorization
        /// </summary>
        public static class Authentication
        {
            /// <summary>Invalid credentials provided</summary>
            public const string InvalidCredentials = "AUTH-001";
            
            /// <summary>Account has been locked</summary>
            public const string AccountLocked = "AUTH-002";
            
            /// <summary>Account has been disabled</summary>
            public const string AccountDisabled = "AUTH-003";
            
            /// <summary>Authentication token has expired</summary>
            public const string TokenExpired = "AUTH-004";
            
            /// <summary>Invalid authentication token</summary>
            public const string InvalidToken = "AUTH-005";
            
            /// <summary>Authentication token is missing</summary>
            public const string MissingToken = "AUTH-006";
            
            /// <summary>User has insufficient permissions for this action</summary>
            public const string InsufficientPermissions = "AUTH-007";
            
            /// <summary>Password reset is required</summary>
            public const string PasswordResetRequired = "AUTH-008";
            
            /// <summary>Multi-factor authentication is required</summary>
            public const string MfaRequired = "AUTH-009";
            
            /// <summary>User registration failed</summary>
            public const string RegistrationFailed = "AUTH-010";
        }

        /// <summary>
        /// Error codes related to pricing calculations
        /// </summary>
        public static class Pricing
        {
            /// <summary>Pricing calculation failed</summary>
            public const string CalculationFailed = "PRICING-001";
            
            /// <summary>Invalid calculation parameters</summary>
            public const string InvalidParameters = "PRICING-002";
            
            /// <summary>The specified country is not supported</summary>
            public const string CountryNotSupported = "PRICING-003";
            
            /// <summary>The specified service type is not supported</summary>
            public const string ServiceTypeNotSupported = "PRICING-004";
            
            /// <summary>Error evaluating pricing rules</summary>
            public const string RuleEvaluationFailed = "PRICING-005";
            
            /// <summary>Error evaluating pricing expression</summary>
            public const string ExpressionEvaluationFailed = "PRICING-006";
            
            /// <summary>Invalid transaction volume specified</summary>
            public const string InvalidTransactionVolume = "PRICING-007";
            
            /// <summary>Invalid filing frequency specified</summary>
            public const string InvalidFilingFrequency = "PRICING-008";
            
            /// <summary>Calculation not found</summary>
            public const string CalculationNotFound = "PRICING-009";
            
            /// <summary>Failed to save calculation</summary>
            public const string SaveCalculationFailed = "PRICING-010";
        }

        /// <summary>
        /// Error codes related to country operations
        /// </summary>
        public static class Country
        {
            /// <summary>Country not found</summary>
            public const string CountryNotFound = "COUNTRY-001";
            
            /// <summary>Invalid country code</summary>
            public const string InvalidCountryCode = "COUNTRY-002";
            
            /// <summary>Duplicate country code</summary>
            public const string DuplicateCountryCode = "COUNTRY-003";
            
            /// <summary>Invalid VAT rate</summary>
            public const string InvalidVatRate = "COUNTRY-004";
            
            /// <summary>Country creation failed</summary>
            public const string CountryCreationFailed = "COUNTRY-005";
            
            /// <summary>Country update failed</summary>
            public const string CountryUpdateFailed = "COUNTRY-006";
            
            /// <summary>Country deletion failed</summary>
            public const string CountryDeletionFailed = "COUNTRY-007";
            
            /// <summary>Country is in use and cannot be modified/deleted</summary>
            public const string CountryInUse = "COUNTRY-008";
        }

        /// <summary>
        /// Error codes related to VAT rule operations
        /// </summary>
        public static class Rule
        {
            /// <summary>Rule not found</summary>
            public const string RuleNotFound = "RULE-001";
            
            /// <summary>Invalid rule type</summary>
            public const string InvalidRuleType = "RULE-002";
            
            /// <summary>Invalid rule expression</summary>
            public const string InvalidRuleExpression = "RULE-003";
            
            /// <summary>Duplicate rule ID</summary>
            public const string DuplicateRuleId = "RULE-004";
            
            /// <summary>Rule creation failed</summary>
            public const string RuleCreationFailed = "RULE-005";
            
            /// <summary>Rule update failed</summary>
            public const string RuleUpdateFailed = "RULE-006";
            
            /// <summary>Rule deletion failed</summary>
            public const string RuleDeletionFailed = "RULE-007";
            
            /// <summary>Rule validation failed</summary>
            public const string RuleValidationFailed = "RULE-008";
            
            /// <summary>Rule import failed</summary>
            public const string RuleImportFailed = "RULE-009";
            
            /// <summary>Rule export failed</summary>
            public const string RuleExportFailed = "RULE-010";
            
            /// <summary>Rule is in use and cannot be modified/deleted</summary>
            public const string RuleInUse = "RULE-011";
        }

        /// <summary>
        /// Error codes related to report generation and management
        /// </summary>
        public static class Report
        {
            /// <summary>Report not found</summary>
            public const string ReportNotFound = "REPORT-001";
            
            /// <summary>Report generation failed</summary>
            public const string ReportGenerationFailed = "REPORT-002";
            
            /// <summary>Invalid report format</summary>
            public const string InvalidReportFormat = "REPORT-003";
            
            /// <summary>Report download failed</summary>
            public const string ReportDownloadFailed = "REPORT-004";
            
            /// <summary>Report email delivery failed</summary>
            public const string ReportEmailFailed = "REPORT-005";
            
            /// <summary>Report archiving failed</summary>
            public const string ReportArchiveFailed = "REPORT-006";
            
            /// <summary>Report unarchiving failed</summary>
            public const string ReportUnarchiveFailed = "REPORT-007";
            
            /// <summary>Report deletion failed</summary>
            public const string ReportDeletionFailed = "REPORT-008";
            
            /// <summary>Invalid report parameters</summary>
            public const string InvalidReportParameters = "REPORT-009";
        }

        /// <summary>
        /// Error codes related to external system integrations
        /// </summary>
        public static class Integration
        {
            /// <summary>Integration not found</summary>
            public const string IntegrationNotFound = "INTEGRATION-001";
            
            /// <summary>Connection to external system failed</summary>
            public const string ConnectionFailed = "INTEGRATION-002";
            
            /// <summary>Authentication with external system failed</summary>
            public const string AuthenticationFailed = "INTEGRATION-003";
            
            /// <summary>Data import from external system failed</summary>
            public const string DataImportFailed = "INTEGRATION-004";
            
            /// <summary>Invalid system type specified</summary>
            public const string InvalidSystemType = "INTEGRATION-005";
            
            /// <summary>Document processing failed</summary>
            public const string DocumentProcessingFailed = "INTEGRATION-006";
            
            /// <summary>OCR data extraction failed</summary>
            public const string OcrExtractionFailed = "INTEGRATION-007";
            
            /// <summary>Invalid document format</summary>
            public const string InvalidDocumentFormat = "INTEGRATION-008";
            
            /// <summary>Integration creation failed</summary>
            public const string IntegrationCreationFailed = "INTEGRATION-009";
            
            /// <summary>Integration update failed</summary>
            public const string IntegrationUpdateFailed = "INTEGRATION-010";
            
            /// <summary>Integration deletion failed</summary>
            public const string IntegrationDeletionFailed = "INTEGRATION-011";
        }

        /// <summary>
        /// Error codes related to user management
        /// </summary>
        public static class User
        {
            /// <summary>User not found</summary>
            public const string UserNotFound = "USER-001";
            
            /// <summary>Email is already in use</summary>
            public const string DuplicateEmail = "USER-002";
            
            /// <summary>Invalid email format</summary>
            public const string InvalidEmail = "USER-003";
            
            /// <summary>Password does not meet requirements</summary>
            public const string InvalidPassword = "USER-004";
            
            /// <summary>User creation failed</summary>
            public const string UserCreationFailed = "USER-005";
            
            /// <summary>User update failed</summary>
            public const string UserUpdateFailed = "USER-006";
            
            /// <summary>User deletion failed</summary>
            public const string UserDeletionFailed = "USER-007";
            
            /// <summary>Invalid role specified</summary>
            public const string InvalidRole = "USER-008";
            
            /// <summary>Profile update failed</summary>
            public const string ProfileUpdateFailed = "USER-009";
        }

        /// <summary>
        /// Error codes related to data operations
        /// </summary>
        public static class Data
        {
            /// <summary>Database connection failed</summary>
            public const string DatabaseConnectionFailed = "DATA-001";
            
            /// <summary>Query execution failed</summary>
            public const string QueryExecutionFailed = "DATA-002";
            
            /// <summary>Data validation failed</summary>
            public const string DataValidationFailed = "DATA-003";
            
            /// <summary>Duplicate key violation</summary>
            public const string DuplicateKey = "DATA-004";
            
            /// <summary>Foreign key constraint violation</summary>
            public const string ForeignKeyViolation = "DATA-005";
            
            /// <summary>Database transaction failed</summary>
            public const string TransactionFailed = "DATA-006";
            
            /// <summary>Concurrency conflict</summary>
            public const string ConcurrencyConflict = "DATA-007";
            
            /// <summary>Data migration failed</summary>
            public const string DataMigrationFailed = "DATA-008";
        }
    }
}