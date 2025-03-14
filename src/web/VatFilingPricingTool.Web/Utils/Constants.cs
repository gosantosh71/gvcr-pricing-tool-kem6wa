using System; // System 6.0.0 - Core .NET functionality

namespace VatFilingPricingTool.Web.Utils
{
    /// <summary>
    /// Static class containing global constants used throughout the application
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Contains API endpoint constants for backend service communication
        /// </summary>
        public static class ApiEndpoints
        {
            /// <summary>
            /// Base URL for API calls
            /// </summary>
            public const string BaseUrl = "/api";

            /// <summary>
            /// Current API version
            /// </summary>
            public const string ApiVersion = "v1";
        }

        /// <summary>
        /// Authentication-related API endpoints
        /// </summary>
        public static class AuthEndpoints
        {
            /// <summary>
            /// Base endpoint for authentication operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/auth";

            /// <summary>
            /// Endpoint for user login
            /// </summary>
            public const string Login = Base + "/login";

            /// <summary>
            /// Endpoint for user registration
            /// </summary>
            public const string Register = Base + "/register";

            /// <summary>
            /// Endpoint for refreshing authentication tokens
            /// </summary>
            public const string RefreshToken = Base + "/refresh";

            /// <summary>
            /// Endpoint for user logout
            /// </summary>
            public const string Logout = Base + "/logout";

            /// <summary>
            /// Endpoint for forgot password functionality
            /// </summary>
            public const string ForgotPassword = Base + "/forgot-password";

            /// <summary>
            /// Endpoint for resetting password
            /// </summary>
            public const string ResetPassword = Base + "/reset-password";

            /// <summary>
            /// Endpoint for Azure AD login integration
            /// </summary>
            public const string AzureAdLogin = Base + "/azure-login";
        }

        /// <summary>
        /// User management API endpoints
        /// </summary>
        public static class UserEndpoints
        {
            /// <summary>
            /// Base endpoint for user operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/users";

            /// <summary>
            /// Endpoint for retrieving current user information
            /// </summary>
            public const string GetCurrentUser = Base + "/me";

            /// <summary>
            /// Endpoint for retrieving a specific user by ID
            /// </summary>
            public const string GetUserById = Base + "/{0}"; // Requires formatting with user ID

            /// <summary>
            /// Endpoint for retrieving all users
            /// </summary>
            public const string GetUsers = Base;

            /// <summary>
            /// Endpoint for updating user profile
            /// </summary>
            public const string UpdateProfile = Base + "/profile";

            /// <summary>
            /// Endpoint for updating user roles
            /// </summary>
            public const string UpdateRoles = Base + "/{0}/roles"; // Requires formatting with user ID

            /// <summary>
            /// Endpoint for updating user status
            /// </summary>
            public const string UpdateStatus = Base + "/{0}/status"; // Requires formatting with user ID
        }

        /// <summary>
        /// Country-related API endpoints
        /// </summary>
        public static class CountryEndpoints
        {
            /// <summary>
            /// Base endpoint for country operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/countries";

            /// <summary>
            /// Endpoint for retrieving all countries
            /// </summary>
            public const string GetAll = Base;

            /// <summary>
            /// Endpoint for retrieving a specific country by ID
            /// </summary>
            public const string GetById = Base + "/{0}"; // Requires formatting with country code

            /// <summary>
            /// Endpoint for retrieving active countries
            /// </summary>
            public const string GetActive = Base + "/active";

            /// <summary>
            /// Endpoint for retrieving countries by filing frequency
            /// </summary>
            public const string GetByFilingFrequency = Base + "/filing-frequency/{0}"; // Requires formatting with frequency

            /// <summary>
            /// Endpoint for retrieving country summaries
            /// </summary>
            public const string GetSummaries = Base + "/summaries";

            /// <summary>
            /// Endpoint for searching countries
            /// </summary>
            public const string Search = Base + "/search";
        }

