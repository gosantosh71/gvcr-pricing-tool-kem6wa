namespace VatFilingPricingTool.Common.Constants
{
    /// <summary>
    /// Static class containing all API route constants organized by controller
    /// to ensure consistency in route naming and structure across the application.
    /// </summary>
    public static class ApiRoutes
    {
        /// <summary>
        /// Base API route prefix used for all API endpoints
        /// </summary>
        public const string BaseApiRoute = "api";

        /// <summary>
        /// API version prefix
        /// </summary>
        public const string ApiVersion = "v1";

        /// <summary>
        /// Root API path with version
        /// </summary>
        public const string Root = BaseApiRoute + "/" + ApiVersion;

        /// <summary>
        /// Constants for authentication-related API routes
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Base route for authentication controller
            /// </summary>
            public const string Base = Root + "/auth";

            /// <summary>
            /// Route for user login
            /// </summary>
            public const string Login = Base + "/login";

            /// <summary>
            /// Route for user registration
            /// </summary>
            public const string Register = Base + "/register";

            /// <summary>
            /// Route for initiating password reset
            /// </summary>
            public const string ForgotPassword = Base + "/forgot-password";

            /// <summary>
            /// Route for resetting password with token
            /// </summary>
            public const string ResetPassword = Base + "/reset-password";

            /// <summary>
            /// Route for refreshing authentication token
            /// </summary>
            public const string RefreshToken = Base + "/refresh-token";

            /// <summary>
            /// Route for logging out and invalidating tokens
            /// </summary>
            public const string Logout = Base + "/logout";

            /// <summary>
            /// Route for Azure AD authentication
            /// </summary>
            public const string AzureAd = Base + "/azure-ad";
        }

        /// <summary>
        /// Constants for user management API routes
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Base route for user controller
            /// </summary>
            public const string Base = Root + "/users";

            /// <summary>
            /// Route for getting all users
            /// </summary>
            public const string Get = Base;

            /// <summary>
            /// Route for getting a specific user by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for accessing current user profile
            /// </summary>
            public const string Profile = Base + "/profile";

            /// <summary>
            /// Route for managing user roles
            /// </summary>
            public const string Roles = Base + "/roles";

            /// <summary>
            /// Route for managing user status (active/inactive)
            /// </summary>
            public const string Status = Base + "/status";

            /// <summary>
            /// Route for retrieving user summaries/statistics
            /// </summary>
            public const string Summaries = Base + "/summaries";
        }

        /// <summary>
        /// Constants for pricing calculation API routes
        /// </summary>
        public static class Pricing
        {
            /// <summary>
            /// Base route for pricing controller
            /// </summary>
            public const string Base = Root + "/pricing";

            /// <summary>
            /// Route for calculating VAT filing prices
            /// </summary>
            public const string Calculate = Base + "/calculate";

            /// <summary>
            /// Route for retrieving a specific calculation by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for saving a calculation
            /// </summary>
            public const string Save = Base + "/save";

            /// <summary>
            /// Route for retrieving calculation history
            /// </summary>
            public const string History = Base + "/history";

            /// <summary>
            /// Route for comparing multiple calculations
            /// </summary>
            public const string Compare = Base + "/compare";

            /// <summary>
            /// Route for deleting a calculation
            /// </summary>
            public const string Delete = Base + "/{id}";
        }

        /// <summary>
        /// Constants for country management API routes
        /// </summary>
        public static class Country
        {
            /// <summary>
            /// Base route for country controller
            /// </summary>
            public const string Base = Root + "/countries";

            /// <summary>
            /// Route for getting all countries
            /// </summary>
            public const string Get = Base;

            /// <summary>
            /// Route for getting a specific country by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for creating a new country
            /// </summary>
            public const string Create = Base;

            /// <summary>
            /// Route for updating a country
            /// </summary>
            public const string Update = Base + "/{id}";

            /// <summary>
            /// Route for deleting a country
            /// </summary>
            public const string Delete = Base + "/{id}";

            /// <summary>
            /// Route for filtering countries by filing frequency
            /// </summary>
            public const string GetByFrequency = Base + "/frequency/{frequency}";

            /// <summary>
            /// Route for retrieving country summaries/statistics
            /// </summary>
            public const string Summaries = Base + "/summaries";
        }

        /// <summary>
        /// Constants for VAT rule management API routes
        /// </summary>
        public static class Rule
        {
            /// <summary>
            /// Base route for rule controller
            /// </summary>
            public const string Base = Root + "/rules";

            /// <summary>
            /// Route for getting all rules
            /// </summary>
            public const string Get = Base;

            /// <summary>
            /// Route for getting a specific rule by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for getting rules by country
            /// </summary>
            public const string GetByCountry = Base + "/country/{countryCode}";

            /// <summary>
            /// Route for creating a new rule
            /// </summary>
            public const string Create = Base;

            /// <summary>
            /// Route for updating a rule
            /// </summary>
            public const string Update = Base + "/{id}";

            /// <summary>
            /// Route for deleting a rule
            /// </summary>
            public const string Delete = Base + "/{id}";

            /// <summary>
            /// Route for validating rule consistency
            /// </summary>
            public const string Validate = Base + "/validate";

            /// <summary>
            /// Route for importing rules in bulk
            /// </summary>
            public const string Import = Base + "/import";

            /// <summary>
            /// Route for exporting rules
            /// </summary>
            public const string Export = Base + "/export";
        }

        /// <summary>
        /// Constants for report generation and management API routes
        /// </summary>
        public static class Report
        {
            /// <summary>
            /// Base route for report controller
            /// </summary>
            public const string Base = Root + "/reports";

            /// <summary>
            /// Route for generating a new report
            /// </summary>
            public const string Generate = Base + "/generate";

            /// <summary>
            /// Route for getting a specific report by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for getting all reports
            /// </summary>
            public const string GetAll = Base;

            /// <summary>
            /// Route for downloading a report
            /// </summary>
            public const string Download = Base + "/{id}/download";

            /// <summary>
            /// Route for emailing a report
            /// </summary>
            public const string Email = Base + "/{id}/email";

            /// <summary>
            /// Route for archiving a report
            /// </summary>
            public const string Archive = Base + "/{id}/archive";

            /// <summary>
            /// Route for unarchiving a report
            /// </summary>
            public const string Unarchive = Base + "/{id}/unarchive";

            /// <summary>
            /// Route for deleting a report
            /// </summary>
            public const string Delete = Base + "/{id}";
        }

        /// <summary>
        /// Constants for external system integration API routes
        /// </summary>
        public static class Integration
        {
            /// <summary>
            /// Base route for integration controller
            /// </summary>
            public const string Base = Root + "/integrations";

            /// <summary>
            /// Route for getting all integrations
            /// </summary>
            public const string Get = Base;

            /// <summary>
            /// Route for getting a specific integration by ID
            /// </summary>
            public const string GetById = Base + "/{id}";

            /// <summary>
            /// Route for creating a new integration
            /// </summary>
            public const string Create = Base;

            /// <summary>
            /// Route for updating an integration
            /// </summary>
            public const string Update = Base + "/{id}";

            /// <summary>
            /// Route for deleting an integration
            /// </summary>
            public const string Delete = Base + "/{id}";

            /// <summary>
            /// Route for testing an integration connection
            /// </summary>
            public const string TestConnection = Base + "/{id}/test-connection";

            /// <summary>
            /// Route for importing data from an external system
            /// </summary>
            public const string Import = Base + "/{id}/import";

            /// <summary>
            /// Route for processing documents via OCR
            /// </summary>
            public const string ProcessDocument = Base + "/process-document";

            /// <summary>
            /// Route for retrieving available system types for integration
            /// </summary>
            public const string SystemTypes = Base + "/system-types";
        }

        /// <summary>
        /// Constants for administrative API routes
        /// </summary>
        public static class Admin
        {
            /// <summary>
            /// Base route for admin controller
            /// </summary>
            public const string Base = Root + "/admin";

            /// <summary>
            /// Route for administrative user management
            /// </summary>
            public const string Users = Base + "/users";

            /// <summary>
            /// Route for administrative rule management
            /// </summary>
            public const string Rules = Base + "/rules";

            /// <summary>
            /// Route for administrative country management
            /// </summary>
            public const string Countries = Base + "/countries";

            /// <summary>
            /// Route for system settings management
            /// </summary>
            public const string Settings = Base + "/settings";

            /// <summary>
            /// Route for accessing audit logs
            /// </summary>
            public const string AuditLogs = Base + "/audit-logs";
        }
    }
}