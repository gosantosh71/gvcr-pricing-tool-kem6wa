using System; // Version 6.0.0 - Core .NET functionality
using System.Collections.Generic;

namespace VatFilingPricingTool.Domain.Constants
{
    /// <summary>
    /// Static class containing all domain-specific constants organized by category
    /// </summary>
    public static class DomainConstants
    {
        /// <summary>
        /// Default values used throughout the domain
        /// </summary>
        public static class Defaults
        {
            /// <summary>
            /// Default currency code (Euro)
            /// </summary>
            public const string DefaultCurrency = "EUR";

            /// <summary>
            /// Default country code (United Kingdom)
            /// </summary>
            public const string DefaultCountryCode = "GB";

            /// <summary>
            /// Default transaction volume for new calculations
            /// </summary>
            public const int DefaultTransactionVolume = 100;

            /// <summary>
            /// Default service type for VAT filing
            /// </summary>
            public const string DefaultServiceType = "StandardFiling";

            /// <summary>
            /// Default filing frequency
            /// </summary>
            public const string DefaultFilingFrequency = "Monthly";
        }

        /// <summary>
        /// Validation constants for domain entities and value objects
        /// </summary>
        public static class Validation
        {
            /// <summary>
            /// Minimum allowed transaction volume
            /// </summary>
            public const int MinTransactionVolume = 1;

            /// <summary>
            /// Maximum allowed transaction volume
            /// </summary>
            public const int MaxTransactionVolume = 100000;

            /// <summary>
            /// Minimum VAT rate percentage (0%)
            /// </summary>
            public const decimal MinVatRate = 0.0m;

            /// <summary>
            /// Maximum VAT rate percentage (100%)
            /// </summary>
            public const decimal MaxVatRate = 100.0m;

            /// <summary>
            /// Length of ISO country codes (ISO 3166-1 alpha-2)
            /// </summary>
            public const int CountryCodeLength = 2;

            /// <summary>
            /// Length of ISO currency codes (ISO 4217)
            /// </summary>
            public const int CurrencyCodeLength = 3;

            /// <summary>
            /// Regex pattern for valid currency codes
            /// </summary>
            public const string CurrencyCodePattern = "^[A-Z]{3}$";

            /// <summary>
            /// Minimum length for name fields
            /// </summary>
            public const int MinNameLength = 2;

            /// <summary>
            /// Maximum length for name fields
            /// </summary>
            public const int MaxNameLength = 100;

            /// <summary>
            /// Minimum length for description fields
            /// </summary>
            public const int MinDescriptionLength = 0;

            /// <summary>
            /// Maximum length for description fields
            /// </summary>
            public const int MaxDescriptionLength = 500;

            /// <summary>
            /// Maximum length for rule expressions
            /// </summary>
            public const int MaxRuleExpressionLength = 2000;
        }

        /// <summary>
        /// Constants related to pricing calculations
        /// </summary>
        public static class Calculation
        {
            /// <summary>
            /// Minimum allowed discount percentage
            /// </summary>
            public const decimal MinimumDiscount = 0.0m;

            /// <summary>
            /// Maximum allowed discount percentage
            /// </summary>
            public const decimal MaximumDiscount = 50.0m;

            /// <summary>
            /// Transaction volume threshold for volume discount
            /// </summary>
            public const decimal VolumeDiscountThreshold = 500;

            /// <summary>
            /// Percentage discount for high volume transactions
            /// </summary>
            public const decimal VolumeDiscountPercentage = 5.0m;

            /// <summary>
            /// Country count threshold for multi-country discount
            /// </summary>
            public const decimal MultiCountryDiscountThreshold = 3;

            /// <summary>
            /// Percentage discount for multi-country filing
            /// </summary>
            public const decimal MultiCountryDiscountPercentage = 7.5m;

            /// <summary>
            /// Display name for volume discount
            /// </summary>
            public const string VolumeDiscountName = "High Volume Discount";

            /// <summary>
            /// Display name for multi-country discount
            /// </summary>
            public const string MultiCountryDiscountName = "Multi-Country Discount";
        }

        /// <summary>
        /// Constants related to the rule engine and rule evaluation
        /// </summary>
        public static class Rules
        {
            /// <summary>
            /// Default priority for rules
            /// </summary>
            public const int DefaultRulePriority = 100;

            /// <summary>
            /// Minimum allowed rule priority (higher number = lower priority)
            /// </summary>
            public const int MinRulePriority = 1;

            /// <summary>
            /// Maximum allowed rule priority
            /// </summary>
            public const int MaxRulePriority = 1000;

            /// <summary>
            /// Parameter name for base rate in rule expressions
            /// </summary>
            public const string BaseRateParameterName = "basePrice";