        /// <summary>
        /// Pricing calculation API endpoints
        /// </summary>
        public static class PricingEndpoints
        {
            /// <summary>
            /// Base endpoint for pricing operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/pricing";

            /// <summary>
            /// Endpoint for calculating VAT filing prices
            /// </summary>
            public const string Calculate = Base + "/calculate";

            /// <summary>
            /// Endpoint for retrieving a specific calculation by ID
            /// </summary>
            public const string GetById = Base + "/{0}"; // Requires formatting with calculation ID

            /// <summary>
            /// Endpoint for saving a calculation
            /// </summary>
            public const string Save = Base;

            /// <summary>
            /// Endpoint for retrieving calculation history
            /// </summary>
            public const string GetHistory = Base + "/history";

            /// <summary>
            /// Endpoint for deleting a calculation
            /// </summary>
            public const string Delete = Base + "/{0}"; // Requires formatting with calculation ID

            /// <summary>
            /// Endpoint for retrieving available service types
            /// </summary>
            public const string GetServiceTypes = Base + "/service-types";

            /// <summary>
            /// Endpoint for retrieving available filing frequencies
            /// </summary>
            public const string GetFilingFrequencies = Base + "/filing-frequencies";

            /// <summary>
            /// Endpoint for retrieving available additional services
            /// </summary>
            public const string GetAdditionalServices = Base + "/additional-services";

            /// <summary>
            /// Endpoint for comparing multiple pricing scenarios
            /// </summary>
            public const string Compare = Base + "/compare";
        }

        /// <summary>
        /// Tax rule API endpoints
        /// </summary>
        public static class RuleEndpoints
        {
            /// <summary>
            /// Base endpoint for rule operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/rules";

            /// <summary>
            /// Endpoint for retrieving a specific rule by ID
            /// </summary>
            public const string GetById = Base + "/{0}"; // Requires formatting with rule ID

            /// <summary>
            /// Endpoint for retrieving all rules
            /// </summary>
            public const string GetAll = Base;

            /// <summary>
            /// Endpoint for retrieving rule summaries
            /// </summary>
            public const string GetSummaries = Base + "/summaries";

            /// <summary>
            /// Endpoint for creating a new rule
            /// </summary>
            public const string Create = Base;

            /// <summary>
            /// Endpoint for updating an existing rule
            /// </summary>
            public const string Update = Base + "/{0}"; // Requires formatting with rule ID

            /// <summary>
            /// Endpoint for deleting a rule
            /// </summary>
            public const string Delete = Base + "/{0}"; // Requires formatting with rule ID

            /// <summary>
            /// Endpoint for validating a rule expression
            /// </summary>
            public const string ValidateExpression = Base + "/validate-expression";

            /// <summary>
            /// Endpoint for importing rules
            /// </summary>
            public const string Import = Base + "/import";

            /// <summary>
            /// Endpoint for exporting rules
            /// </summary>
            public const string Export = Base + "/export";
        }

        /// <summary>
        /// Report generation API endpoints
        /// </summary>
        public static class ReportEndpoints
        {
            /// <summary>
            /// Base endpoint for report operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/reports";

            /// <summary>
            /// Endpoint for generating a report
            /// </summary>
            public const string Generate = Base + "/generate";

            /// <summary>
            /// Endpoint for retrieving a specific report by ID
            /// </summary>
            public const string GetById = Base + "/{0}"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for retrieving all reports
            /// </summary>
            public const string GetAll = Base;

            /// <summary>
            /// Endpoint for downloading a report
            /// </summary>
            public const string Download = Base + "/{0}/download"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for emailing a report
            /// </summary>
            public const string Email = Base + "/{0}/email"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for archiving a report
            /// </summary>
            public const string Archive = Base + "/{0}/archive"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for unarchiving a report
            /// </summary>
            public const string Unarchive = Base + "/{0}/unarchive"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for deleting a report
            /// </summary>
            public const string Delete = Base + "/{0}"; // Requires formatting with report ID

