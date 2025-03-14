using System;

namespace VatFilingPricingTool.Common.Constants
{
    /// <summary>
    /// Defines standardized cache key constants used throughout the VAT Filing Pricing Tool application.
    /// This class centralizes cache key definitions to ensure consistent caching patterns and prevent key collisions.
    /// </summary>
    public static class CacheKeys
    {
        /// <summary>
        /// Base prefixes for different cache key categories
        /// </summary>
        public static class Prefix
        {
            /// <summary>
            /// Base prefix for all cache keys
            /// </summary>
            public const string Base = "VAT:";
            
            /// <summary>
            /// Prefix for country-related cache keys
            /// </summary>
            public const string Country = Base + "Country:";
            
            /// <summary>
            /// Prefix for calculation-related cache keys
            /// </summary>
            public const string Calculation = Base + "Calculation:";
            
            /// <summary>
            /// Prefix for rule-related cache keys
            /// </summary>
            public const string Rule = Base + "Rule:";
            
            /// <summary>
            /// Prefix for service-related cache keys
            /// </summary>
            public const string Service = Base + "Service:";
            
            /// <summary>
            /// Prefix for user-related cache keys
            /// </summary>
            public const string User = Base + "User:";
            
            /// <summary>
            /// Prefix for report-related cache keys
            /// </summary>
            public const string Report = Base + "Report:";
        }

        /// <summary>
        /// Cache keys for country-related data
        /// </summary>
        public static class Country
        {
            /// <summary>
            /// Cache key for all countries
            /// </summary>
            public const string All = Prefix.Country + "All";
            
            /// <summary>
            /// Cache key for active countries
            /// </summary>
            public const string Active = Prefix.Country + "Active";
            
            /// <summary>
            /// Cache key format for country by code
            /// Format: {0} = country code
            /// </summary>
            public const string ByCode = Prefix.Country + "Code:{0}";
            
            /// <summary>
            /// Cache key format for countries by filing frequency
            /// Format: {0} = filing frequency
            /// </summary>
            public const string ByFilingFrequency = Prefix.Country + "FilingFrequency:{0}";
            
            /// <summary>
            /// Cache key for country summaries
            /// </summary>
            public const string Summaries = Prefix.Country + "Summaries";
        }

        /// <summary>
        /// Cache keys for calculation-related data
        /// </summary>
        public static class Calculation
        {
            /// <summary>
            /// Cache key format for calculation by ID
            /// Format: {0} = calculation ID
            /// </summary>
            public const string ById = Prefix.Calculation + "Id:{0}";
            
            /// <summary>
            /// Cache key format for calculations by user
            /// Format: {0} = user ID
            /// </summary>
            public const string ByUser = Prefix.Calculation + "User:{0}";
            
            /// <summary>
            /// Cache key format for calculation results
            /// Format: {0} = parameter hash
            /// </summary>
            public const string Result = Prefix.Calculation + "Result:{0}";
        }

        /// <summary>
        /// Cache keys for rule-related data
        /// </summary>
        public static class Rule
        {
            /// <summary>
            /// Cache key for all rules
            /// </summary>
            public const string All = Prefix.Rule + "All";
            
            /// <summary>
            /// Cache key format for rule by ID
            /// Format: {0} = rule ID
            /// </summary>
            public const string ById = Prefix.Rule + "Id:{0}";
            
            /// <summary>
            /// Cache key format for rules by country
            /// Format: {0} = country code
            /// </summary>
            public const string ByCountry = Prefix.Rule + "Country:{0}";
            
            /// <summary>
            /// Cache key format for rules by type
            /// Format: {0} = rule type
            /// </summary>
            public const string ByType = Prefix.Rule + "Type:{0}";
        }

        /// <summary>
        /// Cache keys for service-related data
        /// </summary>
        public static class Service
        {
            /// <summary>
            /// Cache key for all services
            /// </summary>
            public const string All = Prefix.Service + "All";
            
            /// <summary>
            /// Cache key format for service by ID
            /// Format: {0} = service ID
            /// </summary>
            public const string ById = Prefix.Service + "Id:{0}";
            
            /// <summary>
            /// Cache key format for services by type
            /// Format: {0} = service type
            /// </summary>
            public const string ByType = Prefix.Service + "Type:{0}";
            
            /// <summary>
            /// Cache key for additional services
            /// </summary>
            public const string AdditionalServices = Prefix.Service + "Additional";
        }

        /// <summary>
        /// Cache keys for user-related data
        /// </summary>
        public static class User
        {
            /// <summary>
            /// Cache key format for user profile
            /// Format: {0} = user ID
            /// </summary>
            public const string Profile = Prefix.User + "Profile:{0}";
            
            /// <summary>
            /// Cache key format for user preferences
            /// Format: {0} = user ID
            /// </summary>
            public const string Preferences = Prefix.User + "Preferences:{0}";
            
            /// <summary>
            /// Cache key format for user roles
            /// Format: {0} = user ID
            /// </summary>
            public const string Roles = Prefix.User + "Roles:{0}";
        }

        /// <summary>
        /// Cache keys for report-related data
        /// </summary>
        public static class Report
        {
            /// <summary>
            /// Cache key format for report by ID
            /// Format: {0} = report ID
            /// </summary>
            public const string ById = Prefix.Report + "Id:{0}";
            
            /// <summary>
            /// Cache key format for reports by user
            /// Format: {0} = user ID
            /// </summary>
            public const string ByUser = Prefix.Report + "User:{0}";
            
            /// <summary>
            /// Cache key for report templates
            /// </summary>
            public const string Templates = Prefix.Report + "Templates";
        }

        /// <summary>
        /// Helper methods for working with cache keys
        /// </summary>
        public static class Utility
        {
            /// <summary>
            /// Formats a cache key with parameters
            /// </summary>
            /// <param name="key">The cache key format</param>
            /// <param name="parameters">Parameters to format the key with</param>
            /// <returns>Formatted cache key</returns>
            public static string FormatKey(string key, params object[] parameters)
            {
                if (parameters == null || parameters.Length == 0)
                {
                    return key;
                }

                return string.Format(key, parameters);
            }

            /// <summary>
            /// Creates a user-specific cache key
            /// </summary>
            /// <param name="key">The base cache key</param>
            /// <param name="userId">The user ID</param>
            /// <returns>User-specific cache key</returns>
            public static string GetUserSpecificKey(string key, string userId)
            {
                if (string.IsNullOrEmpty(key))
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentNullException(nameof(userId));
                }

                return $"{key}:User:{userId}";
            }
        }
    }
}