            /// <summary>
            /// Parameter name for transaction volume in rule expressions
            /// </summary>
            public const string TransactionVolumeParameterName = "transactionVolume";

            /// <summary>
            /// Parameter name for service type in rule expressions
            /// </summary>
            public const string ServiceTypeParameterName = "serviceType";

            /// <summary>
            /// Parameter name for filing frequency in rule expressions
            /// </summary>
            public const string FilingFrequencyParameterName = "filingFrequency";

            /// <summary>
            /// Parameter name for country code in rule expressions
            /// </summary>
            public const string CountryCodeParameterName = "countryCode";
        }

        /// <summary>
        /// Constants related to date and time handling
        /// </summary>
        public static class DateTimeConstants
        {
            /// <summary>
            /// Default date format string
            /// </summary>
            public const string DefaultDateFormat = "yyyy-MM-dd";

            /// <summary>
            /// Default time format string
            /// </summary>
            public const string DefaultTimeFormat = "HH:mm:ss";

            /// <summary>
            /// Default date and time format string
            /// </summary>
            public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            /// <summary>
            /// Default number of days until data expiration
            /// </summary>
            public const int DefaultExpirationDays = 365;

            /// <summary>
            /// Default number of days until data archiving
            /// </summary>
            public const int DefaultArchiveDays = 1095; // 3 years
        }

        /// <summary>
        /// Country-specific constants and configurations
        /// </summary>
        public static class CountrySpecific
        {
            /// <summary>
            /// Mapping of country codes to their currency codes
            /// </summary>
            public static readonly Dictionary<string, string> CountryCurrencies = new Dictionary<string, string>
            {
                { "GB", "GBP" }, // United Kingdom - Pound Sterling
                { "DE", "EUR" }, // Germany - Euro
                { "FR", "EUR" }, // France - Euro
                { "IT", "EUR" }, // Italy - Euro
                { "ES", "EUR" }, // Spain - Euro
                { "NL", "EUR" }, // Netherlands - Euro
                { "BE", "EUR" }, // Belgium - Euro
                { "SE", "SEK" }, // Sweden - Swedish Krona
                { "DK", "DKK" }, // Denmark - Danish Krone
                { "PL", "PLN" }, // Poland - Polish Złoty
                { "IE", "EUR" }, // Ireland - Euro
                { "AT", "EUR" }, // Austria - Euro
                { "FI", "EUR" }, // Finland - Euro
                { "US", "USD" }  // United States - US Dollar
            };

            /// <summary>
            /// Standard VAT rates for countries (as of 2023)
            /// </summary>
            public static readonly Dictionary<string, decimal> StandardVatRates = new Dictionary<string, decimal>
            {
                { "GB", 20.0m }, // United Kingdom
                { "DE", 19.0m }, // Germany
                { "FR", 20.0m }, // France
                { "IT", 22.0m }, // Italy
                { "ES", 21.0m }, // Spain
                { "NL", 21.0m }, // Netherlands
                { "BE", 21.0m }, // Belgium
                { "SE", 25.0m }, // Sweden
                { "DK", 25.0m }, // Denmark
                { "PL", 23.0m }, // Poland
                { "IE", 23.0m }, // Ireland
                { "AT", 20.0m }, // Austria
                { "FI", 24.0m }, // Finland
                { "US", 0.0m }   // United States (no VAT, uses sales tax)
            };

            /// <summary>
            /// Available filing frequencies by country
            /// </summary>
            public static readonly Dictionary<string, string[]> FilingFrequencies = new Dictionary<string, string[]>
            {
                { "GB", new[] { "Monthly", "Quarterly", "Annually" } },
                { "DE", new[] { "Monthly", "Quarterly", "Annually" } },
                { "FR", new[] { "Monthly", "Quarterly", "Annually" } },
                { "IT", new[] { "Monthly", "Quarterly" } },
                { "ES", new[] { "Monthly", "Quarterly" } },
                { "NL", new[] { "Monthly", "Quarterly", "Annually" } },
                { "BE", new[] { "Monthly", "Quarterly" } },
                { "SE", new[] { "Monthly", "Quarterly" } },
                { "DK", new[] { "Monthly", "Quarterly", "Semi-Annually" } },
                { "PL", new[] { "Monthly", "Quarterly" } },
                { "IE", new[] { "Bi-Monthly", "Quarterly", "Annually" } },
                { "AT", new[] { "Monthly", "Quarterly", "Annually" } },
                { "FI", new[] { "Monthly", "Quarterly", "Annually" } },
                { "US", new[] { "Monthly", "Quarterly", "Annually" } }
            };
        }
    }
}