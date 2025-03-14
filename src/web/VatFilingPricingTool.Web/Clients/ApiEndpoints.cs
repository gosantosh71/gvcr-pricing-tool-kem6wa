using System; // System package version: 6.0.0

namespace VatFilingPricingTool.Web.Clients
{
    /// <summary>
    /// Static class containing all API endpoint constants organized by controller.
    /// Provides a centralized location for all API endpoint URLs used by the client
    /// to communicate with the backend services, ensuring consistency across the application.
    /// </summary>
    public static class ApiEndpoints
    {
        /// <summary>
        /// The base URL for all API requests.
        /// </summary>
        public static readonly string BaseUrl = "api";

        /// <summary>
        /// The API version string to be included in the URL.
        /// </summary>
        public static readonly string ApiVersion = "v1";

        /// <summary>
        /// Constants for authentication-related API endpoints.
        /// </summary>
        public static class Auth
        {
            /// <summary>
            /// Base route for authentication endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/auth";

            /// <summary>
            /// Endpoint for user login.
            /// </summary>
            public static readonly string Login = $"{Base}/login";

            /// <summary>
            /// Endpoint for user registration.
            /// </summary>
            public static readonly string Register = $"{Base}/register";

            /// <summary>
            /// Endpoint for initiating the forgot password process.
            /// </summary>
            public static readonly string ForgotPassword = $"{Base}/forgot-password";

            /// <summary>
            /// Endpoint for resetting user password.
            /// </summary>
            public static readonly string ResetPassword = $"{Base}/reset-password";

            /// <summary>
            /// Endpoint for refreshing the authentication token.
            /// </summary>
            public static readonly string RefreshToken = $"{Base}/refresh-token";

            /// <summary>
            /// Endpoint for user logout.
            /// </summary>
            public static readonly string Logout = $"{Base}/logout";

            /// <summary>
            /// Endpoint for Azure AD authentication.
            /// </summary>
            public static readonly string AzureAd = $"{Base}/azure-ad";

            /// <summary>
            /// Endpoint for validating an authentication token.
            /// </summary>
            public static readonly string ValidateToken = $"{Base}/validate-token";

            /// <summary>
            /// Endpoint for retrieving the current authenticated user.
            /// </summary>
            public static readonly string CurrentUser = $"{Base}/current-user";
        }

        /// <summary>
        /// Constants for user management API endpoints.
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Base route for user management endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/users";

            /// <summary>
            /// Endpoint for retrieving all users.
            /// </summary>
            public static readonly string Get = Base;

            /// <summary>
            /// Endpoint for retrieving a specific user by ID.
            /// </summary>
            public static readonly string GetById = $"{Base}/{{id}}";

            /// <summary>
            /// Endpoint for retrieving user profile.
            /// </summary>
            public static readonly string Profile = $"{Base}/profile";

            /// <summary>
            /// Endpoint for updating user profile.
            /// </summary>
            public static readonly string UpdateProfile = $"{Base}/profile";

            /// <summary>
            /// Endpoint for changing user password.
            /// </summary>
            public static readonly string ChangePassword = $"{Base}/change-password";
        }

        /// <summary>
        /// Constants for pricing calculation API endpoints.
        /// </summary>
        public static class Pricing
        {
            /// <summary>
            /// Base route for pricing calculation endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/pricing";

            /// <summary>
            /// Endpoint for calculating VAT filing pricing.
            /// </summary>
            public static readonly string Calculate = $"{Base}/calculate";

            /// <summary>
            /// Endpoint for retrieving a specific calculation by ID.
            /// </summary>
            public static readonly string GetById = $"{Base}/{{id}}";

            /// <summary>
            /// Endpoint for saving a calculation.
            /// </summary>
            public static readonly string Save = Base;

            /// <summary>
            /// Endpoint for retrieving calculation history.
            /// </summary>
            public static readonly string History = $"{Base}/history";

            /// <summary>
            /// Endpoint for comparing multiple calculations.
            /// </summary>
            public static readonly string Compare = $"{Base}/compare";

            /// <summary>
            /// Endpoint for deleting a calculation.
            /// </summary>
            public static readonly string Delete = $"{Base}/{{id}}";

            /// <summary>
            /// Endpoint for retrieving available service types.
            /// </summary>
            public static readonly string ServiceTypes = $"{Base}/service-types";

            /// <summary>
            /// Endpoint for retrieving available filing frequencies.
            /// </summary>
            public static readonly string FilingFrequencies = $"{Base}/filing-frequencies";

            /// <summary>
            /// Endpoint for retrieving available additional services.
            /// </summary>
            public static readonly string AdditionalServices = $"{Base}/additional-services";
        }

        /// <summary>
        /// Constants for country management API endpoints.
        /// </summary>
        public static class Country
        {
            /// <summary>
            /// Base route for country management endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/countries";

            /// <summary>
            /// Endpoint for retrieving all countries.
            /// </summary>
            public static readonly string Get = Base;

            /// <summary>
            /// Endpoint for retrieving a specific country by ID.
            /// </summary>
            public static readonly string GetById = $"{Base}/{{id}}";

            /// <summary>
            /// Endpoint for retrieving countries by filing frequency.
            /// </summary>
            public static readonly string GetByFrequency = $"{Base}/by-frequency/{{frequency}}";
        }

        /// <summary>
        /// Constants for report generation and management API endpoints.
        /// </summary>
        public static class Report
        {
            /// <summary>
            /// Base route for report generation and management endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/reports";

            /// <summary>
            /// Endpoint for generating a report.
            /// </summary>
            public static readonly string Generate = $"{Base}/generate";

            /// <summary>
            /// Endpoint for retrieving a specific report by ID.
            /// </summary>
            public static readonly string GetById = $"{Base}/{{id}}";

            /// <summary>
            /// Endpoint for retrieving all reports.
            /// </summary>
            public static readonly string GetAll = Base;

            /// <summary>
            /// Endpoint for downloading a report.
            /// </summary>
            public static readonly string Download = $"{Base}/{{id}}/download";

            /// <summary>
            /// Endpoint for emailing a report.
            /// </summary>
            public static readonly string Email = $"{Base}/{{id}}/email";
        }

        /// <summary>
        /// Constants for administrative API endpoints.
        /// </summary>
        public static class Admin
        {
            /// <summary>
            /// Base route for administrative endpoints.
            /// </summary>
            public static readonly string Base = $"{BaseUrl}/{ApiVersion}/admin";

            /// <summary>
            /// Endpoint for user administration.
            /// </summary>
            public static readonly string Users = $"{Base}/users";

            /// <summary>
            /// Endpoint for pricing rule administration.
            /// </summary>
            public static readonly string Rules = $"{Base}/rules";

            /// <summary>
            /// Endpoint for country configuration administration.
            /// </summary>
            public static readonly string Countries = $"{Base}/countries";

            /// <summary>
            /// Endpoint for system settings administration.
            /// </summary>
            public static readonly string Settings = $"{Base}/settings";

            /// <summary>
            /// Endpoint for audit log administration.
            /// </summary>
            public static readonly string AuditLogs = $"{Base}/audit-logs";
        }
    }
}