            /// <summary>
            /// Endpoint for retrieving available report formats
            /// </summary>
            public const string GetFormats = Base + "/formats";
        }

        /// <summary>
        /// External system integration API endpoints
        /// </summary>
        public static class IntegrationEndpoints
        {
            /// <summary>
            /// Base endpoint for integration operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/integration";

            /// <summary>
            /// Endpoint for ERP system integration
            /// </summary>
            public const string Erp = Base + "/erp";

            /// <summary>
            /// Endpoint for OCR document processing
            /// </summary>
            public const string Ocr = Base + "/ocr";

            /// <summary>
            /// Endpoint for importing data
            /// </summary>
            public const string Import = Base + "/import";

            /// <summary>
            /// Endpoint for exporting data
            /// </summary>
            public const string Export = Base + "/export";

            /// <summary>
            /// Endpoint for checking integration status
            /// </summary>
            public const string Status = Base + "/status";
        }

        /// <summary>
        /// Admin API endpoints
        /// </summary>
        public static class AdminEndpoints
        {
            /// <summary>
            /// Base endpoint for admin operations
            /// </summary>
            public const string Base = ApiEndpoints.BaseUrl + "/" + ApiEndpoints.ApiVersion + "/admin";

            /// <summary>
            /// Endpoint for admin user management
            /// </summary>
            public const string Users = Base + "/users";

            /// <summary>
            /// Endpoint for admin rule management
            /// </summary>
            public const string Rules = Base + "/rules";

            /// <summary>
            /// Endpoint for admin country management
            /// </summary>
            public const string Countries = Base + "/countries";

            /// <summary>
            /// Endpoint for admin audit log access
            /// </summary>
            public const string AuditLogs = Base + "/audit-logs";

            /// <summary>
            /// Endpoint for admin system settings
            /// </summary>
            public const string Settings = Base + "/settings";
        }

        /// <summary>
        /// Constants for local storage key names used in the application
        /// </summary>
        public static class LocalStorageKeys
        {
            /// <summary>
            /// Key for storing authentication token
            /// </summary>
            public const string AuthToken = "vat_pricing_auth_token";

            /// <summary>
            /// Key for storing refresh token
            /// </summary>
            public const string RefreshToken = "vat_pricing_refresh_token";

            /// <summary>
            /// Key for storing token expiration time
            /// </summary>
            public const string TokenExpires = "vat_pricing_token_expires";

            /// <summary>
            /// Key for storing user data
            /// </summary>
            public const string UserData = "vat_pricing_user_data";

            /// <summary>
            /// Prefix for calculation storage keys
            /// </summary>
            public const string CalculationPrefix = "vat_pricing_calc_";

            /// <summary>
            /// Prefix for user preferences storage keys
            /// </summary>
            public const string UserPreferencesPrefix = "vat_pricing_pref_";

            /// <summary>
            /// Key for storing theme preference
            /// </summary>
            public const string ThemePreference = UserPreferencesPrefix + "theme";

            /// <summary>
            /// Key for storing language preference
            /// </summary>
            public const string LanguagePreference = UserPreferencesPrefix + "language";

            /// <summary>
            /// Key for storing last visited page
            /// </summary>
            public const string LastVisitedPage = UserPreferencesPrefix + "last_page";
        }

        /// <summary>
        /// Constants for validation rules used throughout the application
        /// </summary>
        public static class ValidationConstants
        {
            /// <summary>
            /// Minimum password length
            /// </summary>
            public const int MinPasswordLength = 8;

            /// <summary>
            /// Maximum password length
            /// </summary>
            public const int MaxPasswordLength = 128;

            /// <summary>
            /// Minimum username length
            /// </summary>
            public const int MinUsernameLength = 3;

            /// <summary>
            /// Maximum username length
            /// </summary>
            public const int MaxUsernameLength = 50;

            /// <summary>
            /// Maximum email length
            /// </summary>
            public const int MaxEmailLength = 254;

            /// <summary>
            /// Minimum transaction volume
            /// </summary>
            public const int MinTransactionVolume = 1;

            /// <summary>
            /// Maximum transaction volume
            /// </summary>
            public const int MaxTransactionVolume = 100000;

            /// <summary>
            /// Regular expression pattern for validating email addresses
            /// </summary>
            public const string EmailRegexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            /// <summary>
            /// Regular expression pattern for validating passwords
            /// Complex password requiring at least one uppercase letter, one lowercase letter,
            /// one digit, and one special character
            /// </summary>
            public const string PasswordRegexPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$";
        }

        /// <summary>
        /// Constants for formatting values in the UI
        /// </summary>
        public static class FormatConstants
        {
            /// <summary>
            /// Format string for currency values
            /// </summary>
            public const string CurrencyFormat = "{0:C}";

            /// <summary>
            /// Format string for date values
            /// </summary>
            public const string DateFormat = "yyyy-MM-dd";

            /// <summary>
            /// Format string for time values
            /// </summary>
            public const string TimeFormat = "HH:mm:ss";

            /// <summary>
            /// Format string for date and time values
            /// </summary>
            public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            /// <summary>
            /// Format string for percentage values
            /// </summary>
            public const string PercentageFormat = "{0:P}";

            /// <summary>
            /// Format string for number values
            /// </summary>
            public const string NumberFormat = "{0:N}";
        }

        /// <summary>
        /// General application constants
        /// </summary>
        public static class ApplicationConstants
        {
            /// <summary>
            /// Name of the application
            /// </summary>
            public const string ApplicationName = "VAT Filing Pricing Tool";

            /// <summary>
            /// Current version of the application
            /// </summary>
            public const string ApplicationVersion = "1.0.0";

            /// <summary>
            /// Company name
            /// </summary>
            public const string CompanyName = "Financial Solutions Ltd.";

            /// <summary>
            /// Support email address
            /// </summary>
            public const string SupportEmail = "support@vatfilingpricingtool.com";

            /// <summary>
            /// Support phone number
            /// </summary>
            public const string SupportPhone = "+1-555-VAT-HELP";

            /// <summary>
            /// Copyright text
            /// </summary>
            public const string CopyrightText = "© " + DateTime.Now.Year.ToString() + " " + CompanyName + ". All rights reserved.";

            /// <summary>
            /// Default page size for paginated results
            /// </summary>
            public const int DefaultPageSize = 10;

            /// <summary>
            /// Maximum page size for paginated results
            /// </summary>
            public const int MaxPageSize = 100;
        }

        /// <summary>
        /// Error message constants used throughout the application
        /// </summary>
        public static class ErrorMessages
        {
            /// <summary>
            /// Error message for invalid credentials
            /// </summary>
            public const string InvalidCredentials = "The email or password you entered is incorrect. Please try again.";

            /// <summary>
            /// Error message for invalid email format
            /// </summary>
            public const string InvalidEmail = "Please enter a valid email address.";

            /// <summary>
            /// Error message for invalid password format
            /// </summary>
            public const string InvalidPassword = "Password must be at least 8 characters and include uppercase, lowercase, number, and special character.";

            /// <summary>
            /// Error message for required fields
            /// </summary>
            public const string RequiredField = "This field is required.";

            /// <summary>
            /// Error message for server errors
            /// </summary>
            public const string ServerError = "An unexpected error occurred. Please try again later or contact support.";

            /// <summary>
            /// Error message for connection errors
            /// </summary>
            public const string ConnectionError = "Unable to connect to the server. Please check your internet connection and try again.";

            /// <summary>
            /// Error message for validation errors
            /// </summary>
            public const string ValidationError = "Please correct the errors in the form before submitting.";

            /// <summary>
            /// Error message for unauthorized access
            /// </summary>
            public const string UnauthorizedAccess = "You do not have permission to access this resource.";

            /// <summary>
            /// Error message for expired sessions
            /// </summary>
            public const string SessionExpired = "Your session has expired. Please log in again.";
        }
    }
}